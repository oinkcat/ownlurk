using System;
using System.Collections.Generic;

namespace WikiReader.Dom;

/// <summary>
/// Документ Wiki-разметки
/// </summary>
public class WikiDocument : IWikiContentElement
{
    private const string DefaultTitle = "Unnamed";

    /// <summary>
    /// Идентификатор документа
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Заголовок документа
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Элементы содержимого
    /// </summary>
    public List<WikiElement> Content { get; }

    /// <summary>
    /// Абзацы статьи
    /// </summary>
    public string[] Paragraphs { get; set; }

    public WikiDocument(string title = DefaultTitle)
    {
        Title = title;
        Content = new List<WikiElement>();
    }
}