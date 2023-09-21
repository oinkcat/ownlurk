using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using WikiReader.Toc;
using LurkViewer.Services;

namespace LurkViewer.Views;

/// <summary>
/// Страница просмотра текста статьи
/// </summary>
public partial class ArticleViewPage : ContentPage
{
	private readonly Article viewingArticle;

	private bool isLoaded;

	public ArticleViewPage(Article article)
	{
		viewingArticle = article;
		IsBusy = true;

		InitializeComponent();
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();

		if(!isLoaded)
		{
			await DisplayRenderedArticle(viewingArticle);
			isLoaded = true;
		}

		IsBusy = false;
    }

	private async Task DisplayRenderedArticle(Article articleToDisplay)
    {
        Title = $"{articleToDisplay.Name} - Loading...";

        string layout = await LurkLibrary.Instance.GetRenderedArticle(articleToDisplay);
        ArticleBrowser.Source = new HtmlWebViewSource { Html = layout };

		Title = articleToDisplay.Name;
    }

    private async void ArticleBrowser_Navigating(object sender, WebNavigatingEventArgs e)
    {
		const string InlineDataUriScheme = "data";

		var uriInfo = new Uri(e.Url);

		// Признак внутренней ссылки - домен 127.0.0.1
		if(uriInfo.IsLoopback)
		{
			string articleName = Uri.UnescapeDataString(uriInfo.LocalPath).TrimStart('/');
			var articleToJump = LurkLibrary.Instance.GetArticleByName(articleName);

			if(articleToJump != null)
			{
				await DisplayRenderedArticle(articleToJump);
			}
		}

		if(uriInfo.Scheme != InlineDataUriScheme)
        {
            e.Cancel = true;
        }
    }
}