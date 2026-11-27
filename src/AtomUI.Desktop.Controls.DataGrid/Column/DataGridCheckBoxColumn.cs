// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A column that displays the bound value as a check box in each cell. Only the check box of the current
/// cell is enabled.
/// </summary>
public class DataGridCheckBoxColumn : DataGridBoundColumn
{
    private CheckBox? _currentCheckBox;
    private DataGrid? _owningGrid;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataGridCheckBoxColumn" /> class.
    /// </summary>
    public DataGridCheckBoxColumn()
    {
        BindingTarget = CheckBox.IsCheckedProperty;
        IsReadOnly    = false;
    }

    /// <summary>
    /// Defines the <see cref="IsThreeState" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsThreeStateProperty =
        CheckBox.IsThreeStateProperty.AddOwner<DataGridCheckBoxColumn>();

    /// <summary>
    /// Gets or sets a value that indicates whether the hosted <see cref="T:System.Windows.Controls.CheckBox" /> controls allow three states or two. 
    /// </summary>
    /// <returns>
    /// true if the hosted controls support three states; false if they support two states. The default is false. 
    /// </returns>
    public bool IsThreeState
    {
        get => GetValue(IsThreeStateProperty);
        set => SetValue(IsThreeStateProperty, value);
    }

    /// <summary>
    /// Calls the base implementation, and reports a change of <see cref="IsThreeState" /> through
    /// <c>NotifyPropertyChanged</c>.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsThreeStateProperty)
        {
            NotifyPropertyChanged(change.Property.Name);
        }
    }

    /// <summary>
    /// Causes the column cell being edited to revert to the specified value.
    /// </summary>
    /// <param name="editingElement">
    /// The element that the column displays for a cell in editing mode.
    /// </param>
    /// <param name="uneditedValue">
    /// The previous, unedited value in the cell being edited.
    /// </param>
    protected override void CancelCellEdit(Control editingElement, object uneditedValue)
    {
        if (editingElement is CheckBox editingCheckBox)
        {
            editingCheckBox.IsChecked = (bool?)uneditedValue;
        }
    }

    ///  <summary>
    ///  Gets a <see cref="T:System.Windows.Controls.CheckBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.
    ///  </summary>
    ///  <param name="cell">
    ///  The cell that will contain the generated element.
    ///  </param>
    ///  <param name="dataItem">
    ///  The data item represented by the row that contains the intended cell.
    /// </param>
    ///  <returns>
    ///  A new <see cref="T:System.Windows.Controls.CheckBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.
    ///  </returns>
    protected override Control GenerateEditingElementDirect(DataGridCell cell, object dataItem)
    {
        var checkBox = new CheckBox
        {
            Margin = new Thickness(0)
        };
        ConfigureCheckBox(checkBox);
        return checkBox;
    }

    /// <summary>                
    /// Gets a read-only <see cref="T:System.Windows.Controls.CheckBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the generated element.
    /// </param>
    /// <param name="dataItem">
    /// The data item represented by the row that contains the intended cell.
    /// </param>
    /// <returns>
    /// A new, read-only <see cref="T:System.Windows.Controls.CheckBox" /> control that is bound to the column's <see cref="P:System.Windows.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </returns>
    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        bool     isEnabled       = false;
        CheckBox checkBoxElement = new CheckBox();
        if (EnsureOwningGrid())
        {
            Debug.Assert(OwningGrid != null);
            if (cell.RowIndex != -1 && cell.ColumnIndex != -1 &&
                cell.OwningRow != null &&
                cell.OwningRow.Slot == OwningGrid.CurrentSlot &&
                cell.ColumnIndex == OwningGrid.CurrentColumnIndex)
            {
                isEnabled = true;
                if (_currentCheckBox != null)
                {
                    _currentCheckBox.IsEnabled = false;
                }
                _currentCheckBox = checkBoxElement;
            }
        }
        checkBoxElement.IsEnabled        = isEnabled;
        checkBoxElement.IsHitTestVisible = false;
        ConfigureCheckBox(checkBoxElement);
        if (Binding != null && BindingTarget != null)
        {
            checkBoxElement.Bind(BindingTarget, Binding);
        }
        return checkBoxElement;
    }

    /// <summary>
    /// Called when a cell in the column enters editing mode.
    /// </summary>
    /// <param name="editingElement">
    /// The element that the column displays for a cell in editing mode.
    /// </param>
    /// <param name="editingEventArgs">
    /// Information about the user gesture that is causing a cell to enter editing mode.
    /// </param>
    /// <returns>
    /// The unedited value. 
    /// </returns>
    protected override object? PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        if (editingElement is CheckBox editingCheckBox)
        {
            void EditValue()
            {
                // User clicked the checkbox itself or pressed space, let's toggle the IsChecked value
                if (editingCheckBox.IsThreeState)
                {
                    switch (editingCheckBox.IsChecked)
                    {
                        case false:
                            editingCheckBox.IsChecked = true;
                            break;
                        case true:
                            editingCheckBox.IsChecked = null;
                            break;
                        case null:
                            editingCheckBox.IsChecked = false;
                            break;
                    }
                }
                else
                {
                    editingCheckBox.IsChecked = !editingCheckBox.IsChecked;
                }
            }

            bool? uneditedValue = editingCheckBox.IsChecked;
            if (editingEventArgs is PointerPressedEventArgs args)
            {
                void ProcessPointerArgs()
                {
                    // Editing was triggered by a mouse click
                    Point position = args.GetPosition(editingCheckBox);
                    Rect  rect     = new Rect(0, 0, editingCheckBox.Bounds.Width, editingCheckBox.Bounds.Height);
                    if (rect.Contains(position))
                    {
                        EditValue();
                    }
                }
                    
                if (editingCheckBox.Bounds.Width == 0 && editingCheckBox.Bounds.Height == 0)
                {
                    IDisposable? boundsSubscription = null;
                    boundsSubscription = editingCheckBox.GetObservable(Visual.BoundsProperty).Subscribe(bounds =>
                    {
                        if (bounds.Width != 0 || bounds.Height != 0)
                        {
                            Dispatcher.UIThread.Post(ProcessPointerArgs);
                            boundsSubscription?.Dispose();
                        }
                    });
                }
                else
                {
                    ProcessPointerArgs();
                }
            }

            return uneditedValue;
        }
        return false;
    }

    /// <summary>
    /// Called by the DataGrid control when this column asks for its elements to be
    /// updated, because its CheckBoxContent or IsThreeState property changed.
    /// </summary>
    protected internal override void RefreshCellContent(Control element, string propertyName)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }
        if (element is CheckBox checkBox)
        {
            DataGridHelper.SyncColumnProperty(this, checkBox, IsThreeStateProperty);
        }
        else
        {
            throw DataGridError.DataGrid.ValueIsNotAnInstanceOf("element", typeof(CheckBox));
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
        _owningGrid.CurrentCellChanged        -= HandleCurrentCellChanged;
        _owningGrid.KeyDown                   -= HandleKeyDown;
        _owningGrid.LoadingRow                -= HandleLoadingRow;
        _owningGrid                           =  null;
        _currentCheckBox                      =  null;
    }

    private void ConfigureCheckBox(CheckBox checkBox)
    {
        checkBox.HorizontalAlignment = HorizontalAlignment.Center;
        checkBox.VerticalAlignment   = VerticalAlignment.Center;
        DataGridHelper.SyncColumnProperty(this, checkBox, IsThreeStateProperty);
    }

    private bool EnsureOwningGrid()
    {
        if (OwningGrid != null)
        {
            if (OwningGrid != _owningGrid)
            {
                _owningGrid                           =  OwningGrid;
                _owningGrid.Columns.CollectionChanged += HandleColumnsCollectionChanged;
                _owningGrid.CurrentCellChanged        += HandleCurrentCellChanged;
                _owningGrid.KeyDown                   += HandleKeyDown;
                _owningGrid.LoadingRow                += HandleLoadingRow;
            }
            return true;
        }
        return false;
    }

    private void HandleCurrentCellChanged(object? sender, EventArgs e)
    {
        if (_currentCheckBox != null)
        {
            _currentCheckBox.IsEnabled = false;
        }
        if (OwningGrid != null && OwningGrid.CurrentColumn == this
                               && OwningGrid.IsSlotVisible(OwningGrid.CurrentSlot))
        {
            if (OwningGrid.DisplayData.GetDisplayedElement(OwningGrid.CurrentSlot) is DataGridRow row)
            {
                CheckBox? checkBox = GetCellContent(row) as CheckBox;
                if (checkBox != null)
                {
                    checkBox.IsEnabled = true;
                }
                _currentCheckBox = checkBox;
            }
        }
    }

    private void HandleKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space && OwningGrid != null &&
            OwningGrid.CurrentColumn == this)
        {
            if (OwningGrid.DisplayData.GetDisplayedElement(OwningGrid.CurrentSlot) is DataGridRow row)
            {
                CheckBox? checkBox = GetCellContent(row) as CheckBox;
                if (checkBox == _currentCheckBox)
                {
                    // TODO Review: verify that Space on the current cell is meant to start editing through BeginEdit,
                    // with the event sender as the routed event source, and correct it where it is not. Remove this
                    // TODO when that is done.
                    OwningGrid.BeginEdit(new RoutedEventArgs()
                    {
                        Source = sender
                    });
                }
            }
        }
    }

    private void HandleLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is CheckBox checkBox)
            {
                if (OwningGrid.CurrentColumnIndex == Index && OwningGrid.CurrentSlot == e.Row.Slot)
                {
                    if (_currentCheckBox != null)
                    {
                        _currentCheckBox.IsEnabled = false;
                    }
                    checkBox.IsEnabled = true;
                    _currentCheckBox   = checkBox;
                }
                else
                {
                    checkBox.IsEnabled = false;
                }
            }
        }
    }

    /// <summary>
    /// Calls the base implementation, then releases the handlers this column attached to the owning grid
    /// through <c>ReleaseOwningGrid</c>.
    /// </summary>
    protected internal override void NotifyOwningGridAboutToDetached()
    {
        base.NotifyOwningGridAboutToDetached();
        ReleaseOwningGrid();
    }
}
