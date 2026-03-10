// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

[TemplatePart(DataGridCellThemeConstants.RightGridLinePart, typeof(Rectangle))]
[PseudoClasses(StdPseudoClass.Selected, StdPseudoClass.Current, StdPseudoClass.Edited, StdPseudoClass.Invalid,
    StdPseudoClass.Focus)]
public class DataGridCell : ContentControl
{
    #region 公共属性定义

    public static readonly DirectProperty<DataGridCell, bool> IsValidProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
            nameof(IsValid),
            o => o.IsValid);

    bool _isValid = true;

    public bool IsValid
    {
        get => _isValid;
        internal set => SetAndRaise(IsValidProperty, ref _isValid, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<DataGridCell>();

    internal static readonly DirectProperty<DataGridCell, bool> IsSortingProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
            nameof(IsSorting),
            o => o.IsSorting, 
            (o, v) => o.IsSorting = v);
    
    internal static readonly DirectProperty<DataGridCell, bool> OwningColumnDraggingProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
            nameof(OwningColumnDragging),
            o => o.OwningColumnDragging, 
            (o, v) => o.OwningColumnDragging = v);

    internal static readonly DirectProperty<DataGridCell, bool> IsFrozenProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
            nameof(IsFrozen),
            o => o.IsFrozen, 
            (o, v) => o.IsFrozen = v);
    
    internal static readonly DirectProperty<DataGridCell, bool> IsShowFrozenShadowProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
            nameof(IsShowFrozenShadow),
            o => o.IsShowFrozenShadow, 
            (o, v) => o.IsShowFrozenShadow = v);
    
    internal static readonly DirectProperty<DataGridCell, FrozenColumnShadowPosition> FrozenShadowPositionProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, FrozenColumnShadowPosition>(
            nameof(FrozenShadowPosition),
            o => o.FrozenShadowPosition, 
            (o, v) => o.FrozenShadowPosition = v);
    
    internal static readonly DirectProperty<DataGridCell, bool> IsClipContentProperty =
        AvaloniaProperty.RegisterDirect<DataGridCell, bool>(
            nameof(IsClipContent),
            o => o.IsClipContent, 
            (o, v) => o.IsClipContent = v);
    
    internal SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    bool _isSorting = false;

    public bool IsSorting
    {
        get => _isSorting;
        internal set => SetAndRaise(IsSortingProperty, ref _isSorting, value);
    }
    
    bool _owningColumnDragging = false;

    public bool OwningColumnDragging
    {
        get => _owningColumnDragging;
        internal set => SetAndRaise(OwningColumnDraggingProperty, ref _owningColumnDragging, value);
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
    
    bool _isClipContent = true;

    internal bool IsClipContent
    {
        get => _isClipContent;
        set => SetAndRaise(IsClipContentProperty, ref _isClipContent, value);
    }
    
    FrozenColumnShadowPosition _frozenShadowPosition = FrozenColumnShadowPosition.Right;

    internal FrozenColumnShadowPosition FrozenShadowPosition
    {
        get => _frozenShadowPosition;
        set => SetAndRaise(FrozenShadowPositionProperty, ref _frozenShadowPosition, value);
    }
    
    internal DataGridRow? OwningRow { get; set; }

    private DataGridColumn? _owningColumn;

    internal DataGridColumn? OwningColumn
    {
        get => _owningColumn;
        set
        {
            if (_owningColumn != value)
            {
                _owningColumn = value;
                OnOwningColumnSet(value);
            }
        }
    }

    internal DataGrid? OwningGrid => OwningRow?.OwningGrid ?? OwningColumn?.OwningGrid;

    internal double ActualRightGridLineWidth => _rightGridLine?.Bounds.Width ?? 0;

    internal int ColumnIndex => OwningColumn?.Index ?? -1;
    internal int RowIndex => OwningRow?.Index ?? -1;

    internal bool IsCurrent => OwningGrid != null &&
                               OwningColumn != null &&
                               OwningRow != null &&
                               OwningGrid.CurrentColumnIndex == OwningColumn.Index &&
                               OwningGrid.CurrentSlot == OwningRow.Slot;

    private bool IsEdited => OwningGrid != null &&
                             OwningGrid.EditingRow == OwningRow &&
                             OwningGrid.EditingColumnIndex == ColumnIndex;

    private bool IsMouseOver
    {
        get => OwningRow != null && OwningRow.MouseOverColumnIndex == ColumnIndex;
        set
        {
            Debug.Assert(OwningRow != null);
            if (value != IsMouseOver)
            {
                if (value)
                {
                    OwningRow.MouseOverColumnIndex = ColumnIndex;
                }
                else
                {
                    OwningRow.MouseOverColumnIndex = null;
                }
            }
        }
    }

    #endregion

    private Rectangle? _rightGridLine;
    private CompositeDisposable? _bindingDisposables;

    static DataGridCell()
    {
        PointerPressedEvent.AddClassHandler<DataGridCell>(
            (x, e) => x.HandlePointerPressed(e), handledEventsToo: true);
        FocusableProperty.OverrideDefaultValue<DataGridCell>(true);
        IsTabStopProperty.OverrideDefaultValue<DataGridCell>(false);
        AutomationProperties.IsOffscreenBehaviorProperty.OverrideDefaultValue<DataGridCell>(
            IsOffscreenBehavior.FromClip);
        AffectsRender<DataGridCell>(IsShowFrozenShadowProperty);
    }

    /// <summary>
    /// Builds the visual tree for the cell control when a new template is applied.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        UpdatePseudoClasses();
        _rightGridLine = e.NameScope.Find<Rectangle>(DataGridCellThemeConstants.RightGridLinePart);
        if (_rightGridLine != null && OwningColumn == null)
        {
            // Turn off the right GridLine for filler cells
            _rightGridLine.IsVisible = false;
        }
        else
        {
            EnsureGridLine(null);
        }
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (OwningRow != null)
        {
            IsMouseOver = true;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        if (OwningRow != null)
        {
            IsMouseOver = false;
        }
    }

    //TODO TabStop
    private void HandlePointerPressed(PointerPressedEventArgs e)
    {
        // OwningGrid is null for TopLeftHeaderCell and TopRightHeaderCell because they have no OwningRow
        if (OwningGrid == null || OwningColumn == null || OwningRow == null)
        {
            return;
        }

        var handledByChild = e.Handled;
        OwningGrid.NotifyCellPointerPressed(new DataGridCellPointerPressedEventArgs(this, OwningRow, OwningColumn, e));

        // If a child control already handled this click (for example, a ToggleButton),
        // let the child complete its own interaction without DataGrid overriding focus/state.
        if (handledByChild)
        {
            return;
        }

        // If handlers on DataGrid canceled selection explicitly, do not continue.
        if (e.Handled)
        {
            return;
        }

        // Check if the click originated from a focusable child control (e.g., AutoComplete
        // whose inner TextBox explicitly resets e.Handled to false). In that case, we should
        // not steal focus from the child, but still update DataGrid row/cell state.
        var focusableChildClicked = HasFocusableChildAtSource(e.Source as Visual);

        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            if (!focusableChildClicked && OwningGrid.IsTabStop)
            {
                OwningGrid.Focus();
            }

            if (OwningRow != null)
            {
                var handled = false;
                if (OwningColumn != null && OwningColumn.IsEditable())
                {
                    // When a focusable child was clicked, disable edit mode transitions
                    // to prevent DataGrid's focus management from interfering with
                    // the natural focus transfer between child controls.
                    var allowEdit = !handledByChild && !focusableChildClicked;
                    handled = OwningGrid.UpdateStateOnMouseLeftButtonDown(e, ColumnIndex, OwningRow.Slot, allowEdit);
                }

                // Do not handle PointerPressed with touch or pen,
                // so we can start scroll gesture on the same event.
                // When a focusable child was clicked, do not mark the event as handled
                // to allow Avalonia's default focus management to work correctly,
                // ensuring proper :focus-within pseudo-class transitions.
                if (e.Pointer.Type != PointerType.Touch && e.Pointer.Type != PointerType.Pen
                    && !focusableChildClicked)
                {
                    e.Handled = e.Handled || handled;
                }
            }
        }
        else if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
        {
            if (!focusableChildClicked && OwningGrid.IsTabStop)
            {
                OwningGrid.Focus();
            }

            if (OwningRow != null)
            {
                var handled = OwningGrid.UpdateStateOnMouseRightButtonDown(e, ColumnIndex, OwningRow.Slot, !handledByChild);
                if (!focusableChildClicked)
                {
                    e.Handled = e.Handled || handled;
                }
            }
        }
    }

    /// <summary>
    /// Walks up the visual tree from the event source to this cell, checking if any ancestor
    /// is a focusable input element. This handles cases where child controls (e.g., AutoComplete)
    /// don't mark PointerPressed as handled but still need to retain keyboard focus.
    /// </summary>
    private bool HasFocusableChildAtSource(Visual? source)
    {
        while (source != null && source != this)
        {
            if (source is InputElement { Focusable: true, IsEffectivelyEnabled: true })
            {
                return true;
            }
            source = source.GetVisualParent();
        }
        return false;
    }

    internal void UpdatePseudoClasses()
    {
        if (OwningGrid == null || OwningColumn == null || OwningRow == null || !OwningRow.IsVisible ||
            OwningRow.Slot == -1)
        {
            return;
        }

        PseudoClasses.Set(StdPseudoClass.Selected, OwningRow.IsSelected);
        PseudoClasses.Set(StdPseudoClass.Current, IsCurrent);
        PseudoClasses.Set(StdPseudoClass.Edited, IsEdited);
        PseudoClasses.Set(StdPseudoClass.Invalid, !IsValid);
        PseudoClasses.Set(StdPseudoClass.Focus, OwningGrid.IsFocused && IsCurrent);
    }

    // Makes sure the right gridline has the proper stroke and visibility. If lastVisibleColumn is specified, the 
    // right gridline will be collapsed if this cell belongs to the lastVisibleColumn and there is no filler column
    internal void EnsureGridLine(DataGridColumn? lastVisibleColumn)
    {
        if (OwningGrid != null && _rightGridLine != null)
        {
            bool newVisibility =
                OwningGrid.AreVerticalGridLinesVisible
                && ((OwningGrid.ColumnsInternal.FillerColumn != null &&
                     OwningGrid.ColumnsInternal.FillerColumn.IsActive) || OwningColumn != lastVisibleColumn);

            if (newVisibility != _rightGridLine.IsVisible)
            {
                _rightGridLine.IsVisible = newVisibility;
            }

            _rightGridLine.Width = OwningGrid.BorderThickness.Left;
        }
    }

    private void OnOwningColumnSet(DataGridColumn? column)
    {
        if (column == null)
        {
            Classes.Clear();
            ClearValue(ThemeProperty);
        }
        else
        {
            if (Theme != column.CellTheme)
            {
                Theme = column.CellTheme;
            }

            Classes.Replace(column.CellStyleClasses);
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // 可能会影响性能
        if (OwningGrid != null && OwningColumn != null && OwningGrid.DataConnection.AllowSort)
        {
            _bindingDisposables?.Dispose();
            _bindingDisposables = new CompositeDisposable(2);
            _bindingDisposables.Add(BindUtils.RelayBind(OwningColumn.HeaderCell,
                DataGridColumnHeader.CurrentSortingStateProperty,
                this,
                IsSortingProperty,
                (v) =>
                {
                    if (v is not null && OwningColumn is not DataGridFillerColumn)
                    {
                        return v == ListSortDirection.Ascending || v == ListSortDirection.Descending;
                    }

                    return false;
                },
                BindingPriority.Template));
            _bindingDisposables.Add(BindUtils.RelayBind(OwningColumn.HeaderCell,
                DataGridColumnHeader.HeaderDragModeProperty,
                this,
                OwningColumnDraggingProperty,
                (v) =>
                {
                    if (OwningColumn is not DataGridFillerColumn)
                    {
                        return v == DataGridColumnHeader.DragMode.Reorder;
                    }

                    return false;
                },
                BindingPriority.Template));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _bindingDisposables?.Dispose();
        _bindingDisposables = null;
    }
}
