using DevLog.Data;
using DevLog.Models;
using DevLog.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DevLog.Controllers
{
    public class HomeController(DevLogDbContext dbContext) : Controller
    {
        public async Task<IActionResult> Index(string? tag)
        {
            var normalizedTag = string.IsNullOrWhiteSpace(tag)
                ? null
                : tag.Trim().ToLowerInvariant();

            var postsQuery = dbContext.Posts
                .AsNoTracking()
                .Include(post => post.Tags)
                .OrderByDescending(post => post.CreatedAtUtc)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(normalizedTag))
            {
                postsQuery = postsQuery.Where(post => post.Tags.Any(existingTag => existingTag.Name == normalizedTag));
            }

            var posts = await postsQuery
                .Select(post => new PostListItemViewModel
                {
                    Id = post.Id,
                    Title = post.Title,
                    Content = post.Content,
                    CreatedAtUtc = post.CreatedAtUtc,
                    UpdatedAtUtc = post.UpdatedAtUtc,
                    LikesCount = post.LikesCount,
                    DislikesCount = post.DislikesCount,
                    ImageMimeType = post.ImageMimeType,
                    ImageBase64 = post.ImageData != null ? Convert.ToBase64String(post.ImageData) : null,
                    Tags = post.Tags
                        .OrderBy(existingTag => existingTag.Name)
                        .Select(existingTag => existingTag.Name)
                        .ToList()
                })
                .ToListAsync();

            var model = new HomeIndexViewModel
            {
                SelectedTag = normalizedTag,
                AvailableTags = await dbContext.Tags
                    .AsNoTracking()
                    .OrderBy(existingTag => existingTag.Name)
                    .Select(existingTag => existingTag.Name)
                    .ToListAsync(),
                Posts = posts
            };

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}
