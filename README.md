# StringFunctions

`StringFunctions` — небольшая библиотека с утилитами для работы со строками.

Сейчас библиотека включает девять основных блоков:

- проверка баланса скобок и кавычек;
- нормализация строк;
- разбор строковых диапазонов целых чисел в коллекцию;
- форматирование коллекции целых чисел обратно в строку диапазонов;
- согласование существительных с числом в русском языке;
- перевод целых чисел в запись словами (прописью) на русском языке;
- запись денежной суммы прописью на русском языке;
- порядковые числительные словами на русском языке;
- запись даты прописью на русском языке.

Библиотека ориентирована на практическое использование в прикладных проектах и делает упор на:

- предсказуемый контракт;
- отсутствие исключений для ошибок пользовательского ввода;
- возврат результатов через `ResultType`;
- поддержку нескольких версий .NET;
- покрытие тестами.

---

## Поддерживаемые платформы

Библиотека мультитаргетится на:

- `net6.0`
- `net7.0`
- `net8.0`
- `net9.0`
- `net10.0`

Несмотря на то что `net6.0` и `net7.0` уже не поддерживаются Microsoft, они сохранены ради совместимости с существующими проектами.

---

## Зависимости

Публичный API библиотеки использует `ResultType`.

Это означает, что методы, которые могут завершиться ошибкой пользовательского ввода или валидации, возвращают:

- `Result<T>` — если нужно вернуть значение;
- `Result` — если нужно вернуть только факт успеха или текст ошибки.

Такой подход позволяет не использовать исключения для штатных ошибок входных данных.

---

## Возможности библиотеки

### 1. Проверка баланса скобок и кавычек

Проверка выполняется методом `IsBracesBalanced`.

Поддерживаются:

- заранее известные наборы скобок;
- пользовательские пары символов;
- смешанный сценарий, когда используются и известные типы, и пользовательские пары.

Метод возвращает:

- признак сбалансированности;
- символ, на котором баланс нарушается.

Пример:

```csharp
using StringFunctions;
using StringFunctions.Braces;

string source = "(a + b) * [c]";

var result = source.IsBracesBalanced(KnownBracesTypes.CommonBraces);

if (result.IsSuccess)
{
    var (isBalanced, unbalancedSymbol) = result.Value;
    Console.WriteLine(isBalanced);       // True
    Console.WriteLine(unbalancedSymbol); // '\0'
}
else
{
    Console.WriteLine(result.Error);
}
```

Пример с ошибкой:

```csharp
using StringFunctions;
using StringFunctions.Braces;

string source = "(a + b]";

var result = source.IsBracesBalanced(KnownBracesTypes.CommonBraces);

if (result.IsSuccess)
{
    var (isBalanced, unbalancedSymbol) = result.Value;
    Console.WriteLine(isBalanced);       // False
    Console.WriteLine(unbalancedSymbol); // '('
}
```

---

### 2. Нормализация строк

Нормализация выполняется методом `NormalizeString`.

Метод приводит строку к более чистому виду по внутренним правилам библиотеки, убирая:

- лишние пробелы;
- лишние пробелы перед пунктуацией;
- повторяющиеся пробелы;
- лишние разделители вокруг некоторых открывающих и закрывающих символов.

Пример:

```csharp
using StringFunctions;

string source = "  Привет   ,   мир !  ";

var result = source.NormalizeString();

if (result.IsSuccess)
{
    Console.WriteLine(result.Value); // "Привет, мир!"
}
else
{
    Console.WriteLine(result.Error);
}
```

Для `null` возвращается `Failure`, а для пустой или состоящей только из пробелов строки — `Success(string.Empty)`.

---

### 3. Разбор строковых диапазонов целых чисел

Разбор выполняется методом `IntRangeParser.Parse`.

Поддерживаются формы записи:

- `N`
- `N-M`
- `-N`
- `N-`

Дополнительно поддерживаются:

- явный `0`;
- `0-N`;
- `0-`;
- пробелы вокруг `-` внутри диапазона.

Результат всегда:

- отсортирован по возрастанию;
- не содержит дублей.

Пример:

