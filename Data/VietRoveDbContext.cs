using Microsoft.EntityFrameworkCore;
using VietRove.Entities;

namespace VietRove.Data
{
    public class VietRoveDbContext : DbContext
    {
        public VietRoveDbContext(DbContextOptions<VietRoveDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<GuideProfile> GuideProfiles { get; set; } = null!;
        public DbSet<GuideCategory> GuideCategories { get; set; } = null!;
        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Article> Articles { get; set; } = null!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuideCategory>()
                .HasKey(gc => new { gc.GuideId, gc.CategoryId });

            base.OnModelCreating(modelBuilder);
        }
    }
}