// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;

namespace AtomUI.Desktop.Controls;

public abstract partial class DataGridColumn : IDataGridColumnGroupItemInternal
{
    #region 常量定义

    internal const bool DefaultIsReadOnly = true;

    #endregion

    #region 属性定义

    internal int Index { get; set; }
    internal bool? CanUserReorderInternal { get; set; }
    internal bool? CanUserResizeInternal { get; set; }
    internal bool? CanUserSortInternal { get; set; }
    internal bool? CanUserFilterInternal { get; set; }
    internal bool? IsSorterTooltipVisibleInternal { get; set; }
    
    DataGridHeaderViewItem? IDataGridColumnGroupItemInternal.GroupHeaderViewItem { get; set; }

    internal bool ActualCanUserResize
    {
        get
        {
            if (OwningGrid == null || OwningGrid.CanUserResizeColumns == false || this is DataGridFillerColumn)
            {
                return false;
            }

            return CanUserResizeInternal ?? true;
        }
    }

    // MaxWidth from local setting or DataGrid setting
    internal double ActualMaxWidth => _maxWidth ?? OwningGrid?.MaxColumnWidth ?? double.PositiveInfinity;

    // MinWidth from local setting or DataGrid setting
    internal double ActualMinWidth
    {
        get
        {
            double minWidth = _minWidth ?? OwningGrid?.MinColumnWidth ?? 0;
            if (Width.IsStar)
            {
                return Math.Max(DataGrid.MinimumStarColumnWidth, minWidth);
            }

            return minWidth;
        }
    }

    internal bool DisplayIndexHasChanged { get; set; }

    internal int DisplayIndexWithFiller
    {
        get => _displayIndexWithFiller;
        set => _displayIndexWithFiller = value;
    }

    internal bool HasHeaderCell => _headerCell != null;

    internal DataGridColumnHeader HeaderCell
    {
        get
        {
            _headerCell ??= CreateHeader();
            return _headerCell;
        }
    }

    /// <summary>
    /// Tracks whether or not this column inherits its Width value from the DataGrid.
    /// </summary>
    internal bool InheritsWidth { get; private set; }

    /// <summary>
    /// When a column is initially added, we won't know its initial desired value
    /// until all rows have been measured.  We use this variable to track whether or
    /// not the column has been fully measured.
    /// </summary>
    internal bool IsInitialDesiredWidthDetermined { get; set; }
    
    internal double LayoutRoundedWidth { get; private set; }

    internal BindingExpressionBase? CellEditBinding => _editBinding;

    IDataGridColumnGroupItem? IDataGridColumnGroupItem.GroupParent { get; set; }
    ObservableCollection<IDataGridColumnGroupItem> IDataGridColumnGroupItem.GroupChildren { get; } = new();

    event EventHandler<DataGridColumnGroupChangedArgs>? IDataGridColumnGroupItem.GroupChanged
    {
        add => _groupChanged += value;
        remove => _groupChanged -= value;
    }

    private EventHandler<DataGridColumnGroupChangedArgs>? _groupChanged;

    #endregion

    private bool? _isReadOnly;
    private double? _maxWidth;
    private double? _minWidth;
    private bool _settingWidthInternally;
    private int _displayIndexWithFiller;
    private object? _header;
    private IDataTemplate? _headerTemplate;
    private DataGridColumnHeader? _headerCell;
    private Control? _editingElement;
    private BindingExpressionBase? _editBinding;
    private ControlTheme? _cellTheme;
    private Classes? _cellStyleClasses;
    private bool _setWidthInternalNoCallback;
    private DataGridColumnSortState _sortState = DataGridColumnSortState.None;

