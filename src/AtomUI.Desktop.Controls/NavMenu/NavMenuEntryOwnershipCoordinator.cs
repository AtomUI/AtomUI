using System.Collections;
using System.Collections.Specialized;
using Avalonia.Collections;

namespace AtomUI.Desktop.Controls;

internal sealed class NavMenuEntryOwnershipCoordinator
{
    private readonly object _owner;
    private readonly Func<IEnumerable> _rootEntriesAccessor;
    private readonly HashSet<INavMenuEntry> _ownedEntries = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<INotifyCollectionChanged, IDisposable> _sourceSubscriptions =
        new(ReferenceEqualityComparer.Instance);

    // Index of the first occurrence of every tracked direct entry, valid only while the last applied
    // reconcile succeeded and its scope held no custom node. In that state every tracked entry in the
    // scope is a direct entry owned by this coordinator's owner, so appending built-in entries can be
    // validated against this index alone instead of rescanning the whole scope on every addition.
    private readonly Dictionary<INavMenuEntry, int> _directEntryIndexes = new(ReferenceEqualityComparer.Instance);
    private int _directEntryCount;
    private bool _isDirectScopeIndexed = true;

    public NavMenuEntryOwnershipCoordinator(object owner, Func<IEnumerable> rootEntriesAccessor)
    {
        _owner = owner;
        _rootEntriesAccessor = rootEntriesAccessor;
    }

    public void Validate(IEnumerable rootEntries)
    {
        Process(rootEntries, false);
    }

    public void Synchronize()
    {
        Process(_rootEntriesAccessor(), true);
    }

    /// <summary>
    /// Reconciles after a post-mutation root change, validating only the appended entries when the
    /// change is a pure append of built-in entries to an indexed direct scope.
    /// </summary>
    public void Synchronize(NotifyCollectionChangedEventArgs change)
    {
        if (change.Action == NotifyCollectionChangedAction.Add &&
            change.NewItems is { } newItems &&
            TryAppend(newItems, change.NewStartingIndex, true))
        {
            return;
        }

        Synchronize();
    }

    /// <summary>
    /// Validates appending <paramref name="entries"/> at <paramref name="startIndex"/> before the
    /// mutation. Returns <c>false</c> when the append is not eligible for incremental validation and
    /// the caller must validate the prospective scope instead.
    /// </summary>
    public bool TryValidateAppend(IList entries, int startIndex)
    {
        return TryAppend(entries, startIndex, false);
    }

    /// <summary>
    /// Applies ownership for entries already appended at <paramref name="startIndex"/>. Returns
    /// <c>false</c> when the append is not eligible and the caller must run <see cref="Synchronize()"/>.
    /// </summary>
    public bool TrySynchronizeAppend(IList entries, int startIndex)
    {
        return TryAppend(entries, startIndex, true);
    }

    private bool TryAppend(IList entries, int startIndex, bool apply)
    {
        if (!_isDirectScopeIndexed ||
            entries.Count == 0 ||
            startIndex != _directEntryCount ||
            _rootEntriesAccessor() is not IList currentEntries ||
            currentEntries.Count != startIndex + (apply ? entries.Count : 0))
        {
            return false;
        }

        for (var offset = 0; offset < entries.Count; offset++)
        {
            var item = entries[offset];
            // Custom nodes open nested scopes and invalid items must report through the full scan,
            // so only built-in entries take the incremental path.
            if (item is not (NavMenuNode or NavMenuGroup or NavMenuDivider) ||
                (apply && !ReferenceEquals(currentEntries[startIndex + offset], item)))
            {
                return false;
            }
        }

        // Appended entries come last in scope order, so the existing occurrence is always the first
        // one and the same exceptions as the full reconcile are produced in the same precedence.
        InvalidOperationException? validationException = null;
        List<(INavMenuEntry Entry, int Index)>? appendedEntries = null;
        Dictionary<INavMenuEntry, int>? appendedIndexes = null;
        for (var offset = 0; offset < entries.Count; offset++)
        {
            var entry = (INavMenuEntry)entries[offset]!;
            var index = startIndex + offset;
            if (!NavMenuEntryOwnership.IsTracked(entry))
            {
                continue;
            }

            if (_directEntryIndexes.TryGetValue(entry, out var firstIndex) ||
                (appendedIndexes is not null && appendedIndexes.TryGetValue(entry, out firstIndex)))
            {
                validationException ??= NavMenuEntryOwnership.CreateDuplicateException(
                    entry,
                    _owner,
                    firstIndex,
                    index);
                continue;
            }

            (appendedIndexes ??= new Dictionary<INavMenuEntry, int>(ReferenceEqualityComparer.Instance))
                .Add(entry, index);
            (appendedEntries ??= []).Add((entry, index));
        }

        if (appendedEntries is not null)
        {
            foreach (var (entry, _) in appendedEntries)
            {
                if (NavMenuEntryOwnership.TryGetOwner(entry, out var currentOwner) &&
                    !ReferenceEquals(currentOwner, _owner))
                {
                    validationException ??= NavMenuEntryOwnership.CreateAlreadyAttachedException(
                        entry,
                        _owner,
                        currentOwner!);
                }
            }
        }

        if (validationException is not null)
        {
            if (apply)
            {
                // The invalid entry stays in the post-mutation scope; later changes must rescan it.
                InvalidateDirectScopeIndex();
            }

            throw validationException;
        }

        if (!apply)
        {
            return true;
        }

        if (appendedEntries is not null)
        {
            foreach (var (entry, index) in appendedEntries)
            {
                if (!NavMenuEntryOwnership.TryGetOwner(entry, out _))
                {
                    NavMenuEntryOwnership.Attach(entry, _owner);
                }

                _ownedEntries.Add(entry);
                _directEntryIndexes.Add(entry, index);
            }
        }

        _directEntryCount += entries.Count;
        return true;
    }

