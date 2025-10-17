using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using UrlShortener.Application.Abstractions;
using UrlShortener.Application.Services;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

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
            new OpenApiSecurityScheme { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme, Id = "basic" } },
            Array.Empty<string>()
        }
    });
});


// CORS for Angular
builder.Services.AddCors(o => o.AddPolicy("AllowAngular",
    p => p.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200")));

// EF InMemory
builder.Services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase("UrlShortenerDb"));

// Basic Auth
builder.Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, SimpleAuthHandler>("Basic", null);

// DI
builder.Services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
builder.Services.AddSingleton<IShortCodeGenerator, ShaShortCodeGenerator>();
builder.Services.AddScoped<UrlService>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Redirect: /{shortCode}
app.MapGet("/{shortCode}", async (string shortCode, AppDbContext db) =>
{
    var url = await db.ShortUrls.FirstOrDefaultAsync(x => x.ShortCode == shortCode);
    return url is null ? Results.NotFound() : Results.Redirect(url.OriginalUrl);
});

app.Run();