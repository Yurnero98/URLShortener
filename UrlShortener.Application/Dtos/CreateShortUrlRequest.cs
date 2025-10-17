using System.ComponentModel.DataAnnotations;

namespace UrlShortener.Application.Dtos;

public record CreateShortUrlRequest( [Required, Url] string OriginalUrl);