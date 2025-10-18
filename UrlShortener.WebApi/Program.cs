using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Services;
using UrlShortener.WebApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

// MVC + Razor
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddAntiforgery();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UrlShortener API", Version = "v1" });

    c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "basic",
        Description = "Login: admin/user, Password: admin/user"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "basic" }
            },
            Array.Empty<string>()
        }
    });
});

// CORS for Angular 
builder.Services.AddCors(o => o.AddPolicy("AllowAngular", p =>
    p.WithOrigins("http://localhost:4200")
     .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials()
));

// EF InMemory
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseInMemoryDatabase("UrlShortenerDb"));

// Basic Auth
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, SimpleAuthHandler>("Basic", _ => { });

builder.Services.AddAuthorization();

// DI
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddSingleton<IShortCodeGenerator, ShaShortCodeGenerator>();
builder.Services.AddScoped<UrlService>();

var app = builder.Build();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCors("AllowAngular");

app.UseMiddleware<BasicFromCookieMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

app.MapGet("/{shortCode:regex(^[A-Za-z0-9_-]+$):length(4,32)}",
    async (string shortCode, AppDbContext db) =>
    {
        var url = await db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == shortCode);
        return url is null ? Results.NotFound() : Results.Redirect(url.OriginalUrl);
    });

app.Run();