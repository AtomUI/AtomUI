using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public sealed class DataGridRowReorderColumn : DataGridColumn
{
    private DataGrid? _owningGrid;
    
    public DataGridRowReorderColumn()
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
        // 检查只能有一列排序列
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
