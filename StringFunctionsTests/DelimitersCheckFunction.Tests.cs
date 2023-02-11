using StringFunctions;

namespace StringFunctions.Tests;

public class DelimiterCheckFunctionTests
{
  private readonly string _delimitersList = ";:,.() []{}<>#@";

  [Theory]
  [InlineData('.')]
  [InlineData('(')]
  [InlineData('#')]
  public void Check_symbols_for_delimiter_in_known_delimiters_list_returns_true(char symbol) => Assert.True(_delimitersList.IsDelimiter(symbol));

  [Theory]
  [InlineData('a')]
  [InlineData('b')]
  [InlineData('à')]
  public void Check_no_delimiter_symbol_in_known_delimiters_list_returns_false(char symbol) => Assert.False(_delimitersList.IsDelimiter(symbol));
}