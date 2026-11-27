using System.Reactive.Disposables;
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
    private IDisposable? _owningGridBindings;

    // The binding subscribes to the property changes of the grid. The column that created this control releases
    // it through ReleaseOwningGridBindings when the row is discarded.
    internal void BindOwningGridProperties(DataGrid owningGrid)
    {
        _owningGridBindings?.Dispose();
        _owningGridBindings = new CompositeDisposable(2)
        {
            Bind(IsMotionEnabledProperty, owningGrid[!DataGrid.IsMotionEnabledProperty]),
            Bind(IsEnabledProperty, owningGrid[!DataGrid.IsEnabledProperty])
        };
    }

    internal void ReleaseOwningGridBindings()
    {
        _owningGridBindings?.Dispose();
        _owningGridBindings = null;
    }

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

    // Moves, release and capture loss of an active drag are handled by DataGridRowsPresenter, which owns the
    // pointer capture; this row may be recycled while the drag continues, so it must not end the session.

    #region 内部协作 API

    internal void NotifyLoadingRow(DataGridRow row)
    {
        OwningRow = row;
    }

    internal void NotifyUnLoadingRow(DataGridRow row)
    {
        if (ReferenceEquals(OwningRow, row))
        {
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
        base.OnApplyTemplate(e);
        _indicatorButton = e.NameScope.Find<IconButton>(DataGridRowReorderHandleConstants.IndicatorIconButtonPart);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _indicatorButton = null;
        base.OnDetachedFromVisualTree(e);
    }
}
