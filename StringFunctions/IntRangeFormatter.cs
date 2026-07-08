using System.Text;

using ResultType;

namespace StringFunctions;

/// <summary>
/// Предоставляет методы для форматирования набора целых чисел в строковое представление диапазонов.
/// </summary>
/// <remarks>
/// <para>
/// Метод принимает произвольную последовательность значений, нормализует её
/// (сортирует по возрастанию и удаляет дубликаты), после чего собирает строку диапазонов.
/// </para>
/// <para>
/// Базовая перегрузка форматирует диапазоны только в явном виде: <c>N</c> и <c>N-M</c>.
/// Перегрузка с <c>maxRangeValue</c> может дополнительно использовать открытые диапазоны:
/// <c>-N</c>, <c>N-</c> и <c>0-</c>.
/// </para>
/// <para>
/// Ошибки пользовательского ввода возвращаются через <c>Result&lt;string&gt;</c>,
/// без генерации исключений для некорректных значений последовательности.
/// </para>
/// </remarks>
public static class IntRangeFormatter
{
  private const string _delimiters = " ,.;_:#!|\\/'\"";
  private const int _maxInitialBufferCapacity = 4_096;

  /// <summary>
  /// Форматирует последовательность целых чисел в строку диапазонов.
  /// </summary>
  /// <param name="values">Последовательность значений для форматирования.</param>
  /// <param name="separator">
  /// Разделитель между токенами результата. Для гарантированного round-trip с <see cref="IntRangeParser"/>
  /// должен состоять только из символов, поддерживаемых парсером как разделители.
  /// </param>
  /// <returns>
  /// Успешный результат со строкой диапазонов либо ошибку валидации.
  /// </returns>
  public static Result<string> Format(IEnumerable<int>? values, string? separator = ",") =>
    FormatCore(values, separator, hasMaxRangeValue: false, maxRangeValue: 0, useOpenRanges: false);

  /// <summary>
  /// Форматирует последовательность целых чисел в строку диапазонов с учётом верхней границы диапазона.
  /// </summary>
  /// <param name="values">Последовательность значений для форматирования.</param>
  /// <param name="maxRangeValue">Максимально допустимое значение в последовательности.</param>
  /// <param name="separator">
  /// Разделитель между токенами результата. Для гарантированного round-trip с <see cref="IntRangeParser"/>
  /// должен состоять только из символов, поддерживаемых парсером как разделители.
  /// </param>
  /// <param name="useOpenRanges">
  /// Если <see langword="true"/>, formatter может использовать открытые диапазоны:
  /// <c>-N</c>, <c>N-</c> и <c>0-</c>.
  /// </param>
  /// <returns>
  /// Успешный результат со строкой диапазонов либо ошибку валидации.
  /// </returns>
  public static Result<string> Format(
    IEnumerable<int>? values,
    int maxRangeValue,
    string? separator = ",",
    bool useOpenRanges = true) =>
    FormatCore(values, separator, hasMaxRangeValue: true, maxRangeValue: maxRangeValue, useOpenRanges: useOpenRanges);

  private static Result<string> FormatCore(
    IEnumerable<int>? values,
    string? separator,
    bool hasMaxRangeValue,
    int maxRangeValue,
    bool useOpenRanges)
  {
    if (values is null)
      return Result.Failure<string>("Коллекция значений не может быть null.");

    Result validateSeparatorResult = ValidateSeparator(separator);
    if (validateSeparatorResult.IsFailure)
      return Result.Failure<string>(validateSeparatorResult.Error!);

    if (hasMaxRangeValue && maxRangeValue < 0)
      return Result.Failure<string>("Максимальное значение диапазона должно быть не меньше 0.");

    Result<List<int>> normalizedValuesResult = CollectAndNormalizeValues(values, hasMaxRangeValue, maxRangeValue);
    if (normalizedValuesResult.IsFailure)
      return Result.Failure<string>(normalizedValuesResult.Error!);

    List<int> normalizedValues = normalizedValuesResult.Value!;

    if (normalizedValues.Count == 0)
      return Result.Success(string.Empty);

    string text = FormatNormalizedValues(normalizedValues, separator!, hasMaxRangeValue, maxRangeValue, useOpenRanges);
    return Result.Success(text);
  }

