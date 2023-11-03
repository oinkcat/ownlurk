using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using WikiReader.Toc;

namespace LurkViewer.Services
{
    /// <summary>
    /// Управляет списком избранных статей
    /// </summary>
    internal class FavoritesManager
    {
        private const string FavoritesFileName = "favs.txt";

        private class ArticlesComparer : IComparer<Article>
        {
            public int Compare(Article x, Article y) => x.Name.CompareTo(y.Name);
        }

        private readonly static SortedSet<Article> favorites = new(new ArticlesComparer());

        private string FavoritesFilePath
        {
            get => Path.Combine(FileSystem.AppDataDirectory, FavoritesFileName);
        }

        /// <summary>
        /// Все избранные статьи
        /// </summary>
        public IList<Article> Favorites => favorites.ToImmutableList();

        public FavoritesManager()
        {
            Task.Run(LoadFavorites).Wait();
        }

        private async Task LoadFavorites()
        {
            if(File.Exists(FavoritesFilePath))
            {
                var loadedFavNames = await File.ReadAllLinesAsync(FavoritesFilePath);

                foreach(string name in loadedFavNames)
                {
                    var article = LurkLibrary.Instance.GetArticleByName(name);

                    if(article != null)
                    {
                        favorites.Add(article);
                    }
                }
            }
        }

        /// <summary>
        /// Добавить статью в список избранного
        /// </summary>
        /// <param name="article">Добавляемая статья</param>
        public async void AddToFavorites(Article article)
        {
            favorites.Add(article);
            await SaveFavorites();
        }

        private async Task SaveFavorites()
        {
            var favNames = favorites.Select(f => f.Name);
            await File.WriteAllLinesAsync(FavoritesFilePath, favNames);
        }

        /// <summary>
        /// Удалить стстью из списка избранных
        /// </summary>
        /// <param name="article">Удаляемая статья</param>
        public async void RemoveFromFavorites(Article article)
        {
            if(CheckIsFavorited(article))
            {
                favorites.Remove(article);
                await SaveFavorites();
            }
        }

        /// <summary>
        /// Проверить статью на добавленность в избранное
        /// </summary>
        /// <param name="articleToCheck">Проверяемая статья</param>
        /// <returns>Статья принадлежит к избранным</returns>
        public bool CheckIsFavorited(Article articleToCheck)
        {
            return favorites.Contains(articleToCheck);
        }
    }
}
