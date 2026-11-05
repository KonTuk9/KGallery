using DynamicData;
using DynamicData.Aggregation;
using KGallery.Abstractions;
using KGallery.DB;
using KGallery.Models;
using KGallery.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace KGallery.Services
{
    public class CategoriesService : IAsyncInitializer
    {
        public SourceCache<CategoryViewModel, int> Categories { get; } = new SourceCache<CategoryViewModel, int>(f => f.Id);

        public async Task InitializeAsync()
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            Dictionary<int, int> categoryImagesCount = await context.Categories.AsNoTracking()
            .Select(f => new { f.Id, f.Images.Count })
            .ToDictionaryAsync(f => f.Id, f => f.Count);

            List<Category> categories = await context.Categories.AsNoTracking().ToListAsync();
            Categories.AddOrUpdate(categories.Select(f => new CategoryViewModel(f)
            {
                ImagesCount = categoryImagesCount[f.Id],
                Thumbnail = PickRandomThumbnail(f)
            }));
        }

        private Thumbnail PickRandomThumbnail(Category category)
        {
            using ApplicationDbContext context = new ApplicationDbContext();
            int count = context.Images.AsNoTracking().Count(f => f.Categories.Contains(category));
            Thumbnail thumbnail = context.Images.AsNoTracking().Where(f => f.Categories.Contains(category)).Skip(Random.Shared.Next(0, count)).Select(f => f.Thumbnail).FirstOrDefault();
            return thumbnail;
        }

        public async Task UpdateCategoryAsync(int id)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            CategoryViewModel viewModel = Categories.Lookup(id).Value;
            if (viewModel.Thumbnail == null)
            {
                viewModel.Thumbnail = PickRandomThumbnail(viewModel.Category);
            }
            else if (!await context.Categories.AsNoTracking().Where(f => f.Id == id).Include(f => f.Images).AnyAsync(f => f.Images.Any(x => x.Id == viewModel.Thumbnail.ImageItemId)))
            {
                viewModel.Thumbnail = PickRandomThumbnail(viewModel.Category);
            }

            viewModel.ImagesCount = await context.Categories.AsNoTracking().Where(f => f.Id == id).Select(f => f.Images.Count).FirstAsync();

            Categories.Refresh(viewModel);
        }

        public async Task AddAsync(Category category)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            await context.AddAsync(category);
            await context.SaveChangesAsync();

            Categories.AddOrUpdate(new CategoryViewModel(category));
        }

        public async Task EditPropertyAsync<TValue>(int id, Expression<Func<Category, TValue>> property, [NotNull] TValue value)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            await context.Categories.Where(f => f.Id == id).ExecuteUpdateAsync(setter => setter.SetProperty(property, value));
            Categories.Refresh(Categories.Lookup(id).Value);
        }

        public async Task DeleteAsync(int id)
        {
            await using ApplicationDbContext context = new ApplicationDbContext();
            context.Categories.Remove(await context.Categories.FindAsync(id));
            await context.SaveChangesAsync();
            Categories.Remove(id);
        }
    }
}
