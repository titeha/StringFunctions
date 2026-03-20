using ResultType;

using StringFunctions.Braces;

namespace StringFunctions;

/// <summary>
/// Предоставляет методы для проверки баланса скобок и кавычек в строке.
/// </summary>
public static class StringFunctions
{
  private const char ZeroCodeSymbol = '\x0';
  private const string NoBracesTypesPresentError = "Не указаны виды проверяемых символов.";
  private const string NullSourceError = "Проверяемая строка не может быть null.";

  /// <summary>
  /// Проверяет баланс скобок и кавычек в строке.
  /// </summary>
  /// <param name="source">Проверяемая строка.</param>
  /// <param name="bracesTypes">Набор известных типов скобок и кавычек.</param>
  /// <param name="bracesSymbols">Дополнительные пользовательские пары символов.</param>
  /// <returns>
  /// Успешный результат с кортежем:
  /// <list type="bullet">
  /// <item><description><c>IsBalanced</c> — признак сбалансированности.</description></item>
  /// <item><description><c>UnbalancedSymbol</c> — проблемный символ или <c>'\0'</c>, если баланс корректен.</description></item>
  /// </list>
  /// </returns>
  /// <remarks>
  /// <para>Для <see langword="null"/> возвращается <c>Failure</c>.</para>
  /// <para>Пустая строка считается сбалансированной и возвращает <c>Success((true, '\0'))</c>.</para>
  /// <para>Если не указаны ни <paramref name="bracesTypes"/>, ни <paramref name="bracesSymbols"/>, возвращается <c>Failure</c>.</para>
  /// </remarks>
  public static Result<(bool IsBalanced, char UnbalancedSymbol)> IsBracesBalanced(
    this string? source,
    KnownBracesTypes bracesTypes = KnownBracesTypes.Other,
    params (char, char)[] bracesSymbols)
  {
    if (source is null)
      return Result.Failure<(bool IsBalanced, char UnbalancedSymbol)>(NullSourceError);

    if (source.Length == 0)
      return Result.Success((true, ZeroCodeSymbol));

    Result<BraceManager> managerResult = CreateBraceManager(bracesTypes, bracesSymbols);
    if (managerResult.IsFailure)
      return Result.Failure<(bool IsBalanced, char UnbalancedSymbol)>(managerResult.Error!);

    return CheckBracesBalance(source, managerResult.Value!);
  }

  private static Result<BraceManager> CreateBraceManager(
    KnownBracesTypes bracesTypes,
    params (char, char)[] bracesSymbols)
  {
    bool hasTypes = !bracesTypes.IsEmpty();
    bool hasSymbols = bracesSymbols.Length > 0;

    if (!hasTypes && !hasSymbols)
      return Result.Failure<BraceManager>(NoBracesTypesPresentError);

    try
    {
      if (hasTypes && hasSymbols)
        return Result.Success(new BraceManager(bracesTypes, bracesSymbols));

      if (hasTypes)
        return Result.Success(new BraceManager(bracesTypes));

      return Result.Success(new BraceManager(bracesSymbols));
    }
    catch (ArgumentException ex)
    {
      string message = string.IsNullOrWhiteSpace(ex.Message)
        ? NoBracesTypesPresentError
        : ex.Message;

      return Result.Failure<BraceManager>(message);
    }
  }

  private static Result<(bool IsBalanced, char UnbalancedSymbol)> CheckBracesBalance(string source, BraceManager manager)
  {
    char[] bracesList = manager.BracesList;
    var stack = new Stack<char>();
    char returnSymbol = ZeroCodeSymbol;

    int lastIndex = source.IndexOfAny(bracesList);

    while (lastIndex >= 0)
    {
      char current = source[lastIndex];

      if (manager.IsPaired(current))
        if (manager.IsOpening(current))
          stack.Push(current);
        else if (stack.Count > 0 && manager.IsPair(current, stack.Peek()))
          stack.Pop();
        else
        {
          returnSymbol = stack.Count > 0 ? stack.Pop() : current;
          break;
        }
      else if (stack.Count > 0 && manager.IsPair(current, stack.Peek()))
        stack.Pop();
      else
        stack.Push(current);

      lastIndex = source.IndexOfAny(bracesList, lastIndex + 1);
    }

    bool isBalanced = stack.Count == 0 && returnSymbol == ZeroCodeSymbol;

    if (!isBalanced && returnSymbol == ZeroCodeSymbol)
      returnSymbol = stack.Pop();

    return Result.Success((isBalanced, returnSymbol));
  }
}
