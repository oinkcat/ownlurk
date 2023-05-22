using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom;

/// <summary>
/// Элемент ссылки
/// </summary>
public class WikiLinkElement : WikiElement
{
    /// <summary>
    /// Идентификатор ресурса
    /// </summary>
    public WikiTextElement Uri { get; set; }

    /// <summary>
    /// Отображаемое содержимое
    /// </summary>
    public List<WikiElement> Content { get; set; } = new();

    /// <summary>
    /// Внешняя ссылка
    /// </summary>
    public bool IsExternal { get; set; }

    public override string ToString() => Uri.Text;
}