using System.Text;

namespace StringFunctions.Russian;

/// <summary>
/// Преобразует целые числа в порядковые числительные словами на русском языке
/// («двадцать первый», «две тысячи двадцать шестой») с учётом рода и падежа.
/// </summary>
/// <remarks>
/// <para>
/// В составном порядковом числительном склоняется и принимает род только последнее слово;
/// предшествующая часть остаётся количественным числительным в именительном падеже.
/// </para>
/// <para>
/// Отрицательные числа записываются с приставкой «минус». Метод не использует исключения.
/// </para>
/// </remarks>
public static class RussianOrdinalToWords
{
  private const string _minus = "минус";
  private const string _leadingOneThousand = "одна тысяча";
  private const string _thirdMasculine = "третий";

  private static readonly ulong[] _scalePow =
    [1UL, 1_000UL, 1_000_000UL, 1_000_000_000UL, 1_000_000_000_000UL, 1_000_000_000_000_000UL, 1_000_000_000_000_000_000UL];

  private static readonly string[] _unitOrdinal =
    ["", "первый", "второй", "третий", "четвёртый", "пятый", "шестой", "седьмой", "восьмой", "девятый"];

  private static readonly string[] _teenOrdinal =
    ["десятый", "одиннадцатый", "двенадцатый", "тринадцатый", "четырнадцатый",
     "пятнадцатый", "шестнадцатый", "семнадцатый", "восемнадцатый", "девятнадцатый"];

  private static readonly string[] _tensOrdinal =
    ["", "", "двадцатый", "тридцатый", "сороковой", "пятидесятый",
     "шестидесятый", "семидесятый", "восьмидесятый", "девяностый"];

  private static readonly string[] _hundredsOrdinal =
    ["", "сотый", "двухсотый", "трёхсотый", "четырёхсотый", "пятисотый",
     "шестисотый", "семисотый", "восьмисотый", "девятисотый"];

  private static readonly string[] _scaleOrdinal =
    ["", "тысячный", "миллионный", "миллиардный", "триллионный", "квадриллионный", "квинтиллионный"];

  // Соединительные основы для сложных порядковых (двухтысячный, стотысячный).
  private static readonly string[] _unitStem =
    ["", "одно", "двух", "трёх", "четырёх", "пяти", "шести", "семи", "восьми", "девяти"];

  private static readonly string[] _teenStem =
    ["десяти", "одиннадцати", "двенадцати", "тринадцати", "четырнадцати",
     "пятнадцати", "шестнадцати", "семнадцати", "восемнадцати", "девятнадцати"];

  private static readonly string[] _tensStem =
    ["", "", "двадцати", "тридцати", "сорока", "пятидесяти",
     "шестидесяти", "семидесяти", "восьмидесяти", "девяноста"];

  private static readonly string[] _hundredsStem =
    ["", "сто", "двухсот", "трёхсот", "четырёхсот", "пятисот",
     "шестисот", "семисот", "восьмисот", "девятисот"];

  // Падежные формы слова «третий» (мягкое склонение): [М, Ж, С] x 6 падежей.
  private static readonly string[][] _thirdForms =
  [
    ["третий", "третьего", "третьему", "третий", "третьим", "третьем"],
    ["третья", "третьей", "третьей", "третью", "третьей", "третьей"],
    ["третье", "третьего", "третьему", "третье", "третьим", "третьем"]
  ];

  /// <summary>
  /// Преобразует число в порядковое числительное словами.
  /// </summary>
  /// <param name="number">Преобразуемое число.</param>
  /// <param name="gender">Грамматический род. По умолчанию мужской.</param>
  /// <param name="grammaticalCase">Падеж. По умолчанию именительный.</param>
  /// <returns>Порядковое числительное словами.</returns>
  public static string Convert(
    long number,
    RussianGender gender = RussianGender.Masculine,
    RussianCase grammaticalCase = RussianCase.Nominative)
  {
    int caseIndex = (int)grammaticalCase;

    if (number == 0)
      return DeclineOrdinal("нулевой", gender, caseIndex);

    bool negative = number < 0;
    ulong magnitude = negative ? unchecked((ulong)(-number)) : (ulong)number;

    string ordinalMasculine = ResolveOrdinalWord(magnitude, out ulong cardinalPrefixValue);

    var parts = new List<string>();

    if (negative)
      parts.Add(_minus);

    if (cardinalPrefixValue > 0)
      parts.Add(FixLeadingOneThousand(RussianNumberToWords.Convert((long)cardinalPrefixValue)));

    parts.Add(DeclineOrdinal(ordinalMasculine, gender, caseIndex));

    return string.Join(' ', parts);
  }

