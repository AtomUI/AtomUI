using System.Collections.Immutable;

namespace AtomUI.Desktop.Controls;

public partial class DataGrid
{
    internal long QueryRevision { get; private set; }

    /// <summary>
    /// Loads the rows of the current query again from the source, anchored to the first fully displayed row so
    /// that the view stays where it is. Does nothing while range presentation is not attached or no source is set.
    /// </summary>
    public void Reload() => BeginRangeLoad(
        isInvalidation: false,
        CaptureFirstCompleteRangeAnchor());

    /// <summary>
    /// Sets the sort of one field in the current query, or removes it, and reports the change as a sort gesture.
    /// Nothing happens when the resulting query equals the current one. Otherwise an open edit is committed first
    /// and the change is dropped when that commit fails, then the new query is loaded from the first page.
    /// </summary>
    /// <param name="field">
    /// The field to sort by.
    /// </param>
    /// <param name="direction">
    /// The sort direction, which must be one that the field allows, or <see langword="null" /> to remove the
    /// sort of the field.
    /// </param>
    /// <param name="mode">
    /// <see cref="DataGridSortUpdateMode.Replace" /> makes the field the only sort.
    /// <see cref="DataGridSortUpdateMode.AppendOrReplace" /> replaces the sort of the field, or appends it.
    /// </param>
    /// <exception cref="DataGridSourceContractException">
    /// <c>ItemsSource</c> is set and its schema does not contain <paramref name="field" />.
    /// </exception>
    public void SetSort(
        DataGridFieldId field,
        DataGridSortDirection? direction,
        DataGridSortUpdateMode mode = DataGridSortUpdateMode.Replace)
    {
        var allowedDirections = DataGridSortDirections.All;
        if (ItemsSource is not null)
        {
            if (!ItemsSource.Schema.TryGetField(field, out var schemaField))
            {
                throw new DataGridSourceContractException(
                    $"Source schema does not contain field '{field}'.");
            }
            allowedDirections = schemaField.SortDirections;
        }
        var updated = DataGridSortPolicy.Apply(
            Query, field, direction, mode, allowedDirections);
        SetQuery(updated, DataGridQueryChangeReason.SortGesture);
    }

    /// <summary>
    /// Removes all sorts from the current query and reports the change as a sort gesture.
    /// The side effects are those of <see cref="SetSort" />.
    /// </summary>
    public void ClearSorts() => SetQuery(
        Query.WithSorts(ImmutableArray<DataGridSort>.Empty),
        DataGridQueryChangeReason.SortGesture);

    internal void ApplySortGesture(
        DataGridFieldId field,
        bool append,
        DataGridSortDirections allowedDirections)
    {
        if (allowedDirections == DataGridSortDirections.None)
        {
            return;
        }
        SetQuery(
            DataGridSortPolicy.ApplyGesture(Query, field, append, allowedDirections),
            DataGridQueryChangeReason.SortGesture);
    }

    internal void ApplyFilterGesture(
        DataGridColumn column,
        IReadOnlyList<object> values)
    {
        if (ItemsSource is null ||
            !column.EffectiveCanUserFilter ||
            column.FieldId is not { IsValid: true } field ||
            !ItemsSource.Schema.TryGetField(field, out var schemaField))
        {
            return;
        }

        var oldIndex = FindFilter(Query.Filters, field);
        if (values.Count == 0)
        {
            if (oldIndex >= 0)
            {
                SetQuery(
                    Query.WithFilters(RemoveFilterAt(Query.Filters, oldIndex)),
                    DataGridQueryChangeReason.FilterGesture);
            }
            return;
        }
        if (schemaField.FilterOperators.IsEmpty)
        {
            return;
        }

        var filterOperator = schemaField.FilterOperators[0];
        if (values.Count < filterOperator.MinimumValueCount ||
            values.Count > filterOperator.MaximumValueCount)
        {
            throw new DataGridSourceContractException(
                $"Filter '{filterOperator.Id}' expects between " +
                $"{filterOperator.MinimumValueCount} and {filterOperator.MaximumValueCount} values.");
        }

        var scalarValues = ImmutableArray.CreateBuilder<DataGridScalar>(values.Count);
        foreach (var value in values)
        {
            if (!DataGridScalar.TryFromValue(value, out var scalar))
            {
                throw new DataGridSourceContractException(
                    $"Filter value type '{value.GetType()}' cannot be represented by DataGridScalar.");
            }
            scalarValues.Add(scalar);
        }
        var replacement = new DataGridFilter(field, filterOperator.Id, scalarValues.MoveToImmutable());
        var filters = oldIndex < 0
            ? Query.Filters.Add(replacement)
            : Query.Filters.SetItem(oldIndex, replacement);
        SetQuery(Query.WithFilters(filters), DataGridQueryChangeReason.FilterGesture);
    }

