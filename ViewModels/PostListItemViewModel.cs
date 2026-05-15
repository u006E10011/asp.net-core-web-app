namespace DevLog.ViewModels;

public class PostListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public int LikesCount { get; init; }
    public int DislikesCount { get; init; }
    public string? ImageMimeType { get; init; }
    public string? ImageBase64 { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
