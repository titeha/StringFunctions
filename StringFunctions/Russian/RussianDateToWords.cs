using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Записывает дату прописью на русском языке: «девятнадцатое июня две тысячи двадцать шестого года».
/// </summary>
/// <remarks>
/// <para>
/// День выводится порядковым числительным среднего рода, месяц — в родительном падеже,
/// год — порядковым числительным в родительном падеже со словом «года».
/// </para>
/// <para>
/// Падеж дня выбирается параметром: именительный («девятнадцатое июня …») либо
/// родительный («девятнадцатого июня …»). Ошибки входных данных возвращаются через <see cref="Result{T}"/>.
/// </para>
/// </remarks>
public static class RussianDateToWords
{
  private static readonly string[] _monthsGenitive =
  [
    "", "января", "февраля", "марта", "апреля", "мая", "июня",
    "июля", "августа", "сентября", "октября", "ноября", "декабря"
  ];

  /// <summary>
  /// Записывает дату прописью по составляющим.
  /// </summary>
  /// <param name="year">Год (положительный).</param>
  /// <param name="month">Месяц, 1–12.</param>
  /// <param name="day">День, 1–31.</param>
  /// <param name="dayCase">
  /// Падеж дня: <see cref="RussianCase.Nominative"/> («девятнадцатое …») либо
  /// <see cref="RussianCase.Genitive"/> («девятнадцатого …»). По умолчанию именительный.
  /// </param>
  /// <returns>Дата прописью либо ошибка валидации.</returns>
  public static Result<string> Convert(
    int year,
    int month,
    int day,
    RussianCase dayCase = RussianCase.Nominative)
  {
    if (year < 1)
      return Result.Failure<string>("Год должен быть положительным.");

    if (month is < 1 or > 12)
      return Result.Failure<string>($"Месяц должен быть в диапазоне 1..12. Значение: {month}.");

    if (day is < 1 or > 31)
      return Result.Failure<string>($"День должен быть в диапазоне 1..31. Значение: {day}.");

    if (!Enum.IsDefined(typeof(RussianCase), dayCase))
      return Result.Failure<string>($"Недопустимый падеж дня. Значение: {(int)dayCase}.");

    if (!IsValidDate(year, month, day))
      return Result.Failure<string>($"Дата не существует. Значение: {year:D4}-{month:D2}-{day:D2}.");

    string dayWords = RussianOrdinalToWords.Convert(day, RussianGender.Neuter, dayCase);
    string monthWords = _monthsGenitive[month];
    string yearWords = RussianOrdinalToWords.Convert(year, RussianGender.Masculine, RussianCase.Genitive);

    return Result.Success($"{dayWords} {monthWords} {yearWords} года");
  }


  private static bool IsValidDate(int year, int month, int day) =>
    day <= GetDaysInMonth(year, month);

  private static int GetDaysInMonth(int year, int month) =>
    month switch
    {
      2 => IsLeapYear(year) ? 29 : 28,
      4 or 6 or 9 or 11 => 30,
      _ => 31
    };

  private static bool IsLeapYear(int year) =>
    year % 4 == 0 && (year % 100 != 0 || year % 400 == 0);

  /// <summary>
  /// Записывает дату прописью по <see cref="DateOnly"/>.
  /// </summary>
  /// <param name="date">Дата.</param>
  /// <param name="dayCase">Падеж дня. По умолчанию именительный.</param>
  /// <returns>Дата прописью.</returns>
  public static Result<string> Convert(DateOnly date, RussianCase dayCase = RussianCase.Nominative) =>
    Convert(date.Year, date.Month, date.Day, dayCase);

  /// <summary>
  /// Записывает дату прописью по <see cref="DateTime"/> (используется только дата).
  /// </summary>
  /// <param name="date">Дата и время.</param>
  /// <param name="dayCase">Падеж дня. По умолчанию именительный.</param>
  /// <returns>Дата прописью.</returns>
  public static Result<string> Convert(DateTime date, RussianCase dayCase = RussianCase.Nominative) =>
    Convert(date.Year, date.Month, date.Day, dayCase);
}
