namespace DevLog.ViewModels.Admin;

public class AdminPostListItemViewModel
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? UpdatedAtUtc { get; init; }
    public int LikesCount { get; init; }
    public int DislikesCount { get; init; }
    public IReadOnlyList<string> Tags { get; init; } = Array.Empty<string>();
}
