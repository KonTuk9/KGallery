using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KGallery.Abstractions;
using KGallery.Services;
using KGallery.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KGallery.Controls
{
    /// <summary>
    /// Interaction logic for ImageOverviewDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class ImageOverviewDialog : UserControl, IOpenDialog<ImageViewModel>
    {
        private readonly GlobalInput _globalInput;

        public ImageOverviewDialog(GlobalInput globalInput)
        {
            _globalInput = globalInput;
            InitializeComponent();
        }

        public void Open(ImageViewModel image)
        {
            Image = image;
            Visibility = Visibility.Visible;
            _globalInput.Unsubscribe(OnDialogKeyDown);
        }

        [ObservableProperty]
        private ImageViewModel _image;

        [RelayCommand]
        private void Close()
        {
            _globalInput.Unsubscribe(OnDialogKeyDown);
            Visibility = Visibility.Collapsed;
        }

        private void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
