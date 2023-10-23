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
    }

	private async Task DisplayRenderedArticle(Article articleToDisplay)
    {
		// Распарсить и загрузить страницу
        Title = $"{articleToDisplay.Name} - Loading...";

        var infoToRender = await LurkLibrary.Instance.GetRenderedArticle(articleToDisplay);
        ArticleBrowser.Source = new HtmlWebViewSource { Html = infoToRender.RenderedLayout };

        Title = articleToDisplay.Name;

		// Показать панель содержания
		var selectHandler = new EventHandler<int>(OnParagraphSelected);
		ArticleContentsHelper.InitializeContentsPane(infoToRender.ParagraphNames, selectHandler);
    }

	// На панели содержимого был выбран раздел
	private async void OnParagraphSelected(object sender, int idx)
	{
		const string BlankUrl = "about:blank";

		string anchorUrl = $"{BlankUrl}#p_{idx}";
		await ArticleBrowser.EvaluateJavaScriptAsync($"location.href = \"{anchorUrl}\"");
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
                Console.WriteLine("TEST 2");
            }
		}

		if(uriInfo.Scheme != InlineDataUriScheme)
        {
            e.Cancel = true;
        }
    }
}