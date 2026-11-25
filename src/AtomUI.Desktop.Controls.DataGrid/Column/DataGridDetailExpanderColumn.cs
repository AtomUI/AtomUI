using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public sealed class DataGridDetailExpanderColumn : DataGridColumn
{
    private DataGrid? _owningGrid;
    
    public DataGridDetailExpanderColumn()
    {
        IsReadOnly = true;    
    }
    
    protected override Control? GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? editBinding)
    {
        editBinding = null;
        return null;
    }

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

    protected internal override void NotifyOwningGridAboutToDetached()
    {
        base.NotifyOwningGridAboutToDetached();
        ReleaseOwningGrid();
    }
    
    public override bool IsEditable()
    {
        return false;
    }
}
