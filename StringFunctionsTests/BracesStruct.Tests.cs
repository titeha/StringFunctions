namespace StringFunctions.Braces.Tests;

public class BracesStructTests
{
  private const char _openRoundBrace = '(';
  private const char _closeRoundBrace = ')';
  private const char _openSquareBrace = '[';

  private Brace _testBrace;

  [Fact]
  public void Chech_is_paired_for_rounded_braces_Returns_true()
  {
    _testBrace = new ('(', ')');

    Assert.True(_testBrace.IsPaired);
  }

  [Fact]
  public void Check_is_paired_for_quotas_Returns_false()
  {
    _testBrace = new ('"');

    Assert.False(_testBrace.IsPaired);
  }

  [Fact]
  public void Check_has_close_round_brace_in_ronded_braces_set_Returns_true()
  {
    _testBrace = new('(', ')');

    Assert.True(_testBrace.HasThisBrace(_closeRoundBrace));
  }

  [Fact]
  public void Check_has_open_square_brace_in_rounded_braces_set_Returns_false()
  {
    _testBrace = new('(', ')');

    Assert.False(_testBrace.HasThisBrace(_openSquareBrace));
  }

  [Fact]
  public void Check_is_opening_brace_in_rounded_braces_set_for_opening_round_brace_Returns_true()
  {
    _testBrace= new('(', ')');

    Assert.True(_testBrace.IsOpening(_openRoundBrace));
  }

  [Fact]
  public void Check_is_opening_brace_in_rounded_braces_set_for_closing_round_brace_Return_false()
  {
    _testBrace = new('(', ')');

    Assert.False(_testBrace.IsOpening(_closeRoundBrace));
  }

  [Fact]
  public void Check_is_pair_for_opening_rounded_brace_in_rounded_braces_set_Returns_false()
  {
    _testBrace = new('(', ')');

    Assert.False(_testBrace.IsPair(_openRoundBrace));
  }

  [Fact]
  public void Check_is_pair_for_closing_rounded_brace_in_rounded_braces_set_Returns_true()
  {
    _testBrace = new('(', ')');

    Assert.True(_testBrace.IsPair(_closeRoundBrace));
  }

  [Fact]
  public void Check_is_pair_for_opening_square_brace_in_rounded_braces_set_Returns_false()
  {
    _testBrace = new('(', ')');

    Assert.False(_testBrace.IsPair(_openSquareBrace));
  }

  [Fact]
  public void Check_is_pair_for_closing_rounded_brace_in_square_braces_set_Returns_false()
  {
    _testBrace = new Brace('[', ']');

    Assert.False(_testBrace.IsPair(_closeRoundBrace));
  }

  [Fact]
  public void Call_to_string_method_on_rounded_braces_set_Returns_rounded_braces_string()
  {
    _testBrace = new('(', ')');
    const string _expectedValue = "()";

    string _actualValue = _testBrace.ToString();

    Assert.Equal(_expectedValue, _actualValue);
  }

  [Fact]
  public void Check_equatable_method_for_rounded_and_squared_braces_set_on_rounded_braces_set_Returns_correct_values()
  {
    _testBrace = new('(', ')');
    Brace _otherBrace = new('(', ')');

    Assert.True(_testBrace.Equals(_otherBrace));
    Assert.False(_testBrace.Equals(new('[', ']')));
  }
}