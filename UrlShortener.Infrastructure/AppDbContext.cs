using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
    public DbSet<AboutInfo> AboutInfos => Set<AboutInfo>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        // Unique indexes for fast lookup and no duplicates
        b.Entity<ShortUrl>().HasIndex(x => x.OriginalUrl).IsUnique();
        b.Entity<ShortUrl>().HasIndex(x => x.ShortCode).IsUnique();

        // About page seed with algorithm description
        b.Entity<AboutInfo>().HasData(new AboutInfo
        {
            Id = 1,
            Content = "",
            UpdatedAtUtc = DateTime.UtcNow
        });
    }
}