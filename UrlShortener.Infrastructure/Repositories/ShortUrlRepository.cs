using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Abstractions;
using UrlShortener.Domain;

namespace UrlShortener.Infrastructure.Repositories;

public class ShortUrlRepository(AppDbContext db) : IShortUrlRepository
{
    private readonly AppDbContext _db = db;

    public Task<IReadOnlyList<ShortUrl>> GetAllAsync() =>
        _db.ShortUrls.AsNoTracking().ToListAsync().ContinueWith(t => (IReadOnlyList<ShortUrl>)t.Result);

    public Task<ShortUrl?> GetByIdAsync(int id) =>
        _db.ShortUrls.FirstOrDefaultAsync(x => x.Id == id);

    public Task<ShortUrl?> GetByOriginalAsync(string original) =>
        _db.ShortUrls.FirstOrDefaultAsync(x => x.OriginalUrl == original);

    public Task<ShortUrl?> GetByCodeAsync(string code) =>
        _db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == code);

    public async Task<ShortUrl> AddAsync(ShortUrl entity)
    {
        var e = await _db.ShortUrls.AddAsync(entity);
        return e.Entity;
    }

    public Task<bool> DeleteAsync(ShortUrl entity)
    {
        _db.ShortUrls.Remove(entity);
        return Task.FromResult(true);
    }

    public Task SaveAsync() => _db.SaveChangesAsync();
}