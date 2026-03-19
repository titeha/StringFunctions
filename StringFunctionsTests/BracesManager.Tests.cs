namespace StringFunctions.Braces.Tests;

public class BracesManagerTests
{
  private readonly char[] _expectedCommonBracesList = ['(', ')', '[', ']', '{', '}'];

  private BraceManager _testBraceManager = null!;

  public static TheoryData<char> ClosingBraces => [')', ']', '}'];

  public static TheoryData<char, char> MatchingOpeningAndClosingBracePairs => new()
  {
    { ')', '(' },
    { ']', '[' },
    { '}', '{' },
  };

  public static TheoryData<char, char> MismatchedOpeningAndClosingBracePairs => new()
  {
    { ')', '[' },
    { ')', '{' },
    { ']', '{' },
  };

  [Fact]
  public void Craete_brace_manager_without_parameters_Exception_throws() => Assert.Throws<ArgumentException>(() => new BraceManager());

  [Theory]
  [InlineData(KnownBracesTypes.Other)]
  [InlineData((KnownBracesTypes)1024)]
  public void Create_brace_manager_with_incorrect_braces_types_Exception_throws(KnownBracesTypes bracesTypes) => Assert.Throws<ArgumentException>(() => new BraceManager(bracesTypes));

  [Fact]
  public void Create_brace_manager_with_correct_parameters_Manager_is_not_null()
  {
    var _actualBraceManager = new BraceManager(KnownBracesTypes.All);

    Assert.NotNull(_actualBraceManager);
  }

  [Fact]
  public void Get_braces_list_from_correct_brace_manager_Returns_braces_list_not_empty()
  {
    _testBraceManager = new(KnownBracesTypes.CommonBraces);
    char[] _resultBracesList = _testBraceManager.BracesList;

    Assert.NotEmpty(_resultBracesList);
    Assert.Equal(_resultBracesList, _expectedCommonBracesList);
  }

  [Fact]
  public void Create_braces_with_dublicates_braces_Result_braces_list_has_no_dublicates()
  {
    BraceManager _dublicateBraceManages = new(KnownBracesTypes.CommonBraces, ('(', ')'));

    char[] _resultBraceList = _dublicateBraceManages.BracesList;

    Assert.Equal(_resultBraceList, _expectedCommonBracesList);
  }

  [Theory]
  [InlineData('(')]
  [InlineData('[')]
  [InlineData('{')]
  public void Check_is_opening_brace_on_common_braces_set_with_opening_brace_Returns_true(char brace)
  {
    _testBraceManager = new(KnownBracesTypes.CommonBraces);

    Assert.True(_testBraceManager.IsOpening(brace));
  }

  [Theory]
  [MemberData(nameof(ClosingBraces))]
  public void Check_is_opening_brace_on_common_braces_set_with_closing_brace_Returns_false(char brace)
  {
    _testBraceManager = new(KnownBracesTypes.CommonBraces);

    Assert.False(_testBraceManager.IsOpening(brace));
  }

  [Theory]
  [MemberData(nameof(MatchingOpeningAndClosingBracePairs))]
  public void Check_is_pair_on_common_braces_set_with_opening_and_closing_brace_Returns_true(char first, char second)
  {
    _testBraceManager = new(KnownBracesTypes.CommonBraces);

    Assert.True(_testBraceManager.IsPair(first, second));
  }

  [Theory]
  [MemberData(nameof(MismatchedOpeningAndClosingBracePairs))]
  public void Check_is_pair_on_common_braces_set_with_mismatched_opening_and_closing_brace_Returns_false(char first, char second)
  {
    _testBraceManager = new(KnownBracesTypes.CommonBraces);

    Assert.False(_testBraceManager.IsPair(first, second));
  }

  [Theory]
  [InlineData('(')]
  [InlineData(']')]
  [InlineData('{')]
  public void Check_is_paired_on_common_braces_set_with_common_braces_Returns_true(char brace)
  {
    _testBraceManager = new(KnownBracesTypes.CommonBraces);

    Assert.True(_testBraceManager.IsPaired(brace));
  }

  [Theory]
  [InlineData('"')]
  [InlineData('\'')]
  [InlineData('|')]
  public void Check_is_paired_on_common_quotas_set_with_additional_no_paired_set_Returns_false(char quotas)
  {
    BraceManager _quotasManager = new(KnownBracesTypes.CommonQuotas, ('|', '|'));

    Assert.False(_quotasManager.IsPaired(quotas));
  }
}