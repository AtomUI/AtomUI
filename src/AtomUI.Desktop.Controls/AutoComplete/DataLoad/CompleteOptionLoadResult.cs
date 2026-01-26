using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public class CompleteOptionLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public RpcErrorCode ErrorCode { get; init; } = RpcErrorCode.Unknown;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<IAutoCompleteOption>? Data { get; init; }
}