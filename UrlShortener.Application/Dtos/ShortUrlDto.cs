namespace UrlShortener.Application.Dtos;

public record ShortUrlDto(int Id, string OriginalUrl, string ShortCode, string CreatedBy, DateTime CreatedDate);