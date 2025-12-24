// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Diagnostics;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Utils;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Mixins;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Utilities;

namespace AtomUI.Desktop.Controls;

[PseudoClasses(StdPseudoClass.DragIndicator, StdPseudoClass.Pressed, StdPseudoClass.SortAscending, StdPseudoClass.SortDescending)]
internal partial class DataGridColumnHeader : ContentControl
{
    internal enum DragMode
    {
        None = 0,
        MouseDown = 1,
        Drag = 2,
        Resize = 3,
        Reorder = 4
    }

    #region 公共属性定义

    public static readonly StyledProperty<IBrush?> SeparatorBrushProperty =
        AvaloniaProperty.Register<DataGridColumnHeader, IBrush?>(nameof(SeparatorBrush));
    
    public static readonly StyledProperty<bool> IsSeparatorsVisibleProperty =
        AvaloniaProperty.Register<DataGridColumnHeader, bool>(
            nameof(IsSeparatorsVisible),
            defaultValue: true);
    
    public IBrush? SeparatorBrush
    {
        get => GetValue(SeparatorBrushProperty);
        set => SetValue(SeparatorBrushProperty, value);
    }
    
    public bool IsSeparatorsVisible
    {
        get => GetValue(IsSeparatorsVisibleProperty);
        set => SetValue(IsSeparatorsVisibleProperty, value);
    }

    #endregion

    #region 公共事件定义

    public event EventHandler<KeyModifiers>? LeftClick;

    #endregion

