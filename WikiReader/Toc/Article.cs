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
}
