// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections.ObjectModel;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
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

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A data table with editable, sortable, filterable and groupable columns, row details, selection and paging.
/// A row source that supports range loading is read by range instead of as a whole.
/// </summary>
[PseudoClasses(StdPseudoClass.Invalid, DataGridPseudoClass.EmptyRows, DataGridPseudoClass.EmptyColumns)]
public partial class DataGrid : TemplatedControl,
                                ICustomizableSizeTypeAware,
                                IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGrid>();
    
    /// <summary>
    /// Identifies the <see cref="IsOperating" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsOperatingProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsOperating));
    
    /// <summary>
    /// Identifies the <see cref="OperatingMsg" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> OperatingMsgProperty =
        AvaloniaProperty.Register<DataGrid, string?>(nameof(OperatingMsg));
    
    /// <summary>
    /// Identifies the <see cref="CustomOperatingIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CustomOperatingIndicatorProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(CustomOperatingIndicator));

    /// <summary>
    /// Identifies the <see cref="CustomOperatingIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> CustomOperatingIndicatorTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(CustomOperatingIndicatorTemplate));

    /// <summary>
    /// Identifies the <see cref="IsFrameBorderVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFrameBorderVisibleProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsFrameBorderVisible), false);

    /// <summary>
    /// Identifies the <see cref="CanUserReorderColumns" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanUserReorderColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserReorderColumns));
    
    /// <summary>
    /// Identifies the <see cref="CanUserResizeColumns" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanUserResizeColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserResizeColumns));
    
    /// <summary>
    /// Identifies the <see cref="CanUserSortColumns" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanUserSortColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserSortColumns), false);
    
    /// <summary>
    /// Identifies the <see cref="CanUserFilterColumns" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanUserFilterColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserFilterColumns), false);
    
    /// <summary>
    /// Identifies the <see cref="CanUserReorderRows" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> CanUserReorderRowsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(CanUserReorderRows), false);
    
    /// <summary>
    /// Identifies the <see cref="IsSorterTooltipVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSorterTooltipVisibleProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsSorterTooltipVisible), true);
    
    /// <summary>
    /// Identifies the <see cref="ColumnHeaderHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> ColumnHeaderHeightProperty = 
        AvaloniaProperty.Register<DataGrid, double>(nameof(ColumnHeaderHeight), defaultValue: double.NaN, validate: IsValidColumnHeaderHeight);
    
    /// <summary>
    /// Identifies the <see cref="ColumnWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridLength> ColumnWidthProperty =
        AvaloniaProperty.Register<DataGrid, DataGridLength>(nameof(ColumnWidth), defaultValue: DataGridLength.Auto);
    
    /// <summary>
    /// Identifies the <see cref="RowGroupTheme" /> property.
    /// </summary>
    public static readonly StyledProperty<ControlTheme> RowGroupThemeProperty =
        AvaloniaProperty.Register<DataGrid, ControlTheme>(nameof(RowGroupTheme));

    /// <summary>
    /// Identifies the <see cref="LeftFrozenColumnCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> LeftFrozenColumnCountProperty =
        AvaloniaProperty.Register<DataGrid, int>(
            nameof(LeftFrozenColumnCount),
            validate: ValidateFrozenColumnCount);
    
    /// <summary>
    /// Identifies the <see cref="RightFrozenColumnCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> RightFrozenColumnCountProperty =
        AvaloniaProperty.Register<DataGrid, int>(
            nameof(RightFrozenColumnCount),
            validate: ValidateFrozenColumnCount);

    /// <summary>
    /// Identifies the <see cref="GridLinesVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridGridLinesVisibility> GridLinesVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, DataGridGridLinesVisibility>(nameof(GridLinesVisibility),
            DataGridGridLinesVisibility.Horizontal);

    /// <summary>
    /// Identifies the <see cref="HeadersVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridHeadersVisibility> HeadersVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, DataGridHeadersVisibility>(nameof(HeadersVisibility),
            DataGridHeadersVisibility.Column);

    /// <summary>
    /// Identifies the <see cref="HorizontalScrollBarVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, ScrollBarVisibility>(nameof(HorizontalScrollBarVisibility));

    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsReadOnly));

    /// <summary>
    /// Identifies the <see cref="IsRowGroupHeadersFrozen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRowGroupHeadersFrozenProperty =
        AvaloniaProperty.Register<DataGrid, bool>(
            nameof(IsRowGroupHeadersFrozen),
            defaultValue: true);

    /// <summary>
    /// Identifies the <see cref="IsValid" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, bool>(
            nameof(IsValid),
            o => o.IsValid);

    /// <summary>
    /// Identifies the <see cref="MaxColumnWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MaxColumnWidthProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(MaxColumnWidth),
            defaultValue: DefaultMaxColumnWidth,
            validate: IsValidColumnWidth);

    /// <summary>
    /// Identifies the <see cref="MinColumnWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MinColumnWidthProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(MinColumnWidth),
            defaultValue: DefaultMinColumnWidth,
            validate: IsValidMinColumnWidth);

    /// <summary>
    /// Identifies the <see cref="IsScrollInertiaEnabled" /> property.
    /// </summary>
    public static readonly AttachedProperty<bool> IsScrollInertiaEnabledProperty =
        ScrollViewer.IsScrollInertiaEnabledProperty.AddOwner<DataGrid>();

    /// <summary>
    /// Identifies the <see cref="RowHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> RowHeightProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(RowHeight),
            defaultValue: double.NaN,
            validate: IsValidRowHeight);

    /// <summary>
    /// Identifies the <see cref="RowHeaderWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> RowHeaderWidthProperty =
        AvaloniaProperty.Register<DataGrid, double>(
            nameof(RowHeaderWidth),
            defaultValue: double.NaN,
            validate: IsValidRowHeaderWidth);
    
    /// <summary>
    /// Identifies the <see cref="RowHeaderContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RowHeaderContentTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(RowHeaderContentTemplate));

    /// <summary>
    /// Identifies the <see cref="SelectionMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridSelectionMode> SelectionModeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridSelectionMode>(nameof(SelectionMode), DataGridSelectionMode.None);
    
    /// <summary>
    /// Identifies the <see cref="SelectTriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridSelectTriggerType> SelectTriggerTypeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridSelectTriggerType>(nameof(SelectTriggerType), DataGridSelectTriggerType.SelectIndicator);

    /// <summary>
    /// Identifies the <see cref="VerticalScrollBarVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, ScrollBarVisibility>(nameof(VerticalScrollBarVisibility));

    /// <summary>
    /// Identifies the <see cref="ClipboardCopyMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridClipboardCopyMode> ClipboardCopyModeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridClipboardCopyMode>(
            nameof(ClipboardCopyMode),
            defaultValue: DataGridClipboardCopyMode.ExcludeHeader);

    /// <summary>
    /// Identifies the <see cref="AutoGenerateColumns" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> AutoGenerateColumnsProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(AutoGenerateColumns));

    /// <summary>
    /// Identifies the <see cref="ItemsSource" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, IDataGridSource?> ItemsSourceProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, IDataGridSource?>(
            nameof(ItemsSource),
            control => control.ItemsSource,
            (control, value) => control.ItemsSource = value,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="Query" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridQuery> QueryProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridQuery>(
            nameof(Query),
            control => control.Query,
            (control, value) => control.Query = value,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="GroupExpansion" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridGroupExpansion> GroupExpansionProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridGroupExpansion>(
            nameof(GroupExpansion),
            control => control.GroupExpansion,
            (control, value) => control.GroupExpansion = value,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="PageRequest" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridPageRequest?> PageRequestProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridPageRequest?>(
            nameof(PageRequest),
            control => control.PageRequest,
            (control, value) => control.PageRequest = value,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="Selection" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridSelectionState> SelectionProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridSelectionState>(
            nameof(Selection),
            control => control.Selection,
            (control, value) => control.Selection = value,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="CurrentRowKey" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridRowKey?> CurrentRowKeyProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridRowKey?>(
            nameof(CurrentRowKey),
            control => control.CurrentRowKey,
            (control, value) => control.CurrentRowKey = value,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="AppliedQuery" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridQuery> AppliedQueryProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridQuery>(
            nameof(AppliedQuery),
            control => control.AppliedQuery);

    /// <summary>
    /// Identifies the <see cref="AppliedPageRequest" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridPageRequest?> AppliedPageRequestProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridPageRequest?>(
            nameof(AppliedPageRequest),
            control => control.AppliedPageRequest);

    /// <summary>
    /// Identifies the <see cref="LoadState" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, DataGridLoadState> LoadStateProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, DataGridLoadState>(
            nameof(LoadState),
            control => control.LoadState);

    /// <summary>
    /// Identifies the <see cref="LoadError" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, Exception?> LoadErrorProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, Exception?>(
            nameof(LoadError),
            control => control.LoadError);

    /// <summary>
    /// Identifies the <see cref="TotalItemCount" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, long> TotalItemCountProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, long>(
            nameof(TotalItemCount),
            control => control.TotalItemCount);

    /// <summary>
    /// Identifies the <see cref="TotalEntryCount" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, int> TotalEntryCountProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, int>(
            nameof(TotalEntryCount),
            control => control.TotalEntryCount);

    /// <summary>
    /// Identifies the <see cref="IsDataStale" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGrid, bool> IsDataStaleProperty =
        AvaloniaProperty.RegisterDirect<DataGrid, bool>(
            nameof(IsDataStale),
            control => control.IsDataStale);

    /// <summary>
    /// Identifies the <see cref="IsRowDetailsFrozen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsRowDetailsFrozenProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsRowDetailsFrozen));

    /// <summary>
    /// Identifies the <see cref="RowDetailsTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RowDetailsTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(RowDetailsTemplate));

    /// <summary>
    /// Identifies the <see cref="RowDetailsVisibilityMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridRowDetailsVisibilityMode> RowDetailsVisibilityModeProperty =
        AvaloniaProperty.Register<DataGrid, DataGridRowDetailsVisibilityMode>(nameof(RowDetailsVisibilityMode),
            DataGridRowDetailsVisibilityMode.Collapsed);

    /// <summary>
    /// Identifies the <see cref="Title" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TitleProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(Title));

    /// <summary>
    /// Identifies the <see cref="TitleTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TitleTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(TitleTemplate));

    /// <summary>
    /// Identifies the <see cref="Footer" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> FooterProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(Footer));

    /// <summary>
    /// Identifies the <see cref="FooterTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(FooterTemplate));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> EmptyIndicatorProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(EmptyIndicator));
    
    /// <summary>
    /// Identifies the <see cref="EmptyIndicatorTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> EmptyIndicatorTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(EmptyIndicatorTemplate));
    
    /// <summary>
    /// Identifies the <see cref="PaginationVisibility" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridPaginationVisibility> PaginationVisibilityProperty =
        AvaloniaProperty.Register<DataGrid, DataGridPaginationVisibility>(nameof(PaginationVisibility), DataGridPaginationVisibility.Bottom);
    
    /// <summary>
    /// Identifies the <see cref="TopPaginationAlign" /> property.
    /// </summary>
    public static readonly StyledProperty<PaginationAlign> TopPaginationAlignProperty =
        AvaloniaProperty.Register<DataGrid, PaginationAlign>(nameof(TopPaginationAlign), PaginationAlign.End);
    
    /// <summary>
    /// Identifies the <see cref="BottomPaginationAlign" /> property.
    /// </summary>
    public static readonly StyledProperty<PaginationAlign> BottomPaginationAlignProperty =
        AvaloniaProperty.Register<DataGrid, PaginationAlign>(nameof(BottomPaginationAlign), PaginationAlign.End);
    
    /// <summary>
    /// Identifies the <see cref="PageSize" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PageSizeProperty =
        AvaloniaProperty.Register<DataGrid, int>(nameof(PageSize));

    /// <summary>
    /// Identifies the <see cref="IsShowPageSizeSelector" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShowPageSizeSelectorProperty =
        AvaloniaProperty.Register<DataGrid, bool>(nameof(IsShowPageSizeSelector), true);

    /// <summary>
    /// Identifies the <see cref="TopPaginationExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> TopPaginationExtraContentProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(TopPaginationExtraContent));

    /// <summary>
    /// Identifies the <see cref="TopPaginationExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> TopPaginationExtraContentTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(TopPaginationExtraContentTemplate));

    /// <summary>
    /// Identifies the <see cref="BottomPaginationExtraContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> BottomPaginationExtraContentProperty =
        AvaloniaProperty.Register<DataGrid, object?>(nameof(BottomPaginationExtraContent));

    /// <summary>
    /// Identifies the <see cref="BottomPaginationExtraContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> BottomPaginationExtraContentTemplateProperty =
        AvaloniaProperty.Register<DataGrid, IDataTemplate?>(nameof(BottomPaginationExtraContentTemplate));

    /// <summary>
    /// Identifies the <see cref="IsHideOnSinglePage" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsHideOnSinglePageProperty =
        AbstractPagination.IsHideOnSinglePageProperty.AddOwner<DataGrid>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGrid>();

    /// <summary>
    /// Density preset of the grid parts. The pagers, rows, row headers and column group headers take their sizes
    /// from it.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Shows the loading indicator over the grid content while <see langword="true" />. The indicator also shows
    /// while <see cref="LoadState" /> is <see cref="DataGridLoadState.Loading" />, whatever this property is.
    /// </summary>
    public bool IsOperating
    {
        get => GetValue(IsOperatingProperty);
        set => SetValue(IsOperatingProperty, value);
    }
    
    /// <summary>
    /// Text shown with the loading indicator. The theme's default is the <c>Operating</c> language resource.
    /// </summary>
    public string? OperatingMsg
    {
        get => GetValue(OperatingMsgProperty);
        set => SetValue(OperatingMsgProperty, value);
    }
    
    /// <summary>
    /// Content that replaces the built-in loading indicator. <see langword="null" /> keeps the built-in one.
    /// </summary>
    [DependsOn(nameof(CustomOperatingIndicatorTemplate))]
    public object? CustomOperatingIndicator
    {
        get => GetValue(CustomOperatingIndicatorProperty);
        set => SetValue(CustomOperatingIndicatorProperty, value);
    }
    
    /// <summary>
    /// Template that builds <see cref="CustomOperatingIndicator" />. Changing it re-evaluates that content.
    /// </summary>
    public IDataTemplate? CustomOperatingIndicatorTemplate
    {
        get => GetValue(CustomOperatingIndicatorTemplateProperty);
        set => SetValue(CustomOperatingIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// Draws the border of the outer frame with <see cref="TemplatedControl.BorderThickness" /> and rounds all
    /// four corners with <see cref="TemplatedControl.CornerRadius" />. When <see langword="false" />, the frame
    /// border has no thickness, only the top corners are rounded, the separator under the title is hidden and
    /// the last row keeps its bottom grid line. The default is <see langword="false" />.
    /// </summary>
    public bool IsFrameBorderVisible
    {
        get => GetValue(IsFrameBorderVisibleProperty);
        set => SetValue(IsFrameBorderVisibleProperty, value);
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
    public bool IsSorterTooltipVisible
    {
        get => GetValue(IsSorterTooltipVisibleProperty);
        set => SetValue(IsSorterTooltipVisibleProperty, value);
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

    /// <summary>
    /// Whether the grid content is valid. <see langword="false" /> adds the <c>invalid</c> pseudo-class.
    /// The setter is internal.
    /// </summary>
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
    /// The property which determines how DataGrid content is copied to the Clipboard.
    /// </summary>
    public DataGridClipboardCopyMode ClipboardCopyMode
    {
        get => GetValue(ClipboardCopyModeProperty);
        set => SetValue(ClipboardCopyModeProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates whether columns are created
    /// automatically from the source schema.
    /// </summary>
    public bool AutoGenerateColumns
    {
        get => GetValue(AutoGenerateColumnsProperty);
        set => SetValue(AutoGenerateColumnsProperty, value);
    }

    private IDataGridSource? _source;

    /// <summary>
    /// Gets or sets the range-based data source that supplies the grid.
    /// </summary>
    /// <remarks>
    /// The data source is externally owned. The grid subscribes to its invalidation signal while attached,
    /// but does not dispose it when this property changes or when the grid is detached.
    /// </remarks>
    public IDataGridSource? ItemsSource
    {
        get => _source;
        set => SetItemsSource(value);
    }

    private DataGridQuery _query = DataGridQuery.Empty;

    /// <summary>
    /// The filters, sorts and groups that the grid applies to its data. Assigning a new value commits the active
    /// edit (the assignment is dropped when the commit fails), expands all groups when the groups changed, moves
    /// <see cref="PageRequest" /> back to the first page, raises <see cref="QueryChanged" /> and reloads the rows.
    /// </summary>
    public DataGridQuery Query
    {
        get => _query;
        set => SetQuery(value, DataGridQueryChangeReason.External);
    }

    private DataGridGroupExpansion _groupExpansion = DataGridGroupExpansion.AllExpanded;

    /// <summary>
    /// Which groups of <see cref="Query" /> are expanded; all of them by default. Assigning a value commits the
    /// active edit (the assignment is dropped when the commit fails) and reloads the rows.
    /// <see cref="ExpandGroup" /> and <see cref="CollapseGroup" /> change a single group.
    /// </summary>
    public DataGridGroupExpansion GroupExpansion
    {
        get => _groupExpansion;
        set => SetGroupExpansion(value);
    }

    private DataGridPageRequest? _pageRequest;

    /// <summary>
    /// The data window that the grid asks the source for, given by its start index and its row count.
    /// <see langword="null" /> leaves the window to the grid, which derives it from the current page and
    /// <see cref="PageSize" />. Assigning a value commits the active edit (the assignment is dropped when the
    /// commit fails), copies the row count into <see cref="PageSize" /> (0 for <see langword="null" />) and
    /// reloads the rows.
    /// </summary>
    public DataGridPageRequest? PageRequest
    {
        get => _pageRequest;
        set => SetPageRequest(value);
    }

    private DataGridSelectionState _selection = DataGridSelectionState.Empty;

    /// <summary>
    /// The row selection: explicit keys, index intervals and all rows matching a query, minus the excluded keys.
    /// The default value selects nothing. An assigned value is normalized for <see cref="SelectionMode" /> before
    /// it is stored, and <see cref="SelectionChanged" /> is raised after the new state has been stored.
    /// </summary>
    public DataGridSelectionState Selection
    {
        get => _selection;
        set => SetSelectionState(value);
    }

    private DataGridRowKey? _currentRowKey;

    /// <summary>
    /// The key of the current row, or <see langword="null" /> when the grid has no current row. A key whose
    /// <c>IsValid</c> is <see langword="false" /> throws <see cref="ArgumentException" />. With range presentation
    /// active, a key that is in the committed data moves the current cell to its row and commits the active edit;
    /// any other value clears the current cell.
    /// </summary>
    public DataGridRowKey? CurrentRowKey
    {
        get => _currentRowKey;
        set => SetCurrentRowKey(value);
    }

    private DataGridQuery _appliedQuery = DataGridQuery.Empty;

    /// <summary>
    /// The <see cref="Query" /> that the presented rows were loaded for. It follows <see cref="Query" /> only when
    /// a load succeeds or falls back to the previous snapshot, and it is empty while no snapshot is applied.
    /// </summary>
    public DataGridQuery AppliedQuery => _appliedQuery;

    private DataGridPageRequest? _appliedPageRequest;

    /// <summary>
    /// The <see cref="PageRequest" /> that the presented rows were loaded for, or <see langword="null" /> while
    /// no snapshot is applied. It is updated together with <see cref="AppliedQuery" />.
    /// </summary>
    public DataGridPageRequest? AppliedPageRequest => _appliedPageRequest;

    private DataGridLoadState _dataLoadState;

    /// <summary>
    /// The state of the latest data load. It is <see cref="DataGridLoadState.Ready" /> after a load succeeds and
    /// <see cref="DataGridLoadState.Error" /> after one fails; <see cref="DataGridLoadState.Loading" /> also shows
    /// the loading indicator.
    /// </summary>
    public DataGridLoadState LoadState => _dataLoadState;

    private Exception? _loadError;

    /// <summary>
    /// The error of the latest failed load, set together with <see cref="LoadState" />
    /// <see cref="DataGridLoadState.Error" />. It is cleared when the next load starts and after a load succeeds.
    /// </summary>
    public Exception? LoadError => _loadError;

    private long _totalItemCount;

    /// <summary>
    /// The total number of data rows that the source reports for <see cref="AppliedQuery" />, over all pages.
    /// It is updated together with <see cref="AppliedQuery" />.
    /// </summary>
    public long TotalItemCount => _totalItemCount;

    private int _totalEntryCount;

    /// <summary>
    /// The number of presentation entries in the applied snapshot: the rows plus the group headers. It is updated
    /// together with <see cref="AppliedQuery" />.
    /// </summary>
    public int TotalEntryCount => _totalEntryCount;

    private bool _isDataStale;

    /// <summary>
    /// <see langword="true" /> while the presented rows are known to be out of date: after a mutation or an
    /// invalidation requested a reload, until that reload succeeds.
    /// </summary>
    public bool IsDataStale => _isDataStale;

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
    /// Content of the title area above the table. The area exists only while this is not <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(TitleTemplateProperty))]
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="Title" />. Changing it re-evaluates <see cref="Title" />.
    /// </summary>
    public IDataTemplate? TitleTemplate
    {
        get => GetValue(TitleTemplateProperty);
        set => SetValue(TitleTemplateProperty, value);
    }

    /// <summary>
    /// Content of the footer area below the table and above the bottom pager. The area exists only while this is
    /// not <see langword="null" />.
    /// </summary>
    [DependsOn(nameof(FooterTemplateProperty))]
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="Footer" />. Changing it re-evaluates <see cref="Footer" />.
    /// </summary>
    public IDataTemplate? FooterTemplate
    {
        get => GetValue(FooterTemplateProperty);
        set => SetValue(FooterTemplateProperty, value);
    }
    
    /// <summary>
    /// Content shown in place of the rows while the data source is empty. The default is an <see cref="Empty" />
    /// control of middle size with the simple preset image.
    /// </summary>
    [DependsOn(nameof(EmptyIndicatorTemplate))]
    public object? EmptyIndicator
    {
        get => GetValue(EmptyIndicatorProperty);
        set => SetValue(EmptyIndicatorProperty, value);
    }

    /// <summary>
    /// Template that builds <see cref="EmptyIndicator" />. Changing it re-evaluates <see cref="EmptyIndicator" />.
    /// </summary>
    public IDataTemplate? EmptyIndicatorTemplate
    {
        get => GetValue(EmptyIndicatorTemplateProperty);
        set => SetValue(EmptyIndicatorTemplateProperty, value);
    }

    /// <summary>
    /// The edges of the grid that show a pager; the bottom edge by default. No pager is shown while the page size
    /// is 0, while there is no data or while <see cref="IsHideOnSinglePage" /> applies.
    /// </summary>
    public DataGridPaginationVisibility PaginationVisibility
    {
        get => GetValue(PaginationVisibilityProperty);
        set => SetValue(PaginationVisibilityProperty, value);
    }
    
    /// <summary>
    /// Alignment of the pager at the top edge of the grid; <c>End</c> by default. The extra content of that pager
    /// is placed on the opposite side: on the left at <c>End</c>, otherwise on the right.
    /// </summary>
    public PaginationAlign TopPaginationAlign
    {
        get => GetValue(TopPaginationAlignProperty);
        set => SetValue(TopPaginationAlignProperty, value);
    }
    
    /// <summary>
    /// Alignment of the pager at the bottom edge of the grid; <c>End</c> by default. The extra content of that
    /// pager is placed on the opposite side: on the left at <c>End</c>, otherwise on the right.
    /// </summary>
    public PaginationAlign BottomPaginationAlign
    {
        get => GetValue(BottomPaginationAlignProperty);
        set => SetValue(BottomPaginationAlignProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the pagination area includes a page-size selector.
    /// </summary>
    public bool IsShowPageSizeSelector
    {
        get => GetValue(IsShowPageSizeSelectorProperty);
        set => SetValue(IsShowPageSizeSelectorProperty, value);
    }

    /// <summary>
    /// Gets or sets the extra content shown on the opposite side of the top pagination.
    /// </summary>
    public object? TopPaginationExtraContent
    {
        get => GetValue(TopPaginationExtraContentProperty);
        set => SetValue(TopPaginationExtraContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to render <see cref="TopPaginationExtraContent" />.
    /// </summary>
    public IDataTemplate? TopPaginationExtraContentTemplate
    {
        get => GetValue(TopPaginationExtraContentTemplateProperty);
        set => SetValue(TopPaginationExtraContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the extra content shown on the opposite side of the bottom pagination.
    /// </summary>
    public object? BottomPaginationExtraContent
    {
        get => GetValue(BottomPaginationExtraContentProperty);
        set => SetValue(BottomPaginationExtraContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template used to render <see cref="BottomPaginationExtraContent" />.
    /// </summary>
    public IDataTemplate? BottomPaginationExtraContentTemplate
    {
        get => GetValue(BottomPaginationExtraContentTemplateProperty);
        set => SetValue(BottomPaginationExtraContentTemplateProperty, value);
    }

    /// <summary>
    /// Hides the pager while all rows fit on one page, that is while <see cref="TotalItemCount" /> is not greater
    /// than the page size.
    /// </summary>
    public bool IsHideOnSinglePage
    {
        get => GetValue(IsHideOnSinglePageProperty);
        set => SetValue(IsHideOnSinglePageProperty, value);
    }
    
    /// <summary>
    /// The number of rows per page; 0 by default. 0 hides the pager and disables paging. Assigning
    /// <see cref="PageRequest" /> overwrites this value with the row count of the request.
    /// </summary>
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

    /// <summary>
    /// The root nodes of the column group tree, shown in the group header row above the column headers while the
    /// collection is not empty. A node is either a group that nests columns and other groups, or a column.
    /// </summary>
    public ObservableCollection<IDataGridColumnGroupItem> ColumnGroups => ColumnGroupsInternal;

    /// <summary>
    /// Turns the animations of the grid and its parts on or off. The template passes the value to both pagers,
    /// and the theme's default is the <c>EnableMotion</c> design token.
    /// </summary>
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

    /// <summary>
    /// Occurs one time for each public, non-static property in the bound data type when the
    /// source schema is changed and the
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
    /// Occurs when the row has been successfully committed or canceled.
    /// </summary>
    public event EventHandler<DataGridRowEditEndedEventArgs>? RowEditEnded;

    /// <summary>
    /// Occurs immediately before the row has been successfully committed or canceled.
    /// </summary>
    public event EventHandler<DataGridRowEditEndingEventArgs>? RowEditEnding;

    /// <summary>
    /// Occurs when an inline edit has left edit mode but the data source rejected or could not receive it.
    /// </summary>
    public event EventHandler<DataGridEditCommitFailedEventArgs>? EditCommitFailed;

    /// <summary>
    /// Occurs when the immutable declarative selection expression changes.
    /// </summary>
    public event EventHandler<DataGridSelectionChangedEventArgs>? SelectionChanged;

    /// <summary>
    /// Occurs when the <see cref="DataGridColumn" /> sorting request is triggered.
    /// </summary>
    public event EventHandler<DataGridColumnEventArgs>? Sorting;

    /// <summary>
    /// Occurs when the <see cref="DataGridColumn" /> filtering request is triggered.
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

    /// <summary>
    /// Raised after the grid has processed a scroll of its horizontal scroll bar. The sender is the scroll bar.
    /// </summary>
    public event EventHandler<ScrollEventArgs>? HorizontalScroll;
    /// <summary>
    /// Raised after the grid has processed a scroll of its vertical scroll bar. The sender is the scroll bar.
    /// </summary>
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
    
    /// <summary>
    /// Raised when a page index change completed
    /// </summary>
    public event EventHandler<PageChangedEventArgs>? PageChanged;

    /// <summary>
    /// Raised when a page index change is requested
    /// </summary>
    public event EventHandler<PageChangingEventArgs>? PageChanging;

    /// <summary>
    /// Raised after <see cref="Query" /> changed and the change was projected onto the columns, before the rows
    /// are reloaded. Also raised with the reason <see cref="DataGridQueryChangeReason.LoadRollback" /> when a
    /// failed load restores the previous query. A handler that assigns <see cref="Query" /> again replaces the
    /// pending reload with its own.
    /// </summary>
    public event EventHandler<DataGridQueryChangedEventArgs>? QueryChanged;

    #endregion

    /// <summary>
    /// Gets the data item bound to the row that contains the current cell.
    /// </summary>
    protected object? CurrentItem
    {
        get
        {
            return TryGetCommittedRangeEntry(CurrentSlot, out var entry) &&
                   entry.Kind == DataGridSourceEntryKind.Data
                ? entry.Item
                : null;
        }
    }

    static DataGrid()
    {
        AffectsMeasure<DataGrid>(
            ColumnHeaderHeightProperty,
            HorizontalScrollBarVisibilityProperty,
            VerticalScrollBarVisibilityProperty);

        SizeTypeProperty.OverrideDefaultValue<DataGrid>(CustomizableSizeType.Large);

        CanUserReorderRowsProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandleCanUserReorderRowsChanged(e));
        CanUserResizeColumnsProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandleCanUserResizeColumnsChanged(e));
        CanUserFilterColumnsProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandleCanUserFilterColumnsChanged(e));
        IsPopupPinnedOpenProperty.Changed.AddClassHandler<DataGrid>((x, e) =>
            x.HandlePopupPinnedOpenChanged(e));
        ColumnWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleColumnWidthChanged(e));
        LeftFrozenColumnCountProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleFrozenColumnCountChanged(e));
        RightFrozenColumnCountProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleFrozenColumnCountChanged(e));
        GridLinesVisibilityProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleGridLinesVisibilityChanged(e));
        HeadersVisibilityProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleHeadersVisibilityChanged(e));
        IsReadOnlyProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleIsReadOnlyChanged(e));
        MaxColumnWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleMaxColumnWidthChanged(e));
        MinColumnWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleMinColumnWidthChanged(e));
        RowHeightProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleRowHeightChanged(e));
        RowHeaderWidthProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleRowHeaderWidthChanged(e));
        SelectionModeProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleSelectionModeChanged(e));
        IsEnabledProperty.Changed.AddClassHandler<DataGrid>((x, e) => x.HandleIsEnabledChanged(e));
        IsOperatingProperty.Changed.AddClassHandler<DataGrid>((x, _) => x.UpdateEffectiveIsOperating());
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
        CurrentCellCoordinates   = new DataGridCellCoordinates(-1, -1);
        _loadedRows              = new List<DataGridRow>();
        _lostFocusActions        = new Queue<Action>();

        DisplayData                            =  new DataGridDisplayData(this);
        ColumnGroupsInternal                   =  new ObservableCollection<IDataGridColumnGroupItem>();
        ColumnsInternal                        =  CreateColumnsInstance();
        ColumnsInternal.CollectionChanged      += HandleColumnsInternalCollectionChanged;
        ColumnGroupsInternal.CollectionChanged += HandleGroupColumnsInternalCollectionChanged;
        RowHeightEstimate                      =  DefaultRowHeight;
        RowDetailsHeightEstimate               =  0;
        _rowHeaderDesiredWidth                 =  0;

        RangeDataAccess       = new DataGridRangeDataAccess(this);
        _rowDetailsHeightEstimateTable = new IndexToValueTable<double>();

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
    /// <param name="editingEventArgs">
    /// Provides information about the user gesture that caused the call to BeginEdit. Can be null.
    /// </param>
    /// <returns>
    /// True if operation was successful. False otherwise.
    /// </returns>
    public bool BeginEdit(RoutedEventArgs editingEventArgs)
    {
        // Editing follows the current cell; row selection is an independent feature in this grid.
        if (CurrentColumnIndex == -1 || CurrentSlot == -1)
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
    /// <param name="editingUnit">
    /// Specifies whether to cancel edit for a Cell or Row.
    /// </param>
    /// <returns>
    /// True if operation was successful. False otherwise.
    /// </returns>
    public bool CancelEdit(DataGridEditingUnit editingUnit = DataGridEditingUnit.Row)
    {
        return CancelEdit(editingUnit, raiseEvents: true);
    }

    /// <summary>
    /// Commits editing mode and pushes changes to the backend.
    /// </summary>
    /// <returns>
    /// True if operation was successful. False otherwise.
    /// </returns>
    public bool CommitEdit()
    {
        return CommitEdit(DataGridEditingUnit.Row, true);
    }

    /// <summary>
    /// Commits editing mode for the specified DataGridEditingUnit and pushes changes to the backend.
    /// </summary>
    /// <param name="editingUnit">
    /// Specifies whether to commit edit for a Cell or Row.
    /// </param>
    /// <param name="exitEditingMode">
    /// Editing mode is left if True.
    /// </param>
    /// <returns>
    /// True if operation was successful. False otherwise.
    /// </returns>
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
    /// <param name="item">
    /// A currently cached source item.
    /// </param>
    /// <param name="column">
    /// a column from the DataGrid's columns collection
    /// </param>
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
            var slot = item is null ? -1 : FindCommittedRangeSlot(item);
            if (slot < 0)
            {
                return;
            }

            int columnIndex = (column == null) ? FirstDisplayedNonFillerColumnIndex : column.Index;
            ScrollSlotIntoView(
                columnIndex, slot,
                forCurrentCellChange: true,
                forceHorizontalScroll: true);
        }
    }

    /// <summary>
    /// Called when the grid is attached to a visual tree. Besides the base behavior, it sets up the token owner
    /// of the content, attaches the range data source, subscribes to the page change events of both pagers and
    /// refreshes the pinned-open filter target. An override must call the base member, which this implementation
    /// does first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureContentTokenOwner();
        AttachRangeSource();

        if (_topPagination != null)
        {
            _topPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _topPagination.CurrentPageChanged += HandlePageChangeRequest;
        }
        if (_bottomPagination != null)
        {
            _bottomPagination.CurrentPageChanged -= HandlePageChangeRequest;
            _bottomPagination.CurrentPageChanged += HandlePageChangeRequest;
        }

        RefreshPopupPinnedOpenFilterTarget();
    }

    /// <summary>
    /// Called when the grid is detached from a visual tree. It releases the token owner binding of the content,
    /// detaches the range data source, cancels a running row reorder and suspends the pinned-open filter target,
    /// then calls the base member and unsubscribes from the page change events of both pagers. An override must
    /// call the base member.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _contentTokenOwnerBinding?.Dispose();
        _contentTokenOwnerBinding = null;
        DetachRangeSource();
        CancelRowReorder();
        SuspendPopupPinnedOpenFilterTarget();
        base.OnDetachedFromVisualTree(e);
        if (_topPagination != null)
        {
            _topPagination.CurrentPageChanged -= HandlePageChangeRequest;
        }
        if (_bottomPagination != null)
        {
            _bottomPagination.CurrentPageChanged -= HandlePageChangeRequest;
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
    /// <param name="availableSize">
    /// The available size that this element can give to child elements. Indicates an upper limit that
    /// child elements should not exceed.
    /// </param>
    /// <returns>
    /// The size that the <see cref="T:DataGridError.DataGridRow" /> determines it needs during layout, based on its
    /// calculations of child object allocated sizes.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        // Delay layout until after the initial measure to avoid invalid calculations when the
        // DataGrid is not part of the visual tree
        if (!_measured)
        {
            _measured = true;

            // Source attachment owns initial row materialization.
            if (IsRangePresentationActive)
            {
                if (AutoGenerateColumns)
                {
                    AutoGenerateColumnsPrivate();
                }
            }
            else
            {
                RefreshRowsAndColumns(clearRows: false);
            }

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

    /// <inheritdoc />
    protected override void OnDataContextBeginUpdate()
    {
        base.OnDataContextBeginUpdate();

        NotifyDataContextPropertyForAllRowCells(GetAllRows(), true);
    }

    /// <inheritdoc />
    protected override void OnDataContextEndUpdate()
    {
        base.OnDataContextEndUpdate();

        NotifyDataContextPropertyForAllRowCells(GetAllRows(), false);
        UpdateColumnDataContext();
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
    /// <param name="e">
    /// PointerWheelEventArgs
    /// </param>
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        var delta = e.Delta;

        // KeyModifiers.Shift should scroll in horizontal direction. This does not work on every platform. 
        // If Shift-Key is pressed and X is close to 0 we swap the Vector.
        if (e.KeyModifiers == KeyModifiers.Shift && MathUtils.IsZero(delta.X))
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
    /// Raises the EditCommitFailed event.
    /// </summary>
    protected virtual void NotifyEditCommitFailed(DataGridEditCommitFailedEventArgs e)
    {
        EditCommitFailed?.Invoke(this, e);
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

    private Control? _contentTokenOwnerHost;
    private IDisposable? _contentTokenOwnerBinding;

    private void ConfigureContentTokenOwner()
    {
        _contentTokenOwnerBinding?.Dispose();
        // Spin owns its mask and indicator; the content remains the DataGrid's implementation subtree.
        // ControlTheme cannot bind ThemeScope.TokenOwner because the attached property is internal to Core
        // (AXAML reports AVLN2000). Reuse the scoped forwarding helper and release on detach/re-template.
        _contentTokenOwnerBinding = _contentTokenOwnerHost is null
            ? null
            : TokenOwnerForwarding.Forward(this, _contentTokenOwnerHost);
    }

    /// <summary>
    /// Builds the visual tree for the column header when a new template is applied.
    /// </summary>
    //TODO Validation UI
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        CancelRowReorder();
        SuspendPopupPinnedOpenFilterTarget();

        _contentTokenOwnerHost = e.NameScope.Find<Control>("GridContent");
        ConfigureContentTokenOwner();

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

                // Columns were added before our Template was applied, add the ColumnHeaders now.
                // DisplayIndexMap already stores the same display order that the old temp-list sort rebuilt.
                int displayedColumnCount = ColumnsInternal.GetDisplayedColumnCount();
                for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
                {
                    DataGridColumn column = ColumnsInternal.GetDisplayedColumnAtDisplayIndex(displayIndex);
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

        if (IsFrameBorderVisible)
        {
            ConfigureFrameBorderThickness();
        }

        ConfigureFrameCornerRadius();
        ConfigureHeaderCornerRadius();
        ConfigurePaginationVisibility();
        SetValue(EmptyIndicatorProperty, new Empty()
        {
            SizeType    = AtomUI.SizeType.Middle,
            PresetImage = PresetEmptyImage.Simple
        }, BindingPriority.Template);

        if (_topPagination != null)
        {
            _topPagination.CurrentPageChanged -= HandlePageChangeRequest;
        }
        if (_bottomPagination != null)
        {
            _bottomPagination.CurrentPageChanged -= HandlePageChangeRequest;
        }

        _topPagination    = e.NameScope.Find<Pagination>(DataGridThemeConstants.TopPaginationPart);
        _bottomPagination = e.NameScope.Find<Pagination>(DataGridThemeConstants.BottomPaginationPart);

        SyncRangePaginationState();

        if (_topPagination != null)
        {
            _topPagination.CurrentPageChanged += HandlePageChangeRequest;
        }
        
        if (_bottomPagination != null)
        {
            _bottomPagination.CurrentPageChanged += HandlePageChangeRequest;
        }
        
        _templatedApplied = true;
        RefreshPopupPinnedOpenFilterTarget();
        RestoreRangePresentationAfterTemplate();
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

    /// <summary>
    /// Selects all rows of the committed data by adding an all-matching scope to <see cref="Selection" />; keys
    /// that are already selected stay selected. Does nothing unless <see cref="SelectionMode" /> is
    /// <see cref="DataGridSelectionMode.Extended" />, <see cref="TotalItemCount" /> is greater than zero and a
    /// committed snapshot exists.
    /// </summary>
    public void SelectAll()
    {
        var scope = GetCommittedSelectionScope();
        if (SelectionMode == DataGridSelectionMode.Extended &&
            TotalItemCount > 0 &&
            scope is not null)
        {
            Selection = Selection.WithAllMatching(scope);
        }
    }

    /// <summary>
    /// Raises the LoadingRowGroup event
    /// </summary>
    /// <param name="e">
    /// EventArgs
    /// </param>
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
    /// <param name="e">
    /// EventArgs
    /// </param>
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
    /// <param name="e">
    /// Contains the necessary information for generating the row clipboard content.
    /// </param>
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

    /// <summary>
    /// Besides the base behavior, keeps the derived state of the grid in step with the changed property: frame
    /// border and corner radii, pager visibility, column sort and filter capabilities, row grid lines, frozen
    /// column limits and the pinned-open filter target. An override must call the base member, which this
    /// implementation does first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        var refreshDisplayedRowsGridLines = false;
        if (change.Property == BorderThicknessProperty ||
            change.Property == GridLinesVisibilityProperty ||
            change.Property == IsFrameBorderVisibleProperty ||
            change.Property == FooterProperty)
        {
            ConfigureFrameBorderThickness();
            refreshDisplayedRowsGridLines = true;
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == IsFrameBorderVisibleProperty)
        {
            ConfigureFrameCornerRadius();
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == TitleProperty ||
            change.Property == HeadersVisibilityProperty)
        {
            ConfigureHeaderCornerRadius();
        }

        if (change.Property == IsVisibleProperty)
        {
            if (IsVisible)
            {
                RefreshPopupPinnedOpenFilterTarget();
            }
            else
            {
                SuspendPopupPinnedOpenFilterTarget();
            }
        }

        if (change.Property == PageSizeProperty)
        {
            ReConfigurePagination();
            refreshDisplayedRowsGridLines = true;
        }
        else if (change.Property == PaginationVisibilityProperty ||
                 change.Property == IsHideOnSinglePageProperty)
        {
            ConfigurePaginationVisibility();
            refreshDisplayedRowsGridLines = true;
        }

        if (change.Property == CanUserSortColumnsProperty ||
            change.Property == CanUserFilterColumnsProperty)
        {
            RefreshColumnSortCapabilities();
        }

        if (refreshDisplayedRowsGridLines)
        {
            RefreshDisplayedRowsGridLines();
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
        }
    }
}
