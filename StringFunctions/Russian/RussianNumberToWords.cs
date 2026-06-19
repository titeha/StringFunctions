namespace StringFunctions.Russian;

/// <summary>
/// Преобразует целые числа в их запись словами на русском языке (прописью)
/// с поддержкой склонения по падежам.
/// </summary>
/// <remarks>
/// <para>
/// Учитывается грамматический род единиц (<c>один/одна/одно</c>, <c>два/две</c>) и
/// согласование разрядных слов (<c>тысяча/тысячи/тысяч</c>, <c>миллион/миллиона/миллионов</c> и т. д.).
/// Разряд «тысяча» имеет женский род, разряды от «миллиона» и выше — мужской.
/// </para>
/// <para>
/// Поддерживается весь диапазон <see cref="long"/>, включая отрицательные значения
/// (с приставкой «минус») и <see cref="long.MinValue"/>.
/// </para>
/// <para>
/// Винительный падеж формируется для неодушевлённого счёта: он совпадает с именительным,
/// кроме женского рода единицы («одну») и согласуемого с ней разрядного слова («одну тысячу»).
/// </para>
/// <para>
/// Метод не использует исключения и возвращает строку напрямую: для любого <see cref="long"/>
/// и любого падежа результат определён.
/// </para>
/// </remarks>
public static class RussianNumberToWords
{
  private const string _minus = "минус";

  // Порядок форм: [Именительный, Родительный, Дательный, Винительный, Творительный, Предложный].
  private static readonly string[] _zero =
    ["ноль", "нуля", "нулю", "ноль", "нулём", "нуле"];

  private static readonly string[][] _unitsMasculine =
  [
    ["", "", "", "", "", ""],
    ["один", "одного", "одному", "один", "одним", "одном"],
    ["два", "двух", "двум", "два", "двумя", "двух"],
    ["три", "трёх", "трём", "три", "тремя", "трёх"],
    ["четыре", "четырёх", "четырём", "четыре", "четырьмя", "четырёх"],
    ["пять", "пяти", "пяти", "пять", "пятью", "пяти"],
    ["шесть", "шести", "шести", "шесть", "шестью", "шести"],
    ["семь", "семи", "семи", "семь", "семью", "семи"],
    ["восемь", "восьми", "восьми", "восемь", "восемью", "восьми"],
    ["девять", "девяти", "девяти", "девять", "девятью", "девяти"]
  ];

  private static readonly string[][] _unitsFeminine =
  [
    ["", "", "", "", "", ""],
    ["одна", "одной", "одной", "одну", "одной", "одной"],
    ["две", "двух", "двум", "две", "двумя", "двух"],
    ["три", "трёх", "трём", "три", "тремя", "трёх"],
    ["четыре", "четырёх", "четырём", "четыре", "четырьмя", "четырёх"],
    ["пять", "пяти", "пяти", "пять", "пятью", "пяти"],
    ["шесть", "шести", "шести", "шесть", "шестью", "шести"],
    ["семь", "семи", "семи", "семь", "семью", "семи"],
    ["восемь", "восьми", "восьми", "восемь", "восемью", "восьми"],
    ["девять", "девяти", "девяти", "девять", "девятью", "девяти"]
  ];

  private static readonly string[][] _unitsNeuter =
  [
    ["", "", "", "", "", ""],
    ["одно", "одного", "одному", "одно", "одним", "одном"],
    ["два", "двух", "двум", "два", "двумя", "двух"],
    ["три", "трёх", "трём", "три", "тремя", "трёх"],
    ["четыре", "четырёх", "четырём", "четыре", "четырьмя", "четырёх"],
    ["пять", "пяти", "пяти", "пять", "пятью", "пяти"],
    ["шесть", "шести", "шести", "шесть", "шестью", "шести"],
    ["семь", "семи", "семи", "семь", "семью", "семи"],
    ["восемь", "восьми", "восьми", "восемь", "восемью", "восьми"],
    ["девять", "девяти", "девяти", "девять", "девятью", "девяти"]
  ];

  // Значения 10..19 (индекс = value - 10). Склоняются по «мягкому» образцу.
  private static readonly string[][] _teens =
  [
    SoftSign("десять"),
    SoftSign("одиннадцать"),
    SoftSign("двенадцать"),
    SoftSign("тринадцать"),
    SoftSign("четырнадцать"),
    SoftSign("пятнадцать"),
    SoftSign("шестнадцать"),
    SoftSign("семнадцать"),
    SoftSign("восемнадцать"),
    SoftSign("девятнадцать")
  ];

