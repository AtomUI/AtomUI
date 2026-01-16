using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public record CascaderItemLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public RpcErrorCode ErrorCode { get; init; } = RpcErrorCode.Unknown;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<ICascaderViewItemData>? Data { get; init; }
}