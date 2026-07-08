namespace StringFunctions.Russian;

/// <summary>
/// Описание денежной единицы для записи суммы прописью: основная и разменная единицы,
/// число разменных единиц в основной и сокращение для записи разменной части цифрами.
/// </summary>
/// <remarks>
/// Готовые варианты доступны как статические свойства (<see cref="Rubles"/>, <see cref="Dollars"/>,
/// <see cref="Euros"/>); для других валют создайте экземпляр напрямую.
/// </remarks>
public sealed class RussianCurrency
{
  /// <summary>
  /// Создаёт описание валюты.
  /// </summary>
  /// <param name="major">Основная единица (рубль, доллар), её формы и род.</param>
  /// <param name="minor">Разменная единица (копейка, цент), её формы и род.</param>
  /// <param name="minorAbbreviation">Сокращение разменной части для записи цифрами, например «коп.».</param>
  /// <param name="minorUnitsPerMajor">Число разменных единиц в одной основной. По умолчанию 100.</param>
  /// <exception cref="ArgumentNullException"><paramref name="minorAbbreviation"/> равно <c>null</c>.</exception>
  /// <exception cref="ArgumentOutOfRangeException"><paramref name="minorUnitsPerMajor"/> меньше 1.</exception>
  public RussianCurrency(RussianNoun major, RussianNoun minor, string minorAbbreviation, int minorUnitsPerMajor = 100)
  {
    if (minorUnitsPerMajor < 1)
      throw new ArgumentOutOfRangeException(nameof(minorUnitsPerMajor), "Число разменных единиц должно быть не меньше 1.");

    if (!major.IsValid)
      throw new ArgumentException("Описание основной денежной единицы некорректно.", nameof(major));

    if (!minor.IsValid)
      throw new ArgumentException("Описание разменной денежной единицы некорректно.", nameof(minor));

    Major = major;
    Minor = minor;
    MinorAbbreviation = minorAbbreviation ?? throw new ArgumentNullException(nameof(minorAbbreviation));
    MinorUnitsPerMajor = minorUnitsPerMajor;
  }

  /// <summary>Основная денежная единица.</summary>
  public RussianNoun Major { get; }

  /// <summary>Разменная денежная единица.</summary>
  public RussianNoun Minor { get; }

  /// <summary>Сокращение разменной части для записи цифрами.</summary>
  public string MinorAbbreviation { get; }

  /// <summary>Число разменных единиц в одной основной.</summary>
  public int MinorUnitsPerMajor { get; }

  /// <summary>Российский рубль (рубль/копейка).</summary>
  public static RussianCurrency Rubles { get; } = new(
    new RussianNoun("рубль", "рубля", "рублей", RussianGender.Masculine),
    new RussianNoun("копейка", "копейки", "копеек", RussianGender.Feminine),
    "коп.");

  /// <summary>Доллар США (доллар/цент).</summary>
  public static RussianCurrency Dollars { get; } = new(
    new RussianNoun("доллар", "доллара", "долларов", RussianGender.Masculine),
    new RussianNoun("цент", "цента", "центов", RussianGender.Masculine),
    "ц.");

  /// <summary>Евро (евро — несклоняемое — / цент).</summary>
  public static RussianCurrency Euros { get; } = new(
    new RussianNoun("евро", "евро", "евро", RussianGender.Masculine),
    new RussianNoun("цент", "цента", "центов", RussianGender.Masculine),
    "ц.");
}
