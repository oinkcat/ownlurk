using Microsoft.Maui.Controls;

namespace LurkViewer;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new AppShell();
    }
}