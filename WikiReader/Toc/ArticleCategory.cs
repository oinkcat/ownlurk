using System;
using System.Collections.Generic;

namespace WikiReader.Toc;

/// <summary>
/// Категория статей
/// </summary>
public class ArticleCategory
{
    /// <summary>
    /// Имя категории
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Статьи категории
    /// </summary>
    public SortedList<string, Article> Articles { get; set; }

    /// <summary>
    /// Число статей в категории
    /// </summary>
    public int ArticlesCount => Articles.Count;

    /// <summary>
    /// Создать информацию о категории
    /// </summary>
    /// <param name="name">Имя новой категории</param>
    /// <returns>Информация о новой категории</returns>
    public static ArticleCategory Create(string name) => new()
    {
        Name = name,
        Articles = new SortedList<string, Article>()
    };

    /// <summary>
    /// Добавить информацию о статье
    /// </summary>
    /// <param name="articleId">Идентификатор статьи</param>
    /// <param name="articleName">Название статьи</param>
    public void AddArticleInfo(int articleId, string articleName)
    {
        var newArticle = new Article
        {
            Id = articleId,
            Name = articleName
        };

        _ = Articles.TryAdd(articleName, newArticle);
    }
}
