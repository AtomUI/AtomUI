namespace AtomUI.Desktop.Controls;

public interface IMentionOption
{
    string? Key { get; }
    string Header { get; }
    bool IsEnabled { get; }
    object? Value { get; }
}

public record MentionOption : IMentionOption
{
    public string Header { get; init; } = string.Empty;
    public bool IsEnabled { get; init; } = true;
    public object? Value { get; init; }
    public string? Key { get; init; }
}
