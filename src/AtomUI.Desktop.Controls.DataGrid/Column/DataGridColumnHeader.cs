// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Collections.Specialized;
using System.Diagnostics;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Utils;
using AtomUI.Generated.AtomUIDesktopControlsDataGrid;
using AtomUI.Utils;
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
using Avalonia.Media;

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
    
    internal static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridColumnHeader>();
    
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

    internal static readonly DirectProperty<DataGridColumnHeader, bool> FilterIndicatorVisibleProperty =
        AvaloniaProperty.RegisterDirect<DataGridColumnHeader, bool>(
            nameof(FilterIndicatorVisible),
            o => o.FilterIndicatorVisible,
            (o, v) => o.FilterIndicatorVisible = v);
    
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridColumnHeader>();

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<DataGridColumnHeader>();
    
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
            SetAndRaise(IsMiddleVisibleProperty, ref _isMiddleVisible, value);
            PseudoClasses.Set(DataGridPseudoClass.MiddleColumnHeader, value);
        }
    }

    private bool _isSeparatorFullHeight;
    internal bool IsSeparatorFullHeight
    {
        get => _isSeparatorFullHeight;
        set => SetAndRaise(IsSeparatorFullHeightProperty, ref _isSeparatorFullHeight, value);
    }

    internal CustomizableSizeType SizeType
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
            if (ReferenceEquals(_owningColumn, value))
            {
                return;
            }

            CloseFilterPopupForLifecycle();
            UnregisterFilterItems();
            _owningColumn = value;
            RegisterFilterItems(value);
            if (_filterIndicator != null)
            {
                _filterIndicator.OwningColumn = OwningColumn;
            }
            ConfigureIndicatorLayoutVisible();
        }
    }
    
    private bool _indicatorLayoutVisible = true;
    internal bool IndicatorLayoutVisible
    {
        get => _indicatorLayoutVisible;
        set => SetAndRaise(IndicatorLayoutVisibleProperty, ref _indicatorLayoutVisible, value);
    }

    private bool _filterIndicatorVisible;
    internal bool FilterIndicatorVisible
    {
        get => _filterIndicatorVisible;
        set => SetAndRaise(FilterIndicatorVisibleProperty, ref _filterIndicatorVisible, value);
    }
    
    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
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
    
    private static Point? s_lastMousePositionHeaders;
    private static Cursor? s_originalCursor;
    private static double s_originalHorizontalOffset;
    private static double s_originalWidth;
    private static Point? s_dragStart;
    private static DataGridColumn? s_dragColumn;
    private static DataGridColumn? s_currentDraggingOverColumn;
    private static DataGridColumnHeader? s_dragOwner;
    private static double s_leftFrozenColumnsWidth;
    private static double s_rightFrozenColumnsWidth;
    private bool _areHandlersSuspended;
    private bool _desiredSeparatorVisibility = true;
    private IDisposable? _popupPinnedOpenRelay;
    private INotifyCollectionChanged? _subscribedFilterItems;
    private RectangleGeometry? _clipGeometry;
    private static Lazy<Cursor> s_resizeCursor = new (() => new Cursor(StandardCursorType.SizeWestEast));
    
    static DataGridColumnHeader()
    {
        AffectsMeasure<DataGridColumnHeader>(CanUserSortProperty);
        IsSeparatorsVisibleProperty.Changed.AddClassHandler<DataGridColumnHeader>((x, e) => x.HandleIsSeparatorsVisibleChanged(e));
        PressedMixin.Attach<DataGridColumnHeader>();
        PointerPressedEvent.AddClassHandler<DataGridColumnHeader>((x, e) => x.HandlePointerPressed(e));
        PointerPressedEvent.AddClassHandler<DataGridColumnHeader>((x, e) => x.NotifyHeaderPointerPressed(e));
        PointerReleasedEvent.AddClassHandler<DataGridColumnHeader>((x, e) => x.HandlePointerReleased(e));
        PointerReleasedEvent.AddClassHandler<DataGridColumnHeader>((x, e) => x.NotifyHeaderPointerReleased(e));
        PointerMovedEvent.AddClassHandler<DataGridColumnHeader>((x, e) => x.HandlePointerMoved(e));
        IsTabStopProperty.OverrideDefaultValue<DataGridColumnHeader>(false);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridColumnHeader>(IsOffscreenBehavior.FromClip);
    }

    public DataGridColumnHeader()
    {
        Classes.Add(DataGridSemanticParts.HeaderCellClass);
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

    internal void UpdateClipGeometry(Rect clipRect)
    {
        if (_clipGeometry is null)
        {
            _clipGeometry = new RectangleGeometry
            {
                Rect = clipRect
            };
        }
        else if (!_clipGeometry.Rect.Equals(clipRect))
        {
            _clipGeometry.Rect = clipRect;
            InvalidateVisual();
        }

        if (!ReferenceEquals(Clip, _clipGeometry))
        {
            Clip = _clipGeometry;
        }
    }

    internal void ClearClipGeometry()
    {
        if (Clip is not null)
        {
            Clip = null;
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
        if (OwningColumn is not null)
        {
            CurrentSortingState = OwningColumn.SortState.Direction switch
            {
                DataGridSortDirection.Ascending => ListSortDirection.Ascending,
                DataGridSortDirection.Descending => ListSortDirection.Descending,
                _ => null
            };
        }
        PseudoClasses.Set(StdPseudoClass.SortAscending, CurrentSortingState == ListSortDirection.Ascending);
        PseudoClasses.Set(StdPseudoClass.SortDescending, CurrentSortingState == ListSortDirection.Descending);
        if (OwningColumn is not null)
        {
            OwningGrid?.UpdateRealizedSortState(OwningColumn, CurrentSortingState is not null);
        }
    }
    
    //TODO DragDrop

    internal void HandleMouseLeftButtonDown(ref bool handled, PointerEventArgs args, Point mousePosition)
    {
        IsPressed = true;

        if (OwningGrid != null && OwningGrid.ColumnHeaders != null)
        {
            HeaderDragMode            = DragMode.MouseDown;
            s_dragOwner                = this;
            s_leftFrozenColumnsWidth   = OwningGrid.ColumnsInternal.GetVisibleLeftFrozenEdgedColumnsWidth();
            s_rightFrozenColumnsWidth   = OwningGrid.ColumnsInternal.GetVisibleRightFrozenEdgedColumnsWidth();
            s_lastMousePositionHeaders = this.Translate(OwningGrid.ColumnHeaders, mousePosition);

            double          distanceFromLeft  = mousePosition.X;
            double          distanceFromRight = Bounds.Width - distanceFromLeft;
            DataGridColumn? currentColumn     = OwningColumn;
            DataGridColumn? previousColumn    = null;
            if (!(OwningColumn is DataGridFillerColumn))
            {
                Debug.Assert(currentColumn != null);
                previousColumn = OwningGrid.ColumnsInternal.GetPreviousVisibleNonFillerColumn(currentColumn);
            }

            if (HeaderDragMode == DragMode.MouseDown && s_dragColumn == null && (distanceFromRight <= ResizeRegionWidth))
            {
                Debug.Assert(currentColumn != null);
                handled = TrySetResizeColumn(currentColumn);
            }
            else if (HeaderDragMode == DragMode.MouseDown && s_dragColumn == null && distanceFromLeft <= ResizeRegionWidth && previousColumn != null)
            {
                handled = TrySetResizeColumn(previousColumn);
            }

            if (HeaderDragMode == DragMode.Resize && s_dragColumn != null)
            {
                s_dragStart                = s_lastMousePositionHeaders;
                s_originalWidth            = s_dragColumn.ActualWidth;
                s_originalHorizontalOffset = OwningGrid.HorizontalOffset;

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
            (column.OwningGrid.ColumnsInternal.LastVisibleColumn == column || !MathUtils.AreClose(column.OwningGrid.ColumnsInternal.VisibleEdgedColumnsWidth, column.OwningGrid.CellsWidth)))
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
            s_dragColumn = column;
            s_dragOwner  = this;

            HeaderDragMode = DragMode.Resize;

            return true;
        }
        return false;
    }

    private void HandlePointerEntered(PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        Point mousePosition = e.GetPosition(this);
        HandleMouseEnter(mousePosition);
        UpdatePseudoClasses();
    }

    private void HandlePointerExited(PointerEventArgs e)
    {
        if (!IsEnabled)
        {
            return;
        }

        HandleMouseLeave();
        UpdatePseudoClasses();
    }

    private void HandlePointerPressed(PointerPressedEventArgs e)
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

    private void NotifyHeaderPointerPressed(PointerPressedEventArgs e)
    {
        OwningColumn?.NotifyHeaderPointerPressed(e);
    }

    private void HandlePointerReleased(PointerReleasedEventArgs e)
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

    private void NotifyHeaderPointerReleased(PointerReleasedEventArgs e)
    {
        OwningColumn?.NotifyHeaderPointerReleased(e);
    }

    private void HandlePointerMoved(PointerEventArgs e)
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
            leftEdge = s_leftFrozenColumnsWidth;
        }

        if (OwningGrid.RightFrozenColumnCount > 0)
        {
            rightEdge -=  s_rightFrozenColumnsWidth;
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
        var columns = OwningGrid.ColumnsInternal;
        int displayedColumnCount = columns.GetDisplayedColumnCount();
        for (int displayIndex = 0; displayIndex < displayedColumnCount; displayIndex++)
        {
            DataGridColumn column = columns.GetDisplayedColumnAtDisplayIndex(displayIndex);
            if (!column.IsVisible)
            {
                continue;
            }

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
        ReleaseDragState(
            notifyDraggingOverCleared: HeaderDragMode == DragMode.Reorder || s_currentDraggingOverColumn != null,
            removeDragIndicator: true);
    }

    private void ReleaseDragStateIfOwned(bool notifyDraggingOverCleared, bool removeDragIndicator)
    {
        if (ReferenceEquals(s_dragOwner, this))
        {
            ReleaseDragState(notifyDraggingOverCleared, removeDragIndicator);
        }
    }

    private void ReleaseDragState(bool notifyDraggingOverCleared, bool removeDragIndicator)
    {
        if (s_dragColumn != null)
        {
            s_dragColumn.HeaderCell.Cursor = s_originalCursor;
        }
        HeaderDragMode            = DragMode.None;
        s_dragColumn               = null;
        s_dragStart                = null;
        s_dragOwner                = null;
        s_lastMousePositionHeaders = null;
        s_originalCursor           = null;
        s_originalHorizontalOffset = 0;
        s_originalWidth            = 0;
        s_leftFrozenColumnsWidth   = 0;
        s_rightFrozenColumnsWidth  = 0;
        s_currentDraggingOverColumn = null;
        if (notifyDraggingOverCleared)
        {
            DataGridColumnDraggingOverEventArgs draggingOverEventArgs =
                new DataGridColumnDraggingOverEventArgs(null, null);
            OwningGrid?.NotifyColumnDraggingOver(draggingOverEventArgs);
        }

        if (OwningGrid != null && OwningGrid.ColumnHeaders != null)
        {
            OwningGrid.ColumnHeaders.DragColumn = null;
            if (removeDragIndicator)
            {
                OwningGrid.ColumnHeaders.DragIndicator = null;
            }
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
        // 拖拽 ghost 只是列头预览，不属于表头单元格语义部件。
        dragIndicator.Classes.Remove(DataGridSemanticParts.HeaderCellClass);
        
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
        s_dragColumn    = OwningColumn;
        HeaderDragMode = DragMode.Reorder;
        s_dragStart     = mousePosition;
        s_dragOwner     = this;

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
            if (TopLevel.GetTopLevel(this) == null)
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
        if (HeaderDragMode == DragMode.MouseDown && s_dragColumn == null && s_lastMousePositionHeaders != null)
        {
            var distanceFromInitial = (Vector)(mousePositionHeaders - s_lastMousePositionHeaders);
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
            Debug.Assert(s_dragStart != null);
            // Find header we're hovering over
            
            DataGridColumn? targetColumn = GetReorderingTargetColumn(mousePositionHeaders, !OwningColumn.IsFrozen /*scroll*/, out double scrollAmount);

            if (s_currentDraggingOverColumn != targetColumn &&
                (targetColumn == null || !targetColumn.IsFrozen))
            {
                DataGridColumnDraggingOverEventArgs draggingOverEventArgs =
                    new DataGridColumnDraggingOverEventArgs(s_dragColumn, targetColumn);
                OwningGrid.NotifyColumnDraggingOver(draggingOverEventArgs);
                s_currentDraggingOverColumn =  targetColumn;
            }
            
            OwningGrid.ColumnHeaders.DragIndicatorOffset = mousePosition.X - s_dragStart.Value.X + scrollAmount;
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
        if (HeaderDragMode == DragMode.Resize && s_dragColumn != null && s_dragStart.HasValue)
        {
            // resize column
            double mouseDelta   = Math.Round(mousePositionHeaders.X - s_dragStart.Value.X);
            double desiredWidth = s_originalWidth + mouseDelta;

            desiredWidth = Math.Max(s_dragColumn.ActualMinWidth, Math.Min(s_dragColumn.ActualMaxWidth, desiredWidth));
            s_dragColumn.Resize(s_dragColumn.Width,
                new(s_dragColumn.Width.Value, s_dragColumn.Width.UnitType, s_dragColumn.Width.DesiredValue, desiredWidth),
                true);

            OwningGrid.UpdateHorizontalOffset(s_originalHorizontalOffset);

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
            var resizeCursor = s_resizeCursor.Value;
            if (Cursor != resizeCursor)
            {
                s_originalCursor = Cursor;
                Cursor          = resizeCursor;
            }
        }
        else
        {
            if (s_originalCursor != null)
            {
                Cursor = s_originalCursor;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ClearFilterIndicator();
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
        if (change.Property == CanUserSortProperty ||
            change.Property == IsSeparatorsVisibleProperty ||
            change.Property == CanUserFilterProperty)
        {
            ConfigureIndicatorLayoutVisible();
            _filterIndicator?.RefreshFilterFlyoutState();
        }
        NotifyPropertyChangedForSorting(change);
    }

    private void ConfigureIndicatorLayoutVisible()
    {
        ConfigureFilterIndicatorVisible();
        IndicatorLayoutVisible = CanUserSort || FilterIndicatorVisible;
    }

    private void ConfigureFilterIndicatorVisible()
    {
        FilterIndicatorVisible = IsSeparatorsVisible &&
                                 CanUserFilter &&
                                 OwningColumn?.HasFilterItems == true;
    }

    private void RegisterFilterItems(DataGridColumn? column)
    {
        if (ReferenceEquals(_subscribedFilterItems, column?.Filters))
        {
            return;
        }

        UnregisterFilterItems();
        if (column?.Filters is INotifyCollectionChanged filters)
        {
            _subscribedFilterItems = filters;
            _subscribedFilterItems.CollectionChanged += HandleFilterItemsChanged;
        }
    }

    private void UnregisterFilterItems()
    {
        if (_subscribedFilterItems != null)
        {
            _subscribedFilterItems.CollectionChanged -= HandleFilterItemsChanged;
            _subscribedFilterItems = null;
        }
    }

    private void HandleFilterItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        ConfigureIndicatorLayoutVisible();
        _filterIndicator?.RefreshFilterFlyoutState();
    }

    internal void NotifyFilterItemsChanged()
    {
        RegisterFilterItems(OwningColumn);
        ConfigureIndicatorLayoutVisible();
        _filterIndicator?.RefreshFilterFlyoutState();
    }

    internal void NotifySelectedFilterValuesChanged()
    {
        _filterIndicator?.RefreshSelectedFilterValues();
    }

    private void ClearFilterIndicator()
    {
        if (_filterIndicator != null)
        {
            _filterIndicator.ClosePopupForLifecycle();
            _popupPinnedOpenRelay?.Dispose();
            _popupPinnedOpenRelay = null;
            _filterIndicator.SetCurrentValue(DataGridFilterIndicator.IsPopupPinnedOpenProperty, false);
            _filterIndicator.FilterRequest -= HandleFilterRequest;
            _filterIndicator.OwningColumn = null;
            _filterIndicator = null;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        RegisterFilterItems(OwningColumn);
        ConfigureIndicatorLayoutVisible();
        if (_filterIndicator != null)
        {
            _filterIndicator.OwningColumn = OwningColumn;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        CloseFilterPopupForLifecycle();
        base.OnDetachedFromVisualTree(e);
        var removeDragIndicator = e.AttachmentPoint != null;
        ReleaseDragStateIfOwned(
            notifyDraggingOverCleared: removeDragIndicator,
            removeDragIndicator: removeDragIndicator);
        UnregisterFilterItems();
    }

}
