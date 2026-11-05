using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using KGallery.Abstractions;
using KGallery.Services;
using KGallery.Text;
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
    /// Interaction logic for EditMultipleImagesCategoriesDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class EditMultipleImagesCategoriesDialog : UserControl, IOpenDialog<IEnumerable<ImageViewModel>>
    {
        public ReadOnlyObservableCollection<SelectableCategoryViewModel> Categories => _categories;
        private ReadOnlyObservableCollection<SelectableCategoryViewModel> _categories;
        private readonly IObservableCache<SelectableCategoryViewModel, int> _unfilteredCategories;

        private readonly GlobalInput _input;
        private readonly ImagesService _imageService;
        private readonly IContentDialogService _contentDialogService;
        private IEnumerable<ImageViewModel> _images;

        public EditMultipleImagesCategoriesDialog(GlobalInput input, ImagesService imagesService, IContentDialogService contentDialogService, CategoriesService categoriesService)
        {
            _contentDialogService = contentDialogService;
            _imageService = imagesService;
            _input = input;

            _unfilteredCategories = categoriesService.Categories.Connect().Transform(f => new SelectableCategoryViewModel(f)).AsObservableCache();
            _unfilteredCategories.Connect().Filter(GetFilter()).ObserveOn(SynchronizationContext.Current)
            .SortAndBind(out _categories, SortExpressionComparer<SelectableCategoryViewModel>.Descending(f => f.ImagesCount)).Subscribe();

            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

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

        [ObservableProperty]
        private string _searchText;

        public void Open(IEnumerable<ImageViewModel> images)
        {
            _images = images;

            Task.Run(() =>
            {
                HashSet<int> intersection = new HashSet<int>(images.First().Categories);

                foreach (ImageViewModel image in images.Skip(1))
                {
                    intersection.IntersectWith(image.Categories);
                }
                foreach (int id in intersection)
                {
                    _unfilteredCategories.Lookup(id).Value.IsChecked = true;
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
                foreach (SelectableCategoryViewModel category in _unfilteredCategories.Items)
                {
                    category.IsChecked = false;
                }
            });
        }

        [RelayCommand]
        private async Task Submit()
        {
            await _imageService.EditRangeCategoriesAsync(_images.Select(f => f.Id), _unfilteredCategories.Items.Where(f => f.IsChecked).Select(f => f.Id));
            Close();
        }

        [RelayCommand]
        private async Task Delete()
        {
            string endWord = StringMethods.GetEndWord(_images.Count());
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "Remove images",
                CloseButtonText = "Cancel",
                Content = $"Are you sure to remove {_images.Count()} image{endWord} from this category?\nImage{endWord} will not be deleted.",
                PrimaryButtonText = "Remove"
            });

            if (result != ContentDialogResult.Primary)
            {
                return;
            }
            await _imageService.DeleteRangeAsync(_images.Select(f => f.Id));
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
