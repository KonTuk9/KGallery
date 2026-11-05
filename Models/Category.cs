namespace KGallery.Models
{
    public class Category
    {
        public int Id { get; set; }
        public bool Favourited { get; set; }
        public string Name { get; set; }
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public List<ImageItem> Images { get; set; } = [];
    }
}
