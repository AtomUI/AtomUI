using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A half-open range of display entries that a fetch asks a data source for. Display entries are the data
/// rows and group headers of the window in the order of the query; the window is the requested page or all
/// rows. Only a range created by the constructor is usable: the <c>default</c> value has a count of 0.
/// </summary>
public readonly struct DataGridRange : IEquatable<DataGridRange>
{
    /// <summary>
    /// Creates a range of <paramref name="count" /> display entries.
    /// </summary>
    /// <param name="startIndex">
    /// Index of the first display entry of the range. See <see cref="StartIndex" />.
    /// </param>
    /// <param name="count">
    /// Number of display entries in the range. Must be positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex" /> is negative or <paramref name="count" /> is not positive.
    /// </exception>
    /// <exception cref="OverflowException">
    /// <paramref name="startIndex" /> plus <paramref name="count" /> exceeds <see cref="int.MaxValue" />.
    /// </exception>
    public DataGridRange(int startIndex, int count)
    {
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }
        _ = checked(startIndex + count);
        StartIndex = startIndex;
        Count = count;
    }

    /// <summary>
    /// Index of the first display entry of the range, counted from the first display entry of the window.
    /// Never negative.
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    /// Number of display entries in the range. It is positive for every created range and <c>0</c> only for
    /// the <c>default</c> value.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Index just past the last display entry of the range.
    /// </summary>
    public int EndExclusive => checked(StartIndex + Count);

    /// <summary>
    /// Returns whether both ranges have the same <see cref="StartIndex" /> and <see cref="Count" />.
    /// </summary>
    public bool Equals(DataGridRange other) =>
        StartIndex == other.StartIndex && Count == other.Count;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DataGridRange other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(StartIndex, Count);

    /// <summary>
    /// Returns whether two ranges have the same start index and count.
    /// </summary>
    public static bool operator ==(DataGridRange left, DataGridRange right) => left.Equals(right);

    /// <summary>
    /// Returns whether two ranges differ in start index or in count.
    /// </summary>
    public static bool operator !=(DataGridRange left, DataGridRange right) => !left.Equals(right);
}

/// <summary>
/// The page of data rows that the ranges of a fetch request are counted from, as an alternative to
/// loading all rows continuously. A page starts at <see cref="DataStartIndex" /> and holds up to
/// <see cref="DataCount" /> rows, or fewer when the data ends earlier.
/// The <c>default</c> value has a count of 0 and is not a valid page.
/// </summary>
public readonly struct DataGridPageRequest : IEquatable<DataGridPageRequest>
{
    /// <summary>
    /// Creates a page of <paramref name="dataCount" /> data rows.
    /// </summary>
    /// <param name="dataStartIndex">
    /// Index of the first data row of the page over all data rows. See <see cref="DataStartIndex" />.
    /// </param>
    /// <param name="dataCount">
    /// Number of data rows that the page holds. Must be positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="dataStartIndex" /> is negative or <paramref name="dataCount" /> is not positive.
    /// </exception>
    /// <exception cref="OverflowException">
    /// <paramref name="dataStartIndex" /> plus <paramref name="dataCount" /> exceeds
    /// <see cref="long.MaxValue" />.
    /// </exception>
    public DataGridPageRequest(long dataStartIndex, int dataCount)
    {
        if (dataStartIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataStartIndex));
        }
        if (dataCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataCount));
        }
        _ = checked(dataStartIndex + dataCount);
        DataStartIndex = dataStartIndex;
        DataCount = dataCount;
    }

    /// <summary>
    /// Index of the first data row of the page over all data rows. Never negative.
    /// </summary>
    public long DataStartIndex { get; }

    /// <summary>
    /// The page size, that is the number of data rows that the page holds. A page holds fewer rows when the
    /// data ends before the page does.
    /// </summary>
    public int DataCount { get; }

    /// <summary>
    /// Returns whether both pages have the same <see cref="DataStartIndex" /> and <see cref="DataCount" />.
    /// </summary>
    public bool Equals(DataGridPageRequest other) =>
        DataStartIndex == other.DataStartIndex && DataCount == other.DataCount;

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DataGridPageRequest other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(DataStartIndex, DataCount);

    /// <summary>
    /// Returns whether two pages have the same start index and size.
    /// </summary>
    public static bool operator ==(DataGridPageRequest left, DataGridPageRequest right) => left.Equals(right);

    /// <summary>
    /// Returns whether two pages differ in start index or in size.
    /// </summary>
    public static bool operator !=(DataGridPageRequest left, DataGridPageRequest right) => !left.Equals(right);
}

