using System.Security.Cryptography;
using System.Text;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Services;

public class ShaShortCodeGenerator : IShortCodeGenerator
{
    public string Generate(string input)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToBase64String(hash).Replace("+", "").Replace("/", "").Replace("=", "").Substring(0, 8);
    }
}