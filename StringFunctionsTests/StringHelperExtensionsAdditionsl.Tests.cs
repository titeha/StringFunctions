namespace StringFunctions.Tests;

public class StringHelperExtensionsAdditionalTests
{
  [Fact]
  public void NormalizeString_ConvertsWhitespaceCharactersToRegularSpaces()
  {
    var result = "alpha\t\t beta\r\n gamma".NormalizeString();

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Equal("alpha beta gamma", result.Value);
  }

  [Fact]
  public void IsDelimiter_NullDelimiterSet_ReturnsFalse()
  {
    string? delimiters = null;

    Assert.False(delimiters.IsDelimiter(','));
  }
}
