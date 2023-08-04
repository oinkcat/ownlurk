using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using WikiReader.Toc;
using LurkViewer.Services;

namespace LurkViewer.Views;

/// <summary>
/// Страница категорий
/// </summary>
public partial class CategoriesPage : ContentPage
{
    private readonly LurkLibrary library;

    private ArticleCategory selected;

    public bool IsLoading { get; private set; }

    /// <summary>
    /// Все имеющиеся категории
    /// </summary>
    public IList<ArticleCategory> AllCategories { get; private set; }

    /// <summary>
    /// Выбранная категория
    /// </summary>
    public ArticleCategory SelectedCategory
    {
        get => selected;
        set
        {
            selected = value;

            if(selected != null)
            {
                GoToSelectedCategory();
            }
        }
    }

    public CategoriesPage()
    {
        library = LurkLibrary.Instance;
        IsLoading = true;

        InitializeComponent();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (AllCategories == null)
        {
            await library.Initialize();
            AllCategories = library.Categories;
        }

        IsLoading = false;
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(AllCategories));
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        SelectedCategory = null;
        OnPropertyChanged(nameof(SelectedCategory));

        base.OnNavigatedTo(args);
    }

    private async void GoToSelectedCategory()
    {
        await Navigation.PushAsync(new ArticlesPage(SelectedCategory));
    }
}