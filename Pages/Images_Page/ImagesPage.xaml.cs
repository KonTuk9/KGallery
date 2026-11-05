using KGallery.Abstractions;
using KGallery.Controls;
using System.Windows;
using System.Windows.Input;

namespace KGallery.Pages.Images_Page
{
    /// <summary>
    /// Interaction logic for ImagesPage.xaml
    /// </summary>
    public partial class ImagesPage : UiPage
    {
        private ImageViewer _viewer;
        private ImagesPageViewModel _viewModel => DataContext as ImagesPageViewModel;

        public ImagesPage(ImagesPageViewModel viewModel, ImageViewer imageViewer) : base(viewModel)
        {
            _viewer = imageViewer;
            InitializeComponent();
            Loaded += OnLoaded;
            imagesListBox.SelectionChanged += (_, _) => viewModel.CanDoSelectionActions = imagesListBox.SelectedItems.Count > 0;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewer.SetImages(_viewModel.Images);
            _viewModel.SetCategory(null);
        }

        private void ImageDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Wpf.Ui.Controls.Image image)
            {
                ((ImagesPageViewModel)DataContext).OpenImageCommand.Execute(image.DataContext);
            }
        }
    }
}
