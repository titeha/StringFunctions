using ResultType;

using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianColloquialTimeToWordsTests
{
  private static string Success(Result<string> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    return result.Value!;
  }

  [Theory]
  // Целые часы.
  [InlineData(6, 0, "шесть часов")]
  [InlineData(13, 0, "час")]
  [InlineData(0, 0, "двенадцать часов")]
  // До получаса — счёт в следующий час.
  [InlineData(6, 1, "одна минута седьмого")]
  [InlineData(6, 2, "две минуты седьмого")]
  [InlineData(6, 5, "пять минут седьмого")]
  [InlineData(6, 15, "четверть седьмого")]
  [InlineData(6, 20, "двадцать минут седьмого")]
  [InlineData(6, 30, "половина седьмого")]
  [InlineData(0, 15, "четверть первого")]
  [InlineData(12, 30, "половина первого")]
  [InlineData(23, 30, "половина двенадцатого")]
  // После получаса — на убыль.
  [InlineData(6, 35, "без двадцати пяти семь")]
  [InlineData(6, 40, "без двадцати семь")]
  [InlineData(6, 45, "без четверти семь")]
  [InlineData(6, 55, "без пяти семь")]
  [InlineData(0, 45, "без четверти час")]
  [InlineData(1, 45, "без четверти два")]
  [InlineData(23, 45, "без четверти двенадцать")]
  public void Convert_Colloquial(int hours, int minutes, string expected) =>
    Assert.Equal(expected, Success(RussianColloquialTimeToWords.Convert(hours, minutes)));

  [Theory]
  [InlineData(6, 0, "шесть часов утра")]
  [InlineData(15, 0, "три часа дня")]
  [InlineData(13, 0, "час дня")]
  [InlineData(19, 0, "семь часов вечера")]
  [InlineData(0, 0, "двенадцать часов ночи")]
  [InlineData(21, 15, "четверть десятого вечера")]
  [InlineData(6, 45, "без четверти семь утра")]
  public void Convert_WithPartOfDay(int hours, int minutes, string expected) =>
    Assert.Equal(expected, Success(RussianColloquialTimeToWords.Convert(hours, minutes, includePartOfDay: true)));

  [Fact]
  public void Convert_TimeOnly_Overload() =>
    Assert.Equal("половина седьмого", Success(RussianColloquialTimeToWords.Convert(new TimeOnly(6, 30))));

  [Fact]
  public void Convert_Extension_OnTimeOnly() =>
    Assert.Equal("без четверти семь", Success(new TimeOnly(6, 45).ToRussianColloquialWords()));

  [Theory]
  [InlineData(24, 0)]
  [InlineData(-1, 0)]
  [InlineData(6, 60)]
  public void Convert_InvalidInput_ReturnsFailure(int hours, int minutes)
  {
    var result = RussianColloquialTimeToWords.Convert(hours, minutes);

    Assert.True(result.IsFailure);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
  }
}
