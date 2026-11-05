using KGallery.Abstractions;
using KGallery.Controls;
using System.Windows;
using System.Windows.Input;

namespace KGallery.Pages.Category_Images_Page
{
    /// <summary>
    /// Interaction logic for CategoryImagesPage.xaml
    /// </summary>
    public partial class CategoryImagesPage : UiPage
    {
        private ImageViewer _viewer;
        private CategoryImagesPageViewModel _viewModel => DataContext as CategoryImagesPageViewModel;

        public CategoryImagesPage(CategoryImagesPageViewModel viewModel, ImageViewer imageViewer) : base(viewModel)
        {
            InitializeComponent();
            _viewer = imageViewer;
            Loaded += OnLoaded;
            imagesListBox.SelectionChanged += (_, _) => viewModel.CanDoSelectionActions = imagesListBox.SelectedItems.Count > 0;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewer.SetImages(_viewModel.Images);
        }

        private void ImageDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Image image)
            {
                ((ImagesPageViewModelBase)DataContext).OpenImageCommand.Execute(image.DataContext);
            }
        }
    }
}
