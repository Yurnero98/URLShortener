using UrlShortener.Application.Dtos;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Services;

public class UrlService
{
    private readonly IShortUrlRepository _repo;
    private readonly IShortCodeGenerator _gen;

    public UrlService(IShortUrlRepository repo, IShortCodeGenerator gen)
    {
        _repo = repo; _gen = gen;
    }

    public async Task<List<ShortUrlDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new ShortUrlDto(x.Id, x.OriginalUrl, x.ShortCode, x.CreatedBy, x.CreatedDate))
            .ToList();
    }

    public async Task<ShortUrlDto?> GetByIdAsync(int id)
    {
        var e = await _repo.GetByIdAsync(id);
        return e is null ? null : new ShortUrlDto(e.Id, e.OriginalUrl, e.ShortCode, e.CreatedBy, e.CreatedDate);
    }

    public async Task<(bool ok, string? error, ShortUrlDto? created)> CreateAsync(string originalUrl, string userName)
    {
        if (await _repo.GetByOriginalAsync(originalUrl) is not null)
            return (false, "URL already exists.", null);

        var code = _gen.Generate(originalUrl);
        var retries = 3;
        while (retries-- > 0 && await _repo.GetByCodeAsync(code) is not null)
            code = _gen.Generate(originalUrl + Guid.NewGuid());

        if (await _repo.GetByCodeAsync(code) is not null)
            return (false, "Failed to generate unique short code.", null);

        var entity = new ShortUrl { OriginalUrl = originalUrl, ShortCode = code, CreatedBy = userName };
        var created = await _repo.AddAsync(entity);
        await _repo.SaveAsync();

        return (true, null, new ShortUrlDto(created.Id, created.OriginalUrl, created.ShortCode, created.CreatedBy, created.CreatedDate));
    }

    public async Task<(bool ok, string? error)> DeleteAsync(int id, string userName, bool isAdmin)
    {
        var e = await _repo.GetByIdAsync(id);
        if (e is null) return (false, "Not found");
        if (!isAdmin && !string.Equals(e.CreatedBy, userName, StringComparison.OrdinalIgnoreCase))
            return (false, "Forbidden");

        var ok = await _repo.DeleteAsync(e);
        if (ok) await _repo.SaveAsync();
        return (ok, ok ? null : "Delete failed");
    }
}