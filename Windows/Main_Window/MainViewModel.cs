using KGallery.Abstractions;
using KGallery.Pages.About_Page;
using KGallery.Pages.Categories_Page;
using KGallery.Pages.Images_Page;
using KGallery.Pages.Settings_Page;
using System.Windows;
using Wpf.Ui.Controls;

namespace KGallery.Windows.Main_Window
{
    public partial class MainViewModel : ViewModel
    {
        public FrameworkElement[] Menu { get; } =
        {
            new NavigationViewItem("Images", SymbolRegular.Image16, typeof(ImagesPage)),
            new NavigationViewItem("Categories", SymbolRegular.ArchiveMultiple16, typeof(CategoriesPage)),
        };

        public FrameworkElement[] Footer { get; } =
        {
            new NavigationViewItem("Settings", SymbolRegular.Settings16, typeof(SettingsPage)),
            new NavigationViewItem("About", SymbolRegular.QuestionCircle16, typeof(AboutPage))
        };
    }
}
