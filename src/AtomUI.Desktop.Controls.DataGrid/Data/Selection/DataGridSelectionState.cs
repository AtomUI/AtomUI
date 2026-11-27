using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Identifies the data view that a selection belongs to: its source, query and snapshot.
/// </summary>
/// <remarks>
/// <see cref="DataGridSelectionState.AllMatchingQuery" /> and <see cref="DataGridSelectionState.IndexIntervals" />
/// select rows only when their scope equals the scope of those rows. Explicit and excluded keys are not tied
/// to a scope.
/// </remarks>
public sealed class DataGridSelectionScope : IEquatable<DataGridSelectionScope>
{
    /// <summary>
    /// Creates a scope from a source, the query and the snapshot identity of the rows it covers.
    /// </summary>
    /// <param name="source">
    /// The source the rows come from. Compared by reference in equality.
    /// </param>
    /// <param name="query">
    /// The query the rows were loaded under.
    /// </param>
    /// <param name="snapshot">
    /// The snapshot identity the rows were loaded under.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="snapshot" /> is not a valid snapshot identity.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="source" /> or <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    public DataGridSelectionScope(
        IDataGridSource source,
        DataGridQuery query,
        DataGridSnapshotId snapshot)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(query);
        if (!snapshot.IsValid)
        {
            throw new ArgumentException("The snapshot identity must be valid.", nameof(snapshot));
        }

        Source = source;
        Query = query;
        Snapshot = snapshot;
    }

    /// <summary>
    /// The source the rows come from. Two scopes over different source instances are never equal.
    /// </summary>
    public IDataGridSource Source { get; }

    /// <summary>
    /// The query the rows were loaded under.
    /// </summary>
    public DataGridQuery Query { get; }

    /// <summary>
    /// The snapshot identity the rows were loaded under. It is always valid.
    /// </summary>
    public DataGridSnapshotId Snapshot { get; }

    /// <summary>
    /// Returns whether both scopes have the same <see cref="Source" /> instance and equal
    /// <see cref="Query" /> and <see cref="Snapshot" /> values.
    /// </summary>
    public bool Equals(DataGridSelectionScope? other) =>
        ReferenceEquals(this, other) ||
        other is not null &&
        ReferenceEquals(Source, other.Source) &&
        Query == other.Query &&
        Snapshot == other.Snapshot;

    /// <summary>
    /// Returns whether <paramref name="obj" /> is an equal <see cref="DataGridSelectionScope" />.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DataGridSelectionScope other && Equals(other);

    /// <summary>
    /// Returns a hash code that is equal for equal scopes.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(
        RuntimeHelpers.GetHashCode(Source),
        Query,
        Snapshot);
}

