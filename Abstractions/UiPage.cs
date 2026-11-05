using System.Windows;
using System.Windows.Controls;

namespace KGallery.Abstractions
{
    public class UiPage : Page
    {
        public UiPage(ViewModel viewModel)
        {
            DataContext = viewModel;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected virtual void OnUnloaded(object sender, RoutedEventArgs e) { }
        protected virtual void OnLoaded(object sender, RoutedEventArgs e) { }
    }
}
