using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WikiReader.Dom
{
    /// <summary>
    /// Элемент списка
    /// </summary>
    public class WikiListElement : WikiElement
    {
        /// <summary>
        /// Элементы списка
        /// </summary>
        public List<List<WikiElement>> ContentItems { get; } = new();

        /// <summary>
        /// Является нумерованным
        /// </summary>
        public bool IsNumbered { get; set; }

        /// <summary>
        /// Добавить новый элемент списка
        /// </summary>
        /// <returns>Новый элемент списка</returns>
        public List<WikiElement> AddElement()
        {
            var listElementContents = new List<WikiElement>();
            ContentItems.Add(listElementContents);

            return listElementContents;
        }

        public override void AcceptHtmlGenerationVisitor(HtmlGenerationVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}
