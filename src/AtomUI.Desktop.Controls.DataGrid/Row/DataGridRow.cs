// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using AtomUI.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
using Avalonia.Media;
using Avalonia.VisualTree;

using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// Visual container follows the DataGrid owner, including TokenTransitions in its template.
/// <summary>
/// One row of a <see cref="DataGrid" />: it holds the cells, the row header and the details section of one
/// data item. The grid creates, fills and recycles its rows itself.
/// </summary>
[TokenOwnerTransparent]
[PseudoClasses(StdPseudoClass.Selected, StdPseudoClass.Editing, StdPseudoClass.Invalid, DataGridPseudoClass.RowHover)]
public partial class DataGridRow : TemplatedControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the Header dependency property.
    /// </summary>
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<DataGridRow, object?>(nameof(Header));
    
    /// <summary>
    /// Identifies the <see cref="IsSelected" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridRow, bool> IsSelectedProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, bool>(
            nameof(IsSelected),
            o => o.IsSelected,
            (o, v) => o.IsSelected = v);
    
    /// <summary>
    /// Identifies the <see cref="IsValid" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridRow, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, bool>(
            nameof(IsValid),
            o => o.IsValid);

    /// <summary>
    /// Identifies the <see cref="DetailsTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> DetailsTemplateProperty =
        AvaloniaProperty.Register<DataGridRow, IDataTemplate?>(nameof(DetailsTemplate));
    
    /// <summary>
    /// Identifies the <see cref="HeaderContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> HeaderContentTemplateProperty =
        AvaloniaProperty.Register<DataGridRow, IDataTemplate?>(nameof(HeaderContentTemplate));
    
    /// <summary>
    /// Identifies the <see cref="IsDetailsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDetailsVisibleProperty =
        AvaloniaProperty.Register<DataGridRow, bool>(nameof(IsDetailsVisible));
    
    /// <summary>
    /// Identifies the <see cref="Index" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridRow, int> IndexProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, int>(nameof(Index), 
            o => o.Index, 
            (o, v) => o.Index = v);
    
    /// <summary>
    /// Identifies the <see cref="LogicIndex" /> property.
    /// </summary>
    public static readonly DirectProperty<DataGridRow, int> LogicIndexProperty =
        AvaloniaProperty.RegisterDirect<DataGridRow, int>(nameof(LogicIndex), 
            o => o.LogicIndex,
            (o, v) => o.LogicIndex = v);
    
    /// <summary>
    /// Content of the row header cell of the row, shown by its <see cref="DataGridRowHeader" />. A
    /// <see langword="null" /> leaves the cell empty.
    /// </summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    private bool _isSelected;
    
    /// <summary>
    /// Whether the row is selected. Setting it selects or deselects the row in the owning grid, and the grid
    /// assigns it itself whenever it applies the state of the row, so it follows the selection of the grid.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set => SetAndRaise(IsSelectedProperty, ref _isSelected, value);
    }
    
    /// <summary>
    /// Gets a value that indicates whether the data in a row is valid.
    /// </summary>
    public bool IsValid
    {
        get => _isValid;
        internal set => SetAndRaise(IsValidProperty, ref _isValid, value);
    }
    private bool _isValid = true;
    
    /// <summary>
    /// Gets or sets the template that is used to display the details section of the row.
    /// </summary>
    public IDataTemplate? DetailsTemplate
    {
        get => GetValue(DetailsTemplateProperty);
        set => SetValue(DetailsTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that is used to display the header content of the row.
    /// </summary>
    public IDataTemplate? HeaderContentTemplate
    {
        get => GetValue(HeaderContentTemplateProperty);
        set => SetValue(HeaderContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a value that indicates when the details section of the row is displayed.
    /// </summary>
    public bool IsDetailsVisible
    {
        get => GetValue(IsDetailsVisibleProperty);
        set => SetValue(IsDetailsVisibleProperty, value);
    }
    
    /// <summary>
    /// Index of the row
    /// </summary>
    public int Index
    {
        get => _index;
        internal set => SetAndRaise(IndexProperty, ref _index, value);
    }
    private int _index;
    
    /// <summary>
    /// The index of the row counted from 1, for display: always <see cref="Index" /> plus 1, so it is <c>0</c>
    /// while the row has no slot.
    /// </summary>
    public int LogicIndex
    {
        get => _logicIndex;
        internal set => SetAndRaise(LogicIndexProperty, ref _logicIndex, value);
    }
    private int _logicIndex;
    
    #endregion

    static DataGridRow()
    {
        HeaderProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleHeaderChanged(e));
        DetailsTemplateProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleDetailsTemplateChanged(e));
        HeaderContentTemplateProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleHeaderContentTemplateChanged(e));
        IsDetailsVisibleProperty.Changed.AddClassHandler<DataGridRow>((x, e) => x.HandleIsDetailsVisibleChanged(e));
        PointerPressedEvent.AddClassHandler<DataGridRow>((x, e) => x.HandlePointerPressed(e), handledEventsToo: true);
        IsTabStopProperty.OverrideDefaultValue<DataGridRow>(false);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridRow>(IsOffscreenBehavior.FromClip);
    }
    
    public DataGridRow()
    {
        Classes.Add(DataGridSemanticParts.BodyRowClass);
        MinHeight                 =  DefaultMinHeight;
        Index                     =  -1;
        IsValid                   =  true;
        Slot                      =  -1;
        _mouseOverColumnIndex     =  null;
        _detailsDesiredHeight     =  double.NaN;
        _detailsLoaded            =  false;
        _appliedDetailsVisibility =  false;
        Cells                     =  new DataGridCellCollection(this);
        Cells.CellAdded           += HandleCellAdded;
        Cells.CellRemoved         += HandleCellRemoved;
    }
    
    /// <summary>
    /// Returns the row which contains the given element
    /// </summary>
    /// <param name="element">
    /// element contained in a row
    /// </param>
    /// <returns>
    /// Row that contains the element, or null if not found
    /// </returns>
    public static DataGridRow? GetRowContainingElement(Control element)
    {
        // Walk up the tree to find the DataGridRow that contains the element
        Visual?      parent = element;
        DataGridRow? row    = parent as DataGridRow;
        while ((parent != null) && (row == null))
        {
            parent = parent.GetVisualParent();
            row    = parent as DataGridRow;
        }
        return row;
    }
    
    /// <summary>
    /// Arranges the content of the <see cref="T:AtomUI.Desktop.Controls.DataGridRow" />.
    /// </summary>
    /// <param name="finalSize">
    /// The final area within the parent that this element should use to arrange itself and its children.
    /// </param>
    /// <returns>
    /// The actual size used by the <see cref="T:AtomUI.Desktop.Controls.DataGridRow" />.
    /// </returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (OwningGrid == null)
        {
            return base.ArrangeOverride(finalSize);
        }
        
        // If the DataGrid was scrolled horizontally after our last Arrange, we need to make sure
        // the Cells and Details are Arranged again
        if (!MathUtils.AreClose(_lastHorizontalOffset, OwningGrid.HorizontalOffset))
        {
            _lastHorizontalOffset = OwningGrid.HorizontalOffset;
            InvalidateHorizontalArrange();
        }

        Size size = base.ArrangeOverride(finalSize);

        if (_checkDetailsContentHeight)
        {
            _checkDetailsContentHeight = false;
            EnsureDetailsContentHeight();
        }
        
        if (_bottomGridLine != null)
        {
            UpdateBottomGridLineClip(new Rect(
                0,
                0,
                Math.Max(0, DesiredSize.Width - OwningGrid.HorizontalOffset),
                _bottomGridLine.DesiredSize.Height));
        }

        return size;
    }

    internal void ApplyHiddenClipGeometry()
    {
        _hiddenClipGeometry ??= new RectangleGeometry();
        if (!ReferenceEquals(Clip, _hiddenClipGeometry))
        {
            Clip = _hiddenClipGeometry;
        }
    }

    internal void ClearHiddenClipGeometry()
    {
        if (Clip is not null)
        {
            Clip = null;
        }
    }
    
    /// <summary>
    /// Measures the row and returns a desired width of at least the width of the cell area of the owning grid,
    /// so that the row spans all columns.
    /// </summary>
    /// <param name="availableSize">
    /// The available size that the row can give to its children.
    /// </param>
    /// <returns>
    /// The measured size, with the width raised to the width of the cells of the grid, or the size measured by
    /// the base class when the row has no owning grid.
    /// </returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (OwningGrid == null)
        {
            return base.MeasureOverride(availableSize);
        }

        //Allow the DataGrid specific components to adjust themselves based on new values
        _headerElement?.InvalidateMeasure();
        _cellsElement?.InvalidateMeasure();
        _detailsElement?.InvalidateMeasure();

        Size desiredSize = base.MeasureOverride(availableSize);
        return desiredSize.WithWidth(Math.Max(desiredSize.Width, OwningGrid.CellsWidth));
    }
    
    /// <summary>
    /// Resolves the parts of the new row template and moves the existing content into them: the cells go into
    /// the new cells presenter, a pending details template is applied, and the grid lines and the row header
    /// are brought in line with the current state.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        RootElement = e.NameScope.Find<Panel>(DataGridRowThemeConstants.RootLayoutPart);
        if (RootElement != null)
        {
            ApplyState();
        }

        bool updateVerticalScrollBar = false;
        if (_cellsElement != null)
        {
            // If we're applying a new template, we  want to remove the cells from the previous _cellsElement
            _cellsElement.Children.Clear();
            updateVerticalScrollBar = true;
        }

        _cellsElement = e.NameScope.Find<DataGridCellsPresenter>(DataGridRowThemeConstants.CellsPresenterPart);
        if (_cellsElement != null)
        {
            _cellsElement.OwningRow = this;
            if (Parent is DataGridRowsPresenter dataGridRowsPresenter)
            {
                _cellsElement.OwningRowsPresenter = dataGridRowsPresenter;
            }
            // Cells that were already added before the Template was applied need to
            // be added to the Canvas
            if (Cells.Count > 0)
            {
                foreach (DataGridCell cell in Cells)
                {
                    _cellsElement.Children.Add(cell);
                }
            }
        }

        _detailsElement = e.NameScope.Find<DataGridDetailsPresenter>(DataGridRowThemeConstants.DetailsPresenterPart);
        if (_detailsElement != null && OwningGrid != null)
        {
            _detailsElement.OwningRow = this;
            if (ActualDetailsVisibility && ActualDetailsTemplate != null && _appliedDetailsTemplate == null)
            {
                // Apply the DetailsTemplate now that the row template is applied.
                SetDetailsVisibilityInternal(ActualDetailsVisibility, raiseNotification: _detailsVisibilityNotificationPending, animate: false);
                _detailsVisibilityNotificationPending = false;
            }
        }

        _bottomGridLineClipGeometry = null;
        _bottomGridLine             = e.NameScope.Find<Rectangle>(DataGridRowThemeConstants.BottomGridLinePart);
        EnsureGridLines();

        _headerElement = e.NameScope.Find<DataGridRowHeader>(DataGridRowThemeConstants.RowHeaderPart);
        if (_headerElement != null)
        {
            _headerElement.Owner = this;
            if (Header != null)
            {
                _headerElement.Content = Header;
            }
            EnsureHeaderStyleAndVisibility(null);
        }

        //The height of this row might have changed after applying a new style, so fix the vertical scroll bar
        if (OwningGrid != null && updateVerticalScrollBar)
        {
            OwningGrid.UpdateVerticalScrollBar();
        }
        
        if (OwningGrid != null && OwningGrid.IsRowHeadersVisible)
        {
            ApplyHeaderContentTemplate();
        }
        
    }
    
    /// <summary>
    /// Calls the base member, then sets <c>IsMouseOver</c>, which turns on the hover pseudo-class of the row.
    /// </summary>
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        IsMouseOver = true;
    }

    private void UpdateBottomGridLineClip(Rect clipRect)
    {
        if (_bottomGridLine is null)
        {
            return;
        }

        if (_bottomGridLineClipGeometry is null)
        {
            _bottomGridLineClipGeometry = new RectangleGeometry
            {
                Rect = clipRect
            };
        }
        else if (!_bottomGridLineClipGeometry.Rect.Equals(clipRect))
        {
            _bottomGridLineClipGeometry.Rect = clipRect;
            _bottomGridLine.InvalidateVisual();
        }

        if (!ReferenceEquals(_bottomGridLine.Clip, _bottomGridLineClipGeometry))
        {
            _bottomGridLine.Clip = _bottomGridLineClipGeometry;
        }
    }
    
    /// <summary>
    /// Clears <c>IsMouseOver</c> and with it the hover pseudo-class of the row, then calls the base member.
    /// </summary>
    protected override void OnPointerExited(PointerEventArgs e)
    {
        IsMouseOver = false;
        base.OnPointerExited(e);
    }
    
    /// <summary>
    /// Calls the base member, then keeps the row in step with changes of its own properties.
    /// </summary>
    /// <remarks>
    /// A new <c>DataContext</c> refreshes the cell content of the template columns of a recycled row and
    /// re-applies the header content template. A change of <see cref="IsSelected" /> is forwarded to the
    /// selection of the owning grid, to the selected pseudo-class and to the state of the cells. A change of
    /// <see cref="Index" /> sets <see cref="LogicIndex" /> to the new index plus 1.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DataContextProperty)
        {
            var owner = OwningGrid;
            if (owner != null && IsRecycled)
            {
                var columns = owner.ColumnsItemsInternal;
                var nc      = columns.Count;
                for (int ci = 0; ci < nc; ci++)
                {
                    if (columns[ci] is DataGridTemplateColumn column)
                    {
                        var content = (Control?)Cells[column.Index].Content;
                        if (content != null)
                        {
                            column.RefreshCellContent(content, nameof(DataGridTemplateColumn.CellTemplate));
                        }
                    }
                }
            }

            if (owner != null)
            {
                ApplyHeaderContentTemplate();
            }
        }
        else if (change.Property == IsSelectedProperty)
        {
            var value = change.GetNewValue<bool>();

            if (OwningGrid != null && Slot != -1)
            {
                OwningGrid.SetRowSelection(Slot, value, false);
            }

            PseudoClasses.Set(StdPseudoClass.Selected, value);
            // The row is the only authority for the selected state: a selection change must be passed on to the
            // :selected pseudo-class of every cell, otherwise cell-level styles (such as the background of the
            // sorted column giving way to the selection) cannot follow the selected state of the row
            ApplyCellsState();
        }
        else if (change.Property == IndexProperty)
        {
            LogicIndex = Index + 1;
        }
    }

}
