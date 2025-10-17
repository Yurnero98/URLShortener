using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Dtos;
using UrlShortener.Application.Services;

namespace UrlShortener.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UrlsController(UrlService service) : ControllerBase
{
    private readonly UrlService _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
        => (await _service.GetByIdAsync(id)) is { } dto ? Ok(dto) : NotFound();

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateShortUrlRequest req)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);
        var user = User.Identity?.Name ?? "unknown";
        var (ok, error, created) = await _service.CreateAsync(req.OriginalUrl, user);
        return ok ? Ok(created) : BadRequest(error);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        var user = User.Identity?.Name ?? "unknown";
        var isAdmin = User.IsInRole("Admin");
        var (ok, error) = await _service.DeleteAsync(id, user, isAdmin);
        if (!ok && error == "Forbidden") return Forbid();
        return ok ? Ok() : NotFound();
    }
}