  private static Result ValidateSeparator(string? separator)
  {
    if (separator is null)
      return Result.Failure("Разделитель не может быть null.");

    if (separator.Length == 0)
      return Result.Failure("Разделитель не может быть пустой строкой.");

    for (int i = 0; i < separator.Length; i++)
    {
      char c = separator[i];

      if (_delimiters.IndexOf(c) < 0)
      {
        return Result.Failure(
          $"Разделитель содержит неподдерживаемый символ '{c}'. Для гарантированного round-trip используйте только символы из набора '{_delimiters}'.");
      }
    }

    return Result.Success();
  }

  private static Result<List<int>> CollectAndNormalizeValues(
    IEnumerable<int> values,
    bool hasMaxRangeValue,
    int maxRangeValue)
  {
    int capacity = 0;
    if (values.TryGetNonEnumeratedCount(out int count) && count > 0)
      capacity = Math.Min(count, _maxInitialBufferCapacity);

    var buffer = capacity > 0 ? new List<int>(capacity) : [];

    bool sortedUnique = true;
    bool hasPrev = false;
    int prev = 0;
    int index = 0;

    foreach (int value in values)
    {
      if (value < 0)
        return Result.Failure<List<int>>($"Значение должно быть не меньше 0. Значение: {value}. Индекс: {index}.");

      if (hasMaxRangeValue && value > maxRangeValue)
        return Result.Failure<List<int>>($"Значение должно быть в диапазоне 0..{maxRangeValue}. Значение: {value}. Индекс: {index}.");

      if (hasPrev && value <= prev)
        sortedUnique = false;

      buffer.Add(value);
      prev = value;
      hasPrev = true;
      index++;
    }

    if (buffer.Count <= 1)
      return Result.Success(buffer);

    if (sortedUnique)
      return Result.Success(buffer);

    buffer.Sort();

    int writeIndex = 1;

    for (int readIndex = 1; readIndex < buffer.Count; readIndex++)
    {
      if (buffer[readIndex] == buffer[writeIndex - 1])
        continue;

      buffer[writeIndex++] = buffer[readIndex];
    }

    if (writeIndex < buffer.Count)
      buffer.RemoveRange(writeIndex, buffer.Count - writeIndex);

    return Result.Success(buffer);
  }

  private static string FormatNormalizedValues(
    List<int> values,
    string separator,
    bool hasMaxRangeValue,
    int maxRangeValue,
    bool useOpenRanges)
  {
    var result = new StringBuilder(Math.Max(16, values.Count * (separator.Length + 4)));

    int runStart = values[0];
    int runEnd = values[0];

    for (int i = 1; i < values.Count; i++)
    {
      int current = values[i];

      if (current == runEnd + 1)
      {
        runEnd = current;
        continue;
      }

      AppendRange(result, runStart, runEnd, separator, hasMaxRangeValue, maxRangeValue, useOpenRanges);
      runStart = current;
      runEnd = current;
    }

    AppendRange(result, runStart, runEnd, separator, hasMaxRangeValue, maxRangeValue, useOpenRanges);
    return result.ToString();
  }

  private static void AppendRange(
    StringBuilder builder,
    int start,
    int end,
    string separator,
    bool hasMaxRangeValue,
    int maxRangeValue,
    bool useOpenRanges)
  {
    if (builder.Length != 0)
      builder.Append(separator);

    if (start == end)
    {
      builder.Append(start);
      return;
    }

    if (useOpenRanges && hasMaxRangeValue)
    {
      if (start == 0 && end == maxRangeValue)
      {
        builder.Append('0');
        builder.Append('-');
        return;
      }

      if (start == 1)
      {
        builder.Append('-');
        builder.Append(end);
        return;
      }

      if (end == maxRangeValue)
      {
        builder.Append(start);
        builder.Append('-');
        return;
      }
    }

    builder.Append(start);
    builder.Append('-');
    builder.Append(end);
  }
}
