namespace AtomUI.Desktop.Controls;

public record MentionOptionLoadResult
{
    public bool IsSuccess { get; init; } = true;
    public MentionOptionLoadErrorCode ErrorCode { get; init; } = MentionOptionLoadErrorCode.None;
    public string? UserFriendlyMessage { get; init; }
    public IReadOnlyList<IMentionOption>? Data { get; init; }
}