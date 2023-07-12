using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom;

/// <summary>
/// Элемент ссылки
/// </summary>
public class WikiLinkElement : WikiElement, IWikiContentElement
{
    /// <summary>
    /// Идентификатор ресурса
    /// </summary>
    public WikiTextElement Uri { get; set; }

    /// <summary>
    /// Отображаемое содержимое
    /// </summary>
    public List<WikiElement> Content { get; } = new();

    /// <summary>
    /// Внешняя ссылка
    /// </summary>
    public bool IsExternal { get; set; }

    /// <summary>
    /// Исправить текст содержимого внешней ссылки (отделен пробелом от URL)
    /// </summary>
    public void FixExternalLinkContent()
    {
        if(IsExternal)
        {
            string[] contentParts = Uri.Text.Split(' ', 2);

            if(contentParts.Length > 1)
            {
                Uri.Text = contentParts[0];
                string linkDescriptionText = contentParts[1];
                Content.Insert(0, new WikiTextElement(linkDescriptionText));
            }
        }
    }

    public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        string prefix = IsExternal ? "ext:" : String.Empty;
        return $"{prefix}{Uri.Text}";
    }
}