/// <summary>
/// A range of data indexes that selects rows of the scope it is bound to.
/// </summary>
/// <remarks>
/// The interval covers <see cref="StartIndex" /> but not <see cref="EndIndexExclusive" />, so it is never empty.
/// </remarks>
public readonly struct DataGridSelectionInterval : IEquatable<DataGridSelectionInterval>
{
    /// <summary>
    /// Creates an interval that covers the data indexes from <paramref name="startIndex" /> up to, but not
    /// including, <paramref name="endIndexExclusive" />.
    /// </summary>
    /// <param name="startIndex">
    /// The first data index covered. It must not be negative.
    /// </param>
    /// <param name="endIndexExclusive">
    /// The first data index after the interval. It must be greater than <paramref name="startIndex" />.
    /// </param>
    /// <param name="scope">
    /// The scope whose rows the interval selects.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="startIndex" /> is negative, or <paramref name="endIndexExclusive" /> is not
    /// greater than <paramref name="startIndex" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="scope" /> is <see langword="null" />.
    /// </exception>
    public DataGridSelectionInterval(
        long startIndex,
        long endIndexExclusive,
        DataGridSelectionScope scope)
    {
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex));
        }
        if (endIndexExclusive <= startIndex)
        {
            throw new ArgumentOutOfRangeException(nameof(endIndexExclusive));
        }
        ArgumentNullException.ThrowIfNull(scope);

        StartIndex = startIndex;
        EndIndexExclusive = endIndexExclusive;
        Scope = scope;
    }

    /// <summary>
    /// The first data index the interval covers. It is never negative.
    /// </summary>
    public long StartIndex { get; }

    /// <summary>
    /// The first data index after the interval. It is always greater than <see cref="StartIndex" />.
    /// </summary>
    public long EndIndexExclusive { get; }

    /// <summary>
    /// The scope the interval is bound to. The interval selects a row only when that row belongs to an equal
    /// scope.
    /// </summary>
    public DataGridSelectionScope Scope { get; }

    /// <summary>
    /// Determines whether the given data index lies in the interval, start included and end excluded.
    /// <see cref="Scope" /> is not compared; the caller tests it when it matters.
    /// </summary>
    public bool Contains(long dataIndex) =>
        dataIndex >= StartIndex && dataIndex < EndIndexExclusive;

    /// <summary>
    /// Returns whether both intervals have the same start index, end index and an equal <see cref="Scope" />.
    /// </summary>
    public bool Equals(DataGridSelectionInterval other) =>
        StartIndex == other.StartIndex &&
        EndIndexExclusive == other.EndIndexExclusive &&
        Equals(Scope, other.Scope);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is an equal <see cref="DataGridSelectionInterval" />.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DataGridSelectionInterval other && Equals(other);

    /// <summary>
    /// Returns a hash code that is equal for equal intervals.
    /// </summary>
    public override int GetHashCode() => HashCode.Combine(
        StartIndex,
        EndIndexExclusive,
        Scope);

    /// <summary>
    /// Returns whether both intervals are equal.
    /// </summary>
    public static bool operator ==(
        DataGridSelectionInterval left,
        DataGridSelectionInterval right) => left.Equals(right);

    /// <summary>
    /// Returns whether both intervals differ.
    /// </summary>
    public static bool operator !=(
        DataGridSelectionInterval left,
        DataGridSelectionInterval right) => !left.Equals(right);
}

internal enum DataGridSelectionTransition
{
    Preserve,
    SortOrGroup,
    Filter,
    Invalidation
}

/// <summary>
/// An immutable description of the selected rows of a <see cref="DataGrid" />.
/// It is the value of the <see cref="DataGrid.Selection" /> property.
/// </summary>
/// <remarks>
/// A row is selected when its key is in <see cref="ExplicitKeys" />, when the row belongs to the scope of
/// <see cref="AllMatchingQuery" />, or when its data index lies in one of <see cref="IndexIntervals" />,
/// and when its key is not in <see cref="ExcludedKeys" />.
/// </remarks>
public sealed class DataGridSelectionState : IEquatable<DataGridSelectionState>
{
    /// <summary>
    /// Creates a selection, normalizing the keys and the intervals it receives.
    /// </summary>
    /// <param name="explicitKeys">
    /// The keys of the rows that are selected individually.
    /// </param>
    /// <param name="allMatchingQuery">
    /// The scope whose matching rows are all selected, or <see langword="null" /> for none.
    /// </param>
    /// <param name="indexIntervals">
    /// The data index ranges of selected rows. All of them must share one scope.
    /// </param>
    /// <param name="excludedKeys">
    /// The keys of rows that stay unselected although the query or an interval covers them.
    /// </param>
    /// <exception cref="ArgumentException">
    /// A key is not valid, an interval has no scope, the intervals do not share one scope, or that scope
    /// differs from <paramref name="allMatchingQuery" /> when it is not <see langword="null" />.
    /// </exception>
    /// <remarks>
    /// Both key arrays are sorted and deduplicated. The intervals are sorted, and overlapping or adjacent
    /// intervals are merged. A default array counts as empty.
    /// </remarks>
    public DataGridSelectionState(
        ImmutableArray<DataGridRowKey> explicitKeys,
        DataGridSelectionScope? allMatchingQuery,
        ImmutableArray<DataGridSelectionInterval> indexIntervals,
        ImmutableArray<DataGridRowKey> excludedKeys)
    {
        ExplicitKeys = NormalizeKeys(explicitKeys, nameof(explicitKeys));
        IndexIntervals = NormalizeIntervals(indexIntervals, allMatchingQuery);
        ExcludedKeys = NormalizeKeys(excludedKeys, nameof(excludedKeys));
        AllMatchingQuery = allMatchingQuery;
    }

