using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace LurkViewer.Services
{
    /// <summary>
    /// Вспомогательные операции для отображения содержания статьи
    /// </summary>
    internal static class ArticleContentsHelper
    {
        /// <summary>
        /// Инициализироывть панель содержимого
        /// </summary>
        /// <param name="contents">Элементы содержимого</param>
        /// <param name="handler">Обработчик выбора элемента</param>
        public static void InitializeContentsPane(string[] contents, EventHandler<int> handler)
        {
            var shell = Shell.Current as AppShell;

            shell.PopulateArticleContents(contents);
            shell.ParagraphSelected += handler;
            shell.FlyoutBehavior = FlyoutBehavior.Flyout;
        }

        /// <summary>
        /// Убрать панель содержимого
        /// </summary>
        public static void DestroyContentsPane()
        {
            var shell = Shell.Current as AppShell;

            shell.RemoveHandlers();
            shell.FlyoutBehavior = FlyoutBehavior.Disabled;
        }
    }
}
