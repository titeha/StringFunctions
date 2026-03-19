namespace StringFunctions.Tests;

public class IntRangeParserTests
{
  private static List<int> ParseSuccess(string source, int maxRangeValue)
  {
    var result = IntRangeParser.Parse(source, maxRangeValue);

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.False(result.IsFailure);
    Assert.NotNull(result.Value);

    return result.Value!;
  }

  private static string ParseFailure(string? source, int maxRangeValue)
  {
    var result = IntRangeParser.Parse(source, maxRangeValue);

    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);

    string error = result.Error!;
    Assert.False(string.IsNullOrWhiteSpace(error));

    return error;
  }

  [Fact]
  public void Parse_NullSource_ReturnsFailure()
  {
    string error = ParseFailure(null, 10);
    Assert.Contains("null", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_NegativeMaxRangeValue_ReturnsFailure()
  {
    string error = ParseFailure("1", -1);
    Assert.Contains("не меньше 0", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_EmptyString_ReturnsEmptyList()
  {
    List<int> actual = ParseSuccess(string.Empty, 10);
    Assert.Empty(actual);
  }

  [Fact]
  public void Parse_OnlyDelimiters_ReturnsEmptyList()
  {
    List<int> actual = ParseSuccess(" ,.;_:#!|\\/'\"  ", 10);
    Assert.Empty(actual);
  }

  [Fact]
  public void Parse_SinglePositiveValue_ReturnsSingleValue()
  {
    List<int> actual = ParseSuccess("5", 10);
    Assert.Equal([5], actual);
  }

  [Fact]
  public void Parse_SingleZero_ReturnsSingleZero()
  {
    List<int> actual = ParseSuccess("0", 10);
    Assert.Equal([0], actual);
  }

  [Fact]
  public void Parse_RegularRange_ReturnsExpectedValues()
  {
    List<int> actual = ParseSuccess("3-7", 10);
    Assert.Equal([3, 4, 5, 6, 7], actual);
  }

  [Fact]
  public void Parse_ReversedRange_NormalizesBounds()
  {
    List<int> actual = ParseSuccess("7-3", 10);
    Assert.Equal([3, 4, 5, 6, 7], actual);
  }

  [Fact]
  public void Parse_OpenLeftRange_StartsFromOne()
  {
    List<int> actual = ParseSuccess("-5", 10);
    Assert.Equal([1, 2, 3, 4, 5], actual);
  }

  [Fact]
  public void Parse_OpenLeftRange_WithZeroRightBound_ReturnsFailure()
  {
    string error = ParseFailure("-0", 10);
    Assert.Contains("1..10", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_ExplicitZeroRange_IsAllowed()
  {
    List<int> actual = ParseSuccess("0-5", 10);
    Assert.Equal([0, 1, 2, 3, 4, 5], actual);
  }

  [Fact]
  public void Parse_OpenRightRange_FromZero_IsAllowed()
  {
    List<int> actual = ParseSuccess("0-", 5);
    Assert.Equal([0, 1, 2, 3, 4, 5], actual);
  }

  [Fact]
  public void Parse_OpenRightRange_UsesMaxRangeValue()
  {
    List<int> actual = ParseSuccess("10-", 15);
    Assert.Equal([10, 11, 12, 13, 14, 15], actual);
  }

  [Fact]
  public void Parse_OpenLeftMustBeFirst_ReturnsFailure()
  {
    string error = ParseFailure("10,-5", 20);
    Assert.Contains("только первым токеном", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_OpenRightMustBeLast_ReturnsFailure()
  {
    string error = ParseFailure("10-,15", 20);
    Assert.Contains("только последним токеном", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_SpacesAroundDash_AreAllowed()
  {
    List<int> actual = ParseSuccess("0 - 3, 10 - 12, 20 -", 25);
    Assert.Equal([0, 1, 2, 3, 10, 11, 12, 20, 21, 22, 23, 24, 25], actual);
  }

  [Theory]
  [InlineData("10 - 12")]
  [InlineData("10- 12")]
  [InlineData("10 -12")]
  [InlineData("- 5")]
  [InlineData("10 -")]
  public void Parse_DashWithWhitespaceVariants_AreAllowed(string source)
  {
    List<int> actual = ParseSuccess(source, 12);

    if (source == "- 5")
      Assert.Equal([1, 2, 3, 4, 5], actual);
    else if (source == "10 -")
      Assert.Equal([10, 11, 12], actual);
    else
      Assert.Equal([10, 11, 12], actual);
  }

  [Fact]
  public void Parse_DuplicatesAndOverlaps_ReturnsSortedUniqueValues()
  {
    List<int> actual = ParseSuccess("5,3,5,1,2-4,3-6", 10);
    Assert.Equal([1, 2, 3, 4, 5, 6], actual);
  }

  [Fact]
  public void Parse_AllSupportedSeparators_AreHandled()
  {
    string source = "1,2;3 4|5/6'7\"8";
    List<int> actual = ParseSuccess(source, 10);
    Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], actual);
  }

  [Fact]
  public void Parse_InvalidToken_ReturnsFailure()
  {
    string error = ParseFailure("1,abc,3", 10);
    Assert.Contains("abc", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_MultipleDashes_ReturnsFailure()
  {
    string error = ParseFailure("1--5", 10);
    Assert.Contains("больше одного", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_ExplicitNegativeValue_ReturnsFailure()
  {
    string error = ParseFailure("-1-5", 10);
    Assert.NotNull(error);
  }

  [Fact]
  public void Parse_ValueGreaterThanMaxRangeValue_ReturnsFailure()
  {
    string error = ParseFailure("11", 10);
    Assert.Contains("0..10", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_RangeGreaterThanMaxRangeValue_ReturnsFailure()
  {
    string error = ParseFailure("5-12", 10);
    Assert.Contains("0..10", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_SparseHugeRange_ReturnsOnlyRequestedValues()
  {
    List<int> actual = ParseSuccess("1,500000,1000000", 1_000_000);
    Assert.Equal([1, 500000, 1000000], actual);
  }

  [Fact]
  public void Parse_DenseLargeRange_ReturnsCorrectBoundsAndCount()
  {
    List<int> actual = ParseSuccess("1-1000", 1000);

    Assert.Equal(1000, actual.Count);
    Assert.Equal(1, actual[0]);
    Assert.Equal(1000, actual[^1]);
  }

  [Fact]
  public void Parse_MixedScenario_ReturnsExpectedValues()
  {
    List<int> actual = ParseSuccess("0-3, 8, 10 - 12, 20-", 22);
    Assert.Equal([0, 1, 2, 3, 8, 10, 11, 12, 20, 21, 22], actual);
  }
}
