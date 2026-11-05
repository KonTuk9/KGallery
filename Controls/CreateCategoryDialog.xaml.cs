using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KGallery.Abstractions;
using KGallery.Models;
using KGallery.Services;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KGallery.Controls
{
    /// <summary>
    /// Interaction logic for CreateCategoryDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class CreateCategoryDialog : UserControl, IOpenDialog<NoValue>
    {
        private readonly CategoriesService _categoriesService;
        private readonly GlobalInput _input;

        public CreateCategoryDialog(CategoriesService categoriesService, GlobalInput input)
        {
            _input = input;
            _categoriesService = categoriesService;

            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void Open(NoValue parameter = default)
        {
            Visibility = Visibility.Visible;
            _input.Subscribe(OnDialogKeyDown);
        }

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string _categoryName;

        [RelayCommand]
        private void Close()
        {
            _input.Unsubscribe(OnDialogKeyDown);
            Visibility = Visibility.Collapsed;
        }

        public bool CanSubmit => !string.IsNullOrWhiteSpace(CategoryName) && !_categoriesService.Categories.Items.Any(f => f.Name.Equals(CategoryName, StringComparison.OrdinalIgnoreCase));

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            await _categoriesService.AddAsync(new Category() { Name = CategoryName });
            Close();
            CategoryName = string.Empty;
        }

        private async void OnDialogKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && CanSubmit)
            {
                await Submit();
            }
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
