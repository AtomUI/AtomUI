namespace AtomUI.Desktop.Controls;

public interface IMentionOption
{
    string? Key { get; }
    object? Header { get; }
    bool IsEnabled { get; }
    object? Value { get; }
}

public record MentionOption : IMentionOption
{
    public object? Header { get; init; }
    public bool IsEnabled { get; init; } = true;
    public object? Value { get; init; }
    public string? Key { get; init; }
}
