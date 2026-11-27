namespace AtomUI.Controls.Data;

/// <summary>
/// Entry-level view of a <c>ListCollectionView</c>, used by the ListView selection model in AtomUI.Desktop.Controls.
/// </summary>
/// <remarks>
/// <c>ListCollectionView</c> implements the interface explicitly. It translates between the index in the source
/// collection, the index in the projected view and the stable entry id, so that the selection can follow an entry.
/// <c>EntryChangePrepared</c> is raised once a source change has been recorded in the entry store and before the view
/// handles the public collection change; <c>EntryChangeCommitted</c> is raised after that handling, also when it threw.
/// <c>ProjectionCommitted</c> is raised when the view has committed a new projection of its entries.
/// </remarks>
internal interface IListCollectionEntryView
{
    int SourceEntryCount { get; }
    IReadOnlyList<ListCollectionEntry> LogicalEntries { get; }

    bool TryGetSourceEntry(int sourceIndex, out ListCollectionEntry? entry);
    bool TryGetViewNode(int viewIndex, out ListCollectionViewNode? node);
    bool TryGetSourceIndex(long entryId, out int sourceIndex);
    bool TryGetViewIndex(long entryId, out int viewIndex);

    event EventHandler<ListCollectionEntryChangeEventArgs>? EntryChangePrepared;
    event EventHandler? EntryChangeCommitted;
    event EventHandler? ProjectionCommitted;
}
