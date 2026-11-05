using DynamicData;
using DynamicData.Aggregation;
using KGallery.Abstractions;
using KGallery.DB;
using KGallery.Images;
using KGallery.Models;
using KGallery.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using System.IO;

namespace KGallery.Services
{
    public class ImagesService : IAsyncInitializer
    {
        public SourceCache<ImageViewModel, int> Images { get; } = new SourceCache<ImageViewModel, int>(f => f.Id);
        public string ImagesDirectory { get; private set; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

        private readonly CategoriesService _categoriesService;

        public ImagesService(CategoriesService categoriesService, SettingsService settingsService)
        {
            _categoriesService = categoriesService;
            if (!string.IsNullOrWhiteSpace(settingsService.Configuration.BaseDirectory))
            {
                ImagesDirectory = settingsService.Configuration.BaseDirectory;
                settingsService.SettingsSubject.Subscribe(f => ImagesDirectory = f.BaseDirectory);
            }
        }

        public async Task InitializeAsync()
        {
            if (!Directory.Exists(ImagesDirectory))
            {
                Directory.CreateDirectory(ImagesDirectory);
            }

            await using ApplicationDbContext context = new ApplicationDbContext();
            List<ImageItem> items = await context.Images.AsNoTracking().Include(f => f.Thumbnail).ToListAsync();
            Images.AddOrUpdate(items.Select(f => new ImageViewModel(f) { Categories = context.Images.AsNoTracking().Where(x => x.Id == f.Id).SelectMany(d => d.Categories.Select(e => e.Id)).ToHashSet() }));
        }

        public async Task AddAsync(ImageItem item, int initialCategoryId = -1)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            byte[] thumbnailData = await Imaging.CreateThumbnailPixelsAsync(item.ImagePath);
            item.Categories = await context.Categories.Where(f => f.Id == initialCategoryId).ToListAsync();
            item.Thumbnail = new Thumbnail()
            {
                Data = thumbnailData,
                ImageItem = item,
            };
            await context.Images.AddAsync(item);
            await context.SaveChangesAsync();

            ImageViewModel viewModel = new ImageViewModel(item);
            viewModel.Categories = initialCategoryId == -1 ? viewModel.Categories : new HashSet<int>() { initialCategoryId };
            Images.AddOrUpdate(viewModel);

            if (initialCategoryId != -1)
            {
                await _categoriesService.UpdateCategoryAsync(initialCategoryId);
            }
        }

        public async Task DeleteAsync(int id)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            ImageItem item = await context.Images.AsNoTracking().FirstAsync(f => f.Id == id);
            context.Images.Remove(item);
            await context.SaveChangesAsync();

            DeleteOwnFile(item.ImagePath);

            HashSet<int> categories = Images.Lookup(id).Value.Categories;
            Images.Remove(id);

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            Parallel.ForEach(categories, async f => await _categoriesService.UpdateCategoryAsync(f));
        }

        private void DeleteOwnFile(string path)
        {
            if (Path.GetDirectoryName(path) == ImagesDirectory)
            {
                File.Delete(path);
            }
        }

        public async Task EditFavouriteAsync(int id, bool value)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            await context.Images.Where(f => f.Id == id).ExecuteUpdateAsync(setter => setter.SetProperty(f => f.Favourited, value));

