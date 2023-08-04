using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom;

/// <summary>
/// Тип форматирования
/// </summary>
public enum FormattingType
{
    Undefined,
    Bold,
    Italic
}

/// <summary>
/// Элемент форматированного текста
/// </summary>
public class WikiFormattedElement : WikiElement, IWikiContentElement
{
    /// <summary>
    /// Отображаемое содержимое
    /// </summary>
    public List<WikiElement> Content { get; } = new();

    /// <summary>
    /// Тип форматирования
    /// </summary>
    public FormattingType Type { get; set; }

    public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
    {
        visitor.Visit(this);
    }
}