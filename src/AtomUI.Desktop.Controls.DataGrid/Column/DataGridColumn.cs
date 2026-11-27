// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// The base class of a <see cref="DataGrid" /> column. A column holds the header, the width, the visibility
/// and the sort and filter settings of one grid column, and creates the elements of its cells through
/// <see cref="GenerateElement" /> and <see cref="GenerateEditingElement" />. It is not a control: it is a
/// resource host that forwards lookups to its <see cref="OwningGrid" />.
/// </summary>
public abstract partial class DataGridColumn : AvaloniaObject, IResourceHost, IThemeVariantHost, IDataContextProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Width" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridLength> WidthProperty = 
        AvaloniaProperty.Register<DataGridColumn, DataGridLength>(nameof(Width), coerce: CoerceWidth);
    
    /// <summary>
    /// Defines the <see cref="IsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsVisibleProperty =
        Control.IsVisibleProperty.AddOwner<DataGridColumn>();

    /// <summary>
    /// Identifies the <see cref="DataContext" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> DataContextProperty =
        StyledElement.DataContextProperty.AddOwner<DataGridColumn>();
    
    /// <summary>
    /// Identifies the <see cref="CellTheme" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridColumn, ControlTheme?> CellThemeProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumn, ControlTheme?>(
            nameof(CellTheme),
            o => o.CellTheme,
            (o, v) => o.CellTheme = v);
    
    /// <summary>
    /// Identifies the <see cref="Header" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridColumn, object?> HeaderProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumn, object?>(
            nameof(Header),
            o => o.Header,
            (o, v) => o.Header = v);
    
    /// <summary>
    /// Identifies the <see cref="HeaderTemplate" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridColumn, IDataTemplate?> HeaderTemplateProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumn, IDataTemplate?>(
            nameof(HeaderTemplate),
            o => o.HeaderTemplate,
            (o, v) => o.HeaderTemplate = v);
    
    /// <summary>
    /// Identifies the <see cref="SupportedSortDirections" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridSortDirections> SupportedSortDirectionsProperty =
        AvaloniaProperty.Register<DataGridColumn, DataGridSortDirections>(nameof(SupportedSortDirections), DataGridSortDirections.All);

    /// <summary>
    /// Identifies the <see cref="FieldId" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridFieldId?> FieldIdProperty =
        AvaloniaProperty.Register<DataGridColumn, DataGridFieldId?>(
            nameof(FieldId),
            validate: static value => !value.HasValue || value.Value.IsValid);

    /// <summary>
    /// Identifies the <see cref="SortState" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridColumn, DataGridColumnSortState> SortStateProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumn, DataGridColumnSortState>(
            nameof(SortState),
            column => column.SortState);

    /// <summary>
    /// Identifies the <see cref="Filters" /> property.
    /// </summary>
    public static readonly StyledProperty<IEnumerable?> FiltersProperty =
        AvaloniaProperty.Register<DataGridColumn, IEnumerable?>(nameof(Filters));

    /// <summary>
    /// Identifies the <see cref="SelectedFilterValues" /> property.
    /// </summary>
    public static readonly StyledProperty<IList?> SelectedFilterValuesProperty =
        AvaloniaProperty.Register<DataGridColumn, IList?>(
            nameof(SelectedFilterValues),
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Identifies the <see cref="FilterPresenterMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridFilterPresenterMode> FilterPresenterModeProperty =
        AvaloniaProperty.Register<DataGridColumn, DataGridFilterPresenterMode>(nameof(FilterPresenterMode));

    /// <summary>
    /// Identifies the <see cref="FilterSelectionMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridFilterSelectionMode> FilterSelectionModeProperty =
        AvaloniaProperty.Register<DataGridColumn, DataGridFilterSelectionMode>(
            nameof(FilterSelectionMode),
            DataGridFilterSelectionMode.Multiple);

    /// <summary>
    /// Identifies the <see cref="FilterApplyMode" /> property.
    /// </summary>
    public static readonly StyledProperty<DataGridFilterApplyMode> FilterApplyModeProperty =
        AvaloniaProperty.Register<DataGridColumn, DataGridFilterApplyMode>(nameof(FilterApplyMode));
    
    /// <summary>
    /// Identifies the <see cref="HeaderContentHorizontalAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<HorizontalAlignment> HeaderContentHorizontalAlignmentProperty = 
        AvaloniaProperty.Register<DataGridColumn, HorizontalAlignment>(nameof (HeaderContentHorizontalAlignment), HorizontalAlignment.Left);
    
    /// <summary>
    /// Identifies the <see cref="HeaderContentVerticalAlignment" /> property.
    /// </summary>
    public static readonly StyledProperty<VerticalAlignment> HeaderContentVerticalAlignmentProperty =
        AvaloniaProperty.Register<DataGridColumn, VerticalAlignment>(nameof (HeaderContentVerticalAlignment), VerticalAlignment.Center);
    
    /// <summary>
    /// Determines whether or not this column is visible.
    /// </summary>
    public bool IsVisible
    {
        get => GetValue(IsVisibleProperty);
        set => SetValue(IsVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the data context of the column. The column is not a control, so the value is not passed on
    /// to the cells; it only serves bindings that are anchored to the column.
    /// </summary>
    public object? DataContext
    {
        get => GetValue(DataContextProperty);
        set => SetValue(DataContextProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the width of the column as a <see cref="DataGridLength" />: a pixel size, a star share of
    /// the remaining width or automatic sizing. The width is limited by <see cref="MinWidth" /> and
    /// <see cref="MaxWidth" /> and is reported by <see cref="ActualWidth" />. While no value is set, and again
    /// after it is cleared, the column takes the <see cref="DataGrid.ColumnWidth" /> of the owning grid, or
    /// <see cref="DataGridLength.Auto" /> when it has no grid.
    /// </summary>
    public DataGridLength Width
    {
        get => GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the theme that the cells of this column use. When it is <see langword="null" />, the cells
    /// have no explicit theme and get the one that the grid's styles provide.
    /// </summary>
    public ControlTheme? CellTheme
    {
        get => _cellTheme;
        set => SetAndRaise(CellThemeProperty, ref _cellTheme, value);
    }
    
    /// <summary>
    /// Gets or sets the <see cref="DataGridColumnHeader" /> content
    /// </summary>
    public object? Header
    {
        get => _header;
        set => SetAndRaise(HeaderProperty, ref _header, value);
    }
    
    /// <summary>
    /// Gets or sets an <see cref="IDataTemplate" /> for the <see cref="Header" />
    /// </summary>
    public IDataTemplate? HeaderTemplate
    {
        get => _headerTemplate;
        set => SetAndRaise(HeaderTemplateProperty, ref _headerTemplate, value);
    }
    
    /// <summary>
    /// Gets or sets the sort directions that the user can choose for this column. The default is
    /// <see cref="DataGridSortDirections.All" />. While a <see cref="FieldId" /> is set, the directions are
    /// further limited to those that the query field supports.
    /// </summary>
    public DataGridSortDirections SupportedSortDirections
    {
        get => GetValue(SupportedSortDirectionsProperty);
        set => SetValue(SupportedSortDirectionsProperty, value);
    }

    /// <summary>
    /// Gets or sets the field of the owning grid's query that this column stands for. The column contributes
    /// a sort or a filter to the query only while a valid field is set; <see langword="null" /> leaves it
    /// without one. The default value of <see cref="DataGridFieldId" /> is rejected by validation.
    /// </summary>
    public DataGridFieldId? FieldId
    {
        get => GetValue(FieldIdProperty);
        set => SetValue(FieldIdProperty, value);
    }

    /// <summary>
    /// Gets the sort state of the column as the owning grid reports it: the direction the column is sorted
    /// in and the position of its sort among the active sorts, or <see cref="DataGridColumnSortState.None" />
    /// when the column is not sorted. Applications can only read the value; changes are raised through
    /// <see cref="SortStateProperty" />.
    /// </summary>
    public DataGridColumnSortState SortState => _sortState;
    
    /// <summary>
    /// Gets or sets how the content of the column header is aligned horizontally. The default is
    /// <see cref="HorizontalAlignment.Left" />. No framework code reads this value, so setting it does not
    /// change the header.
    /// </summary>
    public HorizontalAlignment HeaderContentHorizontalAlignment
    {
        get => GetValue(HeaderContentHorizontalAlignmentProperty);
        set => SetValue(HeaderContentHorizontalAlignmentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets how the content of the column header is aligned vertically. The default is
    /// <see cref="VerticalAlignment.Center" />. No framework code reads this value, so setting it does not
    /// change the header.
    /// </summary>
    public VerticalAlignment HeaderContentVerticalAlignment
    {
        get => GetValue(HeaderContentVerticalAlignmentProperty);
        set => SetValue(HeaderContentVerticalAlignmentProperty, value);
    }

    /// <summary>
    /// Actual visible width after Width, MinWidth, and MaxWidth setting at the Column level and DataGrid level
    /// have been taken into account
    /// </summary>
    public double ActualWidth
    {
        get
        {
            if (OwningGrid == null || double.IsNaN(Width.DisplayValue))
            {
                return ActualMinWidth;
            }
            return Width.DisplayValue;
        }
    }
    
    /// <summary>
    /// Gets or sets the maximum width of the column in device-independent pixels. When no value is set, the
    /// getter reports positive infinity and the effective maximum comes from the owning grid. Setting a
    /// negative value or a value below <see cref="ActualMinWidth" /> throws.
    /// </summary>
    public double MaxWidth
    {
        get => _maxWidth ?? double.PositiveInfinity;
        set
        {
            if (value < 0)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "MaxWidth", 0);
            }
            if (value < ActualMinWidth)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "MaxWidth", "MinWidth");
            }
            if (!_maxWidth.HasValue || !MathUtils.AreClose(_maxWidth.Value, value))
            {
                double oldValue = ActualMaxWidth;
                _maxWidth = value;
                if (OwningGrid != null)
                {
                    OwningGrid.HandleColumnMaxWidthChanged(this, oldValue);
                }
            }
        }
    }

    /// <summary>
    /// Gets or sets the minimum width of the column in device-independent pixels. When no value is set, the
    /// getter reports 0 and the effective minimum comes from the owning grid. Setting <see cref="double.NaN" />,
    /// a negative value, positive infinity or a value above <see cref="ActualMaxWidth" /> throws.
    /// </summary>
    public double MinWidth
    {
        get => _minWidth ?? 0;
        set
        {
            if (double.IsNaN(value))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToNAN("MinWidth");
            }
            if (value < 0)
            {
                throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo("value", "MinWidth", 0);
            }
            if (double.IsPositiveInfinity(value))
            {
                throw DataGridError.DataGrid.ValueCannotBeSetToInfinity("MinWidth");
            }
            if (value > ActualMaxWidth)
            {
                throw DataGridError.DataGrid.ValueMustBeLessThanOrEqualTo("value", "MinWidth", "MaxWidth");
            }
            if (!_minWidth.HasValue || !MathUtils.AreClose(_minWidth.Value, value))
            {
                double oldValue = ActualMinWidth;
                _minWidth = value;
                if (OwningGrid != null)
                {
                    OwningGrid.HandleColumnMinWidthChanged(this, oldValue);
                }
            }
        }
    }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the user can change the column display position by
    /// dragging the column header.
    /// </summary>
    /// <returns>
    /// true if the user can drag the column header to a new position; otherwise, false. The default is the current <see cref="P:AtomUI.Desktop.Controls.DataGrid.CanUserReorderColumns" /> property value.
    /// </returns>
    public bool CanUserReorder
    {
        get => CanUserReorderInternal ??
               OwningGrid?.CanUserReorderColumns ??
               DataGrid.DefaultCanUserReorderColumns;
        set =>  CanUserReorderInternal = value;
    }
    
    /// <summary>
    /// Gets or sets a value that indicates whether the user can adjust the column width using the mouse.
    /// </summary>
    /// <returns>
    /// true if the user can resize the column; false if the user cannot resize the column. The default is the current <see cref="P:AtomUI.Desktop.Controls.DataGrid.CanUserResizeColumns" /> property value.
    /// </returns>
    public bool CanUserResize
    {
        get => CanUserResizeInternal ??
               OwningGrid?.CanUserResizeColumns ??
               DataGrid.DefaultCanUserResizeColumns;
        set
        {
            CanUserResizeInternal = value;
            OwningGrid?.HandleColumnCanUserResizeChanged(this);
        }
    }

    /// <summary>
    /// Gets or sets whether the user can filter this column through its header. Setting the value also
    /// passes it to the header cell. When the column has no value of its own, the owning grid decides; a
    /// column without a grid uses <see cref="DataGrid.DefaultCanUserFilterColumns" />.
    /// </summary>
    public bool CanUserFilter
    {
        get
        {
            var canUserFilter = OwningGrid is null
                ? CanUserFilterInternal ?? DataGrid.DefaultCanUserFilterColumns
                : EffectiveCanUserFilter;
            if (HasHeaderCell)
            {
                HeaderCell.CanUserFilter = canUserFilter;
            }
            return canUserFilter;
        }
        set
        {
            CanUserFilterInternal = value;
            if (HasHeaderCell)
            {
                HeaderCell.CanUserFilter = EffectiveCanUserFilter;
            }
            OwningGrid?.RefreshPopupPinnedOpenFilterTarget();
        }
    }

    /// <summary>
    /// Gets or sets whether the header of this column shows the tooltip of its sort indicator. When the
    /// column has no value of its own, it follows the <see cref="DataGrid.IsSorterTooltipVisible" /> of the
    /// owning grid; a column without a grid uses <see cref="DataGrid.DefaultIsSorterTooltipVisible" />.
    /// </summary>
    public bool IsSorterTooltipVisible
    {
        get
        {
            var isSorterTooltipVisible = IsSorterTooltipVisibleInternal ??
                                    OwningGrid?.IsSorterTooltipVisible ??
                                    DataGrid.DefaultIsSorterTooltipVisible;
            if (HasHeaderCell)
            {
                HeaderCell.IsSorterTooltipVisible = isSorterTooltipVisible;
            }
            return isSorterTooltipVisible;
        }
        
        set
        {
            IsSorterTooltipVisibleInternal = value;
            if (HasHeaderCell)
            {
                HeaderCell.IsSorterTooltipVisible = value;
            }
        }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the user can sort the column by clicking the column header.
    /// </summary>
    /// <returns>
    /// true if the user can sort the column; false if the user cannot sort the column. The default is the current <see cref="P:AtomUI.Desktop.Controls.DataGrid.CanUserSortColumns" /> property value.
    /// </returns>
    public bool? CanUserSort
    {
        get => CanUserSortInternal;
        set
        {
            CanUserSortInternal = value;
            RefreshSortProjection();
        }
    }

    /// <summary>
    /// Gets or sets the display position of the column relative to the other columns in the <see cref="T:AtomUI.Desktop.Controls.DataGrid" />.
    /// </summary>
    /// <returns>
    /// The zero-based position of the column as it is displayed in the associated <see cref="T:AtomUI.Desktop.Controls.DataGrid" />. The default is the index of the corresponding <see cref="P:System.Collections.ObjectModel.Collection`1.Item(System.Int32)" /> in the <see cref="P:AtomUI.Desktop.Controls.DataGrid.Columns" /> collection.
    /// </returns>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// When setting this property, the specified value is less than -1 or equal to <see cref="F:System.Int32.MaxValue" />.
    ///
    /// -or-
    ///
    /// When setting this property on a column in a <see cref="T:AtomUI.Desktop.Controls.DataGrid" />, the specified value is less than zero or greater than or equal to the number of columns in the <see cref="T:AtomUI.Desktop.Controls.DataGrid" />.
    /// </exception>
    /// <exception cref="T:System.InvalidOperationException">
    /// When setting this property, the <see cref="T:AtomUI.Desktop.Controls.DataGrid" /> is already making <see cref="P:AtomUI.Desktop.Controls.DataGridColumn.DisplayIndex" /> adjustments. For example, this exception is thrown when you attempt to set <see cref="P:AtomUI.Desktop.Controls.DataGridColumn.DisplayIndex" /> in a <see cref="E:Avalonia.Controls.DataGrid.ColumnDisplayIndexChanged" /> event handler.
    ///
    /// -or-
    ///
    /// When setting this property, the specified value would result in a frozen column being displayed in the range of unfrozen columns, or an unfrozen column being displayed in the range of frozen columns.
    /// </exception>
    public int DisplayIndex
    {
        get
        {
            if (OwningGrid != null && 
                OwningGrid.ColumnsInternal.RowGroupSpacerColumn != null && 
                OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented)
            {
                return _displayIndexWithFiller - 1;
            }
            return _displayIndexWithFiller;
        }
        set
        {
            if (value == Int32.MaxValue)
            {
                throw DataGridError.DataGrid.ValueMustBeLessThan(nameof(value), nameof(DisplayIndex), Int32.MaxValue);
            }
            if (OwningGrid != null)
            {
                if (OwningGrid.ColumnsInternal.RowGroupSpacerColumn != null && OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented)
                {
                    value++;
                }
                if (_displayIndexWithFiller != value)
                {
                    if (value < 0 || value >= OwningGrid.ColumnsItemsInternal.Count)
                    {
                        throw DataGridError.DataGrid.ValueMustBeBetween(nameof(value), nameof(DisplayIndex), 0, true, OwningGrid.Columns.Count, false);
                    }
                    // Will throw an error if a visible frozen column is placed inside a non-frozen area or vice-versa.
                    OwningGrid.HandleColumnDisplayIndexChanging(this, value);
                    _displayIndexWithFiller = value;
                    try
                    {
                        OwningGrid.InDisplayIndexAdjustments = true;
                        OwningGrid.HandleColumnDisplayIndexChanged(this);
                        OwningGrid.HandleColumnDisplayIndexChangedPostNotification();
                    }
                    finally
                    {
                        OwningGrid.InDisplayIndexAdjustments = false;
                    }
                }
            }
            else
            {
                if (value < -1)
                {
                    throw DataGridError.DataGrid.ValueMustBeGreaterThanOrEqualTo(nameof(value), nameof(DisplayIndex), -1);
                }
                _displayIndexWithFiller = value;
            }
        }
    }

    /// <summary>
    /// Gets the style classes that the cells of this column use: the class list of each cell is replaced
    /// with these classes. The collection is created on first access.
    /// </summary>
    public Classes CellStyleClasses => _cellStyleClasses ??= new();
    
    /// <summary>
    /// The binding that will be used to get or set cell content for the clipboard.
    /// </summary>
    public virtual BindingBase? ClipboardContentBinding { get; set; }
    
    /// <summary>
    /// Gets or sets an object associated with this column.
    /// </summary>
    public object? Tag { get; set; }

    /// <summary>
    /// Gets or sets the filter items that the filter of this column offers, for example the values a user
    /// can pick. When the items change, selected filter values that are no longer among them are removed
    /// from <see cref="SelectedFilterValues" />.
    /// </summary>
    public IEnumerable? Filters
    {
        get => GetValue(FiltersProperty);
        set => SetValue(FiltersProperty, value);
    }

    /// <summary>
    /// Gets or sets the filter values that are selected for this column. The property binds two-way by
    /// default, so the filter writes the user's selection back to the source. Every change is passed to the
    /// owning grid as a filter gesture. <see langword="null" /> means that the column has no selection; when
    /// the column joins a grid, it then leaves the filter that the grid already has untouched.
    /// </summary>
    public IList? SelectedFilterValues
    {
        get => GetValue(SelectedFilterValuesProperty);
        set => SetValue(SelectedFilterValuesProperty, value);
    }

    /// <summary>
    /// Gets or sets how the filter of this column is presented in its header: as a menu
    /// (<see cref="DataGridFilterPresenterMode.Menu" />, the default) or as a tree
    /// (<see cref="DataGridFilterPresenterMode.Tree" />).
    /// </summary>
    public DataGridFilterPresenterMode FilterPresenterMode
    {
        get => GetValue(FilterPresenterModeProperty);
        set => SetValue(FilterPresenterModeProperty, value);
    }

    /// <summary>
    /// Gets or sets how many filter values the user can select for this column: one with
    /// <see cref="DataGridFilterSelectionMode.Single" />, or several with
    /// <see cref="DataGridFilterSelectionMode.Multiple" />, the default.
    /// </summary>
    public DataGridFilterSelectionMode FilterSelectionMode
    {
        get => GetValue(FilterSelectionModeProperty);
        set => SetValue(FilterSelectionModeProperty, value);
    }

    /// <summary>
    /// Gets or sets when the filter selection of this column is applied: when the user confirms it
    /// (<see cref="DataGridFilterApplyMode.Confirm" />, the default), when the filter flyout closes
    /// (<see cref="DataGridFilterApplyMode.Close" />), or whenever the selection changes
    /// (<see cref="DataGridFilterApplyMode.SelectionChanged" />).
    /// </summary>
    public DataGridFilterApplyMode FilterApplyMode
    {
        get => GetValue(FilterApplyModeProperty);
        set => SetValue(FilterApplyModeProperty, value);
    }
    
    /// <summary>
    /// A custom filter that receives the filter condition as its first parameter and the data record as
    /// its second parameter. No framework code invokes this delegate.
    /// </summary>
    public Func<object, object, bool>? OnFilter { get; set; }

    /// <summary>
    /// A custom filter evaluator that decides whether a data record matches a filter condition. No
    /// framework code invokes this delegate.
    /// </summary>
    public Func<object?, object?, bool>? FilterEvaluator { get; set; }
    
    /// <summary>
    /// Gets or sets whether the cells of this column can be edited. <see langword="true" /> makes every cell
    /// of the column read-only, even when the owning grid is editable, and a read-only owning grid keeps
    /// the column read-only even after <see langword="false" /> was set. Without a value on the column, the
    /// owning grid decides for this column; a column without a grid uses the default of its column type.
    /// After the first assignment the column always has an explicit value.
    /// </summary>
    public virtual bool IsReadOnly
    {
        get
        {
            if (OwningGrid == null)
            {
                return _isReadOnly ?? DefaultIsReadOnly;
            }
            if (_isReadOnly != null)
            {
                return _isReadOnly.Value || OwningGrid.IsReadOnly;
            }
            return OwningGrid.GetColumnReadOnlyState(this, DefaultIsReadOnly);
        }
        set
        {
            if (value != _isReadOnly)
            {
                OwningGrid?.HandleColumnReadOnlyStateChanging(this, value);
                _isReadOnly = value;
            }
        }
    }
    
    /// <summary>
    /// Gets whether the grid created this column through its column auto-generation, rather than the
    /// application adding it to <see cref="DataGrid.Columns" />. Only the grid sets this value.
    /// </summary>
    public bool IsAutoGenerated { get; internal set; }

    /// <summary>
    /// Gets or sets whether the column is frozen. The value is <see langword="true" /> when the column is
    /// frozen on either side. Setting it assigns the value to both <see cref="IsLeftFrozen" /> and
    /// <see cref="IsRightFrozen" />.
    /// </summary>
    public bool IsFrozen
    {
        get => IsLeftFrozen || IsRightFrozen;
        set  {
            IsLeftFrozen  = value;
            IsRightFrozen = value;
        }
    }
    
    /// <summary>
    /// Gets whether the owning grid keeps this column fixed at its left edge while the other columns
    /// scroll horizontally. Only the grid sets this value.
    /// </summary>
    public bool IsLeftFrozen { get; internal set; }
    
    /// <summary>
    /// Gets whether the owning grid keeps this column fixed at its right edge while the other columns
    /// scroll horizontally. Only the grid sets this value.
    /// </summary>
    public bool IsRightFrozen { get; internal set; }
    
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Occurs when the pointer is pressed over the column's header
    /// </summary>
    public event EventHandler<PointerPressedEventArgs>? HeaderPointerPressed;
    /// <summary>
    /// Occurs when the pointer is released over the column's header
    /// </summary>
    public event EventHandler<PointerReleasedEventArgs>? HeaderPointerReleased;

    internal void NotifyHeaderPointerPressed(PointerPressedEventArgs e)
    {
        HeaderPointerPressed?.Invoke(this, e);
    }

    internal void NotifyHeaderPointerReleased(PointerReleasedEventArgs e)
    {
        HeaderPointerReleased?.Invoke(this, e);
    }

    #endregion
    
    #region 继承属性定义
    
    private DataGrid? _owningGrid;
    /// <summary>
    /// Gets the <see cref="DataGrid" /> that this column belongs to, or <see langword="null" /> when the
    /// column is not in a grid. The column collection of the grid assigns it when the column is added, and
    /// clears it, after <see cref="NotifyOwningGridAboutToDetached" />, when the column is removed. An
    /// assignment moves the resource lookup of the column and the bindings of its header cell to the new grid.
    /// </summary>
    protected internal DataGrid? OwningGrid
    {
        get => _owningGrid;
        internal set
        {
            if (ReferenceEquals(_owningGrid, value))
            {
                NotifyOwningGridAttached(_owningGrid);
                return;
            }

            UnregisterOwningGridResourceHost();
            _headerCell?.ReleaseOwningGridBindings();
            _owningGrid = value;
            RegisterOwningGridResourceHost(_owningGrid);
            if (_owningGrid is not null)
            {
                _headerCell?.BindOwningGridProperties(_owningGrid);
            }
            NotifyOwningGridAttached(_owningGrid);
        }
    }
    #endregion

    #region 资源宿主定义

    /// <summary>
    /// Raised when the owning grid raised its own <see cref="StyledElement.ResourcesChanged" /> event, and
    /// once whenever the column is attached to or detached from a grid, after the new resource owner is in
    /// place.
    /// </summary>
    public event EventHandler<ResourcesChangedEventArgs>? ResourcesChanged;
    /// <summary>
    /// Raised when the owning grid raised its own <see cref="StyledElement.ActualThemeVariantChanged" />
    /// event, and once whenever the column is attached to or detached from a grid, after
    /// <see cref="ResourcesChanged" />.
    /// </summary>
    public event EventHandler? ActualThemeVariantChanged;

    private DataGrid? _subscribedResourceHostGrid;

    /// <summary>
    /// Always <see langword="true" />: the column takes part in resource lookups because it forwards them
    /// to its owning grid and to the application.
    /// </summary>
    public bool HasResources => true;

    /// <summary>
    /// Gets the theme variant of the owning grid, or of the application when the column has no grid. When
    /// there is no application either, the value is <see cref="ThemeVariant.Default" />.
    /// </summary>
    public ThemeVariant ActualThemeVariant =>
        _owningGrid?.ActualThemeVariant ??
        Application.Current?.ActualThemeVariant ??
        ThemeVariant.Default;

    /// <summary>
    /// Looks a resource key up in the owning grid and then in the application.
    /// </summary>
    /// <param name="key">
    /// The resource key to look up.
    /// </param>
    /// <param name="theme">
    /// The theme variant to look the key up in, or <see langword="null" /> to leave the choice to the
    /// resource owner.
    /// </param>
    /// <param name="value">
    /// When the method returns, the resource that was found, or <see langword="null" /> when the key was
    /// not found.
    /// </param>
    /// <returns>
    /// <see langword="true" /> with the resource in <paramref name="value" /> when the grid or the
    /// application provides the key; otherwise <see langword="false" /> with <paramref name="value" /> set
    /// to <see langword="null" />.
    /// </returns>
    public bool TryGetResource(object key, ThemeVariant? theme, out object? value)
    {
        if (_owningGrid?.TryFindResource(key, theme, out value) == true)
        {
            return true;
        }

        if (Application.Current?.TryGetResource(key, theme, out value) == true)
        {
            return true;
        }

        value = null;
        return false;
    }

    void IResourceHost.NotifyHostedResourcesChanged(ResourcesChangedEventArgs e)
    {
        ResourcesChanged?.Invoke(this, e);
    }

    private void RegisterOwningGridResourceHost(DataGrid? owningGrid)
    {
        if (owningGrid is null)
        {
            RaiseResourcesChanged();
            return;
        }

        _subscribedResourceHostGrid = owningGrid;
        _subscribedResourceHostGrid.ResourcesChanged += HandleOwningGridResourcesChanged;
        _subscribedResourceHostGrid.ActualThemeVariantChanged += HandleOwningGridActualThemeVariantChanged;
        RaiseResourcesChanged();
        ActualThemeVariantChanged?.Invoke(this, EventArgs.Empty);
    }

    private void UnregisterOwningGridResourceHost()
    {
        if (_subscribedResourceHostGrid is null)
        {
            return;
        }

        _subscribedResourceHostGrid.ResourcesChanged -= HandleOwningGridResourcesChanged;
        _subscribedResourceHostGrid.ActualThemeVariantChanged -= HandleOwningGridActualThemeVariantChanged;
        _subscribedResourceHostGrid = null;
    }

    private void HandleOwningGridResourcesChanged(object? sender, ResourcesChangedEventArgs e)
    {
        ResourcesChanged?.Invoke(this, e);
    }

    private void HandleOwningGridActualThemeVariantChanged(object? sender, EventArgs e)
    {
        ActualThemeVariantChanged?.Invoke(this, e);
    }

    private void RaiseResourcesChanged()
    {
        ResourcesChanged?.Invoke(this, ResourcesChangedEventArgs.Create());
    }

    #endregion
    
    /// <summary>
    /// Initializes a new instance of the <see cref="T:AtomUI.Desktop.Controls.DataGridColumn" /> class.
    /// </summary>
    protected internal DataGridColumn()
    {
        _displayIndexWithFiller         = -1;
        IsInitialDesiredWidthDetermined = false;
        InheritsWidth                   = true;
    }

    /// <summary>
    /// Determines whether the items that a <see cref="NotifyCollectionChangedAction.Remove" /> event
    /// reports as removed contain the given item.
    /// </summary>
    /// <param name="e">
    /// The collection change event to inspect.
    /// </param>
    /// <param name="item">
    /// The item to look for, compared with <c>Equals</c>.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the event reports a removal and one of its old items equals
    /// <paramref name="item" />; otherwise <see langword="false" />.
    /// </returns>
    protected static bool RemovedItemsContain(NotifyCollectionChangedEventArgs e, object item)
    {
        if (e.Action != NotifyCollectionChangedAction.Remove ||
            e.OldItems is not { Count: > 0 } oldItems)
        {
            return false;
        }

        for (var i = 0; i < oldItems.Count; i++)
        {
            if (Equals(oldItems[i], item))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Called after <see cref="OwningGrid" /> is assigned: also when the same grid is assigned again, and
    /// once with <see langword="null" /> when the column is detached. For a grid that is not
    /// <see langword="null" />, it subscribes the column to its filter collections and refreshes its sort
    /// projection. Overrides must call the base member to keep both.
    /// </summary>
    /// <param name="owningGrid">
    /// The grid that now owns the column, or <see langword="null" /> when the column was detached.
    /// </param>
    protected virtual void NotifyOwningGridAttached(DataGrid? owningGrid)
    {
        if (owningGrid != null)
        {
            RegisterFilterCollectionSubscriptions();
            RefreshSortProjection();
        }
    }

    internal void NotifyOwningGridAttachedAfterColumnCollectionUpdate()
    {
        RefreshSortProjection();
        PruneSelectedFilterValuesToFilterItems();
        // The query is owned by the grid, so only a column with explicitly selected filter values writes them
        // back on attach. A null value must not clear a filter that already exists.
        if (SelectedFilterValues is not null)
        {
            ApplySelectedFilterValuesToQuery();
        }
    }
    
    /// <summary>
    /// Called by the owning grid before the column is detached from it, while <see cref="OwningGrid" />
    /// still returns the grid. It releases the subscriptions to the filter collections of the column and
    /// leaves the query of the grid unchanged, because the grid owns that query. Overrides must call the
    /// base member to release the subscriptions.
    /// </summary>
    protected internal virtual void NotifyOwningGridAboutToDetached()
    {
        RemoveFilterQueryProjection();
        ReleaseFilterCollectionSubscriptions();
    }
    
    /// <summary>
    /// Reacts to changes of the column's own properties after the base member processed them: it informs
    /// the owning grid and the header cell about a changed <see cref="IsVisible" /> or <see cref="Width" />,
    /// refreshes the sort projection for <see cref="FieldId" /> and <see cref="SupportedSortDirections" />,
    /// and re-registers and applies the filter settings for <see cref="Filters" />,
    /// <see cref="FilterPresenterMode" />, <see cref="FilterSelectionMode" /> and
    /// <see cref="SelectedFilterValues" />. Overrides must call the base member, otherwise none of this
    /// happens.
    /// </summary>
    /// <param name="change">
    /// The change that was raised for one of the column's properties.
    /// </param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsVisibleProperty)
        {
            OwningGrid?.HandleColumnVisibleStateChanging(this);
            var isVisible = change.GetNewValue<bool>();
        
            if (_headerCell != null)
            {
                _headerCell.IsVisible = isVisible;
            }
        
            OwningGrid?.HandleColumnVisibleStateChanged(this);
            NotifyPropertyChanged(change.Property.Name);
        }
        else if (change.Property == WidthProperty)
        {
            InheritsWidth = false;
            if (_setWidthInternalNoCallback == false)
            {
                var grid  = OwningGrid;
                if (change is AvaloniaPropertyChangedEventArgs<DataGridLength> dataGridLengthChange)
                {
                    var width = dataGridLengthChange.NewValue.Value;
                    if (grid != null)
                    {
                        var oldWidth = dataGridLengthChange.OldValue.Value;
                        if (width.IsStar != oldWidth.IsStar)
                        {
                            SetWidthInternalNoCallback(width);
                            IsInitialDesiredWidthDetermined = false;
                            grid.HandleColumnWidthChanged(this);
                        }
                        else
                        {
                            Resize(oldWidth, width, false);
                        }
                    }
                    else
                    {
                        SetWidthInternalNoCallback(width);
                    }
                }
                
            }
        }
        else if (change.Property == FieldIdProperty ||
                 change.Property == SupportedSortDirectionsProperty)
        {
            RefreshSortProjection();
        }
        else if (change.Property == FiltersProperty)
        {
            RegisterFilterItems(change.OldValue as IEnumerable, change.NewValue as IEnumerable);
            PruneSelectedFilterValuesToFilterItems();
            NotifyFilterItemsChanged();
        }
        else if (change.Property == FilterPresenterModeProperty ||
                 change.Property == FilterSelectionModeProperty)
        {
            if (HasHeaderCell)
            {
                HeaderCell.NotifyFilterConfigurationChanged();
            }
        }
        else if (change.Property == SelectedFilterValuesProperty)
        {
            RegisterSelectedFilterValues(change.OldValue as IList, change.NewValue as IList);
            ApplySelectedFilterValuesToQuery();
            NotifySelectedFilterValuesChanged();
        }
    }
    
    /// <summary>
    /// Returns the content element that this column shows in the given row.
    /// </summary>
    /// <param name="dataGridRow">
    /// The row whose cell content to return.
    /// </param>
    /// <returns>
    /// The cell content when it is a <see cref="Control" />; <see langword="null" /> when the row belongs
    /// to another grid or the content is not a <see cref="Control" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dataGridRow" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The column has no owning grid.
    /// </exception>
    public Control? GetCellContent(DataGridRow dataGridRow)
    {
        dataGridRow = dataGridRow ?? throw new ArgumentNullException(nameof(dataGridRow));
        if (OwningGrid == null)
        {
            throw DataGridError.DataGrid.NoOwningGrid(GetType());
        }
        if (dataGridRow.OwningGrid == OwningGrid)
        {
            DataGridCell dataGridCell = dataGridRow.Cells[Index];
            return dataGridCell.Content as Control;
        }
        return null;
    }

    /// <summary>
    /// Returns the content element that this column shows in the row that presents the given data item.
    /// </summary>
    /// <param name="dataItem">
    /// The data item of the row whose cell content to return.
    /// </param>
    /// <returns>
    /// The cell content when it is a <see cref="Control" />; <see langword="null" /> when the grid has no
    /// row for the item, the row belongs to another grid or the content is not a <see cref="Control" />.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="dataItem" /> is <see langword="null" />.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// The column has no owning grid.
    /// </exception>
    public Control? GetCellContent(object dataItem)
    {
        dataItem = dataItem ?? throw new ArgumentNullException(nameof(dataItem));
        if (OwningGrid == null)
        {
            throw DataGridError.DataGrid.NoOwningGrid(GetType());
        }
        DataGridRow? dataGridRow = OwningGrid.GetRowFromItem(dataItem);
        if (dataGridRow == null)
        {
            return null;
        }
        return GetCellContent(dataGridRow);
    }

    /// <summary>
    /// Returns the column which contains the given element
    /// </summary>
    /// <param name="element">
    /// element contained in a column
    /// </param>
    /// <returns>
    /// Column that contains the element, or null if not found
    /// </returns>
    public static DataGridColumn? GetColumnContainingElement(Control element)
    {
        // Use FindAncestorOfType instead of a hand-written visual tree walk: under Avalonia 12 the decoration
        // system of the window can form cyclic edges in the visual tree, and walking them makes the CPU spin.
        return element.FindAncestorOfType<DataGridCell>(includeSelf: true)?.OwningColumn
               ?? element.FindAncestorOfType<DataGridColumnHeader>(includeSelf: true)?.OwningColumn;
    }

    /// <summary>
    /// Clears the current sort direction
    /// </summary>
    public void ClearSort()
    {
        //InvokeProcessSort is already validating if sorting is possible
        _headerCell?.InvokeClearSort();
    }
    
    /// <summary>
    /// Changes the sort direction of this column
    /// </summary>
    /// <param name="direction">
    /// New sort direction
    /// </param>
    public void Sort(ListSortDirection? direction = null)
    {
        //InvokeProcessSort is already validating if sorting is possible
        _headerCell?.InvokeProcessSort(KeyModifiers.None, direction);
    }

    /// <summary>
    /// Clears the current filter of this column by passing a filter gesture with no values to the owning
    /// grid. The call does nothing while the column has no grid.
    /// </summary>
    public void ClearFilter()
    {
        SetSelectedFilterValuesFromFilterRequest(Array.Empty<object>());
    }

    /// <summary>
    /// Passes the given values to the owning grid as a filter gesture for this column, the same way as a
    /// selection in the filter of the column. The call does nothing while the column has no grid.
    /// </summary>
    /// <param name="filterValues">
    /// The filter values to select.
    /// </param>
    public void Filter(List<object> filterValues)
    {
        SetSelectedFilterValuesFromFilterRequest(filterValues);
    }

    /// <summary>
    /// When overridden in a derived class, causes the column cell being edited to revert to the unedited value.
    /// </summary>
    /// <param name="editingElement">
    /// The element that the column displays for a cell in editing mode.
    /// </param>
    /// <param name="uneditedValue">
    /// The previous, unedited value in the cell being edited.
    /// </param>
    protected virtual void CancelCellEdit(Control editingElement, object uneditedValue)
    { }

    /// <summary>
    /// When overridden in a derived class, gets an editing element that is bound to the column's <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the generated element.
    /// </param>
    /// <param name="dataItem">
    /// The data item represented by the row that contains the intended cell.
    /// </param>
    /// <param name="binding">
    /// When the method returns, contains the applied binding.
    /// </param>
    /// <returns>
    /// A new editing element that is bound to the column's <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </returns>
    protected abstract Control? GenerateEditingElement(DataGridCell cell, object dataItem, out BindingExpressionBase? binding);

    /// <summary>
    /// When overridden in a derived class, gets a read-only element that is bound to the column's
    /// <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </summary>
    /// <param name="cell">
    /// The cell that will contain the generated element.
    /// </param>
    /// <param name="dataItem">
    /// The data item represented by the row that contains the intended cell.
    /// </param>
    /// <returns>
    /// A new, read-only element that is bound to the column's <see cref="P:AtomUI.Desktop.Controls.DataGridBoundColumn.Binding" /> property value.
    /// </returns>
    protected abstract Control? GenerateElement(DataGridCell cell, object dataItem);

    /// <summary>
    /// Called by a specific column type when one of its properties changed,
    /// and its current cells need to be updated.
    /// </summary>
    /// <param name="propertyName">
    /// Indicates which property changed and caused this call
    /// </param>
    protected void NotifyPropertyChanged(string propertyName)
    {
        OwningGrid?.RefreshColumnElements(this, propertyName);
    }
    
    /// <summary>
    /// Set whether to allow editing
    /// </summary>
    public virtual bool IsEditable()
    {
        return true;
    }

    /// <summary>
    /// When overridden in a derived class, called when a cell in the column enters editing mode.
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
    protected abstract object? PrepareCellForEdit(Control editingElement, RoutedEventArgs editingEventArgs);

    /// <summary>
    /// Called by the DataGrid control when a column asked for its
    /// elements to be refreshed, typically because one of its properties changed.
    /// </summary>
    /// <param name="element">
    /// Indicates the element that needs to be refreshed
    /// </param>
    /// <param name="propertyName">
    /// Indicates which property changed and caused this call
    /// </param>
    protected internal virtual void RefreshCellContent(Control element, string propertyName)
    { }

    /// <summary>
    /// When overridden in a derived class, called when a cell in the column exits editing mode.
    /// </summary>
    protected virtual void EndCellEdit(Control editingElement)
    { }
}
