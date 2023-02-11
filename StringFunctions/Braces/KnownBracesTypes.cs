namespace StringFunctions.Braces;

[Flags]
public enum KnownBracesTypes
{
  Other = 0,
  RoundedBraces = 1,
  SquareBraces = 1 << 1,
  FigureBraces = 1 << 2,
  CommonBraces = RoundedBraces | SquareBraces | FigureBraces,
  CornerBraces = 1 << 3,
  AllBraces = CommonBraces | CornerBraces,
  Quotas = 1 << 4,
  Apostrofe = 1 << 5,
  CommonQuotas = Quotas | Apostrofe,
  CommonSymbols = CommonBraces | CommonQuotas,
  CornerQuotas = 1 << 6,
  HandWritesQuotas = 1 << 7,
  AllQuotas = CommonQuotas | CornerQuotas | HandWritesQuotas,
  Tilda = 1 << 8,
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