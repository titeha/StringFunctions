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
