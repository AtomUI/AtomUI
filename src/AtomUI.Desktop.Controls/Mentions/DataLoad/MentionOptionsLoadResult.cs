using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

public record MentionOptionsLoadResult
{
    public bool IsSuccess => StatusCode == RpcStatusCode.Success;
    public RpcStatusCode StatusCode { get; init; } = RpcStatusCode.Success;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<IMentionOption>? Data { get; init; }
}