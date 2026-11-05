using DynamicData;
using KGallery.DB;
using KGallery.Images;
using KGallery.Models;
using KGallery.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Channels;

namespace KGallery.Services
{
    public class ImageScanningService
    {
        private readonly ImagesService _imageService;
        private readonly CategoriesService _categoriesService;

        public ImageScanningService(ImagesService imagesService, CategoriesService categoriesService)
        {
            _imageService = imagesService;
            _categoriesService = categoriesService;
        }

        public async Task<int> EnsureFilesExistAsync()
        {
            int count = 0;

            Channel<int> channel = Channel.CreateBounded<int>(8);
            HashSet<int> categoriesUpdate = new HashSet<int>();

            await using ApplicationDbContext context = new ApplicationDbContext();

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
            await Parallel.ForEachAsync(context.Images.AsAsyncEnumerable(), options, async (item, token) =>
            {
                if (!File.Exists(item.ImagePath))
                {
                    categoriesUpdate.UnionWith(_imageService.Images.Lookup(item.Id).Value.Categories);
                    context.Images.Remove(item);
                    await channel.Writer.WriteAsync(item.Id);
                    count++;
                }
            });
            channel.Writer.TryComplete();
            await context.SaveChangesAsync();

            _imageService.Images.Remove(await channel.Reader.ReadAllAsync().ToArrayAsync());

            Parallel.ForEach(categoriesUpdate, options, async id => await _categoriesService.UpdateCategoryAsync(id));

            return count;
        }

        public async Task RefreshThumbnailsAsync()
        {
            Channel<int> channel = Channel.CreateBounded<int>(8);

            await using ApplicationDbContext context = new ApplicationDbContext();

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
            await Parallel.ForEachAsync(context.Images.AsAsyncEnumerable(), options, async (item, ct) =>
            {
                FileInfo info = new FileInfo(item.ImagePath);
                if ((item.LastEditTime - info.LastWriteTime).Seconds == 0)
                {
                    item.LastEditTime = info.LastWriteTime;
                    await channel.Writer.WriteAsync(item.Id);
                }
            });
            channel.Writer.TryComplete();

            int[] images = await channel.Reader.ReadAllAsync().ToArrayAsync();
            await foreach (Thumbnail thumbnail in context.Thumbnails.Where(f => images.Contains(f.ImageItemId)).Include(f => f.ImageItem).AsAsyncEnumerable())
            {
                byte[] pixels = await Imaging.CreateThumbnailPixelsAsync(thumbnail.ImageItem.ImagePath);
                thumbnail.Data = pixels;
                _imageService.Images.Lookup(thumbnail.ImageItemId).Value.Thumbnail = pixels;

                foreach (CategoryViewModel category in _categoriesService.Categories.Items.Where(f => f.Thumbnail != null && f.Thumbnail.Id == thumbnail.Id))
                {
                    category.Thumbnail = thumbnail;
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
