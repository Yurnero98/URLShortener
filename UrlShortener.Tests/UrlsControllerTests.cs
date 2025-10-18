using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Dtos;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Repositories;
using UrlShortener.Infrastructure.Services;
using UrlShortener.WebApi.Controllers;

namespace UrlShortener.Tests;

[TestClass]
public class UrlsControllerTests
{
    private const string Yt1 =
        "https://www.youtube.com/watch?v=_AAdae7diOU&list=RD_AAdae7diOU&index=1";
    private const string Yt8 =
        "https://www.youtube.com/watch?v=wL8DVHuWI7Y&list=RD_AAdae7diOU&index=8";
    private const string GDrive =
        "https://drive.google.com/file/d/1yLTwR1_5aYbpiB_p-dsQMtkie_UJ0cwK/view";

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

    private static UrlsController NewController(UrlService svc, string? userName = null, params string[] roles)
    {
        var ctrl = new UrlsController(svc);

        var claims = new List<Claim>();
        if (!string.IsNullOrWhiteSpace(userName))
            claims.Add(new Claim(ClaimTypes.Name, userName));

        foreach (var r in roles)
            claims.Add(new Claim(ClaimTypes.Role, r));

        var identity = new ClaimsIdentity(claims, authenticationType: "TestAuth");
        var user = new ClaimsPrincipal(identity);

        ctrl.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return ctrl;
    }

    [TestMethod]
    public async Task GetAll_Returns_Ok_With_Sorted_List()
    {
        using var db = NewDb();
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = GDrive, ShortCode = "DRV00001", CreatedBy = "u", CreatedDate = DateTime.UtcNow.AddMinutes(-10) });
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = Yt1, ShortCode = "YTI00001", CreatedBy = "u", CreatedDate = DateTime.UtcNow.AddMinutes(-5) });
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = Yt8, ShortCode = "YTI00008", CreatedBy = "u", CreatedDate = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var ctrl = NewController(svc);

        var result = await ctrl.GetAll() as OkObjectResult;
        Assert.IsNotNull(result);

        var list = result.Value as List<ShortUrlDto>;
        Assert.IsNotNull(list);
        Assert.HasCount(3, list);
        Assert.AreEqual(Yt8, list[0].OriginalUrl);
        Assert.AreEqual(Yt1, list[1].OriginalUrl);
        Assert.AreEqual(GDrive, list[2].OriginalUrl);
    }

    [TestMethod]
    public async Task Get_Returns_Ok_When_Found()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = Yt8, ShortCode = "YT8OK000", CreatedBy = "user" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var ctrl = NewController(svc);

        var result = await ctrl.Get(e.Id) as OkObjectResult;
        Assert.IsNotNull(result);

        var dto = result.Value as ShortUrlDto;
        Assert.IsNotNull(dto);
        Assert.AreEqual(e.Id, dto!.Id);
        Assert.AreEqual(Yt8, dto.OriginalUrl);
    }

    [TestMethod]
    public async Task Get_Returns_NotFound_When_Missing()
    {
        using var db = NewDb();
        var svc = NewService(db);
        var ctrl = NewController(svc);

        var result = await ctrl.Get(999);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
    }

    [TestMethod]
    public async Task Create_Returns_Ok_With_CreatedDto()
    {
        using var db = NewDb();
        var svc = NewService(db);
        var ctrl = NewController(svc, userName: "admin"); // [Authorize] 

        var req = new CreateShortUrlRequest(Yt1);

        var action = await ctrl.Create(req) as OkObjectResult;
        Assert.IsNotNull(action);

        var created = action.Value as ShortUrlDto;
        Assert.IsNotNull(created);
        Assert.AreEqual(Yt1, created!.OriginalUrl);
        Assert.AreEqual("admin", created.CreatedBy);
        Assert.AreEqual(8, created.ShortCode.Length);
    }

    [TestMethod]
    public async Task Create_Returns_BadRequest_On_Duplicate()
    {
        using var db = NewDb();
        db.ShortUrls.Add(new ShortUrl { OriginalUrl = GDrive, ShortCode = "DRV00002", CreatedBy = "seed" });
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var ctrl = NewController(svc, userName: "user");

        var req = new CreateShortUrlRequest(GDrive);

        var action = await ctrl.Create(req) as BadRequestObjectResult;
        Assert.IsNotNull(action);
        Assert.AreEqual("URL already exists.", action!.Value);
    }

    [TestMethod]
    public async Task Delete_Returns_Ok_For_Owner()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = Yt1, ShortCode = "OWNR0001", CreatedBy = "ivan" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var ctrl = NewController(svc, userName: "ivan");

        var action = await ctrl.Delete(e.Id);
        Assert.IsInstanceOfType(action, typeof(OkResult));
        Assert.AreEqual(0, await db.ShortUrls.CountAsync());
    }

    [TestMethod]
    public async Task Delete_Returns_Forbid_For_NotOwner_And_NotAdmin()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = GDrive, ShortCode = "NRADM001", CreatedBy = "owner" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var ctrl = NewController(svc, userName: "notowner");

        var action = await ctrl.Delete(e.Id);
        Assert.IsInstanceOfType(action, typeof(ForbidResult));
        Assert.AreEqual(1, await db.ShortUrls.CountAsync());
    }

    [TestMethod]
    public async Task Delete_Returns_Ok_For_Admin_Deleting_Others_Item()
    {
        using var db = NewDb();
        var e = new ShortUrl { OriginalUrl = Yt8, ShortCode = "ADMDEL01", CreatedBy = "someone" };
        db.ShortUrls.Add(e);
        await db.SaveChangesAsync();

        var svc = NewService(db);
        var ctrl = NewController(svc, userName: "admin", roles: "Admin");

        var action = await ctrl.Delete(e.Id);
        Assert.IsInstanceOfType(action, typeof(OkResult));
        Assert.AreEqual(0, await db.ShortUrls.CountAsync());
    }

    [TestMethod]
    public async Task Delete_Returns_NotFound_When_Id_Missing()
    {
        using var db = NewDb();
        var svc = NewService(db);
        var ctrl = NewController(svc, userName: "any");

        var action = await ctrl.Delete(123456);
        Assert.IsInstanceOfType(action, typeof(NotFoundResult));
    }
}