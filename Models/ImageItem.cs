namespace KGallery.Models
{
    public class ImageItem
    {
        public int Id { get; set; }
        public bool Favourited { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public DateTime LastEditTime { get; set; } = DateTime.Now;
        public string ImagePath { get; set; }
        public Thumbnail Thumbnail { get; set; } = null;
        public List<Category> Categories { get; set; } = null;
    }
}
