using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianNumberExtensionsTests
{
  [Fact]
  public void ToRussianWords_Long_ReturnsWords() =>
    Assert.Equal("пять", 5L.ToRussianWords());

  [Fact]
  public void ToRussianWords_Int_ReturnsWords() =>
    Assert.Equal("пять", 5.ToRussianWords());

  [Fact]
  public void ToRussianWords_WithGender_RespectsGender() =>
    Assert.Equal("две", 2.ToRussianWords(RussianGender.Feminine));

  [Fact]
  public void ToRussianWords_WithCase_Declines() =>
    Assert.Equal("пятисот двадцати трёх", 523.ToRussianWords(RussianCase.Genitive));

  [Fact]
  public void ToRussianWords_WithCaseAndGender_Declines() =>
    Assert.Equal("одной", 1.ToRussianWords(RussianCase.Genitive, RussianGender.Feminine));

  [Fact]
  public void GetRussianPluralForm_Extension_ReturnsForm() =>
    Assert.Equal(RussianPluralForm.Many, 5.GetRussianPluralForm());

  [Fact]
  public void Pluralize_Extension_PicksForm()
  {
    var result = 5.Pluralize("яблоко", "яблока", "яблок");

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Equal("яблок", result.Value);
  }

  [Fact]
  public void Quantify_Extension_BuildsString()
  {
    var result = 2.Quantify("файл", "файла", "файлов");

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Equal("2 файла", result.Value);
  }
}