```csharp
using StringFunctions;

var result = IntRangeParser.Parse("1,3-5,8,10-12", 20);

if (result.IsSuccess)
{
    // [1, 3, 4, 5, 8, 10, 11, 12]
    Console.WriteLine(string.Join(", ", result.Value));
}
else
{
    Console.WriteLine(result.Error);
}
```

Пример с открытыми диапазонами:

```csharp
using StringFunctions;

var result = IntRangeParser.Parse("-5, 10-", 12);

if (result.IsSuccess)
{
    // [1, 2, 3, 4, 5, 10, 11, 12]
    Console.WriteLine(string.Join(", ", result.Value));
}
```

Пример с нулём:

```csharp
using StringFunctions;

var result = IntRangeParser.Parse("0-3, 10 - 12", 20);

if (result.IsSuccess)
{
    // [0, 1, 2, 3, 10, 11, 12]
    Console.WriteLine(string.Join(", ", result.Value));
}
```

Полная спецификация: [docs/int-range-parser.md](docs/int-range-parser.md)

---

### 4. Форматирование коллекции чисел в строку диапазонов

Форматирование выполняется методом `IntRangeFormatter.Format`.

Библиотека поддерживает две перегрузки:

- базовую — без знания `maxRangeValue`;
- расширенную — с `maxRangeValue` и возможностью использовать открытые диапазоны.

Formatter:

- принимает `IEnumerable<int>`;
- сортирует входные значения;
- удаляет дубли;
- склеивает соседние значения в диапазоны.

Пример базовой перегрузки:

```csharp
using StringFunctions;

var result = IntRangeFormatter.Format(new[] { 7, 3, 2, 1, 3, 8, 9, 5 }, ", ");

if (result.IsSuccess)
{
    Console.WriteLine(result.Value); // "1-3, 5, 7-9"
}
```

Пример с использованием открытых диапазонов:

```csharp
using StringFunctions;

var result = IntRangeFormatter.Format(new[] { 1, 2, 3, 4, 5 }, 10);

if (result.IsSuccess)
{
    Console.WriteLine(result.Value); // "-5"
}
```

Пример с явным нулём:

```csharp
using StringFunctions;

var result = IntRangeFormatter.Format(new[] { 0, 1, 2, 3 }, 3);

if (result.IsSuccess)
{
    Console.WriteLine(result.Value); // "0-"
}
```

Полная спецификация: [docs/int-range-formatter.md](docs/int-range-formatter.md)

---

### 5. Согласование существительных с числом (русский язык)

Русский язык требует разной формы существительного при счёте: «1 яблоко», «2 яблока»,
«5 яблок». Класс `RussianPlural` (пространство имён `StringFunctions.Russian`) помогает
подобрать правильную форму.

- `GetForm` — определяет грамматическую форму (`One` / `Few` / `Many`) и не может завершиться ошибкой;
- `Pluralize` — возвращает подходящую форму слова;
- `Quantify` — собирает строку вида «5 яблок».

Знак числа не влияет на результат.

Пример:

```csharp
using StringFunctions.Russian;

var word = RussianPlural.Pluralize(5, "яблоко", "яблока", "яблок");
Console.WriteLine(word.Value); // "яблок"

var phrase = RussianPlural.Quantify(2, "файл", "файла", "файлов");
Console.WriteLine(phrase.Value); // "2 файла"

RussianPluralForm form = RussianPlural.GetForm(21);
Console.WriteLine(form); // One
```

---

### 6. Число прописью (русский язык)

Класс `RussianNumberToWords` (пространство имён `StringFunctions.Russian`) переводит
целые числа в запись словами в именительном падеже.

- учитывается грамматический род единиц (`один/одна/одно`, `два/две`);
- разрядные слова согласуются с числом (`тысяча/тысячи/тысяч`, `миллион/миллиона/миллионов`);
- поддерживается склонение по всем 6 падежам (`RussianCase`);
- поддерживается весь диапазон `long`, включая отрицательные значения и `long.MinValue`.

Перевод поддерживает склонение по падежам через перегрузку с `RussianCase`, а
винительный падеж формируется для неодушевлённого счёта.

Пример:

