using System;
using System.Collections.Generic;
using LurkViewer.Services;
using Microsoft.Maui.Controls;
using WikiReader.Toc;

namespace LurkViewer.Views;

/// <summary>
/// ������� ������ ���������
/// </summary>
public partial class ArticlesPage : ContentPage
{
	private Article selected;

	/// <summary>
	/// ��������� ������
	/// </summary>
	public Article SelectedArticle
	{
		get => selected;
		set
		{
			selected = value;

			if(selected != null)
			{
				GoToArticleViewer();
			}
		}
	}

	/// <summary>
	/// ��� ������ � ���������
	/// </summary>
	public IList<Article> AllArticles { get; }

	public ArticlesPage(ArticleCategory category)
	{
		AllArticles = category.Articles.Values;

		InitializeComponent();

		Title = $"{category.Name} - ������:";
		BindingContext = this;
	}

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        SelectedArticle = null;
		OnPropertyChanged(nameof(SelectedArticle));

        ArticleContentsHelper.DestroyContentsPane();

        base.OnNavigatedTo(args);
    }

    private async void GoToArticleViewer()
	{
        await Navigation.PushAsync(new ArticleViewPage(SelectedArticle));
    }
}