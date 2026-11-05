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
    /// Interaction logic for CategoryOverviewDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class CategoryOverviewDialog : UserControl, IOpenDialog<CategoryViewModel>
    {
        private readonly GlobalInput _input;

        public CategoryOverviewDialog(GlobalInput input)
        {
            _input = input;

            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        [ObservableProperty]
        private CategoryViewModel _categoryViewModel;

        public void Open(CategoryViewModel category)
        {
            CategoryViewModel = category;
            Visibility = Visibility.Visible;
            _input.Subscribe(OnDialogKeyDown);
        }

        [RelayCommand]
        private void Close()
        {
            _input.Unsubscribe(OnDialogKeyDown);
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