    internal bool EffectiveCanUserSort
    {
        get
        {
            if (OwningGrid is null || this is DataGridFillerColumn)
            {
                return false;
            }

            if (OwningGrid.ItemsSource is not { } source ||
                !(CanUserSortInternal ?? OwningGrid.CanUserSortColumns) ||
                FieldId is not { IsValid: true } queryField ||
                !source.Schema.TryGetField(queryField, out var schemaField) ||
                (SupportedSortDirections & schemaField.SortDirections) == DataGridSortDirections.None ||
                OwningGrid.EditingRow is not null)
            {
                return false;
            }
            return true;
        }
    }

    internal DataGridSortDirections EffectiveSupportedSortDirections
    {
        get
        {
            if (OwningGrid?.ItemsSource is { } source &&
                FieldId is { IsValid: true } queryField &&
                source.Schema.TryGetField(queryField, out var schemaField))
            {
                return SupportedSortDirections & schemaField.SortDirections;
            }
            return DataGridSortDirections.None;
        }
    }

    internal bool EffectiveCanUserFilter
    {
        get
        {
            if (OwningGrid is null || this is DataGridFillerColumn)
            {
                return false;
            }
            return OwningGrid.ItemsSource is { } source &&
                   (CanUserFilterInternal ?? OwningGrid.CanUserFilterColumns) &&
                   FieldId is { IsValid: true } queryField &&
                   source.Schema.TryGetField(queryField, out var schemaField) &&
                   !schemaField.FilterOperators.IsEmpty &&
                   OwningGrid.EditingRow is null;
        }
    }

    internal void SetSortState(DataGridColumnSortState value)
    {
        if (_sortState == value)
        {
            return;
        }

        SetAndRaise(SortStateProperty, ref _sortState, value);
        if (HasHeaderCell)
        {
            HeaderCell.CurrentSortingState = value.Direction switch
            {
                DataGridSortDirection.Ascending => ListSortDirection.Ascending,
                DataGridSortDirection.Descending => ListSortDirection.Descending,
                _ => null
            };
            HeaderCell.UpdatePseudoClasses();
        }
    }

    internal void RefreshSortProjection()
    {
        if (HasHeaderCell)
        {
            HeaderCell.CanUserSort = EffectiveCanUserSort;
            HeaderCell.SupportedSortDirections = EffectiveSupportedSortDirections;
            HeaderCell.CanUserFilter = EffectiveCanUserFilter;
        }
        OwningGrid?.ProjectQueryToColumnsForColumn(this);
    }

    /// <summary>
    /// Gets the value of a cell according to the specified binding.
    /// </summary>
    /// <param name="item">The item associated with a cell.</param>
    /// <param name="binding">The binding to get the value of.</param>
    /// <returns>The resultant cell value.</returns>
    internal object? GetCellValue(object item, BindingBase? binding)
    {
        Debug.Assert(OwningGrid != null);

        object? content = null;
        if (binding != null)
        {
            OwningGrid.ClipboardContentControl.DataContext = item;
            var sub = OwningGrid.ClipboardContentControl.Bind(ContentControl.ContentProperty, binding);
            content = OwningGrid.ClipboardContentControl.GetValue(ContentControl.ContentProperty);
            sub.Dispose();
        }

        return content;
    }

