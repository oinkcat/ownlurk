using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom;

/// <summary>
/// Текстовый элемент
/// </summary>
public class WikiTextElement : WikiElement
{
    /// <summary>
    /// Содерэимое элемента
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Экранированы ли символы
    /// </summary>
    public bool IsEscaped { get; set; }

    public WikiTextElement(string text) => Text = text;

    /// <summary>
    /// Выдать текст элемента как строку
    /// </summary>
    /// <returns>Текст элемента</returns>
    public override string ToString() => Text;
}