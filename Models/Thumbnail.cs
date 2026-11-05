namespace KGallery.Models
{
    public class Thumbnail
    {
        public int Id { get; set; }
        public int ImageItemId { get; set; }
        public ImageItem ImageItem { get; set; }
        public byte[] Data { get; set; }
    }
}
