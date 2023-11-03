using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader.Templates
{
    /// <summary>
    /// Генерирует разметку элемента текста на иностранном языке
    /// </summary>
    internal class WikiLangRenderer : WikiTemplateRenderer
    {
        private readonly string languageCode;

        public WikiLangRenderer(WikiTemplateElement elem) : base(elem)
        {
            languageCode = elem.Name.Contains('-') ? elem.Name.Split('-')[1] : "?";
        }

        /// <inheritdoc />
        public override void GenerateLayout(TextWriter writer, HtmlGenerationVisitor visitor)
        {
            writer.Write($"<i>{languageCode}</i>: ");
            visitor.VisitMultipleElements(elem.Substitutions[0]);
        }
    }
}