/// <summary>
/// The immutable set of row groups that are collapsed in a data grid. A collapsed group appears as its
/// header only: the rows and group headers under it are left out of range results. Every group that is not
/// listed is expanded. <see cref="Collapse" /> and <see cref="Expand" /> return a changed copy and leave
/// this instance unchanged.
/// </summary>
public sealed class DataGridGroupExpansion : IEquatable<DataGridGroupExpansion>
{
    private readonly int _structuralHashCode;

    /// <summary>
    /// The expansion without collapsed groups. It is the initial state of a grid, and <see cref="Expand" />
    /// returns it when the last collapsed group is expanded.
    /// </summary>
    public static DataGridGroupExpansion AllExpanded { get; } = new(default);

    /// <summary>
    /// Creates an expansion with the given collapsed groups, sorted by group path text and without duplicates.
    /// </summary>
    /// <param name="collapsedGroups">
    /// The groups to collapse. A <c>default</c> or empty array creates an expansion without collapsed groups.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="collapsedGroups" /> contains a group key that is not valid.
    /// </exception>
    public DataGridGroupExpansion(ImmutableArray<DataGridGroupKey> collapsedGroups)
    {
        if (collapsedGroups.IsDefaultOrEmpty)
        {
            CollapsedGroups = ImmutableArray<DataGridGroupKey>.Empty;
            _structuralHashCode = 0;
            return;
        }

        var sorted = collapsedGroups.ToArray();
        foreach (var key in sorted)
        {
            if (!key.IsValid)
            {
                throw new ArgumentException(
                    "Collapsed group identities must be valid.", nameof(collapsedGroups));
            }
        }
        Array.Sort(sorted, static (left, right) =>
            StringComparer.Ordinal.Compare(left.Value, right.Value));

        var builder = ImmutableArray.CreateBuilder<DataGridGroupKey>(sorted.Length);
        DataGridGroupKey? previous = null;
        foreach (var key in sorted)
        {
            if (previous is null || previous.Value != key)
            {
                builder.Add(key);
                previous = key;
            }
        }
        CollapsedGroups = builder.ToImmutable();
        _structuralHashCode = ComputeStructuralHashCode(CollapsedGroups);
    }

    /// <summary>
    /// The collapsed groups, sorted by group path text and without duplicates. Empty when every group is
    /// expanded.
    /// </summary>
    public ImmutableArray<DataGridGroupKey> CollapsedGroups { get; }

    /// <summary>
    /// Returns an expansion in which the group is also collapsed, or this instance when the group is
    /// collapsed already. This instance is not changed.
    /// </summary>
    /// <param name="key">
    /// Identity of the group to collapse. Must be valid.
    /// </param>
    /// <returns>
    /// The changed expansion, or this instance when nothing changes.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="key" /> is not valid.
    /// </exception>
    public DataGridGroupExpansion Collapse(DataGridGroupKey key)
    {
        ValidateKey(key);
        var index = Find(key);
        if (index >= 0)
        {
            return this;
        }
        var insertionIndex = ~index;
        var builder = ImmutableArray.CreateBuilder<DataGridGroupKey>(CollapsedGroups.Length + 1);
        for (var current = 0; current < insertionIndex; current++)
        {
            builder.Add(CollapsedGroups[current]);
        }
        builder.Add(key);
        for (var current = insertionIndex; current < CollapsedGroups.Length; current++)
        {
            builder.Add(CollapsedGroups[current]);
        }
        return new DataGridGroupExpansion(builder.MoveToImmutable());
    }

