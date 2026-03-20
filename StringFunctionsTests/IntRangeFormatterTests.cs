using ResultType;

namespace StringFunctions.Tests;

public class IntRangeFormatterTests
{
  private static string FormatSuccess(IEnumerable<int>? values, string? separator = ",")
  {
    Result<string> result = IntRangeFormatter.Format(values, separator);

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.False(result.IsFailure);
    Assert.NotNull(result.Value);

    return result.Value!;
  }

  private static string FormatSuccess(IEnumerable<int>? values, int maxRangeValue, string? separator = ",", bool useOpenRanges = true)
  {
    Result<string> result = IntRangeFormatter.Format(values, maxRangeValue, separator, useOpenRanges);

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.False(result.IsFailure);
    Assert.NotNull(result.Value);

    return result.Value!;
  }

  private static string FormatFailure(IEnumerable<int>? values, string? separator = ",")
  {
    Result<string> result = IntRangeFormatter.Format(values, separator);

    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);

    string error = result.Error!;
    Assert.False(string.IsNullOrWhiteSpace(error));

    return error;
  }

  private static string FormatFailure(IEnumerable<int>? values, int maxRangeValue, string? separator = ",", bool useOpenRanges = true)
  {
    Result<string> result = IntRangeFormatter.Format(values, maxRangeValue, separator, useOpenRanges);

    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);

    string error = result.Error!;
    Assert.False(string.IsNullOrWhiteSpace(error));

    return error;
  }

  [Fact]
  public void Format_NullCollection_ReturnsFailure()
  {
    string error = FormatFailure(null);

    Assert.Contains("null", error);
  }

  [Fact]
  public void Format_EmptyCollection_ReturnsEmptyString()
  {
    string actual = FormatSuccess([]);

    Assert.Equal(string.Empty, actual);
  }

  [Fact]
  public void Format_SingleZero_ReturnsZero()
  {
    string actual = FormatSuccess([0]);

    Assert.Equal("0", actual);
  }

  [Fact]
  public void Format_SingleValue_ReturnsSingleValue()
  {
    string actual = FormatSuccess([5]);

    Assert.Equal("5", actual);
  }

  [Fact]
  public void Format_ContiguousValues_ReturnsSingleRange()
  {
    string actual = FormatSuccess([1, 2, 3]);

    Assert.Equal("1-3", actual);
  }

  [Fact]
  public void Format_UnsortedValuesWithDuplicates_NormalizesSequence()
  {
    string actual = FormatSuccess([7, 3, 2, 1, 3, 8, 9, 5]);

    Assert.Equal("1-3,5,7-9", actual);
  }

  [Fact]
  public void Format_CustomSeparator_IsUsed()
  {
    string actual = FormatSuccess([1, 2, 3, 5, 7, 8, 9], "; ");

    Assert.Equal("1-3; 5; 7-9", actual);
  }

  [Fact]
  public void Format_InvalidSeparator_ReturnsFailure()
  {
    string error = FormatFailure([1, 2, 3], " -> ");

    Assert.Contains("Разделитель", error);
  }

  [Fact]
  public void Format_NegativeValue_ReturnsFailure()
  {
    string error = FormatFailure([-1, 0, 1]);

    Assert.Contains("не меньше 0", error);
  }

  [Fact]
  public void Format_MaxRangeLessThanZero_ReturnsFailure()
  {
    string error = FormatFailure([0, 1, 2], -1);

    Assert.Contains("не меньше 0", error);
  }

  [Fact]
  public void Format_ValueGreaterThanMaxRange_ReturnsFailure()
  {
    string error = FormatFailure([0, 1, 2, 11], 10);

    Assert.Contains("0..10", error);
  }

  [Fact]
  public void Format_WithMaxRange_UsesOpenLeftRange()
  {
    string actual = FormatSuccess([1, 2, 3, 4, 5], 10);

    Assert.Equal("-5", actual);
  }

  [Fact]
  public void Format_WithMaxRange_UsesOpenRightRange()
  {
    string actual = FormatSuccess([10, 11, 12], 12);

    Assert.Equal("10-", actual);
  }

  [Fact]
  public void Format_WithMaxRange_UsesOpenRangeFromZero()
  {
    string actual = FormatSuccess([0, 1, 2, 3], 3);

    Assert.Equal("0-", actual);
  }

  [Fact]
  public void Format_WithMaxRange_DoesNotCollapseZeroToLeftOpenRange()
  {
    string actual = FormatSuccess([0, 1, 2, 3], 10);

    Assert.Equal("0-3", actual);
  }

  [Fact]
  public void Format_WithMaxRange_AndDisabledOpenRanges_UsesExplicitRange()
  {
    string actual = FormatSuccess([1, 2, 3, 4, 5], 10, useOpenRanges: false);

    Assert.Equal("1-5", actual);
  }

  [Fact]
  public void Format_SingleOne_DoesNotUseOpenLeftNotation()
  {
    string actual = FormatSuccess([1], 10);

    Assert.Equal("1", actual);
  }

  [Fact]
  public void Format_SingleMaxValue_DoesNotUseOpenRightNotation()
  {
    string actual = FormatSuccess([10], 10);

    Assert.Equal("10", actual);
  }

  [Fact]
  public void Format_SingleZeroWithMax_DoesNotUseOpenRightNotation()
  {
    string actual = FormatSuccess([0], 10);

    Assert.Equal("0", actual);
  }

  [Fact]
  public void Format_RoundTrip_WithoutOpenRanges_ReturnsNormalizedValues()
  {
    int[] input = [7, 3, 2, 1, 3, 8, 9, 5, 0];

    string text = FormatSuccess(input, separator: ", ");
    Result<List<int>> parsed = IntRangeParser.Parse(text, 100);

    Assert.True(parsed.IsSuccess, parsed.IsFailure ? parsed.Error : string.Empty);
    Assert.Equal([0, 1, 2, 3, 5, 7, 8, 9], parsed.Value!);
  }

  [Fact]
  public void Format_RoundTrip_WithOpenRanges_ReturnsNormalizedValues()
  {
    int[] input = [0, 1, 2, 3, 10, 11, 12];

    string text = FormatSuccess(input, 12, separator: ", ", useOpenRanges: true);
    Result<List<int>> parsed = IntRangeParser.Parse(text, 12);

    Assert.True(parsed.IsSuccess, parsed.IsFailure ? parsed.Error : string.Empty);
    Assert.Equal([0, 1, 2, 3, 10, 11, 12], parsed.Value!);
  }
}
