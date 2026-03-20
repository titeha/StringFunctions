using ResultType;

namespace StringFunctions.Tests;

public class StringNormalizationFunctionTests
{
  private const string ExpectedString = "String for normalization";

  private static string Success(Result<string> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.NotNull(result.Value);
    return result.Value;
  }

  private static string Failure(Result<string> result)
  {
    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
    return result.Error!;
  }

  [Fact]
  public void Remove_leader_closing_braces_and_punctuation_symbols_and_special_symbols_Returns_new_string()
  {
    const string sourceString = " .,)]^}-_>!?:#;String for normalization ";
    string actualString = Success(sourceString.NormalizeString());
    Assert.Equal(ExpectedString, actualString);
  }

  [Fact]
  public void Remove_punctuation_and_whitespace_after_opening_symbols_Returns_new_string()
  {
    const string sourceString = "String for normalization( .,!:";
    string actualString = Success(sourceString.NormalizeString());
    Assert.Equal(ExpectedString + '(', actualString);
  }

  [Fact]
  public void Remove_punctuation_and_whitespace_before_closing_symbols_Returns_new_string()
  {
    const string sourceString = "String for normalization,.!:; )";
    string actualString = Success(sourceString.NormalizeString());
    Assert.Equal(ExpectedString + ')', actualString);
  }

  [Fact]
  public void Remove_whitespace_before_punctuation_Returns_new_string()
  {
    const string sourceString = "String for normalization . , : ; ! ?";
    string actualString = Success(sourceString.NormalizeString());
    Assert.Equal($"{ExpectedString}.,:;!?", actualString);
  }

  [Fact]
  public void Remove_double_whitespaces_Returns_new_string()
  {
    const string sourceString = "String  for   normalization";
    string actualString = Success(sourceString.NormalizeString());
    Assert.Equal(ExpectedString, actualString);
  }

  [Fact]
  public void Doesnt_need_normalize_String_no_changes()
  {
    string actualString = Success(ExpectedString.NormalizeString());
    Assert.Equal(ExpectedString, actualString);
  }

  [Fact]
  public void Normalize_empty_string_Returns_empty_string()
  {
    string actualString = Success(string.Empty.NormalizeString());
    Assert.Equal(string.Empty, actualString);
  }

  [Fact]
  public void Normalize_whitespace_string_Returns_empty_string()
  {
    string actualString = Success("   \t  ".NormalizeString());
    Assert.Equal(string.Empty, actualString);
  }

  [Fact]
  public void Normalize_null_string_Returns_failure()
  {
    const string? source = null;
    string error = Failure(source.NormalizeString());
    Assert.Contains("null", error, StringComparison.OrdinalIgnoreCase);
  }
}
