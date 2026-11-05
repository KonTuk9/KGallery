using KGallery.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace KGallery.DB
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<ImageItem> Images { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Thumbnail> Thumbnails { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string path = Path.Join(AppDomain.CurrentDomain.BaseDirectory, "ImagesDb.db");
            optionsBuilder.UseSqlite($"Data Source={path}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ImageItem>()
                .HasOne(i => i.Thumbnail)
                .WithOne(t => t.ImageItem)
                .HasForeignKey<Thumbnail>(t => t.ImageItemId)
                .IsRequired();

            modelBuilder.Entity<ImageItem>()
                .HasMany(i => i.Categories)
                .WithMany(c => c.Images)
                .UsingEntity(j => j.ToTable("ImageCategories"));

            modelBuilder.Entity<Thumbnail>()
                .HasIndex(t => t.ImageItemId)
                .IsUnique();
        }
    }
}
