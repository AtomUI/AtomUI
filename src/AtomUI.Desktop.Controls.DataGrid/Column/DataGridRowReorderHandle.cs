using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace AtomUI.Desktop.Controls;

internal class DataGridRowReorderHandle : TemplatedControl
{
    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DataGridRowReorderHandle>();

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    internal DataGrid? OwningGrid { get; set; }
    internal DataGridRow? OwningRow;

    private IconButton? _indicatorButton;

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (!IsEnabled ||
            OwningGrid is not { RowsPresenter: { } rowsPresenter } owningGrid ||
            OwningRow is not { } owningRow ||
            !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        if (owningGrid.TryBeginRowReorder(
                this,
                owningRow,
                e.Pointer,
                e.GetPosition(rowsPresenter)))
        {
            e.Handled = true;
        }
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (OwningGrid is not { RowsPresenter: { } rowsPresenter } owningGrid)
        {
            return;
        }

        owningGrid.HandleRowReorderPointerMoved(
            this,
            e.Pointer,
            e.GetPosition(rowsPresenter),
            e.GetCurrentPoint(this).Properties.IsLeftButtonPressed);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
        {
            OwningGrid?.HandleRowReorderPointerReleased(this, e.Pointer);
        }
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        OwningGrid?.HandleRowReorderPointerCaptureLost(this, e.Pointer);
        base.OnPointerCaptureLost(e);
    }

    #region 内部协作 API

    internal void NotifyLoadingRow(DataGridRow row)
    {
        OwningRow = row;
    }

    internal void NotifyUnLoadingRow(DataGridRow row)
    {
        if (ReferenceEquals(OwningRow, row))
        {
            OwningGrid?.CancelRowReorder(this, row);
            OwningRow = null;
        }
    }

    internal IDisposable? SuppressIndicatorTransitions()
    {
        return _indicatorButton is { } button ? new TransitionSuppressionScope(button) : null;
    }

    #endregion

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        OwningGrid?.CancelRowReorder(this);
        base.OnApplyTemplate(e);
        _indicatorButton = e.NameScope.Find<IconButton>(DataGridRowReorderHandleConstants.IndicatorIconButtonPart);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        OwningGrid?.CancelRowReorder(this);
        _indicatorButton = null;
        base.OnDetachedFromVisualTree(e);
    }
}
