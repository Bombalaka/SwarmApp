using Microsoft.EntityFrameworkCore;
using app.Models;

namespace app.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // This represents the Posts table in your database
    public DbSet<Post> Posts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the Post entity
        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Content).HasMaxLength(1000);
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            // Let Entity Framework handle timestamps in application code
            
            // Ignore ImageFile property - it's only for file uploads, not database storage
            entity.Ignore(e => e.ImageFile);
        });
    }
}
