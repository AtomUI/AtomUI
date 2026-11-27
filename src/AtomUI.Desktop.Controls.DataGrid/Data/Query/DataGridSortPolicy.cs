using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Builds the sort list of a <see cref="DataGridQuery" /> for a sort request or a sort gesture.
/// A sort gesture advances one field through its allowed directions.
/// </summary>
public static class DataGridSortPolicy
{
    /// <summary>
    /// Adds a sort for one field, changes its direction or removes it, and returns the updated query.
    /// </summary>
    /// <param name="query">
    /// The query whose sort list is updated. It is not modified.
    /// </param>
    /// <param name="field">
    /// The field to sort by.
    /// </param>
    /// <param name="requestedDirection">
    /// The direction to sort <paramref name="field" /> in, or <see langword="null" /> to remove its sort.
    /// </param>
    /// <param name="mode">
    /// <see cref="DataGridSortUpdateMode.Replace" /> makes the field the only sort.
    /// <see cref="DataGridSortUpdateMode.AppendOrReplace" /> keeps the other sorts: the sort of the field is
    /// replaced in place, or the field is appended as the last sort.
    /// </param>
    /// <param name="allowedDirections">
    /// The directions the field may be sorted in. This argument is not validated when
    /// <paramref name="requestedDirection" /> is <see langword="null" />.
    /// </param>
    /// <returns>
    /// A query with the updated sort list, or <paramref name="query" /> itself when nothing changes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="field" /> is not a valid field identity.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="mode" /> is not a known <see cref="DataGridSortUpdateMode" /> value.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="allowedDirections" /> contains flags outside <see cref="DataGridSortDirections.All" />,
    /// or <paramref name="requestedDirection" /> is not a known <see cref="DataGridSortDirection" /> value.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="allowedDirections" /> is <see cref="DataGridSortDirections.None" />, or does not include
    /// <paramref name="requestedDirection" />.
    /// </exception>
    public static DataGridQuery Apply(
        DataGridQuery query,
        DataGridFieldId field,
        DataGridSortDirection? requestedDirection,
        DataGridSortUpdateMode mode,
        DataGridSortDirections allowedDirections)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateField(field);
        ValidateMode(mode);

        var existingIndex = Find(query.Sorts, field);
        if (requestedDirection is null)
        {
            return existingIndex < 0 ? query : query.WithSorts(RemoveAt(query.Sorts, existingIndex));
        }

        ValidateAllowedDirections(allowedDirections);
        ValidateRequestedDirection(requestedDirection.Value, allowedDirections);
        var replacement = new DataGridSort(field, requestedDirection.Value);

        if (mode == DataGridSortUpdateMode.Replace)
        {
            if (query.Sorts.Length == 1 && query.Sorts[0] == replacement)
            {
                return query;
            }
            return query.WithSorts([replacement]);
        }

