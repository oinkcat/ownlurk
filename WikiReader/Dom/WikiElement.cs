using System;
using System.Collections.Generic;

namespace WikiReader.Dom;

/// <summary>
/// Элемент Wiki-разметки
/// </summary>
public abstract class WikiElement
{
    /// <summary>
    /// Сгенерировать HTML разметку для данного Wiki элемента
    /// </summary>
    /// <param name="visitor">Средство обхода узлов разметки</param>
    public abstract void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor);
}