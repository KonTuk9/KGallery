using KGallery.Abstractions;
using KGallery.Controls;
using KGallery.Services;
using Wpf.Ui;

namespace KGallery.Pages.Images_Page
{
    public partial class ImagesPageViewModel : ImagesPageViewModelBase
    {
        public ImagesPageViewModel(ISnackbarService snackbarService, ImagesService imagesService, OpenDialogFactory openDialogFactory, IContentDialogService contentDialogService, ApplyFiltersDialog filtersDialog) : base(snackbarService, imagesService, openDialogFactory, contentDialogService, filtersDialog)
        {
        }
    }
}
