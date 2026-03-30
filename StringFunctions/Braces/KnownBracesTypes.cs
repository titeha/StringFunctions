namespace StringFunctions.Braces;

/// <summary>
/// Определяет известные наборы скобок и кавычек, которые могут использоваться
/// при проверке баланса символов в строке.
/// </summary>
/// <remarks>
/// Перечисление поддерживает побитовое комбинирование значений.
/// </remarks>
[Flags]
public enum KnownBracesTypes
{
  /// <summary>Пустой набор символов.</summary>
  Other = 0,

  /// <summary>Круглые скобки: <c>()</c>.</summary>
  RoundedBraces = 1,

  /// <summary>Квадратные скобки: <c>[]</c>.</summary>
  SquareBraces = 1 << 1,

  /// <summary>Фигурные скобки: <c>{}</c>.</summary>
  FigureBraces = 1 << 2,

  /// <summary>Базовый набор распространённых скобок: круглые, квадратные и фигурные.</summary>
  CommonBraces = RoundedBraces | SquareBraces | FigureBraces,

  /// <summary>Угловые скобки: <c>&lt;&gt;</c>.</summary>
  CornerBraces = 1 << 3,

  /// <summary>Все поддерживаемые виды скобок.</summary>
  AllBraces = CommonBraces | CornerBraces,

  /// <summary>Обычные двойные кавычки: <c>""</c>.</summary>
  Quotas = 1 << 4,

  /// <summary>Одиночные кавычки: <c>''</c>.</summary>
  Apostrofe = 1 << 5,

  /// <summary>Базовый набор распространённых кавычек: двойные и одиночные.</summary>
  CommonQuotas = Quotas | Apostrofe,

  /// <summary>Смешанный набор распространённых скобок и кавычек.</summary>
  CommonSymbols = CommonBraces | CommonQuotas,

  /// <summary>Угловые кавычки: <c>«»</c>.</summary>
  CornerQuotas = 1 << 6,

  /// <summary>Типографские кавычки: <c>“”</c>.</summary>
  HandWritesQuotas = 1 << 7,

  /// <summary>Все поддерживаемые виды кавычек.</summary>
  AllQuotas = CommonQuotas | CornerQuotas | HandWritesQuotas,

  /// <summary>Тильда: <c>~~</c>.</summary>
  Tilda = 1 << 8,

  /// <summary>Полный набор всех поддерживаемых скобок и кавычек.</summary>
  All = AllBraces | AllQuotas | Tilda
}

internal static class KnownBracesTypesExtension
{
  public static bool IsRoundedBraces(this KnownBracesTypes value) => (value & KnownBracesTypes.RoundedBraces) == KnownBracesTypes.RoundedBraces;

  public static bool IsSquareBraces(this KnownBracesTypes value) => (value & KnownBracesTypes.SquareBraces) == KnownBracesTypes.SquareBraces;

  public static bool IsFigureBraces(this KnownBracesTypes value) => (value & KnownBracesTypes.FigureBraces) == KnownBracesTypes.FigureBraces;

  public static bool IsCornerBraces(this KnownBracesTypes value) => (value & KnownBracesTypes.CornerBraces) == KnownBracesTypes.CornerBraces;

  public static bool IsQuotas(this KnownBracesTypes value) => (value & KnownBracesTypes.Quotas) == KnownBracesTypes.Quotas;

  public static bool IsApostrofe(this KnownBracesTypes value) => (value & KnownBracesTypes.Apostrofe) == KnownBracesTypes.Apostrofe;

  public static bool IsCornerQuotas(this KnownBracesTypes value) => (value & KnownBracesTypes.CornerQuotas) == KnownBracesTypes.CornerQuotas;

  public static bool IsHandwriteQuotas(this KnownBracesTypes value) => (value & KnownBracesTypes.HandWritesQuotas) == KnownBracesTypes.HandWritesQuotas;

  public static bool IsTilda(this KnownBracesTypes value) => (value & KnownBracesTypes.Tilda) == KnownBracesTypes.Tilda;

  public static bool IsEmpty(this KnownBracesTypes value) => value == KnownBracesTypes.Other
                                                             || !(value.IsApostrofe()
                                                             || value.IsCornerBraces()
                                                             || value.IsCornerQuotas()
                                                             || value.IsFigureBraces()
                                                             || value.IsHandwriteQuotas()
                                                             || value.IsQuotas()
                                                             || value.IsRoundedBraces()
                                                             || value.IsSquareBraces()
                                                             || value.IsTilda());
}