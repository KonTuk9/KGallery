using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using KGallery.Abstractions;
using KGallery.Images;
using KGallery.Models;
using KGallery.Services;
using KGallery.ViewModels;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace KGallery.Controls
{
    /// <summary>
    /// Interaction logic for EditImageCategoriesDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class EditImageCategoriesDialog : UserControl, IOpenDialog<ImageViewModel>
    {
        public ReadOnlyObservableCollection<SelectableCategoryViewModel> Categories => _categories;
        private ReadOnlyObservableCollection<SelectableCategoryViewModel> _categories;
        private readonly IObservableCache<SelectableCategoryViewModel, int> _unfilteredCategories;

        private readonly GlobalInput _input;
        private readonly ImagesService _imageService;
        private readonly IContentDialogService _contentDialogService;
        private ImageViewModel _image;

        public EditImageCategoriesDialog(CategoriesService categoriesService, GlobalInput input, ImagesService imagesService, IContentDialogService contentDialogService)
        {
            _input = input;
            _contentDialogService = contentDialogService;
            _imageService = imagesService;
            _unfilteredCategories = categoriesService.Categories.Connect().Transform(f => new SelectableCategoryViewModel(f)).AsObservableCache();

            _unfilteredCategories.Connect().Filter(GetFilter()).ObserveOn(SynchronizationContext.Current)
            .SortAndBind(out _categories, SortExpressionComparer<SelectableCategoryViewModel>.Descending(f => f.ImagesCount)).Subscribe();
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        [ObservableProperty]
        private string _searchText;

        private IObservable<Func<SelectableCategoryViewModel, bool>> GetFilter()
        {
            return this.WhenPropertyChanged(x => x.SearchText).Select(_ =>
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    return f => true;
                }

                return (Func<SelectableCategoryViewModel, bool>)(f => f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            });
        }

        public void Open(ImageViewModel image)
        {

            _image = image;

            this.image.Source = Imaging.LoadImage(image.ImagePath);

            List<Category> categories = null;

            Task.Run(async () =>
            {
                categories = await _imageService.GetCategoriesAsync(image.Id);
                foreach (SelectableCategoryViewModel item in _unfilteredCategories.Items)
                {
                    item.IsChecked = categories.Any(x => x.Id == item.Id);
                }
            }).ContinueWith(t =>
            {
                Visibility = Visibility.Visible;
                _input.Subscribe(OnDialogKeyDown);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        [RelayCommand]
        private void Close()
        {
            _input.Unsubscribe(OnDialogKeyDown);
            Visibility = Visibility.Collapsed;

            Task.Run(() =>
            {
                foreach (SelectableCategoryViewModel item in _unfilteredCategories.Items)
                {
                    item.IsChecked = false;
                }
            });
        }

        [RelayCommand]
        private async Task Submit()
        {
            await _imageService.EditCategoriesAsync(_image.Id, _unfilteredCategories.Items.Where(f => f.IsChecked).Select(f => f.Id));
            Close();
        }

        [RelayCommand]
        private async Task Delete()
        {
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "Delete image",
                Content = "Are you sure to delete this image?",
                CloseButtonText = "Cancel",
                PrimaryButtonText = "Delete",
            });
            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            Close();
            await _imageService.DeleteAsync(_image.Id);
        }

        private async void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await Submit();
            }
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
