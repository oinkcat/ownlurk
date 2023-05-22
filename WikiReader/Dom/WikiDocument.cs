using System;
using System.Collections.Generic;

namespace WikiReader.Dom;

/// <summary>
/// Документ Wiki-разметки
/// </summary>
public class WikiDocument
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
    public List<WikiElement> Contents { get; }

    public WikiDocument(string title = DefaultTitle)
    {
        Title = title;
        Contents = new List<WikiElement>();
    }

    /// <summary>
    /// Добавить содержимое
    /// </summary>
    /// <param name="element">Элемент содержимого</param>
    public void AppendContent(WikiElement element)
    {
        if(element != null)
        {
            Contents.Add(element);
        }
    }
}