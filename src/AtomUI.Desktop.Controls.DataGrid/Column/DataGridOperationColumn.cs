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

    /// <summary>
    /// Returns <see langword="null" />, because the cells of this column have no editing element.
    /// </summary>
    /// <param name="cell">
    /// The cell that requested the editing element.
    /// </param>
    /// <param name="dataItem">
    /// The data item of the row that owns the cell.
    /// </param>
    /// <param name="editBinding">
    /// Always <see langword="null" />.
    /// </param>
    /// <returns>
    /// Always <see langword="null" />.
    /// </returns>
    protected override Control? GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? editBinding)
    {
        editBinding = null;
        return null;
    }

    /// <summary>
    /// Creates the operation buttons that a cell of this column shows, and binds them to the property changes
    /// of the owning grid.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the element.
    /// </param>
    /// <param name="dataItem">
    /// The data item of the row that owns the cell.
    /// </param>
    /// <returns>
    /// A new <see cref="DataGridOperationButtons" /> instance.
    /// </returns>
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

    /// <summary>
    /// Returns <see langword="null" /> without reading the editing element, because this column never edits.
    /// </summary>
    /// <param name="editingElement">
    /// The editing element of the cell. This column does not generate one.
    /// </param>
    /// <param name="editingEventArgs">
    /// The event arguments of the request that started the edit.
    /// </param>
    /// <returns>
    /// Always <see langword="null" />.
    /// </returns>
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

    /// <summary>
    /// Calls the base implementation, then attaches the handlers of this column to the owning grid through
    /// <c>ConfigureOwningGrid</c>.
    /// </summary>
    protected override void NotifyOwningGridAttached(DataGrid? owningGrid)
    {
        base.NotifyOwningGridAttached(owningGrid);
        ConfigureOwningGrid();
    }

    /// <summary>
    /// Calls the base implementation, then detaches the handlers of this column from the owning grid through
    /// <c>ReleaseOwningGrid</c>.
    /// </summary>
    protected internal override void NotifyOwningGridAboutToDetached()
    {
        base.NotifyOwningGridAboutToDetached();
        ReleaseOwningGrid();
    }

    /// <summary>
    /// Returns <see langword="false" />, because the cells of this column cannot be edited.
    /// </summary>
    /// <returns>
    /// Always <see langword="false" />.
    /// </returns>
    public override bool IsEditable()
    {
        return false;
    }
}
