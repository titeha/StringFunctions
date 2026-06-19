using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Разбирает количественное числительное, записанное словами на русском языке, обратно в число.
/// </summary>
/// <remarks>
/// <para>
/// Поддерживаются все падежи и роды (форма берётся из тех же таблиц склонения, что и генератор),
/// приставка «минус» и слово «ноль». Регистр, буква «ё»/«е» и лишние пробелы не важны.
/// </para>
/// <para>
/// Разбор строгий: любое нераспознанное слово или некорректный порядок слов возвращают <c>Failure</c>.
/// Порядковые числительные («двадцать первый») в текущей версии не поддерживаются.
/// </para>
/// </remarks>
public static class RussianNumberParser
{
  private enum Kind
  {
    Adder,
    Scale,
    Zero
  }

  private readonly struct Entry(Kind kind, long value, int requiredAbove, int setRank, long multiplier)
  {
    public Kind Kind { get; } = kind;

    public long Value { get; } = value;

    public int RequiredAbove { get; } = requiredAbove;

    public int SetRank { get; } = setRank;

    public long Multiplier { get; } = multiplier;
  }

  private const int _groupStartRank = 4;

  private static readonly long[] _scalePow =
    [1L, 1_000L, 1_000_000L, 1_000_000_000L, 1_000_000_000_000L, 1_000_000_000_000_000L, 1_000_000_000_000_000_000L];

  private static readonly Dictionary<string, Entry> _vocabulary = BuildVocabulary();
  private static readonly string _minus = Normalize(RussianNumberToWords.MinusWord);

  /// <summary>
  /// Разбирает количественное числительное, записанное словами, в число.
  /// </summary>
  /// <param name="text">Числительное прописью, например «сто двадцать три» или «минус пять».</param>
  /// <returns>
  /// <see cref="Result{T}"/> с числом при успехе либо описание ошибки, если строка
  /// не является корректным количественным числительным.
  /// </returns>
  public static Result<long> Parse(string? text)
  {
    if (text is null)
      return Result.Failure<long>("Строка не может быть null.");

    string[] tokens = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

    if (tokens.Length == 0)
      return Result.Failure<long>("Строка не содержит числительного.");

    int index = 0;
    long sign = 1;

    if (Normalize(tokens[0]) == _minus)
    {
      sign = -1;
      index = 1;
    }

    long result = 0;
    long current = 0;
    int lastPlaceRank = _groupStartRank;
    long lastScale = long.MaxValue;
    bool sawValue = false;
    bool sawZero = false;

    try
    {
      for (; index < tokens.Length; index++)
      {
        string token = Normalize(tokens[index]);

        if (!_vocabulary.TryGetValue(token, out Entry entry))
          return Result.Failure<long>($"Нераспознанное слово: '{tokens[index]}'.");

        switch (entry.Kind)
        {
          case Kind.Zero:
            if (sawValue)
              return Result.Failure<long>("Слово «ноль» не может сочетаться с другими числительными.");

            sawZero = true;
            sawValue = true;
            break;

          case Kind.Adder:
            if (sawZero || lastPlaceRank <= entry.RequiredAbove)
              return Result.Failure<long>($"Недопустимый порядок слов рядом с '{tokens[index]}'.");

            current = checked(current + entry.Value);
            lastPlaceRank = entry.SetRank;
            sawValue = true;
            break;

          default: // Kind.Scale
            if (sawZero || entry.Multiplier >= lastScale)
              return Result.Failure<long>($"Недопустимый порядок разрядов рядом с '{tokens[index]}'.");

            long groupValue = current == 0 ? 1 : current;
            result = checked(result + groupValue * entry.Multiplier);
            lastScale = entry.Multiplier;
            current = 0;
            lastPlaceRank = _groupStartRank;
            sawValue = true;
            break;
        }
      }

      if (!sawValue)
        return Result.Failure<long>("Строка не содержит числительного.");

      result = checked(result + current);
      return Result.Success(sign * result);
    }
    catch (OverflowException)
    {
      return Result.Failure<long>("Число выходит за пределы диапазона Int64.");
    }
  }

  private static Dictionary<string, Entry> BuildVocabulary()
  {
    var vocabulary = new Dictionary<string, Entry>(StringComparer.Ordinal);

    foreach ((string form, int value) in RussianNumberToWords.EnumerateAdderForms())
    {
      (int requiredAbove, int setRank) = value switch
      {
        < 10 => (1, 1),    // единицы
        < 20 => (2, 1),    // 10–19 (занимают и десятки, и единицы)
        < 100 => (2, 2),   // десятки
        _ => (3, 3)        // сотни
      };

      vocabulary[Normalize(form)] = new Entry(Kind.Adder, value, requiredAbove, setRank, 0);
    }

    foreach ((string form, int scale) in RussianNumberToWords.EnumerateScaleForms())
      vocabulary[Normalize(form)] = new Entry(Kind.Scale, 0, 0, 0, _scalePow[scale]);

    foreach (string form in RussianNumberToWords.ZeroForms)
      vocabulary[Normalize(form)] = new Entry(Kind.Zero, 0, 0, 0, 0);

    return vocabulary;
  }

  private static string Normalize(string word) =>
    word.Trim().ToLowerInvariant().Replace('ё', 'е');
}
