using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianOrdinalToWordsTests
{
  [Theory]
  [InlineData(1, "первый")]
  [InlineData(2, "второй")]
  [InlineData(3, "третий")]
  [InlineData(4, "четвёртый")]
  [InlineData(7, "седьмой")]
  [InlineData(10, "десятый")]
  [InlineData(11, "одиннадцатый")]
  [InlineData(19, "девятнадцатый")]
  [InlineData(20, "двадцатый")]
  [InlineData(21, "двадцать первый")]
  [InlineData(40, "сороковой")]
  [InlineData(100, "сотый")]
  [InlineData(132, "сто тридцать второй")]
  [InlineData(200, "двухсотый")]
  [InlineData(2026, "две тысячи двадцать шестой")]
  [InlineData(1917, "тысяча девятьсот семнадцатый")]
  [InlineData(1900, "тысяча девятисотый")]
  public void Convert_Masculine_Nominative(long number, string expected) =>
    Assert.Equal(expected, RussianOrdinalToWords.Convert(number));

  [Theory]
  [InlineData(1000, "тысячный")]
  [InlineData(2000, "двухтысячный")]
  [InlineData(5000, "пятитысячный")]
  [InlineData(1000000, "миллионный")]
  public void Convert_RoundScale_UsesScaleOrdinal(long number, string expected) =>
    Assert.Equal(expected, RussianOrdinalToWords.Convert(number));

  [Theory]
  [InlineData(1, RussianGender.Feminine, "первая")]
  [InlineData(1, RussianGender.Neuter, "первое")]
  [InlineData(3, RussianGender.Feminine, "третья")]
  [InlineData(3, RussianGender.Neuter, "третье")]
  [InlineData(2, RussianGender.Feminine, "вторая")]
  [InlineData(21, RussianGender.Neuter, "двадцать первое")]
  public void Convert_RespectsGender(long number, RussianGender gender, string expected) =>
    Assert.Equal(expected, RussianOrdinalToWords.Convert(number, gender));

  [Theory]
  [InlineData(2026, RussianGender.Masculine, RussianCase.Genitive, "две тысячи двадцать шестого")]
  [InlineData(2023, RussianGender.Masculine, RussianCase.Genitive, "две тысячи двадцать третьего")]
  [InlineData(1, RussianGender.Neuter, RussianCase.Genitive, "первого")]
  [InlineData(5, RussianGender.Masculine, RussianCase.Dative, "пятому")]
  [InlineData(2, RussianGender.Masculine, RussianCase.Instrumental, "вторым")]
  public void Convert_DeclinesOnlyLastWord(long number, RussianGender gender, RussianCase grammaticalCase, string expected) =>
    Assert.Equal(expected, RussianOrdinalToWords.Convert(number, gender, grammaticalCase));

  [Fact]
  public void Convert_Zero_ReturnsNulevoy() =>
    Assert.Equal("нулевой", RussianOrdinalToWords.Convert(0));

  [Fact]
  public void Convert_Negative_PrefixedWithMinus() =>
    Assert.Equal("минус первый", RussianOrdinalToWords.Convert(-1));

  [Fact]
  public void ToRussianOrdinal_Extension_Works() =>
    Assert.Equal("двадцать первый", 21.ToRussianOrdinal());

  [Fact]
  public void Convert_InvalidEnums_FallsBackWithoutThrowing()
  {
    Exception? exception = Record.Exception(() => RussianOrdinalToWords.Convert(1, (RussianGender)999, (RussianCase)999));

    Assert.Null(exception);
    Assert.Equal("первый", RussianOrdinalToWords.Convert(1, (RussianGender)999, (RussianCase)999));
  }

  [Fact]
  public void Convert_LongMinValue_DoesNotThrow()
  {
    Exception? exception = Record.Exception(() => RussianOrdinalToWords.Convert(long.MinValue));

    Assert.Null(exception);
  }

}
