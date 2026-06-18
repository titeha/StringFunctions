using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianNumberToWordsTests
{
  [Theory]
  [InlineData(0, "ноль")]
  [InlineData(1, "один")]
  [InlineData(2, "два")]
  [InlineData(11, "одиннадцать")]
  [InlineData(19, "девятнадцать")]
  [InlineData(21, "двадцать один")]
  [InlineData(40, "сорок")]
  [InlineData(90, "девяносто")]
  [InlineData(100, "сто")]
  [InlineData(101, "сто один")]
  [InlineData(215, "двести пятнадцать")]
  [InlineData(999, "девятьсот девяносто девять")]
  public void Convert_DefaultMasculine_ReturnsExpected(long number, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number));

  [Theory]
  [InlineData(1, RussianGender.Masculine, "один")]
  [InlineData(1, RussianGender.Feminine, "одна")]
  [InlineData(1, RussianGender.Neuter, "одно")]
  [InlineData(2, RussianGender.Masculine, "два")]
  [InlineData(2, RussianGender.Feminine, "две")]
  [InlineData(2, RussianGender.Neuter, "два")]
  [InlineData(22, RussianGender.Feminine, "двадцать две")]
  public void Convert_RespectsGenderOfUnits(long number, RussianGender gender, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number, gender));

  [Theory]
  [InlineData(1000, "одна тысяча")]
  [InlineData(2000, "две тысячи")]
  [InlineData(5000, "пять тысяч")]
  [InlineData(21000, "двадцать одна тысяча")]
  [InlineData(1000000, "один миллион")]
  [InlineData(2000000, "два миллиона")]
  [InlineData(5000000, "пять миллионов")]
  public void Convert_AppliesScaleWordsWithGenderAndAgreement(long number, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number));

  [Fact]
  public void Convert_LargeMixedNumber_ReturnsExpected() =>
    Assert.Equal(
      "один миллион двести тридцать четыре тысячи пятьсот шестьдесят семь",
      RussianNumberToWords.Convert(1_234_567));

  [Theory]
  [InlineData(-5, "минус пять")]
  [InlineData(-21, "минус двадцать один")]
  public void Convert_NegativeNumbers_PrefixedWithMinus(long number, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number));

  [Fact]
  public void Convert_LongMaxValue_ReturnsExpectedWithoutThrowing() =>
    Assert.Equal(
      "девять квинтиллионов двести двадцать три квадриллиона триста семьдесят два триллиона " +
      "тридцать шесть миллиардов восемьсот пятьдесят четыре миллиона семьсот семьдесят пять тысяч восемьсот семь",
      RussianNumberToWords.Convert(long.MaxValue));

  [Fact]
  public void Convert_LongMinValue_ReturnsExpectedWithoutThrowing() =>
    Assert.Equal(
      "минус девять квинтиллионов двести двадцать три квадриллиона триста семьдесят два триллиона " +
      "тридцать шесть миллиардов восемьсот пятьдесят четыре миллиона семьсот семьдесят пять тысяч восемьсот восемь",
      RussianNumberToWords.Convert(long.MinValue));
}
