using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows edit/delete actions per row; edit puts every editable cell of the row in edit and saves them together.
/// </summary>
public sealed class DataGridOperationColumn : DataGridColumn
{
    private DataGrid? _owningGrid;

    public DataGridOperationColumn()
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
        var operationButtons = new DataGridOperationButtons();
        operationButtons.OwningGrid = OwningGrid;
        operationButtons.BindOwningGridProperties(OwningGrid);
        return operationButtons;
    }

    internal override void ReleaseElement(Control element)
    {
        if (element is DataGridOperationButtons operationButtons)
        {
            operationButtons.ReleaseOwningGridBinding();
        }
    }

    protected override object? PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        return null;
    }

    internal override DataGridColumnHeader CreateHeader()
    {
        DataGridColumnHeader headerCell = base.CreateHeader();
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
            if (GetCellContent(e.Row) is DataGridOperationButtons operationButtons)
            {
                operationButtons.NotifyLoadingRow(e.Row);
            }
        }
    }

    private void HandleUnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is DataGridOperationButtons operationButtons)
            {
                operationButtons.NotifyUnLoadingRow(e.Row);
            }
        }
    }

    protected override void NotifyOwningGridAttached(DataGrid? owningGrid)
    {
        base.NotifyOwningGridAttached(owningGrid);
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
