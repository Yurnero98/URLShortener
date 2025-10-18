namespace UrlShortener.Domain.Entities;

public class AboutInfo
{
    public int Id { get; set; } = 1;        
    public string Content { get; set; } = ""; 
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}