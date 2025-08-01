using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using WikiReader.Dom;

namespace WikiReader
{
    /// <summary>
    /// Генерирует разметку HTML из объектов Wiki-разметки
    /// </summary>
    public class HtmlGenerator
    {
        private const string TitleField = "TITLE";

        private const string ContentField = "CONTENT";

        private const string FieldRegexTemplate = "%(.+)%";

        private readonly WikiDocument document;

        private readonly string templatePath;

        private TextWriter outHtmlWriter;

        public HtmlGenerator(WikiDocument document, string templatePath)
        {
            this.document = document;
            this.templatePath = templatePath;
        }

        /// <summary>
        /// Сгенерировать HTML документ
        /// </summary>
        /// <param name="outHtmlWriter">Объект для записи разметки</param>
        public void Generate(TextWriter outHtmlWriter)
        {
            this.outHtmlWriter = outHtmlWriter;

            var (contentStartPos, templateText) = LoadAndPrepareTemplate();

            if(contentStartPos == -1)
            {
                throw new ApplicationException("No content placeholder field found!");
            }

            WriteDocumentContentToTemplate(contentStartPos, templateText);
        }

        private (int, string) LoadAndPrepareTemplate()
        {
            string templateText = File.ReadAllText(templatePath);

            var fieldRegex = new Regex(FieldRegexTemplate, RegexOptions.Compiled);

            int contentStartPos = -1;
            int extraLength = 0;

            string preparedTemplateText = fieldRegex.Replace(templateText, match =>
            {
                string fieldName = match.Groups[1].Value;

                if (fieldName == TitleField)
                {
                    extraLength = document.Title.Length - match.Value.Length;
                    return document.Title;
                }
                else if (fieldName == ContentField)
                {
                    if(contentStartPos == -1)
                    {
                        contentStartPos = match.Index + extraLength;
                    }
                    return String.Empty;
                }
                else
                {
                    return match.Value;
                }
            });

            return (contentStartPos, preparedTemplateText);
        }

        private void WriteDocumentContentToTemplate(int contentPos, string templateText)
        {
            outHtmlWriter.Write(templateText[0..contentPos]);

            var htmlVisitor = new HtmlGenerationVisitor(outHtmlWriter);

            foreach (var contentElement in document.Content)
            {
                contentElement.AcceptHtmlGenerationVisitor(htmlVisitor);
            }

            outHtmlWriter.Write(templateText[contentPos..]);
        }
    }
}
