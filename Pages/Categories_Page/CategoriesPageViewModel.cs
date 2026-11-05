using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using DynamicData.Binding;
using KGallery.Abstractions;
using KGallery.Controls;
using KGallery.Pages.Category_Images_Page;
using KGallery.Services;
using KGallery.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui;
using Wpf.Ui.Controls;
using Wpf.Ui.Extensions;

namespace KGallery.Pages.Categories_Page
{
    public partial class CategoriesPageViewModel : ViewModel
    {
        private readonly CategoriesService _categoriesService;
        private readonly OpenDialogFactory _openDialogFactory;
        private readonly IContentDialogService _contentDialogService;
        private readonly INavigationService _navigationService;

        private ReadOnlyObservableCollection<CategoryViewModel> _categories;
        public ReadOnlyObservableCollection<CategoryViewModel> Categories => _categories;

        public CategoriesPageViewModel(INavigationService navigationService, CategoriesService categoriesService, OpenDialogFactory openDialogFactory, IContentDialogService contentDialogService)
        {
            _navigationService = navigationService;
            _categoriesService = categoriesService;
            _openDialogFactory = openDialogFactory;
            _contentDialogService = contentDialogService;
            _categoriesService.Categories.Connect().Filter(GetFilter()).ObserveOn(SynchronizationContext.Current).SortAndBind(out _categories, GetSorting()).Subscribe();
        }

        [RelayCommand]
        private void NavigateToImages(CategoryViewModel category)
        {
            CategoryImagesPageViewModel viewModel = App.ServiceProvider.GetRequiredService<CategoryImagesPageViewModel>();
            viewModel.SetCategory(category.Category);
            _navigationService.NavigateWithHierarchy(typeof(CategoryImagesPage), viewModel);
        }

        [ObservableProperty]
        private int _selectedSorting;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(SelectionMode), nameof(DisableOnSelectionVisibility))]
        private bool _isSelectionEnabled;

        public SelectionMode SelectionMode => IsSelectionEnabled ? SelectionMode.Extended : SelectionMode.Single;
        public Visibility DisableOnSelectionVisibility => IsSelectionEnabled ? Visibility.Collapsed : Visibility.Visible;

        private IObservable<Func<CategoryViewModel, bool>> GetFilter()
        {
            return this.WhenPropertyChanged(f => f.SearchText).Select(_ =>
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    return f => true;
                }
                return (Func<CategoryViewModel, bool>)(f => f.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            });
        }

        private IObservable<IComparer<CategoryViewModel>> GetSorting()
        {
            return this.WhenPropertyChanged(f => f.SelectedSorting).Select(_ =>
            {
                return SelectedSorting switch
                {
                    0 => SortExpressionComparer<CategoryViewModel>.Descending(f => f.Favourited).ThenByDescending(f => f.ImagesCount).ThenByAscending(f => f.Name),
                    1 => SortExpressionComparer<CategoryViewModel>.Descending(f => f.ImagesCount).ThenByDescending(f => f.Favourited),
                    2 => SortExpressionComparer<CategoryViewModel>.Ascending(f => f.Name).ThenByDescending(f => f.Favourited),
                    3 => SortExpressionComparer<CategoryViewModel>.Descending(f => f.CreationTime).ThenByDescending(f => f.Favourited),
                    _ => throw new NotImplementedException(),
                };
            });
        }

        [RelayCommand]
        private void AddCategory()
        {
            _openDialogFactory.ShowDialog<CreateCategoryDialog, NoValue>(default);
        }

        [RelayCommand]
        private async Task ToggleFavourite(CategoryViewModel category)
        {
            await _categoriesService.EditPropertyAsync(category.Id, f => f.Favourited, category.Favourited);
        }

        [RelayCommand]
        private async Task DeleteCategory(CategoryViewModel category)
        {
            if (category.ImagesCount > 0)
            {
                string endWord = category.ImagesCount > 1 ? "s" : "";
                ContentDialogResult result = await _contentDialogService.ShowSimpleDialogAsync(new SimpleContentDialogCreateOptions()
                {
                    Title = "Delete category",
                    Content = $"Are you sure to delete this category with {category.ImagesCount} image{endWord}?\nImages will not be deleted.",
                    CloseButtonText = "Cancel",
                    PrimaryButtonText = "Delete"
                });
                if (result != ContentDialogResult.Primary)
                {
                    return;
                }
            }
            await _categoriesService.DeleteAsync(category.Id);
        }

        [RelayCommand]
        private void RenameCategory(CategoryViewModel category)
        {
            _openDialogFactory.ShowDialog<RenameCategoryDialog, CategoryViewModel>(category);
        }

        [RelayCommand]
        private void OpenCategoryOverview(CategoryViewModel category)
        {
            _openDialogFactory.ShowDialog<CategoryOverviewDialog, CategoryViewModel>(category);
        }
    }
}