  // Десятки (индекс = число десятков). 0 и 1 не используются.
  private static readonly string[][] _tens =
  [
    ["", "", "", "", "", ""],
    ["", "", "", "", "", ""],
    SoftSign("двадцать"),
    SoftSign("тридцать"),
    ["сорок", "сорока", "сорока", "сорок", "сорока", "сорока"],
    ["пятьдесят", "пятидесяти", "пятидесяти", "пятьдесят", "пятьюдесятью", "пятидесяти"],
    ["шестьдесят", "шестидесяти", "шестидесяти", "шестьдесят", "шестьюдесятью", "шестидесяти"],
    ["семьдесят", "семидесяти", "семидесяти", "семьдесят", "семьюдесятью", "семидесяти"],
    ["восемьдесят", "восьмидесяти", "восьмидесяти", "восемьдесят", "восемьюдесятью", "восьмидесяти"],
    ["девяносто", "девяноста", "девяноста", "девяносто", "девяноста", "девяноста"]
  ];

  // Сотни (индекс = число сотен). 0 не используется.
  private static readonly string[][] _hundreds =
  [
    ["", "", "", "", "", ""],
    ["сто", "ста", "ста", "сто", "ста", "ста"],
    ["двести", "двухсот", "двумстам", "двести", "двумястами", "двухстах"],
    ["триста", "трёхсот", "трёмстам", "триста", "тремястами", "трёхстах"],
    ["четыреста", "четырёхсот", "четырёмстам", "четыреста", "четырьмястами", "четырёхстах"],
    ["пятьсот", "пятисот", "пятистам", "пятьсот", "пятьюстами", "пятистах"],
    ["шестьсот", "шестисот", "шестистам", "шестьсот", "шестьюстами", "шестистах"],
    ["семьсот", "семисот", "семистам", "семьсот", "семьюстами", "семистах"],
    ["восемьсот", "восьмисот", "восьмистам", "восемьсот", "восемьюстами", "восьмистах"],
    ["девятьсот", "девятисот", "девятистам", "девятьсот", "девятьюстами", "девятистах"]
  ];

  // Разрядные слова (индекс = разряд: 1 = тысяча, 2 = миллион, ...).
  // Для каждого разряда: [единственное число (6 падежей), множественное число (6 падежей)].
  private static readonly string[][][] _scales =
  [
    [[], []],
    [
      ["тысяча", "тысячи", "тысяче", "тысячу", "тысячей", "тысяче"],
      ["тысячи", "тысяч", "тысячам", "тысячи", "тысячами", "тысячах"]
    ],
    [
      ["миллион", "миллиона", "миллиону", "миллион", "миллионом", "миллионе"],
      ["миллионы", "миллионов", "миллионам", "миллионы", "миллионами", "миллионах"]
    ],
    [
      ["миллиард", "миллиарда", "миллиарду", "миллиард", "миллиардом", "миллиарде"],
      ["миллиарды", "миллиардов", "миллиардам", "миллиарды", "миллиардами", "миллиардах"]
    ],
    [
      ["триллион", "триллиона", "триллиону", "триллион", "триллионом", "триллионе"],
      ["триллионы", "триллионов", "триллионам", "триллионы", "триллионами", "триллионах"]
    ],
    [
      ["квадриллион", "квадриллиона", "квадриллиону", "квадриллион", "квадриллионом", "квадриллионе"],
      ["квадриллионы", "квадриллионов", "квадриллионам", "квадриллионы", "квадриллионами", "квадриллионах"]
    ],
    [
      ["квинтиллион", "квинтиллиона", "квинтиллиону", "квинтиллион", "квинтиллионом", "квинтиллионе"],
      ["квинтиллионы", "квинтиллионов", "квинтиллионам", "квинтиллионы", "квинтиллионами", "квинтиллионах"]
    ]
  ];

  /// <summary>
  /// Преобразует число в запись словами на русском языке в именительном падеже.
  /// </summary>
  /// <param name="number">Преобразуемое число. Поддерживается весь диапазон <see cref="long"/>.</param>
  /// <param name="gender">Грамматический род единиц младшего разряда. По умолчанию мужской.</param>
  /// <returns>Число, записанное словами в именительном падеже.</returns>
  public static string Convert(long number, RussianGender gender = RussianGender.Masculine) =>
    Convert(number, RussianCase.Nominative, gender);

  /// <summary>
  /// Преобразует число в запись словами на русском языке в заданном падеже.
  /// </summary>
  /// <param name="number">Преобразуемое число. Поддерживается весь диапазон <see cref="long"/>.</param>
  /// <param name="grammaticalCase">Падеж, в котором записывается числительное.</param>
  /// <param name="gender">Грамматический род единиц младшего разряда. По умолчанию мужской.</param>
  /// <returns>Число, записанное словами в указанном падеже.</returns>
  public static string Convert(long number, RussianCase grammaticalCase, RussianGender gender = RussianGender.Masculine)
  {
    int caseIndex = (int)grammaticalCase;

    if (number == 0)
      return _zero[caseIndex];

    // unchecked-отрицание корректно обрабатывает long.MinValue.
    ulong magnitude = number < 0 ? unchecked((ulong)(-number)) : (ulong)number;

    var words = new List<string>();

    if (number < 0)
      words.Add(_minus);

    AppendMagnitude(words, magnitude, caseIndex, gender);

    return string.Join(' ', words);
  }

