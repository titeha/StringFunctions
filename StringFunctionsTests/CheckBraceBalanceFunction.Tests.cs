using StringFunctions.Braces;

namespace StringFunctions.Tests;

public class CheckBraceBalanceFunction
{
  private const char _roundOpenBrace = '(';
  private const char _roundCloseBrace = ')';
  private const char _squareOpenBrace = '[';
  private const char _squareCloseBrace = ']';
  private const char _quota = '"';
  private const char _customSet = '|';

  [Fact]
  public void Check_balance_round_braces_in_balanced_string_Returns_true()
  {
    const string _checkString = "  (s(d)d)  ";

    (bool _result, _) = _checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces);

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_square_braces_in_balanced_string_Returns_true()
  {
    const string _checkString = "text [text] test";

    (bool _result, _) = _checkString.IsBracesBalanced(KnownBracesTypes.SquareBraces);

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_quotas_in_balanced_string_Returns_true()
  {
    const string _checkString = "Text in \"Quotas\" for check balance";

    (bool _result, _) = _checkString.IsBracesBalanced(KnownBracesTypes.Quotas);

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_custom_set_in_balanced_string_Returns_true()
  {
    const string _checkString = "Test | balance checking | on custom set";

    (bool _result, _) = _checkString.IsBracesBalanced(bracesSymbols: ('|', '|'));

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_on_empty_string_other_braces_set_Returns_true()
  {
    (bool _result, _) = string.Empty.IsBracesBalanced(KnownBracesTypes.Other);

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_round_braces_on_empty_string_Returns_true()
  {
    (bool _result, _) = string.Empty.IsBracesBalanced(KnownBracesTypes.RoundedBraces);

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_mixed_braces_and_quotas_in_balances_string_Returns_true()
  {
    const string _checkString = "Check (balance [Mixed braces] \"and quotas\") string";

    (bool _result, _) = _checkString.IsBracesBalanced(KnownBracesTypes.CommonSymbols);

    Assert.True(_result);
  }

  [Fact]
  public void Check_balance_roung_braces_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "check (balance( braces) on string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces);

    Assert.False(_result);
    Assert.Equal(_roundOpenBrace, _sym);
  }

  [Fact]
  public void Check_balance_mixed_set_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check (balance[braces ) unbalanced mixed braces string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.CommonBraces);

    Assert.False(_result);
    Assert.Equal(_squareOpenBrace, _sym);
  }

  [Fact]
  public void Check_balance_start_closing_round_brace_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check ) balance (unbalanced) string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces);

    Assert.False(_result);
    Assert.Equal(_roundCloseBrace, _sym);
  }

  [Fact]
  public void Check_balance_excess_closing_brace_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check (balance) unbalanced) string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces);

    Assert.False(_result);
    Assert.Equal(_roundCloseBrace, _sym);
  }

  [Fact]
  public void Check_blance_excess_opening_round_brace_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check (balance) (unbalanced string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces);

    Assert.False(_result);
    Assert.Equal(_roundOpenBrace, _sym);
  }

  [Fact]
  public void Check_balance_excess_closing_square_brace_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check (balance) unbalanced] string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.CommonBraces);

    Assert.False(_result);
    Assert.Equal(_squareCloseBrace, _sym);
  }

  [Fact]
  public void Check_balance_excess_quota_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check \"balance\" unbalanced\" string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(KnownBracesTypes.Quotas);

    Assert.False(_result);
    Assert.Equal(_quota, _sym);
  }

  [Fact]
  public void Check_balance_excess_custom_set_in_unbalanced_string_Returns_false()
  {
    const string _checkString = "Check | balance | unbalanced | string";

    (bool _result, char _sym) = _checkString.IsBracesBalanced(bracesSymbols: ('|', '|'));

    Assert.False(_result);
    Assert.Equal(_customSet, _sym);
  }
}