    private DataGridSelectionState()
    {
        ExplicitKeys = ImmutableArray<DataGridRowKey>.Empty;
        IndexIntervals = ImmutableArray<DataGridSelectionInterval>.Empty;
        ExcludedKeys = ImmutableArray<DataGridRowKey>.Empty;
    }

    /// <summary>
    /// The selection that contains no row.
    /// </summary>
    public static DataGridSelectionState Empty { get; } = new();

    /// <summary>
    /// The keys of the rows that are selected individually, in ascending key order and without duplicates.
    /// </summary>
    public ImmutableArray<DataGridRowKey> ExplicitKeys { get; }

    /// <summary>
    /// The scope whose matching rows are all selected, or <see langword="null" /> for none.
    /// It selects only rows of an equal scope.
    /// </summary>
    public DataGridSelectionScope? AllMatchingQuery { get; }

    /// <summary>
    /// The selected ranges of data indexes, each bound to the scope whose rows it selects.
    /// They are sorted by start index, do not overlap or touch, and all share one scope.
    /// </summary>
    public ImmutableArray<DataGridSelectionInterval> IndexIntervals { get; }

    /// <summary>
    /// The keys of rows that stay unselected although the query or an interval covers them.
    /// Like <see cref="ExplicitKeys" />, the array is sorted and has no duplicates.
    /// </summary>
    public ImmutableArray<DataGridRowKey> ExcludedKeys { get; }

    /// <summary>
    /// Is <see langword="true" /> when the selection has no explicit key, query or interval.
    /// <see cref="ExcludedKeys" /> alone do not make it non-empty.
    /// </summary>
    public bool IsEmpty =>
        ExplicitKeys.IsEmpty &&
        AllMatchingQuery is null &&
        IndexIntervals.IsEmpty;

    /// <summary>
    /// Determines whether the row with the given key and data index is selected in the given scope.
    /// </summary>
    /// <param name="key">
    /// The key of the row.
    /// </param>
    /// <param name="dataIndex">
    /// The data index of the row. Only the intervals are tested against it.
    /// </param>
    /// <param name="scope">
    /// The scope the row belongs to. The keys are tested regardless of it; the query and the intervals
    /// select the row only when it is equal to their scope.
    /// </param>
    /// <returns>
    /// <see langword="false" /> when the key is excluded, even if it is also selected explicitly. Otherwise
    /// <see langword="true" /> when the key is selected explicitly, or when the query or an interval of an equal
    /// scope covers the row, and <see langword="false" /> when nothing does.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="key" /> is not a valid key.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="scope" /> is <see langword="null" />.
    /// </exception>
    public bool Contains(
        DataGridRowKey key,
        long dataIndex,
        DataGridSelectionScope scope)
    {
        ValidateKey(key, nameof(key));
        ArgumentNullException.ThrowIfNull(scope);
        if (ContainsKey(ExcludedKeys, key))
        {
            return false;
        }
        if (ContainsKey(ExplicitKeys, key))
        {
            return true;
        }
        if (AllMatchingQuery?.Equals(scope) == true)
        {
            return true;
        }
        foreach (var interval in IndexIntervals)
        {
            if (interval.Scope.Equals(scope) && interval.Contains(dataIndex))
            {
                return true;
            }
        }
        return false;
    }

    internal DataGridSelectionState WithKey(
        DataGridRowKey key,
        long dataIndex,
        DataGridSelectionScope scope,
        bool isSelected,
        bool single)
    {
        ValidateKey(key, nameof(key));
        if (single)
        {
            if (isSelected)
            {
                return new DataGridSelectionState([key], null, [], []);
            }

            return ContainsKey(ExplicitKeys, key)
                ? Empty
                : this;
        }

        if (isSelected)
        {
            var alreadyCovered = AllMatchingQuery?.Equals(scope) == true ||
                                 IsInInterval(dataIndex, scope);
            var explicitKeys = alreadyCovered || ContainsKey(ExplicitKeys, key)
                ? ExplicitKeys
                : ExplicitKeys.Add(key);
            return new DataGridSelectionState(
                explicitKeys,
                AllMatchingQuery,
                IndexIntervals,
                RemoveKey(ExcludedKeys, key));
        }

        var withoutExplicit = RemoveKey(ExplicitKeys, key);
        var covered = AllMatchingQuery?.Equals(scope) == true ||
                      IsInInterval(dataIndex, scope);
        var exclusions = covered && !ContainsKey(ExcludedKeys, key)
            ? ExcludedKeys.Add(key)
            : ExcludedKeys;
        return new DataGridSelectionState(
            withoutExplicit,
            AllMatchingQuery,
            IndexIntervals,
            exclusions);
    }

