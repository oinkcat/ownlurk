using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom;

/// <summary>
/// Элемент заголовка
/// </summary>
public class WikiHeaderElement : WikiElement, IWikiContentElement
{
    /// <summary>
    /// Уровень заголовка
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Порядковый индекс
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Элементы содержимого
    /// </summary>
    public List<WikiElement> Content { get; } = new();

    public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
    {
        visitor.Visit(this);
    }
}