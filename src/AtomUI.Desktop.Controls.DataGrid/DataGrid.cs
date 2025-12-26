// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Data;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Styling;
using Avalonia.Utilities;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.Invalid, DataGridPseudoClass.EmptyRows, DataGridPseudoClass.EmptyColumns)]
public partial class DataGrid : TemplatedControl,
                                ISizeTypeAware,
                                IMotionAwareControl,
                                IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGrid>();
    
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsOperating));
    
    public static readonly StyledProperty<string?> OperatingMsgProperty =
        AvaloniaProperty.Register<DataGrid, string?>(nameof(OperatingMsg));
    
    public static readonly StyledProperty<object?> CustomOperatingIndicatorProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(CustomOperatingIndicator));

    public static readonly StyledProperty<IDataTemplate?> CustomOperatingIndicatorTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(CustomOperatingIndicatorTemplate));

    public static readonly StyledProperty<bool> IsShowFrameBorderProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsShowFrameBorder), false);

    public static readonly StyledProperty<bool> CanUserReorderColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserReorderColumns));
    
    public static readonly StyledProperty<bool> CanUserResizeColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserResizeColumns));
    
    public static readonly StyledProperty<bool> CanUserSortColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserSortColumns), false);
    
    public static readonly StyledProperty<bool> CanUserFilterColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserFilterColumns), false);
    
    public static readonly StyledProperty<bool> CanUserReorderRowsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserReorderRows), false);
    
    public static readonly StyledProperty<bool> ShowSorterTooltipProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(ShowSorterTooltip), true);
    
    public static readonly StyledProperty<double> ColumnHeaderHeightProperty = 
        AvaloniaProperty.Register<DataGrid, double>(nameof(ColumnHeaderHeight), defaultValue: double.NaN, validate: IsValidColumnHeaderHeight);
    
    public static readonly StyledProperty<DataGridLength> ColumnWidthProperty =
        AvaloniaProperty.Register<DataGrid, DataGridLength>(nameof(ColumnWidth), defaultValue: DataGridLength.Auto);
    
    public static readonly StyledProperty<ControlTheme> RowGroupThemeProperty =
        AvaloniaProperty.Register<DataGrid, ControlTheme>(nameof(RowGroupTheme));

    public static readonly StyledProperty<int> LeftFrozenColumnCountProperty =
        AvaloniaProperty.Register<DataGrid, int>(
            nameof(LeftFrozenColumnCount),
            validate: ValidateFrozenColumnCount);
    
    public static readonly StyledProperty<int> RightFrozenColumnCountProperty =
        AvaloniaProperty.Register<DataGrid, int>(
            nameof(RightFrozenColumnCount),
            validate: ValidateFrozenColumnCount);

    public static readonly StyledProperty<DataGridGridLinesVisibility> GridLinesVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, DataGridGridLinesVisibility>(nameof(GridLinesVisibility),
            DataGridGridLinesVisibility.Horizontal);

    public static readonly StyledProperty<DataGridHeadersVisibility> HeadersVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, DataGridHeadersVisibility>(nameof(HeadersVisibility),
            DataGridHeadersVisibility.Column);

    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility));

    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsReadOnly));

    public static readonly StyledProperty<bool> IsRowGroupHeadersFrozenProperty =
        AvaloniaProperty.Register<DataGrid, bool>(
            nameof(IsRowGroupHeadersFrozen),
            defaultValue: true);

    public static readonly DirectProperty<DataGrid, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, bool>(
            nameof(IsValid),
            o => o.IsValid);

    public static readonly StyledProperty<double> MaxColumnWidthProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(MaxColumnWidth),
            defaultValue: DefaultMaxColumnWidth,
            validate: IsValidColumnWidth);

    public static readonly StyledProperty<double> MinColumnWidthProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(MinColumnWidth),
            defaultValue: DefaultMinColumnWidth,
            validate: IsValidMinColumnWidth);

    public static readonly AttachedProperty<bool> IsScrollInertiaEnabledProperty =
        ScrollViewer.IsScrollInertiaEnabledProperty.AddOwner<DataGrid>();

    public static readonly StyledProperty<double> RowHeightProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(RowHeight),
            defaultValue: double.NaN,
            validate: IsValidRowHeight);

    public static readonly StyledProperty<double> RowHeaderWidthProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(RowHeaderWidth),
            defaultValue: double.NaN,
            validate: IsValidRowHeaderWidth);
    
    public static readonly StyledProperty<IDataTemplate?> RowHeaderContentTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(RowHeaderContentTemplate));

    public static readonly StyledProperty<DataGridSelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridSelectionMode>(nameof(SelectionMode), DataGridSelectionMode.None);
    
    public static readonly StyledProperty<DataGridSelectTriggerType> SelectTriggerTypeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridSelectTriggerType>(nameof(SelectTriggerType), DataGridSelectTriggerType.SelectIndicator);

    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, ScrollBarVisibility>(nameof(VerticalScrollBarVisibility));

    public static readonly DirectProperty<DataGrid, int> SelectedIndexProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, int>(
            nameof(SelectedIndex),
            o => o.SelectedIndex,
            (o, v) => o.SelectedIndex = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly DirectProperty<DataGrid, object?> SelectedItemProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, object?>(
            nameof(SelectedItem),
            o => o.SelectedItem,
            (o, v) => o.SelectedItem = v,
            defaultBindingMode: BindingMode.TwoWay);

    public static readonly StyledProperty<DataGridClipboardCopyMode> ClipboardCopyModeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridClipboardCopyMode>(
            nameof(ClipboardCopyMode),
            defaultValue: DataGridClipboardCopyMode.ExcludeHeader);

    public static readonly StyledProperty<bool> AutoGenerateColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(AutoGenerateColumns));

    public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
        AvaloniaProperty.Register<DataGrid, IEnumerable?>(nameof(ItemsSource));

    public static readonly StyledProperty<bool> IsRowDetailsFrozenProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsRowDetailsFrozen));

    public static readonly StyledProperty<IDataTemplate?> RowDetailsTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(RowDetailsTemplate));

    public static readonly StyledProperty<DataGridRowDetailsVisibilityMode> RowDetailsVisibilityModeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridRowDetailsVisibilityMode>(nameof(RowDetailsVisibilityMode),
            DataGridRowDetailsVisibilityMode.Collapsed);

    public static readonly DirectProperty<DataGrid, IDataGridCollectionView?> CollectionViewProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, IDataGridCollectionView?>(nameof(CollectionView),
            o => o.CollectionView);

    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(Title));

    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(TitleTemplate));

    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(Footer));

    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(FooterTemplate));
    
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(EmptyIndicator));
    
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    public static readonly StyledProperty<DataGridPaginationVisibility> PaginationVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, DataGridPaginationVisibility>(nameof(PaginationVisibility), DataGridPaginationVisibility.Bottom);
    
    public static readonly StyledProperty<PaginationAlign> TopPaginationAlignProperty =
        AvaloniaProperty.Register<DataGrid, PaginationAlign>(nameof(TopPaginationAlign), PaginationAlign.End);
    
    public static readonly StyledProperty<PaginationAlign> BottomPaginationAlignProperty =
        AvaloniaProperty.Register<DataGrid, PaginationAlign>(nameof(BottomPaginationAlign), PaginationAlign.End);
    
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<DataGrid, int>(nameof(PageSize));
    
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AbstractPagination.IsHideOnSinglePageProperty.AddOwner<DataGrid>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGrid>();

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }
    
    public string? OperatingMsg
    {
        get => GetValue(OperatingMsgProperty);
        set => SetValue(OperatingMsgProperty, value);
    }
    
    [DependsOn(nameof(CustomOperatingIndicatorTemplate))]
    public object? CustomOperatingIndicator
    {
        get => GetValue(CustomOperatingIndicatorProperty);
        set => SetValue(CustomOperatingIndicatorProperty, value);
    }
    
    public IDataTemplate? CustomOperatingIndicatorTemplate
    {
        get => GetValue(CustomOperatingIndicatorTemplateProperty);
        set => SetValue(CustomOperatingIndicatorTemplateProperty, value);
    }

    public bool IsShowFrameBorder
    {
        get => GetValue(IsShowFrameBorderProperty);
        set => SetValue(IsShowFrameBorderProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can change
    /// the column display order by dragging column headers with the mouse.
    /// </summary>
    public bool CanUserReorderColumns
    {
        get => GetValue(CanUserReorderColumnsProperty);
        set => SetValue(CanUserReorderColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can adjust column widths using the mouse.
    /// </summary>
    public bool CanUserResizeColumns
    {
        get => GetValue(CanUserResizeColumnsProperty);
        set => SetValue(CanUserResizeColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can sort columns by clicking the column header.
    /// </summary>
    public bool CanUserSortColumns
    {
        get => GetValue(CanUserSortColumnsProperty);
        set => SetValue(CanUserSortColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can filter columns by clicking the column filter indicator.
    /// </summary>
    public bool CanUserFilterColumns
    {
        get => GetValue(CanUserFilterColumnsProperty);
        set => SetValue(CanUserFilterColumnsProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the user can change
    /// the row order by dragging row reorder handle with the mouse.
    /// </summary>
    public bool CanUserReorderRows
    {
        get => GetValue(CanUserReorderRowsProperty);
        set => SetValue(CanUserReorderRowsProperty, value);
    }

    /// <summary>
    /// If header show next sorter direction tooltip
    /// </summary>
    public bool ShowSorterTooltip
    {
        get => GetValue(ShowSorterTooltipProperty);
        set => SetValue(ShowSorterTooltipProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the column headers row.
    /// </summary>
    public double ColumnHeaderHeight
    {
        get => GetValue(ColumnHeaderHeightProperty);
        set => SetValue(ColumnHeaderHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the standard width or automatic sizing mode of columns in the control.
    /// </summary>
    public DataGridLength ColumnWidth
    {
        get => GetValue(ColumnWidthProperty);
        set => SetValue(ColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the theme applied to all row groups.
    /// </summary>
    public ControlTheme RowGroupTheme
    {
        get => GetValue(RowGroupThemeProperty);
        set => SetValue(RowGroupThemeProperty, value);
    }

    /// <summary>
    /// Gets or sets the number of left edge columns that the user cannot scroll horizontally.
    /// </summary>
    public int LeftFrozenColumnCount
    {
        get => GetValue(LeftFrozenColumnCountProperty);
        set => SetValue(LeftFrozenColumnCountProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the number of right edge columns that the user cannot scroll horizontally.
    /// </summary>
    public int RightFrozenColumnCount
    {
        get => GetValue(RightFrozenColumnCountProperty);
        set => SetValue(RightFrozenColumnCountProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates which grid lines separating inner cells are shown.
    /// </summary>
    public DataGridGridLinesVisibility GridLinesVisibility
    {
        get => GetValue(GridLinesVisibilityProperty);
        set => SetValue(GridLinesVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates the visibility of row and column headers.
    /// </summary>
    public DataGridHeadersVisibility HeadersVisibility
    {
        get => GetValue(HeadersVisibilityProperty);
        set => SetValue(HeadersVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how the horizontal scroll bar is displayed.
    /// </summary>
    public ScrollBarVisibility HorizontalScrollBarVisibility
    {
        get => GetValue(HorizontalScrollBarVisibilityProperty);
        set => SetValue(HorizontalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can edit the values in the control.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the row group header sections
    /// remain fixed at the width of the display area or can scroll horizontally.
    /// </summary>
    public bool IsRowGroupHeadersFrozen
    {
        get => GetValue(IsRowGroupHeadersFrozenProperty);
        set => SetValue(IsRowGroupHeadersFrozenProperty, value);
    }

    /// <summary>
    /// Gets or sets whether scroll gestures should include inertia in their behavior and value.
    /// </summary>
    public bool IsScrollInertiaEnabled
    {
        get => GetValue(IsScrollInertiaEnabledProperty);
        set => SetValue(IsScrollInertiaEnabledProperty, value);
    }

    private bool _isValid = true;

    public bool IsValid
    {
        get => _isValid;
        internal set
        {
            SetAndRaise(IsValidProperty, ref _isValid, value);
            PseudoClasses.Set(StdPseudoClass.Invalid, !value);
        }
    }

    /// <summary>
    /// Gets or sets the maximum width of columns in the <see cref="T:DataGrid" /> .
    /// </summary>
    public double MaxColumnWidth
    {
        get => GetValue(MaxColumnWidthProperty);
        set => SetValue(MaxColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width of columns in the <see cref="T:DataGrid" />.
    /// </summary>
    public double MinColumnWidth
    {
        get => GetValue(MinColumnWidthProperty);
        set => SetValue(MinColumnWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the standard height of rows in the control.
    /// </summary>
    public double RowHeight
    {
        get => GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the row header column.
    /// </summary>
    public double RowHeaderWidth
    {
        get => GetValue(RowHeaderWidthProperty);
        set => SetValue(RowHeaderWidthProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that is used to display the content of the row header.
    /// </summary>
    public IDataTemplate? RowHeaderContentTemplate
    {
        get => GetValue(RowHeaderContentTemplateProperty);
        set => SetValue(RowHeaderContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how the vertical scroll bar is displayed.
    /// </summary>
    public ScrollBarVisibility VerticalScrollBarVisibility
    {
        get => GetValue(VerticalScrollBarVisibilityProperty);
        set => SetValue(VerticalScrollBarVisibilityProperty, value);
    }

    /// <summary>
    /// Gets or sets the selection behavior of the data grid.
    /// </summary>
    public DataGridSelectionMode SelectionMode
    {
        get => GetValue(SelectionModeProperty);
        set => SetValue(SelectionModeProperty, value);
    }
    
    /// <summary>
    /// Get or trigger the type of row selection
    /// </summary>
    public DataGridSelectTriggerType SelectTriggerType
    {
        get => GetValue(SelectTriggerTypeProperty);
        set => SetValue(SelectTriggerTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the index of the current selection.
    /// </summary>
    /// <returns>
    /// The index of the current selection, or -1 if the selection is empty.
    /// </returns>
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetAndRaise(SelectedIndexProperty, ref _selectedIndex, value);
    }

    private int _selectedIndex = -1;

    /// <summary>
    /// Gets or sets the data item corresponding to the selected row.
    /// </summary>
    public object? SelectedItem
    {
        get => _selectedItem;
        set => SetAndRaise(SelectedItemProperty, ref _selectedItem, value);
    }

    private object? _selectedItem;

    /// <summary>
    /// The property which determines how DataGrid content is copied to the Clipboard.
    /// </summary>
    public DataGridClipboardCopyMode ClipboardCopyMode
    {
        get => GetValue(ClipboardCopyModeProperty);
        set => SetValue(ClipboardCopyModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether columns are created
    /// automatically when the <see cref="P:DataGrid.ItemsSource" /> property is set.
    /// </summary>
    public bool AutoGenerateColumns
    {
        get => GetValue(AutoGenerateColumnsProperty);
        set => SetValue(AutoGenerateColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets a collection that is used to generate the content of the control.
    /// </summary>
    public IEnumerable? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the row details sections remain
    /// fixed at the width of the display area or can scroll horizontally.
    /// </summary>
    public bool IsRowDetailsFrozen
    {
        get => GetValue(IsRowDetailsFrozenProperty);
        set => SetValue(IsRowDetailsFrozenProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that is used to display the content of the details section of rows.
    /// </summary>
    public IDataTemplate? RowDetailsTemplate
    {
        get => GetValue(RowDetailsTemplateProperty);
        set => SetValue(RowDetailsTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates when the details sections of rows are displayed.
    /// </summary>
    public DataGridRowDetailsVisibilityMode RowDetailsVisibilityMode
    {
        get => GetValue(RowDetailsVisibilityModeProperty);
        set => SetValue(RowDetailsVisibilityModeProperty, value);
    }

    /// <summary>
    /// Gets current <see cref="IDataGridCollectionView"/>.
    /// </summary>
    public IDataGridCollectionView? CollectionView => DataConnection.CollectionView;

    [DependsOn(nameof(TitleTemplateProperty))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    [DependsOn(nameof(FooterTemplateProperty))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
    
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }

    public DataGridPaginationVisibility PaginationVisibility
    {
        get => GetValue(PaginationVisibilityProperty);
        set => SetValue(PaginationVisibilityProperty, value);
    }
    
    public PaginationAlign TopPaginationAlign
    {
        get => GetValue(TopPaginationAlignProperty);
        set => SetValue(TopPaginationAlignProperty, value);
    }
    
    public PaginationAlign BottomPaginationAlign
    {
        get => GetValue(BottomPaginationAlignProperty);
        set => SetValue(BottomPaginationAlignProperty, value);
    }

    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
    }
    
    public int PageSize
    {
        get => GetValue(PageSizeProperty);
        set => SetValue(PageSizeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the column that contains the current cell.
    /// </summary>
    public DataGridColumn? CurrentColumn
    {
        get
        {
            if (CurrentColumnIndex == -1)
            {
                return null;
            }

            Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
            return ColumnsItemsInternal[CurrentColumnIndex];
        }
        set
        {
            DataGridColumn? dataGridColumn = value;
            if (dataGridColumn == null)
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToNull("value", "CurrentColumn");
            }

            if (CurrentColumn != dataGridColumn)
            {
                if (dataGridColumn.OwningGrid != this)
                {
                    // Provided column does not belong to this DataGrid
                    throw DataGridError.DataGrid.ColumnNotInThisDataGrid();
                }

                if (!dataGridColumn.IsVisible)
                {
                    // CurrentColumn cannot be set to an invisible column
                    throw DataGridError.DataGrid.ColumnCannotBeCollapsed();
                }

                if (CurrentSlot == -1)
                {
                    // There is no current row so the current column cannot be set
                    throw DataGridError.DataGrid.NoCurrentRow();
                }

                bool beginEdit = _editingColumnIndex != -1;

                //exitEditingMode, keepFocus, raiseEvents
                if (!EndCellEdit(DataGridEditAction.Commit, true, ContainsFocus, true))
                {
                    // Edited value couldn't be committed or aborted
                    return;
                }

                UpdateSelectionAndCurrency(dataGridColumn.Index, CurrentSlot, DataGridSelectionAction.None,
                    false); //scrollIntoView
                Debug.Assert(_successfullyUpdatedSelection);

                if (beginEdit &&
                    _editingColumnIndex == -1 &&
                    CurrentSlot != -1 &&
                    CurrentColumnIndex != -1 &&
                    CurrentColumnIndex == dataGridColumn.Index &&
                    dataGridColumn.OwningGrid == this &&
                    !GetColumnEffectiveReadOnlyState(dataGridColumn))
                {
                    // Returning to editing mode since the grid was in that mode prior to the EndCellEdit call above.
                    BeginCellEdit(new RoutedEventArgs());
                }
            }
        }
    }

    /// <summary>
    /// Gets a collection that contains all the columns in the control.
    /// we use a backing field here because the field's type
    /// is a subclass of the property's
    /// </summary>
    public ObservableCollection<DataGridColumn> Columns => ColumnsInternal;

    public ObservableCollection<IDataGridColumnGroupItem> ColumnGroups => ColumnGroupsInternal;

    /// <summary>
    /// Gets a list that contains the data items corresponding to the selected rows.
    /// </summary>
    public IList SelectedItems => _selectedItems;

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 属性验证方法

    private static bool IsValidColumnHeaderHeight(double value)
    {
        return double.IsNaN(value) ||
               (value >= MinimumColumnHeaderHeight && value <= MaxHeadersThickness);
    }

    private static bool ValidateFrozenColumnCount(int value) => value >= 0;

    private static bool IsValidColumnWidth(double value)
    {
        return !double.IsNaN(value) && value > 0;
    }

    private static bool IsValidMinColumnWidth(double value)
    {
        return !double.IsNaN(value) && !double.IsPositiveInfinity(value) && value >= 0;
    }

    private static bool IsValidRowHeight(double value)
    {
        return double.IsNaN(value) ||
               (value >= DataGridRow.MinimumHeight &&
                value <= DataGridRow.MaximumHeight);
    }

    private static bool IsValidRowHeaderWidth(double value)
    {
        return double.IsNaN(value) ||
               (value >= MinimumRowHeaderWidth &&
                value <= MaxHeadersThickness);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<SelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<DataGrid, SelectionChangedEventArgs>(nameof(SelectionChanged), RoutingStrategies.Bubble);

    /// <summary>
    /// Occurs one time for each public, non-static property in the bound data type when the
    /// <see cref="P:DataGrid.ItemsSource" /> property is changed and the
    /// <see cref="P:DataGrid.AutoGenerateColumns" /> property is true.
    /// </summary>
    public event EventHandler<DataGridAutoGeneratingColumnEventArgs>? AutoGeneratingColumn;

    /// <summary>
    /// Occurs before a cell or row enters editing mode.
    /// </summary>
    public event EventHandler<DataGridBeginningEditEventArgs>? BeginningEdit;

    /// <summary>
    /// Occurs after cell editing has ended.
    /// </summary>
    public event EventHandler<DataGridCellEditEndedEventArgs>? CellEditEnded;

    /// <summary>
    /// Occurs immediately before cell editing has ended.
    /// </summary>
    public event EventHandler<DataGridCellEditEndingEventArgs>? CellEditEnding;

    /// <summary>
    /// Occurs when cell is mouse-pressed.
    /// </summary>
    public event EventHandler<DataGridCellPointerPressedEventArgs>? CellPointerPressed;

    /// <summary>
    /// Occurs when the <see cref="P:DataGridColumn.DisplayIndex" />
    /// property of a column changes.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? ColumnDisplayIndexChanged;

    /// <summary>
    /// Raised when column reordering ends, to allow subscribers to clean up.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? ColumnReordered;

    /// <summary>
    /// Raised when starting a column reordering action.  Subscribers to this event can
    /// set tooltip and caret UIElements, constrain tooltip position, indicate that
    /// a preview should be shown, or cancel reordering.
    /// </summary>
    public event EventHandler<DataGridColumnReorderingEventArgs>? ColumnReordering;
    
    /// <summary>
    /// This event is dispatched when the drag indicator is dragged to a specific column.
    /// </summary>
    public event EventHandler<DataGridColumnDraggingOverEventArgs>? ColumnDraggingOver;
    
    /// <summary>
    /// Raised when row reordering ends, to allow subscribers to clean up.
    /// </summary>
    public event EventHandler<DataGridRowEventArgs>? RowReordered;
    
    /// <summary>
    /// Raised when starting a row reordering action.  Subscribers to this event can
    /// set tooltip and caret UIElements, constrain tooltip position, indicate that
    /// a preview should be shown, or cancel reordering.
    /// </summary>
    public event EventHandler<DataGridRowReorderingEventArgs>? RowReordering;

    /// <summary>
    /// Occurs when a different cell becomes the current cell.
    /// </summary>
    public event EventHandler<EventArgs>? CurrentCellChanged;

    /// <summary>
    /// Occurs after a <see cref="T:DataGridError.DataGridRow" />
    /// is instantiated, so that you can customize it before it is used.
    /// </summary>
    public event EventHandler<DataGridRowEventArgs>? LoadingRow;

    /// <summary>
    /// Occurs when a cell in a <see cref="T:DataGridError.DataGridTemplateColumn" /> enters editing mode.
    ///
    /// </summary>
    public event EventHandler<DataGridPreparingCellForEditEventArgs>? PreparingCellForEdit;

    /// <summary>
    /// Occurs when the row has been successfully committed or cancelled.
    /// </summary>
    public event EventHandler<DataGridRowEditEndedEventArgs>? RowEditEnded;

    /// <summary>
    /// Occurs immediately before the row has been successfully committed or cancelled.
    /// </summary>
    public event EventHandler<DataGridRowEditEndingEventArgs>? RowEditEnding;

    /// <summary>
    /// Occurs when the <see cref="P:DataGrid.SelectedItem" /> or
    /// <see cref="P:DataGrid.SelectedItems" /> property value changes.
    /// </summary>
    public event EventHandler<SelectionChangedEventArgs>? SelectionChanged
    {
        add => AddHandler(SelectionChangedEvent, value);
        remove => RemoveHandler(SelectionChangedEvent, value);
    }

    /// <summary>
    /// Occurs when the <see cref="DataGridColumn"/> sorting request is triggered.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? Sorting;

    /// <summary>
    /// Occurs when the <see cref="DataGridColumn"/> filtering request is triggered.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? Filtering;

    /// <summary>
    /// Occurs when a <see cref="T:DataGridError.DataGridRow" />
    /// object becomes available for reuse.
    /// </summary>
    public event EventHandler<DataGridRowEventArgs>? UnloadingRow;

    /// <summary>
    /// Occurs when a new row details template is applied to a row, so that you can customize
    /// the details section before it is used.
    /// </summary>
    public event EventHandler<DataGridRowDetailsEventArgs>? LoadingRowDetails;

    /// <summary>
    /// Occurs when the <see cref="P:DataGrid.RowDetailsVisibilityMode" />
    /// property value changes.
    /// </summary>
    public event EventHandler<DataGridRowDetailsEventArgs>? RowDetailsVisibilityChanged;

    /// <summary>
    /// Occurs when a row details element becomes available for reuse.
    /// </summary>
    public event EventHandler<DataGridRowDetailsEventArgs>? UnloadingRowDetails;

    public event EventHandler<ScrollEventArgs>? HorizontalScroll;
    public event EventHandler<ScrollEventArgs>? VerticalScroll;

    /// <summary>
    /// Occurs before a DataGridRowGroupHeader header is used.
    /// </summary>
    public event EventHandler<DataGridRowGroupHeaderEventArgs>? LoadingRowGroup;

    /// <summary>
    /// Occurs when the DataGridRowGroupHeader is available for reuse.
    /// </summary>
    public event EventHandler<DataGridRowGroupHeaderEventArgs>? UnloadingRowGroup;

    /// <summary>
    /// This event is raised by OnCopyingRowClipboardContent method after the default row content is prepared.
    /// Event listeners can modify or add to the row clipboard content.
    /// </summary>
    public event EventHandler<DataGridRowClipboardEventArgs>? CopyingRowClipboardContent;

    #endregion

    /// <summary>
    /// Gets the data item bound to the row that contains the current cell.
    /// </summary>
    protected object? CurrentItem
    {
        get
        {
            if (CurrentSlot == -1 || ItemsSource == null || RowGroupHeadersTable.Contains(CurrentSlot))
            {
                return null;
            }

            return DataConnection.GetDataItem(RowIndexFromSlot(CurrentSlot));
        }
    }

    static DataGrid()
    {
        AffectsMeasure<DataGrid>(
            ColumnHeaderHeightProperty,
            HorizontalScrollBarVisibilityProperty,
            VerticalScrollBarVisibilityProperty);

        SizeTypeProperty.OverrideDefaultValue<DataGrid>(SizeType.Large);

        ItemsSourceProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleItemsSourcePropertyChanged(e));
        CanUserResizeColumnsProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandleCanUserResizeColumnsChanged(e));
        ColumnWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleColumnWidthChanged(e));
        LeftFrozenColumnCountProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleFrozenColumnCountChanged(e));
        GridLinesVisibilityProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleGridLinesVisibilityChanged(e));
        HeadersVisibilityProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleHeadersVisibilityChanged(e));
        IsReadOnlyProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleIsReadOnlyChanged(e));
        MaxColumnWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleMaxColumnWidthChanged(e));
        MinColumnWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleMinColumnWidthChanged(e));
        RowHeightProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleRowHeightChanged(e));
        RowHeaderWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleRowHeaderWidthChanged(e));
        SelectionModeProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleSelectionModeChanged(e));
        SelectedIndexProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleSelectedIndexChanged(e));
        SelectedItemProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleSelectedItemChanged(e));
        IsEnabledProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleIsEnabledChanged(e));
        IsRowGroupHeadersFrozenProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandleIsRowGroupHeadersFrozenChanged(e));
        RowDetailsTemplateProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleRowDetailsTemplateChanged(e));
        RowHeaderContentTemplateProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleRowHeaderContentTemplateChanged(e));
        RowDetailsVisibilityModeProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandleRowDetailsVisibilityModeChanged(e));
        AutoGenerateColumnsProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleAutoGenerateColumnsChanged(e));

        FocusableProperty.OverrideDefaultValue<DataGrid>(true);
    }

    public DataGrid()
    {
        this.RegisterResources();
        KeyDown += HandleKeyDown;
        KeyUp   += HandleKeyUp;

        //TODO: Check if override works
        GotFocus  += HandleGotFocus;
        LostFocus += HandleLostFocus;

        CurrentCellCoordinates   = new DataGridCellCoordinates(-1, -1);
        _loadedRows              = new List<DataGridRow>();
        _lostFocusActions        = new Queue<Action>();
        _selectedItems           = new DataGridSelectedItemsCollection(this);
        RowGroupHeadersTable     = new IndexToValueTable<DataGridRowGroupInfo>();
        _bindingValidationErrors = new List<Exception>();

        DisplayData                            =  new DataGridDisplayData(this);
        ColumnGroupsInternal                   =  new ObservableCollection<IDataGridColumnGroupItem>();
        ColumnsInternal                        =  CreateColumnsInstance();
        ColumnsInternal.CollectionChanged      += HandleColumnsInternalCollectionChanged;
        ColumnGroupsInternal.CollectionChanged += HandleGroupColumnsInternalCollectionChanged;
        RowHeightEstimate                      =  DefaultRowHeight;
        RowDetailsHeightEstimate               =  0;
        _rowHeaderDesiredWidth                 =  0;

        DataConnection       = new DataGridDataConnection(this);
        _showDetailsTable    = new IndexToValueTable<bool>();
        _collapsedSlotsTable = new IndexToValueTable<bool>();

        AnchorSlot          = -1;
        _lastEstimatedRow   = -1;
        _editingColumnIndex = -1;
        _mouseOverRowIndex  = null;

        RowGroupHeaderHeightEstimate = DefaultRowHeight;
        RowGroupSublevelIndents      = [];
        _rowGroupHeightsByLevel      = [];
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Enters editing mode for the current cell and current row (if they're not already in editing mode).
    /// </summary>
    /// <param name="editingEventArgs">Provides information about the user gesture that caused the call to BeginEdit. Can be null.</param>
    /// <returns>True if operation was successful. False otherwise.</returns>
    public bool BeginEdit(RoutedEventArgs editingEventArgs)
    {
        if (CurrentColumnIndex == -1 || !GetRowSelection(CurrentSlot))
        {
            return false;
        }

        Debug.Assert(CurrentColumnIndex >= 0);
        Debug.Assert(CurrentColumnIndex < ColumnsItemsInternal.Count);
        Debug.Assert(CurrentSlot >= -1);
        Debug.Assert(CurrentSlot < SlotCount);
        Debug.Assert(EditingRow == null || EditingRow.Slot == CurrentSlot);

        if (GetColumnEffectiveReadOnlyState(CurrentColumn))
        {
            // Current column is read-only
            return false;
        }

        return BeginCellEdit(editingEventArgs);
    }

    /// <summary>
    /// Cancels editing mode for the specified DataGridEditingUnit and restores its original value.
    /// </summary>
    /// <param name="editingUnit">Specifies whether to cancel edit for a Cell or Row.</param>
    /// <returns>True if operation was successful. False otherwise.</returns>
    public bool CancelEdit(DataGridEditingUnit editingUnit = DataGridEditingUnit.Row)
    {
        return CancelEdit(editingUnit, raiseEvents: true);
    }

    /// <summary>
    /// Commits editing mode and pushes changes to the backend.
    /// </summary>
    /// <returns>True if operation was successful. False otherwise.</returns>
    public bool CommitEdit()
    {
        return CommitEdit(DataGridEditingUnit.Row, true);
    }

    /// <summary>
    /// Commits editing mode for the specified DataGridEditingUnit and pushes changes to the backend.
    /// </summary>
    /// <param name="editingUnit">Specifies whether to commit edit for a Cell or Row.</param>
    /// <param name="exitEditingMode">Editing mode is left if True.</param>
    /// <returns>True if operation was successful. False otherwise.</returns>
    public bool CommitEdit(DataGridEditingUnit editingUnit, bool exitEditingMode)
    {
        if (!EndCellEdit(
                editAction: DataGridEditAction.Commit,
                exitEditingMode: editingUnit == DataGridEditingUnit.Cell ? exitEditingMode : true,
                keepFocus: ContainsFocus,
                raiseEvents: true))
        {
            return false;
        }

        if (editingUnit == DataGridEditingUnit.Row)
        {
            return EndRowEdit(DataGridEditAction.Commit, exitEditingMode, raiseEvents: true);
        }

        return true;
    }

    /// <summary>
    /// Scrolls the specified item or RowGroupHeader and/or column into view.
    /// If item is not null: scrolls the row representing the item into view;
    /// If column is not null: scrolls the column into view;
    /// If both item and column are null, the method returns without scrolling.
    /// </summary>
    /// <param name="item">an item from the DataGrid's items source or a CollectionViewGroup from the collection view</param>
    /// <param name="column">a column from the DataGrid's columns collection</param>
    public void ScrollIntoView(object? item, DataGridColumn? column)
    {
        if ((column == null && (item == null || FirstDisplayedNonFillerColumnIndex == -1))
            || (column != null && column.OwningGrid != this))
        {
            // no-op
            return;
        }

        if (item == null && column != null)
        {
            // scroll column into view
            ScrollSlotIntoView(
                column.Index,
                DisplayData.FirstScrollingSlot,
                forCurrentCellChange: false,
                forceHorizontalScroll: true);
        }
        else
        {
            int                   slot         = -1;
            DataGridRowGroupInfo? rowGroupInfo = null;
            if (item is DataGridCollectionViewGroup collectionViewGroup)
            {
                rowGroupInfo = RowGroupInfoFromCollectionViewGroup(collectionViewGroup);
                if (rowGroupInfo == null)
                {
                    Debug.Assert(false);
                    return;
                }

                slot = rowGroupInfo.Slot;
            }
            else
            {
                // the row index will be set to -1 if the item is null or not in the list
                int rowIndex = DataConnection.IndexOf(item);
                if (rowIndex == -1)
                {
                    return;
                }

                slot = SlotFromRowIndex(rowIndex);
            }

            int columnIndex = (column == null) ? FirstDisplayedNonFillerColumnIndex : column.Index;

            if (_collapsedSlotsTable.Contains(slot))
            {
                // We need to expand all parent RowGroups so that the slot is visible
                if (rowGroupInfo != null)
                {
                    ExpandRowGroupParentChain(rowGroupInfo.Level - 1, rowGroupInfo.Slot);
                }
                else
                {
                    rowGroupInfo = RowGroupHeadersTable.GetValueAt(RowGroupHeadersTable.GetPreviousIndex(slot));
                    Debug.Assert(rowGroupInfo != null);
                    ExpandRowGroupParentChain(rowGroupInfo.Level, rowGroupInfo.Slot);
                }

                // Update Scrollbar and display information
                NegVerticalOffset = 0;
                SetVerticalOffset(0);
                ResetDisplayedRows();
                DisplayData.FirstScrollingSlot = 0;
                ComputeScrollBarsLayout();
            }

            ScrollSlotIntoView(
                columnIndex, slot,
                forCurrentCellChange: true,
                forceHorizontalScroll: true);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (DataConnection.DataSource != null && !DataConnection.EventsWired)
        {
            DataConnection.WireEvents(DataConnection.DataSource);
            InitializeElements(true /*recycleRows*/);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        // When wired to INotifyCollectionChanged, the DataGrid will be cleaned up by GC
        if (DataConnection.DataSource != null && DataConnection.EventsWired)
        {
            DataConnection.UnWireEvents(DataConnection.DataSource);
        }
    }

    /// <summary>
    /// Arranges the content of the <see cref="T:DataGridError.DataGridRow" />.
    /// </summary>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    /// <returns>
    /// The actual size used by the <see cref="T:DataGridError.DataGridRow" />.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (_makeFirstDisplayedCellCurrentCellPending)
        {
            MakeFirstDisplayedCellCurrentCell();
        }

        if (!MathUtils.AreClose(Bounds.Width, finalSize.Width))
        {
            // If our final width has changed, we might need to update the filler
            InvalidateColumnHeadersArrange();
            InvalidateCellsArrange();
        }

        return base.ArrangeOverride(finalSize);
    }

    /// <summary>
    /// Measures the children of a <see cref="T:DataGridError.DataGridRow" /> to prepare for
    /// arranging them during the
    /// <see cref="M:DataGridError.DataGridRow.ArrangeOverride(System.Windows.Size)" /> pass.
    /// </summary>
    /// <returns>
    /// The size that the <see cref="T:DataGridError.DataGridRow" /> determines it needs during layout, based on its calculations of child object allocated sizes.
    /// </returns>
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that
    /// child elements should not exceed.
    /// </param>
    protected override Size MeasureOverride(Size availableSize)
    {
        // Delay layout until after the initial measure to avoid invalid calculations when the
        // DataGrid is not part of the visual tree
        if (!_measured)
        {
            _measured = true;

            // We don't need to clear the rows because it was already done when the ItemsSource changed
            RefreshRowsAndColumns(clearRows: false);

            //// Update our estimates now that the DataGrid has all of the information necessary
            UpdateRowDetailsHeightEstimate();

            // Update frozen columns to account for columns added prior to loading or autogenerated columns
            if (FrozenColumnCountWithFiller > 0)
            {
                ProcessFrozenColumnCount();
            }
        }

        Size desiredSize;
        // This is a shortcut to skip layout if we don't have any columns
        if (ColumnsInternal.VisibleEdgedColumnsWidth == 0)
        {
            if (_hScrollBar != null && _hScrollBar.IsVisible)
            {
                _hScrollBar.IsVisible = false;
            }

            if (_vScrollBar != null && _vScrollBar.IsVisible)
            {
                _vScrollBar.IsVisible = false;
            }

            desiredSize = base.MeasureOverride(availableSize);
        }
        else
        {
            if (_rowsPresenter != null)
            {
                _rowsPresenter.InvalidateMeasure();
            }

            InvalidateColumnHeadersMeasure();

            desiredSize = base.MeasureOverride(availableSize);

            ComputeScrollBarsLayout();
        }

        return desiredSize;
    }

    /// <inheritdoc/>
    protected override void OnDataContextBeginUpdate()
    {
        base.OnDataContextBeginUpdate();

        NotifyDataContextPropertyForAllRowCells(GetAllRows(), true);
    }

    /// <inheritdoc/>
    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();

        NotifyDataContextPropertyForAllRowCells(GetAllRows(), false);
    }

    /// <summary>
    /// Raises the BeginningEdit event.
    /// </summary>
    protected virtual void NotifyBeginningEdit(DataGridBeginningEditEventArgs e)
    {
        BeginningEdit?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the CellEditEnded event.
    /// </summary>
    protected virtual void NotifyCellEditEnded(DataGridCellEditEndedEventArgs e)
    {
        CellEditEnded?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the CellEditEnding event.
    /// </summary>
    protected virtual void NotifyCellEditEnding(DataGridCellEditEndingEventArgs e)
    {
        CellEditEnding?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the CellPointerPressed event.
    /// </summary>
    internal virtual void NotifyCellPointerPressed(DataGridCellPointerPressedEventArgs e)
    {
        CellPointerPressed?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the CurrentCellChanged event.
    /// </summary>
    protected virtual void NotifyCurrentCellChanged(EventArgs e)
    {
        CurrentCellChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the LoadingRow event for row preparation.
    /// </summary>
    protected virtual void NotifyLoadingRow(DataGridRowEventArgs e)
    {
        if (LoadingRow != null)
        {
            Debug.Assert(!_loadedRows.Contains(e.Row));
            _loadedRows.Add(e.Row);
            LoadingOrUnloadingRow = true;
            LoadingRow(this, e);
            LoadingOrUnloadingRow = false;
            Debug.Assert(_loadedRows.Contains(e.Row));
            _loadedRows.Remove(e.Row);
        }
    }

    /// <summary>
    /// Scrolls the DataGrid according to the direction of the delta.
    /// </summary>
    /// <param name="e">PointerWheelEventArgs</param>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var delta = e.Delta;

        // KeyModifiers.Shift should scroll in horizontal direction. This does not work on every platform. 
        // If Shift-Key is pressed and X is close to 0 we swap the Vector.
        if (e.KeyModifiers == KeyModifiers.Shift && MathUtilities.IsZero(delta.X))
        {
            delta = new Vector(delta.Y, delta.X);
        }

        if (UpdateScroll(delta * MouseWheelDelta))
        {
            e.Handled = true;
        }
        else
        {
            e.Handled = e.Handled || !ScrollViewer.GetIsScrollChainingEnabled(this);
        }
    }

    /// <summary>
    /// Raises the PreparingCellForEdit event.
    /// </summary>
    protected virtual void NotifyPreparingCellForEdit(DataGridPreparingCellForEditEventArgs e)
    {
        PreparingCellForEdit?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the RowEditEnded event.
    /// </summary>
    protected virtual void NotifyRowEditEnded(DataGridRowEditEndedEventArgs e)
    {
        RowEditEnded?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the RowEditEnding event.
    /// </summary>
    protected virtual void NotifyRowEditEnding(DataGridRowEditEndingEventArgs e)
    {
        RowEditEnding?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the SelectionChanged event and clears the _selectionChanged.
    /// This event won't get raised again until after _selectionChanged is set back to true.
    /// </summary>
    protected virtual void NotifySelectionChanged(SelectionChangedEventArgs e)
    {
        RaiseEvent(e);
    }

    /// <summary>
    /// Raises the UnloadingRow event for row recycling.
    /// </summary>
    protected virtual void NotifyUnloadingRow(DataGridRowEventArgs e)
    {
        EventHandler<DataGridRowEventArgs>? handler = UnloadingRow;
        if (handler != null)
        {
            LoadingOrUnloadingRow = true;
            handler(this, e);
            LoadingOrUnloadingRow = false;
        }
    }

    /// <summary>
    /// Comparator class so we can sort list by the display index
    /// </summary>
    public class DisplayIndexComparer : IComparer<DataGridColumn>
    {
        int IComparer<DataGridColumn>.Compare(DataGridColumn? x, DataGridColumn? y)
        {
            if (x == null && y == null)
            {
                return 0;
            }

            if (x == null && y != null)
            {
                return -1;
            }

            if (x != null && y == null)
            {
                return 1;
            }

            Debug.Assert(x != null && y != null);
            return (x.DisplayIndexWithFiller < y.DisplayIndexWithFiller) ? -1 : 1;
        }
    }

    /// <summary>
    /// Builds the visual tree for the column header when a new template is applied.
    /// </summary>
    //TODO Validation UI
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        // The template has changed, so we need to refresh the visuals
        _measured = false;

        if (_columnHeadersPresenter != null)
        {
            // If we're applying a new template, we want to remove the old column headers first
            _columnHeadersPresenter.Children.Clear();
        }

        _columnHeadersPresenter =
            e.NameScope.Find<DataGridColumnHeadersPresenter>(DataGridThemeConstants.ColumnHeadersPresenterPart);
        _groupColumnHeadersPresenter = e.NameScope.Find<DataGridGroupColumnHeadersPresenter>(DataGridThemeConstants.GroupColumnHeadersPresenterPart);
        _dataGridDraggingOverIndicator = e.NameScope.Find<DataGridColumnDraggingOverIndicator>(DataGridThemeConstants.DraggingOverIndicatorPart);
        if (_groupColumnHeadersPresenter != null)
        {
            _groupColumnHeadersPresenter.OwningGrid = this;
        }

        CheckFrozenColumnCount();

        if (ColumnGroups.Count > 0)
        {
            if (_groupColumnHeadersPresenter != null)
            {
                if (ColumnsInternal.FillerColumn != null)
                {
                    ColumnsInternal.FillerColumn.IsRepresented = false;
                }

                BuildColumnGroupView();
                SetupColumnGroupFrozenState();
            }
        }
        else
        {
            if (_columnHeadersPresenter != null)
            {
                if (ColumnsInternal.FillerColumn != null)
                {
                    ColumnsInternal.FillerColumn.IsRepresented = false;
                }

                _columnHeadersPresenter.OwningGrid = this;

                // Columns were added before our Template was applied, add the ColumnHeaders now
                var sortedInternal = new List<DataGridColumn>(ColumnsItemsInternal);
                sortedInternal.Sort(new DisplayIndexComparer());

                foreach (DataGridColumn column in sortedInternal)
                {
                    InsertDisplayedColumnHeader(column);
                }
            }
        }

        if (_rowsPresenter != null)
        {
            // If we're applying a new template, we want to remove the old rows first
            UnloadElements(recycle: false);
        }

        _rowsPresenter = e.NameScope.Find<DataGridRowsPresenter>(DataGridThemeConstants.RowsPresenterPart);

        if (_rowsPresenter != null)
        {
            _rowsPresenter.OwningGrid = this;
            InvalidateRowHeightEstimate();
            UpdateRowDetailsHeightEstimate();
        }

        if (_hScrollBar != null)
        {
            _hScrollBar.Scroll -= HandleHorizontalScrollBarScroll;
        }

        _hScrollBar = e.NameScope.Find<ScrollBar>(DataGridThemeConstants.HorizontalScrollbarPart);

        if (_hScrollBar != null)
        {
            _hScrollBar.IsTabStop   =  false;
            _hScrollBar.Maximum     =  0.0;
            _hScrollBar.Orientation =  Orientation.Horizontal;
            _hScrollBar.IsVisible   =  false;
            _hScrollBar.Scroll      += HandleHorizontalScrollBarScroll;
        }

        if (_vScrollBar != null)
        {
            _vScrollBar.Scroll -= HandleVerticalScrollBarScroll;
        }

        _vScrollBar = e.NameScope.Find<ScrollBar>(DataGridThemeConstants.VerticalScrollbarPart);

        if (_vScrollBar != null)
        {
            _vScrollBar.IsTabStop   =  false;
            _vScrollBar.Maximum     =  0.0;
            _vScrollBar.Orientation =  Orientation.Vertical;
            _vScrollBar.IsVisible   =  false;
            _vScrollBar.Scroll      += HandleVerticalScrollBarScroll;
        }

        _topLeftCornerHeader = e.NameScope.Find<ContentControl>(DataGridThemeConstants.TopLeftCornerPart);
        EnsureTopLeftCornerHeader(); // EnsureTopLeftCornerHeader checks for a null _topLeftCornerHeader;
        _topRightCornerHeader = e.NameScope.Find<ContentControl>(DataGridThemeConstants.TopRightCornerPart);
        _bottomRightCorner    = e.NameScope.Find<Visual>(DataGridThemeConstants.BottomRightCornerPart);

        if (IsShowFrameBorder)
        {
            ConfigureFrameBorderThickness();
        }

        ConfigureHeaderCornerRadius();
        ConfigurePaginationVisibility();
        SetValue(EmptyIndicatorProperty, new Empty()
        {
            SizeType    = SizeType.Middle,
            PresetImage = PresetEmptyImage.Simple
        }, BindingPriority.Template);

        _topPagination = e.NameScope.Find<Pagination>(DataGridThemeConstants.TopPaginationPart);
        _bottomPagination = e.NameScope.Find<Pagination>(DataGridThemeConstants.BottomPaginationPart);

        if (_topPagination != null)
        {
            _topPagination.CurrentPageChanged += HandlePageChangeRequest;
        }
        
        if (_bottomPagination != null)
        {
            _bottomPagination.CurrentPageChanged += HandlePageChangeRequest;
        }
        
        _templatedApplied = true;
    }

    /// <summary>
    /// Raises the LoadingRowDetails for row details preparation
    /// </summary>
    protected virtual void NotifyLoadingRowDetails(DataGridRowDetailsEventArgs e)
    {
        EventHandler<DataGridRowDetailsEventArgs>? handler = LoadingRowDetails;
        if (handler != null)
        {
            LoadingOrUnloadingRow = true;
            handler(this, e);
            LoadingOrUnloadingRow = false;
        }
    }

    /// <summary>
    /// Raises the UnloadingRowDetails event
    /// </summary>
    protected virtual void NotifyUnloadingRowDetails(DataGridRowDetailsEventArgs e)
    {
        EventHandler<DataGridRowDetailsEventArgs>? handler = UnloadingRowDetails;
        if (handler != null)
        {
            LoadingOrUnloadingRow = true;
            handler(this, e);
            LoadingOrUnloadingRow = false;
        }
    }

    public void SelectAll()
    {
        SetRowsSelection(0, SlotCount - 1);
    }

    /// <summary>
    /// Returns the Group at the indicated level or null if the item is not in the ItemsSource
    /// </summary>
    /// <param name="item">item</param>
    /// <param name="groupLevel">groupLevel</param>
    /// <returns>The group the given item falls under or null if the item is not in the ItemsSource</returns>
    public DataGridCollectionViewGroup? GetGroupFromItem(object item, int groupLevel)
    {
        int itemIndex = DataConnection.IndexOf(item);
        if (itemIndex == -1)
        {
            return null;
        }

        int                   groupHeaderSlot = RowGroupHeadersTable.GetPreviousIndex(SlotFromRowIndex(itemIndex));
        DataGridRowGroupInfo? rowGroupInfo    = RowGroupHeadersTable.GetValueAt(groupHeaderSlot);
        while (rowGroupInfo != null && rowGroupInfo.Level != groupLevel)
        {
            groupHeaderSlot = RowGroupHeadersTable.GetPreviousIndex(rowGroupInfo.Slot);
            rowGroupInfo    = RowGroupHeadersTable.GetValueAt(groupHeaderSlot);
        }

        return rowGroupInfo?.CollectionViewGroup;
    }

    /// <summary>
    /// Raises the LoadingRowGroup event
    /// </summary>
    /// <param name="e">EventArgs</param>
    protected virtual void NotifyLoadingRowGroup(DataGridRowGroupHeaderEventArgs e)
    {
        EventHandler<DataGridRowGroupHeaderEventArgs>? handler = LoadingRowGroup;
        if (handler != null)
        {
            LoadingOrUnloadingRow = true;
            handler(this, e);
            LoadingOrUnloadingRow = false;
        }
    }

    /// <summary>
    /// Raises the UnLoadingRowGroup event
    /// </summary>
    /// <param name="e">EventArgs</param>
    protected virtual void OnUnloadingRowGroup(DataGridRowGroupHeaderEventArgs e)
    {
        EventHandler<DataGridRowGroupHeaderEventArgs>? handler = UnloadingRowGroup;
        if (handler != null)
        {
            LoadingOrUnloadingRow = true;
            handler(this, e);
            LoadingOrUnloadingRow = false;
        }
    }

    /// <summary>
    /// This method raises the CopyingRowClipboardContent event.
    /// </summary>
    /// <param name="e">Contains the necessary information for generating the row clipboard content.</param>
    protected virtual void OnCopyingRowClipboardContent(DataGridRowClipboardEventArgs e)
    {
        CopyingRowClipboardContent?.Invoke(this, e);
    }

    /// <summary>
    /// Raises the AutoGeneratingColumn event.
    /// </summary>
    protected virtual void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
    {
        AutoGeneratingColumn?.Invoke(this, e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == BorderThicknessProperty ||
            change.Property == GridLinesVisibilityProperty ||
            change.Property == IsShowFrameBorderProperty ||
            change.Property == FooterProperty)
        {
            ConfigureFrameBorderThickness();
        }

        if (change.Property == TitleProperty ||
            change.Property == HeadersVisibilityProperty)
        {
            ConfigureHeaderCornerRadius();
        }

        if (change.Property == PageSizeProperty ||
            change.Property == PaginationVisibilityProperty)
        {
            ConfigurePaginationVisibility();
        }

        if (_templatedApplied)
        {
            if (change.Property == IsGroupHeaderModeProperty)
            {
           
                if (IsGroupHeaderMode)
                {
                    SetupColumnGroupFrozenState();
                }
            }
            else if (change.Property == LeftFrozenColumnCountProperty ||
                     change.Property == RightFrozenColumnCountProperty)
            {
                CheckFrozenColumnCount();
            }
            else if (change.Property == PageSizeProperty)
            {
                ReConfigurePagination();
            }
        }
    }
}