    /// <summary>
    /// Collapses the group with the given key. An open edit is committed first, and the change is dropped when
    /// that commit fails.
    /// </summary>
    /// <param name="key">
    /// The <see cref="DataGridGroupKey" /> of the group to collapse.
    /// </param>
    public void CollapseGroup(DataGridGroupKey key) =>
        SetGroupExpansion(GroupExpansion.Collapse(key), key);

    /// <summary>
    /// Expands the group with the given key. An open edit is committed first, and the change is dropped when
    /// that commit fails.
    /// </summary>
    /// <param name="key">
    /// The <see cref="DataGridGroupKey" /> of the group to expand.
    /// </param>
    public void ExpandGroup(DataGridGroupKey key) =>
        SetGroupExpansion(GroupExpansion.Expand(key), key);

    private void SetQuery(DataGridQuery? value, DataGridQueryChangeReason reason)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (Query == value)
        {
            return;
        }
        var groupsChanged = !DataGridQueryValidation.SequenceEqual(Query.Groups, value.Groups);
        var nextExpansion = groupsChanged
            ? DataGridGroupExpansion.AllExpanded
            : GroupExpansion;
        var nextPageRequest = PageRequest is { } pageRequest && pageRequest.DataStartIndex != 0
            ? new DataGridPageRequest(0, pageRequest.DataCount)
            : PageRequest;
        ValidateRangeSourceScope(ItemsSource, value, nextPageRequest, nextExpansion);
        if (EditingRow is not null && !CommitEdit())
        {
            return;
        }

        _pendingRangeNavigation = null;
        _rangeBringIntoViewIntent = checked(_rangeBringIntoViewIntent + 1);
        CancelRowReorder();

        var oldQuery = Query;
        var revision = checked(QueryRevision + 1);
        QueryRevision = revision;
        _rangeDesiredOffset = 0;
        if (PageRequest != nextPageRequest)
        {
            SetPageRequestValue(nextPageRequest);
        }
        if (GroupExpansion != nextExpansion)
        {
            SetAndRaise(GroupExpansionProperty, ref _groupExpansion, nextExpansion);
        }
        SetAndRaise(QueryProperty, ref _query, value);
        ProjectQueryToColumns(oldQuery, value);
        QueryChanged?.Invoke(
            this,
            new DataGridQueryChangedEventArgs(oldQuery, value, revision, reason));