    internal DataGridSelectionState WithAllMatching(DataGridSelectionScope scope) =>
        new(
            ExplicitKeys,
            scope,
            ImmutableArray<DataGridSelectionInterval>.Empty,
            ImmutableArray<DataGridRowKey>.Empty);

    internal DataGridSelectionState WithInterval(
        long startIndex,
        long endIndexExclusive,
        DataGridSelectionScope scope) =>
        new(
            ExplicitKeys,
            AllMatchingQuery,
            IndexIntervals.Add(new DataGridSelectionInterval(
                startIndex,
                endIndexExclusive,
                scope)),
            ExcludedKeys);

    internal DataGridSelectionState NormalizeForMode(DataGridSelectionMode mode)
    {
        if (mode == DataGridSelectionMode.None)
        {
            return Empty;
        }
        if (mode == DataGridSelectionMode.Extended || IsEmpty)
        {
            return this;
        }
        return ExplicitKeys.IsEmpty
            ? Empty
            : new DataGridSelectionState([ExplicitKeys[0]], null, [], []);
    }

    internal DataGridSelectionState TransitionTo(
        DataGridSelectionScope nextScope,
        DataGridSelectionTransition transition)
    {
        ArgumentNullException.ThrowIfNull(nextScope);
        if (transition == DataGridSelectionTransition.Filter)
        {
            return ExplicitKeys.IsEmpty
                ? Empty
                : new DataGridSelectionState(ExplicitKeys, null, [], []);
        }

        var allMatching = AllMatchingQuery is null ? null : nextScope;
        var intervals = transition is DataGridSelectionTransition.SortOrGroup or
            DataGridSelectionTransition.Invalidation
            ? ImmutableArray<DataGridSelectionInterval>.Empty
            : RebindIntervals(IndexIntervals, nextScope);
        var exclusions = intervals.IsEmpty && allMatching is null
            ? ImmutableArray<DataGridRowKey>.Empty
            : ExcludedKeys;
        if (ExplicitKeys.IsEmpty && allMatching is null && intervals.IsEmpty)
        {
            return Empty;
        }
        return new DataGridSelectionState(
            ExplicitKeys,
            allMatching,
            intervals,
            exclusions);
    }

    /// <summary>
    /// Returns whether both selections have an equal <see cref="AllMatchingQuery" /> and the same
    /// explicit keys, intervals and excluded keys, each array compared element by element.
    /// </summary>
    public bool Equals(DataGridSelectionState? other) =>
        ReferenceEquals(this, other) ||
        other is not null &&
        Equals(AllMatchingQuery, other.AllMatchingQuery) &&
        SequenceEqual(ExplicitKeys, other.ExplicitKeys) &&
        SequenceEqual(IndexIntervals, other.IndexIntervals) &&
        SequenceEqual(ExcludedKeys, other.ExcludedKeys);

    /// <summary>
    /// Returns whether <paramref name="obj" /> is an equal <see cref="DataGridSelectionState" />.
    /// </summary>
    public override bool Equals(object? obj) =>
        obj is DataGridSelectionState other && Equals(other);

