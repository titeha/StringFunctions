## IntRangeParser

Парсер строковых диапазонов целых чисел.

Поддерживает:

- `N`
- `N-M`
- `-N` -> от `1` до `N`
- `N-` -> от `N` до `maxRangeValue`
- явный `0`
- пробелы вокруг `-`

Пример:

```csharp
var result = IntRangeParser.Parse("0-3, 10 - 12, 20-", 25);
```

Полный контракт: [docs/int-range-parser.md](docs/int-range-parser.md)

## IntRangeFormatter

`IntRangeFormatter` преобразует последовательность целых чисел в нормализованную строку диапазонов.

Поддерживает:

- одиночные значения: `N`
- обычные диапазоны: `N-M`
- открытые диапазоны во второй перегрузке:
  - `-N` для `1..N`
  - `N-` для `N..maxRangeValue`
  - `0-` для `0..maxRangeValue`
- `IEnumerable<int>` как вход
- сортировку и удаление дублей перед форматированием

Пример:

```csharp
var result = IntRangeFormatter.Format(new[] { 7, 3, 2, 1, 3, 8, 9, 5 }, ", ");

if (result.IsSuccess)
{
    Console.WriteLine(result.Value);
    // 1-3, 5, 7-9
}
```

Пример с открытыми диапазонами:

```csharp
var result = IntRangeFormatter.Format(new[] { 1, 2, 3, 4, 5 }, 10);

if (result.IsSuccess)
{
    Console.WriteLine(result.Value);
    // -5
}
```

Полный контракт formatter-а: [docs/int-range-formatter.md](docs/int-range-formatter.md)
