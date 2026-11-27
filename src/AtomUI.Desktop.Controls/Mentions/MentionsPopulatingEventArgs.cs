using System.ComponentModel;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Data for the <see cref="Mentions.Populating" /> event, which the control raises before it filters its
/// candidates for the current text; it is not raised while <see cref="Mentions.OptionsAsyncLoader" /> is set.
/// Setting <see cref="CancelEventArgs.Cancel" /> to <see langword="true" /> keeps the control from filtering
/// and from raising <see cref="Mentions.Populated" />. After cancelling, the handler changes the candidates
/// itself and calls <see cref="Mentions.PopulateComplete" /> to show them.
/// </summary>
public class MentionsPopulatingEventArgs : CancelEventArgs
{
    /// <summary>
    /// The text typed after the trigger character, which the candidates are matched against; it is
    /// <see langword="null" /> when there is none.
    /// </summary>
    public string? Predicate { get; }

    /// <summary>
    /// Creates the arguments for a populate run.
    /// </summary>
    /// <param name="predicate">
    /// The text to match the candidates against; stored as <see cref="Predicate" />.
    /// </param>
    public MentionsPopulatingEventArgs(string? predicate)
    {
        Predicate = predicate;
    }
}