```csharp
using StringFunctions.Russian;

Console.WriteLine(RussianNumberToWords.Convert(1_234_567));
// "один миллион двести тридцать четыре тысячи пятьсот шестьдесят семь"

Console.WriteLine(RussianNumberToWords.Convert(2000, RussianGender.Feminine));
// "две тысячи"

Console.WriteLine(RussianNumberToWords.Convert(523, RussianCase.Genitive));
// "пятисот двадцати трёх"

Console.WriteLine(RussianNumberToWords.Convert(-5));
// "минус пять"
```

### Эргономичные методы расширения

Для чисел `int` и `long` доступны методы расширения (пространство имён `StringFunctions.Russian`):

```csharp
using StringFunctions.Russian;

Console.WriteLine(523.ToRussianWords(RussianCase.Genitive)); // "пятисот двадцати трёх"
Console.WriteLine(5.Pluralize("яблоко", "яблока", "яблок").Value); // "яблок"
Console.WriteLine(2.Quantify("файл", "файла", "файлов").Value); // "2 файла"
RussianPluralForm form = 5.GetRussianPluralForm(); // Many
```

---

### 7. Сумма (валюта) прописью (русский язык)

Класс `RussianMoneyToWords` (пространство имён `StringFunctions.Russian`) записывает денежную
сумму прописью.

- основная часть всегда прописью и согласуется с валютой (род, счётная форма);
- разменная часть — прописью или цифрами (`RussianMinorFormat`);
- валюта настраивается через `RussianCurrency`; готовые варианты: `Rubles`, `Dollars`, `Euros`;
- поддерживаются перегрузки по `(long major, int minor)` и по `decimal`.

Пример:

```csharp
using StringFunctions.Russian;

Console.WriteLine(RussianMoneyToWords.Convert(123, 45, RussianCurrency.Rubles).Value);
// "сто двадцать три рубля сорок пять копеек"

Console.WriteLine(RussianMoneyToWords.Convert(100, 5, RussianCurrency.Rubles, RussianMinorFormat.Digits).Value);
// "сто рублей 05 коп."

Console.WriteLine(123.45m.ToRussianMoney().Value);
// "сто двадцать три рубля сорок пять копеек"
```

Своя валюта задаётся через `RussianCurrency` и `RussianNoun`:

```csharp
var tenge = new RussianCurrency(
    new RussianNoun("тенге", "тенге", "тенге", RussianGender.Masculine),
    new RussianNoun("тиын", "тиына", "тиынов", RussianGender.Masculine),
    "тиын");
```

---

### 8. Порядковые числительные (русский язык)

Класс `RussianOrdinalToWords` (пространство имён `StringFunctions.Russian`) переводит число
в порядковое числительное с учётом рода и падежа.

- в составном числительном склоняется и принимает род только последнее слово;
- поддерживаются все роды и падежи (`RussianGender`, `RussianCase`);
- круглые разряды дают `тысячный`, `двухтысячный`, `миллионный` и т. п.

```csharp
using StringFunctions.Russian;

Console.WriteLine(RussianOrdinalToWords.Convert(2026)); // "две тысячи двадцать шестой"
Console.WriteLine(RussianOrdinalToWords.Convert(2026, RussianGender.Masculine, RussianCase.Genitive));
// "две тысячи двадцать шестого"
Console.WriteLine(21.ToRussianOrdinal(RussianGender.Neuter)); // "двадцать первое"
```

---

### 9. Дата прописью (русский язык)

Класс `RussianDateToWords` (пространство имён `StringFunctions.Russian`) записывает дату прописью.

- день — порядковое среднего рода, месяц — в родительном падеже, год — порядковое в родительном со словом «года»;
- падеж дня выбирается параметром (именительный или родительный);
- есть перегрузки по `(year, month, day)`, `DateOnly` и `DateTime`.

```csharp
using StringFunctions.Russian;

Console.WriteLine(RussianDateToWords.Convert(2026, 6, 19).Value);
// "девятнадцатое июня две тысячи двадцать шестого года"

Console.WriteLine(RussianDateToWords.Convert(2026, 6, 19, RussianCase.Genitive).Value);
// "девятнадцатого июня две тысячи двадцать шестого года"

Console.WriteLine(new DateOnly(2025, 3, 1).ToRussianWords().Value);
// "первое марта две тысячи двадцать пятого года"
```

