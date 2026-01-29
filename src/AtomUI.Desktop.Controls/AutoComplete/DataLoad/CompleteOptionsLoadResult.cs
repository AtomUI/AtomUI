using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class CompleteOptionsLoadResult
{
    public bool IsSuccess => StatusCode == RpcStatusCode.Success;
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Success;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<IAutoCompleteOption>? Data { get; init; }
}