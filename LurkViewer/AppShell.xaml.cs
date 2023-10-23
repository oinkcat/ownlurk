using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using LurkViewer.Views;
using WikiReader.Dom;
using System.Linq;

namespace LurkViewer;

public partial class AppShell : Shell
{
    /// <summary>
    /// Выбран абзац
    /// </summary>
    public event EventHandler<int> ParagraphSelected;

    /// <summary>
    /// Текущая отображаемая страница
    /// </summary>
    public WikiDocument CurrentDocument { get; set; }

    public AppShell()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Заполнить элементы содержимого статьи
    /// </summary>
    /// <param name="paragraphNames">Названия заголовков</param>
    public void PopulateArticleContents(IList<string> paragraphNames)
    {
        // Удалить старые элементы
        foreach(var menuItem in Items.OfType<MenuItem>())
        {
            Items.Remove(menuItem);
        }

        // Добавить новые элементы содержимого
        int paragraphIdx = 0;

        foreach(string menuText in paragraphNames)
        {
            Items.Add(new MenuItem
            {
                Text = menuText,
                Command = new Command((pIdx) =>
                {
                    ParagraphSelected?.Invoke(this, (int)pIdx);
                }),
                CommandParameter = paragraphIdx++
            });
        }
    }

    /// <summary>
    /// Удалить обработчики выбора
    /// </summary>
    public void RemoveHandlers()
    {
        if(ParagraphSelected == null) { return; }

        foreach(var d in ParagraphSelected.GetInvocationList())
        {
            ParagraphSelected -= (EventHandler<int>)d;
        }
    }
}