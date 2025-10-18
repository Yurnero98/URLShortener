namespace UrlShortener.Domain.Entities;

public class ShortUrl
{
    public int Id { get; set; }
    public string OriginalUrl { get; set; } = null!;
    public string ShortCode { get; set; } = null!;
    public string CreatedBy { get; set; } = null!;
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}