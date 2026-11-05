using KGallery.Abstractions;
using KGallery.Images;
using KGallery.Services;
using KGallery.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KGallery.Controls
{
    /// <summary>
    /// Interaction logic for ImageViewer.xaml
    /// </summary>
    public partial class ImageViewer : UserControl, IOpenDialog<ImageViewModel>
    {
        private ReadOnlyObservableCollection<ImageViewModel> _images;
        private int _index;
        private readonly GlobalInput _input;

        public ImageViewer(GlobalInput input)
        {
            _input = input;
            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void SetImages(ReadOnlyObservableCollection<ImageViewModel> source)
        {
            _images = source;
        }

        public void Open(ImageViewModel item)
        {
            _input.Subscribe(ImageViewerKeyDown);

            image.Source = Imaging.LoadImage(item.ImagePath);

            _index = _images.IndexOf(item);

            if (_index < 0)
            {
                throw new InvalidOperationException();
            }

            Visibility = Visibility.Visible;
        }

        private void CloseViewer()
        {
            Visibility = Visibility.Collapsed;
            image.Source = null;
            _input.Unsubscribe(ImageViewerKeyDown);
        }

        private void CloseViewer(object sender, MouseButtonEventArgs e)
        {
            CloseViewer();
        }

        private void SetPreviousImage()
        {
            if (_images.Count < 2)
            {
                return;
            }

            _index = _index > 0 ? _index - 1 : _images.Count - 1;
            image.Source = Images.Imaging.LoadImage(_images[_index].ImagePath);
        }

        private void SetNextImage()
        {
            if (_images.Count < 2)
            {
                return;
            }

            _index = _index < _images.Count - 1 ? _index + 1 : 0;
            image.Source = Images.Imaging.LoadImage(_images[_index].ImagePath);
        }

        private void ImageViewerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D || e.Key == Key.Right)
            {
                SetNextImage();
            }
            else if (e.Key == Key.Left || e.Key == Key.A)
            {
                SetPreviousImage();
            }
            else if (e.Key == Key.Escape)
            {
                CloseViewer();
            }
        }
    }
}
