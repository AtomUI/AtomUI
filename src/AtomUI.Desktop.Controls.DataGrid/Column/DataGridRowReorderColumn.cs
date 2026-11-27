using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A column whose cells show a handle for dragging their row to another position.
/// The owning <see cref="DataGrid" /> has to allow row reordering through
/// <see cref="DataGrid.CanUserReorderRows" /> and takes only one column of this type: attaching the column to
/// a grid that does not allow reordering, or adding a second one, throws. The column never edits its cells.
/// </summary>
public sealed class DataGridRowReorderColumn : DataGridColumn
{
    private DataGrid? _owningGrid;
    
    public DataGridRowReorderColumn()
    {
        IsReadOnly = true;    
    }
    
    /// <summary>
    /// The column never edits its cells, so it creates no editing element.
    /// </summary>
    /// <returns>
    /// Always <see langword="null" />; <paramref name="editBinding" /> is set to <see langword="null" />.
    /// </returns>
    protected override Control? GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? editBinding)
    {
        editBinding = null;
        return null;
    }

    /// <summary>
    /// Creates the drag handle for the row of the cell. The handle is not focusable and is bound to the owning
    /// grid.
    /// </summary>
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        Debug.Assert(OwningGrid != null);
        var handle = new DataGridRowReorderHandle();
        handle.OwningGrid                                         = OwningGrid;
        handle.Focusable                                          = false;
        handle.BindOwningGridProperties(OwningGrid);
        return handle;
    }

    internal override void ReleaseElement(Control element)
    {
        if (element is DataGridRowReorderHandle handle)
        {
            handle.ReleaseOwningGridBindings();
        }
    }

    /// <summary>
    /// Returns <see langword="null" /> without using the arguments, because a cell of this column never enters
    /// editing mode.
    /// </summary>
    protected override object? PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        return null;
    }
    
    internal override DataGridColumnHeader CreateHeader()
    { 
        DataGridColumnHeader headerCell = base.CreateHeader();
        headerCell.IsEnabled              = false;
        headerCell.IndicatorLayoutVisible = false;
        return headerCell;
    }
    
    private void ConfigureOwningGrid()
    {
        if (OwningGrid != null)
        {
            if (OwningGrid != _owningGrid)
            {
                _owningGrid                           =  OwningGrid;
                _owningGrid.Columns.CollectionChanged += HandleColumnsCollectionChanged;
                _owningGrid.LoadingRow                += HandleLoadingRow;
                _owningGrid.UnloadingRow              += HandleUnLoadingRow;
            }
        }
    }
    
    private void HandleColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_owningGrid != null && RemovedItemsContain(e, this))
        {
            ReleaseOwningGrid();
        }

        EnsureOnlyOneReorderColumn();
    }

    private void ReleaseOwningGrid()
    {
        if (_owningGrid == null)
        {
            return;
        }

        _owningGrid.CancelRowReorder();
        _owningGrid.Columns.CollectionChanged -= HandleColumnsCollectionChanged;
        _owningGrid.LoadingRow                -= HandleLoadingRow;
        _owningGrid.UnloadingRow              -= HandleUnLoadingRow;
        _owningGrid                           =  null;
    }

    private void EnsureOnlyOneReorderColumn()
    {
        // The grid allows only one row reorder column.
        if (_owningGrid != null)
        {
            var reorderColumnCount = 0;
            for (var i = 0; i < _owningGrid.Columns.Count; i++)
            {
                if (_owningGrid.Columns[i] is DataGridRowReorderColumn)
                {
                    reorderColumnCount++;
                    if (reorderColumnCount > 1)
                    {
                        throw DataGridError.DataGridRow.RowReorderColumnAlreadyExistException();
                    }
                }
            }
        }
    }
    
    private void HandleLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is DataGridRowReorderHandle handle)
            {
                handle.NotifyLoadingRow(e.Row);
            }
        }
    }
    
    private void HandleUnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is DataGridRowReorderHandle handle)
            {
                handle.NotifyUnLoadingRow(e.Row);
            }
        }
    }
    
    /// <summary>
    /// Called whenever <see cref="DataGridColumn.OwningGrid" /> is assigned. Calls the base member and rejects a
    /// grid that does not allow row reordering, and then subscribes this column to the column collection and row
    /// loading events of the grid.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// <paramref name="owningGrid" /> is not <see langword="null" /> and its
    /// <see cref="DataGrid.CanUserReorderRows" /> is <see langword="false" />.
    /// </exception>
    protected override void NotifyOwningGridAttached(DataGrid? owningGrid)
    {
        base.NotifyOwningGridAttached(owningGrid);
        if (owningGrid != null)
        {
            if (!owningGrid.CanUserReorderRows)
            {
                throw DataGridError.DataGridRow.RowReorderNotAllowedException();
            }
        }

        ConfigureOwningGrid();
    }
    
    /// <summary>
    /// Called before the column is detached from its owning grid. Calls the base member and then releases the
    /// grid: cancels a row reorder in progress and unsubscribes from its column collection and row loading
    /// events.
    /// </summary>
    protected internal override void NotifyOwningGridAboutToDetached()
    {
        base.NotifyOwningGridAboutToDetached();
        ReleaseOwningGrid();
    }
    
    /// <summary>
    /// The column only reorders rows and never edits its cells.
    /// </summary>
    /// <returns>
    /// Always <see langword="false" />.
    /// </returns>
    public override bool IsEditable()
    {
        return false;
    }
}
