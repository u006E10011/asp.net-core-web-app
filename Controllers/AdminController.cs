using System.Security.Claims;
using DevLog.Configuration;
using DevLog.Data;
using DevLog.Models;
using DevLog.ViewModels.Admin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DevLog.Controllers;

[Route("admin")]
public class AdminController(DevLogDbContext dbContext, IOptions<AdminAuthOptions> adminAuthOptions) : Controller
{
    private readonly AdminAuthOptions _adminAuthOptions = adminAuthOptions.Value;

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new AdminLoginViewModel
        {
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? Url.Action(nameof(Index)) : returnUrl
        });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AdminLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usernameMatches = string.Equals(model.Username, _adminAuthOptions.Username, StringComparison.Ordinal);
        var passwordMatches = string.Equals(model.Password, _adminAuthOptions.Password, StringComparison.Ordinal)
            || BCrypt.Net.BCrypt.Verify(model.Password, _adminAuthOptions.PasswordHash);

        if (!usernameMatches || !passwordMatches)
        {
            ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
            return View(model);
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, _adminAuthOptions.Username),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("logout")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet("")]
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var posts = await dbContext.Posts
            .AsNoTracking()
            .Include(post => post.Tags)
            .OrderByDescending(post => post.CreatedAtUtc)
            .Select(post => new AdminPostListItemViewModel
            {
                Id = post.Id,
                Title = post.Title,
                CreatedAtUtc = post.CreatedAtUtc,
                UpdatedAtUtc = post.UpdatedAtUtc,
                LikesCount = post.LikesCount,
                DislikesCount = post.DislikesCount,
                Tags = post.Tags
                    .OrderBy(existingTag => existingTag.Name)
                    .Select(existingTag => existingTag.Name)
                    .ToList()
            })
            .ToListAsync();

        return View(new AdminIndexViewModel { Posts = posts });
    }

    [HttpGet("create")]
    [Authorize]
    public IActionResult Create()
    {
        return View("Edit", new AdminPostFormViewModel());
    }

    [HttpPost("create")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminPostFormViewModel model)
    {
        if (!await PopulateImageAsync(model))
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Можно загружать только изображения.");
            return View("Edit", model);
        }

        if (!ModelState.IsValid)
        {
            return View("Edit", model);
        }

        var post = new Post
        {
            Title = model.Title.Trim(),
            Content = model.Content.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            LikesCount = model.LikesCount,
            DislikesCount = model.DislikesCount
        };

        await ApplyImageAsync(post, model);
        await ApplyTagsAsync(post, model.TagsInput);

        dbContext.Posts.Add(post);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpGet("edit/{id:int}")]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        var post = await dbContext.Posts
            .AsNoTracking()
            .Include(existingPost => existingPost.Tags)
            .SingleOrDefaultAsync(existingPost => existingPost.Id == id);

        if (post is null)
        {
            return NotFound();
        }

        return View(new AdminPostFormViewModel
        {
            Id = post.Id,
            Title = post.Title,
            Content = post.Content,
            TagsInput = string.Join(", ", post.Tags.OrderBy(tag => tag.Name).Select(tag => $"#{tag.Name}")),
            LikesCount = post.LikesCount,
            DislikesCount = post.DislikesCount,
            HasExistingImage = post.ImageData is not null
        });
    }

    [HttpPost("edit/{id:int}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminPostFormViewModel model)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!await PopulateImageAsync(model))
        {
            ModelState.AddModelError(nameof(model.ImageFile), "Можно загружать только изображения.");
            return View(model);
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var post = await dbContext.Posts
            .Include(existingPost => existingPost.Tags)
            .SingleOrDefaultAsync(existingPost => existingPost.Id == id);

        if (post is null)
        {
            return NotFound();
        }

        post.Title = model.Title.Trim();
        post.Content = model.Content.Trim();
        post.LikesCount = model.LikesCount;
        post.DislikesCount = model.DislikesCount;
        post.UpdatedAtUtc = DateTime.UtcNow;

        await ApplyImageAsync(post, model);
        await ApplyTagsAsync(post, model.TagsInput);

        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost("delete/{id:int}")]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var post = await dbContext.Posts.FindAsync(id);
        if (post is null)
        {
            return NotFound();
        }

        dbContext.Posts.Remove(post);
        await dbContext.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private static async Task<bool> PopulateImageAsync(AdminPostFormViewModel model)
    {
        if (model.ImageFile is null || model.ImageFile.Length == 0)
        {
            return true;
        }

        if (!model.ImageFile.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static async Task ApplyImageAsync(Post post, AdminPostFormViewModel model)
    {
        if (model.RemoveImage)
        {
            post.ImageData = null;
            post.ImageMimeType = null;
            post.ImageFileName = null;
        }

        if (model.ImageFile is null || model.ImageFile.Length == 0)
        {
            return;
        }

        await using var memoryStream = new MemoryStream();
        await model.ImageFile.CopyToAsync(memoryStream);

        post.ImageData = memoryStream.ToArray();
        post.ImageMimeType = model.ImageFile.ContentType;
        post.ImageFileName = model.ImageFile.FileName;
    }

    private async Task ApplyTagsAsync(Post post, string rawTags)
    {
        post.Tags.Clear();

        var normalizedTags = rawTags
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(tag => tag.Trim())
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.StartsWith('#') ? tag[1..] : tag)
            .Select(tag => tag.Trim().ToLowerInvariant())
            .Where(tag => tag.Length > 0)
            .Distinct()
            .ToList();

        if (normalizedTags.Count == 0)
        {
            return;
        }

        var existingTags = await dbContext.Tags
            .Where(tag => normalizedTags.Contains(tag.Name))
            .ToDictionaryAsync(tag => tag.Name);

        foreach (var tagName in normalizedTags)
        {
            if (!existingTags.TryGetValue(tagName, out var tag))
            {
                tag = new Tag { Name = tagName };
                existingTags[tagName] = tag;
            }

            post.Tags.Add(tag);
        }
    }
}
