using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A column that selects rows with a selector in the cell of each row.
/// The selector is a radio button while the owning <see cref="DataGrid" /> uses
/// <see cref="DataGridSelectionMode.Single" /> and a check box otherwise, and it is checked while the row is
/// selected.
/// </summary>
/// <remarks>
/// Clicking the selector selects the row, or removes a selected row from the selection outside single selection
/// mode. Outside single selection mode the header also shows a check box that selects all rows or clears the
/// selection; in extended selection mode it shows the indeterminate state while only some rows are selected.
/// The column never edits its cells.
/// </remarks>
public class DataGridSelectionColumn : DataGridColumn
{
    private DataGrid? _owningGrid;
    private CheckBox? _headerCheckBox;
    private readonly HashSet<DataGridRow> _trackedRows = new();

    /// <summary>
    /// Always <see langword="true" />. The override declares no setter: the cells only show a selector and are
    /// never edited.
    /// </summary>
    public override bool IsReadOnly => true;
    
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
    /// Creates the selector that the cell shows: a radio button while the owning grid uses
    /// <see cref="DataGridSelectionMode.Single" /> and a check box otherwise.
    /// The selector is centered in the cell, the cell does not clip it, and it is checked when the row of the cell
    /// is selected.
    /// </summary>
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        EnsureOwningGrid();
        Debug.Assert(_owningGrid != null);
        Control selector;
        if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
        {
            selector = BuildRadioButton();
        }
        else
        {
            selector = BuildCheckBox();
        }
        SyncSelectorCheckedState(selector, cell.OwningRow?.IsSelected ?? false);
        cell.SetCurrentValue(DataGridCell.IsClipContentProperty, false);
        return selector;
    }

    private CheckBox BuildCheckBox()
    {
        var checkBoxElement = new SelectionCheckBox()
        {
            IsThreeState = false
        };
        checkBoxElement.HorizontalAlignment = HorizontalAlignment.Center;
        checkBoxElement.VerticalAlignment   = VerticalAlignment.Center;
        return checkBoxElement;
    }

    private RadioButton BuildRadioButton()
    {
        var radioButton = new SelectionRadioButton
        {
            IsThreeState = false
        };
        radioButton.HorizontalAlignment =  HorizontalAlignment.Center;
        radioButton.VerticalAlignment   =  VerticalAlignment.Center;
        return radioButton;
    }
    
    /// <summary>
    /// Returns <see langword="null" /> without using the arguments, because a cell of this column never enters
    /// editing mode.
    /// </summary>
    protected override object? PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        return null;
    }

    internal bool NotifyAboutToUpdateSelection(PointerPressedEventArgs e, DataGridCell cell)
    {
        Debug.Assert(_owningGrid != null);
        var   control = cell.Content as Control;
        Debug.Assert(control != null);
        Point position = e.GetPosition(control);
        Rect  rect     = new Rect(control.Bounds.Size);
        return rect.Contains(position);
    }

    internal DataGridSelectionAction GetSelectionAction(DataGridCell cell)
    {
        Debug.Assert(_owningGrid != null);
        if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
        {
            return DataGridSelectionAction.SelectCurrent;
        }
        var   checkBox = cell.Content as CheckBox;
        Debug.Assert(checkBox != null);
        if (checkBox.IsChecked.HasValue && !checkBox.IsChecked.Value)
        {
            if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
            {
                return DataGridSelectionAction.SelectCurrent;
            }

            return DataGridSelectionAction.AddCurrentToSelection;
        }

        return DataGridSelectionAction.RemoveCurrentFromSelection;
    }
    
    private bool EnsureOwningGrid()
    {
        if (OwningGrid != null)
        {
            if (OwningGrid != _owningGrid)
            {
                _owningGrid                           =  OwningGrid;
                _owningGrid.Columns.CollectionChanged += HandleColumnsCollectionChanged;
                _owningGrid.LoadingRow                += HandleLoadingRow;
                _owningGrid.UnloadingRow              += HandleUnLoadingRow;
                _owningGrid.SelectionChanged          += HandleSelectionChanged;
                _owningGrid.PropertyChanged           += HandleDataGridPropertyChanged;
            }
            return true;
        }
        return false;
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
        _owningGrid.SelectionChanged          -= HandleSelectionChanged;
        _owningGrid.PropertyChanged           -= HandleDataGridPropertyChanged;
        UntrackRows();
        _owningGrid                           =  null;
    }

    private void HandleDataGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (_owningGrid != null)
        {
            if (change.Property == DataGrid.ItemsSourceProperty)
            {
                SyncHeaderCheckBoxState();
            }
            else if (change.Property == DataGrid.SelectionProperty ||
                     change.Property == DataGrid.TotalItemCountProperty)
            {
                SyncHeaderCheckBoxState();
            }
            else if (change.Property == DataGrid.SelectionModeProperty)
            {
                foreach (var row in _owningGrid.GetAllRows())
                {
                    var cell = row.Cells[Index];
                    Control selector;
                    if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
                    {
                        selector = BuildRadioButton();
                    }
                    else
                    {
                        selector = BuildCheckBox();
                    }
                    SyncSelectorCheckedState(selector, row.IsSelected);
                    cell.Content = selector;
                }

                if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
                {
                    if (_headerCheckBox != null)
                    {
                        _headerCheckBox.IsVisible = false;
                        _headerCheckBox.IsChecked = false;
                    }
                }
                else
                {
                    if (_headerCheckBox != null)
                    {
                        _headerCheckBox.IsVisible = true;
                        SyncHeaderCheckBoxState();
                    }
                }
            }
        }
    }

    private void HandleSelectionChanged(object? sender, DataGridSelectionChangedEventArgs e)
    {
        if (_owningGrid == null)
        {
            return;
        }

        SyncHeaderCheckBoxState();

        foreach (var row in _owningGrid.GetAllRows())
        {
            SyncSelectorCheckedState(row.Cells[Index].Content as Control, row.IsSelected);
        }
    }

    private void HandleLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            TrackRow(e.Row);
            SyncRowSelectorCheckedState(e.Row);
        }
    }

    private void HandleUnLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        UntrackRow(e.Row);
    }

    /// <summary>
    /// Called before the column is detached from its owning grid. Calls the base member and then releases the
    /// grid: unsubscribes from its column collection, row loading and selection events, and stops observing the
    /// rows.
    /// </summary>
    protected internal override void NotifyOwningGridAboutToDetached()
    {
        base.NotifyOwningGridAboutToDetached();
        ReleaseOwningGrid();
    }
    
    internal override DataGridColumnHeader CreateHeader()
    {
        EnsureOwningGrid();
        DataGridColumnHeader? header = null;
        if (OwningGrid == null || OwningGrid.SelectionMode == DataGridSelectionMode.Single)
        {
            header                        = base.CreateHeader();
            header.IndicatorLayoutVisible = false;
        }
        else
        {
            header = new DataGridColumnHeader
            {
                OwningColumn           = this,
                IndicatorLayoutVisible = false
            };

            header.BindOwningGridProperties(OwningGrid);
            header[!DataGridColumnHeader.SupportedSortDirectionsProperty]    = this[!SupportedSortDirectionsProperty];
            header[!DataGridColumnHeader.HorizontalContentAlignmentProperty] = this[!HeaderContentHorizontalAlignmentProperty];
            header[!DataGridColumnHeader.VerticalContentAlignmentProperty] = this[!HeaderContentVerticalAlignmentProperty];
            
            _headerCheckBox = new SelectionHeaderCheckBox(this);
            header.Content  = _headerCheckBox;
            SyncHeaderCheckBoxState();
        }
        return header;
    }

    private void SyncSelectorCheckedState(Control? selector, bool isSelected)
    {
        if (selector is CheckBox checkBox)
        {
            checkBox.IsChecked = isSelected;
        }
        else if (selector is RadioButton radioButton)
        {
            radioButton.IsChecked = isSelected;
        }
    }

    private void SyncRowSelectorCheckedState(DataGridRow row)
    {
        if (_owningGrid == null || row.OwningGrid != _owningGrid)
        {
            return;
        }

        SyncSelectorCheckedState(GetCellContent(row), row.IsSelected);
        SyncHeaderCheckBoxState();
    }

    private void SyncHeaderCheckBoxState()
    {
        if (_headerCheckBox == null || _owningGrid == null)
        {
            return;
        }

        if (_owningGrid.SelectionMode != DataGridSelectionMode.Extended)
        {
            _headerCheckBox.IsChecked = false;
            return;
        }

        if (_owningGrid.IsAllRowSelected())
        {
            _headerCheckBox.IsChecked = true;
        }
        else if (!_owningGrid.Selection.IsEmpty)
        {
            _headerCheckBox.IsChecked = null;
        }
        else
        {
            _headerCheckBox.IsChecked = false;
        }
    }

    private void TrackRow(DataGridRow row)
    {
        if (_trackedRows.Add(row))
        {
            row.PropertyChanged += HandleRowPropertyChanged;
        }
    }

    private void UntrackRow(DataGridRow row)
    {
        if (_trackedRows.Remove(row))
        {
            row.PropertyChanged -= HandleRowPropertyChanged;
        }
    }

    private void UntrackRows()
    {
        foreach (var row in _trackedRows)
        {
            row.PropertyChanged -= HandleRowPropertyChanged;
        }
        _trackedRows.Clear();
    }

    private void HandleRowPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == DataGridRow.IsSelectedProperty && sender is DataGridRow row)
        {
            SyncRowSelectorCheckedState(row);
        }
    }

    internal void HandleSelectedAllChanged(CheckBox checkBox)
    {
        if (checkBox.IsChecked == false)
        {
            OwningGrid?.ClearRowSelection(true);
        }
        else if (checkBox.IsChecked == true)
        {
            OwningGrid?.SelectAll();
        }
    }
}

internal class SelectionHeaderCheckBox : CheckBox
{
    private readonly DataGridSelectionColumn _owningColumn;

    static SelectionHeaderCheckBox()
    {
        Button.ClickEvent.AddClassHandler<SelectionHeaderCheckBox>(
            (checkBox, _) => checkBox._owningColumn.HandleSelectedAllChanged(checkBox));
    }

    public SelectionHeaderCheckBox(DataGridSelectionColumn owningColumn)
    {
        _owningColumn = owningColumn;
    }

    protected override Type StyleKeyOverride { get; } = typeof(CheckBox);
}

internal class SelectionRadioButton : RadioButton
{
    protected override Type StyleKeyOverride { get; } = typeof(RadioButton);
    
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = false;
    }
}

internal class SelectionCheckBox : CheckBox
{
    protected override Type StyleKeyOverride { get; } = typeof(CheckBox);
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        e.Handled = false;
    }
}
