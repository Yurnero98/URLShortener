using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace UrlShortener.WebApi.Pages.About;

[ValidateAntiForgeryToken]
public class IndexModel(IWebHostEnvironment env) : PageModel
{
    private const string FileName = "about.md";

    public string DisplayHtml { get; private set; } = "";
    public string EditContent { get; private set; } = "";
    public string UpdatedLocal { get; private set; } = "";
    public string? StatusMessage { get; private set; }

    [BindProperty]
    public string AboutContent { get; set; } = "";

    private string GetFilePath() => Path.Combine(env.WebRootPath, FileName);

    public async Task<IActionResult> OnGetAsync()
    {
        var path = GetFilePath();

        if (!System.IO.File.Exists(path))
        {
            DisplayHtml = "<p><em>about.md file not found in wwwroot.</em></p>";
            EditContent = "";
            UpdatedLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            return Page();
        }

        var markdown = await System.IO.File.ReadAllTextAsync(path);
        DisplayHtml = Markdown.ToHtml(markdown);
        EditContent = markdown;
        UpdatedLocal = System.IO.File.GetLastWriteTime(path).ToLocalTime().ToString("yyyy-MM-dd HH:mm");

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!(User?.Identity?.IsAuthenticated == true && User.IsInRole("Admin")))
            return Forbid();

        if (string.IsNullOrWhiteSpace(AboutContent))
        {
            StatusMessage = "Content must not be empty.";
            return await OnGetAsync();
        }

        var path = GetFilePath();

        if (!Directory.Exists(env.WebRootPath))
            Directory.CreateDirectory(env.WebRootPath);

        await System.IO.File.WriteAllTextAsync(path, AboutContent.Trim());
        StatusMessage = "Saved successfully.";

        return await OnGetAsync();
    }
}