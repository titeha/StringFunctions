using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
#if NET8_0_OR_GREATER
using System.Runtime.InteropServices;
#endif
using ResultType;

namespace StringFunctions;

/// <summary>
/// Предоставляет методы для разбора строкового представления целых чисел и диапазонов целых чисел.
/// </summary>
/// <remarks>
/// Поддерживаются следующие формы записи:
/// <list type="bullet">
/// <item><description><c>N</c> — одиночное значение.</description></item>
/// <item><description><c>N-M</c> — диапазон значений от <c>N</c> до <c>M</c> включительно.</description></item>
/// <item><description><c>-N</c> — открытый слева диапазон, интерпретируемый как <c>1..N</c>.</description></item>
/// <item><description><c>N-</c> — открытый справа диапазон, интерпретируемый как <c>N..maxRangeValue</c>.</description></item>
/// </list>
/// <para>
/// Явно заданный <c>0</c> допускается, например <c>0</c>, <c>0-N</c> и <c>0-</c>.
/// </para>
/// <para>
/// Ошибки пользовательского ввода возвращаются через <c>Result&lt;List&lt;int&gt;&gt;</c>,
/// без генерации исключений для некорректного формата входной строки.
/// </para>
/// <para>
/// Результат разбора всегда содержит уникальные значения, отсортированные по возрастанию.
/// </para>
/// </remarks>
public static class IntRangeParser
{
  private const string _delimiters = " ,.;_:#!|\\/'\"";

  private const int _bitsPerWord = 64;
  private const int _wordShift = 6;
  private const int _wordBitMask = _bitsPerWord - 1;

  // If an exact dense bitset fits in ~64 KiB payload, use it.
  private const int _denseWordsThreshold = 8_192;

  // Segmented fallback for large maxRangeValue.
  private const int _wordsPerSegment = 8_192; // 8192 * 8 = 65_536 bytes payload
  private const int _bitsPerSegment = _wordsPerSegment * _bitsPerWord; // 524_288
  private const int _segmentShift = 19; // log2(524_288)
  private const int _segmentBitMask = _bitsPerSegment - 1;

  // For large maxRangeValue and a small number of tokens, it can be better
  // to normalize ranges first and only then choose a storage strategy.
  private const int _largeRangeAdaptiveThreshold = _denseWordsThreshold * _bitsPerWord; // 524_288
  private const int _adaptiveTokenThreshold = 64;
  private const int _smallMergedRangeThreshold = 16;
  private const int _directMaterializeCountThreshold = 8_192;

  private readonly struct RangeBounds(int start, int end)
  {
    public int Start { get; } = start;

    public int End { get; } = end;
  }

  private readonly struct MergeInfo(int count, long cardinality)
  {
    public int Count { get; } = count;

    public long Cardinality { get; } = cardinality;
  }

  /// <summary>
  /// Разбирает строку с числами и диапазонами целых чисел и возвращает
  /// отсортированный список уникальных значений.
  /// </summary>
  /// <param name="rangeSource">
  /// Исходная строка с токенами диапазонов. Поддерживаются формы:
  /// <c>N</c>, <c>N-M</c>, <c>-N</c>, <c>N-</c>.
  /// </param>
  /// <param name="maxRangeValue">
  /// Максимально допустимое значение правой границы. Должно быть не меньше <c>0</c>.
  /// </param>
  /// <returns>
  /// <see cref="Result{T}"/> с результатом разбора.
  /// При успехе возвращает отсортированный список уникальных значений.
  /// При ошибке возвращает текстовое описание причины.
  /// </returns>
  /// <remarks>
  /// <para>
  /// Семантика диапазонов:
  /// </para>
  /// <list type="bullet">
  /// <item><description><c>-N</c> трактуется как диапазон от <c>1</c> до <c>N</c>.</description></item>
  /// <item><description><c>0-N</c>, <c>0</c> и <c>0-</c> допустимы.</description></item>
  /// <item><description>Явные значения меньше <c>0</c> считаются ошибкой.</description></item>
  /// <item><description>Пробелы вокруг символа <c>-</c> внутри диапазона допустимы.</description></item>
  /// </list>
  /// <para>
  /// Поддерживаемые разделители токенов:
  /// <c>" ,.;_:#!|\\/'\""</c>.
  /// </para>
  /// <para>
  /// Метод не использует исключения для ошибок пользовательского ввода; ошибки
  /// возвращаются через <see cref="Result{T}"/>.
  /// </para>
  /// </remarks>
  public static Result<List<int>> Parse(string? rangeSource, int maxRangeValue)
  {
    if (rangeSource is null)
      return Result.Failure<List<int>>("Исходная строка диапазона не может быть null.");

    if (maxRangeValue < 0)
      return Result.Failure<List<int>>("Максимальное значение диапазона должно быть не меньше 0.");

    rangeSource = NormalizeSpacesAroundDash(rangeSource);

    ReadOnlySpan<char> source = rangeSource.AsSpan();
    int tokenCount = CountTokens(source);

    if (tokenCount == 0)
      return Result.Success<List<int>>([]);

    // 1. One token -> direct fast path.
    if (tokenCount == 1)
      return ParseSingleTokenFastPath(source, maxRangeValue);

    // 2. Huge maxRangeValue + a small number of tokens -> adaptive path.
    if (maxRangeValue > _largeRangeAdaptiveThreshold && tokenCount <= _adaptiveTokenThreshold)
      return ParseAdaptiveLargeRange(source, tokenCount, maxRangeValue);

    // 3. Baseline streaming path.
    IBitSet bitSet = CreateBitSet(maxRangeValue);

    int position = 0;
    for (int tokenIndex = 0; TryReadNextToken(source, ref position, out ReadOnlySpan<char> token); tokenIndex++)
    {
      Result tokenResult = ParseTokenIntoBitSet(token, tokenIndex, tokenCount, maxRangeValue, bitSet);

      if (tokenResult.IsFailure)
        return Result.Failure<List<int>>(tokenResult.Error!);
    }

    return Result.Success(bitSet.ToList());
  }

