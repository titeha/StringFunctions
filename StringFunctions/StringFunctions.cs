using ResultType;

using StringFunctions.Braces;

namespace StringFunctions;

/// <summary>
/// Предоставляет методы для работы со строками.
/// </summary>
public static class StringFunctions
{
  private const char _zeroCodeSymbol = '\x0';
  private const string _noBracesTypesPresentError = "Не указаны виды проверяемых символов.";
  private const string _nullSourceError = "Проверяемая строка не может быть null.";

  /// <summary>
  /// Проверяет баланс скобок и кавычек в строке.
  /// </summary>
  /// <param name="source">Проверяемая строка.</param>
  /// <param name="bracesTypes">Набор известных типов скобок и кавычек.</param>
  /// <param name="bracesSymbols">Дополнительные пользовательские пары символов.</param>
  /// <returns>
  /// Успешный результат содержит кортеж:
  /// <list type="bullet">
  /// <item><description><c>IsBalanced</c> — признак сбалансированности.</description></item>
  /// <item><description><c>UnbalancedSymbol</c> — несбалансированный символ или <c>'\0'</c>, если баланс не нарушен.</description></item>
  /// </list>
  /// Если входные параметры некорректны, возвращается <c>Failure</c> с текстом ошибки.
  /// </returns>
  /// <remarks>
  /// Пустая строка считается сбалансированной.
  /// Значение <c>null</c> считается ошибкой входных данных.
  /// </remarks>
  public static Result<(bool IsBalanced, char UnbalancedSymbol)> IsBracesBalanced(
    this string? source,
    KnownBracesTypes bracesTypes = KnownBracesTypes.Other,
    params (char, char)[] bracesSymbols)
  {
    if (source is null)
      return Result.Failure<(bool IsBalanced, char UnbalancedSymbol)>(_nullSourceError);

    if (source.Length == 0)
      return Result.Success((true, _zeroCodeSymbol));

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
      return Result.Failure<BraceManager>(_noBracesTypesPresentError);

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
        ? _noBracesTypesPresentError
        : ex.Message;

      return Result.Failure<BraceManager>(message);
    }
  }

  private static Result<(bool IsBalanced, char UnbalancedSymbol)> CheckBracesBalance(string source, BraceManager manager)
  {
    char[] bracesList = manager.BracesList;
    var stack = new Stack<char>();
    char returnSymbol = _zeroCodeSymbol;

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

    bool isBalanced = stack.Count == 0 && returnSymbol == _zeroCodeSymbol;

    if (!isBalanced && returnSymbol == _zeroCodeSymbol)
      returnSymbol = stack.Pop();

    return Result.Success((isBalanced, returnSymbol));
  }
}
