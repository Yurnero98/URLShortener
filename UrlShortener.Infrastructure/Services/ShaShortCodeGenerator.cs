using System.Security.Cryptography;
using System.Text;
using UrlShortener.Application.Abstractions;

namespace UrlShortener.Infrastructure.Services;

public class ShaShortCodeGenerator : IShortCodeGenerator
{
    public string Generate(string input)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 8);
    }
}