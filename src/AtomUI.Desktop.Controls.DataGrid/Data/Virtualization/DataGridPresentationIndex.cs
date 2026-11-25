namespace AtomUI.Desktop.Controls;

internal readonly struct DataGridViewportAnchor
{
    public DataGridViewportAnchor(
        DataGridRowKey rowKey,
        DataGridGroupKey groupKey,
        int fallbackSlot,
        double screenOffset)
    {
        if (rowKey.IsValid && groupKey.IsValid)
        {
            throw new ArgumentException("An anchor cannot identify both a row and a group.");
        }
        if (fallbackSlot < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(fallbackSlot));
        }
        if (!double.IsFinite(screenOffset))
        {
            throw new ArgumentOutOfRangeException(nameof(screenOffset));
        }
        RowKey = rowKey;
        GroupKey = groupKey;
        FallbackSlot = fallbackSlot;
        ScreenOffset = screenOffset;
    }

    public DataGridRowKey RowKey { get; }

    public DataGridGroupKey GroupKey { get; }

    public int FallbackSlot { get; }

    public double ScreenOffset { get; }
}

internal sealed class DataGridPresentationIndex
{
    private readonly SparseHeightDeltaIndex _heights;
    private readonly Dictionary<int, DataGridSourceEntry> _entriesBySlot;
    private readonly Dictionary<int, int> _slotsByWindowDataIndex;
    private readonly Dictionary<DataGridRowKey, int> _rowSlots;
    private readonly Dictionary<DataGridGroupKey, int> _groupSlots;

    public DataGridPresentationIndex(
        DataGridPresentationSnapshot snapshot,
        SparseHeightDeltaIndex heights)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(heights);
        Snapshot = snapshot;
        _heights = heights;
        _entriesBySlot = new Dictionary<int, DataGridSourceEntry>(
            snapshot.Blocks.Sum(static block => block.Entries.Length));
        _slotsByWindowDataIndex = new Dictionary<int, int>();
        _rowSlots = new Dictionary<DataGridRowKey, int>();
        _groupSlots = new Dictionary<DataGridGroupKey, int>();

        foreach (var block in snapshot.Blocks)
        {
            for (var offset = 0; offset < block.Entries.Length; offset++)
            {
                var slot = checked(block.Range.StartIndex + offset);
                if (slot >= snapshot.TotalEntryCount ||
                    !_entriesBySlot.TryAdd(slot, block.Entries[offset]))
                {
                    throw Invariant($"Committed range contains invalid or duplicate slot {slot}.");
                }
                var entry = block.Entries[offset];
                if (entry.Kind == DataGridSourceEntryKind.Data)
                {
                    if (!_slotsByWindowDataIndex.TryAdd(entry.WindowDataIndex, slot))
                    {
                        throw Invariant(
                            $"Committed ranges contain duplicate window data index {entry.WindowDataIndex}.");
                    }
                    if (!_rowSlots.TryAdd(entry.RowKey, slot))
                    {
                        throw Invariant($"Committed ranges contain duplicate row key '{entry.RowKey}'.");
                    }
                }
                else if (!_groupSlots.TryAdd(entry.Group!.Key, slot))
                {
                    throw Invariant(
                        $"Committed ranges contain duplicate group key '{entry.Group.Key}'.");
                }
            }
        }