    /// <summary>
    /// Returns an expansion in which the group is expanded, this instance when it is expanded already, or
    /// <see cref="AllExpanded" /> when no collapsed group is left. This instance is not changed.
    /// </summary>
    /// <param name="key">
    /// Identity of the group to expand. Must be valid.
    /// </param>
    /// <returns>
    /// The changed expansion, this instance, or <see cref="AllExpanded" />.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="key" /> is not valid.
    /// </exception>
    public DataGridGroupExpansion Expand(DataGridGroupKey key)
    {
        ValidateKey(key);
        var index = Find(key);
        if (index < 0)
        {
            return this;
        }
        var builder = ImmutableArray.CreateBuilder<DataGridGroupKey>(CollapsedGroups.Length - 1);
        for (var current = 0; current < CollapsedGroups.Length; current++)
        {
            if (current != index)
            {
                builder.Add(CollapsedGroups[current]);
            }
        }
        return builder.Count == 0
            ? AllExpanded
            : new DataGridGroupExpansion(builder.MoveToImmutable());
    }

    /// <summary>
    /// Returns whether <paramref name="other" /> is not <see langword="null" /> and has the same collapsed groups.
    /// </summary>
    public bool Equals(DataGridGroupExpansion? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        if (other is null ||
            _structuralHashCode != other._structuralHashCode ||
            CollapsedGroups.Length != other.CollapsedGroups.Length)
        {
            return false;
        }
        for (var index = 0; index < CollapsedGroups.Length; index++)
        {
            if (CollapsedGroups[index] != other.CollapsedGroups[index])
            {
                return false;
            }
        }
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DataGridGroupExpansion other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => _structuralHashCode;

    /// <summary>
    /// Returns whether both expansions are <see langword="null" />, or neither is and they have the same
    /// collapsed groups.
    /// </summary>
    public static bool operator ==(DataGridGroupExpansion? left, DataGridGroupExpansion? right) =>
        left is null ? right is null : left.Equals(right);

    /// <summary>
    /// Returns whether exactly one expansion is <see langword="null" />, or both have different collapsed
    /// groups.
    /// </summary>
    public static bool operator !=(DataGridGroupExpansion? left, DataGridGroupExpansion? right) => !(left == right);

    private int Find(DataGridGroupKey key)
    {
        var low = 0;
        var high = CollapsedGroups.Length - 1;
        while (low <= high)
        {
            var middle = low + ((high - low) >> 1);
            var comparison = StringComparer.Ordinal.Compare(CollapsedGroups[middle].Value, key.Value);
            if (comparison == 0)
            {
                return middle;
            }
            if (comparison < 0)
            {
                low = middle + 1;
            }
            else
            {
                high = middle - 1;
            }
        }
        return ~low;
    }

    private static void ValidateKey(DataGridGroupKey key)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException("The group identity must be valid.", nameof(key));
        }
    }

    private static int ComputeStructuralHashCode(ImmutableArray<DataGridGroupKey> keys)
    {
        var hash = new HashCode();
        foreach (var key in keys)
        {
            hash.Add(key);
        }
        return hash.ToHashCode();
    }
}

/// <summary>
/// Says whether a <see cref="DataGridSourceEntry" /> is a data row or a group header.
/// </summary>
public enum DataGridSourceEntryKind
{
    /// <summary>
    /// The entry is a data row: <see cref="DataGridSourceEntry.RowKey" /> is valid and
    /// <see cref="DataGridSourceEntry.Group" /> is <see langword="null" />.
    /// </summary>
    Data,
    /// <summary>
    /// The entry is the header of the group in <see cref="DataGridSourceEntry.Group" />. It has no row, and
    /// <see cref="DataGridSourceEntry.WindowDataIndex" /> and <see cref="DataGridSourceEntry.DataIndex" />
    /// are <c>-1</c>.
    /// </summary>
    GroupHeader
}