  private static void AppendMagnitude(List<string> words, ulong magnitude, int caseIndex, RussianGender unitsGender)
  {
    // Разбиваем на триады (0..999), младшая — с индексом 0.
    Span<int> triads = stackalloc int[_scales.Length];
    int triadCount = 0;

    while (magnitude > 0)
    {
      triads[triadCount++] = (int)(magnitude % 1000);
      magnitude /= 1000;
    }

    for (int scale = triadCount - 1; scale >= 0; scale--)
    {
      int triad = triads[scale];

      if (triad == 0)
        continue;

      RussianGender triadGender = scale switch
      {
        0 => unitsGender,
        1 => RussianGender.Feminine,
        _ => RussianGender.Masculine
      };

      AppendTriad(words, triad, caseIndex, triadGender);

      if (scale > 0)
        words.Add(ScaleWord(scale, triad, caseIndex));
    }
  }

  private static void AppendTriad(List<string> words, int triad, int caseIndex, RussianGender gender)
  {
    int hundreds = triad / 100;
    int lastTwo = triad % 100;

    if (hundreds > 0)
      words.Add(_hundreds[hundreds][caseIndex]);

    if (lastTwo >= 10 && lastTwo <= 19)
    {
      words.Add(_teens[lastTwo - 10][caseIndex]);
      return;
    }

    int tens = lastTwo / 10;
    int units = lastTwo % 10;

    if (tens > 0)
      words.Add(_tens[tens][caseIndex]);

    if (units > 0)
      words.Add(UnitWord(units, caseIndex, gender));
  }

  private static string UnitWord(int unit, int caseIndex, RussianGender gender)
  {
    string[][] table = gender switch
    {
      RussianGender.Feminine => _unitsFeminine,
      RussianGender.Neuter => _unitsNeuter,
      _ => _unitsMasculine
    };

    return table[unit][caseIndex];
  }

  private static string ScaleWord(int scale, int triad, int caseIndex)
  {
    string[] singular = _scales[scale][0];
    string[] plural = _scales[scale][1];
    RussianPluralForm form = RussianPlural.GetForm(triad);

    // Именительный и винительный: счётные формы (одна тысяча, две тысячи, пять тысяч).
    if (caseIndex == (int)RussianCase.Nominative || caseIndex == (int)RussianCase.Accusative)
    {
      return form switch
      {
        // Винительный единицы отличается от именительного только в ж. р. («одну тысячу»).
        RussianPluralForm.One => singular[caseIndex],
        RussianPluralForm.Few => singular[(int)RussianCase.Genitive],
        _ => plural[(int)RussianCase.Genitive]
      };
    }

    // Косвенные падежи: «один» -> существительное в ед. ч., иначе во мн. ч.
    return form == RussianPluralForm.One
      ? singular[caseIndex]
      : plural[caseIndex];
  }

  // Склонение слов на мягкий знак (десять, одиннадцать, ..., двадцать, тридцать).
  private static string[] SoftSign(string nominative)
  {
    string stem = nominative[..^1];
    string oblique = stem + "и";

    return [nominative, oblique, oblique, nominative, stem + "ью", oblique];
  }

  /// <summary>Слово «минус». Используется обратным парсером.</summary>
  internal static string MinusWord => _minus;

  /// <summary>Все падежные формы слова «ноль». Используются обратным парсером.</summary>
  internal static IReadOnlyList<string> ZeroForms => _zero;

  /// <summary>
  /// Перечисляет все словоформы слагаемых (единицы, 10–19, десятки, сотни) во всех падежах и родах
  /// вместе с их числовым значением. Используется обратным парсером для построения словаря.
  /// </summary>
  internal static IEnumerable<(string Form, int Value)> EnumerateAdderForms()
  {
    for (int value = 1; value <= 9; value++)
    {
      foreach (string form in _unitsMasculine[value])
        if (form.Length > 0)
          yield return (form, value);

      foreach (string form in _unitsFeminine[value])
        if (form.Length > 0)
          yield return (form, value);

      foreach (string form in _unitsNeuter[value])
        if (form.Length > 0)
          yield return (form, value);
    }

    for (int teen = 0; teen <= 9; teen++)
      foreach (string form in _teens[teen])
        yield return (form, 10 + teen);

    for (int tens = 2; tens <= 9; tens++)
      foreach (string form in _tens[tens])
        if (form.Length > 0)
          yield return (form, tens * 10);

    for (int hundreds = 1; hundreds <= 9; hundreds++)
      foreach (string form in _hundreds[hundreds])
        if (form.Length > 0)
          yield return (form, hundreds * 100);
  }

  /// <summary>
  /// Перечисляет все словоформы разрядных слов (тысяча, миллион, ...) во всех падежах и числах
  /// вместе с индексом разряда. Используется обратным парсером.
  /// </summary>
  internal static IEnumerable<(string Form, int Scale)> EnumerateScaleForms()
  {
    for (int scale = 1; scale < _scales.Length; scale++)
    {
      foreach (string form in _scales[scale][0])
        yield return (form, scale);

      foreach (string form in _scales[scale][1])
        yield return (form, scale);
    }
  }
}
