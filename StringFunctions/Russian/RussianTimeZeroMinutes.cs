namespace StringFunctions.Russian;

/// <summary>
/// Как озвучивать «цифровое» время, когда минут ноль (ровно час).
/// </summary>
public enum RussianTimeZeroMinutes
{
  /// <summary>Опускать минуты: «пятнадцать часов».</summary>
  Omit,

  /// <summary>Добавлять слово «ровно»: «пятнадцать часов ровно».</summary>
  Sharp
}