    private void InvalidateDirectScopeIndex()
    {
        _isDirectScopeIndexed = false;
        _directEntryIndexes.Clear();
        _directEntryCount     = 0;
    }

    private void Process(IEnumerable rootEntries, bool apply)
    {
        if (apply)
        {
            // Re-established only after this reconcile commits a direct-only scope.
            InvalidateDirectScopeIndex();
        }

        var expectedOwners = new Dictionary<INavMenuEntry, object>(ReferenceEqualityComparer.Instance);
        var firstOccurrences = new Dictionary<INavMenuEntry, EntryOccurrence>(ReferenceEqualityComparer.Instance);
        var activeEntries = new HashSet<INavMenuEntry>(ReferenceEqualityComparer.Instance);
        var nestedSources = new HashSet<INotifyCollectionChanged>(ReferenceEqualityComparer.Instance);
        var pendingEntries = new Stack<PendingEntry>();
        InvalidOperationException? validationException = null;
        var containsCustomNode = false;

        var rootEntryCount = PushEntries(rootEntries, _owner, _owner, false, nestedSources, pendingEntries);
        while (pendingEntries.Count > 0)
        {
            var pendingEntry = pendingEntries.Pop();
            if (pendingEntry.IsExit)
            {
                activeEntries.Remove((INavMenuEntry)pendingEntry.Item!);
                continue;
            }

            INavMenuEntry? entry = null;
            try
            {
                entry = NavMenuEntryValidation.Validate(
                    pendingEntry.Item,
                    pendingEntry.ValidationOwner,
                    pendingEntry.Index);
            }
            catch (InvalidOperationException exception)
            {
                validationException ??= exception;
            }

            if (entry is null)
            {
                continue;
            }

            if (_owner is INavMenuEntry ownerEntry &&
                !ReferenceEquals(pendingEntry.ValidationOwner, _owner) &&
                NavMenuEntryOwnership.IsTracked(entry))
            {
                try
                {
                    NavMenuEntryGraph.ValidateInsertion(ownerEntry, entry);
                }
                catch (InvalidOperationException exception)
                {
                    validationException ??= exception;
                }
            }

            if (!activeEntries.Add(entry))
            {
                validationException ??= new InvalidOperationException(
                    $"{pendingEntry.ValidationOwner.GetType().Name} contains a NavMenu entry cycle involving " +
                    $"{entry.GetType().Name}.");
                continue;
            }

            if (NavMenuEntryOwnership.IsTracked(entry))
            {
                if (!expectedOwners.TryAdd(entry, pendingEntry.StructuralOwner))
                {
                    var firstOccurrence = firstOccurrences[entry];
                    validationException ??= ReferenceEquals(
                        firstOccurrence.Owner,
                        pendingEntry.ValidationOwner)
                        ? NavMenuEntryOwnership.CreateDuplicateException(
                            entry,
                            pendingEntry.ValidationOwner,
                            firstOccurrence.Index,
                            pendingEntry.Index)
                        : NavMenuEntryOwnership.CreateTreeDuplicateException(
                            entry,
                            _owner,
                            firstOccurrence.Owner,
                            pendingEntry.ValidationOwner);
                }
                else
                {
                    firstOccurrences.Add(
                        entry,
                        new EntryOccurrence(pendingEntry.ValidationOwner, pendingEntry.Index));
                }
            }

            pendingEntries.Push(PendingEntry.Exit(entry));
            switch (entry)
            {
                case NavMenuNode:
                case NavMenuGroup:
                    break;

                case INavMenuNode customNode:
                    containsCustomNode = true;
                    PushEntries(
                        customNode.Entries,
                        pendingEntry.StructuralOwner,
                        customNode,
                        true,
                        nestedSources,
                        pendingEntries);
                    break;
            }
        }

        var currentOwnedEntries = new HashSet<INavMenuEntry>(ReferenceEqualityComparer.Instance);
        foreach (var (entry, expectedOwner) in expectedOwners)
        {
            if (ReferenceEquals(expectedOwner, _owner))
            {
                currentOwnedEntries.Add(entry);
            }

            if (NavMenuEntryOwnership.TryGetOwner(entry, out var currentOwner) &&
                !ReferenceEquals(currentOwner, expectedOwner))
            {
                validationException ??= NavMenuEntryOwnership.CreateAlreadyAttachedException(
                    entry,
                    expectedOwner,
                    currentOwner!);
            }
        }

        if (!apply)
        {
            if (validationException is not null)
            {
                throw validationException;
            }

            return;
        }

        foreach (var entry in _ownedEntries)
        {
            if (!currentOwnedEntries.Contains(entry))
            {
                NavMenuEntryOwnership.Detach(entry, _owner);
            }
        }

        _ownedEntries.IntersectWith(currentOwnedEntries);
        ReconcileSubscriptions(nestedSources);

        if (validationException is not null)
        {
            throw validationException;
        }

        foreach (var (entry, expectedOwner) in expectedOwners)
        {
            if (!NavMenuEntryOwnership.TryGetOwner(entry, out _))
            {
                NavMenuEntryOwnership.Attach(entry, expectedOwner);
            }
        }

        _ownedEntries.Clear();
        _ownedEntries.UnionWith(currentOwnedEntries);

        if (!containsCustomNode)
        {
            // Without custom nodes every occurrence is a direct entry of this scope.
            foreach (var (entry, occurrence) in firstOccurrences)
            {
                _directEntryIndexes.Add(entry, occurrence.Index);
            }

            _directEntryCount     = rootEntryCount;
            _isDirectScopeIndexed = true;
        }
    }

