using System.Collections.Generic;

namespace LurkViewer.Models;

/// <summary>
/// Элемент алфавитного указателя
/// </summary>
public record AlphabetIndexItem<T>(char Letter, IList<T> Items);