using System.Collections;

namespace StringFunctions.Braces;

internal readonly struct Brace : IEnumerable<char>, IEquatable<Brace>
{
  #region Поля
  private readonly char _closingBrace;
  private readonly char _openingBrace;
  #endregion

  #region Свойства
  public bool IsPaired => _closingBrace != _openingBrace;
  #endregion

  #region Конструкторы
  public Brace(char opening, char closing)
  {
    _openingBrace = opening;
    _closingBrace = closing;
  }

  internal Brace((char, char) braces) : this(braces.Item1, braces.Item2) { }
  #endregion

  #region Методы
  public bool Equals(Brace other) => other._closingBrace == _closingBrace && other._openingBrace == _openingBrace;

  public bool HasThisBrace(char checking) => checking == _openingBrace || checking == _closingBrace;

  public bool IsOpening(char candidate) => candidate == _openingBrace;

  public bool IsPair(char candidate) => candidate == _closingBrace;

  public override string ToString() => string.Concat(GetBraces());

  private IEnumerable<char> GetBraces()
  {
    yield return _openingBrace;
    yield return _closingBrace;
  }

  public IEnumerator<char> GetEnumerator()
  {
    yield return _openingBrace;
    yield return _closingBrace;
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
  #endregion
}