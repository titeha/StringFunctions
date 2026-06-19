using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Записывает время прописью на русском языке в разговорном виде:
/// «половина седьмого», «без четверти семь», «пять минут седьмого».
/// </summary>
/// <remarks>
/// <para>
/// Разговорный формат неявно 12-часовой. До получаса минуты считаются в следующий час
/// («пять минут седьмого» — 6:05), после получаса — на убыль («без двадцати семь» — 6:40).
/// </para>
/// <para>
/// Конвенции: в форме «без …» слово «минут» опускается; час, равный единице, произносится как
/// «час» («без четверти час», «час дня»). По запросу добавляется часть суток
/// (ночи — 0–3, утра — 4–11, дня — 12–16, вечера — 17–23). Ошибки возвращаются через <see cref="Result{T}"/>.
/// </para>
/// </remarks>
public static class RussianColloquialTimeToWords
{
  private const string _without = "без";
  private const string _quarterNominative = "четверть";
  private const string _quarterGenitive = "четверти";
  private const string _half = "половина";
  private const string _oneHourWord = "час";

  private static readonly RussianNoun _hour = new("час", "часа", "часов", RussianGender.Masculine);
  private static readonly RussianNoun _minute = new("минута", "минуты", "минут", RussianGender.Feminine);

  /// <summary>
  /// Записывает время прописью в разговорном виде по часам и минутам.
  /// </summary>
  /// <param name="hours">Часы, 0–23.</param>
  /// <param name="minutes">Минуты, 0–59.</param>
  /// <param name="includePartOfDay">Добавлять ли часть суток («утра», «дня», «вечера», «ночи»).</param>
  /// <returns>Время прописью либо ошибка валидации.</returns>
  public static Result<string> Convert(int hours, int minutes, bool includePartOfDay = false)
  {
    if (hours is < 0 or > 23)
      return Result.Failure<string>($"Часы должны быть в диапазоне 0..23. Значение: {hours}.");

    if (minutes is < 0 or > 59)
      return Result.Failure<string>($"Минуты должны быть в диапазоне 0..59. Значение: {minutes}.");

    string core = BuildCore(hours, minutes);

    return Result.Success(includePartOfDay ? $"{core} {PartOfDay(hours)}" : core);
  }

  /// <summary>
  /// Записывает время прописью в разговорном виде по <see cref="TimeOnly"/>.
  /// </summary>
  /// <param name="time">Время (используются часы и минуты).</param>
  /// <param name="includePartOfDay">Добавлять ли часть суток.</param>
  /// <returns>Время прописью.</returns>
  public static Result<string> Convert(TimeOnly time, bool includePartOfDay = false) =>
    Convert(time.Hour, time.Minute, includePartOfDay);

  private static string BuildCore(int hours, int minutes)
  {
    if (minutes == 0)
      return WholeHour(hours);

    int nextHour = (hours % 12) + 1;

    if (minutes <= 30)
    {
      string minutesPart = minutes switch
      {
        15 => _quarterNominative,
        30 => _half,
        _ => $"{RussianNumberToWords.Convert(minutes, _minute.Gender)} {_minute.Form(minutes)}"
      };

      string ordinalHour = RussianOrdinalToWords.Convert(nextHour, RussianGender.Masculine, RussianCase.Genitive);
      return $"{minutesPart} {ordinalHour}";
    }

    int remaining = 60 - minutes;

    string remainingPart = remaining == 15
      ? _quarterGenitive
      : RussianNumberToWords.Convert(remaining, RussianCase.Genitive, _minute.Gender);

    string hourWord = nextHour == 1
      ? _oneHourWord
      : RussianNumberToWords.Convert(nextHour, _hour.Gender);

    return $"{_without} {remainingPart} {hourWord}";
  }

  private static string WholeHour(int hours)
  {
    int spokenHour = hours % 12 == 0 ? 12 : hours % 12;

    return spokenHour == 1
      ? _oneHourWord
      : $"{RussianNumberToWords.Convert(spokenHour, _hour.Gender)} {_hour.Form(spokenHour)}";
  }

  private static string PartOfDay(int hours) =>
    hours switch
    {
      <= 3 => "ночи",
      <= 11 => "утра",
      <= 16 => "дня",
      _ => "вечера"
    };
}
