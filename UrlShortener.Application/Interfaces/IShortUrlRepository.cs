using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IShortUrlRepository
{
    Task<IReadOnlyList<ShortUrl>> GetAllAsync();
    Task<ShortUrl?> GetByIdAsync(int id);
    Task<ShortUrl?> GetByOriginalAsync(string original);
    Task<ShortUrl?> GetByCodeAsync(string code);
    Task<ShortUrl> AddAsync(ShortUrl entity);
    Task<bool> DeleteAsync(ShortUrl entity);
    Task SaveAsync();
}