/// <summary>
/// Describes one row group for its header in a range result: the field and value that form the group, its
/// nesting depth and the number of data rows in it.
/// </summary>
public sealed class DataGridGroupEntry
{
    /// <summary>
    /// Creates the description of one row group.
    /// </summary>
    /// <param name="key">
    /// Identity of the group, by its group path text.
    /// </param>
    /// <param name="field">
    /// The field that forms the group at <paramref name="level" />.
    /// </param>
    /// <param name="value">
    /// The value of <paramref name="field" /> that the rows of the group share.
    /// </param>
    /// <param name="level">
    /// Nesting depth of the group; <c>0</c> for a top-level group.
    /// </param>
    /// <param name="leafCount">
    /// Number of data rows in the group. Must be positive.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="key" /> or <paramref name="field" /> is not valid.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="level" /> is negative or <paramref name="leafCount" /> is not positive.
    /// </exception>
    public DataGridGroupEntry(
        DataGridGroupKey key,
        DataGridFieldId field,
        DataGridScalar value,
        int level,
        long leafCount)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException("The group identity must be valid.", nameof(key));
        }
        if (!field.IsValid)
        {
            throw new ArgumentException("The field identity must be valid.", nameof(field));
        }
        if (level < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(level));
        }
        if (leafCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(leafCount));
        }

        Key = key;
        Field = field;
        Value = value;
        Level = level;
        LeafCount = leafCount;
    }

    /// <summary>
    /// Identity of the group. It is the key that <see cref="DataGridGroupExpansion.Collapse" /> and
    /// <see cref="DataGridGroupExpansion.Expand" /> take for this group.
    /// </summary>
    public DataGridGroupKey Key { get; }

    /// <summary>
    /// The field that forms the group at <see cref="Level" />. It is the field of the group level at that
    /// index in <see cref="DataGridQuery.Groups" />.
    /// </summary>
    public DataGridFieldId Field { get; }

    /// <summary>
    /// The value of <see cref="Field" /> that the rows of the group share.
    /// </summary>
    public DataGridScalar Value { get; }

    /// <summary>
    /// Nesting depth of the group, <c>0</c> for a top-level group. It indexes the group levels of
    /// <see cref="DataGridQuery.Groups" />.
    /// </summary>
    public int Level { get; }

    /// <summary>
    /// Number of data rows in the group; the group header shows it as the item count. Always positive.
    /// </summary>
    public long LeafCount { get; }
}

/// <summary>
/// One display entry of a <see cref="DataGridRangeResult" />: a data row or a group header.
/// Entries are created by <see cref="CreateData" /> or <see cref="CreateGroupHeader" />.
/// The <c>default</c> value is not a usable entry, because its row key is not valid.
/// </summary>
public readonly struct DataGridSourceEntry
{
    private DataGridSourceEntry(
        DataGridSourceEntryKind kind,
        DataGridRowKey rowKey,
        object? item,
        DataGridGroupEntry? group,
        int windowDataIndex,
        long dataIndex)
    {
        Kind = kind;
        RowKey = rowKey;
        Item = item;
        Group = group;
        WindowDataIndex = windowDataIndex;
        DataIndex = dataIndex;
    }

    /// <summary>
    /// Whether this entry is a data row or a group header, and therefore which of its members carry a value.
    /// </summary>
    public DataGridSourceEntryKind Kind { get; }

    /// <summary>
    /// Identity of the data row. Set for <see cref="DataGridSourceEntryKind.Data" />; the <c>default</c>
    /// value for <see cref="DataGridSourceEntryKind.GroupHeader" />.
    /// </summary>
    public DataGridRowKey RowKey { get; }

    /// <summary>
    /// The data item of the row, or <see langword="null" />. When it is set, it is an instance of the item
    /// type that the schema of the source declares. <see langword="null" /> for a group header.
    /// </summary>
    public object? Item { get; }

    /// <summary>
    /// The group whose header this entry is. <see langword="null" /> for a data row.
    /// </summary>
    public DataGridGroupEntry? Group { get; }

    /// <summary>
    /// Index of the data row within the window, that is within the requested page or, without a page request,
    /// within all rows. It is <c>-1</c> for a group header.
    /// </summary>
    public int WindowDataIndex { get; }

    /// <summary>
    /// Index of the data row over all data rows: <see cref="WindowDataIndex" /> plus the start index of the
    /// requested page. It is <c>-1</c> for a group header.
    /// </summary>
    public long DataIndex { get; }

    /// <summary>
    /// Creates an entry for a data row.
    /// </summary>
    /// <param name="rowKey">
    /// Identity of the row.
    /// </param>
    /// <param name="item">
    /// The data item of the row, or <see langword="null" />.
    /// </param>
    /// <param name="windowDataIndex">
    /// Index of the row within the window. See <see cref="WindowDataIndex" />.
    /// </param>
    /// <param name="dataIndex">
    /// Index of the row over all data rows. See <see cref="DataIndex" />.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="rowKey" /> is not valid.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="windowDataIndex" /> or <paramref name="dataIndex" /> is negative.
    /// </exception>
    public static DataGridSourceEntry CreateData(
        DataGridRowKey rowKey,
        object? item,
        int windowDataIndex,
        long dataIndex)
    {
        if (!rowKey.IsValid)
        {
            throw new ArgumentException("The row key must be valid.", nameof(rowKey));
        }
        if (windowDataIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowDataIndex));
        }
        if (dataIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataIndex));
        }
        return new DataGridSourceEntry(
            DataGridSourceEntryKind.Data,
            rowKey,
            item,
            null,
            windowDataIndex,
            dataIndex);
    }

    /// <summary>
    /// Creates an entry for the header of a group. The entry has no row, and both of its indexes
    /// are <c>-1</c>.
    /// </summary>
    /// <param name="group">
    /// The group whose header this entry is.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="group" /> is <see langword="null" />.
    /// </exception>
    public static DataGridSourceEntry CreateGroupHeader(DataGridGroupEntry group)
    {
        ArgumentNullException.ThrowIfNull(group);
        return new DataGridSourceEntry(
            DataGridSourceEntryKind.GroupHeader,
            default,
            null,
            group,
            -1,
            -1);
    }
}

