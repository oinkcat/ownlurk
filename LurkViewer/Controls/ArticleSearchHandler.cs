using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls;
using WikiReader.Toc;
using LurkViewer.Services;
using LurkViewer.Views;

namespace LurkViewer.Controls;

/// <summary>
/// Выполняет поиск статей
/// </summary>
public class ArticleSearchHandler : SearchHandler
{
    protected override void OnQueryChanged(string oldValue, string newValue)
    {
        base.OnQueryChanged(oldValue, newValue);

        ItemsSource = LurkLibrary.Instance.SearchArticlesByName(newValue);
    }

    protected override void OnItemSelected(object item)
    {
        base.OnItemSelected(item);

        if(item is Article article)
        {
            Application.Current.MainPage.Navigation.PushAsync(new ArticleViewPage(article));
        }
    }
}
