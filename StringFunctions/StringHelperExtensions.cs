using System.Text;

using ResultType;

namespace StringFunctions;

/// <summary>
/// Предоставляет вспомогательные методы расширения для работы со строками.
/// </summary>
public static class StringHelperExtensions
{
  private static readonly string LeftBraces = "([{<";
  private static readonly string RightBraces = ")]}>";
  private static readonly string LeftQuotes = "\"'«“";
  private static readonly string RightQuotes = "»”";
  private static readonly string Punctuation = ".,?!;:";
  private static readonly string CommonSpecialSymbols = "-\\/_";
  private static readonly string OtherSpecialSymbols = "$#=+%^&|";

  private const char _whitespace = ' ';
  private const string _nullSourceError = "Исходная строка не может быть null.";

  private static readonly string _openingSymbols = LeftBraces + LeftQuotes;
  private static readonly string _closingSymbols = RightQuotes + RightBraces;
  private static readonly string _allSpecialSymbols = string.Concat(CommonSpecialSymbols, OtherSpecialSymbols);
  private static readonly string _fullDelimitersList = string.Concat(_openingSymbols, Punctuation, _closingSymbols, _allSpecialSymbols) + _whitespace;
  private static readonly string _punctuationClosingSpecialSymbols = string.Concat(Punctuation, _closingSymbols, _allSpecialSymbols);
  private static readonly string _punctuationWhitespace = Punctuation + _whitespace;
  private static readonly string _rightBracesAndRightQuotes = RightBraces + RightQuotes;

  /// <summary>
  /// Нормализует строку, удаляя лишние пробелы и некорректные сочетания пробелов,
  /// знаков препинания, скобок, кавычек и специальных символов.
  /// </summary>
  /// <param name="source">Исходная строка.</param>
  /// <returns>
  /// Успешный результат содержит нормализованную строку.
  /// Если входная строка равна <c>null</c>, возвращается <c>Failure</c> с текстом ошибки.
  /// </returns>
  /// <remarks>
  /// Пустая строка и строка, состоящая только из пробельных символов,
  /// нормализуются в пустую строку.
  /// </remarks>
  public static Result<string> NormalizeString(this string? source)
  {
    if (source is null)
      return Result.Failure<string>(_nullSourceError);

    if (string.IsNullOrWhiteSpace(source))
      return Result.Success(string.Empty);

    return Result.Success(NormalizeStringCore(source));
  }

  private static string NormalizeStringCore(string source)
  {
    ReadOnlySpan<char> text = source.AsSpan().Trim();
    if (text.IsEmpty)
      return string.Empty;

    var normalized = new StringBuilder(text.Length);

    foreach (char current in text)
    {
      // Убираем ведущие закрывающие/пунктуационные/спецсимволы.
      if (normalized.Length == 0)
      {
        if (_punctuationClosingSpecialSymbols.IsDelimiter(current) || current == _whitespace)
          continue;

        normalized.Append(current);
        continue;
      }

      char previous = normalized[^1];

      // Убираем пунктуацию и пробелы сразу после открывающих символов.
      if (_openingSymbols.IsDelimiter(previous) && _punctuationWhitespace.IsDelimiter(current))
        continue;

      // Перед закрывающим символом удаляем все пробелы и пунктуацию.
      if (_rightBracesAndRightQuotes.IsDelimiter(current))
      {
        while (normalized.Length > 0 && _punctuationWhitespace.IsDelimiter(normalized[^1]))
          normalized.Length--;

        normalized.Append(current);
        continue;
      }

      // Убираем пробел перед пунктуацией.
      if (Punctuation.IsDelimiter(current))
      {
        while (normalized.Length > 0 && normalized[^1] == _whitespace)
          normalized.Length--;

        normalized.Append(current);
        continue;
      }

      // Схлопываем повторные пробелы.
      if (current == _whitespace && previous == _whitespace)
        continue;

      normalized.Append(current);
    }

    return normalized.ToString();
  }

  public static bool IsDelimiter(this string delimiters, char source) => delimiters.IndexOf(source) >= 0;

  public static bool IsNullOrEmpty(this string? source) => string.IsNullOrEmpty(source);

  public static bool IsNullOrWhiteSpace(this string? source) => string.IsNullOrWhiteSpace(source);
}
