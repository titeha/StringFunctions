namespace StringFunctions.Braces;

internal class BraceManager
{
  #region Константа
  private const string _noBracesTypesPresent = "Не указаны виды проверяемых символов!";
  #endregion

  #region Поля
  private static readonly Dictionary<KnownBracesTypes, (char, char)> _knownBracesValues = new()
  {
    [RoundedBraces] = ('(', ')'),
    [SquareBraces] = ('[', ']'),
    [FigureBraces] = ('{', '}'),
    [CornerBraces] = ('<', '>'),
    [Quotas] = ('"', '"'),
    [Apostrofe] = ('\'', '\''),
    [CornerQuotas] = ('«', '»'),
    [HandWritesQuotas] = ('“', '”'),
    [Tilda] = ('~', '~')
  };

  private readonly static Brace _roundBrace = new Brace(_knownBracesValues[RoundedBraces]);
  private readonly static Brace _squareBrace = new Brace(_knownBracesValues[SquareBraces]);
  private readonly static Brace _figureBrace = new Brace(_knownBracesValues[FigureBraces]);
  private readonly static Brace _cornerBrace = new Brace(_knownBracesValues[CornerBraces]);
  private readonly static Brace _quotas = new Brace(_knownBracesValues[Quotas]);
  private readonly static Brace _apostrofe = new Brace(_knownBracesValues[Apostrofe]);
  private readonly static Brace _cornerQuotas = new Brace(_knownBracesValues[CornerQuotas]);
  private readonly static Brace _handWriteQuotas = new Brace(_knownBracesValues[HandWritesQuotas]);
  private readonly static Brace _tilda = new Brace(_knownBracesValues[Tilda]);

  private readonly List<Brace> _bracesList = new List<Brace>();
  #endregion

  #region Свойство
  public char[] BracesList => _bracesList.Distinct().Aggregate(string.Empty, (r, b) => string.Concat(r, b.ToString()), r => r.ToCharArray());
  #endregion

  #region Конструкторы
  public BraceManager(KnownBracesTypes bracesTypes)
  {
    if (bracesTypes.IsEmpty())
      ThrowArgumentException();

    AddCommonBracesPair(bracesTypes);
  }

  public BraceManager(params (char, char)[] bracesSymbols)
  {
    if (bracesSymbols.Length == 0)
      ThrowArgumentException();

    AddCustomBracesPair(bracesSymbols);

    _bracesList = _bracesList.Distinct().ToList();
  }

  public BraceManager(KnownBracesTypes bracesTypes, params (char, char)[] bracesSymbols)
  {
    if (bracesTypes.IsEmpty() || bracesSymbols.Length == 0)
      ThrowArgumentException();

    AddCustomBracesPair(bracesSymbols);
    AddCommonBracesPair(bracesTypes);

    _bracesList = _bracesList.Distinct().ToList();
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

  private void ThrowArgumentException() => throw new ArgumentException(_noBracesTypesPresent);
  #endregion
}