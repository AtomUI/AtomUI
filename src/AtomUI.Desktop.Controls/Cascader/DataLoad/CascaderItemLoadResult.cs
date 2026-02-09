using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public record CascaderItemLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Unknown;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<ICascaderOption>? Data { get; init; }
}