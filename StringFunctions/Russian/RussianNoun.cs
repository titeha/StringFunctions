namespace StringFunctions.Russian;

/// <summary>
/// Существительное, согласуемое с числом: три счётные формы и грамматический род.
/// </summary>
/// <remarks>
/// Формы соответствуют <see cref="RussianPluralForm"/>: <see cref="One"/> — для 1, 21, 101;
/// <see cref="Few"/> — для 2–4, 22–24; <see cref="Many"/> — для 0, 5–20 и т. п.
/// Для несклоняемых слов (например, «евро») все три формы совпадают.
/// </remarks>
public readonly struct RussianNoun
{
  /// <summary>
  /// Создаёт описание существительного.
  /// </summary>
  /// <param name="one">Форма для чисел, оканчивающихся на 1 (кроме 11), например «рубль».</param>
  /// <param name="few">Форма для чисел, оканчивающихся на 2–4 (кроме 12–14), например «рубля».</param>
  /// <param name="many">Форма для остальных чисел, например «рублей».</param>
  /// <param name="gender">Грамматический род (влияет на «один/одна», «два/две»). По умолчанию мужской.</param>
  /// <exception cref="ArgumentNullException">Любая из форм равна <c>null</c>.</exception>
  public RussianNoun(string one, string few, string many, RussianGender gender = RussianGender.Masculine)
  {
    One = one ?? throw new ArgumentNullException(nameof(one));
    Few = few ?? throw new ArgumentNullException(nameof(few));
    Many = many ?? throw new ArgumentNullException(nameof(many));
    Gender = gender;
  }

  /// <summary>Форма для чисел, оканчивающихся на 1 (кроме 11). Пример: «рубль».</summary>
  public string One { get; }

  /// <summary>Форма для чисел, оканчивающихся на 2–4 (кроме 12–14). Пример: «рубля».</summary>
  public string Few { get; }

  /// <summary>Форма для остальных чисел. Пример: «рублей».</summary>
  public string Many { get; }

  /// <summary>Грамматический род существительного.</summary>
  public RussianGender Gender { get; }

  /// <summary>Возвращает форму, согласованную с числом <paramref name="count"/>.</summary>
  public string Form(long count) =>
    RussianPlural.GetForm(count) switch
    {
      RussianPluralForm.One => One,
      RussianPluralForm.Few => Few,
      _ => Many
    };
}
