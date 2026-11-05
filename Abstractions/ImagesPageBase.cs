using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using KGallery.Controls;
using KGallery.Images;
using KGallery.Models;
using KGallery.Services;
using KGallery.Text;
using KGallery.ViewModels;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace KGallery.Abstractions
{
    public abstract partial class ImagesPageViewModelBase : ViewModel
    {
        protected readonly ISnackbarService _snackbarService;
        protected readonly OpenDialogFactory _openDialogFactory;
        protected readonly ImagesService _imagesService;
        protected readonly IContentDialogService _contentDialogService;
        protected readonly ApplyFiltersDialog _applyFiltersDialog;

        private ReadOnlyObservableCollection<ImageViewModel> _images;
        public ReadOnlyObservableCollection<ImageViewModel> Images => _images;

        protected readonly ISubject<Unit> _filterNotifier;
        protected int _currentCategoryId = -1;

        public ImagesPageViewModelBase(ISnackbarService snackbarService, ImagesService imagesService, OpenDialogFactory openDialogPool, IContentDialogService contentDialogService, ApplyFiltersDialog filtersDialog)
        {
            _applyFiltersDialog = filtersDialog;
            _snackbarService = snackbarService;
            _openDialogFactory = openDialogPool;
            _imagesService = imagesService;
            _contentDialogService = contentDialogService;

            _filterNotifier = filtersDialog.GetNotification();
            _imagesService.Images.Connect().Filter(GetFilter()).ObserveOn(SynchronizationContext.Current).SortAndBind(out _images, GetSorting()).Subscribe();
            filtersDialog.WhenValueChanged(f => f.Exclude).Subscribe(f => _exclude = f);
            filtersDialog.WhenValueChanged(f => f.Include).Subscribe(f => _include = f);
            filtersDialog.WhenValueChanged(f => f.Intersect).Subscribe(f => _intersect = f);
            filtersDialog.WhenValueChanged(f => f.FilterUnsorted).Subscribe(f => _filterUnsorted = f);
        }

        protected virtual IObservable<Func<ImageViewModel, bool>> GetFilter()
        {
            return _filterNotifier.Select(_ =>
            {
                if (_filterUnsorted && _currentCategoryId == -1)
                {
                    return f => f.Categories.Count == 0;
                }
                return (Func<ImageViewModel, bool>)(f => (_currentCategoryId == -1 || f.Categories.Contains(_currentCategoryId)) &&
                (_exclude == null || !_exclude.Any() || !f.Categories.Overlaps(_exclude)) &&
                (_include == null || !_include.Any() || f.Categories.Overlaps(_include)) &&
                (_intersect == null || !_intersect.Any() || f.Categories.IsSupersetOf(_intersect)));
            });
        }

        private IObservable<IComparer<ImageViewModel>> GetSorting()
        {
            var changes = Observable.Merge(this.WhenPropertyChanged(f => f.DateSortingSelected),
            this.WhenPropertyChanged(f => f.FavouriteSortingSelected));
            return changes.Select(_ =>
            {
                if (FavouriteSortingSelected)
                {
                    return SortExpressionComparer<ImageViewModel>.Descending(f => f.Favourited).ThenByDescending(f => f.CreationTime);
                }

                return SortExpressionComparer<ImageViewModel>.Descending(f => f.CreationTime);
            });
        }

        public virtual void SetCategory(Category category)
        {
            _currentCategoryId = category == null ? -1 : category.Id;
            _applyFiltersDialog.IgnoreCategoryId = category == null ? -1 : category.Id;
            _filterNotifier.OnNext(default);
        }

        private IEnumerable<int> _include;
        private IEnumerable<int> _exclude;
        private IEnumerable<int> _intersect;
        private bool _filterUnsorted;

        [ObservableProperty]
        private bool _favouriteSortingSelected;

        [ObservableProperty]
        private bool _dateSortingSelected = true;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectionMode), nameof(DisableOnSelectionVisibility))]
        private bool _isSelectionEnabled;

        public SelectionMode SelectionMode => IsSelectionEnabled ? SelectionMode.Extended : SelectionMode.Single;
        public Visibility DisableOnSelectionVisibility => IsSelectionEnabled ? Visibility.Collapsed : Visibility.Visible;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(EditRangeCategoriesCommand), nameof(DeleteRangeCommand), nameof(RemoveRangeCategoriesCommand))]
        private bool _canDoSelectionActions;

        [ObservableProperty]
        private bool _filtersFlyoutOpened;

        [RelayCommand]
        private void OpenFiltersFlyout()
        {
            FiltersFlyoutOpened = true;
        }

        [RelayCommand]
        private void OpenFiltersSetupDialog()
        {
            FiltersFlyoutOpened = false;
            _openDialogFactory.ShowDialog<ApplyFiltersDialog, NoValue>(default);
        }

        [RelayCommand]
        private async Task DeleteImage(ImageViewModel imageViewModel)
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
            await _imagesService.DeleteAsync(imageViewModel.Id);
        }

        [RelayCommand]
        protected async Task PasteImage()
        {
            if (!Clipboard.ContainsImage())
            {
                _snackbarService.Show("Image was not pasted", "Clipboard does not contain image or it's format is invalid.", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.Info16, 24), TimeSpan.FromSeconds(5));
                return;
            }

            string fileName = Path.Combine(_imagesService.ImagesDirectory, $"{Guid.NewGuid()}.png");
            Imaging.SaveImageFromClipboard(fileName);
            ImageItem item = new ImageItem()
            {
                ImagePath = fileName
            };
            await _imagesService.AddAsync(item, _currentCategoryId);
        }

        [RelayCommand]
        private void OpenImage(ImageViewModel item)
        {
            _openDialogFactory.ShowDialog<ImageViewer, ImageViewModel>(item);
        }

        [RelayCommand]
        private void EditImage(ImageViewModel item)
        {
            _openDialogFactory.ShowDialog<EditImageCategoriesDialog, ImageViewModel>(item);
        }

        [RelayCommand]
        private async Task ToggleFavourite(ImageViewModel item)
        {
            await _imagesService.EditFavouriteAsync(item.Id, item.Favourited);
        }

        [RelayCommand]
        private async Task ImportImagesAsync()
        {
            int skipped = await _imagesService.ImportImagesAsync(_currentCategoryId);
            if (skipped > 0)
            {
                string endWord = StringMethods.GetEndWord(skipped);
                _snackbarService.Show("Duplicates have been detected", $"Skipped {skipped} duplicate image{endWord}", ControlAppearance.Primary, new SymbolIcon(SymbolRegular.Info16, 28), TimeSpan.FromSeconds(5));
            }
        }

        [RelayCommand(CanExecute = nameof(CanDoSelectionActions))]
        private async Task DeleteRange(IList selectedItems)
        {
            string endWord = StringMethods.GetEndWord(selectedItems.Count);
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "Delete images",
                CloseButtonText = "Cancel",
                Content = $"Are you sure to delete {selectedItems.Count} image{endWord}?\nThis action cannot be undone.",
                PrimaryButtonText = "Delete all"
            });

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            await _imagesService.DeleteRangeAsync(selectedItems.Cast<ImageViewModel>().Select(f => f.Id));
        }

        [RelayCommand(CanExecute = nameof(CanDoSelectionActions))]
        private async Task RemoveRangeCategories(IList selectedItems)
        {
            string endWord = StringMethods.GetEndWord(selectedItems.Count);
            ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
            {
                Title = "Remove images",
                CloseButtonText = "Cancel",
                Content = $"Are you sure to remove {selectedItems.Count} image{endWord} from this category?\nImage{endWord} will not be deleted.",
                PrimaryButtonText = "Remove"
            });

            if (result != ContentDialogResult.Primary)
            {
                return;
            }

            await _imagesService.RemoveRangeCategoryAsync(selectedItems.Cast<ImageViewModel>().Select(f => f.Id), _currentCategoryId);
        }

        [RelayCommand(CanExecute = nameof(CanDoSelectionActions))]
        private async Task EditRangeCategories(IList selectedItems)
        {
            _openDialogFactory.ShowDialog<EditMultipleImagesCategoriesDialog, IEnumerable<ImageViewModel>>(selectedItems.Cast<ImageViewModel>());
        }

        [RelayCommand]
        private void OpenImageOverview(ImageViewModel viewModel)
        {
            _openDialogFactory.ShowDialog<ImageOverviewDialog, ImageViewModel>(viewModel);
        }
    }
}
