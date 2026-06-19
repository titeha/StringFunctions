using System.Text;

using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Записывает время прописью на русском языке в «цифровом» (официальном) виде:
/// «пятнадцать часов тридцать минут».
/// </summary>
/// <remarks>
/// <para>
/// Используется 24-часовой формат. Часы согласуются со словом «час», минуты — со словом «минута»
/// (женский род: «одна минута», «две минуты»). Поведение при нулевых минутах задаётся
/// через <see cref="RussianTimeZeroMinutes"/>.
/// </para>
/// <para>
/// Ошибки входных данных возвращаются через <see cref="Result{T}"/>.
/// </para>
/// </remarks>
public static class RussianTimeToWords
{
  private const string _sharp = "ровно";

  private static readonly RussianNoun _hour = new("час", "часа", "часов", RussianGender.Masculine);
  private static readonly RussianNoun _minute = new("минута", "минуты", "минут", RussianGender.Feminine);

  /// <summary>
  /// Записывает время прописью по часам и минутам.
  /// </summary>
  /// <param name="hours">Часы, 0–23.</param>
  /// <param name="minutes">Минуты, 0–59.</param>
  /// <param name="zeroMinutes">Поведение при нулевых минутах. По умолчанию минуты опускаются.</param>
  /// <returns>Время прописью либо ошибка валидации.</returns>
  public static Result<string> Convert(
    int hours,
    int minutes,
    RussianTimeZeroMinutes zeroMinutes = RussianTimeZeroMinutes.Omit)
  {
    if (hours is < 0 or > 23)
      return Result.Failure<string>($"Часы должны быть в диапазоне 0..23. Значение: {hours}.");

    if (minutes is < 0 or > 59)
      return Result.Failure<string>($"Минуты должны быть в диапазоне 0..59. Значение: {minutes}.");

    var builder = new StringBuilder();

    builder.Append(RussianNumberToWords.Convert(hours, _hour.Gender));
    builder.Append(' ');
    builder.Append(_hour.Form(hours));

    if (minutes == 0)
    {
      if (zeroMinutes == RussianTimeZeroMinutes.Sharp)
      {
        builder.Append(' ');
        builder.Append(_sharp);
      }
    }
    else
    {
      builder.Append(' ');
      builder.Append(RussianNumberToWords.Convert(minutes, _minute.Gender));
      builder.Append(' ');
      builder.Append(_minute.Form(minutes));
    }

    return Result.Success(builder.ToString());
  }

  /// <summary>
  /// Записывает время прописью по <see cref="TimeOnly"/>.
  /// </summary>
  /// <param name="time">Время (используются часы и минуты).</param>
  /// <param name="zeroMinutes">Поведение при нулевых минутах. По умолчанию минуты опускаются.</param>
  /// <returns>Время прописью.</returns>
  public static Result<string> Convert(TimeOnly time, RussianTimeZeroMinutes zeroMinutes = RussianTimeZeroMinutes.Omit) =>
    Convert(time.Hour, time.Minute, zeroMinutes);
}
