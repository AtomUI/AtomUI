namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Mentions.Populated" /> event, which the control raises after it filtered its
/// candidates for the current text and handed them to the candidate list.
/// </summary>
public class MentionsPopulatedEventArgs
{
    /// <summary>
    /// The candidates that passed the filter for the current text; the popup lists this same instance. It is
    /// <see langword="null" /> when the control has no option source.
    /// </summary>
    public IList<IMentionOption>? Options { get; init; }
    
    /// <summary>
    /// Creates the arguments for a populated candidate list.
    /// </summary>
    /// <param name="options">
    /// The filtered candidates; stored as <see cref="Options" />.
    /// </param>
    public MentionsPopulatedEventArgs(IList<IMentionOption>? options)
    {
        Options = options;
    }
}