using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using WikiReader.Dom;
using WikiReader.Toc;

namespace LurkViewer.Services
{
    /// <summary>
    /// Кэш документов статей
    /// </summary>
    internal class ArticleDocumentsCache
    {
        private const int NumberItemsInCache = 100;

        private const string DirName = "ParsedArticles";

        private readonly string cacheDir = Path.Combine(FileSystem.CacheDirectory, DirName);

        private static readonly JsonPolymorphismOptions polymorphismOptions = new()
        {
            DerivedTypes =
            {
                new JsonDerivedType(typeof(WikiEolElement), "eol"),
                new JsonDerivedType(typeof(WikiFormattedElement), "fmt"),
                new JsonDerivedType(typeof(WikiHeaderElement), "head"),
                new JsonDerivedType(typeof(WikiLinkElement), "link"),
                new JsonDerivedType(typeof(WikiListElement), "list"),
                new JsonDerivedType(typeof(WikiParagraphElement), "par"),
                new JsonDerivedType(typeof(WikiTemplateElement), "tmpl"),
                new JsonDerivedType(typeof(WikiTextElement), "text")
            }
        };

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            WriteIndented = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver
            {
                Modifiers =
                {
                    (info) => 
                    {
                        if(info.Type != typeof(WikiElement)) { return; }

                        info.PolymorphismOptions = polymorphismOptions;
                    }
                }
            }
        };

        /// <summary>
        /// Поместить разметку документа в кэш
        /// </summary>
        /// <param name="article">Информация о статье</param>
        /// <param name="pageDom">Разметка статьи</param>
        public async Task PutAsync(Article article, WikiDocument pageDom)
        {
            if(!Directory.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            using var writer = new StreamWriter(GetCacheFileName(article));

            await writer.WriteLineAsync($"V: {WikiDocument.Version}");
            string domJson = JsonSerializer.Serialize(pageDom, jsonOptions);
            await writer.WriteAsync(domJson);

            TrimOldestItems();
        }

        // Получить имя кэшированного файла
        private string GetCacheFileName(Article article)
        {
            return Path.Combine(cacheDir, $"{article.Id}.dat");
        }

        // Удалить самые старые элементы кэша
        private void TrimOldestItems()
        {
            var oldestItems = new DirectoryInfo(cacheDir)
                .EnumerateFiles()
                .OrderByDescending(fi => fi.LastAccessTime)
                .Skip(NumberItemsInCache)
                .ToList();

            foreach(var itemToDelete in oldestItems)
            {
                itemToDelete.Delete();
            }
        }

        /// <summary>
        /// Получить разметку статьи из кэша
        /// </summary>
        /// <param name="article">Информация о статье</param>
        /// <returns>Разметка статьи из кэша</returns>
        public async Task<WikiDocument> GetAsync(Article article)
        {
            string fileName = GetCacheFileName(article);
            if (!File.Exists(fileName)) { return null; }

            using var reader = new StreamReader(fileName);
            var versionLineParts = (await reader.ReadLineAsync()).Split();

            if (versionLineParts.Length > 1 && int.TryParse(versionLineParts[1], out var version))
            {
                if (version == WikiDocument.Version)
                {
                    new FileInfo(fileName).LastAccessTime = DateTime.Now;

                    string domJson = await reader.ReadToEndAsync();
                    return JsonSerializer.Deserialize<WikiDocument>(domJson, jsonOptions);
                }
                else
                {
                    reader.Close();
                    Delete(article);
                }
            }
            else
            {
                reader.Close();
                Delete(article);
            }

            return null;
        }

        /// <summary>
        /// Удалить элемент из кэша
        /// </summary>
        /// <param name="article"></param>
        public void Delete(Article article)
        {
            string cachedFilePath = GetCacheFileName(article);

            if(File.Exists(cachedFilePath))
            {
                File.Delete(cachedFilePath);
            }
        }
    }
}
