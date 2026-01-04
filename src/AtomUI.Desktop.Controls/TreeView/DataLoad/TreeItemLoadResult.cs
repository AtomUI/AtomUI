namespace AtomUI.Desktop.Controls;

public record TreeItemLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public TreeItemLoadErrorCode ErrorCode { get; init; } = TreeItemLoadErrorCode.None;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<ITreeViewItemData>? Data { get; init; }
}