﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using WikiReader;
using WikiReader.Bundle;
using WikiReader.Toc;
using LurkViewer.Models;

using CategoryIndexItem = LurkViewer.Models.AlphabetIndexItem<WikiReader.Toc.ArticleCategory>;
using WikiReader.Dom;

namespace LurkViewer.Services
{
    /// <summary>
    /// Собрание статей Lurkmore
    /// </summary>
    internal class LurkLibrary
    {
        private const string BundleName = "lurk_data.zip";

        private const string TemplateName = "template.html";

        private readonly string templateFilePath;

        private readonly ArticleDocumentsCache cache;

        private ContentBundle bundle;

        private TableOfContents toc;

        /// <summary>
        /// Алфавитный указатель категорий
        /// </summary>
        public List<CategoryIndexItem> CategoryIndex { get; private set; }

        /// <summary>
        /// Единственный экземпляр
        /// </summary>
        public static LurkLibrary Instance { get; }

        /// <summary>
        /// Категории статей
        /// </summary>
        public List<ArticleCategory> Categories => toc.Categories;

        static LurkLibrary()
        {
            Instance = new LurkLibrary();
        }

        private LurkLibrary()
        {
            cache = new ArticleDocumentsCache();
            templateFilePath = Path.Combine(FileSystem.CacheDirectory, TemplateName);
        }

        /// <summary>
        /// Инициализировать архив и таблицу содержимого
        /// </summary>
        public async Task Initialize()
        {
            if(bundle != null || toc != null) { return; }

            // Кэшировать ресурсы
            await CacheAssets();

            // Загрузить данные
            bundle = new ContentBundle(await FileSystem.OpenAppPackageFileAsync(BundleName));
            toc = new TableOfContents();
            await toc.LoadFromStream(bundle.GetTocStream());

            // Отфильтровать только имеющиеся статьи
            RemoveEmptyArticlesAndCategories();

            // Построить алфавитный указатель категорий
            BuildAlphabetCategoryIndex();
        }

        private async Task CacheAssets()
        {
            (string, string)[] assets = {
                (TemplateName, templateFilePath)
            };

            foreach((string assetName, string cachedFilePath) in assets)
            {
                using var assetStream = await FileSystem.OpenAppPackageFileAsync(assetName);
                using var cachedStream = File.Create(cachedFilePath);
                await assetStream.CopyToAsync(cachedStream);
            }
        }

        // Удалить статьи с неверными ссылками и пустые категории
        private void RemoveEmptyArticlesAndCategories()
        {
            var existingArticleIds = new HashSet<int>(bundle.GetExistingArticleIds());

            foreach (var category in Categories)
            {
                var articleNamesToRemove = category.Articles
                    .Where(a => !existingArticleIds.Contains(a.Value.Id))
                    .Select(a => a.Key)
                    .ToArray();

                foreach (string name in articleNamesToRemove)
                {
                    category.Articles.Remove(name);
                }
            }

            Categories.RemoveAll(cat => cat.ArticlesCount == 0);
        }

        // Строит алфавитный список категорий
        private void BuildAlphabetCategoryIndex()
        {
            var indexList = Categories
                .GroupBy(c => char.ToUpper(c.Name[0]))
                .Select(g => new CategoryIndexItem(g.Key, [.. g]));

            var nonLetterCategories = indexList
                .TakeWhile(it => !char.IsLetter(it.Letter))
                .SelectMany(it => it.Items);

            CategoryIndex = new[] { new CategoryIndexItem('#', [.. nonLetterCategories]) }
                .Concat(indexList.SkipWhile(it => !char.IsLetter(it.Letter)))
                .ToList();
        }

        /// <summary>
        /// Получить представление статьи в HTML
        /// </summary>
        /// <param name="article">Статья, разметку которой получить</param>
        /// <returns>Информация о статье для вывода</returns>
        public async Task<RenderedArticleInfo> GetRenderedArticle(Article article)
        {
            WikiDocument document = await cache.GetAsync(article);

            if (document == null)
            {
                using var articleStream = bundle.GetArticleStream(article.Id);
                using var articleReader = new StreamReader(articleStream);

                var parser = new WikiParser(await articleReader.ReadToEndAsync());
                parser.Parse();
                document = parser.ParsedDocument;
                document.Title = article.Name;

                await cache.PutAsync(article, parser.ParsedDocument);
            }

            using var layoutWriter = new StringWriter();
            var htmlGenerator = new HtmlGenerator(document, templateFilePath);
            htmlGenerator.Generate(layoutWriter);

            return new RenderedArticleInfo
            {
                Title = article.Name,
                ParagraphNames = document.Paragraphs,
                RenderedLayout = layoutWriter.ToString()
            };
        }

        /// <summary>
        /// Получить информацию о статье по ее имени
        /// </summary>
        /// <param name="articleName">Имя статьи</param>
        /// <returns>Информация о статье</returns>
        public Article GetArticleByName(string articleName)
        {
            int? articleId = toc.GetArticleIdByName(articleName);

            return articleId.HasValue
                ? new Article { Id = articleId.Value, Name = articleName }
                : null;
        }

        /// <summary>
        /// Выполнить поиск статей по части имени
        /// </summary>
        /// <param name="namePart">Часть тмени статьи для поиска</param>
        /// <returns>Список найденных статей</returns>
        public IList<Article> SearchArticlesByName(string namePart)
        {
            string query = namePart.Trim().ToLower();

            if(query.Length > 2)
            {
                return toc.Categories.SelectMany(c => c.Articles)
                    .Where(kv => kv.Value.Name.ToLower().Contains(query))
                    .Select(kv => kv.Value)
                    .DistinctBy(a => a.Id)
                    .OrderBy(a => a.Name)
                    .ToList();
            }
            else
            {
                return Array.Empty<Article>();
            }
        }
    }
}
