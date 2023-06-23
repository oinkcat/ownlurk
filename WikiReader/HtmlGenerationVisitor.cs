using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader
{
    /// <summary>
    /// Генерирует разметку HTML для элементов Wiki разметки
    /// </summary>
    public class HtmlGenerationVisitor : IWikiElementVisitor
    {
        private readonly TextWriter outHtmlWriter;

        public HtmlGenerationVisitor(TextWriter writer)
        {
            outHtmlWriter = writer;
        }

        /// <summary>
        /// Выполнить обход элемента Wiki разметки
        /// </summary>
        /// <param name="element">Элемент, обход которого осуществляется</param>
        public void Visit(WikiElement element)
        {
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
            VisitElementContent(headerElem);
            WriteEndTag(headerTag);

            outHtmlWriter.WriteLine();
        }

        private void VisitLinkElement(WikiLinkElement linkElem)
        {
            string href = linkElem.IsExternal
                    ? linkElem.Uri.Text
                    : $"#{linkElem.Uri.Text}"; // TODO: Internal link href

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
    }
}