            ImageViewModel viewModel = Images.Lookup(id).Value;
            Images.Refresh(viewModel);
        }

        public async Task EditCategoriesAsync(int id, IEnumerable<int> categoriesIds)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            ImageItem item = await context.Images.Include(f => f.Categories).FirstAsync(f => f.Id == id);
            item.Categories = await context.Categories.Where(f => categoriesIds.Contains(f.Id)).ToListAsync();
            await context.SaveChangesAsync();

            ImageViewModel viewModel = Images.Lookup(id).Value;
            IEnumerable<int> oldIds = viewModel.Categories;
            viewModel.Categories = categoriesIds.ToHashSet();
            Images.Refresh(viewModel);

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 4 };
            Parallel.ForEach(oldIds.Union(categoriesIds), options, async i => await _categoriesService.UpdateCategoryAsync(i));
        }

        public async Task RemoveCategoryAsync(int id, int categoryId)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            ImageItem item = await context.Images.Include(f => f.Categories).FirstAsync(f => f.Id == id);
            item.Categories.RemoveAll(f => f.Id == categoryId);
            await context.SaveChangesAsync();

            await _categoriesService.UpdateCategoryAsync(categoryId);

            ImageViewModel viewModel = Images.Lookup(id).Value;
            viewModel.Categories.Remove(categoryId);
            Images.Refresh(viewModel);
        }

        public async Task<List<Category>> GetCategoriesAsync(int id)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            return await context.Images.AsNoTracking().Where(f => f.Id == id).Select(f => f.Categories).FirstOrDefaultAsync();
        }

        public async Task<int> ImportImagesAsync(int initialCategoryId)
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                CheckFileExists = true,
                Filter = "Image files | *.png; *.bmp; *.wmp; *.gif; *.jpg; *jpeg; *.tiff",
                Multiselect = true,
                Title = "Import images",
                AddExtension = true,
                CheckPathExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                await using ApplicationDbContext context = new ApplicationDbContext();
                List<string> files = dialog.FileNames.ToList();
                List<string> remove = await context.Images.AsNoTracking().Where(f => files.Contains(f.ImagePath)).Select(f => f.ImagePath).ToListAsync();
                files.Remove(remove);

                _ = Task.Factory.StartNew(() => AddRange(files, initialCategoryId), TaskCreationOptions.LongRunning);

                return remove.Count;
            }

            return 0;
        }

        private async Task AddRange(List<string> files, int initialCategoryId)
        {
            int i = 0;
            ImageViewModel[] viewModels = new ImageViewModel[files.Count];

            await using ApplicationDbContext dbContext = new ApplicationDbContext();
            foreach (string file in files)
            {
                ImageItem item = new ImageItem() { ImagePath = file };
                byte[] thumbnailData = await Imaging.CreateThumbnailPixelsAsync(file);
                item.Categories = await dbContext.Categories.Where(f => f.Id == initialCategoryId).ToListAsync();
                item.Thumbnail = new Thumbnail()
                {
                    Data = thumbnailData,
                    ImageItem = item,
                };
                await dbContext.Images.AddAsync(item);
                ImageViewModel viewModel = new ImageViewModel(item);
                viewModel.Categories = initialCategoryId == -1 ? viewModel.Categories : new HashSet<int>() { initialCategoryId };
                viewModels[i] = viewModel;
                i++;
            }

            await dbContext.SaveChangesAsync();
            Images.AddOrUpdate(viewModels);

            if (initialCategoryId != -1)
            {
                await _categoriesService.UpdateCategoryAsync(initialCategoryId);
            }
        }

        public async Task DeleteRangeAsync(IEnumerable<int> ids)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            IQueryable<ImageItem> items = context.Images.Where(f => ids.Contains(f.Id));
            context.Images.RemoveRange(items);
            await foreach (ImageItem item in items.AsAsyncEnumerable())
            {
                DeleteOwnFile(item.ImagePath);
            }
            await context.SaveChangesAsync();

            HashSet<int> updateCategories = Images.Items.Where(f => ids.Contains(f.Id)).SelectMany(f => f.Categories).ToHashSet();
            Images.Remove(ids);

            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
            Parallel.ForEach(updateCategories, async f => await _categoriesService.UpdateCategoryAsync(f));
        }

        public async Task RemoveRangeCategoryAsync(IEnumerable<int> ids, int categoryId)
        {
            if (categoryId == -1)
            {
                throw new ArgumentException();
            }

            await using ApplicationDbContext context = new ApplicationDbContext();
            Category category = await context.Categories.FirstAsync(f => f.Id == categoryId);
            IAsyncEnumerable<ImageItem> items = context.Images.Where(f => ids.Contains(f.Id)).Include(f => f.Categories).AsAsyncEnumerable();
            await foreach (ImageItem item in items)
            {
                item.Categories.Remove(category);
            }
            await context.SaveChangesAsync();

            int i = 0;
            ImageViewModel[] viewModels = new ImageViewModel[ids.Count()];
            foreach (int id in ids)
            {
                viewModels[i] = Images.Lookup(id).Value;
                viewModels[i].Categories.Remove(categoryId);
                i++;
            }
            Images.Refresh(viewModels);

            await _categoriesService.UpdateCategoryAsync(categoryId);
        }

        public async Task EditRangeCategoriesAsync(IEnumerable<int> ids, IEnumerable<int> categoriesId)
        {
            ArgumentNullException.ThrowIfNull(categoriesId);
            ArgumentNullException.ThrowIfNull(ids);

            await using ApplicationDbContext context = new ApplicationDbContext();
            List<Category> categories = await context.Categories.Where(f => categoriesId.Contains(f.Id)).ToListAsync();

            HashSet<int> updateCategories = categories.Select(f => f.Id).ToHashSet();

            IAsyncEnumerable<ImageItem> items = context.Images.Where(f => ids.Contains(f.Id)).Include(f => f.Categories).AsAsyncEnumerable();
            await foreach (ImageItem item in items)
            {
                item.Categories = categories;
            }
            await context.SaveChangesAsync();

            int i = 0;
            ImageViewModel[] viewModels = new ImageViewModel[ids.Count()];
            foreach (int id in ids)
            {
                viewModels[i] = Images.Lookup(id).Value;
                viewModels[i].Categories = categoriesId.ToHashSet();
                i++;
            }
            Images.Refresh(viewModels);

            updateCategories.UnionWith(await items.SelectMany(f => f.Categories).Select(f => f.Id).ToHashSetAsync());
            ParallelOptions options = new ParallelOptions() { MaxDegreeOfParallelism = 8 };
            Parallel.ForEach(updateCategories, async f => await _categoriesService.UpdateCategoryAsync(f));
        }
    }
}