    private void ReconcileSubscriptions(HashSet<INotifyCollectionChanged> currentSources)
    {
        foreach (var source in _sourceSubscriptions.Keys.ToArray())
        {
            if (!currentSources.Contains(source))
            {
                _sourceSubscriptions.Remove(source, out var subscription);
                subscription?.Dispose();
            }
        }

        foreach (var source in currentSources)
        {
            if (!_sourceSubscriptions.ContainsKey(source))
            {
                _sourceSubscriptions.Add(source, source.WeakSubscribe(HandleNestedSourceChanged));
            }
        }
    }

    private void HandleNestedSourceChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Synchronize();
    }

    private static int PushEntries(IEnumerable entries,
                                   object structuralOwner,
                                   object validationOwner,
                                   bool trackSource,
                                   HashSet<INotifyCollectionChanged> nestedSources,
                                   Stack<PendingEntry> pendingEntries)
    {
        if (trackSource && entries is INotifyCollectionChanged notifyCollectionChanged)
        {
            nestedSources.Add(notifyCollectionChanged);
        }

        var bufferedEntries = entries.Cast<object?>().ToArray();
        for (var index = bufferedEntries.Length - 1; index >= 0; index--)
        {
            pendingEntries.Push(new PendingEntry(
                bufferedEntries[index],
                structuralOwner,
                validationOwner,
                index,
                false));
        }

        return bufferedEntries.Length;
    }

    private readonly record struct PendingEntry(object? Item,
                                                object StructuralOwner,
                                                object ValidationOwner,
                                                int Index,
                                                bool IsExit)
    {
        public static PendingEntry Exit(INavMenuEntry entry)
        {
            return new PendingEntry(entry, entry, entry, -1, true);
        }
    }

    private readonly record struct EntryOccurrence(object Owner, int Index);
}
