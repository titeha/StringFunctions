using System.Text;

namespace StringFunctions;

public static class StringHelperExtensions
{
  internal static string _leftBraces = "([{<";
  internal static string _rightBraces = ")]}>";
  internal static string _leftQuotas = "\"'«“";
  internal static string _rightQuotas = "»”";
  internal static string _punctuation = ".,?!;:";
  internal static string _commonSpecialSymbols = "-\\/_";
  internal static string _otherSpecialSymbols = "$#=+%^&|";
  internal static char _whitespace = ' ';
  internal static string _openingSymbols = _leftBraces + _leftQuotas;
  internal static string _closingSymbols = _rightQuotas + _rightBraces;
  internal static string _allSpecialSymbols = string.Concat(_commonSpecialSymbols, _otherSpecialSymbols);
  internal static string _fullDelimitersList = string.Concat(_openingSymbols, _punctuation, _closingSymbols, _allSpecialSymbols) + _whitespace;

  private static readonly string _punctuationClosingSpecialSymbols = string.Concat(_punctuation, _closingSymbols, _allSpecialSymbols);
  private static readonly string _punctuationWhitespace = _punctuation + _whitespace;
  private static readonly string _rightBracesAndRightQuotas = _rightBraces + _rightQuotas;

  public static string NormalizeString(this string source)
  {
    StringBuilder _normalizingString = new(source.Trim());
    int i = 0;

    while (i < _normalizingString.Length)
    {
      char _curSymbol = _normalizingString[i];

      if (_fullDelimitersList.IsDelimiter(_curSymbol))
        if (i == 0 && _punctuationClosingSpecialSymbols.IsDelimiter(_curSymbol))
          _normalizingString.Remove(0, 1);
        else if (_punctuationWhitespace.IsDelimiter(_curSymbol) && _openingSymbols.IsDelimiter(_normalizingString[i - 1]))
          _normalizingString.Remove(i, 1);
        else if (_rightBracesAndRightQuotas.IsDelimiter(_curSymbol) && _punctuationWhitespace.IsDelimiter(_normalizingString[i - 1]))
          _normalizingString.Remove(--i, 1);
        else if (_punctuation.IsDelimiter(_curSymbol) && _normalizingString[i - 1].Equals(_whitespace))
          _normalizingString.Remove(--i, 1);
        else if (_curSymbol.Equals(_whitespace) && _normalizingString[i - 1].Equals(_whitespace))
          _normalizingString.Remove(--i, 1);
        else
          i++;
      else
        i++;
    }

    return _normalizingString.ToString();
  }

  public static bool IsDelimiter(this string delimiters, char source) => delimiters.IndexOf(source) >= 0;

  public static bool IsNullOrEmpty(this string? source) => string.IsNullOrEmpty(source);

  public static bool IsNullOrWhiteSpace(this string? source) => string.IsNullOrWhiteSpace(source);
}