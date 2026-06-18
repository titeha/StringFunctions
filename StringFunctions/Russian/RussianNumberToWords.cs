using System.Text;

namespace StringFunctions.Russian;

/// <summary>
/// Преобразует целые числа в их запись словами на русском языке (прописью), в именительном падеже.
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
/// Метод не использует исключения и возвращает строку напрямую: для любого <see cref="long"/>
/// результат определён.
/// </para>
/// </remarks>
public static class RussianNumberToWords
{
  private const string _zero = "ноль";
  private const string _minus = "минус";

  private static readonly string[] _unitsMasculine =
    ["", "один", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять"];

  private static readonly string[] _unitsFeminine =
    ["", "одна", "две", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять"];

  private static readonly string[] _unitsNeuter =
    ["", "одно", "два", "три", "четыре", "пять", "шесть", "семь", "восемь", "девять"];

  private static readonly string[] _teens =
    ["десять", "одиннадцать", "двенадцать", "тринадцать", "четырнадцать",
     "пятнадцать", "шестнадцать", "семнадцать", "восемнадцать", "девятнадцать"];

  private static readonly string[] _tens =
    ["", "", "двадцать", "тридцать", "сорок", "пятьдесят",
     "шестьдесят", "семьдесят", "восемьдесят", "девяносто"];

  private static readonly string[] _hundreds =
    ["", "сто", "двести", "триста", "четыреста", "пятьсот",
     "шестьсот", "семьсот", "восемьсот", "девятьсот"];

  // Разрядные слова по индексу разряда (1 = тысяча, 2 = миллион, ...) в трёх формах.
  private static readonly string[][] _scales =
  [
    [],
    ["тысяча", "тысячи", "тысяч"],
    ["миллион", "миллиона", "миллионов"],
    ["миллиард", "миллиарда", "миллиардов"],
    ["триллион", "триллиона", "триллионов"],
    ["квадриллион", "квадриллиона", "квадриллионов"],
    ["квинтиллион", "квинтиллиона", "квинтиллионов"]
  ];

  /// <summary>
  /// Преобразует число в запись словами на русском языке.
  /// </summary>
  /// <param name="number">Преобразуемое число. Поддерживается весь диапазон <see cref="long"/>.</param>
  /// <param name="gender">
  /// Грамматический род единиц последнего (младшего) разряда: влияет на «один/одна/одно» и «два/две».
  /// По умолчанию мужской.
  /// </param>
  /// <returns>Число, записанное словами в именительном падеже.</returns>
  public static string Convert(long number, RussianGender gender = RussianGender.Masculine)
  {
    if (number == 0)
      return _zero;

    // unchecked-отрицание корректно обрабатывает long.MinValue: его модуль не помещается
    // в long, но в ulong укладывается ровно.
    ulong magnitude = number < 0 ? unchecked((ulong)(-number)) : (ulong)number;

    var words = new List<string>();

    if (number < 0)
      words.Add(_minus);

    AppendMagnitude(words, magnitude, gender);

    return string.Join(' ', words);
  }

  private static void AppendMagnitude(List<string> words, ulong magnitude, RussianGender unitsGender)
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

      AppendTriad(words, triad, triadGender);

      if (scale > 0)
        words.Add(_scales[scale][ScaleFormIndex(triad)]);
    }
  }

  private static void AppendTriad(List<string> words, int triad, RussianGender gender)
  {
    int hundreds = triad / 100;
    int lastTwo = triad % 100;

    if (hundreds > 0)
      words.Add(_hundreds[hundreds]);

    if (lastTwo >= 10 && lastTwo <= 19)
    {
      words.Add(_teens[lastTwo - 10]);
      return;
    }

    int tens = lastTwo / 10;
    int units = lastTwo % 10;

    if (tens > 0)
      words.Add(_tens[tens]);

    if (units > 0)
      words.Add(UnitWord(units, gender));
  }

  private static string UnitWord(int unit, RussianGender gender) =>
    gender switch
    {
      RussianGender.Feminine => _unitsFeminine[unit],
      RussianGender.Neuter => _unitsNeuter[unit],
      _ => _unitsMasculine[unit]
    };

  private static int ScaleFormIndex(int triad) =>
    RussianPlural.GetForm(triad) switch
    {
      RussianPluralForm.One => 0,
      RussianPluralForm.Few => 1,
      _ => 2
    };
}
