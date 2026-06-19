using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Методы расширения для работы с числами на русском языке: согласование с существительным
/// и запись прописью.
/// </summary>
/// <remarks>
/// Это эргономичная обёртка над <see cref="RussianPlural"/> и <see cref="RussianNumberToWords"/>.
/// Предусмотрены перегрузки для <see cref="int"/> и <see cref="long"/>.
/// </remarks>
public static class RussianNumberExtensions
{
  /// <summary>Определяет грамматическую форму существительного, согласуемого с числом.</summary>
  public static RussianPluralForm GetRussianPluralForm(this long count) => RussianPlural.GetForm(count);

  /// <inheritdoc cref="GetRussianPluralForm(long)"/>
  public static RussianPluralForm GetRussianPluralForm(this int count) => RussianPlural.GetForm(count);

  /// <summary>Выбирает форму существительного, согласованную с числом (например, «яблок»).</summary>
  public static Result<string> Pluralize(this long count, string? one, string? few, string? many) =>
    RussianPlural.Pluralize(count, one, few, many);

  /// <inheritdoc cref="Pluralize(long, string, string, string)"/>
  public static Result<string> Pluralize(this int count, string? one, string? few, string? many) =>
    RussianPlural.Pluralize(count, one, few, many);

  /// <summary>Формирует строку из числа и согласованной формы существительного (например, «5 яблок»).</summary>
  public static Result<string> Quantify(this long count, string? one, string? few, string? many, string separator = " ") =>
    RussianPlural.Quantify(count, one, few, many, separator);

  /// <inheritdoc cref="Quantify(long, string, string, string, string)"/>
  public static Result<string> Quantify(this int count, string? one, string? few, string? many, string separator = " ") =>
    RussianPlural.Quantify(count, one, few, many, separator);

  /// <summary>Преобразует число в запись словами на русском языке в именительном падеже.</summary>
  public static string ToRussianWords(this long number, RussianGender gender = RussianGender.Masculine) =>
    RussianNumberToWords.Convert(number, gender);

  /// <inheritdoc cref="ToRussianWords(long, RussianGender)"/>
  public static string ToRussianWords(this int number, RussianGender gender = RussianGender.Masculine) =>
    RussianNumberToWords.Convert(number, gender);

  /// <summary>Преобразует число в запись словами на русском языке в заданном падеже.</summary>
  public static string ToRussianWords(this long number, RussianCase grammaticalCase, RussianGender gender = RussianGender.Masculine) =>
    RussianNumberToWords.Convert(number, grammaticalCase, gender);

  /// <inheritdoc cref="ToRussianWords(long, RussianCase, RussianGender)"/>
  public static string ToRussianWords(this int number, RussianCase grammaticalCase, RussianGender gender = RussianGender.Masculine) =>
    RussianNumberToWords.Convert(number, grammaticalCase, gender);

  /// <summary>Записывает денежную сумму прописью (по умолчанию в рублях).</summary>
  public static Result<string> ToRussianMoney(
    this decimal amount,
    RussianCurrency? currency = null,
    RussianMinorFormat minorFormat = RussianMinorFormat.Words) =>
    RussianMoneyToWords.Convert(amount, currency ?? RussianCurrency.Rubles, minorFormat);

  /// <summary>Преобразует число в порядковое числительное словами на русском языке.</summary>
  public static string ToRussianOrdinal(
    this long number,
    RussianGender gender = RussianGender.Masculine,
    RussianCase grammaticalCase = RussianCase.Nominative) =>
    RussianOrdinalToWords.Convert(number, gender, grammaticalCase);

  /// <inheritdoc cref="ToRussianOrdinal(long, RussianGender, RussianCase)"/>
  public static string ToRussianOrdinal(
    this int number,
    RussianGender gender = RussianGender.Masculine,
    RussianCase grammaticalCase = RussianCase.Nominative) =>
    RussianOrdinalToWords.Convert(number, gender, grammaticalCase);

  /// <summary>Разбирает количественное числительное, записанное словами, в число.</summary>
  public static Result<long> ParseRussianNumber(this string? text) =>
    RussianNumberParser.Parse(text);

  /// <summary>Записывает дату прописью на русском языке.</summary>
  public static Result<string> ToRussianWords(this DateOnly date, RussianCase dayCase = RussianCase.Nominative) =>
    RussianDateToWords.Convert(date, dayCase);

  /// <inheritdoc cref="ToRussianWords(DateOnly, RussianCase)"/>
  public static Result<string> ToRussianWords(this DateTime date, RussianCase dayCase = RussianCase.Nominative) =>
    RussianDateToWords.Convert(date, dayCase);
}
