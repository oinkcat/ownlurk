using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader.Templates;

/// <summary>
/// Генерирует разметку спрятанного элемента
/// </summary>
internal class WikiNsfwRenderer : WikiTemplateRenderer
{
    public WikiNsfwRenderer(WikiTemplateElement elem) : base(elem)
    {
    }

    /// <inheritdoc />
    public override void GenerateLayout(TextWriter writer, HtmlGenerationVisitor visitor)
    {
        writer.Write("<div class=\"folded\">");

        var contentElements = elem.Substitutions.Last();
        visitor.VisitMultipleElements(contentElements);

        writer.Write("</div>");
    }
}
