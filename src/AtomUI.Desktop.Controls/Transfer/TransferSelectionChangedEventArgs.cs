using AtomUI.Controls;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Carries the keys of the items that a transfer lists in its panels after it rebuilt one or both of them. The
/// lists hold the keys of the listed items, not of the checked items.
/// </summary>
public class TransferSelectionChangedEventArgs : EventArgs
{
    /// <summary>
    /// The keys of the items listed in the source panel, or <see langword="null" /> when the source panel was not
    /// rebuilt in the same pass.
    /// </summary>
    public IList<EntityKey>? SourceSelectedKeys { get; }
    /// <summary>
    /// The keys of the items listed in the target panel, or <see langword="null" /> when the target panel was not
    /// rebuilt in the same pass.
    /// </summary>
    public IList<EntityKey>? TargetSelectedKeys { get; }

    /// <summary>
    /// Initializes the event data with the keys of the items listed in the two panels.
    /// </summary>
    /// <param name="sourceSelectedKeys">
    /// The keys of the items listed in the source panel, or <see langword="null" /> when it was not rebuilt.
    /// </param>
    /// <param name="targetSelectedKeys">
    /// The keys of the items listed in the target panel, or <see langword="null" /> when it was not rebuilt.
    /// </param>
    public TransferSelectionChangedEventArgs(IList<EntityKey>? sourceSelectedKeys, IList<EntityKey>? targetSelectedKeys)
    {
        SourceSelectedKeys = sourceSelectedKeys;
        TargetSelectedKeys = targetSelectedKeys;
    }
}