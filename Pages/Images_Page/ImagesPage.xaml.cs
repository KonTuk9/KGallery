using KGallery.Abstractions;
using KGallery.Controls;
using KGallery.Services;
using System.Windows;
using System.Windows.Input;

namespace KGallery.Pages.Images_Page
{
    /// <summary>
    /// Interaction logic for ImagesPage.xaml
    /// </summary>
    public partial class ImagesPage : UiPage
    {
        private readonly ImageViewer _viewer;
        private readonly GlobalInput _input;
        private ImagesPageViewModel _viewModel => DataContext as ImagesPageViewModel;

        public ImagesPage(ImagesPageViewModel viewModel, ImageViewer imageViewer, GlobalInput input) : base(viewModel)
        {
            _input = input;
            _viewer = imageViewer;
            InitializeComponent();
            Loaded += OnLoaded;
            imagesListBox.SelectionChanged += (_, _) => viewModel.CanDoSelectionActions = imagesListBox.SelectedItems.Count > 0;
        }

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            _viewer.SetImages(_viewModel.Images);
            _viewModel.SetCategory(null);
            _input.Subscribe(InputBindings);
        }

        protected override void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _input.Unsubscribe(InputBindings);
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
