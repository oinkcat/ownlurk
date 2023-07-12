using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader.Templates;

/// <summary>
/// Генерирует разметку шаблона цитаты
/// </summary>
internal class WikiQuoteRenderer : WikiTemplateRenderer
{
    private const string PreformattedTextParamName = "pre";

    private TextWriter writer;

    private HtmlGenerationVisitor visitor;

    public WikiQuoteRenderer(WikiTemplateElement elem) : base(elem)
    {
    }

    /// <inheritdoc />
    public override void GenerateLayout(TextWriter writer, HtmlGenerationVisitor visitor)
    {
        this.writer = writer;
        this.visitor = visitor;

        writer.WriteLine("<blockquote>");

        bool preFormattedText = false;

        if (WikiTemplateElement.IsaParameter(elem.Substitutions[0]))
        {
            var (name, value) = WikiTemplateElement.SplitParmeterInfo(elem.Substitutions[0]);
            preFormattedText = (name == PreformattedTextParamName) && (value == "1");
        }

        if(preFormattedText)
        {
            OutputPreFormattedTextQuote(elem.Substitutions[1]);
        }
        else
        {
            OutputGenericQuote(elem.Substitutions[0]);
        }

        writer.WriteLine("</blockquote>");
    }

    private void OutputPreFormattedTextQuote(List<WikiElement> content)
    {
        writer.WriteLine("<pre>");
        OutputGenericQuote(content);
        writer.WriteLine("</pre>");
    }

    private void OutputGenericQuote(List<WikiElement> content)
    {
        visitor.VisitMultipleElements(content);
    }
}
