namespace AtomUI.Desktop.Controls;

public record TreeNodeLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public TreeNodeLoadErrorCode ErrorCode { get; init; } = TreeNodeLoadErrorCode.None;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<ITreeViewItemData>? Data { get; init; }
}