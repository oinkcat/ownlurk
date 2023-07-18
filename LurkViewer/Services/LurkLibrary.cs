﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using WikiReader;
using WikiReader.Bundle;
using WikiReader.Toc;

namespace LurkViewer.Services
{
    /// <summary>
    /// Собрание статей Lurkmore
    /// </summary>
    public class LurkLibrary
    {
        private const string BundleName = "lurk_data.zip";

        private const string TemplateName = "template.html";

        private readonly string bundleFilePath;

        private readonly string templateFilePath;

        private ContentBundle bundle;

        private TableOfContents toc;

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
            bundleFilePath = Path.Combine(FileSystem.AppDataDirectory, BundleName);
            templateFilePath = Path.Combine(FileSystem.AppDataDirectory, TemplateName);
        }

        /// <summary>
        /// Инициализировать компоненты
        /// </summary>
        public async Task Initialize()
        {
            if(bundle != null || toc != null) { return; }

            await CacheAssets();

            bundle = new ContentBundle(bundleFilePath);
            toc = new TableOfContents();
            await toc.LoadFromStream(bundle.GetTocStream());
        }

        private async Task CacheAssets()
        {
            (string, string)[] assets = {
                (BundleName, bundleFilePath),
                (TemplateName, templateFilePath)
            };

            foreach((string assetName, string cachedFilePath) in assets)
            {
                if (!File.Exists(cachedFilePath))
                {
                    using var assetStream = await FileSystem.OpenAppPackageFileAsync(assetName);
                    using var cachedStream = File.Create(cachedFilePath);
                    await assetStream.CopyToAsync(cachedStream);
                }
            }
        }

        /// <summary>
        /// Получить представление статьи в HTML
        /// </summary>
        /// <param name="article">Статья, разметку которой получить</param>
        /// <returns>Текст разметки статьи</returns>
        public async Task<string> GetRenderedArticle(Article article)
        {
            using var articleStream = bundle.GetArticleStream(article.Id);
            using var articleReader = new StreamReader(articleStream);

            var parser = new WikiParser(await articleReader.ReadToEndAsync());
            parser.Parse();
            parser.ParsedDocument.Title = article.Name;

            using var layoutWriter = new StringWriter();
            var htmlGenerator = new HtmlGenerator(parser.ParsedDocument, templateFilePath);
            htmlGenerator.Generate(layoutWriter);

            return layoutWriter.ToString();
        }
    }
}