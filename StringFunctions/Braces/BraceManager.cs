using static StringFunctions.Braces.KnownBracesTypes;

namespace StringFunctions.Braces;

internal class BraceManager
{
  #region Константа
  private const string _noBracesTypesPresent = "Не указаны виды проверяемых символов!";
  private const string _conflictingBraces = "Символ '{0}' задан более чем в одной паре скобок/кавычек.";
  #endregion

  #region Поля
  private readonly static Brace _roundBrace = new(('(', ')'));
  private readonly static Brace _squareBrace = new(('[', ']'));
  private readonly static Brace _figureBrace = new(('{', '}'));
  private readonly static Brace _cornerBrace = new(('<', '>'));
  private readonly static Brace _quotas = new(('"', '"'));
  private readonly static Brace _apostrofe = new(('\'', '\''));
  private readonly static Brace _cornerQuotas = new(('«', '»'));
  private readonly static Brace _handWriteQuotas = new(('“', '”'));
  private readonly static Brace _tilda = new(('~', '~'));

  private readonly List<Brace> _bracesList = new();
  #endregion

  #region Свойство
  public char[] BracesList { get; }
  #endregion

  #region Конструкторы
  public BraceManager(KnownBracesTypes bracesTypes)
  {
    if (bracesTypes.IsEmpty())
      ThrowArgumentException();

    AddCommonBracesPair(bracesTypes);

    _bracesList = _bracesList.Distinct().ToList();
    ValidateNoConflictingBraces();
    BracesList = BuildBracesArray();
  }

  public BraceManager(params (char, char)[] bracesSymbols)
  {
    if (bracesSymbols.Length == 0)
      ThrowArgumentException();

    AddCustomBracesPair(bracesSymbols);

    _bracesList = _bracesList.Distinct().ToList();
    ValidateNoConflictingBraces();
    BracesList = BuildBracesArray();
  }

  public BraceManager(KnownBracesTypes bracesTypes, params (char, char)[] bracesSymbols)
  {
    if (bracesTypes.IsEmpty() || bracesSymbols.Length == 0)
      ThrowArgumentException();

    AddCustomBracesPair(bracesSymbols);
    AddCommonBracesPair(bracesTypes);

    _bracesList = _bracesList.Distinct().ToList();
    ValidateNoConflictingBraces();
    BracesList = BuildBracesArray();
  }
  #endregion

  #region Методы
  private void AddCommonBracesPair(KnownBracesTypes bracesTypes)
  {
    if (bracesTypes.IsRoundedBraces())
      _bracesList.Add(_roundBrace);
    if (bracesTypes.IsSquareBraces())
      _bracesList.Add(_squareBrace);
    if (bracesTypes.IsFigureBraces())
      _bracesList.Add(_figureBrace);
    if (bracesTypes.IsCornerBraces())
      _bracesList.Add(_cornerBrace);
    if (bracesTypes.IsQuotas())
      _bracesList.Add(_quotas);
    if (bracesTypes.IsApostrofe())
      _bracesList.Add(_apostrofe);
    if (bracesTypes.IsCornerQuotas())
      _bracesList.Add(_cornerQuotas);
    if (bracesTypes.IsHandwriteQuotas())
      _bracesList.Add(_handWriteQuotas);
    if (bracesTypes.IsTilda())
      _bracesList.Add(_tilda);
  }

  private void AddCustomBracesPair(params (char, char)[] bracesSymbols)
  {
    for (int i = 0, _count = bracesSymbols.Length; i < _count; i++)
      _bracesList.Add(new Brace(bracesSymbols[i]));
  }

  public bool IsOpening(char candidate) => _bracesList.Any(b => b.IsOpening(candidate));

  public bool IsPair(char candidate, char checking) => _bracesList.Any(b => b.HasThisBrace(checking) && b.IsPair(candidate));

  public bool IsPaired(char candidate) => _bracesList.Single(v => v.HasThisBrace(candidate)).IsPaired;


  private char[] BuildBracesArray()
  {
    var symbols = new List<char>(_bracesList.Count * 2);

    foreach (Brace brace in _bracesList)
      foreach (char symbol in brace)
        if (!symbols.Contains(symbol))
          symbols.Add(symbol);

    return [.. symbols];
  }

  // Гарантирует, что один и тот же символ не принадлежит двум разным парам.
  // Без этой проверки IsPaired (.Single) бросил бы InvalidOperationException
  // уже во время проверки баланса, в обход контракта Result.
  private void ValidateNoConflictingBraces()
  {
    for (int i = 0; i < _bracesList.Count; i++)
      foreach (char symbol in _bracesList[i])
        for (int j = i + 1; j < _bracesList.Count; j++)
          if (_bracesList[j].HasThisBrace(symbol))
            throw new ArgumentException(string.Format(_conflictingBraces, symbol));
  }

  private static void ThrowArgumentException() => throw new ArgumentException(_noBracesTypesPresent);
  #endregion
}