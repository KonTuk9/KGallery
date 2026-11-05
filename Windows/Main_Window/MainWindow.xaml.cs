using KGallery.Abstractions;
using KGallery.Pages.Categories_Page;
using KGallery.Services;
using System.Windows;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace KGallery.Windows.Main_Window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : UiWindow
    {
        public MainWindow(MainViewModel viewModel, INavigationService navigationService, ISnackbarService snackbarService, GlobalInput globalInput, OpenDialogFactory dialogPool, IContentDialogService contentDialogService) : base(viewModel)
        {
            SystemThemeWatcher.Watch(this);

            InitializeComponent();

            globalInput.SetInputHandler(this);

            navigationService.SetNavigationControl(navigationView);
            snackbarService.SetSnackbarPresenter(snackbarPresenter);

            dialogPool.SetContentPresenter(contentPresenter);
            contentDialogService.SetDialogHost(contentDialogHost);

            navigationView.Navigated += OnNavigated;
        }

        private UiPage _previousPage;
        private void OnNavigated(NavigationView sender, NavigatedEventArgs args)
        {
            if (args.Page is UiPage page)
            {
                InputBindings.AddRange(page.InputBindings);

                if (_previousPage != null)
                {
                    foreach (InputBinding binding in _previousPage.InputBindings)
                    {
                        InputBindings.Remove(binding);
                    }
                }
                _previousPage = page;
            }
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            navigationView.Navigate(typeof(CategoriesPage));
        }
    }
}
