using ResultType;

using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianDateToWordsTests
{
  private static string Success(Result<string> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    return result.Value!;
  }

  [Fact]
  public void Convert_NominativeDay_DefaultFormat() =>
    Assert.Equal(
      "девятнадцатое июня две тысячи двадцать шестого года",
      Success(RussianDateToWords.Convert(2026, 6, 19)));

  [Fact]
  public void Convert_GenitiveDay() =>
    Assert.Equal(
      "девятнадцатого июня две тысячи двадцать шестого года",
      Success(RussianDateToWords.Convert(2026, 6, 19, RussianCase.Genitive)));

  [Theory]
  [InlineData(2000, 1, 1, "первое января двухтысячного года")]
  [InlineData(1917, 11, 7, "седьмое ноября тысяча девятьсот семнадцатого года")]
  [InlineData(2026, 12, 31, "тридцать первое декабря две тысячи двадцать шестого года")]
  [InlineData(2024, 2, 29, "двадцать девятое февраля две тысячи двадцать четвёртого года")]
  public void Convert_VariousDates(int year, int month, int day, string expected) =>
    Assert.Equal(expected, Success(RussianDateToWords.Convert(year, month, day)));

  [Fact]
  public void Convert_DateOnly_Overload() =>
    Assert.Equal(
      "первое марта две тысячи двадцать пятого года",
      Success(RussianDateToWords.Convert(new DateOnly(2025, 3, 1))));

  [Fact]
  public void Convert_Extension_OnDateTime() =>
    Assert.Equal(
      "первое марта две тысячи двадцать пятого года",
      Success(new DateTime(2025, 3, 1).ToRussianWords()));

  [Theory]
  [InlineData(2026, 13, 1)]
  [InlineData(2026, 6, 32)]
  [InlineData(0, 6, 19)]
  public void Convert_InvalidInput_ReturnsFailure(int year, int month, int day)
  {
    var result = RussianDateToWords.Convert(year, month, day);

    Assert.True(result.IsFailure);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
  }
}