        var viewportEnd = checked(
            snapshot.CommittedViewport.FirstVisibleIndex +
            snapshot.CommittedViewport.VisibleCount);
        if (viewportEnd > snapshot.TotalEntryCount)
        {
            throw Invariant("The committed viewport exceeds the presentation slot count.");
        }
        for (var slot = snapshot.CommittedViewport.FirstVisibleIndex;
             slot < viewportEnd;
             slot++)
        {
            if (!_entriesBySlot.ContainsKey(slot))
            {
                throw Invariant($"Committed viewport slot {slot} has no pinned range entry.");
            }
        }
    }

    public DataGridPresentationSnapshot Snapshot { get; }

    public int SlotCount => Snapshot.TotalEntryCount;

    public int ChildCount => Snapshot.WindowDataCount;

    public long TotalDataCount => Snapshot.TotalDataCount;

    public int CachedEntryCount => _entriesBySlot.Count;

    public DataGridSourceEntry GetEntry(int slot)
    {
        if (slot < 0 || slot >= SlotCount)
        {
            throw new ArgumentOutOfRangeException(nameof(slot));
        }
        if (_entriesBySlot.TryGetValue(slot, out var entry))
        {
            return entry;
        }
        throw Invariant($"Display slot {slot} is not present in the committed pinned blocks.");
    }

    public bool TryGetEntry(int slot, out DataGridSourceEntry entry) =>
        _entriesBySlot.TryGetValue(slot, out entry);

    public bool TryGetRowChildIndex(int slot, out int childIndex)
    {
        var entry = GetEntry(slot);
        if (entry.Kind == DataGridSourceEntryKind.Data)
        {
            childIndex = entry.WindowDataIndex;
            return true;
        }
        childIndex = -1;
        return false;
    }

    public int? FindSlot(DataGridRowKey rowKey) =>
        _rowSlots.TryGetValue(rowKey, out var slot) ? slot : null;

    public int? FindSlot(object item)
    {
        ArgumentNullException.ThrowIfNull(item);
        foreach (var pair in _entriesBySlot)
        {
            if (pair.Value.Kind == DataGridSourceEntryKind.Data &&
                (ReferenceEquals(pair.Value.Item, item) || Equals(pair.Value.Item, item)))
            {
                return pair.Key;
            }
        }
        return null;
    }

    public int? FindSlotByWindowDataIndex(int windowDataIndex)
    {
        if (windowDataIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowDataIndex));
        }
        return _slotsByWindowDataIndex.TryGetValue(windowDataIndex, out var slot)
            ? slot
            : null;
    }

    public int? FindSlot(DataGridGroupKey groupKey) =>
        _groupSlots.TryGetValue(groupKey, out var slot) ? slot : null;

    public DataGridViewportAnchor CaptureFirstCompleteAnchor(double viewportOffset)
    {
        if (SlotCount == 0)
        {
            throw new InvalidOperationException("An empty presentation has no scroll anchor.");
        }
        var slot = _heights.FindSlotAtOffset(viewportOffset, SlotCount);
        var slotTop = _heights.GetOffset(slot);
        if (viewportOffset > slotTop && slot + 1 < SlotCount)
        {
            slot++;
        }
        return CaptureAnchorAtSlot(slot, viewportOffset);
    }

    public DataGridViewportAnchor CapturePositionAnchor(double viewportOffset)
    {
        if (SlotCount == 0)
        {
            throw new InvalidOperationException("An empty presentation has no scroll anchor.");
        }
        var slot = _heights.FindSlotAtOffset(viewportOffset, SlotCount);
        // Without an identity, RestoreAnchor keeps the visible slot and its intra-row position.
        return new DataGridViewportAnchor(
            default,
            default,
            slot,
            _heights.GetOffset(slot) - viewportOffset);
    }

    public DataGridViewportAnchor CaptureAnchorAtSlot(int slot, double viewportOffset)
    {
        if (!double.IsFinite(viewportOffset) || viewportOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(viewportOffset));
        }
        var entry = GetEntry(slot);
        var screenOffset = _heights.GetOffset(slot) - viewportOffset;
        return entry.Kind == DataGridSourceEntryKind.Data
            ? new DataGridViewportAnchor(entry.RowKey, default, slot, screenOffset)
            : new DataGridViewportAnchor(default, entry.Group!.Key, slot, screenOffset);
    }

    public double RestoreAnchor(DataGridViewportAnchor anchor)
    {
        if (SlotCount == 0)
        {
            return 0;
        }
        int slot;
        if (anchor.RowKey.IsValid && _rowSlots.TryGetValue(anchor.RowKey, out var rowSlot))
        {
            slot = rowSlot;
        }
        else if (anchor.GroupKey.IsValid &&
                 _groupSlots.TryGetValue(anchor.GroupKey, out var groupSlot))
        {
            slot = groupSlot;
        }
        else
        {
            slot = Math.Clamp(anchor.FallbackSlot, 0, SlotCount - 1);
        }
        var slotTop = _heights.GetOffset(slot);
        var offset = slotTop - anchor.ScreenOffset;
        if (!anchor.RowKey.IsValid &&
            !anchor.GroupKey.IsValid &&
            offset >= _heights.GetOffset(slot + 1))
        {
            // A shorter replacement row cannot retain the old intra-row offset. Keep this
            // slot fully visible instead of restoring past it or leaving only a subpixel sliver.
            offset = slotTop;
        }
        return Math.Clamp(
            offset,
            0,
            _heights.GetExtent(SlotCount));
    }

    private static DataGridPresentationInvariantException Invariant(string message) =>
        new(message);
}
