using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

public class SimpleAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public SimpleAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var header))
            return Task.FromResult(AuthenticateResult.Fail("Missing header"));

        try
        {
            var token = header.ToString().Replace("Basic ", "");
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var parts = decoded.Split(':', 2);
            var login = parts[0];
            var pass = parts.Length > 1 ? parts[1] : "";

            var ok = (login == "admin" && pass == "admin") || (login == "user" && pass == "user");
            if (!ok) return Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, login),
                new Claim(ClaimTypes.Role, login == "admin" ? "Admin" : "User")
            };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            return Task.FromResult(AuthenticateResult.Success(
                new AuthenticationTicket(principal, Scheme.Name)));
        }
        catch
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid header format"));
        }
    }
}