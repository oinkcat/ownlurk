using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Substitution = System.Collections.Generic.List<WikiReader.Dom.WikiElement>;

namespace WikiReader.Dom;

/// <summary>
/// Элемент шаблона
/// </summary>
public class WikiTemplateElement : WikiElement
{
    private Substitution currentSubstitution;

    /// <summary>
    /// Название шаблона
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Подстановки для шаблона
    /// </summary>
    public List<Substitution> Substitutions { get; } = new();

    /// <summary>
    /// Является ли заданная подстановка параметром отображения
    /// </summary>
    /// <param name="sub">Проверяемая подстановка</param>
    /// <returns>Является ли параметром</returns>
    public static bool IsaParameter(Substitution sub)
    {
        return (sub.FirstOrDefault() is WikiTextElement textElem) && textElem.Text.Contains('=');
    }

    /// <summary>
    /// Разделить текст параметра на имя и значение
    /// </summary>
    /// <param name="sub">Подстановка для извлечения информации</param>
    /// <returns>Имя и значение параметра</returns>
    public static (string, string) SplitParmeterInfo(Substitution sub)
    {
        string[] parameterParts = (sub.FirstOrDefault() as WikiTextElement).Text.Split('=');
        return (parameterParts[0], parameterParts[1]);
    }

    /// <summary>
    /// Добавить новую подстановку
    /// </summary>
    public void StartNewSubstitution()
    {
        currentSubstitution = new Substitution();
        Substitutions.Add(currentSubstitution);
    }

    /// <summary>
    /// Добавить элемент к содержимому подстановки
    /// </summary>
    /// <param name="element">Элемент содержимого</param>
    public void AppendSubstitutionContent(WikiElement element)
    {
        currentSubstitution.Add(element);
    }

    public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
    {
        visitor.Visit(this);
    }
}
