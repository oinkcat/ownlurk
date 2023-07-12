using System;
using System.Collections.Generic;
using System.Linq;
using WikiReader.Dom;

namespace WikiReader;

/// <summary>
/// Осуществляет обход узлов элементов Wiki разметки
/// </summary>
internal interface IWikiElementVisitor
{
    /// <summary>
    /// Выполнить обход узла элемента
    /// </summary>
    /// <param name="element">Элемент разметки</param>
    void Visit(WikiElement element);
}