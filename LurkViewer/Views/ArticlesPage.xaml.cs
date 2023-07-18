using System.Collections.Generic;
using Microsoft.Maui.Controls;
using WikiReader.Toc;

namespace LurkViewer.Views;

/// <summary>
/// Странца статей категории
/// </summary>
public partial class ArticlesPage : ContentPage
{
	private Article selected;

	/// <summary>
	/// Выбранная статья
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
	/// Все статьи в категории
	/// </summary>
	public IList<Article> AllArticles { get; }

	public ArticlesPage(ArticleCategory category)
	{
		AllArticles = category.Articles;

		InitializeComponent();

		Title = $"{category.Name} - статьи:";
		BindingContext = this;
	}

	private async void GoToArticleViewer()
	{
		await Navigation.PushAsync(new ArticleViewPage(SelectedArticle));
	}
}