> Все задуманные модули работы с числительными реализованы: согласование с числом,
> число прописью, склонение по падежам, сумма/валюта прописью, порядковые числительные и дата.

---

## Краткий контракт `IntRangeParser`

### Поддерживаемый формат

- `N` — одиночное число;
- `N-M` — обычный диапазон;
- `-N` — открытый слева диапазон от `1` до `N`;
- `N-` — открытый справа диапазон от `N` до `maxRangeValue`.

### Правила для нуля

- `0` допустим;
- `0-N` допустим;
- `0-` допустим;
- `-N` всегда означает диапазон от `1`, а не от `0`.

### Ошибки

Метод возвращает `Failure`, если:

- `rangeSource == null`;
- `maxRangeValue < 0`;
- токен имеет некорректный формат;
- явно указано значение меньше `0`;
- граница выходит за пределы `0..maxRangeValue`;
- `-N` используется не первым токеном;
- `N-` используется не последним токеном.

---

## Краткий контракт `IntRangeFormatter`

Formatter принимает произвольную последовательность `IEnumerable<int>` и возвращает нормализованную строку диапазонов.

### Правила

- отрицательные значения недопустимы;
- вход может быть неотсортированным;
- дубли допустимы;
- перед форматированием вход сортируется и дедуплицируется;
- соседние значения объединяются в диапазоны.

### Открытые диапазоны

Во второй перегрузке при `useOpenRanges = true` могут использоваться:

- `-N` для диапазона `1..N`;
- `N-` для диапазона `N..maxRangeValue`;
- `0-` для диапазона `0..maxRangeValue`.

---

## Ограничения и безопасность

Библиотека выполняет только вычисления над строками: нет ввода-вывода, сети, рефлексии,
десериализации и запуска процессов. Поэтому классических векторов внедрения (инъекций, RCE)
у неё нет. Единственный практический риск при работе с недоверенным вводом — **потребление
памяти**, и его стоит учитывать:

- `IntRangeParser.Parse` возвращает список всех значений диапазона, поэтому объём
  выделяемой памяти пропорционален **мощности** результата, а не длине входной строки.
  Запрос вида `"1-"` или `"0-N"` при большом `maxRangeValue` может потребовать сотни
  мегабайт и более.
- Если запрошенный набор не помещается в `List<int>` (превышает `Array.MaxLength`),
  метод возвращает `Failure`, а не бросает исключение.
- **Рекомендация:** при разборе данных из недоверенного источника ограничивайте
  `maxRangeValue` разумным значением на стороне вызывающего кода. Библиотека намеренно
  не навязывает собственный жёсткий лимит, чтобы не ограничивать легитимные сценарии.

`IntRangeFormatter` и `NormalizeString` работают за один проход и выделяют память
пропорционально размеру входных данных.

---

## Подключение

### Вариант 1. Через исходный код / ProjectReference

```xml
<ProjectReference Include="..\StringFunctions\StringFunctions.csproj" />
```

### Вариант 2. Через пакет

После публикации библиотеку можно будет подключать как пакет NuGet.

---

## Тестирование

Для библиотеки добавлены автоматические тесты.

Покрываются:

- parser;
- formatter;
- проверка баланса скобок;
- нормализация строк.

Запуск тестов:

```bash
dotnet test
```

---

## Документация

Дополнительные материалы:

- [docs/int-range-parser.md](docs/int-range-parser.md)
- [docs/int-range-formatter.md](docs/int-range-formatter.md)

Также для публичного API добавлены XML-комментарии.

---

## Лицензия

См. файл [LICENSE](LICENSE).

---

## Статус проекта

Проект развивается как прикладная библиотека с упором на практическую полезность и предсказуемое поведение API.

Основные недавние изменения:

- добавлен `IntRangeParser`;
- добавлен `IntRangeFormatter`;
- публичный API приведён к стилю `ResultType`;
- добавлены тесты и документация.
