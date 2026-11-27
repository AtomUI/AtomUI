namespace AtomUI.Desktop.Controls;

/// <summary>
/// One candidate in the candidate list of a <see cref="Mentions" /> control.
/// Applications supply candidates through <see cref="Mentions.OptionsSource" /> or
/// <see cref="Mentions.OptionsAsyncLoader" />, as <see cref="MentionOption" /> or as a type of their own.
/// Unless the control's <c>FilterValueSelector</c> is set, the first non-null of <see cref="Header" />,
/// <see cref="Value" /> and <see cref="Key" /> is matched against the filter text that narrows the list.
/// </summary>
public interface IMentionOption
{
    /// <summary>
    /// Gets a fallback text that takes part in filtering only when <see cref="Header" /> and <see cref="Value" />
    /// are null. The default option template does not display it, and picking the candidate does not insert it.
    /// </summary>
    string? Key { get; }
    /// <summary>
    /// Gets the text displayed for the candidate in the candidate list. It is also the first text used for
    /// filtering, and the text inserted for a picked candidate when <see cref="Value" /> is null.
    /// </summary>
    object? Header { get; }
    /// <summary>
    /// Gets whether the candidate is enabled. When false, the candidate list disables the candidate's item and
    /// skips the candidate when it pre-selects one as the list opens.
    /// </summary>
    bool IsEnabled { get; }
    /// <summary>
    /// Gets the text inserted into the text area when the candidate is picked. It is used for filtering only when
    /// <see cref="Header" /> is null, and the default option template does not display it.
    /// </summary>
    object? Value { get; }
}

/// <summary>
/// Simple <see cref="IMentionOption" /> implementation for applications that have no model type to implement it on.
/// </summary>
public record MentionOption : IMentionOption
{
    /// <inheritdoc />
    public object? Header { get; init; }
    /// <inheritdoc />
    public bool IsEnabled { get; init; } = true;
    /// <inheritdoc />
    public object? Value { get; init; }
    /// <inheritdoc />
    public string? Key { get; init; }
}
