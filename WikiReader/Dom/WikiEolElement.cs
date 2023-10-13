using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom;

/// <summary>
/// Элемент конца строки
/// </summary>
public class WikiEolElement : WikiElement
{
    /// <summary>
    /// Окончание абзаца
    /// </summary>
    public bool IsaParagraphDelimiter { get; set; }

    public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
    {
        visitor.Visit(this);
    }
}