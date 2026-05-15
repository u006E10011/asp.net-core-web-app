namespace DevLog.ViewModels;

public class HomeIndexViewModel
{
    public string? SelectedTag { get; init; }
    public IReadOnlyList<string> AvailableTags { get; init; } = Array.Empty<string>();
    public IReadOnlyList<PostListItemViewModel> Posts { get; init; } = Array.Empty<PostListItemViewModel>();
}