        if (existingIndex >= 0)
        {
            if (query.Sorts[existingIndex] == replacement)
            {
                return query;
            }
            return query.WithSorts(query.Sorts.SetItem(existingIndex, replacement));
        }
        return query.WithSorts(query.Sorts.Add(replacement));
    }

    /// <summary>
    /// Advances the sort of one field by one step of its direction cycle and returns the updated query.
    /// </summary>
    /// <param name="query">
    /// The query whose sort list is updated. It is not modified.
    /// </param>
    /// <param name="field">
    /// The field to sort by.
    /// </param>
    /// <param name="append">
    /// <see langword="true" /> keeps the sorts of the other fields: the field is appended as the last sort, or
    /// its sort is replaced in place, or it is removed when the cycle ends.
    /// <see langword="false" /> makes the field the only sort, or removes every sort when the cycle ends.
    /// </param>
    /// <param name="allowedDirections">
    /// The directions the field may be sorted in. They define the cycle.
    /// </param>
    /// <returns>
    /// A query with the updated sort list.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="query" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="field" /> is not a valid field identity.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="allowedDirections" /> contains flags outside <see cref="DataGridSortDirections.All" />.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// <paramref name="allowedDirections" /> is <see cref="DataGridSortDirections.None" />.
    /// </exception>
    /// <remarks>
    /// The cycle follows <paramref name="allowedDirections" />. A field that is not sorted turns
    /// <see cref="DataGridSortDirection.Ascending" /> when that direction is allowed, and otherwise
    /// <see cref="DataGridSortDirection.Descending" />. A field that is sorted ascending turns
    /// <see cref="DataGridSortDirection.Descending" /> when that direction is allowed; a field that is sorted
    /// descending, or ascending while descending is not allowed, ends the cycle unsorted.
    /// </remarks>
    public static DataGridQuery ApplyGesture(
        DataGridQuery query,
        DataGridFieldId field,
        bool append,
        DataGridSortDirections allowedDirections)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidateField(field);
        ValidateAllowedDirections(allowedDirections);

        var existingIndex = Find(query.Sorts, field);
        var nextDirection = existingIndex < 0
            ? FirstDirection(allowedDirections)
            : NextDirection(query.Sorts[existingIndex].Direction, allowedDirections);

        if (!append)
        {
            return nextDirection is null
                ? query.WithSorts(ImmutableArray<DataGridSort>.Empty)
                : Apply(
                    query,
                    field,
                    nextDirection,
                    DataGridSortUpdateMode.Replace,
                    allowedDirections);
        }

        return nextDirection is null
            ? existingIndex < 0
                ? query
                : query.WithSorts(RemoveAt(query.Sorts, existingIndex))
            : Apply(
                query,
                field,
                nextDirection,
                DataGridSortUpdateMode.AppendOrReplace,
                allowedDirections);
    }

    private static DataGridSortDirection FirstDirection(DataGridSortDirections allowedDirections) =>
        (allowedDirections & DataGridSortDirections.Ascending) != 0
            ? DataGridSortDirection.Ascending
            : DataGridSortDirection.Descending;

    private static DataGridSortDirection? NextDirection(
        DataGridSortDirection current,
        DataGridSortDirections allowedDirections)
    {
        DataGridQueryValidation.ValidateDirection(current, nameof(current));
        return current switch
        {
            DataGridSortDirection.Ascending
                when (allowedDirections & DataGridSortDirections.Descending) != 0 =>
                DataGridSortDirection.Descending,
            DataGridSortDirection.Ascending => null,
            DataGridSortDirection.Descending => null,
            _ => null
        };
    }

    private static int Find(ImmutableArray<DataGridSort> sorts, DataGridFieldId field)
    {
        for (var index = 0; index < sorts.Length; index++)
        {
            if (sorts[index].Field == field)
            {
                return index;
            }
        }
        return -1;
    }

    private static ImmutableArray<DataGridSort> RemoveAt(
        ImmutableArray<DataGridSort> sorts,
        int removeIndex)
    {
        var builder = ImmutableArray.CreateBuilder<DataGridSort>(sorts.Length - 1);
        for (var index = 0; index < sorts.Length; index++)
        {
            if (index != removeIndex)
            {
                builder.Add(sorts[index]);
            }
        }
        return builder.MoveToImmutable();
    }

    private static void ValidateField(DataGridFieldId field)
    {
        if (!field.IsValid)
        {
            throw new ArgumentException("The field identity must be valid.", nameof(field));
        }
    }

    private static void ValidateMode(DataGridSortUpdateMode mode)
    {
        if (mode is not DataGridSortUpdateMode.Replace and not DataGridSortUpdateMode.AppendOrReplace)
        {
            throw new ArgumentOutOfRangeException(nameof(mode), mode, "Unknown sort update mode.");
        }
    }

    private static void ValidateAllowedDirections(DataGridSortDirections allowedDirections)
    {
        if ((allowedDirections & ~DataGridSortDirections.All) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(allowedDirections), allowedDirections, "Unknown allowed sort direction flags.");
        }
        if (allowedDirections == DataGridSortDirections.None)
        {
            throw new ArgumentException("At least one sort direction must be allowed.", nameof(allowedDirections));
        }
    }

    private static void ValidateRequestedDirection(
        DataGridSortDirection requestedDirection,
        DataGridSortDirections allowedDirections)
    {
        DataGridQueryValidation.ValidateDirection(requestedDirection, nameof(requestedDirection));
        var requiredFlag = requestedDirection == DataGridSortDirection.Ascending
            ? DataGridSortDirections.Ascending
            : DataGridSortDirections.Descending;
        if ((allowedDirections & requiredFlag) == 0)
        {
            throw new ArgumentException(
                $"Direction '{requestedDirection}' is not allowed.", nameof(requestedDirection));
        }
    }
}
