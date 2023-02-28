namespace StringFunctions.Tests;

public class StringNornalizationFunctionTests
{
  private const string _expectedString = "String for normalization";

  [Fact]
  public void Remove_leader_closing_braces_and_punctuation_symbols_and_special_symbols_Returns_new_string()
  {
    const string _sourceString = " .,)]^}-_>!?:#;String for normalization ";

    string _actualString = _sourceString.NormalizeString();

    Assert.Equal(_expectedString, _actualString);
  }

  [Fact]
  public void Remove_punctuation_and_whitespace_after_opening_symbols_Returns_new_string()
  {
    const string _sourceString = "String for normalization( .,!:";

    string _actualString = _sourceString.NormalizeString();

    Assert.Equal(_actualString, _expectedString + '(');
  }

  [Fact]
  public void Remove_punctuation_and_whitespace_before_closing_symbols_Returns_new_string()
  {
    const string _sourceString = "String for normalization,.!:; )";

    string _actuslString = _sourceString.NormalizeString();

    Assert.Equal(_actuslString, _expectedString + ')');
  }

  [Fact]
  public void Remove_whitespace_before_punctuation_Returns_new_string()
  {
    const string _sourceString = "String for normalization . , : ; ! ?";

    string _actualString = _sourceString.NormalizeString();

    Assert.Equal(_actualString, _expectedString + '.' + ',' + ':' + ';' + '!' + '?');
  }

  [Fact]
  public void Remove_double_whitespaces_Returns_new_string()
  {
    const string _sourceString = "String   for   normalization";

    string _actualString = _sourceString.NormalizeString();

    Assert.Equal(_actualString, _expectedString);
  }

  [Fact]
  public void Doesn_need_normalize_String_no_changes()
  {
    Assert.Equal(_expectedString.NormalizeString(), _expectedString);
  }
}