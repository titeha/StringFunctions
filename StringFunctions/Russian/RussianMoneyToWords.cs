using System.Globalization;
using System.Text;

using ResultType;

namespace StringFunctions.Russian;

/// <summary>
/// Записывает денежную сумму прописью на русском языке в именительном падеже.
/// </summary>
/// <remarks>
/// <para>
/// Основная часть суммы всегда записывается словами и согласуется с наименованием
/// валюты (род и счётная форма). Разменная часть записывается либо словами, либо цифрами —
/// см. <see cref="RussianMinorFormat"/>.
/// </para>
/// <para>
/// Валюта описывается через <see cref="RussianCurrency"/>; готовые варианты — рубли, доллары, евро.
/// Ошибки входных данных возвращаются через <see cref="Result{T}"/>.
/// </para>
/// </remarks>
public static class RussianMoneyToWords
{
  private const string _minus = "минус";

  /// <summary>
  /// Записывает сумму прописью по основной и разменной частям.
  /// </summary>
  /// <param name="major">Основная часть (рубли). Может быть отрицательной.</param>
  /// <param name="minor">Разменная часть (копейки), от 0 до <see cref="RussianCurrency.MinorUnitsPerMajor"/> − 1.</param>
  /// <param name="currency">Описание валюты.</param>
  /// <param name="minorFormat">Способ записи разменной части. По умолчанию — прописью.</param>
  /// <returns>Сумма прописью либо ошибка валидации.</returns>
  public static Result<string> Convert(
    long major,
    int minor,
    RussianCurrency currency,
    RussianMinorFormat minorFormat = RussianMinorFormat.Words)
  {
    if (currency is null)
      return Result.Failure<string>("Валюта не может быть null.");

    if (minor < 0 || minor >= currency.MinorUnitsPerMajor)
      return Result.Failure<string>(
        $"Разменная часть должна быть в диапазоне 0..{currency.MinorUnitsPerMajor - 1}. Значение: {minor}.");

    return Result.Success(Assemble(major, minor, currency, minorFormat, forceMinus: false));
  }

  /// <summary>
  /// Записывает сумму прописью по десятичному значению.
  /// </summary>
  /// <param name="amount">Сумма. Дробная часть округляется до разменных единиц.</param>
  /// <param name="currency">Описание валюты.</param>
  /// <param name="minorFormat">Способ записи разменной части. По умолчанию — прописью.</param>
  /// <returns>Сумма прописью либо ошибка валидации.</returns>
  public static Result<string> Convert(
    decimal amount,
    RussianCurrency currency,
    RussianMinorFormat minorFormat = RussianMinorFormat.Words)
  {
    if (currency is null)
      return Result.Failure<string>("Валюта не может быть null.");

    decimal scaled = Math.Round(amount * currency.MinorUnitsPerMajor, MidpointRounding.AwayFromZero);

    if (scaled < long.MinValue || scaled > long.MaxValue)
      return Result.Failure<string>("Сумма выходит за пределы поддерживаемого диапазона.");

    long total = (long)scaled;
    bool negative = total < 0;

    ulong magnitude = negative ? unchecked((ulong)(-total)) : (ulong)total;
    int perMajor = currency.MinorUnitsPerMajor;

    ulong majorMagnitude = magnitude / (ulong)perMajor;
    int minor = (int)(magnitude % (ulong)perMajor);

    if (majorMagnitude > long.MaxValue)
      return Result.Failure<string>("Сумма выходит за пределы поддерживаемого диапазона.");

    return Result.Success(Assemble((long)majorMagnitude, minor, currency, minorFormat, forceMinus: negative));
  }

  private static string Assemble(
    long majorForWords,
    int minor,
    RussianCurrency currency,
    RussianMinorFormat minorFormat,
    bool forceMinus)
  {
    var builder = new StringBuilder();

    if (forceMinus)
    {
      builder.Append(_minus);
      builder.Append(' ');
    }

    builder.Append(RussianNumberToWords.Convert(majorForWords, currency.Major.Gender));
    builder.Append(' ');
    builder.Append(currency.Major.Form(majorForWords));
    builder.Append(' ');

    AppendMinor(builder, minor, currency, minorFormat);

    return builder.ToString();
  }

  private static void AppendMinor(StringBuilder builder, int minor, RussianCurrency currency, RussianMinorFormat minorFormat)
  {
    if (minorFormat == RussianMinorFormat.Digits)
    {
      int width = (currency.MinorUnitsPerMajor - 1).ToString(CultureInfo.InvariantCulture).Length;
      builder.Append(minor.ToString(CultureInfo.InvariantCulture).PadLeft(width, '0'));
      builder.Append(' ');
      builder.Append(currency.MinorAbbreviation);
      return;
    }

    builder.Append(RussianNumberToWords.Convert(minor, currency.Minor.Gender));
    builder.Append(' ');
    builder.Append(currency.Minor.Form(minor));
  }
}
