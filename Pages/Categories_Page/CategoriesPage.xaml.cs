using KGallery.Abstractions;
using System.Windows;
using System.Windows.Input;

namespace KGallery.Pages.Categories_Page
{
    /// <summary>
    /// Interaction logic for CategoriesPage.xaml
    /// </summary>
    public partial class CategoriesPage : UiPage
    {
        public CategoriesPage(CategoriesPageViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }

        private void CategoryItemClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                ((CategoriesPageViewModel)DataContext).NavigateToImagesCommand.Execute(element.DataContext);
            }
        }
    }
}
