namespace UrlShortener.Application.Interfaces;

public interface IShortCodeGenerator
{
    string Generate(string input);
}