    /// <summary>
    /// Coerces a DataGridLength to a valid value.  If any value components are double.NaN, this method
    /// coerces them to a proper initial value.  For star columns, the desired width is calculated based
    /// on the rest of the star columns.  For pixel widths, the desired value is based on the pixel value.
    /// For auto widths, the desired value is initialized as the column's minimum width.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="width">The DataGridLength to coerce.</param>
    /// <returns>The resultant (coerced) DataGridLength.</returns>
    private static DataGridLength CoerceWidth(AvaloniaObject source, DataGridLength width)
    {
        var target = (DataGridColumn)source;

        if (target._setWidthInternalNoCallback)
        {
            return width;
        }

        if (!target.IsSet(WidthProperty))
        {
            return target.OwningGrid?.ColumnWidth ??
                   DataGridLength.Auto;
        }

        double desiredValue = width.DesiredValue;
        if (double.IsNaN(desiredValue))
        {
            var owningGrid = target.OwningGrid;
            if (width.IsStar && owningGrid != null)
            {
                double totalStarValues           = 0;
                double totalStarDesiredValues    = 0;
                double totalNonStarDisplayWidths = 0;
                var    columns                   = owningGrid.ColumnsInternal;
                int    displayedColumnCount      = columns.GetDisplayedColumnCount();
                for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
                {
                    DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
                    if (!column.IsVisible ||
                        column == target ||
                        double.IsNaN(column.Width.DesiredValue))
                    {
                        continue;
                    }

                    if (column.Width.IsStar)
                    {
                        totalStarValues        += column.Width.Value;
                        totalStarDesiredValues += column.Width.DesiredValue;
                    }
                    else
                    {
                        totalNonStarDisplayWidths += column.ActualWidth;
                    }
                }

                if (totalStarValues == 0)
                {
                    // Compute the new star column's desired value based on the available space if there are no other visible star columns
                    desiredValue = Math.Max(target.ActualMinWidth,
                        owningGrid.CellsWidth - totalNonStarDisplayWidths);
                }
                else
                {
                    // Otherwise, compute its desired value based on those of other visible star columns
                    desiredValue = totalStarDesiredValues * width.Value / totalStarValues;
                }
            }
            else if (width.IsAbsolute)
            {
                desiredValue = width.Value;
            }
            else
            {
                desiredValue = target.ActualMinWidth;
            }
        }

        double displayValue = width.DisplayValue;
        if (double.IsNaN(displayValue))
        {
            displayValue = desiredValue;
        }

        displayValue = Math.Max(target.ActualMinWidth, Math.Min(target.ActualMaxWidth, displayValue));

        return new DataGridLength(width.Value, width.UnitType, desiredValue, displayValue);
    }

    internal void CancelCellEditInternal(Control editingElement, object uneditedValue)
    {
        CancelCellEdit(editingElement, uneditedValue);
    }

    internal void EndCellEditInternal(Control editingElement)
    {
        EndCellEdit(editingElement);
    }

    /// <summary>
    /// If the DataGrid is using layout rounding, the pixel snapping will force all widths to
    /// whole numbers. Since the column widths aren't visual elements, they don't go through the normal
    /// rounding process, so we need to do it ourselves.  If we don't, then we'll end up with some
    /// pixel gaps and/or overlaps between columns.
    /// </summary>
    /// <param name="leftEdge"></param>
    internal void ComputeLayoutRoundedWidth(double leftEdge)
    {
        if (OwningGrid != null && OwningGrid.UseLayoutRounding)
        {
            var scale     = LayoutHelper.GetLayoutScale(HeaderCell);
            var roundSize = LayoutHelper.RoundLayoutSizeUp(new Size(leftEdge + ActualWidth, 1), scale);
            LayoutRoundedWidth = roundSize.Width - leftEdge;
        }
        else
        {
            LayoutRoundedWidth = ActualWidth;
        }
    }

    internal virtual DataGridColumnHeader CreateHeader()
    {
        var result = new DataGridColumnHeader
        {
            OwningColumn = this
        };
        Debug.Assert(OwningGrid != null);

        result[!DataGridColumnHeader.ContentProperty]                 = this[!HeaderProperty];
        result[!DataGridColumnHeader.ContentTemplateProperty]         = this[!HeaderTemplateProperty];
        result[!DataGridColumnHeader.SizeTypeProperty]                = OwningGrid[!DataGrid.SizeTypeProperty];
        result[!DataGridColumnHeader.IsMotionEnabledProperty]         = OwningGrid[!DataGrid.IsMotionEnabledProperty];
        result[!DataGridColumnHeader.SupportedSortDirectionsProperty] = this[!SupportedSortDirectionsProperty];
        result[!DataGridColumnHeader.HorizontalContentAlignmentProperty] = this[!HeaderContentHorizontalAlignmentProperty];
        result[!DataGridColumnHeader.VerticalContentAlignmentProperty] = this[!HeaderContentVerticalAlignmentProperty];
        result.CanUserSort = EffectiveCanUserSort;
        result.SupportedSortDirections = EffectiveSupportedSortDirections;
        result.CurrentSortingState = SortState.Direction switch
        {
            DataGridSortDirection.Ascending => ListSortDirection.Ascending,
            DataGridSortDirection.Descending => ListSortDirection.Descending,
            _ => null
        };
        return result;
    }

