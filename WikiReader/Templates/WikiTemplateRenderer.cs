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
    private const string LangTemplateNamePrefix = "lang";

    private static readonly Dictionary<string, Type> tempatesMap = new() {
        ["цитата"] = typeof(WikiQuoteRenderer),
        ["q"] = typeof(WikiQuoteRenderer),
        ["nsfw"] = typeof(WikiSpoilerRenderer),
        ["spoiler"] = typeof(WikiSpoilerRenderer),
        ["acronym"] = typeof(WikiSpoilerRenderer),
    };

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
        string templateName = elem.Name.ToLower();

        if (tempatesMap.TryGetValue(templateName, out var rendererType))
        {
            return Activator.CreateInstance(rendererType, [elem]) as WikiTemplateRenderer;
        }
        else if(templateName.StartsWith(LangTemplateNamePrefix))
        {
            elem.IsInline = true;
            return new WikiLangRenderer(elem);
        }
        else
        {
            return new WikiCommonTemplateRenderer(elem);
        }
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
