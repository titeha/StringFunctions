using ResultType;

using StringFunctions.Braces;

namespace StringFunctions;

public static class StringFunctions
{
  private const char _zeroCodeSym = '\x0';
  private const string _noBracesTypesPresent = "Не указаны виды проверяемых символов.";
  private const string _nullSourceError = "Проверяемая строка не может быть null.";

  /// <summary>
  /// Проверяет баланс скобок и кавычек в строке.
  /// </summary>
  /// <param name="source">Проверяемая строка.</param>
  /// <param name="bracesTypes">Набор известных типов скобок/кавычек.</param>
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
      return Result.Failure<(bool IsBalanced, char UnbalancedSymbol)>(_nullSourceError);

    if (source.Length == 0)
      return Result.Success<(bool IsBalanced, char UnbalancedSymbol)>((true, _zeroCodeSym));

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
      return Result.Failure<BraceManager>(_noBracesTypesPresent);

    if (hasTypes && hasSymbols)
      return Result.Success(new BraceManager(bracesTypes, bracesSymbols));

    if (hasTypes)
      return Result.Success(new BraceManager(bracesTypes));

    return Result.Success(new BraceManager(bracesSymbols));
  }

  private static Result<(bool IsBalanced, char UnbalancedSymbol)> CheckBracesBalance(string source, BraceManager manager)
  {
    char[] bracesList = manager.BracesList;
    var stack = new Stack<char>();
    char returnSymbol = _zeroCodeSym;

    int lastIndex = source.IndexOfAny(bracesList);

    while (lastIndex >= 0)
    {
      char lookingValue = source[lastIndex];

      if (manager.IsPaired(lookingValue))
        if (manager.IsOpening(lookingValue))
          stack.Push(lookingValue);
        else if (stack.Count > 0 && manager.IsPair(lookingValue, stack.Peek()))
          stack.Pop();
        else
        {
          returnSymbol = stack.Count > 0 ? stack.Pop() : lookingValue;
          break;
        }
      else if (stack.Count > 0 && manager.IsPair(lookingValue, stack.Peek()))
        stack.Pop();
      else
        stack.Push(lookingValue);

      lastIndex = source.IndexOfAny(bracesList, lastIndex + 1);
    }

    bool isBalanced = stack.Count == 0 && returnSymbol == _zeroCodeSym;

    if (!isBalanced && returnSymbol == _zeroCodeSym)
      returnSymbol = stack.Pop();

    return Result.Success<(bool IsBalanced, char UnbalancedSymbol)>((isBalanced, returnSymbol));
  }
}
