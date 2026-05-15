using System.ComponentModel.DataAnnotations;

namespace DevLog.Models;

public class Post
{
    public int Id { get; set; }

    [Required]
    [StringLength(160)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(10000)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    [Range(0, int.MaxValue)]
    public int LikesCount { get; set; }

    [Range(0, int.MaxValue)]
    public int DislikesCount { get; set; }

    [StringLength(255)]
    public string? ImageFileName { get; set; }

    [StringLength(120)]
    public string? ImageMimeType { get; set; }

    public byte[]? ImageData { get; set; }

    public ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
