using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Tests;

[TestClass]
public class UrlServiceTests
{
    private static AppDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static UrlService NewService(AppDbContext db)
    {
        var repo = new ShortUrlRepository(db);
        var gen = new ShaShortCodeGenerator();
        return new UrlService(repo, gen);
    }

    private const string Yt1 =
        "https://www.youtube.com/watch?v=_AAdae7diOU&list=RD_AAdae7diOU&index=1";
    private const string Yt8 =
        "https://www.youtube.com/watch?v=wL8DVHuWI7Y&list=RD_AAdae7diOU&index=8";
    private const string GDrive =
        "https://drive.google.com/file/d/1yLTwR1_5aYbpiB_p-dsQMtkie_UJ0cwK/view";

    [TestMethod]
    public async Task Create_Succeeds_AndGenerates8CharCode()
    {
        using var db = NewDb();
        var svc = NewService(db);

        var (ok, err, created) = await svc.CreateAsync(Yt8, "admin");

        Assert.IsTrue(ok);
        Assert.IsNull(err);
        Assert.IsNotNull(created);
        Assert.AreEqual(Yt8, created!.OriginalUrl);
        Assert.IsFalse(string.IsNullOrWhiteSpace(created.ShortCode));
        Assert.AreEqual(8, created.ShortCode.Length, "ShortCode must be 8 chars");
        Assert.AreEqual("admin", created.CreatedBy);
    }

    [TestMethod]
    public async Task Create_Fails_OnDuplicateOriginal()
    {
        using var db = NewDb();
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = Yt8, ShortCode = "ABC12345", CreatedBy = "user" });
        await db.SaveChangesAsync(TestContext.CancellationToken);

        var svc = NewService(db);
        var (ok, err, created) = await svc.CreateAsync(Yt8, "user");

        Assert.IsFalse(ok);
        Assert.IsNull(created);
        Assert.AreEqual("URL already exists.", err);
    }

    [TestMethod]
    public async Task GetById_ReturnsDto_WhenExists()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = GDrive, ShortCode = "DRIVE001", CreatedBy = "user" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync(TestContext.CancellationToken);

        var svc = NewService(db);
        var dto = await svc.GetByIdAsync(e.Id);

        Assert.IsNotNull(dto);
        Assert.AreEqual(e.Id, dto!.Id);
        Assert.AreEqual(GDrive, dto.OriginalUrl);
    }

    [TestMethod]
    public async Task GetById_ReturnsNull_WhenNotFound()
    {
        using var db = NewDb();
        var svc = NewService(db);

        var dto = await svc.GetByIdAsync(999);

        Assert.IsNull(dto);
    }

    [TestMethod]
    public async Task GetAll_ReturnsDescendingByCreatedDate()
    {
        using var db = NewDb();
        db.ShortUrls.Add(new ShortUrl
        {
            OriginalUrl = GDrive,
            ShortCode = "DRIVE002",
            CreatedBy = "u",
            CreatedDate = DateTime.UtcNow.AddMinutes(-10)
        });
        db.ShortUrls.Add(new ShortUrl
        {
            OriginalUrl = Yt1,
            ShortCode = "YTINDX01",
            CreatedBy = "u",
            CreatedDate = DateTime.UtcNow.AddMinutes(-5)
        });
        db.ShortUrls.Add(new ShortUrl
        {
            OriginalUrl = Yt8,
            ShortCode = "YTINDX08",
            CreatedBy = "u",
            CreatedDate = DateTime.UtcNow
        });
        await db.SaveChangesAsync(TestContext.CancellationToken);

        var svc = NewService(db);
        var list = await svc.GetAllAsync();

        Assert.HasCount(3, list);
        Assert.AreEqual(Yt8, list[0].OriginalUrl);
        Assert.AreEqual(Yt1, list[1].OriginalUrl);
        Assert.AreEqual(GDrive, list[2].OriginalUrl);
    }

    [TestMethod]
    public async Task Delete_ReturnsNotFound_WhenIdMissing()
    {
        using var db = NewDb();
        var svc = NewService(db);

        var (ok, err) = await svc.DeleteAsync(123, "user", isAdmin: false);

        Assert.IsFalse(ok);
        Assert.AreEqual("Not found", err);
    }

    [TestMethod]
    public async Task Delete_ForUser_OwnItem_Succeeds()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = Yt1, ShortCode = "MINE0001", CreatedBy = "user" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync(TestContext.CancellationToken);

        var svc = NewService(db);
        var (ok, err) = await svc.DeleteAsync(e.Id, "user", isAdmin: false);

        Assert.IsTrue(ok);
        Assert.IsNull(err);
        Assert.AreEqual(0, await db.ShortUrls.CountAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Delete_ForUser_OthersItem_Forbidden()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = GDrive, ShortCode = "THEIRS01", CreatedBy = "owner" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var (ok, err) = await svc.DeleteAsync(e.Id, "notowner", isAdmin: false);

        Assert.IsFalse(ok);
        Assert.AreEqual("Forbidden", err);
        Assert.AreEqual(1, await db.ShortUrls.CountAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Delete_ForAdmin_OthersItem_Succeeds()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = Yt8, ShortCode = "ANY00001", CreatedBy = "someone" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var (ok, err) = await svc.DeleteAsync(e.Id, "admin", isAdmin: true);

        Assert.IsTrue(ok);
        Assert.IsNull(err);
        Assert.AreEqual(0, await db.ShortUrls.CountAsync(TestContext.CancellationToken));
    }

    public TestContext TestContext { get; set; }
}