  private static IBitSet CreateBitSet(int maxRangeValue)
  {
    int requiredWords = (maxRangeValue + _wordBitMask) >> _wordShift;

    return requiredWords <= _denseWordsThreshold
      ? new DenseBitSet64(maxRangeValue)
      : new SegmentedBitSet64(maxRangeValue);
  }

  private static Result<List<int>> ParseSingleTokenFastPath(ReadOnlySpan<char> source, int maxRangeValue)
  {
    int position = 0;

    if (!TryReadNextToken(source, ref position, out ReadOnlySpan<char> token))
      return Result.Failure<List<int>>("Внутренняя ошибка: ожидался один токен, но токен не был прочитан.");

    Result<RangeBounds> boundsResult = ParseTokenBounds(token, 0, 1, maxRangeValue);

    if (boundsResult.IsFailure)
      return Result.Failure<List<int>>(boundsResult.Error!);

    RangeBounds bounds = boundsResult.Value;
    return Result.Success(MaterializeRange(bounds.Start, bounds.End));
  }

  private static Result<List<int>> ParseAdaptiveLargeRange(
    ReadOnlySpan<char> source,
    int tokenCount,
    int maxRangeValue)
  {
    var ranges = new List<RangeBounds>(tokenCount);

    int position = 0;
    for (int tokenIndex = 0; TryReadNextToken(source, ref position, out ReadOnlySpan<char> token); tokenIndex++)
    {
      Result<RangeBounds> boundsResult = ParseTokenBounds(token, tokenIndex, tokenCount, maxRangeValue);

      if (boundsResult.IsFailure)
        return Result.Failure<List<int>>(boundsResult.Error!);

      ranges.Add(boundsResult.Value);
    }

    if (ranges.Count == 0)
      return Result.Success<List<int>>([]);

    ranges.Sort(static (a, b) =>
    {
      int cmp = a.Start.CompareTo(b.Start);
      return cmp != 0 ? cmp : a.End.CompareTo(b.End);
    });

    MergeInfo mergeInfo = MergeRangesInPlace(ranges);

    // One merged range -> direct materialization.
    if (mergeInfo.Count == 1)
      return Result.Success(MaterializeRange(ranges[0].Start, ranges[0].End));

    // Small merged range count or small total cardinality -> direct fill.
    if (mergeInfo.Count <= _smallMergedRangeThreshold || mergeInfo.Cardinality <= _directMaterializeCountThreshold)
      return Result.Success(MaterializeMergedRanges(ranges, mergeInfo));

    IBitSet bitSet = CreateBitSet(maxRangeValue);

    for (int i = 0; i < mergeInfo.Count; i++)
    {
      RangeBounds bounds = ranges[i];

      if (bounds.Start == bounds.End)
        bitSet.Set(bounds.Start);
      else
        bitSet.SetRange(bounds.Start, bounds.End);
    }

    return Result.Success(bitSet.ToList());
  }

  private static Result ParseTokenIntoBitSet(
    ReadOnlySpan<char> token,
    int tokenIndex,
    int tokenCount,
    int maxRangeValue,
    IBitSet bitSet)
  {
    Result<RangeBounds> boundsResult = ParseTokenBounds(token, tokenIndex, tokenCount, maxRangeValue);

    if (boundsResult.IsFailure)
      return Result.Failure(boundsResult.Error!);

    RangeBounds bounds = boundsResult.Value;

    if (bounds.Start == bounds.End)
      bitSet.Set(bounds.Start);
    else
      bitSet.SetRange(bounds.Start, bounds.End);

    return Result.Success();
  }

