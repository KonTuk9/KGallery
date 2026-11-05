using KGallery.Abstractions;
using KGallery.Controls;
using KGallery.DB;
using KGallery.Pages.About_Page;
using KGallery.Pages.Categories_Page;
using KGallery.Pages.Category_Images_Page;
using KGallery.Pages.Images_Page;
using KGallery.Pages.Settings_Page;
using KGallery.Services;
using KGallery.Text;
using KGallery.Windows.Main_Window;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using Wpf.Ui.Controls;

namespace KGallery
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static ServiceProvider ServiceProvider { get; private set; }

        protected override async void OnStartup(StartupEventArgs e)
        {
            Configure(new ServiceCollection());

            await LoadAsync();

            ServiceProvider.GetRequiredService<MainWindow>().Show();

            await ProcessBackgroundWork();
        }

        private void Configure(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>()
            .AddSingleton<MainViewModel>()
            .AddSingleton<GlobalInput>()

            .AddSingleton<ImageViewer>()
            .AddSingleton<RenameCategoryDialog>()
            .AddSingleton<EditImageCategoriesDialog>()
            .AddSingleton<CreateCategoryDialog>()
            .AddSingleton<ApplyFiltersDialog>()
            .AddSingleton<EditMultipleImagesCategoriesDialog>()
            .AddSingleton<OpenDialogFactory>()
            .AddSingleton<CategoryOverviewDialog>()
            .AddSingleton<ImageOverviewDialog>()

            .AddSingleton<IContentDialogService, ContentDialogService>()
            .AddSingleton<ISnackbarService, SnackbarService>()
            .AddSingleton<INavigationViewPageProvider, NavigationViewPageProvider>()
            .AddSingleton<INavigationService, NavigationService>()

            .AddSingleton<ImagesService>()
            .AddSingleton<CategoriesService>()
            .AddSingleton<ImageScanningService>()
            .AddSingleton<SettingsService>()

            .AddSingleton<IAsyncInitializer[]>(provider =>
            [
                provider.GetRequiredService<ImagesService>(),
                provider.GetRequiredService<CategoriesService>(),
            ])

            .AddSingleton<CategoriesPage>()
            .AddSingleton<CategoriesPageViewModel>()

            .AddSingleton<ImagesPage>()
            .AddSingleton<ImagesPageViewModel>()

            .AddSingleton<SettingsPage>()
            .AddSingleton<SettingsPageViewModel>()

            .AddSingleton<CategoryImagesPage>()
            .AddSingleton<CategoryImagesPageViewModel>()

            .AddTransient<AboutPage>()
            .AddTransient<AboutPageViewModel>();

            ServiceProvider = services.BuildServiceProvider();
        }

        private async Task LoadAsync()
        {
            using ApplicationDbContext context = new ApplicationDbContext();
            await context.Database.EnsureCreatedAsync();

            List<Task> tasks = new List<Task>();

            await ServiceProvider.GetRequiredService<SettingsService>().InitializeAsync();

            foreach (IAsyncInitializer initializer in ServiceProvider.GetRequiredService<IAsyncInitializer[]>())
            {
                tasks.Add(initializer.InitializeAsync());
            }

            await Task.WhenAll(tasks);
        }

        private async Task ProcessBackgroundWork()
        {
            ImageScanningService scanningService = ServiceProvider.GetRequiredService<ImageScanningService>();

            Task imagesExistTask = scanningService.EnsureFilesExistAsync()
            .ContinueWith(task =>
            {
                int result = task.Result;
                if (result > 0)
                {
                    string endWord = StringMethods.GetEndWord(result);
                    ServiceProvider.GetRequiredService<ISnackbarService>()
                    .Show("Deleted files", $"Automatically deleted {result} invalid image{endWord}", ControlAppearance.Caution, new SymbolIcon(SymbolRegular.Warning24, 28), TimeSpan.FromSeconds(7));
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());

            Task updateThumbnailTask = scanningService.RefreshThumbnailsAsync();

            await Task.WhenAll(imagesExistTask, updateThumbnailTask);
        }
    }
}
