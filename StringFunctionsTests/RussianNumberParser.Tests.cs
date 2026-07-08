using ResultType;

using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianNumberParserTests
{
  private static long Success(Result<long> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    return result.Value;
  }

  [Theory]
  [InlineData("ноль", 0)]
  [InlineData("один", 1)]
  [InlineData("сто двадцать три", 123)]
  [InlineData("минус пять", -5)]
  [InlineData("одна тысяча", 1000)]
  [InlineData("две тысячи", 2000)]
  [InlineData("тысяча", 1000)]
  [InlineData("один миллион двести тридцать четыре тысячи пятьсот шестьдесят семь", 1234567)]
  public void Parse_Nominative(string text, long expected) =>
    Assert.Equal(expected, Success(RussianNumberParser.Parse(text)));

  [Theory]
  [InlineData("двадцати трёх", 23)]
  [InlineData("пятьюстами двадцатью тремя", 523)]
  [InlineData("двумя тысячами", 2000)]
  [InlineData("пяти тысячам", 5000)]
  public void Parse_ObliqueCases(string text, long expected) =>
    Assert.Equal(expected, Success(RussianNumberParser.Parse(text)));

  [Theory]
  [InlineData("СТО ДВАДЦАТЬ ТРИ", 123)]
  [InlineData("четырёхсот", 400)]
  [InlineData("четырехсот", 400)]
  [InlineData("  сто   двадцать  три  ", 123)]
  public void Parse_ToleratesCaseYoAndSpaces(string text, long expected) =>
    Assert.Equal(expected, Success(RussianNumberParser.Parse(text)));

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData("привет")]
  [InlineData("сто сто")]
  [InlineData("пятнадцать пять")]
  [InlineData("тысяча миллион")]
  [InlineData("пять сто")]
  [InlineData("минус")]
  [InlineData("ноль один")]
  public void Parse_InvalidInput_ReturnsFailure(string? text)
  {
    var result = RussianNumberParser.Parse(text);

    Assert.True(result.IsFailure);
    Assert.False(string.IsNullOrWhiteSpace(result.Error!));
  }

  [Fact]
  public void Parse_Extension_Works() =>
    Assert.Equal(42, Success("сорок два".ParseRussianNumber()));

  public static TheoryData<long> RoundTripNumbers =>
    [0, 1, 2, 3, 4, 5, 11, 15, 19, 20, 21, 25, 40, 99, 100, 101, 115, 123, 200, 999,
     1000, 2000, 5000, 21000, 1_000_000, 1_234_567, 1_000_000_000, long.MaxValue];

  [Theory]
  [MemberData(nameof(RoundTripNumbers))]
  public void RoundTrip_AllCases_RecoversOriginal(long number)
  {
    foreach (RussianCase grammaticalCase in Enum.GetValues<RussianCase>())
    {
      string words = RussianNumberToWords.Convert(number, grammaticalCase);
      Assert.Equal(number, Success(RussianNumberParser.Parse(words)));
    }
  }

  [Theory]
  [MemberData(nameof(RoundTripNumbers))]
  public void RoundTrip_Negative_RecoversOriginal(long number)
  {
    if (number == 0)
      return;

    string words = RussianNumberToWords.Convert(-number);
    Assert.Equal(-number, Success(RussianNumberParser.Parse(words)));
  }

  [Fact]
  public void RoundTrip_LongMinValue_RecoversOriginal()
  {
    string words = RussianNumberToWords.Convert(long.MinValue);

    Assert.Equal(long.MinValue, Success(RussianNumberParser.Parse(words)));
  }

}
