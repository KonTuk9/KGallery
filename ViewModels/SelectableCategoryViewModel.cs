using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;
using System.Reactive.Linq;

namespace KGallery.ViewModels
{
    public partial class SelectableCategoryViewModel : ObservableObject
    {
        public CategoryViewModel CategoryViewModel { get; }

        public SelectableCategoryViewModel(CategoryViewModel viewModel)
        {
            CategoryViewModel = viewModel;

            viewModel.WhenPropertyChanged(f => f.Name).Subscribe(_ => OnPropertyChanged(nameof(Name)));
            viewModel.WhenPropertyChanged(f => f.ImagesCount).Subscribe(_ => OnPropertyChanged(nameof(ImagesCount)));
        }

        public int Id => CategoryViewModel.Id;
        public string Name => CategoryViewModel.Name;
        public int ImagesCount => CategoryViewModel.ImagesCount;

        [ObservableProperty]
        private bool _isChecked;
    }
}
