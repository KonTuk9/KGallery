using CommunityToolkit.Mvvm.ComponentModel;
using DynamicData.Binding;

namespace KGallery.ViewModels
{
    public partial class FilteringCategoryViewModel : ObservableObject
    {
        public CategoryViewModel CategoryViewModel { get; }

        public FilteringCategoryViewModel(CategoryViewModel viewModel)
        {
            CategoryViewModel = viewModel;

            viewModel.WhenPropertyChanged(f => f.Name).Subscribe(_ => OnPropertyChanged(nameof(Name)));
            viewModel.WhenPropertyChanged(f => f.ImagesCount).Subscribe(_ => OnPropertyChanged(nameof(ImagesCount)));
        }

        public int Id => CategoryViewModel.Id;
        public string Name => CategoryViewModel.Name;
        public int ImagesCount => CategoryViewModel.ImagesCount;

        [ObservableProperty]
        private bool _isIntersect;

        [ObservableProperty]
        private bool _isInclude;

        [ObservableProperty]
        private bool _isExclude;

        [ObservableProperty]
        private bool _isNone = true;
    }
}
