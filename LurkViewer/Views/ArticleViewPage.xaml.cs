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

	private readonly FavoritesManager favManager;

	private readonly ImageSource[] favIcons =
	{
		ImageSource.FromFile("star_e.png"),
		ImageSource.FromFile("star_f.png")
	};

	/// <summary>
	/// Изображение для кнопки добавления в избранное
	/// </summary>
	public ImageSource FavIcon => favManager.CheckIsFavorited(viewingArticle)
		? favIcons[1]
		: favIcons[0];

	/// <summary>
	/// Добавить в избранное/убрать из избранного
	/// </summary>
	public Command FavToggleCommand { get; }

	public ArticleViewPage(Article article)
	{
		favManager = new FavoritesManager();
        viewingArticle = article;

		FavToggleCommand = new Command(() =>
		{
			if(favManager.CheckIsFavorited(viewingArticle))
			{
				favManager.RemoveFromFavorites(viewingArticle);
			}
			else
			{
				favManager.AddToFavorites(viewingArticle);
			}

			OnPropertyChanged(nameof(FavIcon));
		});

        InitializeComponent();
		BindingContext = this;
	}

    protected async override void OnAppearing()
    {
        base.OnAppearing();

		if(!isLoaded)
		{
            await DisplayRenderedArticle(viewingArticle);
			isLoaded = true;
		}

		Shell.Current.FlyoutBehavior = Microsoft.Maui.FlyoutBehavior.Flyout;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        ArticleContentsHelper.DestroyContentsPane();
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

		// Информация об избранном
        OnPropertyChanged(nameof(FavIcon));
    }

	// На панели содержимого был выбран раздел
	private async void OnParagraphSelected(object sender, int idx)
	{
		await ArticleBrowser.EvaluateJavaScriptAsync($"scrollToChapter({idx})");
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