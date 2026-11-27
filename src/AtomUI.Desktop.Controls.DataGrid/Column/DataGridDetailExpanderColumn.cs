using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A column whose cells show a toggle for the row details of their row.
/// The owning <see cref="DataGrid" /> has to use <see cref="DataGridRowDetailsVisibilityMode.Collapsed" />;
/// attaching the column to a grid with another mode throws. The column never edits its cells.
/// </summary>
public sealed class DataGridDetailExpanderColumn : DataGridColumn
{
    private DataGrid? _owningGrid;
    
    public DataGridDetailExpanderColumn()
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
    /// Creates the toggle that expands and collapses the row details of the cell's row, bound to the owning grid.
    /// </summary>
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        Debug.Assert(OwningGrid != null);
        var expander = new DataGridRowExpander();
        expander.BindOwningGridProperties(OwningGrid);
        return expander;
    }

    internal override void ReleaseElement(Control element)
    {
        if (element is DataGridRowExpander expander)
        {
            expander.ReleaseOwningGridBinding();
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
    }

    private void ReleaseOwningGrid()
    {
        if (_owningGrid == null)
        {
            return;
        }

        _owningGrid.Columns.CollectionChanged -= HandleColumnsCollectionChanged;
        _owningGrid.LoadingRow                -= HandleLoadingRow;
        _owningGrid.UnloadingRow              -= HandleUnLoadingRow;
        _owningGrid                           =  null;
    }
    
    private void HandleLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is DataGridRowExpander expander)
            {
                expander.NotifyLoadingRow(e.Row);
            }
        }
    }
    
    private void HandleUnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is DataGridRowExpander expander)
            {
                expander.NotifyUnLoadingRow(e.Row);
            }
        }
    }
    
    /// <summary>
    /// Called whenever <see cref="DataGridColumn.OwningGrid" /> is assigned. Calls the base member and subscribes
    /// this column to the column collection and row loading events of the grid, and rejects a grid whose row
    /// details are not collapsed by default.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// <paramref name="owningGrid" /> is not <see langword="null" /> and its
    /// <see cref="DataGrid.RowDetailsVisibilityMode" /> is not
    /// <see cref="DataGridRowDetailsVisibilityMode.Collapsed" />.
    /// </exception>
    protected override void NotifyOwningGridAttached(DataGrid? owningGrid)
    {
        base.NotifyOwningGridAttached(owningGrid);
        ConfigureOwningGrid();
        if (owningGrid != null)
        {
            if (owningGrid.RowDetailsVisibilityMode != DataGridRowDetailsVisibilityMode.Collapsed)
            {
                throw DataGridError.DataGridColumn.RowDetailsVisibilityModeException();
            }
        }
    }

    /// <summary>
    /// Called before the column is detached from its owning grid. Calls the base member and then unsubscribes
    /// this column from the column collection and row loading events of the grid.
    /// </summary>
    protected internal override void NotifyOwningGridAboutToDetached()
    {
        base.NotifyOwningGridAboutToDetached();
        ReleaseOwningGrid();
    }
    
    /// <summary>
    /// The column only toggles the row details and never edits its cells.
    /// </summary>
    /// <returns>
    /// Always <see langword="false" />.
    /// </returns>
    public override bool IsEditable()
    {
        return false;
    }
}
