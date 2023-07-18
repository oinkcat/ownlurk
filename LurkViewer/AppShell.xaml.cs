using Microsoft.Maui.Controls;
using LurkViewer.Views;

namespace LurkViewer;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute("Home", typeof(CategoriesPage));
    }
}