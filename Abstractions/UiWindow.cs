using Wpf.Ui.Controls;

namespace KGallery.Abstractions
{
    public class UiWindow : FluentWindow
    {
        public UiWindow(ViewModel viewModel) : base()
        {
            DataContext = viewModel;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        protected virtual void OnUnloaded(object sender, System.Windows.RoutedEventArgs e) { }
        protected virtual void OnLoaded(object sender, System.Windows.RoutedEventArgs e) { }
    }
}
