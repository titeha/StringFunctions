namespace StringFunctions.Tests;

public class IntRangeParserLimitTests
{
  private static string ParseLimitedFailure(string source, int maxRangeValue, int maxResultCount)
  {
    var result = IntRangeParser.Parse(source, maxRangeValue, maxResultCount);

    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));

    return result.Error!;
  }

  [Fact]
  public void Parse_WithZeroResultLimit_AllowsEmptyInput()
  {
    var result = IntRangeParser.Parse(string.Empty, maxRangeValue: 100, maxResultCount: 0);

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Empty(result.Value!);
  }

  [Fact]
  public void Parse_WithZeroResultLimit_RejectsNonEmptyResult()
  {
    string error = ParseLimitedFailure("0", maxRangeValue: 100, maxResultCount: 0);

    Assert.Contains("maxResultCount", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_BaselinePath_ReturnsFailureAsSoonAsLimitIsExceeded()
  {
    string error = ParseLimitedFailure("1-10,20", maxRangeValue: 100, maxResultCount: 5);

    Assert.Contains("maxResultCount", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Parse_AdaptiveLargeRangePath_ReturnsFailureWhenSingleRangeExceedsLimit()
  {
    string error = ParseLimitedFailure("1-10,2000000000", maxRangeValue: int.MaxValue, maxResultCount: 5);

    Assert.Contains("maxResultCount", error, StringComparison.OrdinalIgnoreCase);
  }
}