  private static Result<RangeBounds> ParseTokenBounds(
    ReadOnlySpan<char> token,
    int tokenIndex,
    int tokenCount,
    int maxRangeValue)
  {
    int dashIndex = FindSingleDash(token);

    // Single number.
    if (dashIndex == -1)
    {
      Result<int> valueResult = ParseNonNegativeInt(token, tokenIndex, token, "Некорректное число.");

      if (valueResult.IsFailure)
        return Result.Failure<RangeBounds>(valueResult.Error!);

      int value = valueResult.Value;

      if (value > maxRangeValue)
        return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, $"Значение должно быть в диапазоне 0..{maxRangeValue}."));

      return Result.Success(new RangeBounds(value, value));
    }

    // More than one '-'.
    if (dashIndex == -2)
      return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, "Некорректный диапазон: найдено больше одного символа '-'."));

    // -N -> open-left range 1..N
    if (dashIndex == 0)
    {
      if (tokenIndex != 0)
        return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, "Диапазон вида '-N' допустим только первым токеном."));

      ReadOnlySpan<char> right = token[1..];
      Result<int> endResult = ParseNonNegativeInt(right, tokenIndex, token, "Некорректная правая граница диапазона.");

      if (endResult.IsFailure)
        return Result.Failure<RangeBounds>(endResult.Error!);

      int end = endResult.Value;

      // Open-left ranges always start from 1, so -0 is invalid.
      if (end < 1 || end > maxRangeValue)
        return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, $"Правая граница открытого диапазона должна быть в диапазоне 1..{maxRangeValue}."));

      return Result.Success(new RangeBounds(1, end));
    }

    // N-
    if (dashIndex == token.Length - 1)
    {
      if (tokenIndex != tokenCount - 1)
        return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, "Диапазон вида 'N-' допустим только последним токеном."));

      ReadOnlySpan<char> left = token[..dashIndex];
      Result<int> startResult = ParseNonNegativeInt(left, tokenIndex, token, "Некорректная левая граница диапазона.");

      if (startResult.IsFailure)
        return Result.Failure<RangeBounds>(startResult.Error!);

      int start = startResult.Value;

      if (start > maxRangeValue)
        return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, $"Левая граница должна быть в диапазоне 0..{maxRangeValue}."));

      return Result.Success(new RangeBounds(start, maxRangeValue));
    }

    // X-Y
    ReadOnlySpan<char> first = token[..dashIndex];
    ReadOnlySpan<char> second = token[(dashIndex + 1)..];

    Result<int> firstResult = ParseNonNegativeInt(first, tokenIndex, token, "Некорректные границы диапазона.");
    if (firstResult.IsFailure)
      return Result.Failure<RangeBounds>(firstResult.Error!);

    Result<int> secondResult = ParseNonNegativeInt(second, tokenIndex, token, "Некорректные границы диапазона.");
    if (secondResult.IsFailure)
      return Result.Failure<RangeBounds>(secondResult.Error!);

    int startRange = Math.Min(firstResult.Value, secondResult.Value);
    int endRange = Math.Max(firstResult.Value, secondResult.Value);

    if (endRange > maxRangeValue)
      return Result.Failure<RangeBounds>(BuildTokenError(tokenIndex, token, $"Границы диапазона должны быть в пределах 0..{maxRangeValue}."));

    return Result.Success(new RangeBounds(startRange, endRange));
  }

  private static Result<int> ParseNonNegativeInt(
    ReadOnlySpan<char> value,
    int tokenIndex,
    ReadOnlySpan<char> token,
    string errorMessage)
  {
    return int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out int result) && result >= 0
      ? Result.Success(result)
      : Result.Failure<int>(BuildTokenError(tokenIndex, token, errorMessage));
  }

  private static string NormalizeSpacesAroundDash(string source)
  {
    if (source.IndexOf('-') < 0)
      return source;

    bool needsRewrite = false;

    for (int i = 0; i < source.Length; i++)
    {
      if (source[i] == '-' &&
          (i > 0 && char.IsWhiteSpace(source[i - 1]) ||
           i + 1 < source.Length && char.IsWhiteSpace(source[i + 1])))
      {
        needsRewrite = true;
        break;
      }
    }

    if (!needsRewrite)
      return source;

    var result = new System.Text.StringBuilder(source.Length);
    bool previousNonWhitespaceWasDash = false;

    int index = 0;
    while (index < source.Length)
    {
      char c = source[index];

      if (!char.IsWhiteSpace(c))
      {
        result.Append(c);
        previousNonWhitespaceWasDash = c == '-';
        index++;
        continue;
      }

      int next = index;
      while (next < source.Length && char.IsWhiteSpace(source[next]))
        next++;

      bool nextNonWhitespaceIsDash = next < source.Length && source[next] == '-';

      // Удаляем пробелы, если они примыкают к '-'
      if (previousNonWhitespaceWasDash || nextNonWhitespaceIsDash)
      {
        index = next;
        continue;
      }

      // Иначе оставляем один пробел как обычный разделитель токенов
      result.Append(' ');
      previousNonWhitespaceWasDash = false;
      index = next;
    }

    return result.ToString();
  }

  private static MergeInfo MergeRangesInPlace(List<RangeBounds> ranges)
  {
    if (ranges.Count == 0)
      return new MergeInfo(0, 0);

    int writeIndex = 0;
    int currentStart = ranges[0].Start;
    int currentEnd = ranges[0].End;
    long cardinality = 0;

    for (int readIndex = 1; readIndex < ranges.Count; readIndex++)
    {
      RangeBounds next = ranges[readIndex];

      if (next.Start <= currentEnd + 1)
      {
        if (next.End > currentEnd)
          currentEnd = next.End;

        continue;
      }

      ranges[writeIndex++] = new RangeBounds(currentStart, currentEnd);
      cardinality += (long)currentEnd - currentStart + 1;

      currentStart = next.Start;
      currentEnd = next.End;
    }

    ranges[writeIndex++] = new RangeBounds(currentStart, currentEnd);
    cardinality += (long)currentEnd - currentStart + 1;

    return new MergeInfo(writeIndex, cardinality);
  }

  private static List<int> MaterializeRange(int start, int end)
  {
    int count = checked(end - start + 1);
    var result = new List<int>(count);

#if NET8_0_OR_GREATER
    CollectionsMarshal.SetCount(result, count);
    Span<int> destination = CollectionsMarshal.AsSpan(result);

    for (int i = 0; i < count; i++)
      destination[i] = start + i;
#else
    for (int value = start; value <= end; value++)
      result.Add(value);
#endif

    return result;
  }

  private static List<int> MaterializeMergedRanges(List<RangeBounds> ranges, MergeInfo mergeInfo)
  {
    int totalCount = checked((int)mergeInfo.Cardinality);
    var result = new List<int>(totalCount);

#if NET8_0_OR_GREATER
    CollectionsMarshal.SetCount(result, totalCount);
    Span<int> destination = CollectionsMarshal.AsSpan(result);

    int written = 0;

    for (int i = 0; i < mergeInfo.Count; i++)
    {
      RangeBounds bounds = ranges[i];

      for (int value = bounds.Start; value <= bounds.End; value++)
        destination[written++] = value;
    }
#else
    for (int i = 0; i < mergeInfo.Count; i++)
    {
      RangeBounds bounds = ranges[i];

      for (int value = bounds.Start; value <= bounds.End; value++)
        result.Add(value);
    }
#endif

    return result;
  }

  private static string BuildTokenError(int tokenIndex, ReadOnlySpan<char> token, string message) =>
    $"{message} Токен #{tokenIndex + 1}: '{token}'.";

  private static int CountTokens(ReadOnlySpan<char> source)
  {
    int count = 0;
    bool insideToken = false;

    for (int i = 0; i < source.Length; i++)
    {
      if (IsDelimiter(source[i]))
      {
        insideToken = false;
        continue;
      }

      if (!insideToken)
      {
        insideToken = true;
        count++;
      }
    }

    return count;
  }

  // Intentionally left as bool+out: this is a tokenizer cursor, not a domain-level error.
  private static bool TryReadNextToken(ReadOnlySpan<char> source, ref int position, out ReadOnlySpan<char> token)
  {
    while (position < source.Length && IsDelimiter(source[position]))
      position++;

    if (position >= source.Length)
    {
      token = default;
      return false;
    }

    int start = position;

    while (position < source.Length && !IsDelimiter(source[position]))
      position++;

    token = source[start..position];
    return true;
  }

  /// <summary>
  /// Returns:
  /// -1  -> no dash
  /// -2  -> more than one dash
  /// >=0 -> index of the single dash
  /// </summary>
  private static int FindSingleDash(ReadOnlySpan<char> token)
  {
    int dashIndex = -1;

    for (int i = 0; i < token.Length; i++)
    {
      if (token[i] != '-')
        continue;

      if (dashIndex >= 0)
        return -2;

      dashIndex = i;
    }

    return dashIndex;
  }

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static bool IsDelimiter(char ch) => _delimiters.Contains(ch);

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong LowBitsMask(int bitCount) =>
    bitCount >= _bitsPerWord
      ? ulong.MaxValue
      : (1UL << bitCount) - 1UL;

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static ulong CreateMask(int startBitInWord, int endBitInWord) =>
    (ulong.MaxValue << startBitInWord) & LowBitsMask(endBitInWord + 1);

