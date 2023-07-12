using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using WikiReader.Dom;

namespace WikiReader.Templates;

/// <summary>
/// Созлает разметку шаблона
/// </summary>
internal abstract class WikiTemplateRenderer
{
    private const string LayoutNameCite = "цитата";
    private const string LayoutNameNsfw = "nsfw";

    protected readonly WikiTemplateElement elem;

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Создать для элемента разметки шаблона
    /// </summary>
    /// <param name="elem">Элемент разметки шаблона</param>
    /// <returns>Отрисовщик шаблона</returns>
    public static WikiTemplateRenderer CreateForTemplate(WikiTemplateElement elem)
    {
        return elem.Name.ToLower() switch
        {
            LayoutNameCite => new WikiQuoteRenderer(elem),
            LayoutNameNsfw => new WikiNsfwRenderer(elem),
            _ => new WikiCommonTemplateRenderer(elem)
        };
    }

    public WikiTemplateRenderer(WikiTemplateElement elem)
    {
        this.elem = elem;
        Name = elem.Name;
    }

    /// <summary>
    /// Сгенерировать текст разметки для шаблона
    /// </summary>
    /// <param name="writer">Объект записи текста в поток</param>
    /// <param name="visitor">Генератор разметки элементов</param>
    public abstract void GenerateLayout(TextWriter writer, HtmlGenerationVisitor visitor);
}
