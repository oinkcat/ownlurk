using Microsoft.Maui.Controls;
using WikiReader.Toc;
using LurkViewer.Services;

namespace LurkViewer.Views;

public partial class ArticleViewPage : ContentPage
{
	private readonly Article viewingArticle;

	private bool isLoaded;

	public ArticleViewPage(Article article)
	{
		viewingArticle = article;
		IsBusy = true;

		InitializeComponent();
		Title = article.Name;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();

		if(!isLoaded)
		{
			string layout = await LurkLibrary.Instance.GetRenderedArticle(viewingArticle);
			articleBrowser.Source = new HtmlWebViewSource { Html = layout };
			isLoaded = true;
		}

		IsBusy = false;
    }
}