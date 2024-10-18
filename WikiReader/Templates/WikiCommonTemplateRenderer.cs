using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader.Templates;

/// <summary>
/// Генератор разметки остальных шаблонов
/// </summary>
internal class WikiCommonTemplateRenderer : WikiTemplateRenderer
{
    public WikiCommonTemplateRenderer(WikiTemplateElement elem) : base(elem)
    {
    }

    public override void GenerateLayout(TextWriter writer, HtmlGenerationVisitor visitor)
    {
        writer.Write("Template: ");
        writer.Write(Name);
    }
}
