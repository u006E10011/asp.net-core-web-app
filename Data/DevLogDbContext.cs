using DevLog.Models;
using Microsoft.EntityFrameworkCore;

namespace DevLog.Data;

public class DevLogDbContext(DbContextOptions<DevLogDbContext> options) : DbContext(options)
{
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Post>(entity =>
        {
            entity.Property(post => post.CreatedAtUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(post => post.Title).HasMaxLength(160);
            entity.Property(post => post.Content).HasMaxLength(10000);
            entity.Property(post => post.ImageFileName).HasMaxLength(255);
            entity.Property(post => post.ImageMimeType).HasMaxLength(120);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(tag => tag.Name).IsUnique();
            entity.Property(tag => tag.Name).HasMaxLength(64);
        });

        modelBuilder.Entity<Post>()
            .HasMany(post => post.Tags)
            .WithMany(tag => tag.Posts)
            .UsingEntity<Dictionary<string, object>>(
                "PostTag",
                right => right.HasOne<Tag>().WithMany().HasForeignKey("TagId").OnDelete(DeleteBehavior.Cascade),
                left => left.HasOne<Post>().WithMany().HasForeignKey("PostId").OnDelete(DeleteBehavior.Cascade),
                join =>
                {
                    join.HasKey("PostId", "TagId");
                    join.ToTable("PostTags");
                });
    }
}
