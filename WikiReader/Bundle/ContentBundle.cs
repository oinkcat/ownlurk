using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;

namespace WikiReader.Bundle;

/// <summary>
/// Архив контента
/// </summary>
public class ContentBundle : IDisposable
{
    private const string TocEntryName = "toc.csv";

    private const string ContentDirName = "Pages";

    private readonly ZipArchive bundleArchive;

    private bool disposedValue;

    public ContentBundle(string fileName)
    {
        bundleArchive = ZipFile.Open(fileName, ZipArchiveMode.Read);
        ValidateBundle();
    }

    private void ValidateBundle()
    {
        bool tocValid = bundleArchive.GetEntry(TocEntryName)?.Length > 0;
        bool hasPagesDir = bundleArchive.Entries.Any(e => e.FullName.Contains(ContentDirName));

        if(!tocValid || !hasPagesDir)
        {
            throw new InvalidDataException("Content bundle format invalid!");
        }
    }

    /// <summary>
    /// Получить идентификаторы имеющихся статей
    /// </summary>
    /// <returns>Идентификаторы статей в архиве</returns>
    public IEnumerable<int> GetExistingArticleIds()
    {
        return bundleArchive.Entries
            .Where(e => e.FullName.StartsWith(ContentDirName) && (e.Length > 0))
            .Select(e => Path.GetFileNameWithoutExtension(e.FullName))
            .Select(name => int.Parse(name));
    }

    /// <summary>
    /// Получить поток данных таблицы содержимого
    /// </summary>
    /// <returns>Поток данных содержания</returns>
    public Stream GetTocStream()
    {
        return bundleArchive.GetEntry(TocEntryName).Open();
    }

    /// <summary>
    /// Получить поток данных текста статьи
    /// </summary>
    /// <param name="articleId">Идентификатор статьи</param>
    /// <returns>Поток данных текста статьи</returns>
    public Stream GetArticleStream(int articleId)
    {
        string articleEntryName = $"{ContentDirName}/{articleId}.txt";
        return bundleArchive.GetEntry(articleEntryName).Open();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                bundleArchive.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