    /// <summary>
    /// Ensures that this column's width has been coerced to a valid value.
    /// </summary>
    internal void EnsureWidth()
    {
        SetWidthInternalNoCallback(CoerceWidth(this, Width));
    }

    internal Control? GenerateElementInternal(DataGridCell cell, object dataItem)
    {
        return GenerateElement(cell, dataItem);
    }

    internal object? PrepareCellForEditInternal(Control editingElement, RoutedEventArgs editingEventArgs)
    {
        var result = PrepareCellForEdit(editingElement, editingEventArgs);
        editingElement.Focus();

        return result;
    }

    /// <summary>
    /// Attempts to resize the column's width to the desired DisplayValue, but limits the final size
    /// to the column's minimum and maximum values.  If star sizing is being used, then the column
    /// can only decrease in size by the amount that the columns after it can increase in size.
    /// Likewise, the column can only increase in size if other columns can spare the width.
    /// </summary>
    /// <param name="oldWidth">with before resize.</param>
    /// <param name="newWidth">with after resize.</param>
    /// <param name="userInitiated">Whether or not this resize was initiated by a user action.</param>

    //  double value, DataGridLengthUnitType unitType, double desiredValue, double displayValue
    internal void Resize(DataGridLength oldWidth, DataGridLength newWidth, bool userInitiated)
    {
        Debug.Assert(OwningGrid != null);
        double newValue = newWidth.Value;
        double newDesiredValue = newWidth.DesiredValue;
        double newDisplayValue = Math.Max(ActualMinWidth, Math.Min(ActualMaxWidth, newWidth.DisplayValue));
        DataGridLengthUnitType newUnitType = newWidth.UnitType;

        int    starColumnsCount  = 0;
        double totalDisplayWidth = 0;
        var    columns           = OwningGrid.ColumnsInternal;
        int    displayedColumnCount = columns.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
            {
                continue;
            }

            column.EnsureWidth();
            totalDisplayWidth += column.ActualWidth;
            starColumnsCount  += (column != this && column.Width.IsStar) ? 1 : 0;
        }

        bool hasInfiniteAvailableWidth = !OwningGrid.HasFiniteColumnViewport;

        // If we're using star sizing, we can only resize the column as much as the columns to the
        // right will allow (i.e. until they hit their max or min widths).
        if (!hasInfiniteAvailableWidth && (starColumnsCount > 0 ||
                                           (newUnitType == DataGridLengthUnitType.Star && newWidth.IsStar &&
                                            userInitiated)))
        {
            double limitedDisplayValue = oldWidth.DisplayValue;
            double availableIncrease   = Math.Max(0, OwningGrid.CellsWidth - totalDisplayWidth);
            double desiredChange       = newDisplayValue - oldWidth.DisplayValue;
            if (desiredChange > availableIncrease)
            {
                // The desired change is greater than the amount of available space,
                // so we need to decrease the widths of columns to the right to make room.
                desiredChange -= availableIncrease;
                double actualChange = desiredChange +
                                      OwningGrid.DecreaseColumnWidths(DisplayIndex + 1, -desiredChange, userInitiated);
                limitedDisplayValue += availableIncrease + actualChange;
            }
            else if (desiredChange > 0)
            {
                // The desired change is positive but less than the amount of available space,
                // so there's no need to decrease the widths of columns to the right.
                limitedDisplayValue += desiredChange;
            }
            else
            {
                // The desired change is negative, so we need to increase the widths of columns to the right.
                limitedDisplayValue += desiredChange +
                                       OwningGrid.IncreaseColumnWidths(DisplayIndex + 1, -desiredChange, userInitiated);
            }

            if (ActualCanUserResize || (oldWidth.IsStar && !userInitiated))
            {
                newDisplayValue = limitedDisplayValue;
            }
        }