        if (QueryRevision != revision || Query != value)
        {
            return;
        }
        BeginRangeLoad(isInvalidation: false);
    }

    private void SetGroupExpansion(DataGridGroupExpansion? value) =>
        SetGroupExpansion(value, null);

    private void SetGroupExpansion(
        DataGridGroupExpansion? value,
        DataGridGroupKey? actedGroupKey)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (GroupExpansion == value)
        {
            return;
        }
        ValidateRangeSourceScope(ItemsSource, Query, PageRequest, value);
        if (EditingRow is not null && !CommitEdit())
        {
            return;
        }
        _pendingRangeNavigation = null;
        _rangeBringIntoViewIntent = checked(_rangeBringIntoViewIntent + 1);
        CancelRowReorder();
        var anchor = actedGroupKey is { } key
            ? CaptureRangeGroupAnchor(key)
            : CaptureFirstCompleteRangeAnchor();
        SetAndRaise(GroupExpansionProperty, ref _groupExpansion, value);
        BeginRangeLoad(isInvalidation: false, anchor);
    }

    private void SetPageRequest(DataGridPageRequest? value)
    {
        if (PageRequest == value)
        {
            return;
        }
        ValidateRangeSourceScope(ItemsSource, Query, value, GroupExpansion);
        if (EditingRow is not null && !CommitEdit())
        {
            return;
        }

        _pendingRangeNavigation = null;
        _rangeBringIntoViewIntent = checked(_rangeBringIntoViewIntent + 1);
        _rangeDesiredOffset = 0;
        CancelRowReorder();
        SetPageRequestValue(value);
        BeginRangeLoad(isInvalidation: false);
        SyncRangePaginationState();
    }

    private void SetPageRequestValue(DataGridPageRequest? value)
    {
        SetAndRaise(PageRequestProperty, ref _pageRequest, value);
        var pageSize = value?.DataCount ?? 0;
        if (PageSize != pageSize)
        {
            SetCurrentValue(PageSizeProperty, pageSize);
        }
        ConfigurePaginationVisibility();
    }

    private void ProjectQueryToColumns(DataGridQuery oldQuery, DataGridQuery newQuery)
    {
        foreach (var column in ColumnsItemsInternal)
        {
            var oldState = GetColumnSortState(oldQuery, column.FieldId);
            var newState = GetColumnSortState(newQuery, column.FieldId);
            if (oldState != newState || column.SortState != newState)
            {
                column.SetSortState(newState);
                UpdateRealizedSortState(column, newState.Direction is not null);
            }

            var oldFilter = GetColumnFilter(oldQuery, column.FieldId);
            var newFilter = GetColumnFilter(newQuery, column.FieldId);
            if (oldFilter != newFilter && column.HasHeaderCell)
            {
                column.HeaderCell.NotifySelectedFilterValuesChanged();
            }
        }
    }

    private static DataGridFilter? GetColumnFilter(
        DataGridQuery query,
        DataGridFieldId? field)
    {
        if (field is not { IsValid: true } validField)
        {
            return null;
        }
        var index = FindFilter(query.Filters, validField);
        return index < 0 ? null : query.Filters[index];
    }

    private static int FindFilter(
        ImmutableArray<DataGridFilter> filters,
        DataGridFieldId field)
    {
        for (var index = 0; index < filters.Length; index++)
        {
            if (filters[index].Field == field)
            {
                return index;
            }
        }
        return -1;
    }

    private static ImmutableArray<DataGridFilter> RemoveFilterAt(
        ImmutableArray<DataGridFilter> filters,
        int removeIndex)
    {
        var builder = ImmutableArray.CreateBuilder<DataGridFilter>(filters.Length - 1);
        for (var index = 0; index < filters.Length; index++)
        {
            if (index != removeIndex)
            {
                builder.Add(filters[index]);
            }
        }
        return builder.MoveToImmutable();
    }

    internal void ProjectQueryToColumnsForColumn(DataGridColumn column)
    {
        var state = GetColumnSortState(Query, column.FieldId);
        column.SetSortState(state);
        UpdateRealizedSortState(column, state.Direction is not null);
    }

    internal void UpdateRealizedSortState(DataGridColumn column, bool isSorting)
    {
        if (column.Index < 0)
        {
            return;
        }
        foreach (var row in DisplayData.GetScrollingRows().Cast<DataGridRow>())
        {
            if (column.Index < row.Cells.Count)
            {
                row.Cells[column.Index].IsSorting = isSorting;
            }
        }
    }

    private static DataGridColumnSortState GetColumnSortState(
        DataGridQuery query,
        DataGridFieldId? field)
    {
        if (field is not { IsValid: true } validField)
        {
            return DataGridColumnSortState.None;
        }
        for (var priority = 0; priority < query.Sorts.Length; priority++)
        {
            var sort = query.Sorts[priority];
            if (sort.Field == validField)
            {
                return new DataGridColumnSortState(sort.Direction, priority);
            }
        }
        return DataGridColumnSortState.None;
    }

    private void RollBackRangeIntent(DataGridPresentationSnapshot fallback)
    {
        var queryChanged = Query != fallback.Query;
        if (!queryChanged &&
            PageRequest == fallback.PageRequest &&
            GroupExpansion == fallback.GroupExpansion)
        {
            return;
        }
        SetPageRequestValue(fallback.PageRequest);
        SetAndRaise(GroupExpansionProperty, ref _groupExpansion, fallback.GroupExpansion);
        if (queryChanged)
        {
            var oldQuery = Query;
            var revision = checked(QueryRevision + 1);
            QueryRevision = revision;
            SetAndRaise(QueryProperty, ref _query, fallback.Query);
            ProjectQueryToColumns(oldQuery, fallback.Query);
            QueryChanged?.Invoke(
                this,
                new DataGridQueryChangedEventArgs(
                    oldQuery,
                    fallback.Query,
                    revision,
                    DataGridQueryChangeReason.LoadRollback));
        }
        SyncRangePaginationState();
    }
}
