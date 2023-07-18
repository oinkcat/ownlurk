using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace WikiReader.Toc;

/// <summary>
/// Оглавление
/// </summary>
public class TableOfContents
{
    // Индекс для определения идентификатора по имени статьи
    private readonly Dictionary<string, int> index = new();

    /// <summary>
    /// Список категорий
    /// </summary>
    public List<ArticleCategory> Categories { get; } = new();

    /// <summary>
    /// Общее число статей
    /// </summary>
    public int TotalArticlesCount => index.Count;

    /// <summary>
    /// Загрузить из потока данных
    /// </summary>
    public async Task LoadFromStream(Stream tocFileStream)
    {
        index.Clear();
        Categories.Clear();

        await Task.Run(() => LoadTocData(tocFileStream));
    }

    private void LoadTocData(Stream tocFileStream)
    {
        const int NumTocItemParts = 3;
        const char TocItemPartsDelimiter = ';';

        ArticleCategory currentCategory = null;
        string prevCategoryName = null;

        using var tocReader = new StreamReader(tocFileStream);

        while (!tocReader.EndOfStream)
        {
            string tocLine = tocReader.ReadLine();
            var itemParts = tocLine.Split(TocItemPartsDelimiter);

            if (itemParts.Length == NumTocItemParts)
            {
                string categoryName = itemParts[0].Trim();
                string articleName = itemParts[1].Trim();
                int articleId = int.Parse(itemParts[2]);

                if (categoryName != prevCategoryName)
                {
                    currentCategory = ArticleCategory.Create(categoryName);
                    Categories.Add(currentCategory);
                    prevCategoryName = currentCategory.Name;
                }

                if(!String.IsNullOrWhiteSpace(articleName))
                {
                    currentCategory.AddArticleInfo(articleId, articleName);
                    index.TryAdd(articleName, articleId);
                }
            }
        }
    }

    /// <summary>
    /// Получить идентификатор статьи по ее имени
    /// </summary>
    /// <param name="articleName">Имя статьи</param>
    /// <returns>Идентификатор статьи</returns>
    public int? GetArticleIdByName(string articleName)
    {
        return index.TryGetValue(articleName, out int articleId)
            ? articleId
            : null;
    }
}
