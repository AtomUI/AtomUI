using System.Collections.Specialized;

namespace AtomUI.Controls.Data;

/// <summary>
/// One change of the entries of a list collection view, in terms of entries instead of raw items: the
/// collection change action, the starting indexes, the removed entries as snapshots and the added entries.
/// <c>StartsNewLifecycle</c> is <see langword="true" /> only for <c>Reset</c>, which replaces every entry.
/// <c>ListCollectionEntryStore</c> creates it; the selection model of <c>ListView</c> in
/// AtomUI.Desktop.Controls consumes it to keep the selection in step with the entries.
/// </summary>
internal sealed class ListCollectionEntryChangeSet
{
    public NotifyCollectionChangedAction Action { get; }
    public int OldStartingIndex { get; }
    public int NewStartingIndex { get; }
    public IReadOnlyList<ListCollectionEntrySnapshot> OldEntries { get; }
    public IReadOnlyList<ListCollectionEntry> NewEntries { get; }
    public bool StartsNewLifecycle { get; }

    public ListCollectionEntryChangeSet(
        NotifyCollectionChangedAction action,
        int oldStartingIndex,
        int newStartingIndex,
        IReadOnlyList<ListCollectionEntrySnapshot> oldEntries,
        IReadOnlyList<ListCollectionEntry> newEntries,
        bool startsNewLifecycle)
    {
        Action           = action;
        OldStartingIndex = oldStartingIndex;
        NewStartingIndex = newStartingIndex;
        OldEntries       = oldEntries;
        NewEntries       = newEntries;
        StartsNewLifecycle = startsNewLifecycle;
    }
}

/// <summary>
/// Carries a <see cref="ListCollectionEntryChangeSet" /> to the <c>EntryChangePrepared</c> event of an entry view.
/// The event is raised once a source change is recorded in the entry store and before the view handles the
/// public collection change. <c>ListView</c> in AtomUI.Desktop.Controls handles it to keep its selection in step.
/// </summary>
internal sealed class ListCollectionEntryChangeEventArgs : EventArgs
{
    public ListCollectionEntryChangeSet ChangeSet { get; }

    public ListCollectionEntryChangeEventArgs(ListCollectionEntryChangeSet changeSet)
    {
        ChangeSet = changeSet;
    }
}
