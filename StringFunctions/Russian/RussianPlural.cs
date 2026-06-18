using System.Globalization;

using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Предоставляет методы согласования существительных с числами в русском языке.
/// </summary>
/// <remarks>
/// <para>
/// В русском языке форма существительного при счёте зависит от последних цифр числа
/// (1 яблоко, 2 яблока, 5 яблок). Этот класс определяет нужную форму
/// (<see cref="GetForm(long)"/>) и подставляет соответствующее слово
/// (<see cref="Pluralize"/>, <see cref="Quantify"/>).
/// </para>
/// <para>
/// Ошибки пользовательского ввода возвращаются через <see cref="Result{T}"/>,
/// без генерации исключений.
/// </para>
/// </remarks>
public static class RussianPlural
{
  private const string _nullFormError = "Форма '{0}' не может быть null.";

  /// <summary>
  /// Определяет грамматическую форму существительного, согласуемого с числом.
  /// </summary>
  /// <param name="count">Число, с которым согласуется существительное. Знак числа не влияет на результат.</param>
  /// <returns>Грамматическая форма <see cref="RussianPluralForm"/>.</returns>
  /// <remarks>Метод не может завершиться ошибкой и возвращает значение напрямую.</remarks>
  public static RussianPluralForm GetForm(long count)
  {
    // Работаем с остатками, чтобы не переполниться на long.MinValue (Math.Abs бросил бы исключение).
    long lastTwoDigits = count % 100;
    if (lastTwoDigits < 0)
      lastTwoDigits = -lastTwoDigits;

    if (lastTwoDigits >= 11 && lastTwoDigits <= 14)
      return RussianPluralForm.Many;

    long lastDigit = lastTwoDigits % 10;

    return lastDigit switch
    {
      1 => RussianPluralForm.One,
      2 or 3 or 4 => RussianPluralForm.Few,
      _ => RussianPluralForm.Many
    };
  }

  /// <summary>
  /// Выбирает форму существительного, согласованную с числом.
  /// </summary>
  /// <param name="count">Число, с которым согласуется существительное.</param>
  /// <param name="one">Форма для чисел, оканчивающихся на <c>1</c> (кроме <c>11</c>). Пример: «яблоко».</param>
  /// <param name="few">Форма для чисел, оканчивающихся на <c>2</c>–<c>4</c> (кроме <c>12</c>–<c>14</c>). Пример: «яблока».</param>
  /// <param name="many">Форма для остальных чисел. Пример: «яблок».</param>
  /// <returns>
  /// Успешный результат с подходящей формой слова либо <c>Failure</c>, если одна из форм равна <c>null</c>.
  /// </returns>
  public static Result<string> Pluralize(long count, string? one, string? few, string? many)
  {
    if (one is null)
      return Result.Failure<string>(string.Format(_nullFormError, nameof(one)));

    if (few is null)
      return Result.Failure<string>(string.Format(_nullFormError, nameof(few)));

    if (many is null)
      return Result.Failure<string>(string.Format(_nullFormError, nameof(many)));

    string form = GetForm(count) switch
    {
      RussianPluralForm.One => one,
      RussianPluralForm.Few => few,
      _ => many
    };

    return Result.Success(form);
  }

  /// <summary>
  /// Формирует строку из числа и согласованной с ним формы существительного, например «5 яблок».
  /// </summary>
  /// <param name="count">Число, с которым согласуется существительное.</param>
  /// <param name="one">Форма для чисел, оканчивающихся на <c>1</c> (кроме <c>11</c>). Пример: «яблоко».</param>
  /// <param name="few">Форма для чисел, оканчивающихся на <c>2</c>–<c>4</c> (кроме <c>12</c>–<c>14</c>). Пример: «яблока».</param>
  /// <param name="many">Форма для остальных чисел. Пример: «яблок».</param>
  /// <param name="separator">Разделитель между числом и словом. По умолчанию пробел.</param>
  /// <returns>
  /// Успешный результат со строкой вида «<c>{count}{separator}{форма}</c>» либо <c>Failure</c>,
  /// если одна из форм равна <c>null</c>.
  /// </returns>
  public static Result<string> Quantify(long count, string? one, string? few, string? many, string separator = " ")
  {
    Result<string> formResult = Pluralize(count, one, few, many);

    if (formResult.IsFailure)
      return formResult;

    string text = string.Concat(count.ToString(CultureInfo.InvariantCulture), separator, formResult.Value);
    return Result.Success(text);
  }
}
