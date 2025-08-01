using System;
using System.Collections.Generic;

namespace WikiReader.Dom;

/// <summary>
/// Элемент Wiki разметки, имеющий содержимое
/// </summary>
public interface IWikiContentElement
{
    /// <summary>
    /// Содержимое
    /// </summary>
    List<WikiElement> Content { get; set; }

    /// <summary>
    /// Добавить содержимое
    /// </summary>
    /// <param name="element">Элемент содержимого</param>
    public void AppendContent(WikiElement element)
    {
        if(element != null)
        {
            Content.Add(element);
        }
    }
}