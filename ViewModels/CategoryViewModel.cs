using CommunityToolkit.Mvvm.ComponentModel;
using KGallery.Models;

namespace KGallery.ViewModels
{
    public partial class CategoryViewModel : ObservableObject
    {
        public Category Category { get; }

        public int Id => Category.Id;
        public DateTime CreationTime => Category.CreationTime;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private bool _favourited;

        [ObservableProperty]
        private int _imagesCount;

        [ObservableProperty]
        private Thumbnail _thumbnail;

        public CategoryViewModel(Category category)
        {
            Category = category;
            Name = category.Name;
            Favourited = category.Favourited;
        }
    }
}
