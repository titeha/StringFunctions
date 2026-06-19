using ResultType;

using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianMoneyToWordsTests
{
  private static string Success(Result<string> result)
  {
    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    return result.Value!;
  }

  [Theory]
  [InlineData(123, 45, "сто двадцать три рубля сорок пять копеек")]
  [InlineData(1, 1, "один рубль одна копейка")]
  [InlineData(2, 2, "два рубля две копейки")]
  [InlineData(21, 0, "двадцать один рубль ноль копеек")]
  [InlineData(100, 0, "сто рублей ноль копеек")]
  [InlineData(5, 25, "пять рублей двадцать пять копеек")]
  public void Convert_Rubles_Words(long major, int minor, string expected) =>
    Assert.Equal(expected, Success(RussianMoneyToWords.Convert(major, minor, RussianCurrency.Rubles)));

  [Theory]
  [InlineData(123, 45, "сто двадцать три рубля 45 коп.")]
  [InlineData(100, 5, "сто рублей 05 коп.")]
  [InlineData(100, 0, "сто рублей 00 коп.")]
  public void Convert_Rubles_Digits(long major, int minor, string expected) =>
    Assert.Equal(expected, Success(RussianMoneyToWords.Convert(major, minor, RussianCurrency.Rubles, RussianMinorFormat.Digits)));

  [Fact]
  public void Convert_NegativeMajor_PrefixedWithMinus() =>
    Assert.Equal(
      "минус пять рублей пятьдесят копеек",
      Success(RussianMoneyToWords.Convert(-5, 50, RussianCurrency.Rubles)));

  [Fact]
  public void Convert_Decimal_RoundsAndMatches() =>
    Assert.Equal(
      Success(RussianMoneyToWords.Convert(123, 45, RussianCurrency.Rubles)),
      Success(RussianMoneyToWords.Convert(123.45m, RussianCurrency.Rubles)));

  [Fact]
  public void Convert_Decimal_CarriesRoundingIntoMajor() =>
    Assert.Equal(
      "три рубля ноль копеек",
      Success(RussianMoneyToWords.Convert(2.999m, RussianCurrency.Rubles)));

  [Fact]
  public void Convert_Decimal_NegativeZeroMajor_KeepsMinus() =>
    Assert.Equal(
      "минус ноль рублей пятьдесят копеек",
      Success(RussianMoneyToWords.Convert(-0.50m, RussianCurrency.Rubles)));

  [Fact]
  public void Convert_Dollars_Words() =>
    Assert.Equal(
      "пять долларов ноль центов",
      Success(RussianMoneyToWords.Convert(5, 0, RussianCurrency.Dollars)));

  [Fact]
  public void Convert_Euros_AreIndeclinableInMajor() =>
    Assert.Equal(
      "двадцать один евро ноль центов",
      Success(RussianMoneyToWords.Convert(21, 0, RussianCurrency.Euros, RussianMinorFormat.Words)));

  [Fact]
  public void Convert_CustomCurrency_Works()
  {
    var tenge = new RussianCurrency(
      new RussianNoun("тенге", "тенге", "тенге", RussianGender.Masculine),
      new RussianNoun("тиын", "тиына", "тиынов", RussianGender.Masculine),
      "тиын");

    Assert.Equal("сто тенге пять тиынов", Success(RussianMoneyToWords.Convert(100, 5, tenge)));
  }

  [Fact]
  public void Convert_MinorOutOfRange_ReturnsFailure()
  {
    var result = RussianMoneyToWords.Convert(5, 150, RussianCurrency.Rubles);

    Assert.True(result.IsFailure);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
  }

  [Fact]
  public void Convert_NullCurrency_ReturnsFailure()
  {
    var result = RussianMoneyToWords.Convert(5, 0, null!);

    Assert.True(result.IsFailure);
  }

  [Fact]
  public void ToRussianMoney_Extension_DefaultsToRubles() =>
    Assert.Equal(
      "сто двадцать три рубля сорок пять копеек",
      Success(123.45m.ToRussianMoney()));
}
