using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class SelectOptionsLoadResult
{
    public bool IsSuccess => StatusCode == RpcStatusCode.Success;
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Success;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<ISelectOption>? Data { get; init; }
}