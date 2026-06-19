namespace StringFunctions.Russian;

/// <summary>
/// Способ записи разменной части суммы (копеек, центов).
/// </summary>
public enum RussianMinorFormat
{
  /// <summary>Прописью с согласованием: «пять копеек».</summary>
  Words,

  /// <summary>Цифрами с сокращением и ведущими нулями: «05 коп.».</summary>
  Digits
}
