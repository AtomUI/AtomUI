using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public record TreeItemLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public RpcErrorCode ErrorCode { get; init; } = RpcErrorCode.Unknown;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<ITreeViewItemData>? Data { get; init; }
}