using CommunityToolkit.Mvvm.ComponentModel;
using KGallery.Models;

namespace KGallery.ViewModels
{
    public partial class ImageViewModel : ObservableObject
    {
        public ImageItem ImageItem { get; }

        public int Id => ImageItem.Id;
        public DateTime CreationTime => ImageItem.CreationTime;
        public string ImagePath => ImageItem.ImagePath;

        public HashSet<int> Categories { get; set; } = new HashSet<int>();

        [ObservableProperty]
        private byte[] _thumbnail;

        [ObservableProperty]
        private bool _favourited;

        public ImageViewModel(ImageItem imageItem)
        {
            ImageItem = imageItem;

            Favourited = imageItem.Favourited;

            Thumbnail = imageItem.Thumbnail.Data;
        }
    }
}
