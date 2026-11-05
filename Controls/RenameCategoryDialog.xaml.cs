using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using KGallery.Abstractions;
using KGallery.Services;
using KGallery.ViewModels;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace KGallery.Controls
{
    /// <summary>
    /// Interaction logic for RenameCategoryDialog.xaml
    /// </summary>
    [ObservableObject]
    public partial class RenameCategoryDialog : UserControl, IOpenDialog<CategoryViewModel>
    {
        private CategoryViewModel _category;
        private readonly CategoriesService _categoriesService;
        private readonly GlobalInput _input;

        private ReadOnlyObservableCollection<CategoryViewModel> _categories;

        public RenameCategoryDialog(CategoriesService categoriesService, GlobalInput input)
        {
            _input = input;
            _categoriesService = categoriesService;
            _categoriesService.Categories.Connect().ObserveOn(SynchronizationContext.Current).Bind(out _categories).Subscribe();

            InitializeComponent();
            Visibility = Visibility.Collapsed;
        }

        public void Open(CategoryViewModel category)
        {
            _category = category;
            NewName = category.Name;
            Visibility = Visibility.Visible;
            _input.Subscribe(OnDialogKeyDown);
        }


        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
        private string _newName;

        public bool CanSubmit => !string.IsNullOrWhiteSpace(NewName) && !_categories.Any(f => f.Name == NewName);

        [RelayCommand(CanExecute = nameof(CanSubmit))]
        private async Task Submit()
        {
            _category.Name = NewName;
            await _categoriesService.EditPropertyAsync(_category.Id, f => f.Name, NewName);
            Close();
            NewName = string.Empty;
        }

        [RelayCommand]
        private void Close()
        {
            Visibility = Visibility.Collapsed;
            _input.Unsubscribe(OnDialogKeyDown);
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
