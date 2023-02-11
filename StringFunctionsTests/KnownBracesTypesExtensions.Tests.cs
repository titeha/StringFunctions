namespace StringFunctions.Braces.Tests;

public class KnownBracesTypesExtensionsTests
{
  [Theory]
  [InlineData(KnownBracesTypes.RoundedBraces)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_rounded_braces_for_rounded_braces_contains_groups_Returns_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsRoundedBraces());

  [Theory]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_rounded_braces_for_another_braces_group_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsRoundedBraces());

  [Theory]
  [InlineData(KnownBracesTypes.SquareBraces)]
  [InlineData(KnownBracesTypes.CornerBraces)]
  public void Check_is_rounded_braces_for_no_rounded_braces_group_Return_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsRoundedBraces());

  [Theory]
  [InlineData(KnownBracesTypes.SquareBraces)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.All)]
  [InlineData(KnownBracesTypes.CommonBraces)]
  public void Check_is_square_braces_from_square_braces_contains_group_Return_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsSquareBraces());

  [Theory]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_square_braces_in_no_braces_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsSquareBraces());

  [Theory]
  [InlineData(KnownBracesTypes.RoundedBraces)]
  [InlineData(KnownBracesTypes.FigureBraces)]
  public void Check_is_square_braces_from_another_braces_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsSquareBraces());

  [Theory]
  [InlineData(KnownBracesTypes.CommonBraces)]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.FigureBraces)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_figure_braces_from_figure_braces_contains_sets_Return_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsFigureBraces());

  [Theory]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_if_figure_braces_from_no_braces_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsFigureBraces());

  [Theory]
  [InlineData(KnownBracesTypes.SquareBraces)]
  [InlineData(KnownBracesTypes.CornerBraces)]
  public void Check_is_figure_braces_from_another_braces_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsFigureBraces());

  [Theory]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.CornerBraces)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_corner_braces_from_corner_braces_contains_set_Return_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsCornerBraces());

  [Theory]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_corner_bracse_from_no_braces_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsCornerBraces());

  [Theory]
  [InlineData(KnownBracesTypes.CommonBraces)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.SquareBraces)]
  [InlineData(KnownBracesTypes.FigureBraces)]
  public void Check_is_corner_braces_from_another_braces_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsCornerBraces());

  [Theory]
  [InlineData(KnownBracesTypes.Quotas)]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.CommonQuotas)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_quotas_from_quotas_contains_set_Returns_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_quotas_from_no_quotas_set_Returns_true(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.CornerQuotas)]
  [InlineData(KnownBracesTypes.Apostrofe)]
  public void Check_is_quotas_from_another_quotas_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.Apostrofe)]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.CommonQuotas)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_apostrofe_from_apostrofe_contains_set_Return_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsApostrofe());

  [Theory]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_apostrofe_from_no_quotas_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsApostrofe());

  [Theory]
  [InlineData(KnownBracesTypes.CornerQuotas)]
  [InlineData(KnownBracesTypes.Quotas)]
  public void Check_is_apostrofe_from_another_quotas_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsApostrofe());

  [Theory]
  [InlineData(KnownBracesTypes.CornerQuotas)]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_corner_quotas_from_corner_quotas_contains_set_Returns_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsCornerQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.CommonQuotas)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_corner_quotas_from_no_corner_quotas_contains_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsCornerQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.Quotas)]
  [InlineData(KnownBracesTypes.HandWritesQuotas)]
  public void Check_is_corner_quotas_from_another_quotas_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsCornerQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.HandWritesQuotas)]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_handwrite_quotas_quotas_from_handwrite_quotas_set_Returns_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsHandwriteQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.CommonQuotas)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.AllBraces)]
  [InlineData(KnownBracesTypes.Tilda)]
  public void Check_is_handwrite_quotas_from_no_quotas_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsHandwriteQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.Quotas)]
  [InlineData(KnownBracesTypes.CommonQuotas)]
  public void Check_is_handwrite_quotas_from_another_quotas_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsHandwriteQuotas());

  [Theory]
  [InlineData(KnownBracesTypes.Tilda)]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_tilda_from_tilda_contains_set_Returns_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsTilda());

  [Theory]
  [InlineData(KnownBracesTypes.AllQuotas)]
  [InlineData(KnownBracesTypes.CommonQuotas)]
  [InlineData(KnownBracesTypes.CommonSymbols)]
  [InlineData(KnownBracesTypes.CommonBraces)]
  [InlineData(KnownBracesTypes.AllBraces)]
  public void Check_is_tilda_from_no_tilda_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsTilda());

  [Theory]
  [InlineData(KnownBracesTypes.Other)]
  [InlineData((KnownBracesTypes)1024)]
  public void Check_is_empty_from_empty_set_Return_true(KnownBracesTypes bracesTypes) => Assert.True(bracesTypes.IsEmpty());

  [Theory]
  [InlineData(KnownBracesTypes.All)]
  public void Check_is_empty_from_all_symbols_set_Returns_false(KnownBracesTypes bracesTypes) => Assert.False(bracesTypes.IsEmpty()); 
}