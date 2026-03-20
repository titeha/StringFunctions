using ResultType;

namespace StringFunctions.Tests;

public class StringNormalizationFunctionTests
{
  private const string _expectedString = "String for normalization";

  private static string NormalizeSuccess(string? source)
  {
    Result<string> result = source.NormalizeString();

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.False(result.IsFailure);
    Assert.NotNull(result.Value);

    return result.Value!;
  }

  private static string NormalizeFailure(string? source)
  {
    Result<string> result = source.NormalizeString();

    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);

    string error = result.Error!;
    Assert.False(string.IsNullOrWhiteSpace(error));

    return error;
  }

  [Fact]
  public void Normalize_null_string_Returns_failure()
  {
    const string? source = null;

    string error = NormalizeFailure(source);

    Assert.Contains("null", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Normalize_empty_string_Returns_empty_string()
  {
    string actualString = NormalizeSuccess(string.Empty);

    Assert.Equal(string.Empty, actualString);
  }

  [Fact]
  public void Normalize_whitespace_string_Returns_empty_string()
  {
    string actualString = NormalizeSuccess("   \t  \r\n  ");

    Assert.Equal(string.Empty, actualString);
  }

  [Fact]
  public void Remove_leader_closing_braces_and_punctuation_symbols_and_special_symbols_Returns_new_string()
  {
    const string sourceString = " .,)]^}-_>!?:#;String for normalization ";
    string actualString = NormalizeSuccess(sourceString);

    Assert.Equal(_expectedString, actualString);
  }

  [Fact]
  public void Remove_punctuation_and_whitespace_after_opening_symbols_Returns_new_string()
  {
    const string sourceString = "String for normalization( .,!:";
    string actualString = NormalizeSuccess(sourceString);

    Assert.Equal(_expectedString + '(', actualString);
  }

  [Fact]
  public void Remove_punctuation_and_whitespace_before_closing_symbols_Returns_new_string()
  {
    const string sourceString = "String for normalization,.!:; )";
    string actualString = NormalizeSuccess(sourceString);

    Assert.Equal(_expectedString + ')', actualString);
  }

  [Fact]
  public void Remove_whitespace_before_punctuation_Returns_new_string()
  {
    const string sourceString = "String for normalization . , : ; ! ?";
    string actualString = NormalizeSuccess(sourceString);

    Assert.Equal(_expectedString + ".,:;!?", actualString);
  }

  [Fact]
  public void Remove_double_whitespaces_Returns_new_string()
  {
    const string sourceString = "String   for   normalization";
    string actualString = NormalizeSuccess(sourceString);

    Assert.Equal(_expectedString, actualString);
  }

  [Fact]
  public void Doesnt_need_normalize_String_no_changes()
  {
    string actualString = NormalizeSuccess(_expectedString);

    Assert.Equal(_expectedString, actualString);
  }
}
