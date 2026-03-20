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

  private static (bool IsBalanced, char UnbalancedSymbol) Success(Result<(bool IsBalanced, char UnbalancedSymbol)> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);

    return result.Value;
  }

  private static string Failure(Result<(bool IsBalanced, char UnbalancedSymbol)> result)
  {
    Assert.True(result.IsFailure);
    Assert.False(result.IsSuccess);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
    return result.Error!;
  }

  [Fact]
  public void Check_balance_round_braces_in_balanced_string_Returns_true()
  {
    const string checkString = " (s(d)d) ";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));
    Assert.True(IsBalanced);
    Assert.Equal('\0', UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_square_braces_in_balanced_string_Returns_true()
  {
    const string checkString = "text [text] test";

    var (IsBalanced, _) = Success(checkString.IsBracesBalanced(KnownBracesTypes.SquareBraces));
    Assert.True(IsBalanced);
  }

  [Fact]
  public void Check_balance_quotas_in_balanced_string_Returns_true()
  {
    const string checkString = "Text in \"Quotas\" for check balance";

    var (IsBalanced, _) = Success(checkString.IsBracesBalanced(KnownBracesTypes.Quotas));
    Assert.True(IsBalanced);
  }

  [Fact]
  public void Check_balance_custom_set_in_balanced_string_Returns_true()
  {
    const string checkString = "Test | balance checking | on custom set";

    var (IsBalanced, _) = Success(checkString.IsBracesBalanced(bracesSymbols: [('|', '|')]));
    Assert.True(IsBalanced);
  }

  [Fact]
  public void Check_balance_on_empty_string_Returns_true()
  {
    var (IsBalanced, UnbalancedSymbol) = Success(string.Empty.IsBracesBalanced(KnownBracesTypes.Other));
    Assert.True(IsBalanced);
    Assert.Equal('\0', UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_on_null_string_Returns_failure()
  {
    const string? source = null;
    string error = Failure(source.IsBracesBalanced(KnownBracesTypes.RoundedBraces));
    Assert.Contains("null", error, StringComparison.OrdinalIgnoreCase);
  }

  [Fact]
  public void Check_balance_without_any_known_or_custom_braces_Returns_failure()
  {
    string error = Failure("abc".IsBracesBalanced());
    Assert.Contains("Не указаны виды проверяемых символов", error);
  }

  [Fact]
  public void Check_balance_mixed_braces_and_quotas_in_balanced_string_Returns_true()
  {
    const string checkString = "Check (balance [Mixed braces] \"and quotas\") string";

    var (IsBalanced, _) = Success(checkString.IsBracesBalanced(KnownBracesTypes.CommonSymbols));
    Assert.True(IsBalanced);
  }

  [Fact]
  public void Check_balance_round_braces_in_unbalanced_string_Returns_false()
  {
    const string checkString = "check (balance( braces) on string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));
    Assert.False(IsBalanced);
    Assert.Equal(_roundOpenBrace, UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_mixed_set_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance[braces ) unbalanced mixed braces string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.CommonBraces));
    Assert.False(IsBalanced);
    Assert.Equal(_squareOpenBrace, UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_start_closing_round_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check ) balance (unbalanced) string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));
    Assert.False(IsBalanced);
    Assert.Equal(_roundCloseBrace, UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_excess_closing_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance) unbalanced) string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));
    Assert.False(IsBalanced);
    Assert.Equal(_roundCloseBrace, UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_excess_opening_round_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance) (unbalanced string";
    var result = Success(checkString.IsBracesBalanced(KnownBracesTypes.RoundedBraces));
    Assert.False(result.IsBalanced);
    Assert.Equal(_roundOpenBrace, result.UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_excess_closing_square_brace_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check (balance) unbalanced] string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.CommonBraces));
    Assert.False(IsBalanced);
    Assert.Equal(_squareCloseBrace, UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_excess_quota_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check \"balance\" unbalanced\" string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(KnownBracesTypes.Quotas));
    Assert.False(IsBalanced);
    Assert.Equal(_quota, UnbalancedSymbol);
  }

  [Fact]
  public void Check_balance_excess_custom_set_in_unbalanced_string_Returns_false()
  {
    const string checkString = "Check | balance | unbalanced | string";
    var (IsBalanced, UnbalancedSymbol) = Success(checkString.IsBracesBalanced(bracesSymbols: [('|', '|')]));
    Assert.False(IsBalanced);
    Assert.Equal(_customSet, UnbalancedSymbol);
  }
}