    #region 内部属性定义
    
    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridColumnHeader>();
    
    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsFirstVisibleProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsFirstVisible),
            o => o.IsFirstVisible,
            (o, v) => o.IsFirstVisible = v);
    
    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsLastVisibleProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsLastVisible),
            o => o.IsLastVisible,
            (o, v) => o.IsLastVisible = v);
    
    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsMiddleVisibleProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsMiddleVisible),
            o => o.IsMiddleVisible,
            (o, v) => o.IsMiddleVisible = v);
    
    internal static readonly DirectProperty<DataGridColumnHeader, bool> IndicatorLayoutVisibleProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IndicatorLayoutVisible),
            o => o.IndicatorLayoutVisible,
            (o, v) => o.IndicatorLayoutVisible = v);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridColumnHeader>();
    
    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsFrozenProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsFrozen),
            o => o.IsFrozen, 
            (o, v) => o.IsFrozen = v);
    
    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsShowFrozenShadowProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsShowFrozenShadow),
            o => o.IsShowFrozenShadow, 
            (o, v) => o.IsShowFrozenShadow = v);

    internal static readonly DirectProperty<DataGridColumnHeader, bool> IsSeparatorFullHeightProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(IsSeparatorFullHeight),
            o => o.IsSeparatorFullHeight,
            (o, v) => o.IsSeparatorFullHeight = v);

    internal static readonly DirectProperty<DataGridColumnHeader, FrozenColumnShadowPosition> FrozenShadowPositionProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, FrozenColumnShadowPosition>(
            nameof(FrozenShadowPosition),
            o => o.FrozenShadowPosition, 
            (o, v) => o.FrozenShadowPosition = v);
    
    internal static readonly DirectProperty<DataGridColumnHeader, DragMode> HeaderDragModeProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, DragMode>(
            nameof(HeaderDragMode),
            o => o.HeaderDragMode, 
            (o, v) => o.HeaderDragMode = v);
    
    private bool _isFirstVisible = false;
    internal bool IsFirstVisible
    {
        get => _isFirstVisible;
        set
        {
            SetAndRaise(IsFirstVisibleProperty, ref _isFirstVisible, value);
            PseudoClasses.Set(DataGridPseudoClass.FirstColumnHeader, value);
        }
    }
    
    private bool _isLastVisible = false;
    internal bool IsLastVisible
    {
        get => _isLastVisible;
        set
        {
            SetAndRaise(IsLastVisibleProperty, ref _isLastVisible, value);
            PseudoClasses.Set(DataGridPseudoClass.LastColumnHeader, value);
        }
    }
    
    private DragMode _headerDragMode =  DragMode.None;
    internal DragMode HeaderDragMode
    {
        get => _headerDragMode;
        set => SetAndRaise(HeaderDragModeProperty, ref _headerDragMode, value);
    }
    
    private bool _isMiddleVisible = false;
    internal bool IsMiddleVisible
    {
        get => _isMiddleVisible;
        set
        {
            SetAndRaise(IsLastVisibleProperty, ref _isMiddleVisible, value);
            PseudoClasses.Set(DataGridPseudoClass.MiddleColumnHeader, value);
        }
    }

    private bool _isSeparatorFullHeight;
    internal bool IsSeparatorFullHeight
    {
        get => _isSeparatorFullHeight;
        set => SetAndRaise(IsSeparatorFullHeightProperty, ref _isSeparatorFullHeight, value);
    }

    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    private DataGridColumn? _owningColumn;
    internal DataGridColumn? OwningColumn
    {
        get => _owningColumn;
        set
        {
            _owningColumn = value;
            if (_filterIndicator != null)
            {
                _filterIndicator.OwningColumn = OwningColumn;
            }
        }
    }
    
    private bool _indicatorLayoutVisible = true;
    internal bool IndicatorLayoutVisible
    {
        get => _indicatorLayoutVisible;
        set => SetAndRaise(IndicatorLayoutVisibleProperty, ref _indicatorLayoutVisible, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    bool _isFrozen = false;
    internal bool IsFrozen
    {
        get => _isFrozen;
        set => SetAndRaise(IsFrozenProperty, ref _isFrozen, value);
    }
    
    bool _isShowFrozenShadow = false;

    internal bool IsShowFrozenShadow
    {
        get => _isShowFrozenShadow;
        set => SetAndRaise(IsShowFrozenShadowProperty, ref _isShowFrozenShadow, value);
    }
    
    FrozenColumnShadowPosition _frozenShadowPosition = FrozenColumnShadowPosition.Right;

    internal FrozenColumnShadowPosition FrozenShadowPosition
    {
        get => _frozenShadowPosition;
        set => SetAndRaise(FrozenShadowPositionProperty, ref _frozenShadowPosition, value);
    }

    internal DataGrid? OwningGrid => OwningColumn?.OwningGrid;

    internal int ColumnIndex
    {
        get
        {
            if (OwningColumn == null)
            {
                return -1;
            }
            return OwningColumn.Index;
        }
    }

    private bool IsMouseOver { get; set; }

    private bool IsPressed { get; set; }
    
    #endregion

    private const int ResizeRegionWidth = 5;
    
    private static Point? _lastMousePositionHeaders;
    private static Cursor? _originalCursor;
    private static double _originalHorizontalOffset;
    private static double _originalWidth;
    private static Point? _dragStart;
    private static DataGridColumn? _dragColumn;
    private static DataGridColumn? _currentDraggingOverColumn;
    private static double _leftFrozenColumnsWidth;
    private static double _rightFrozenColumnsWidth;
    private bool _areHandlersSuspended;
    private bool _desiredSeparatorVisibility = true;
    private static Lazy<Cursor> ResizeCursor = new (() => new Cursor(StandardCursorType.SizeWestEast));
    
    static DataGridColumnHeader()
    {
        AffectsMeasure<DataGridColumnHeader>(CanUserSortProperty);
        IsSeparatorsVisibleProperty.Changed.AddClassHandler<DataGridColumnHeader>((x, e) => x.HandleIsSeparatorsVisibleChanged(e));
        PressedMixin.Attach<DataGridColumnHeader>();
        IsTabStopProperty.OverrideDefaultValue<DataGridColumnHeader>(false);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridColumnHeader>(IsOffscreenBehavior.FromClip);
    }
    
    public DataGridColumnHeader()
    {
        PointerPressed  += HandlePointerPressed;
        PointerReleased += HandlePointerReleased;
        PointerMoved    += HandlePointerMoved;
        PointerEntered  += HandlePointerEntered;
        PointerExited   += HandlePointerExited;
    }

    // protected override AutomationPeer OnCreateAutomationPeer()
    // {
    //     return new DataGridColumnHeaderAutomationPeer(this);
    // }
    
    internal void UpdateSeparatorVisibility(DataGridColumn? lastVisibleColumn)
    {
        bool newVisibility = _desiredSeparatorVisibility;

        // Collapse separator for the last column if there is no filler column
        if (OwningColumn != null &&
            OwningGrid != null &&
            _desiredSeparatorVisibility &&
            OwningColumn == lastVisibleColumn &&
            OwningGrid.ColumnsInternal.FillerColumn != null &&
            !OwningGrid.ColumnsInternal.FillerColumn.IsActive)
        {
            newVisibility = false;
        }
        // Update the public property if it has changed
        if (IsSeparatorsVisible != newVisibility)
        {
            SetValueNoCallback(IsSeparatorsVisibleProperty, newVisibility);
        }
    }
    
    internal void HandleMouseLeftButtonUpClick(KeyModifiers keyModifiers, ref bool handled)
    {
        LeftClick?.Invoke(this, keyModifiers);
        // completed a click without dragging, so we're sorting
        InvokeProcessSort(keyModifiers);
        handled = true;
    }

    internal void UpdatePseudoClasses()
    {
        if (OwningGrid != null && OwningGrid.DataConnection.AllowSort)
        {
            var sort = OwningColumn?.GetSortDescription();
            if (sort != null)
            {
                CurrentSortingState = sort.Direction;
            }
            else
            {
                CurrentSortingState = null;
            }
        }
        PseudoClasses.Set(StdPseudoClass.SortAscending, CurrentSortingState == ListSortDirection.Ascending);
        PseudoClasses.Set(StdPseudoClass.SortDescending, CurrentSortingState == ListSortDirection.Descending);
    }
    
    //TODO DragDrop

    internal void HandleMouseLeftButtonDown(ref bool handled, PointerEventArgs args, Point mousePosition)
    {
        IsPressed = true;

        if (OwningGrid != null && OwningGrid.ColumnHeaders != null)
        {
            HeaderDragMode            = DragMode.MouseDown;
            _leftFrozenColumnsWidth   = OwningGrid.ColumnsInternal.GetVisibleLeftFrozenEdgedColumnsWidth();
            _rightFrozenColumnsWidth   = OwningGrid.ColumnsInternal.GetVisibleRightFrozenEdgedColumnsWidth();
            _lastMousePositionHeaders = this.Translate(OwningGrid.ColumnHeaders, mousePosition);

            double          distanceFromLeft  = mousePosition.X;
            double          distanceFromRight = Bounds.Width - distanceFromLeft;
            DataGridColumn? currentColumn     = OwningColumn;
            DataGridColumn? previousColumn    = null;
            if (!(OwningColumn is DataGridFillerColumn))
            {
                Debug.Assert(currentColumn != null);
                previousColumn = OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
            }

            if (HeaderDragMode == DragMode.MouseDown && _dragColumn == null && (distanceFromRight <= ResizeRegionWidth))
            {
                Debug.Assert(currentColumn != null);
                handled = TrySetResizeColumn(currentColumn);
            }
            else if (HeaderDragMode == DragMode.MouseDown && _dragColumn == null && distanceFromLeft <= ResizeRegionWidth && previousColumn != null)
            {
                handled = TrySetResizeColumn(previousColumn);
            }

            if (HeaderDragMode == DragMode.Resize && _dragColumn != null)
            {
                _dragStart                = _lastMousePositionHeaders;
                _originalWidth            = _dragColumn.ActualWidth;
                _originalHorizontalOffset = OwningGrid.HorizontalOffset;

                handled = true;
            }
        }
    }

    //TODO DragEvents
    //TODO MouseCapture
    internal void HandleMouseLeftButtonUp(ref bool handled, PointerEventArgs args, Point mousePosition, Point mousePositionHeaders)
    {
        IsPressed = false;
        
        if (OwningGrid != null && OwningGrid.ColumnHeaders != null)
        {
            var horizontalOffset = OwningGrid.HorizontalScrollBar?.Value ?? mousePosition.X;
            if (HeaderDragMode == DragMode.MouseDown)
            {
                HandleMouseLeftButtonUpClick(args.KeyModifiers, ref handled);
            }
            else if (HeaderDragMode == DragMode.Reorder)
            {
                var displayedColumnCount = OwningGrid.ColumnsInternal.GetDisplayedColumnCount();
                // Find header we're hovering over
                int targetIndex = GetReorderingTargetDisplayIndex(mousePositionHeaders);
                
                Debug.Assert(OwningColumn != null);
                if (!OwningColumn.IsFrozen && targetIndex >= OwningGrid.LeftFrozenColumnCount && (targetIndex < displayedColumnCount - OwningGrid.RightFrozenColumnCount))
                {
                    OwningColumn.DisplayIndex = targetIndex;
                    DataGridColumnEventArgs ea = new DataGridColumnEventArgs(OwningColumn);
                    OwningGrid.NotifyColumnReordered(ea);
                    OwningGrid.UpdateHorizontalOffset(horizontalOffset);
                }
            }

            SetDragCursor(mousePosition);

            // Variables that track drag mode states get reset in DataGridColumnHeader_LostMouseCapture
            args.Pointer.Capture(null);
            HandleLostMouseCapture();
            HeaderDragMode = DragMode.None;
            handled        = true;
        }
    }

    //TODO DragEvents
    internal void HandleMouseMove(PointerEventArgs args, Point mousePosition, Point mousePositionHeaders)
    {
        var handled = args.Handled;
        if (handled || OwningGrid == null || OwningGrid.ColumnHeaders == null)
        {
            return;
        }

        Debug.Assert(OwningGrid.Parent is InputElement);

        HandleMouseMoveResize(ref handled, mousePositionHeaders);
        HandleMouseMoveReorder(ref handled, mousePosition, mousePositionHeaders);
        SetDragCursor(mousePosition);
    }
    
    private void HandleIsSeparatorsVisibleChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (!_areHandlersSuspended)
        {
            _desiredSeparatorVisibility = (bool)(change.NewValue ?? false);
            if (OwningGrid != null)
            {
                UpdateSeparatorVisibility(OwningGrid.ColumnsInternal.LastVisibleColumn);
            }
            else
            {
                UpdateSeparatorVisibility(null);
            }
        }
    }
    
    private void SetValueNoCallback<T>(AvaloniaProperty<T> property, T value, BindingPriority priority = BindingPriority.LocalValue)
    {
        _areHandlersSuspended = true;
        try
        {
            SetValue(property, value, priority);
        }
        finally
        {
            _areHandlersSuspended = false;
        }
    }

    private bool CanReorderColumn(DataGridColumn column)
    {
        Debug.Assert(OwningGrid != null);
        return OwningGrid.CanUserReorderColumns
               && !(column is DataGridFillerColumn)
               && !column.IsFrozen
               && column.CanUserReorderInternal.HasValue && column.CanUserReorderInternal.Value;
    }

    /// <summary>
    /// Determines whether a column can be resized by dragging the border of its header.  If star sizing
    /// is being used, there are special conditions that can prevent a column from being resized:
    /// 1. The column is the last visible column.
    /// 2. All columns are constrained by either their maximum or minimum values.
    /// </summary>
    /// <param name="column">Column to check.</param>
    /// <returns>Whether or not the column can be resized by dragging its header.</returns>
    private static bool CanResizeColumn(DataGridColumn column)
    {
        if (column.OwningGrid != null && column.OwningGrid.UsesStarSizing &&
            (column.OwningGrid.ColumnsInternal.LastVisibleColumn == column || !MathUtilities.AreClose(column.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth, column.OwningGrid.CellsWidth)))
        {
            return false;
        }
        return column.ActualCanUserResize;
    }

    private bool TrySetResizeColumn(DataGridColumn column)
    {
        // If datagrid.CanUserResizeColumns == false, then the column can still override it
        if (CanResizeColumn(column))
        {
            _dragColumn = column;

            HeaderDragMode = DragMode.Resize;

            return true;
        }
        return false;
    }

    private void HandlePointerEntered(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        Point mousePosition = e.GetPosition(this);
        HandleMouseEnter(mousePosition);
        UpdatePseudoClasses();
    }

    private void HandlePointerExited(object? sender, PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        HandleMouseLeave();
        UpdatePseudoClasses();
    }

    private void HandlePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (OwningColumn == null || e.Handled || !IsEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        Point mousePosition = e.GetPosition(this);
        bool  handled       = e.Handled;
        HandleMouseLeftButtonDown(ref handled, e, mousePosition);
        e.Handled = handled;

        UpdatePseudoClasses();
    }

    private void HandlePointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (OwningColumn == null || e.Handled || !IsEnabled || e.InitialPressMouseButton != MouseButton.Left)
        {
            return;
        }
        Debug.Assert(OwningGrid != null);
        Point mousePosition        = e.GetPosition(this);
        Point mousePositionHeaders = e.GetPosition(OwningGrid.ColumnHeaders);
        bool  handled              = e.Handled;
        HandleMouseLeftButtonUp(ref handled, e, mousePosition, mousePositionHeaders);
        e.Handled = handled;

        UpdatePseudoClasses();
    }

    private void HandlePointerMoved(object? sender, PointerEventArgs e)
    {
        if (OwningGrid == null || !IsEnabled)
        {
            return;
        }

        Point mousePosition        = e.GetPosition(this);
        Point mousePositionHeaders = e.GetPosition(OwningGrid.ColumnHeaders);

        HandleMouseMove(e, mousePosition, mousePositionHeaders);
    }

    /// <summary>
    /// Returns the column against whose top-left the reordering caret should be positioned
    /// </summary>
    /// <param name="mousePositionHeaders">Mouse position within the ColumnHeadersPresenter</param>
    /// <param name="scroll">Whether or not to scroll horizontally when a column is dragged out of bounds</param>
    /// <param name="scrollAmount">If scroll is true, returns the horizontal amount that was scrolled</param>
    /// <returns></returns>
    private DataGridColumn? GetReorderingTargetColumn(Point mousePositionHeaders, bool scroll, out double scrollAmount)
    {
        scrollAmount = 0;
        Debug.Assert(OwningGrid != null);
        Debug.Assert(OwningColumn != null);
        Debug.Assert(OwningGrid.ColumnsInternal.RowGroupSpacerColumn != null);
        double leftEdge = OwningGrid.ColumnsInternal.RowGroupSpacerColumn.IsRepresented ? OwningGrid.ColumnsInternal.RowGroupSpacerColumn.ActualWidth : 0;
        double rightEdge = OwningGrid.CellsWidth;

        if (OwningGrid.LeftFrozenColumnCount > 0)
        {
            leftEdge = _leftFrozenColumnsWidth;
        }

        if (OwningGrid.RightFrozenColumnCount > 0)
        {
            rightEdge -=  _rightFrozenColumnsWidth;
        }

        if (mousePositionHeaders.X < leftEdge)
        {
            if (scroll &&
                OwningGrid.HorizontalScrollBar != null &&
                OwningGrid.HorizontalScrollBar.IsVisible &&
                OwningGrid.HorizontalScrollBar.Value > 0)
            {
                double newVal = mousePositionHeaders.X - leftEdge;
                scrollAmount = Math.Min(newVal, OwningGrid.HorizontalScrollBar.Value);
                OwningGrid.UpdateHorizontalOffset(scrollAmount + OwningGrid.HorizontalScrollBar.Value);
            }
            mousePositionHeaders = mousePositionHeaders.WithX(leftEdge);
        }
        else if (mousePositionHeaders.X >= rightEdge)
        {
            if (scroll &&
                OwningGrid.HorizontalScrollBar != null &&
                OwningGrid.HorizontalScrollBar.IsVisible &&
                OwningGrid.HorizontalScrollBar.Value < OwningGrid.HorizontalScrollBar.Maximum)
            {
                double newVal = mousePositionHeaders.X - rightEdge;
                scrollAmount = Math.Min(newVal, OwningGrid.HorizontalScrollBar.Maximum - OwningGrid.HorizontalScrollBar.Value);
                OwningGrid.UpdateHorizontalOffset(scrollAmount + OwningGrid.HorizontalScrollBar.Value);
            }
            mousePositionHeaders = mousePositionHeaders.WithX(rightEdge - 1);
        }

        Debug.Assert(OwningGrid.ColumnHeaders != null);
        foreach (DataGridColumn column in OwningGrid.ColumnsInternal.GetVisibleColumns())
        {
            Point  mousePosition = OwningGrid.ColumnHeaders.Translate(column.HeaderCell, mousePositionHeaders);
            if (mousePosition.X >= 0 && mousePosition.X <= column.HeaderCell.Bounds.Width)
            {
                return column;
            }
        }

        return null;
    }

    /// <summary>
    /// Returns the display index to set the column to
    /// </summary>
    /// <param name="mousePositionHeaders">Mouse position relative to the column headers presenter</param>
    /// <returns></returns>
    private int GetReorderingTargetDisplayIndex(Point mousePositionHeaders)
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(OwningColumn != null);
        DataGridColumn? targetColumn = GetReorderingTargetColumn(mousePositionHeaders, false /*scroll*/, out double scrollAmount);
        if (targetColumn != null)
        {
            return targetColumn.DisplayIndex;
        }
        return OwningGrid.Columns.Count - 1;
    }

    /// <summary>
    /// Returns true if the mouse is
    /// - to the left of the element, or within the left half of the element
    /// and
    /// - within the vertical range of the element, or ignoreVertical == true
    /// </summary>
    /// <param name="mousePosition"></param>
    /// <param name="element"></param>
    /// <param name="ignoreVertical"></param>
    /// <returns></returns>
    private bool IsReorderTargeted(Point mousePosition, Control element, bool ignoreVertical)
    {
        Point position = this.Translate(element, mousePosition);

        return (position.X < 0 || (position.X >= 0 && position.X <= element.Bounds.Width / 2))
               && (ignoreVertical || (position.Y >= 0 && position.Y <= element.Bounds.Height));
    }

    /// <summary>
    /// Resets the static DataGridColumnHeader properties when a header loses mouse capture
    /// </summary>
    private void HandleLostMouseCapture()
    {
        // When we stop interacting with the column headers, we need to reset the drag mode
        // and close any popups if they are open.

        if (_dragColumn != null)
        {
            _dragColumn.HeaderCell.Cursor = _originalCursor;
        }
        HeaderDragMode            = DragMode.None;
        _dragColumn               = null;
        _dragStart                = null;
        _lastMousePositionHeaders = null;

        _currentDraggingOverColumn = null;
        DataGridColumnDraggingOverEventArgs draggingOverEventArgs =
            new DataGridColumnDraggingOverEventArgs(null, null);
        OwningGrid?.NotifyColumnDraggingOver(draggingOverEventArgs);

        if (OwningGrid != null && OwningGrid.ColumnHeaders != null)
        {
            OwningGrid.ColumnHeaders.DragColumn            = null;
            OwningGrid.ColumnHeaders.DragIndicator         = null;
        }
    }

    /// <summary>
    /// Sets up the DataGridColumnHeader for the MouseEnter event
    /// </summary>
    /// <param name="mousePosition">mouse position relative to the DataGridColumnHeader</param>
    private void HandleMouseEnter(Point mousePosition)
    {
        IsMouseOver = true;
        SetDragCursor(mousePosition);
    }

    /// <summary>
    /// Sets up the DataGridColumnHeader for the MouseLeave event
    /// </summary>
    private void HandleMouseLeave()
    {
        IsMouseOver = false;
    }

    private void HandleMouseMoveBeginReorder(Point mousePosition)
    {
        Debug.Assert(OwningGrid != null);
        Debug.Assert(OwningColumn != null);
        var dragIndicator = new DataGridColumnHeader
        {
            OwningColumn        = OwningColumn,
            IsEnabled           = false,
            Content             = GetDragIndicatorContent(Content, ContentTemplate),
            IsSeparatorsVisible = false,
            IsVisible = false
        };
        dragIndicator.PseudoClasses.Add(StdPseudoClass.DragIndicator);
        
        // pass the caret's data template to the user for modification
        DataGridColumnReorderingEventArgs columnReorderingEventArgs = new DataGridColumnReorderingEventArgs(OwningColumn)
        {
            DragIndicator         = dragIndicator
        };
        
        OwningGrid.NotifyColumnReordering(columnReorderingEventArgs);
        if (columnReorderingEventArgs.Cancel)
        {
            return;
        }

        // The user didn't cancel, so prepare for the reorder
        _dragColumn    = OwningColumn;
        HeaderDragMode = DragMode.Reorder;
        _dragStart     = mousePosition;

        Debug.Assert(OwningGrid.ColumnHeaders != null);
        // Display the reordering thumb
        OwningGrid.ColumnHeaders.DragColumn            = OwningColumn;
        OwningGrid.ColumnHeaders.DragIndicator         = columnReorderingEventArgs.DragIndicator;

        // If the user didn't style the dragIndicator's Width, default it to the column header's width
        if (double.IsNaN(dragIndicator.Width))
        {
            dragIndicator.Width = DesiredSize.Width;
        }

        if (double.IsNaN(dragIndicator.Height))
        {
            dragIndicator.Height = DesiredSize.Height;
        }
    }

    private object? GetDragIndicatorContent(object? content, IDataTemplate? dataTemplate)
    {
        if (content is ContentControl icc)
        {
            content = icc.Content;
        }

        if (content is Control control)
        {
            if (VisualRoot == null)
            {
                return content;
            }
            control.Measure(Size.Infinity);
            var rect = new Rectangle()
            {
                Width  = control.DesiredSize.Width,
                Height = control.DesiredSize.Height,
                Fill = new VisualBrush
                {
                    Visual = control,
                    Stretch = Stretch.Fill
                }
            };
            return rect;
        }

        if (dataTemplate is not null)
        {
            return dataTemplate.Build(content);
        }
        return content;
    }
    
    //TODO DragEvents
    private void HandleMouseMoveReorder(ref bool handled, Point mousePosition, Point mousePositionHeaders)
    {
        if (handled)
        {
            return;
        }
        Debug.Assert(OwningGrid != null);
        Debug.Assert(OwningColumn != null);
        Debug.Assert(OwningGrid.ColumnHeaders != null);
        //handle entry into reorder mode
        if (HeaderDragMode == DragMode.MouseDown && _dragColumn == null && _lastMousePositionHeaders != null)
        {
            var distanceFromInitial = (Vector)(mousePositionHeaders - _lastMousePositionHeaders);
            if (distanceFromInitial.Length > Constants.DragThreshold)
            {
                handled = CanReorderColumn(OwningColumn);

                if (handled)
                {
                    HandleMouseMoveBeginReorder(mousePosition);
                }
            }
        }

        //handle reorder mode (eg, positioning of the popup)
        if (HeaderDragMode == DragMode.Reorder && OwningGrid.ColumnHeaders.DragIndicator != null)
        {
            Debug.Assert(_dragStart != null);
            // Find header we're hovering over
            
            DataGridColumn? targetColumn = GetReorderingTargetColumn(mousePositionHeaders, !OwningColumn.IsFrozen /*scroll*/, out double scrollAmount);

            if (_currentDraggingOverColumn != targetColumn && (targetColumn != null && !targetColumn.IsFrozen) || targetColumn == null)
            {
                DataGridColumnDraggingOverEventArgs draggingOverEventArgs =
                    new DataGridColumnDraggingOverEventArgs(_dragColumn, targetColumn);
                OwningGrid.NotifyColumnDraggingOver(draggingOverEventArgs);
                _currentDraggingOverColumn =  targetColumn;
            }
            
            OwningGrid.ColumnHeaders.DragIndicatorOffset = mousePosition.X - _dragStart.Value.X + scrollAmount;
            OwningGrid.ColumnHeaders.InvalidateArrange();
            
            handled = true;
        }
    }

    private void HandleMouseMoveResize(ref bool handled, Point mousePositionHeaders)
    {
        if (handled)
        {
            return;
        }
        Debug.Assert(OwningGrid != null);
        if (HeaderDragMode == DragMode.Resize && _dragColumn != null && _dragStart.HasValue)
        {
            // resize column
            double mouseDelta   = Math.Round(mousePositionHeaders.X - _dragStart.Value.X);
            double desiredWidth = _originalWidth + mouseDelta;

            desiredWidth = Math.Max(_dragColumn.ActualMinWidth, Math.Min(_dragColumn.ActualMaxWidth, desiredWidth));
            _dragColumn.Resize(_dragColumn.Width,
                new(_dragColumn.Width.Value, _dragColumn.Width.UnitType, _dragColumn.Width.DesiredValue, desiredWidth),
                true);

            OwningGrid.UpdateHorizontalOffset(_originalHorizontalOffset);

            handled = true;
        }
    }

    private void SetDragCursor(Point mousePosition)
    {
        if (HeaderDragMode != DragMode.None || OwningGrid == null || OwningColumn == null)
        {
            return;
        }

        // set mouse if we can resize column
        double          distanceFromLeft  = mousePosition.X;
        double          distanceFromRight = Bounds.Width - distanceFromLeft;
        DataGridColumn? currentColumn     = OwningColumn;
        DataGridColumn? previousColumn    = null;

        if (!(OwningColumn is DataGridFillerColumn))
        {
            previousColumn = OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
        }

        if ((distanceFromRight <= ResizeRegionWidth && currentColumn != null && CanResizeColumn(currentColumn)) ||
            (distanceFromLeft <= ResizeRegionWidth && previousColumn != null && CanResizeColumn(previousColumn)))
        {
            var resizeCursor = ResizeCursor.Value;
            if (Cursor != resizeCursor)
            {
                _originalCursor = Cursor;
                Cursor          = resizeCursor;
            }
        }
        else
        {
            if (_originalCursor != null)
            {
                Cursor = _originalCursor;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _filterIndicator = e.NameScope.Find<DataGridFilterIndicator>(DataGridColumnHeaderThemeConstants.FilterIndicatorPart);
        if (_filterIndicator != null && OwningColumn != null)
        {
            _filterIndicator.OwningColumn = OwningColumn;
        }
        
        ConfigureFilterIndicator();
        ConfigureIndicatorLayoutVisible();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (IsLoaded)
        {
            if (change.Property == IsMotionEnabledProperty)
            {
                ConfigureTransitions(true);
            }
        }

        if (change.Property == CanUserSortProperty ||
            change.Property == IsSeparatorsVisibleProperty ||
            change.Property == CanUserFilterProperty)
        {
            ConfigureIndicatorLayoutVisible();
        }
        NotifyPropertyChangedForSorting(change);
    }

    private void ConfigureIndicatorLayoutVisible()
    {
        SetCurrentValue(IndicatorLayoutVisibleProperty, CanUserSort || (IsSeparatorsVisible && CanUserFilter && OwningColumn?.Filters.Count > 0));
    }
    
    private void ConfigureTransitions(bool force)
    {
        if (IsMotionEnabled)
        {
            if (force || Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(BackgroundProperty)
                ];
            }
        }
        else
        {
            Transitions = null;
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        ConfigureTransitions(false);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        Transitions = null;
    }
}
