using System;
using System.Collections.Generic;

namespace LurkViewer.Models
{
    /// <summary>
    /// Информация о странице, готовой к показу
    /// </summary>
    internal class RenderedArticleInfo
    {
        /// <summary>
        /// Заголовок статьи
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Имена разделов
        /// </summary>
        public string[] ParagraphNames { get; set; }

        /// <summary>
        /// Разметка для выдачи
        /// </summary>
        public string RenderedLayout { get; set; }
    }
}
