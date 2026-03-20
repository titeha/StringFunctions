using System.Text;

using ResultType;

namespace StringFunctions;

public static class StringHelperExtensions
{
  private static readonly string _leftBraces = "([{<";
  private static readonly string _rightBraces = ")]}>";
  private static readonly string _leftQuotas = "\"'«“";
  private static readonly string _rightQuotas = "»”";
  private static readonly string _punctuation = ".,?!;:";
  private static readonly string _commonSpecialSymbols = "-\\/_";
  private static readonly string _otherSpecialSymbols = "$#=+%^&|";
  private const char _whitespace = ' ';
  private static readonly string _openingSymbols = _leftBraces + _leftQuotas;
  private static readonly string _closingSymbols = _rightQuotas + _rightBraces;
  private static readonly string _allSpecialSymbols = string.Concat(_commonSpecialSymbols, _otherSpecialSymbols);
  private static readonly string _fullDelimitersList = string.Concat(_openingSymbols, _punctuation, _closingSymbols, _allSpecialSymbols) + _whitespace;
  private static readonly string _punctuationClosingSpecialSymbols = string.Concat(_punctuation, _closingSymbols, _allSpecialSymbols);
  private static readonly string _punctuationWhitespace = _punctuation + _whitespace;
  private static readonly string _rightBracesAndRightQuotas = _rightBraces + _rightQuotas;

  private const string _nullSourceError = "Исходная строка не может быть null.";

  /// <summary>
  /// Нормализует строку, удаляя лишние разделители и пробелы по заданным правилам.
  /// </summary>
  /// <param name="source">Исходная строка.</param>
  /// <returns>
  /// Успешный результат с нормализованной строкой либо ошибку, если <paramref name="source"/> равен <see langword="null"/>.
  /// </returns>
  /// <remarks>
  /// <para><see langword="null"/> возвращает <c>Failure</c>.</para>
  /// <para>Пустая или состоящая только из пробелов строка возвращает <c>Success(string.Empty)</c>.</para>
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
    var normalizingString = new StringBuilder(source.Trim());
    int i = 0;

    while (i < normalizingString.Length)
    {
      char curSymbol = normalizingString[i];

      if (_fullDelimitersList.IsDelimiter(curSymbol))
        if (i == 0 && _punctuationClosingSpecialSymbols.IsDelimiter(curSymbol))
          normalizingString.Remove(0, 1);
        else if (i > 0 && _punctuationWhitespace.IsDelimiter(curSymbol) && _openingSymbols.IsDelimiter(normalizingString[i - 1]))
          normalizingString.Remove(i, 1);
        else if (i > 0 && _rightBracesAndRightQuotas.IsDelimiter(curSymbol) && _punctuationWhitespace.IsDelimiter(normalizingString[i - 1]))
          normalizingString.Remove(--i, 1);
        else if (i > 0 && _punctuation.IsDelimiter(curSymbol) && normalizingString[i - 1] == _whitespace)
          normalizingString.Remove(--i, 1);
        else if (i > 0 && curSymbol == _whitespace && normalizingString[i - 1] == _whitespace)
          normalizingString.Remove(--i, 1);
        else
          i++;
      else
        i++;
    }

    return normalizingString.ToString();
  }

  public static bool IsDelimiter(this string delimiters, char source) => delimiters.IndexOf(source) >= 0;

  public static bool IsNullOrEmpty(this string? source) => string.IsNullOrEmpty(source);

  public static bool IsNullOrWhiteSpace(this string? source) => string.IsNullOrWhiteSpace(source);
}