/// <summary>
/// Describes one fetch from a data source: which query, page and group expansion to apply, which display
/// entries to return, and which version of the data they must come from.
/// The <c>default</c> value is not a usable request: it has no query and an empty range.
/// </summary>
public readonly struct DataGridFetchRequest
{
    /// <summary>
    /// Creates a request. The arguments become the properties of the same names.
    /// </summary>
    /// <param name="query">
    /// See <see cref="Query" />.
    /// </param>
    /// <param name="pageRequest">
    /// See <see cref="PageRequest" />.
    /// </param>
    /// <param name="groupExpansion">
    /// See <see cref="GroupExpansion" />.
    /// </param>
    /// <param name="range">
    /// See <see cref="Range" />.
    /// </param>
    /// <param name="expectedSnapshot">
    /// See <see cref="ExpectedSnapshot" />.
    /// </param>
    /// <param name="queryRevision">
    /// See <see cref="QueryRevision" />.
    /// </param>
    /// <param name="dataGeneration">
    /// See <see cref="DataGeneration" />.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> or <paramref name="groupExpansion" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="range" /> is not initialized or is empty,
    /// <paramref name="expectedSnapshot" /> is set but not valid, or
    /// <paramref name="groupExpansion" /> lists collapsed groups while <paramref name="query" />
    /// has no group levels.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="queryRevision" /> or <paramref name="dataGeneration" /> is negative.
    /// </exception>
    public DataGridFetchRequest(
        DataGridQuery query,
        DataGridPageRequest? pageRequest,
        DataGridGroupExpansion groupExpansion,
        DataGridRange range,
        DataGridSnapshotId? expectedSnapshot,
        long queryRevision,
        long dataGeneration)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(groupExpansion);
        if (range.Count <= 0)
        {
            throw new ArgumentException("The range must be initialized and non-empty.", nameof(range));
        }
        if (expectedSnapshot is { IsValid: false })
        {
            throw new ArgumentException("The expected snapshot identity must be valid.", nameof(expectedSnapshot));
        }
        if (queryRevision < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(queryRevision));
        }
        if (dataGeneration < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(dataGeneration));
        }
        if (query.Groups.IsEmpty && !groupExpansion.CollapsedGroups.IsEmpty)
        {
            throw new ArgumentException(
                "Collapsed groups require at least one query group.", nameof(groupExpansion));
        }

        Query = query;
        PageRequest = pageRequest;
        GroupExpansion = groupExpansion;
        Range = range;
        ExpectedSnapshot = expectedSnapshot;
        QueryRevision = queryRevision;
        DataGeneration = dataGeneration;
    }

    /// <summary>
    /// The query of the grid: the sorts, the filters and the group levels that the source applies to the rows
    /// before it takes the page and the range.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// The page that <see cref="Range" /> is counted from. <see langword="null" /> loads the rows
    /// continuously, that is without paging, which supports at most <see cref="int.MaxValue" /> rows.
    /// </summary>
    public DataGridPageRequest? PageRequest { get; }

    /// <summary>
    /// The groups that are collapsed. It must not list a collapsed group when <see cref="Query" /> has no
    /// group levels.
    /// </summary>
    public DataGridGroupExpansion GroupExpansion { get; }

    /// <summary>
    /// The display entries to return, counted from the first display entry of the page. The count must be
    /// positive and must not exceed the maximum range size of the source schema.
    /// The result is shorter only when the range reaches past the last display entry.
    /// </summary>
    public DataGridRange Range { get; }

    /// <summary>
    /// The snapshot, that is the version of the content, that the source must still serve when it answers.
    /// <see langword="null" /> accepts any snapshot. A source that has moved to another snapshot fails the
    /// fetch with <see cref="DataGridSnapshotExpiredException" />.
    /// </summary>
    public DataGridSnapshotId? ExpectedSnapshot { get; }

    /// <summary>
    /// Revision of the query of the grid when the request was created. The grid increments it when the query
    /// changes and compares it to recognize a result that belongs to a superseded query.
    /// </summary>
    public long QueryRevision { get; }

    /// <summary>
    /// Generation of the data when the request was created. The grid compares it with its active generation, as
    /// it does with <see cref="QueryRevision" />, and it is part of the identity of a cached range block.
    /// </summary>
    public long DataGeneration { get; }
}

