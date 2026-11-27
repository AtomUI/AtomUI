using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shows and hides tooltips as the pointer moves over controls that carry an attached <c>ToolTip.Tip</c> value.
/// It opens the tooltip after the host's <c>ToolTip.ShowDelay</c> and closes it when the pointer leaves the host,
/// a pointer button is pressed or the pointer leaves the window. Hosts whose <c>ToolTip.ServiceEnabled</c> is
/// <see langword="false" /> and hosts that set <c>ToolTip.IsCustomShowAndHide</c> are skipped.
/// </summary>
public sealed class ToolTipService : IDisposable
{
    private readonly IDisposable _subscriptions;

    private Control? _tipControl;
    private long _lastTipCloseTime;
    private DispatcherTimer? _timer;
    private ulong _lastTipEventTime;
    private ulong _lastWindowEventTime;

    /// <summary>
    /// Starts handling pointer input at once: subscribes to the raw events of <paramref name="inputManager" /> and to
    /// changes of <c>ToolTip.ServiceEnabled</c> and <c>ToolTip.Tip</c> until <see cref="Dispose" /> is called.
    /// </summary>
    public ToolTipService(IInputManager inputManager)
    {
        _subscriptions = new CompositeDisposable([
            inputManager.Process.Subscribe(InputManager_OnProcess),
            ToolTip.ServiceEnabledProperty.Changed.Subscribe(ServiceEnabledChanged),
            ToolTip.TipProperty.Changed.Subscribe(TipChanged)]);
    }

    /// <summary>
    /// Stops the pending show timer, drops the current tooltip host and disposes the input and property
    /// subscriptions, after which the service no longer reacts to pointer input. A tooltip that is open is left open.
    /// </summary>
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

    /// <summary>
    /// Makes the tooltip host under the pointer the current host. That is the nearest <c>Control</c> among
    /// <paramref name="candidateToolTipHost" /> and its visual parents that carries a <c>ToolTip.Tip</c> and is
    /// enabled or sets <c>ToolTip.ShowOnDisabled</c>. A change of host closes the previous host's tooltip when
    /// it is open and shows the new host's tooltip at once when one closed with this call or within
    /// <c>ToolTip.BetweenShowDelay</c> milliseconds, otherwise after <c>ToolTip.ShowDelay</c>; a tooltip that is
    /// already open stays as it is. Nothing changes while the pointer is over the current tooltip, or when the
    /// walk meets a control that clears <c>ToolTip.ServiceEnabled</c> or sets <c>ToolTip.IsCustomShowAndHide</c>.
    /// </summary>
    /// <param name="root">
    /// The input root of the pointer event. When this is the input root of a current tooltip that is shown in a
    /// different root than its host, the call returns at once. A host found under another root counts as no host.
    /// </param>
    /// <param name="candidateToolTipHost">
    /// The visual under the pointer; the host is looked up on it and on its visual parents. Pass
    /// <see langword="null" /> when nothing was hit: that selects no host and closes the current host's tooltip.
    /// </param>
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
