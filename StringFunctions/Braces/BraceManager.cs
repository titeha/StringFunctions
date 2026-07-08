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

  private List<Brace> _bracesList = [];
  private readonly Dictionary<char, Brace> _braceBySymbol;
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
    NormalizeAndValidateBraces();

    BracesList = BuildBracesArray();
    _braceBySymbol = BuildBraceLookup();
  }

  public BraceManager(params (char, char)[] bracesSymbols)
  {
    if (bracesSymbols is null || bracesSymbols.Length == 0)
      ThrowArgumentException();

    AddCustomBracesPair(bracesSymbols);
    NormalizeAndValidateBraces();

    BracesList = BuildBracesArray();
    _braceBySymbol = BuildBraceLookup();
  }

  public BraceManager(KnownBracesTypes bracesTypes, params (char, char)[] bracesSymbols)
  {
    if (bracesTypes.IsEmpty() || bracesSymbols is null || bracesSymbols.Length == 0)
      ThrowArgumentException();

    AddCustomBracesPair(bracesSymbols);
    AddCommonBracesPair(bracesTypes);
    NormalizeAndValidateBraces();

    BracesList = BuildBracesArray();
    _braceBySymbol = BuildBraceLookup();
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
    for (int i = 0, count = bracesSymbols.Length; i < count; i++)
      _bracesList.Add(new Brace(bracesSymbols[i]));
  }

  public bool IsOpening(char candidate) =>
    _braceBySymbol.TryGetValue(candidate, out Brace brace) && brace.IsOpening(candidate);

  public bool IsPair(char candidate, char checking) =>
    _braceBySymbol.TryGetValue(checking, out Brace brace) && brace.IsPair(candidate);

  public bool IsPaired(char candidate) =>
    _braceBySymbol.TryGetValue(candidate, out Brace brace) && brace.IsPaired;

  private void NormalizeAndValidateBraces()
  {
    _bracesList = _bracesList.Distinct().ToList();
    ValidateNoConflictingBraces();
  }

  private char[] BuildBracesArray()
  {
    var symbols = new List<char>(_bracesList.Count * 2);
    var seen = new HashSet<char>();

    foreach (Brace brace in _bracesList)
      foreach (char symbol in brace)
        if (seen.Add(symbol))
          symbols.Add(symbol);

    return [.. symbols];
  }

  private Dictionary<char, Brace> BuildBraceLookup()
  {
    var lookup = new Dictionary<char, Brace>(BracesList.Length);

    foreach (Brace brace in _bracesList)
      foreach (char symbol in brace)
        lookup.TryAdd(symbol, brace);

    return lookup;
  }

  // Гарантирует, что один и тот же символ не принадлежит двум разным парам.
  // Без этой проверки один и тот же символ мог бы получить неоднозначную роль
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
