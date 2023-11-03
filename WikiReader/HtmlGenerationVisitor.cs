using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WikiReader.Dom;
using WikiReader.Templates;
using System.Reflection.Metadata.Ecma335;

namespace WikiReader
{
    /// <summary>
    /// Генерирует разметку HTML для элементов Wiki разметки
    /// </summary>
    public class HtmlGenerationVisitor : IWikiElementVisitor
    {
        private readonly TextWriter outHtmlWriter;

        private int chapterIdx = 0;

        public HtmlGenerationVisitor(TextWriter writer)
        {
            outHtmlWriter = writer;
        }

        /// <summary>
        /// Выполнить обход списка элементов Wiki разметки
        /// </summary>
        /// <param name="elements">Список элементов, обход которых выполнить</param>
        public void VisitMultipleElements(List<WikiElement> elements)
        {
            foreach (var elem in elements)
            {
                if(elem == null) { continue; }

                elem.AcceptHtmlGenerationVisitor(this);
            }
        }

        /// <summary>
        /// Выполнить обход элемента Wiki разметки
        /// </summary>
        /// <param name="element">Элемент, обход которого осуществляется</param>
        public void Visit(WikiElement element)
        {
            if(element == null) { return; }

            if(element is WikiTextElement textElem)
            {
                outHtmlWriter.Write(textElem.Text);
            }
            else if(element is WikiFormattedElement formattedElem)
            {
                VisitFormattedElement(formattedElem);
            }
            else if(element is WikiHeaderElement headerElem)
            {
                VisitHeaderElement(headerElem);
            }
            else if(element is WikiLinkElement linkElem)
            {
                VisitLinkElement(linkElem);
            }
            else if(element is WikiListElement listElem)
            {
                VisitListElement(listElem);
            }
            else if(element is WikiTemplateElement templateElem)
            {
                VisitTemplateElement(templateElem);
            }
            else if(element is WikiParagraphElement paragraphElem)
            {
                VisitParagraphElement(paragraphElem);
            }
            else if(element is WikiEolElement)
            {
                outHtmlWriter.WriteLine();
            }
        }

        private void VisitFormattedElement(WikiFormattedElement formattedElem)
        {
            string htmlFormattingTag = (formattedElem.Type == FormattingType.Bold)
                ? "strong"
                : "em";

            WriteStartTag(htmlFormattingTag);
            VisitElementContent(formattedElem);
            WriteEndTag(htmlFormattingTag);
        }

        private void WriteStartTag(string tagName, bool appendNewLine = false)
        {
            outHtmlWriter.Write($"<{tagName}>");

            if(appendNewLine)
            {
                outHtmlWriter.WriteLine();
            }
        }

        private void WriteEndTag(string tagName, bool appendNewLine = false)
        {
            outHtmlWriter.Write($"</{tagName}>");

            if (appendNewLine)
            {
                outHtmlWriter.WriteLine();
            }
        }

        private void VisitElementContent(IWikiContentElement contentElem)
        {
            foreach(var elem in contentElem.Content)
            {
                elem.AcceptHtmlGenerationVisitor(this);
            }
        }

        private void VisitHeaderElement(WikiHeaderElement headerElem)
        {
            string headerTag = $"h{headerElem.Level}";
            WriteStartTag(headerTag);
            WriteStartTag($"a name=\"p_{headerElem.Index}\"");
            
            VisitElementContent(headerElem);

            WriteEndTag("a");
            WriteEndTag(headerTag);

            outHtmlWriter.WriteLine();
        }

        private void VisitLinkElement(WikiLinkElement linkElem)
        {
            const string InternalLinkDomain = "127.0.0.1";

            string href = linkElem.IsExternal
                    ? linkElem.Uri.Text
                    : $"http://{InternalLinkDomain}/{linkElem.Uri.Text}";

            outHtmlWriter.Write($"<a href=\"{href}\">");

            if(linkElem.Content.Any())
            {
                VisitElementContent(linkElem);
            }
            else
            {
                outHtmlWriter.Write(linkElem.Uri.Text);
            }

            WriteEndTag("a");
        }

        private void VisitListElement(WikiListElement listElem)
        {
            string listTag = listElem.IsNumbered ? "ol" : "ul";
            WriteStartTag(listTag, true);

            foreach(var itemContents in listElem.ContentItems)
            {
                WriteStartTag("li", true);

                foreach (var elem in itemContents)
                {
                    elem?.AcceptHtmlGenerationVisitor(this);
                }

                WriteEndTag("li", true);
            }

            WriteEndTag(listTag, true);
        }

        private void VisitTemplateElement(WikiTemplateElement templateElem)
        {
            // HACK: устанавливает свойство IsInline для некоторых шаблонов
            var renderer = WikiTemplateRenderer.CreateForTemplate(templateElem);

            string tagName = templateElem.IsInline ? "span" : "div";
            WriteStartTag($"{tagName} class=\"template\"");

            renderer.GenerateLayout(outHtmlWriter, this);

            WriteEndTag(tagName);
        }

        private void VisitParagraphElement(WikiParagraphElement paragraphElem)
        {
            WriteStartTag("p");
            VisitElementContent(paragraphElem);
            WriteEndTag("p");
        }
    }
}