#if NET8_0_OR_GREATER
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static int WriteWordToSpan(ulong word, int wordBaseValue, Span<int> destination)
  {
    int written = 0;

    while (word != 0)
    {
      int bitOffset = BitOperations.TrailingZeroCount(word);
      destination[written++] = wordBaseValue + bitOffset;
      word &= word - 1;
    }

    return written;
  }
#else
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  private static void AppendWordToList(List<int> destination, ulong word, int wordBaseValue)
  {
    while (word != 0)
    {
      int bitOffset = BitOperations.TrailingZeroCount(word);
      destination.Add(wordBaseValue + bitOffset);
      word &= word - 1;
    }
  }
#endif

  private interface IBitSet
  {
    void Set(int value);

    void SetRange(int start, int end);

    List<int> ToList();
  }

  private sealed class DenseBitSet64(int maxAllowedValue) : IBitSet
  {
    private readonly ulong[] _words = new ulong[(maxAllowedValue + _wordBitMask) >> _wordShift];

    private int _count;
    private int _minValueSet = int.MaxValue;
    private int _maxValueSet;
    private bool _hasZero;

    public void Set(int value)
    {
      if (value == 0)
      {
        if (_hasZero)
          return;

        _hasZero = true;
        _count++;

        if (_minValueSet > 0)
          _minValueSet = 0;

        return;
      }

      int bitIndex = value - 1;
      int wordIndex = bitIndex >> _wordShift;
      int bitInWord = bitIndex & _wordBitMask;

      ulong mask = 1UL << bitInWord;
      ulong oldWord = _words[wordIndex];
      ulong newWord = oldWord | mask;

      if (newWord == oldWord)
        return;

      _words[wordIndex] = newWord;
      _count++;
      UpdateMinMax(value, value);
    }

    public void SetRange(int start, int end)
    {
      if (start > end)
        (start, end) = (end, start);

      int originalStart = start;
      int originalEnd = end;

      if (start == 0)
      {
        Set(0);

        if (end == 0)
        {
          UpdateMinMax(0, 0);
          return;
        }

        start = 1;
      }

      int startBitIndex = start - 1;
      int endBitIndex = end - 1;

      int startWordIndex = startBitIndex >> _wordShift;
      int endWordIndex = endBitIndex >> _wordShift;

      if (startWordIndex == endWordIndex)
      {
        SetWordOrMask(startWordIndex, CreateMask(startBitIndex & _wordBitMask, endBitIndex & _wordBitMask));
        UpdateMinMax(originalStart, originalEnd);
        return;
      }

      SetWordOrMask(startWordIndex, ulong.MaxValue << (startBitIndex & _wordBitMask));

      for (int wordIndex = startWordIndex + 1; wordIndex < endWordIndex; wordIndex++)
        FillWord(wordIndex);

      SetWordOrMask(endWordIndex, LowBitsMask((endBitIndex & _wordBitMask) + 1));
      UpdateMinMax(originalStart, originalEnd);
    }

    public List<int> ToList()
    {
      if (_count == 0)
        return [];

      int positiveCount = _count - (_hasZero ? 1 : 0);
      var result = new List<int>(_count);

      if (positiveCount == 0)
      {
        result.Add(0);
        return result;
      }

      int positiveStartValue = _minValueSet <= 0 ? 1 : _minValueSet;
      int startBitIndex = positiveStartValue - 1;
      int endBitIndex = _maxValueSet - 1;

      int startWordIndex = startBitIndex >> _wordShift;
      int endWordIndex = endBitIndex >> _wordShift;

#if NET8_0_OR_GREATER
      CollectionsMarshal.SetCount(result, _count);
      Span<int> destination = CollectionsMarshal.AsSpan(result);

      int written = 0;

      if (_hasZero)
        destination[written++] = 0;

      written += WriteWordsToSpan(startWordIndex, endWordIndex, startBitIndex, endBitIndex, destination[written..]);

      if (written != _count)
        CollectionsMarshal.SetCount(result, written);
#else
      if (_hasZero)
        result.Add(0);

      AppendWordsToList(result, startWordIndex, endWordIndex, startBitIndex, endBitIndex);
#endif

      return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateMinMax(int start, int end)
    {
      if (start < _minValueSet)
        _minValueSet = start;

      if (end > _maxValueSet)
        _maxValueSet = end;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetWordOrMask(int wordIndex, ulong mask)
    {
      ulong oldWord = _words[wordIndex];
      ulong newWord = oldWord | mask;

      if (newWord == oldWord)
        return;

      _words[wordIndex] = newWord;
      _count += BitOperations.PopCount(newWord ^ oldWord);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillWord(int wordIndex)
    {
      ulong oldWord = _words[wordIndex];

      if (oldWord == ulong.MaxValue)
        return;

      _words[wordIndex] = ulong.MaxValue;
      _count += _bitsPerWord - BitOperations.PopCount(oldWord);
    }

#if NET8_0_OR_GREATER
    private int WriteWordsToSpan(
      int startWordIndex,
      int endWordIndex,
      int startBitIndex,
      int endBitIndex,
      Span<int> destination)
    {
      int written = 0;

      if (startWordIndex == endWordIndex)
      {
        ulong mask = CreateMask(startBitIndex & _wordBitMask, endBitIndex & _wordBitMask);
        return WriteWordToSpan(_words[startWordIndex] & mask, 1 + (startWordIndex << _wordShift), destination);
      }

      ulong firstMask = ulong.MaxValue << (startBitIndex & _wordBitMask);
      written += WriteWordToSpan(_words[startWordIndex] & firstMask, 1 + (startWordIndex << _wordShift), destination[written..]);

      for (int wordIndex = startWordIndex + 1; wordIndex < endWordIndex; wordIndex++)
        written += WriteWordToSpan(_words[wordIndex], 1 + (wordIndex << _wordShift), destination[written..]);

      ulong lastMask = LowBitsMask((endBitIndex & _wordBitMask) + 1);
      written += WriteWordToSpan(_words[endWordIndex] & lastMask, 1 + (endWordIndex << _wordShift), destination[written..]);

      return written;
    }
#else
    private void AppendWordsToList(
      List<int> destination,
      int startWordIndex,
      int endWordIndex,
      int startBitIndex,
      int endBitIndex)
    {
      if (startWordIndex == endWordIndex)
      {
        ulong mask = CreateMask(startBitIndex & _wordBitMask, endBitIndex & _wordBitMask);
        AppendWordToList(destination, _words[startWordIndex] & mask, 1 + (startWordIndex << _wordShift));
        return;
      }

      ulong firstMask = ulong.MaxValue << (startBitIndex & _wordBitMask);
      AppendWordToList(destination, _words[startWordIndex] & firstMask, 1 + (startWordIndex << _wordShift));

      for (int wordIndex = startWordIndex + 1; wordIndex < endWordIndex; wordIndex++)
        AppendWordToList(destination, _words[wordIndex], 1 + (wordIndex << _wordShift));

      ulong lastMask = LowBitsMask((endBitIndex & _wordBitMask) + 1);
      AppendWordToList(destination, _words[endWordIndex] & lastMask, 1 + (endWordIndex << _wordShift));
    }
#endif
  }

  private sealed class SegmentedBitSet64 : IBitSet
  {
    private readonly int _maxAllowedValue;
    private readonly ulong[][] _segments;

    private int _count;
    private int _minValueSet = int.MaxValue;
    private int _maxValueSet;
    private bool _hasZero;

    public SegmentedBitSet64(int maxAllowedValue)
    {
      _maxAllowedValue = maxAllowedValue;
      int segmentCount = ((maxAllowedValue - 1) >> _segmentShift) + 1;
      _segments = new ulong[segmentCount][];
    }

    public void Set(int value)
    {
      if (value == 0)
      {
        if (_hasZero)
          return;

        _hasZero = true;
        _count++;

        if (_minValueSet > 0)
          _minValueSet = 0;

        return;
      }

      int bitIndex = value - 1;
      int segmentIndex = bitIndex >> _segmentShift;
      int bitInSegment = bitIndex & _segmentBitMask;
      int wordIndex = bitInSegment >> _wordShift;
      int bitInWord = bitInSegment & _wordBitMask;

      ulong[] words = GetOrCreateSegment(segmentIndex);
      ulong mask = 1UL << bitInWord;

      ulong oldWord = words[wordIndex];
      ulong newWord = oldWord | mask;

      if (newWord == oldWord)
        return;

      words[wordIndex] = newWord;
      _count++;
      UpdateMinMax(value, value);
    }

    public void SetRange(int start, int end)
    {
      if (start > end)
        (start, end) = (end, start);

      int originalStart = start;
      int originalEnd = end;

      if (start == 0)
      {
        Set(0);

        if (end == 0)
        {
          UpdateMinMax(0, 0);
          return;
        }

        start = 1;
      }

      int startBitIndex = start - 1;
      int endBitIndex = end - 1;

      int startSegmentIndex = startBitIndex >> _segmentShift;
      int endSegmentIndex = endBitIndex >> _segmentShift;

      if (startSegmentIndex == endSegmentIndex)
      {
        SetRangeInSegment(startSegmentIndex, startBitIndex & _segmentBitMask, endBitIndex & _segmentBitMask);
        UpdateMinMax(originalStart, originalEnd);
        return;
      }

      SetRangeInSegment(startSegmentIndex, startBitIndex & _segmentBitMask, _bitsPerSegment - 1);

      for (int segmentIndex = startSegmentIndex + 1; segmentIndex < endSegmentIndex; segmentIndex++)
        FillEntireSegment(segmentIndex);

      SetRangeInSegment(endSegmentIndex, 0, endBitIndex & _segmentBitMask);
      UpdateMinMax(originalStart, originalEnd);
    }

    public List<int> ToList()
    {
      if (_count == 0)
        return [];

      int positiveCount = _count - (_hasZero ? 1 : 0);
      var result = new List<int>(_count);

      if (positiveCount == 0)
      {
        result.Add(0);
        return result;
      }

      int positiveStartValue = _minValueSet <= 0 ? 1 : _minValueSet;
      int startBitIndex = positiveStartValue - 1;
      int endBitIndex = _maxValueSet - 1;

      int startSegmentIndex = startBitIndex >> _segmentShift;
      int endSegmentIndex = endBitIndex >> _segmentShift;

#if NET8_0_OR_GREATER
      CollectionsMarshal.SetCount(result, _count);
      Span<int> destination = CollectionsMarshal.AsSpan(result);

      int written = 0;

      if (_hasZero)
        destination[written++] = 0;

      for (int segmentIndex = startSegmentIndex; segmentIndex <= endSegmentIndex; segmentIndex++)
      {
        ulong[]? words = _segments[segmentIndex];
        if (words is null)
          continue;

        int firstBitInSegment = segmentIndex == startSegmentIndex
          ? (startBitIndex & _segmentBitMask)
          : 0;

        int lastBitInSegment = segmentIndex == endSegmentIndex
          ? (endBitIndex & _segmentBitMask)
          : GetValidBitCount(segmentIndex) - 1;

        written += WriteSegmentToSpan(
          words,
          segmentIndex,
          firstBitInSegment,
          lastBitInSegment,
          destination[written..]);
      }

      if (written != _count)
        CollectionsMarshal.SetCount(result, written);
#else
      if (_hasZero)
        result.Add(0);

      for (int segmentIndex = startSegmentIndex; segmentIndex <= endSegmentIndex; segmentIndex++)
      {
        ulong[]? words = _segments[segmentIndex];
        if (words is null)
          continue;

        int firstBitInSegment = segmentIndex == startSegmentIndex
          ? (startBitIndex & _segmentBitMask)
          : 0;

        int lastBitInSegment = segmentIndex == endSegmentIndex
          ? (endBitIndex & _segmentBitMask)
          : GetValidBitCount(segmentIndex) - 1;

        AppendSegmentToList(result, words, segmentIndex, firstBitInSegment, lastBitInSegment);
      }
#endif

      return result;
    }

    private void FillEntireSegment(int segmentIndex)
    {
      ulong[]? words = _segments[segmentIndex];

      int validBitCount = GetValidBitCount(segmentIndex);
      int fullWordCount = validBitCount >> _wordShift;
      int tailBitCount = validBitCount & _wordBitMask;

      if (words is null)
      {
        words = new ulong[_wordsPerSegment];
        _segments[segmentIndex] = words;

        if (fullWordCount > 0)
          Array.Fill(words, ulong.MaxValue, 0, fullWordCount);

        if (tailBitCount != 0)
          words[fullWordCount] = LowBitsMask(tailBitCount);

        _count += validBitCount;
        return;
      }

      for (int wordIndex = 0; wordIndex < fullWordCount; wordIndex++)
        FillWord(words, wordIndex);

      if (tailBitCount != 0)
        SetWordOrMask(words, fullWordCount, LowBitsMask(tailBitCount));
    }

    private void SetRangeInSegment(int segmentIndex, int startBitInclusive, int endBitInclusive)
    {
      if (startBitInclusive > endBitInclusive)
        return;

      ulong[]? words = _segments[segmentIndex];
      bool isNewSegment = words is null;

      if (isNewSegment)
      {
        words = new ulong[_wordsPerSegment];
        _segments[segmentIndex] = words;
      }

      int startWordIndex = startBitInclusive >> _wordShift;
      int endWordIndex = endBitInclusive >> _wordShift;
      int startBitInWord = startBitInclusive & _wordBitMask;
      int endBitInWord = endBitInclusive & _wordBitMask;

      if (startWordIndex == endWordIndex)
      {
        ulong mask = CreateMask(startBitInWord, endBitInWord);

        if (isNewSegment)
        {
          words![startWordIndex] = mask;
          _count += BitOperations.PopCount(mask);
        }
        else
          SetWordOrMask(words!, startWordIndex, mask);

        return;
      }

      ulong headMask = ulong.MaxValue << startBitInWord;

      if (isNewSegment)
      {
        words![startWordIndex] = headMask;
        _count += _bitsPerWord - startBitInWord;
      }
      else
        SetWordOrMask(words!, startWordIndex, headMask);

      int middleWordCount = endWordIndex - startWordIndex - 1;

      if (middleWordCount > 0)
      {
        if (isNewSegment)
        {
          Array.Fill(words!, ulong.MaxValue, startWordIndex + 1, middleWordCount);
          _count += middleWordCount * _bitsPerWord;
        }
        else
          for (int wordIndex = startWordIndex + 1; wordIndex < endWordIndex; wordIndex++)
            FillWord(words!, wordIndex);
      }

      ulong tailMask = LowBitsMask(endBitInWord + 1);

      if (isNewSegment)
      {
        words![endWordIndex] = tailMask;
        _count += endBitInWord + 1;
      }
      else
        SetWordOrMask(words!, endWordIndex, tailMask);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ulong[] GetOrCreateSegment(int segmentIndex) => _segments[segmentIndex] ??= new ulong[_wordsPerSegment];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdateMinMax(int start, int end)
    {
      if (start < _minValueSet)
        _minValueSet = start;

      if (end > _maxValueSet)
        _maxValueSet = end;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetValidBitCount(int segmentIndex)
    {
      int remaining = _maxAllowedValue - segmentIndex * _bitsPerSegment;
      return remaining > _bitsPerSegment ? _bitsPerSegment : remaining;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetWordOrMask(ulong[] words, int wordIndex, ulong mask)
    {
      ulong oldWord = words[wordIndex];
      ulong newWord = oldWord | mask;

      if (newWord == oldWord)
        return;

      words[wordIndex] = newWord;
      _count += BitOperations.PopCount(newWord ^ oldWord);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void FillWord(ulong[] words, int wordIndex)
    {
      ulong oldWord = words[wordIndex];

      if (oldWord == ulong.MaxValue)
        return;

      words[wordIndex] = ulong.MaxValue;
      _count += _bitsPerWord - BitOperations.PopCount(oldWord);
    }

#if NET8_0_OR_GREATER
    private static int WriteSegmentToSpan(
      ulong[] words,
      int segmentIndex,
      int firstBitInSegment,
      int lastBitInSegment,
      Span<int> destination)
    {
      int firstWordIndex = firstBitInSegment >> _wordShift;
      int lastWordIndex = lastBitInSegment >> _wordShift;

      int written = 0;
      int segmentBaseValue = segmentIndex * _bitsPerSegment + 1;

      if (firstWordIndex == lastWordIndex)
      {
        ulong mask = CreateMask(firstBitInSegment & _wordBitMask, lastBitInSegment & _wordBitMask);
        return WriteWordToSpan(words[firstWordIndex] & mask, segmentBaseValue + (firstWordIndex << _wordShift), destination);
      }

      ulong firstMask = ulong.MaxValue << (firstBitInSegment & _wordBitMask);
      written += WriteWordToSpan(words[firstWordIndex] & firstMask, segmentBaseValue + (firstWordIndex << _wordShift), destination[written..]);

      for (int wordIndex = firstWordIndex + 1; wordIndex < lastWordIndex; wordIndex++)
        written += WriteWordToSpan(words[wordIndex], segmentBaseValue + (wordIndex << _wordShift), destination[written..]);

      ulong lastMask = LowBitsMask((lastBitInSegment & _wordBitMask) + 1);
      written += WriteWordToSpan(words[lastWordIndex] & lastMask, segmentBaseValue + (lastWordIndex << _wordShift), destination[written..]);

      return written;
    }
#else
    private static void AppendSegmentToList(
      List<int> destination,
      ulong[] words,
      int segmentIndex,
      int firstBitInSegment,
      int lastBitInSegment)
    {
      int firstWordIndex = firstBitInSegment >> _wordShift;
      int lastWordIndex = lastBitInSegment >> _wordShift;

      int segmentBaseValue = segmentIndex * _bitsPerSegment + 1;

      if (firstWordIndex == lastWordIndex)
      {
        ulong mask = CreateMask(firstBitInSegment & _wordBitMask, lastBitInSegment & _wordBitMask);
        AppendWordToList(destination, words[firstWordIndex] & mask, segmentBaseValue + (firstWordIndex << _wordShift));
        return;
      }

      ulong firstMask = ulong.MaxValue << (firstBitInSegment & _wordBitMask);
      AppendWordToList(destination, words[firstWordIndex] & firstMask, segmentBaseValue + (firstWordIndex << _wordShift));

      for (int wordIndex = firstWordIndex + 1; wordIndex < lastWordIndex; wordIndex++)
        AppendWordToList(destination, words[wordIndex], segmentBaseValue + (wordIndex << _wordShift));

      ulong lastMask = LowBitsMask((lastBitInSegment & _wordBitMask) + 1);
      AppendWordToList(destination, words[lastWordIndex] & lastMask, segmentBaseValue + (lastWordIndex << _wordShift));
    }
#endif
  }
}
