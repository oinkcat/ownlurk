using System;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using WikiReader.Toc;
using LurkViewer.Services;

namespace LurkViewer.Views;

/// <summary>
/// �������� ��������� ������ ������
/// </summary>
public partial class ArticleViewPage : ContentPage
{
	private Article viewingArticle;

	private bool isLoaded;

	private readonly FavoritesManager favManager;

	private readonly ImageSource[] favIcons =
	{
		ImageSource.FromFile("star_e.png"),
		ImageSource.FromFile("star_f.png")
	};

	/// <summary>
	/// ����������� ��� ������ ���������� � ���������
	/// </summary>
	public ImageSource FavIcon => favManager.CheckIsFavorited(viewingArticle)
		? favIcons[1]
		: favIcons[0];

	/// <summary>
	/// �������� � ���������/������ �� ����������
	/// </summary>
	public Command FavToggleCommand { get; }

	public ArticleViewPage(Article article)
    {
        viewingArticle = article;
        favManager = new FavoritesManager();

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
            await DisplayRenderedArticle();
			isLoaded = true;
		}

		Shell.Current.FlyoutBehavior = FlyoutBehavior.Flyout;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Shell.Current.FlyoutBehavior = FlyoutBehavior.Disabled;
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);
        ArticleContentsHelper.DestroyContentsPane();
    }

    private async Task DisplayRenderedArticle()
    {
		// ���������� �� ���������
        OnPropertyChanged(nameof(FavIcon));

        // ���������� � ��������� ��������
        Title = $"{viewingArticle.Name} - Loading...";

        var infoToRender = await LurkLibrary.Instance.GetRenderedArticle(viewingArticle);
        ArticleBrowser.Source = new HtmlWebViewSource { Html = infoToRender.RenderedLayout };

        Title = viewingArticle.Name;

		// �������� ������ ����������
		var selectHandler = new EventHandler<int>(OnParagraphSelected);
		ArticleContentsHelper.InitializeContentsPane(infoToRender.ParagraphNames, selectHandler);
    }

	// �� ������ ����������� ��� ������ ������
	private async void OnParagraphSelected(object sender, int idx)
	{
		await ArticleBrowser.EvaluateJavaScriptAsync($"scrollToChapter({idx})");
	}

    private async void ArticleBrowser_Navigating(object sender, WebNavigatingEventArgs e)
    {
		const string InlineDataUriScheme = "data";

		var uriInfo = new Uri(e.Url);

		// ������� ���������� ������ - ����� 127.0.0.1
		if(uriInfo.IsLoopback)
		{
			string articleName = Uri.UnescapeDataString(uriInfo.LocalPath).TrimStart('/');
			var articleToJump = LurkLibrary.Instance.GetArticleByName(articleName);

			if(articleToJump != null)
			{
                viewingArticle = articleToJump;
                await DisplayRenderedArticle();
            }
		}

		if(uriInfo.Scheme != InlineDataUriScheme)
        {
            e.Cancel = true;
        }
    }
}