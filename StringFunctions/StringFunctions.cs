using CommonClasses;

using StringFunctions.Braces;

namespace StringFunctions;

public static class StringFunctions
{
  #region Методы
  /// <summary>
  /// Функция проверяет баланс скобок в строке. Проверяет баланс всех видов скобок и кавычек
  /// </summary>
  /// <param name="source">Проверяемая строка</param>
  /// <returns>Возвращаемый кортеж, булево значение - признак сбалансированности скобок, символ - несбалансированный символ (символ с кодом 0, если всё ок)</returns>
  public static (bool, char) IsBracesBalanced(this string source, KnownBracesTypes bracesTypes = KnownBracesTypes.Other, params (char, char)[] bracesSymbols)
  {
    if (source.IsNullOrEmpty())
      return (true, '\x0');

    if (bracesTypes.IsEmpty() && bracesSymbols.Length == 0)
      throw new ArgumentException();

    BraceManager _manager;
    if (bracesTypes.IsEmpty() && bracesSymbols.Length > 0)
      _manager = new BraceManager(bracesSymbols);
    else if (!bracesTypes.IsEmpty() && bracesSymbols.Length == 0)
      _manager = new BraceManager(bracesTypes);
    else if (!bracesTypes.IsEmpty() && 0 < bracesSymbols.Length)
      _manager = new BraceManager(bracesTypes, bracesSymbols);
    else
      _manager = new BraceManager(KnownBracesTypes.All);

    return IsBracesBalanced(source, _manager);
  }

  private static (bool, char) IsBracesBalanced(string source, BraceManager manager)
  {
    const char _zeroCodeSym = '\x0';
    char[] _bracesList = manager.BracesList;
    Stack<char> _result = new();
    char _returnSymbol = _zeroCodeSym;
    bool _isBalanced;

    int _lastIndex = source.IndexOfAny(_bracesList);

    while (_lastIndex >= 0)
    {
      char _lookingValue = source[_lastIndex];

      if (manager.IsPaired(_lookingValue))
        if (manager.IsOpening(_lookingValue))
          _result.Push(_lookingValue);
        else if (0 < _result.Count && manager.IsPair(_lookingValue, _result.Peek()))
          _result.Pop();
        else
        {
          _returnSymbol = 0 < _result.Count ? _result.Pop() : _lookingValue;
          break;
        }
      else if (0 < _result.Count && manager.IsPair(_lookingValue, _result.Peek()))
        _result.Pop();
      else
        _result.Push(_lookingValue);

      _lastIndex = source.IndexOfAny(_bracesList, 1 + _lastIndex);
    }

    _isBalanced = 0 == _result.Count && _zeroCodeSym == _returnSymbol;
    if (!_isBalanced && _zeroCodeSym == _returnSymbol)
      _returnSymbol = _result.Pop();

    return (_isBalanced, _returnSymbol);
  }
  #endregion Методы
}