    /// <summary>
    /// Returns a hash code that is equal for equal selections.
    /// </summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(AllMatchingQuery);
        foreach (var key in ExplicitKeys)
        {
            hash.Add(key);
        }
        foreach (var interval in IndexIntervals)
        {
            hash.Add(interval);
        }
        foreach (var key in ExcludedKeys)
        {
            hash.Add(key);
        }
        return hash.ToHashCode();
    }

    private bool IsInInterval(long dataIndex, DataGridSelectionScope scope)
    {
        foreach (var interval in IndexIntervals)
        {
            if (interval.Scope.Equals(scope) && interval.Contains(dataIndex))
            {
                return true;
            }
        }
        return false;
    }

    private static ImmutableArray<DataGridRowKey> NormalizeKeys(
        ImmutableArray<DataGridRowKey> values,
        string parameterName)
    {
        if (values.IsDefaultOrEmpty)
        {
            return ImmutableArray<DataGridRowKey>.Empty;
        }
        var copy = values.ToArray();
        foreach (var value in copy)
        {
            ValidateKey(value, parameterName);
        }
        Array.Sort(copy, DataGridRowKeyComparer.Instance);
        var builder = ImmutableArray.CreateBuilder<DataGridRowKey>(copy.Length);
        foreach (var value in copy)
        {
            if (builder.Count == 0 || builder[^1] != value)
            {
                builder.Add(value);
            }
        }
        return builder.ToImmutable();
    }

    private static ImmutableArray<DataGridSelectionInterval> NormalizeIntervals(
        ImmutableArray<DataGridSelectionInterval> intervals,
        DataGridSelectionScope? allMatching)
    {
        if (intervals.IsDefaultOrEmpty)
        {
            return ImmutableArray<DataGridSelectionInterval>.Empty;
        }
        var copy = intervals.ToArray();
        var scope = copy[0].Scope ??
                    throw new ArgumentException("Selection interval scope cannot be null.", nameof(intervals));
        if (allMatching is not null && !allMatching.Equals(scope))
        {
            throw new ArgumentException(
                "All selection expressions must share one scope.", nameof(intervals));
        }
        foreach (var interval in copy)
        {
            if (interval.Scope is null || !scope.Equals(interval.Scope))
            {
                throw new ArgumentException(
                    "All selection intervals must share one scope.", nameof(intervals));
            }
        }
        Array.Sort(copy, static (left, right) =>
        {
            var start = left.StartIndex.CompareTo(right.StartIndex);
            return start != 0 ? start : left.EndIndexExclusive.CompareTo(right.EndIndexExclusive);
        });

        var builder = ImmutableArray.CreateBuilder<DataGridSelectionInterval>(copy.Length);
        var current = copy[0];
        for (var index = 1; index < copy.Length; index++)
        {
            var next = copy[index];
            if (next.StartIndex <= current.EndIndexExclusive)
            {
                current = new DataGridSelectionInterval(
                    current.StartIndex,
                    Math.Max(current.EndIndexExclusive, next.EndIndexExclusive),
                    scope);
            }
            else
            {
                builder.Add(current);
                current = next;
            }
        }
        builder.Add(current);
        return builder.ToImmutable();
    }

    private static ImmutableArray<DataGridSelectionInterval> RebindIntervals(
        ImmutableArray<DataGridSelectionInterval> intervals,
        DataGridSelectionScope scope)
    {
        if (intervals.IsEmpty)
        {
            return intervals;
        }
        var builder = ImmutableArray.CreateBuilder<DataGridSelectionInterval>(intervals.Length);
        foreach (var interval in intervals)
        {
            builder.Add(new DataGridSelectionInterval(
                interval.StartIndex,
                interval.EndIndexExclusive,
                scope));
        }
        return builder.MoveToImmutable();
    }

    private static bool ContainsKey(
        ImmutableArray<DataGridRowKey> keys,
        DataGridRowKey key) => keys.BinarySearch(key, DataGridRowKeyComparer.Instance) >= 0;

    private static ImmutableArray<DataGridRowKey> RemoveKey(
        ImmutableArray<DataGridRowKey> keys,
        DataGridRowKey key)
    {
        var index = keys.BinarySearch(key, DataGridRowKeyComparer.Instance);
        return index < 0 ? keys : keys.RemoveAt(index);
    }

    private static void ValidateKey(DataGridRowKey key, string parameterName)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException("Selection keys must be valid.", parameterName);
        }
    }

    private static bool SequenceEqual<T>(ImmutableArray<T> left, ImmutableArray<T> right)
        where T : IEquatable<T>
    {
        if (left.Length != right.Length)
        {
            return false;
        }
        for (var index = 0; index < left.Length; index++)
        {
            if (!left[index].Equals(right[index]))
            {
                return false;
            }
        }
        return true;
    }

    private sealed class DataGridRowKeyComparer : IComparer<DataGridRowKey>
    {
        internal static DataGridRowKeyComparer Instance { get; } = new();

        public int Compare(DataGridRowKey left, DataGridRowKey right) => left.CompareTo(right);
    }
}
