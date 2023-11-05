using System;
using System.Collections.Generic;
using LurkViewer.Services;
using Microsoft.Maui.Controls;
using WikiReader.Toc;

namespace LurkViewer.Views;

/// <summary>
/// �������� ������ ����������
/// </summary>
public partial class FavoritesPage : ContentPage
{
	private readonly FavoritesManager favManager = new();

	private Article selectedArticle;

	/// <summary>
	/// ��� ������ ����������
	/// </summary>
	public IList<Article> AllFavorites => favManager.Favorites;

	/// <summary>
	/// ������� ���������� ������ �� ������ ����������
	/// </summary>
	public Command UnFavArticleCommand { get; }

	/// <summary>
	/// ��������� ������
	/// </summary>
	public Article SelectedArticle
	{
		get => selectedArticle;

		set
		{
			selectedArticle = value;

			if(selectedArticle != null)
            {
                OpenSelectedArticle();
            }
		}
	}

	/// <summary>
	/// ������ ���������� ����
	/// </summary>
	public bool IsEmpty => AllFavorites.Count <= 0;

    public FavoritesPage()
	{
		UnFavArticleCommand = new Command((articleParam) =>
		{
			favManager.RemoveFromFavorites(articleParam as Article);
			OnPropertyChanged(nameof(AllFavorites));
		});

		InitializeComponent();
		BindingContext = this;
	}

    private async void OpenSelectedArticle()
    {
		await Navigation.PushAsync(new ArticleViewPage(selectedArticle));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

		OnPropertyChanged(nameof(AllFavorites));
		OnPropertyChanged(nameof(IsEmpty));

		SelectedArticle = null;
		OnPropertyChanged(nameof(SelectedArticle));
    }
}