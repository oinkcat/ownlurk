using System;
using System.Collections.Generic;

namespace WikiReader.Dom;

/// <summary>
/// Элемент абзаца
/// </summary>
public class WikiParagraphElement : WikiElement, IWikiContentElement
{
    /// <inheritdoc />
    public List<WikiElement> Content { get; set; } = new();

    /// <summary>
    /// Является ли пустым
    /// </summary>
    public bool IsEmpty => Content.Count == 0;

    public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
    {
        visitor.Visit(this);
    }
}
