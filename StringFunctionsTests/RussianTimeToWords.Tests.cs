using ResultType;

using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianTimeToWordsTests
{
  private static string Success(Result<string> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    return result.Value!;
  }

  [Theory]
  [InlineData(15, 30, "пятнадцать часов тридцать минут")]
  [InlineData(1, 1, "один час одна минута")]
  [InlineData(2, 2, "два часа две минуты")]
  [InlineData(21, 21, "двадцать один час двадцать одна минута")]
  [InlineData(0, 5, "ноль часов пять минут")]
  [InlineData(23, 59, "двадцать три часа пятьдесят девять минут")]
  public void Convert_HoursAndMinutes(int hours, int minutes, string expected) =>
    Assert.Equal(expected, Success(RussianTimeToWords.Convert(hours, minutes)));

  [Fact]
  public void Convert_ZeroMinutes_OmitByDefault() =>
    Assert.Equal("пятнадцать часов", Success(RussianTimeToWords.Convert(15, 0)));

  [Fact]
  public void Convert_ZeroMinutes_Sharp() =>
    Assert.Equal("пятнадцать часов ровно", Success(RussianTimeToWords.Convert(15, 0, RussianTimeZeroMinutes.Sharp)));

  [Fact]
  public void Convert_Midnight_Omit() =>
    Assert.Equal("ноль часов", Success(RussianTimeToWords.Convert(0, 0)));

  [Fact]
  public void Convert_TimeOnly_Overload() =>
    Assert.Equal("девять часов пять минут", Success(RussianTimeToWords.Convert(new TimeOnly(9, 5))));

  [Fact]
  public void Convert_Extension_OnTimeOnly() =>
    Assert.Equal("пятнадцать часов ровно", Success(new TimeOnly(15, 0).ToRussianWords(RussianTimeZeroMinutes.Sharp)));

  [Theory]
  [InlineData(24, 0)]
  [InlineData(-1, 0)]
  [InlineData(15, 60)]
  [InlineData(15, -1)]
  public void Convert_InvalidInput_ReturnsFailure(int hours, int minutes)
  {
    var result = RussianTimeToWords.Convert(hours, minutes);

    Assert.True(result.IsFailure);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
  }
}
