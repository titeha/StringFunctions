using ResultType;

using StringFunctions.Braces;

namespace StringFunctions.Tests;

public class CheckBraceBalanceFunctionTests
{
  private const char _roundOpenBrace = '(';
  private const char _roundCloseBrace = ')';
  private const char _squareOpenBrace = '[';
  private const char _squareCloseBrace = ']';
  private const char _quota = '"';
  private const char _customSet = '|';

  private static (bool IsBalanced, char UnbalancedSymbol) ParseSuccess(Result<(bool IsBalanced, char UnbalancedSymbol)> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.False(result.IsFailure);
    return result.Value;
  }

  private static string ParseFailure(Result<(bool IsBalanced, char UnbalancedSymbol)> result)
  {
    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);
    string error = result.Error!;
    Assert.False(string.IsNullOrWhiteSpace(error));
    return error;
  }

  [Fact]
  public void Check_balance_with_conflicting_custom_pairs_Returns_failure_without_throwing()
  {
    const string checkString = "(a + b)";

    string error = ParseFailure(checkString.IsBracesBalanced(KnownBracesTypes.Other, ('(', ')'), ('(', ']')));

    Assert.Contains("более чем в одной паре", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Check_balance_round_braces_in_balanced_string_Returns_true()
  {
    const string checkString = " (s(d)d) ";

    var (isBalanced, _) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));

    Assert.True(isBalanced);
  }

  [Fact]
  public void Check_balance_square_braces_in_balanced_string_Returns_true()
  {
    const string checkString = "text [text] test";

    var (isBalanced, _) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.SquareBraces));

    Assert.True(isBalanced);
  }

  [Fact]
  public void Check_balance_quotas_in_balanced_string_Returns_true()
  {
    const string checkString = "Text in \"Quotas\" for check balance";

    var (isBalanced, _) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.Quotas));

    Assert.True(isBalanced);
  }

  [Fact]
  public void Check_balance_custom_set_in_balanced_string_Returns_true()
  {
    const string checkString = "Test | balance checking | on custom set";

    var (isBalanced, _) = ParseSuccess(checkString.IsBracesBalanced(bracesSymbols: [('|', '|')]));

    Assert.True(isBalanced);
  }

  [Fact]
  public void Check_balance_on_empty_string_Returns_true()
  {
    var (isBalanced, symbol) = ParseSuccess(string.Empty.IsBracesBalanced(KnownBracesTypes.Other));

    Assert.True(isBalanced);
    Assert.Equal('\0', symbol);
  }

  [Fact]
  public void Check_balance_on_null_string_Returns_failure()
  {
    const string? source = null;

    string error = ParseFailure(source.IsBracesBalanced(KnownBracesTypes.RoundedBraces));

    Assert.Contains("null", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Check_balance_without_types_and_custom_symbols_Returns_failure()
  {
    const string checkString = "text";

    string error = ParseFailure(checkString.IsBracesBalanced(KnownBracesTypes.Other));

    Assert.NotEmpty(error);
  }

  [Fact]
  public void Check_balance_mixed_braces_and_quotas_in_balanced_string_Returns_true()
  {
    const string checkString = "Check (balance [Mixed braces] \"and quotas\") string";

    var (isBalanced, _) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.CommonSymbols));

    Assert.True(isBalanced);
  }

  [Fact]
  public void Check_balance_round_braces_in_unbalanced_string_Returns_false()
  {
    const string checkString = "check (balance( braces) on string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));

    Assert.False(isBalanced);
    Assert.Equal(_roundOpenBrace, symbol);
  }

  [Fact]
  public void Check_balance_mixed_set_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance[braces ) unbalanced mixed braces string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.CommonBraces));

    Assert.False(isBalanced);
    Assert.Equal(_squareOpenBrace, symbol);
  }

  [Fact]
  public void Check_balance_start_closing_round_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check ) balance (unbalanced) string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));

    Assert.False(isBalanced);
    Assert.Equal(_roundCloseBrace, symbol);
  }

  [Fact]
  public void Check_balance_excess_closing_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance) unbalanced) string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));

    Assert.False(isBalanced);
    Assert.Equal(_roundCloseBrace, symbol);
  }

  [Fact]
  public void Check_blance_excess_opening_round_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance) (unbalanced string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));

    Assert.False(isBalanced);
    Assert.Equal(_roundOpenBrace, symbol);
  }

  [Fact]
  public void Check_balance_excess_closing_square_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance) unbalanced] string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.CommonBraces));

    Assert.False(isBalanced);
    Assert.Equal(_squareCloseBrace, symbol);
  }

  [Fact]
  public void Check_balance_excess_quota_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check \"balance\" unbalanced\" string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(KnownBracesTypes.Quotas));

    Assert.False(isBalanced);
    Assert.Equal(_quota, symbol);
  }

  [Fact]
  public void Check_balance_excess_custom_set_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check | balance | unbalanced | string";

    var (isBalanced, symbol) = ParseSuccess(checkString.IsBracesBalanced(bracesSymbols: [('|', '|')]));

    Assert.False(isBalanced);
    Assert.Equal(_customSet, symbol);
  }
}
