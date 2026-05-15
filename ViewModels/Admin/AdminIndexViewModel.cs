namespace DevLog.ViewModels.Admin;

public class AdminIndexViewModel
{
    public IReadOnlyList<AdminPostListItemViewModel> Posts { get; init; } = Array.Empty<AdminPostListItemViewModel>();
}
