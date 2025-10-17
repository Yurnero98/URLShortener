using Microsoft.EntityFrameworkCore;
using Moq;
using UrlShortener.Application.Services;
using UrlShortener.Domain;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Application.Abstractions;

[TestClass]
public class ShortCodeCollisionTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [TestMethod]
    public async Task Create_Retries_OnShortCodeCollision_AndSucceeds()
    {
        using var db = NewDb();
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = "https://x.com/a", ShortCode = "DUPLCODE", CreatedBy = "seed" });
        await db.SaveChangesAsync();

        var gen = new Mock<IShortCodeGenerator>();
        gen.SetupSequence(g => g.Generate(It.IsAny<string>()))
           .Returns("DUPLCODE")
           .Returns("DUPLCODE")
           .Returns("UNIQ0001");

        var repo = new ShortUrlRepository(db);
        var svc = new UrlService(repo, gen.Object);

        var (ok, err, created) = await svc.CreateAsync("https://example.com/ok", "user");

        Assert.IsTrue(ok);
        Assert.IsNull(err);
        Assert.IsNotNull(created);
        Assert.AreEqual("UNIQ0001", created!.ShortCode);
    }

    [TestMethod]
    public async Task Create_Fails_AfterMaxRetries_OnShortCodeCollision()
    {
        using var db = NewDb();
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = "https://x.com/a", ShortCode = "DUPLCODE", CreatedBy = "seed" });
        await db.SaveChangesAsync();

        var gen = new Mock<IShortCodeGenerator>();
        gen.SetupSequence(g => g.Generate(It.IsAny<string>()))
           .Returns("DUPLCODE")
           .Returns("DUPLCODE")
           .Returns("DUPLCODE")
           .Returns("DUPLCODE");

        var repo = new ShortUrlRepository(db);
        var svc = new UrlService(repo, gen.Object);

        var (ok, err, created) = await svc.CreateAsync("https://example.com/fail", "user");

        Assert.IsFalse(ok);
        Assert.IsNull(created);
        Assert.AreEqual("Failed to generate unique short code.", err);
    }
}