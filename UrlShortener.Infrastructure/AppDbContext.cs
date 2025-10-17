using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain;

namespace UrlShortener.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<ShortUrl>().HasIndex(x => x.OriginalUrl).IsUnique();
        b.Entity<ShortUrl>().HasIndex(x => x.ShortCode).IsUnique();
    }
}