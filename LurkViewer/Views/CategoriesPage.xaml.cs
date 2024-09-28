using System;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using WikiReader.Toc;
using LurkViewer.Services;
using LurkViewer.Models;
using System.Linq;

namespace LurkViewer.Views;

/// <summary>
/// Страница категорий
/// </summary>
public partial class CategoriesPage : ContentPage
{
    private readonly LurkLibrary library;

    private ArticleCategory selected;

    /// <summary>
    /// Идет загрузка
    /// </summary>
    public bool IsLoading { get; private set; }

    /// <summary>
    /// Все имеющиеся категории
    /// </summary>
    public IList<ArticleCategory> AllCategories { get; private set; }

    /// <summary>
    /// Алфавитный указатель
    /// </summary>
    public IList<AlphabetIndexItem<ArticleCategory>> Index { get; private set; }

    /// <summary>
    /// Команда включения и выключения фильтрации
    /// </summary>
    public Command ToggleFilterCommand { get; }

    /// <summary>
    /// Команда фильтрации списка по алфавиту
    /// </summary>
    public Command AlphabetFilterCommand { get; }

    /// <summary>
    /// Буква алфавита, по которой ведется фильтрация названий
    /// </summary>
    public char? FilterLetter { get; private set; }

    /// <summary>
    /// Активен ли фильтр
    /// </summary>
    public bool IsFilterEnabled { get; private set; }

    /// <summary>
    /// В режиме отображения указателя
    /// </summary>
    public bool IsInIndexView => IsFilterEnabled && (FilterLetter == null);

    /// <summary>
    /// Иконка кнопки фильтрации
    /// </summary>
    public ImageSource FilterIcon => new FileImageSource
    {
        File = IsInIndexView ? "no_filter.png" : "filter.png"
    };

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

        ToggleFilterCommand = new Command(ToggleFilter);
        AlphabetFilterCommand = new Command<char>(FilterCategoriesAlphabetically);

        InitializeComponent();

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (AllCategories == null)
        {
            await library.Initialize();
            _ = new FavoritesManager();
            AllCategories = library.Categories;
            Index = library.CategoryIndex;
        }

        IsLoading = false;
        OnPropertyChanged(nameof(IsLoading));
        OnPropertyChanged(nameof(AllCategories));
        OnPropertyChanged(nameof(Index));
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        SelectedCategory = null;
        OnPropertyChanged(nameof(SelectedCategory));

        ArticleContentsHelper.DestroyContentsPane();

        base.OnNavigatedTo(args);
    }

    // Включить/выключить фильтрацию
    private void ToggleFilter(object _)
    {
        IsFilterEnabled ^= true;
        FilterLetter = null;

        if(!IsFilterEnabled)
        {
            AllCategories = library.Categories;
            OnPropertyChanged(nameof(AllCategories));
        }

        OnPropertyChanged(nameof(FilterLetter));
        OnPropertyChanged(nameof(IsFilterEnabled));
        OnPropertyChanged(nameof(IsInIndexView));
        OnPropertyChanged(nameof(FilterIcon));
    }

    // Фильтровать категории по алфавиту
    private void FilterCategoriesAlphabetically(char letter)
    {
        FilterLetter = letter;
        AllCategories = Index.First(it => it.Letter == FilterLetter).Items;

        OnPropertyChanged(nameof(FilterLetter));
        OnPropertyChanged(nameof(IsInIndexView));
        OnPropertyChanged(nameof(AllCategories));
    }

    private async void GoToSelectedCategory()
    {
        await Navigation.PushAsync(new ArticlesPage(SelectedCategory));
    }
}