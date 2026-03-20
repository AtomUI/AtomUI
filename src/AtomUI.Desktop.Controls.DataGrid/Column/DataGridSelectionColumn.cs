using System.Collections.Specialized;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace AtomUI.Desktop.Controls;

public class DataGridSelectionColumn : DataGridColumn
{
    private DataGrid? _owningGrid;
    private CheckBox? _headerCheckBox;
    private CompositeDisposable? _headerBindingDisposables;

    public override bool IsReadOnly => true;
    
    protected override Control? GenerateEditingElement(DataGridCell cell, object dataItem, out ICellEditBinding? editBinding)
    {
        editBinding = null;
        return null;
    }

    protected override Control GenerateElement(DataGridCell cell, object dataItem)
    {
        EnsureOwningGrid();
        Debug.Assert(_owningGrid != null);
        Control? selector;
        if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
        {
            selector = BuildRadioButton();
        }
        else
        {
            selector = BuildCheckBox();
        }
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
                _owningGrid.SelectionChanged          += HandleSelectionChanged;
                _owningGrid.PropertyChanged           += HandleDataGridPropertyChanged;
            }
            return true;
        }
        return false;
    }
    
    private void HandleColumnsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove && e.OldItems.Contains(this) && _owningGrid != null)
            {
                _owningGrid.Columns.CollectionChanged -= HandleColumnsCollectionChanged;
                _owningGrid.LoadingRow                -= HandleLoadingRow;
                _owningGrid.SelectionChanged          -= HandleSelectionChanged;
                _owningGrid.PropertyChanged           -= HandleDataGridPropertyChanged;
                _owningGrid                           =  null;
            }
        }
    }

    private void HandleDataGridPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs change)
    {
        if (_owningGrid != null)
        {
            if (change.Property == DataGrid.SelectionModeProperty)
            {
                foreach (var row in _owningGrid.GetAllRows())
                {
                    var cell = row.Cells[Index];
                    if (_owningGrid.SelectionMode == DataGridSelectionMode.Single)
                    {
                        cell.Content = BuildRadioButton();
                    }
                    else
                    {
                        cell.Content = BuildCheckBox();
                    }
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
                    }
                }
            }
        }
    }

    private void HandleSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (_owningGrid == null)
        {
            return;
        }

        if (_owningGrid.SelectionMode == DataGridSelectionMode.Extended)
        {
            if (_headerCheckBox != null)
            {
                if (_owningGrid.IsAllRowSelected())
                {
                    _headerCheckBox.IsChecked = true;
                } 
                else if (_owningGrid.SelectedItems.Count > 0)
                {
                    _headerCheckBox.IsChecked = null;
                } 
                else if (_owningGrid.SelectedItems.Count == 0)
                {
                    _headerCheckBox.IsChecked = false;
                }
            }
        }

        foreach (var item in e.AddedItems)
        {
            var content = GetCellContent(item);
            if (content == null)
            {
                continue;
            }
            
            if (content is CheckBox checkBox)
            {
                checkBox.IsChecked = true;
            }
            else if (content is RadioButton radioButton)
            {
                radioButton.IsChecked = true;
            }
        }
        foreach (var item in e.RemovedItems)
        {
            var content = GetCellContent(item);
            if (content == null)
            {
                continue;
            }
            
            if (content is CheckBox checkBox)
            {
                checkBox.IsChecked = false;
            }
            else if (content is RadioButton radioButton)
            {
                radioButton.IsChecked = false;
            }
        }
    }

    private void HandleLoadingRow(object? sender, DataGridRowEventArgs e)
    {
        if (OwningGrid != null)
        {
            if (GetCellContent(e.Row) is CheckBox checkBox)
            {
                checkBox.IsChecked = e.Row.IsSelected;
            }
            else if (GetCellContent(e.Row) is RadioButton radioButton)
            {
                radioButton.IsChecked = e.Row.IsSelected;
            }
        }
    }
    
    internal override DataGridColumnHeader CreateHeader()
    {
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
            
            _headerBindingDisposables?.Dispose();
            _headerBindingDisposables = new CompositeDisposable();
            _headerBindingDisposables.Add(BindUtils.RelayBind(OwningGrid, DataGrid.SizeTypeProperty, header, DataGridColumnHeader.SizeTypeProperty));
            _headerBindingDisposables.Add(BindUtils.RelayBind(this, SupportedSortDirectionsProperty, header, DataGridColumnHeader.SupportedSortDirectionsProperty));
            _headerBindingDisposables.Add(BindUtils.RelayBind(this, HeaderContentHorizontalAlignmentProperty, header, DataGridColumnHeader.HorizontalContentAlignmentProperty));
            _headerBindingDisposables.Add(BindUtils.RelayBind(this, HeaderContentVerticalAlignmentProperty, header, DataGridColumnHeader.VerticalContentAlignmentProperty));
            _headerBindingDisposables.Add(BindUtils.RelayBind(OwningGrid, DataGrid.IsMotionEnabledProperty, header, DataGridColumnHeader.IsMotionEnabledProperty));
            
            _headerCheckBox       =  new CheckBox();
            _headerCheckBox.Click += HandleSelectedAllChanged;
            header.Content        =  _headerCheckBox;
        }
        return header;
    }

    private void HandleSelectedAllChanged(object? sender, EventArgs e)
    {
        if (sender is CheckBox checkBox)
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