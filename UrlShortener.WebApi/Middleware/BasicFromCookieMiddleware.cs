namespace UrlShortener.WebApi.Middleware;

public class BasicFromCookieMiddleware
{
    private readonly RequestDelegate _next;
    public BasicFromCookieMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext ctx)
    {
        if (!ctx.Request.Headers.ContainsKey("Authorization") &&
            ctx.Request.Cookies.TryGetValue("auth_basic", out var val) &&
            !string.IsNullOrWhiteSpace(val))
        {
            ctx.Request.Headers.Append("Authorization", $"Basic {val}");
        }
        await _next(ctx);
    }
}