/// <summary>
/// The answer of a data source to a <see cref="DataGridFetchRequest" />: the display entries of the requested
/// range, the counts of the window that the range was taken from, and the snapshot that the data came from.
/// </summary>
public sealed class DataGridRangeResult
{
    /// <summary>
    /// Creates a result. The arguments become the properties of the same names.
    /// </summary>
    /// <param name="startIndex">
    /// See <see cref="StartIndex" />.
    /// </param>
    /// <param name="entries">
    /// See <see cref="Entries" />. A <c>default</c> array is stored as empty.
    /// </param>
    /// <param name="totalEntryCount">
    /// See <see cref="TotalEntryCount" />.
    /// </param>
    /// <param name="windowDataCount">
    /// See <see cref="WindowDataCount" />.
    /// </param>
    /// <param name="totalDataCount">
    /// See <see cref="TotalDataCount" />.
    /// </param>
    /// <param name="snapshot">
    /// See <see cref="Snapshot" />.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex" />, <paramref name="totalEntryCount" />,
    /// <paramref name="windowDataCount" /> or <paramref name="totalDataCount" /> is negative.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> is not valid.
    /// </exception>
    public DataGridRangeResult(
        int startIndex,
        ImmutableArray<DataGridSourceEntry> entries,
        int totalEntryCount,
        int windowDataCount,
        long totalDataCount,
        DataGridSnapshotId snapshot)
    {
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }
        if (totalEntryCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalEntryCount));
        }
        if (windowDataCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(windowDataCount));
        }
        if (totalDataCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(totalDataCount));
        }
        if (!snapshot.IsValid)
        {
            throw new ArgumentException("The snapshot identity must be valid.", nameof(snapshot));
        }

        StartIndex = startIndex;
        Entries = entries.IsDefault ? ImmutableArray<DataGridSourceEntry>.Empty : entries;
        TotalEntryCount = totalEntryCount;
        WindowDataCount = windowDataCount;
        TotalDataCount = totalDataCount;
        Snapshot = snapshot;
    }

    /// <summary>
    /// Start index of the range that the entries belong to. It is the start index of the requested
    /// <see cref="DataGridFetchRequest.Range" />.
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    /// The display entries of the range in the order of the query: as many as the range holds within
    /// <see cref="TotalEntryCount" />, and none when the range starts past the last display entry.
    /// </summary>
    public ImmutableArray<DataGridSourceEntry> Entries { get; }

    /// <summary>
    /// Number of display entries in the whole window, that is data rows plus group headers. It equals
    /// <see cref="WindowDataCount" /> when the query has no group levels.
    /// </summary>
    public int TotalEntryCount { get; }

    /// <summary>
    /// Number of data rows in the window, without group headers. The window holds all rows without a page
    /// request, or the rows of the requested page, which are fewer than its size when the data ends earlier.
    /// </summary>
    public int WindowDataCount { get; }

    /// <summary>
    /// Number of data rows over all pages; the start index of a page is checked against it.
    /// Without a page request it must be at most <see cref="int.MaxValue" />.
    /// </summary>
    public long TotalDataCount { get; }

    /// <summary>
    /// The snapshot, that is the version of the content, that the entries and counts come from. It equals the
    /// expected snapshot of the request when the request names one.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }
}