        if (userInitiated)
        {
            newDesiredValue = newDisplayValue;
            if (!Width.IsStar)
            {
                InheritsWidth = false;
                newValue      = newDisplayValue;
                newUnitType   = DataGridLengthUnitType.Pixel;
            }
            else if (starColumnsCount > 0 && !hasInfiniteAvailableWidth)
            {
                // Recalculate star weight of this column based on the new desired value
                InheritsWidth = false;
                newValue      = Width.Value * newDisplayValue / ActualWidth;
            }
        }
  
        newValue = Math.Min(double.MaxValue, newValue);
        newWidth = new DataGridLength(newValue, newUnitType, newDesiredValue, newDisplayValue);
        SetWidthInternalNoCallback(newWidth);
        // TODO 不知道为什么如果这里不强制通知会造成 Header 的分割线渲染宽度丢失
        // newWidth != oldWidth 命名加上了这个，不知道为啥
        OwningGrid.HandleColumnWidthChanged(this);
    }

    /// <summary>
    /// Sets the column's Width to a new DataGridLength with a different DesiredValue.
    /// </summary>
    /// <param name="desiredValue">The new DesiredValue.</param>
    internal void SetWidthDesiredValue(double desiredValue)
    {
        SetWidthInternalNoCallback(new DataGridLength(Width.Value, Width.UnitType, desiredValue, Width.DisplayValue));
    }

    /// <summary>
    /// Sets the column's Width to a new DataGridLength with a different DisplayValue.
    /// </summary>
    /// <param name="displayValue">The new DisplayValue.</param>
    internal void SetWidthDisplayValue(double displayValue)
    {
        SetWidthInternalNoCallback(new DataGridLength(Width.Value, Width.UnitType, Width.DesiredValue, displayValue));
    }

    /// <summary>
    /// Set the column's Width without breaking inheritance.
    /// </summary>
    /// <param name="width">The new Width.</param>
    internal void SetWidthInternal(DataGridLength width)
    {
        bool originalValue = _settingWidthInternally;
        _settingWidthInternally = true;
        try
        {
            SetCurrentValue(WidthProperty, width);
        }
        finally
        {
            _settingWidthInternally = originalValue;
        }
    }

    /// <summary>
    /// Sets the column's Width directly, without any callback effects.
    /// </summary>
    /// <param name="width">The new Width.</param>
    internal void SetWidthInternalNoCallback(DataGridLength width)
    {
        var originalValue = _setWidthInternalNoCallback;
        _setWidthInternalNoCallback = true;
        try
        {
            SetCurrentValue(WidthProperty, width);
        }
        finally
        {
            _setWidthInternalNoCallback = originalValue;
        }
    }

    /// <summary>
    /// Set the column's star value.  Whenever the star value changes, width inheritance is broken.
    /// </summary>
    /// <param name="value">The new star value.</param>
    internal void SetWidthStarValue(double value)
    {
        InheritsWidth = false;
        SetWidthInternalNoCallback(new DataGridLength(value, Width.UnitType, Width.DesiredValue, Width.DisplayValue));
    }

    //TODO Binding
    internal Control? GenerateEditingElementInternal(DataGridCell cell, object dataItem)
    {
        if (_editingElement == null)
        {
            _editingElement = GenerateEditingElement(cell, dataItem, out _editBinding);
        }

        return _editingElement;
    }

    /// <summary>
    /// Clears the cached editing element.
    /// </summary>
    //TODO Binding
    internal void RemoveEditingElement()
    {
        _editingElement = null;
    }

}
