using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianNumberDeclensionTests
{
  [Theory]
  // Единицы по падежам (мужской род).
  [InlineData(1, RussianCase.Genitive, "одного")]
  [InlineData(1, RussianCase.Dative, "одному")]
  [InlineData(1, RussianCase.Instrumental, "одним")]
  [InlineData(1, RussianCase.Prepositional, "одном")]
  [InlineData(2, RussianCase.Genitive, "двух")]
  [InlineData(2, RussianCase.Instrumental, "двумя")]
  [InlineData(3, RussianCase.Genitive, "трёх")]
  [InlineData(4, RussianCase.Instrumental, "четырьмя")]
  [InlineData(5, RussianCase.Genitive, "пяти")]
  [InlineData(5, RussianCase.Instrumental, "пятью")]
  public void Convert_Units_DeclineByCase(long number, RussianCase grammaticalCase, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number, grammaticalCase));

  [Theory]
  [InlineData(1, RussianCase.Genitive, "одной")]
  [InlineData(1, RussianCase.Accusative, "одну")]
  [InlineData(2, RussianCase.Nominative, "две")]
  [InlineData(2, RussianCase.Accusative, "две")]
  public void Convert_FeminineUnits_DeclineByCase(long number, RussianCase grammaticalCase, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number, grammaticalCase, RussianGender.Feminine));

  [Theory]
  // Составное число во всех косвенных падежах: 523.
  [InlineData(RussianCase.Nominative, "пятьсот двадцать три")]
  [InlineData(RussianCase.Genitive, "пятисот двадцати трёх")]
  [InlineData(RussianCase.Dative, "пятистам двадцати трём")]
  [InlineData(RussianCase.Accusative, "пятьсот двадцать три")]
  [InlineData(RussianCase.Instrumental, "пятьюстами двадцатью тремя")]
  [InlineData(RussianCase.Prepositional, "пятистах двадцати трёх")]
  public void Convert_CompoundNumber_DeclinesAllParts(RussianCase grammaticalCase, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(523, grammaticalCase));

  [Theory]
  // Согласование разрядных слов в косвенных падежах.
  [InlineData(1000, RussianCase.Genitive, "одной тысячи")]
  [InlineData(2000, RussianCase.Instrumental, "двумя тысячами")]
  [InlineData(5000, RussianCase.Dative, "пяти тысячам")]
  [InlineData(1000000, RussianCase.Genitive, "одного миллиона")]
  [InlineData(2000000, RussianCase.Instrumental, "двумя миллионами")]
  [InlineData(5000000, RussianCase.Prepositional, "пяти миллионах")]
  public void Convert_ScaleWords_AgreeInObliqueCases(long number, RussianCase grammaticalCase, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number, grammaticalCase));

  [Fact]
  public void Convert_FeminineThousandsInAccusative_UsesOdnu() =>
    Assert.Equal("двадцать одну тысячу", RussianNumberToWords.Convert(21000, RussianCase.Accusative));

  [Theory]
  [InlineData(0, RussianCase.Genitive, "нуля")]
  [InlineData(0, RussianCase.Instrumental, "нулём")]
  public void Convert_Zero_DeclinesByCase(long number, RussianCase grammaticalCase, string expected) =>
    Assert.Equal(expected, RussianNumberToWords.Convert(number, grammaticalCase));

  [Fact]
  public void Convert_NegativeInGenitive_PrefixedWithMinus() =>
    Assert.Equal("минус пяти", RussianNumberToWords.Convert(-5, RussianCase.Genitive));

  [Fact]
  public void Convert_NominativeOverload_MatchesExplicitNominative() =>
    Assert.Equal(
      RussianNumberToWords.Convert(1_234_567, RussianCase.Nominative),
      RussianNumberToWords.Convert(1_234_567));
}