  // Возвращает порядковое слово в мужском роде именительного падежа и величину
  // количественной части, которую нужно вывести перед ним.
  private static string ResolveOrdinalWord(ulong magnitude, out ulong cardinalPrefixValue)
  {
    Span<int> triads = stackalloc int[_scalePow.Length];
    int triadCount = 0;
    ulong rest = magnitude;

    while (rest > 0)
    {
      triads[triadCount++] = (int)(rest % 1000);
      rest /= 1000;
    }

    int lowestNonZero = 0;
    while (triads[lowestNonZero] == 0)
      lowestNonZero++;

    // Младшая ненулевая триада — это разряд тысяч и выше: порядковое образуется
    // от разрядного слова (тысячный, двухтысячный, миллионный).
    if (lowestNonZero >= 1)
    {
      int value = triads[lowestNonZero];
      cardinalPrefixValue = magnitude - (ulong)value * _scalePow[lowestNonZero];

      return value == 1
        ? _scaleOrdinal[lowestNonZero]
        : CompoundStem(value) + _scaleOrdinal[lowestNonZero];
    }

    // Младшая ненулевая триада — единицы: порядковым становится последний элемент.
    int triad = triads[0];
    int hundreds = triad / 100;
    int lastTwo = triad % 100;

    if (lastTwo >= 10 && lastTwo <= 19)
    {
      cardinalPrefixValue = magnitude - (ulong)lastTwo;
      return _teenOrdinal[lastTwo - 10];
    }

    int tens = lastTwo / 10;
    int units = lastTwo % 10;

    if (units != 0)
    {
      cardinalPrefixValue = magnitude - (ulong)units;
      return _unitOrdinal[units];
    }

    if (tens != 0)
    {
      cardinalPrefixValue = magnitude - (ulong)(tens * 10);
      return _tensOrdinal[tens];
    }

    cardinalPrefixValue = magnitude - (ulong)(hundreds * 100);
    return _hundredsOrdinal[hundreds];
  }

  private static string CompoundStem(int triad)
  {
    var builder = new StringBuilder();

    int hundreds = triad / 100;
    int lastTwo = triad % 100;

    if (hundreds > 0)
      builder.Append(_hundredsStem[hundreds]);

    if (lastTwo >= 10 && lastTwo <= 19)
    {
      builder.Append(_teenStem[lastTwo - 10]);
      return builder.ToString();
    }

    int tens = lastTwo / 10;
    int units = lastTwo % 10;

    if (tens > 0)
      builder.Append(_tensStem[tens]);

    if (units > 0)
      builder.Append(_unitStem[units]);

    return builder.ToString();
  }

  // Склонение порядкового слова как прилагательного по роду и падежу.
  private static string DeclineOrdinal(string masculineNominative, RussianGender gender, int caseIndex)
  {
    if (masculineNominative == _thirdMasculine)
      return _thirdForms[(int)gender][caseIndex];

    string stem = masculineNominative[..^2];

    return gender switch
    {
      RussianGender.Feminine => caseIndex switch
      {
        (int)RussianCase.Nominative => stem + "ая",
        (int)RussianCase.Accusative => stem + "ую",
        _ => stem + "ой"
      },
      RussianGender.Neuter => caseIndex switch
      {
        (int)RussianCase.Nominative => stem + "ое",
        (int)RussianCase.Accusative => stem + "ое",
        (int)RussianCase.Genitive => stem + "ого",
        (int)RussianCase.Dative => stem + "ому",
        (int)RussianCase.Instrumental => stem + "ым",
        _ => stem + "ом"
      },
      _ => caseIndex switch
      {
        (int)RussianCase.Nominative => masculineNominative,
        (int)RussianCase.Accusative => masculineNominative,
        (int)RussianCase.Genitive => stem + "ого",
        (int)RussianCase.Dative => stem + "ому",
        (int)RussianCase.Instrumental => stem + "ым",
        _ => stem + "ом"
      }
    };
  }

  private static string FixLeadingOneThousand(string cardinal) =>
    cardinal.StartsWith(_leadingOneThousand, StringComparison.Ordinal)
      ? cardinal[(_leadingOneThousand.Length - "тысяча".Length)..]
      : cardinal;
}
