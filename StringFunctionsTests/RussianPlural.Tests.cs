using StringFunctions.Russian;

namespace StringFunctions.Russian.Tests;

public class RussianPluralTests
{
  [Theory]
  [InlineData(1, RussianPluralForm.One)]
  [InlineData(21, RussianPluralForm.One)]
  [InlineData(101, RussianPluralForm.One)]
  [InlineData(2, RussianPluralForm.Few)]
  [InlineData(3, RussianPluralForm.Few)]
  [InlineData(4, RussianPluralForm.Few)]
  [InlineData(22, RussianPluralForm.Few)]
  [InlineData(0, RussianPluralForm.Many)]
  [InlineData(5, RussianPluralForm.Many)]
  [InlineData(11, RussianPluralForm.Many)]
  [InlineData(12, RussianPluralForm.Many)]
  [InlineData(13, RussianPluralForm.Many)]
  [InlineData(14, RussianPluralForm.Many)]
  [InlineData(111, RussianPluralForm.Many)]
  [InlineData(25, RussianPluralForm.Many)]
  public void GetForm_ReturnsExpectedForm(long count, RussianPluralForm expected) =>
    Assert.Equal(expected, RussianPlural.GetForm(count));

  [Theory]
  [InlineData(-1, RussianPluralForm.One)]
  [InlineData(-2, RussianPluralForm.Few)]
  [InlineData(-5, RussianPluralForm.Many)]
  [InlineData(-11, RussianPluralForm.Many)]
  public void GetForm_IgnoresSign(long count, RussianPluralForm expected) =>
    Assert.Equal(expected, RussianPlural.GetForm(count));

  [Fact]
  public void GetForm_LongMinValue_DoesNotThrow()
  {
    // long.MinValue = -9223372036854775808, последние две цифры "08" -> Many.
    // Главное — отсутствие переполнения при вычислении остатков.
    RussianPluralForm form = RussianPlural.GetForm(long.MinValue);
    Assert.Equal(RussianPluralForm.Many, form);
  }

  [Theory]
  [InlineData(1, "яблоко")]
  [InlineData(2, "яблока")]
  [InlineData(5, "яблок")]
  [InlineData(21, "яблоко")]
  [InlineData(0, "яблок")]
  public void Pluralize_PicksCorrectForm(long count, string expected)
  {
    var result = RussianPlural.Pluralize(count, "яблоко", "яблока", "яблок");

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Equal(expected, result.Value);
  }

  [Theory]
  [InlineData(1, "1 яблоко")]
  [InlineData(2, "2 яблока")]
  [InlineData(5, "5 яблок")]
  public void Quantify_BuildsExpectedString(long count, string expected)
  {
    var result = RussianPlural.Quantify(count, "яблоко", "яблока", "яблок");

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Equal(expected, result.Value);
  }

  [Fact]
  public void Quantify_CustomSeparator_IsApplied()
  {
    var result = RussianPlural.Quantify(3, "файл", "файла", "файлов", separator: " ");

    Assert.True(result.IsSuccess, result.IsFailure ? result.Error : string.Empty);
    Assert.Equal("3 файла", result.Value);
  }

  [Theory]
  [InlineData(null, "яблока", "яблок")]
  [InlineData("яблоко", null, "яблок")]
  [InlineData("яблоко", "яблока", null)]
  public void Pluralize_NullForm_ReturnsFailure(string? one, string? few, string? many)
  {
    var result = RussianPlural.Pluralize(5, one, few, many);

    Assert.True(result.IsFailure);
    Assert.False(string.IsNullOrWhiteSpace(result.Error));
  }
}
