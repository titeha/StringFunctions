namespace StringFunctions.Braces.Tests;

public class BracesManagerTests
{
  private static readonly char[] ExpectedCommonBracesList = ['(', ')', '[', ']', '{', '}'];

  public static TheoryData<char> ClosingBraces => [')', ']', '}'];

  public static TheoryData<char, char> MatchingOpeningAndClosingBracePairs =>
    new()
    {
      { ')', '(' },
      { ']', '[' },
      { '}', '{' },
    };

  public static TheoryData<char, char> MismatchedOpeningAndClosingBracePairs =>
    new()
    {
      { ')', '[' },
      { ')', '{' },
      { ']', '{' },
    };

  [Fact]
  public void Create_brace_manager_without_parameters_Exception_throws() =>
    Assert.Throws<ArgumentException>(() => new BraceManager());

  [Theory]
  [InlineData(KnownBracesTypes.Other)]
  [InlineData((KnownBracesTypes)1024)]
  public void Create_brace_manager_with_incorrect_braces_types_Exception_throws(KnownBracesTypes bracesTypes) =>
    Assert.Throws<ArgumentException>(() => new BraceManager(bracesTypes));

  [Fact]
  public void Create_brace_manager_with_correct_parameters_Manager_is_not_null()
  {
    var actualBraceManager = new BraceManager(KnownBracesTypes.All);

    Assert.NotNull(actualBraceManager);
  }

  [Fact]
  public void Get_braces_list_from_correct_brace_manager_Returns_expected_braces_list()
  {
    var testBraceManager = new BraceManager(KnownBracesTypes.CommonBraces);
    char[] resultBracesList = testBraceManager.BracesList;

    Assert.NotEmpty(resultBracesList);
    Assert.Equal(ExpectedCommonBracesList, resultBracesList);
  }

  [Fact]
  public void Create_braces_with_duplicates_braces_Result_braces_list_has_no_duplicates()
  {
    var duplicateBraceManager = new BraceManager(KnownBracesTypes.CommonBraces, ('(', ')'));
    char[] resultBraceList = duplicateBraceManager.BracesList;

    Assert.Equal(ExpectedCommonBracesList, resultBraceList);
  }

  [Theory]
  [InlineData('(')]
  [InlineData('[')]
  [InlineData('{')]
  public void Check_is_opening_brace_on_common_braces_set_with_opening_brace_Returns_true(char brace)
  {
    var testBraceManager = new BraceManager(KnownBracesTypes.CommonBraces);

    Assert.True(testBraceManager.IsOpening(brace));
  }

  [Theory]
  [MemberData(nameof(ClosingBraces), DisableDiscoveryEnumeration = true)]
  public void Check_is_opening_brace_on_common_braces_set_with_closing_brace_Returns_false(char brace)
  {
    var testBraceManager = new BraceManager(KnownBracesTypes.CommonBraces);

    Assert.False(testBraceManager.IsOpening(brace));
  }

  [Theory]
  [MemberData(nameof(MatchingOpeningAndClosingBracePairs), DisableDiscoveryEnumeration = true)]
  public void Check_is_pair_on_common_braces_set_with_opening_and_closing_brace_Returns_true(char first, char second)
  {
    var testBraceManager = new BraceManager(KnownBracesTypes.CommonBraces);

    Assert.True(testBraceManager.IsPair(first, second));
  }

  [Theory]
  [MemberData(nameof(MismatchedOpeningAndClosingBracePairs), DisableDiscoveryEnumeration = true)]
  public void Check_is_pair_on_common_braces_set_with_mismatched_opening_and_closing_brace_Returns_false(char first, char second)
  {
    var testBraceManager = new BraceManager(KnownBracesTypes.CommonBraces);

    Assert.False(testBraceManager.IsPair(first, second));
  }

  [Theory]
  [InlineData('(')]
  [InlineData(']')]
  [InlineData('{')]
  public void Check_is_paired_on_common_braces_set_with_common_braces_Returns_true(char brace)
  {
    var testBraceManager = new BraceManager(KnownBracesTypes.CommonBraces);

    Assert.True(testBraceManager.IsPaired(brace));
  }

  [Theory]
  [InlineData('"')]
  [InlineData('\'')]
  [InlineData('|')]
  public void Check_is_paired_on_common_quotas_set_with_additional_no_paired_set_Returns_false(char quotas)
  {
    var quotasManager = new BraceManager(KnownBracesTypes.CommonQuotas, ('|', '|'));

    Assert.False(quotasManager.IsPaired(quotas));
  }
}
