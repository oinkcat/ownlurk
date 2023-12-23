using System;

namespace WikiReader.Toc;

/// <summary>
/// Статья Wiki
/// </summary>
public class Article
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Имя статьи
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Получить строковое представление статьи
    /// </summary>
    /// <returns>Имя статьи</returns>
    public override string ToString() => Name;
}
