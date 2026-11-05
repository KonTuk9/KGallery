using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KGallery.Abstractions;
using KGallery.Controls;
using KGallery.Models;
using KGallery.Services;
using KGallery.ViewModels;
using Wpf.Ui;

namespace KGallery.Pages.Category_Images_Page
{
    public partial class CategoryImagesPageViewModel : ImagesPageViewModelBase
    {
        public CategoryImagesPageViewModel(ISnackbarService snackbarService, ImagesService imagesService, OpenDialogFactory openDialogFactory, IContentDialogService contentDialogService, ApplyFiltersDialog filtersDialog) : base(snackbarService, imagesService, openDialogFactory, contentDialogService, filtersDialog)
        {
            FavouriteSortingSelected = true;
        }

        public override void SetCategory(Category category)
        {
            base.SetCategory(category);
            NavigationHeader = category.Name;
        }

        [ObservableProperty]
        private string _navigationHeader;

        [RelayCommand]
        private async Task RemoveCategory(ImageViewModel item)
        {
            await _imagesService.RemoveCategoryAsync(item.Id, _currentCategoryId);
        }
    }
}
