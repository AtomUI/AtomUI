using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class CompleteOptionLoadResult
{
    public bool IsSuccess => ErrorCode == RpcErrorCode.Success;
    public RpcErrorCode ErrorCode { get; init; } = RpcErrorCode.Success;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<IAutoCompleteOption>? Data { get; init; }
}