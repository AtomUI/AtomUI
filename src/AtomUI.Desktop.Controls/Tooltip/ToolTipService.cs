using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public sealed class ToolTipService : IDisposable
{
    private readonly IDisposable _subscriptions;

    private Control? _tipControl;
    private long _lastTipCloseTime;
    private DispatcherTimer? _timer;
    private ulong _lastTipEventTime;
    private ulong _lastWindowEventTime;

    public ToolTipService(IInputManager inputManager)
    {
        _subscriptions = new CompositeDisposable([
            inputManager.Process.Subscribe(InputManager_OnProcess),
            ToolTip.ServiceEnabledProperty.Changed.Subscribe(ServiceEnabledChanged),
            ToolTip.TipProperty.Changed.Subscribe(TipChanged)]);
    }

    public void Dispose()
    {
        StopTimer();
        if (_tipControl?.GetValue(ToolTip.ToolTipProperty) is { } tooltip)
        {
            tooltip.Closed -= ToolTipClosed;
            tooltip.PointerExited -= ToolTipPointerExited;
        }
        SetTipControl(null);
        _subscriptions.Dispose();
    }

    private void InputManager_OnProcess(RawInputEventArgs e)
    {
        if (e is RawPointerEventArgs pointerEvent)
        {
            var ownerRoot = GetInputRoot(_tipControl);
            var tipRoot = GetInputRoot(_tipControl?.GetValue(ToolTip.ToolTipProperty));
            // A routed pointer event can synchronously close its window before Process runs.
            // Compare input-root identities without requiring the event root's visual to survive.
            var isOwnerEvent = ownerRoot is not null && e.Root == ownerRoot;
            var isTooltipEvent = tipRoot is not null && tipRoot != ownerRoot && e.Root == tipRoot;
            if (isTooltipEvent)
            {
                _lastTipEventTime = pointerEvent.Timestamp;
            }
            else if (isOwnerEvent)
            {
                _lastWindowEventTime = pointerEvent.Timestamp;
            }

            switch (pointerEvent.Type)
            {
                case RawPointerEventType.Move:
                    Update(pointerEvent.Root, pointerEvent.GetInputHitTestResult().element as Visual);
                    break;
                case RawPointerEventType.LeaveWindow
                    when (isOwnerEvent &&
                          _lastTipEventTime != e.Timestamp) ||
                         (isTooltipEvent && _lastWindowEventTime != e.Timestamp):
                    ClearTip();
                    SetTipControl(null);
                    break;
                case RawPointerEventType.LeftButtonDown:
                case RawPointerEventType.RightButtonDown:
                case RawPointerEventType.MiddleButtonDown:
                case RawPointerEventType.XButton1Down:
                case RawPointerEventType.XButton2Down:
                    if (_tipControl is not null && ToolTip.GetIsCustomShowAndHide(_tipControl))
                    {
                        break;
                    }
                    ClearTip();
                    break;
            }

            void ClearTip()
            {
                StopTimer();
                _tipControl?.ClearValue(ToolTip.IsOpenProperty);
            }
        }
    }

    public void Update(IInputRoot root, Visual? candidateToolTipHost)
    {
        var currentToolTip = _tipControl?.GetValue(ToolTip.ToolTipProperty);
        var tipRoot = GetInputRoot(currentToolTip);

        // Overlay tips share the owner root; their hits are recognized by the ancestor walk below.
        if (tipRoot is not null && tipRoot != GetInputRoot(_tipControl) && root == tipRoot)
        {
            return;
        }

        while (candidateToolTipHost != null)
        {
            if (candidateToolTipHost == currentToolTip)
            {
                return;
            }

            if (candidateToolTipHost is Control control)
            {
                if (!ToolTip.GetServiceEnabled(control) || ToolTip.GetIsCustomShowAndHide(control))
                {
                    return;
                }

                if (ToolTip.GetTip(control) != null &&
                    (control.IsEffectivelyEnabled || ToolTip.GetShowOnDisabled(control)))
                {
                    break;
                }
            }

            candidateToolTipHost = candidateToolTipHost?.GetVisualParent();
        }

        var newControl = candidateToolTipHost as Control;
        if (newControl is not null && GetInputRoot(newControl) != root)
        {
            // Hit-test results can outlive a synchronous detach during routed input dispatch.
            newControl = null;
        }

        if (newControl == _tipControl)
        {
            return;
        }

        var oldControl = _tipControl;
        SetTipControl(newControl);
        HandleTipControlChanged(oldControl, newControl);
    }

    private static IInputRoot? GetInputRoot(Visual? visual) =>
        visual?.GetPresentationSource() as IInputRoot;

    private void SetTipControl(Control? control)
    {
        if (_tipControl is not null)
        {
            _tipControl.DetachedFromVisualTree -= HandleTipControlDetached;
        }
        _tipControl = control;
        if (_tipControl is not null)
        {
            _tipControl.DetachedFromVisualTree += HandleTipControlDetached;
        }
    }

    private void HandleTipControlDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender == _tipControl)
        {
            StopTimer();
            if (_tipControl?.GetValue(ToolTip.ToolTipProperty) is { } tooltip)
            {
                // Popup teardown raises PointerExited before Closed; this is a lifecycle exit,
                // not a user request to clear the owner's desired open state.
                tooltip.PointerExited -= ToolTipPointerExited;
            }
            // ToolTip reconciliation owns lifecycle closure and preserves the desired IsOpen state.
            SetTipControl(null);
        }
    }

    private void ServiceEnabledChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.Sender == _tipControl && !ToolTip.GetServiceEnabled(_tipControl))
        {
            StopTimer();
        }
    }

    private void TipChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var control = (Control)e.Sender;

        if (ToolTip.GetIsOpen(control) && e.NewValue != e.OldValue && e.NewValue is not ToolTip)
        {
            if (e.NewValue is null)
            {
                Close(control);
            }
            else
            {
                if (control.GetValue(ToolTip.ToolTipProperty) is { } tip)
                {
                    tip.Content = e.NewValue;
                }
            }
        }
    }

    private void HandleTipControlChanged(Control? oldValue, Control? newValue)
    {
        StopTimer();

        var closedPreviousTip = false;

        if (oldValue != null && ToolTip.GetIsOpen(oldValue))
        {
            Close(oldValue);
            closedPreviousTip = true;
        }

        if (newValue != null && newValue == _tipControl && !ToolTip.GetIsOpen(newValue))
        {
            var betweenShowDelay = ToolTip.GetBetweenShowDelay(newValue);

            int showDelay;

            if (betweenShowDelay >= 0 &&
                (closedPreviousTip ||
                 (DateTime.UtcNow.Ticks - _lastTipCloseTime) <=
                 betweenShowDelay * TimeSpan.TicksPerMillisecond))
            {
                showDelay = 0;
            }
            else
            {
                showDelay = ToolTip.GetShowDelay(newValue);
            }

            if (showDelay == 0)
            {
                Open(newValue);
            }
            else
            {
                StartShowTimer(showDelay, newValue);
            }
        }
    }

    private void ToolTipClosed(object? sender, EventArgs e)
    {
        _lastTipCloseTime = DateTime.UtcNow.Ticks;
        if (sender is ToolTip toolTip)
        {
            toolTip.Closed -= ToolTipClosed;
            toolTip.PointerExited -= ToolTipPointerExited;
        }
    }

    private void ToolTipPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is ToolTip { AdornedControl: { } control } && control != _tipControl)
        {
            Close(control);
        }
    }

    private void StartShowTimer(int showDelay, Control control)
    {
        if (_timer == null)
        {
            _timer = new DispatcherTimer();
            _timer.Tick += HandleShowTimerTick;
        }
        _timer.Stop();
        _timer.Tag = control;
        _timer.Interval = TimeSpan.FromMilliseconds(showDelay);
        _timer.Start();
    }

    private void HandleShowTimerTick(object? sender, EventArgs e)
    {
        if (_timer is null)
        {
            return;
        }
        _timer.Stop();
        if (_timer.Tag is Control pendingControl)
        {
            Open(pendingControl);
        }
    }

    private void Open(Control control)
    {
        StopTimer();

        if (control.IsAttachedToVisualTree())
        {
            ToolTip.SetIsOpen(control, true);

            if (control == _tipControl && ToolTip.GetIsOpen(control) &&
                control.GetValue(ToolTip.ToolTipProperty) is { } tooltip)
            {
                tooltip.Closed += ToolTipClosed;
                tooltip.PointerExited += ToolTipPointerExited;
            }
        }
    }

    private void Close(Control control)
    {
        if (!ToolTip.GetIsCustomShowAndHide(control))
        {
            ToolTip.SetIsOpen(control, false);
        }
    }

    private void StopTimer()
    {
        _timer?.Stop();
        if (_timer != null)
        {
            _timer.Tag = null;
        }
    }
}
