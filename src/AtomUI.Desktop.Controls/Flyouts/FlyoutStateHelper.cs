using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class FlyoutStateHelper : AvaloniaObject
{
    public static readonly StyledProperty<Control?> AnchorTargetProperty =
        AvaloniaProperty.Register<FlyoutStateHelper, Control?>(nameof(AnchorTarget));

    public static readonly StyledProperty<PopupFlyoutBase?> FlyoutProperty =
        AvaloniaProperty.Register<FlyoutStateHelper, PopupFlyoutBase?>(nameof(Flyout));

    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        AvaloniaProperty.Register<FlyoutStateHelper, int>(nameof(MouseEnterDelay), 200);

    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        AvaloniaProperty.Register<FlyoutStateHelper, int>(nameof(MouseLeaveDelay), 200);

    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<FlyoutStateHelper, FlyoutTriggerType>(nameof(TriggerType), FlyoutTriggerType.Click);

    public Control? AnchorTarget
    {
        get => GetValue(AnchorTargetProperty);
        set => SetValue(AnchorTargetProperty, value);
    }

    public PopupFlyoutBase? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }

    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    public event EventHandler<EventArgs>? FlyoutAboutToClose;
    public event EventHandler<EventArgs>? FlyoutAboutToShow;
    public event EventHandler<EventArgs>? FlyoutOpened;
    public event EventHandler<EventArgs>? FlyoutClosed;

    private DispatcherTimer? _mouseEnterDelayTimer;
    private DispatcherTimer? _mouseLeaveDelayTimer;
    private CompositeDisposable? _subscriptions;
    private IDisposable? _popupPointerSubscription;
    private TopLevel? _registeredTopLevel;
    private bool _isFlyoutShowing;

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FlyoutProperty)
        {
            if (change.OldValue is Flyout oldFlyout)
            {
                oldFlyout.Opened -= HandleFlyoutOpened;
                oldFlyout.Closed -= HandleFlyoutClosed;
            }

            if (change.NewValue is Flyout newFlyout)
            {
                newFlyout.Opened += HandleFlyoutOpened;
                newFlyout.Closed += HandleFlyoutClosed;
            }
        }
        else if (change.Property == AnchorTargetProperty || change.Property == TriggerTypeProperty)
        {
            SetupTriggerHandler();
        }
    }

    private void HandleFlyoutOpened(object? sender, EventArgs e)
    {
        if (TriggerType == FlyoutTriggerType.Focus)
        {
            RegisterRootPointerHandler();
            Dispatcher.Post(() => _isFlyoutShowing = false);
        }
        else if (TriggerType == FlyoutTriggerType.Hover)
        {
            _isFlyoutShowing = false;
            SubscribeToPopupPointer();
        }
        else
        {
            _isFlyoutShowing = false;
        }
        FlyoutOpened?.Invoke(this, EventArgs.Empty);
    }

    private void HandleFlyoutClosed(object? sender, EventArgs e)
    {
        UnregisterRootPointerHandler();
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;
        FlyoutClosed?.Invoke(this, EventArgs.Empty);
    }

    #region Root pointer handler for Focus mode

    private void RegisterRootPointerHandler()
    {
        UnregisterRootPointerHandler();
        if (AnchorTarget is null)
        {
            return;
        }

        _registeredTopLevel = TopLevel.GetTopLevel(AnchorTarget);
        _registeredTopLevel?.AddHandler(InputElement.PointerPressedEvent, HandleFocusModeRootPointerPressed,
            RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void UnregisterRootPointerHandler()
    {
        if (_registeredTopLevel != null)
        {
            _registeredTopLevel.RemoveHandler(InputElement.PointerPressedEvent, HandleFocusModeRootPointerPressed);
            _registeredTopLevel = null;
        }
    }

    private void HandleFocusModeRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (Flyout is not { IsOpen: true } || AnchorTarget is null)
        {
            return;
        }

        if (e.Source is Visual source && !IsVisualInFlyoutScope(source))
        {
            HideFlyout(immediately: true);
        }
    }

    #endregion

    #region Timer helpers

    private void StartMouseEnterTimer()
    {
        StopMouseEnterTimer();
        _mouseEnterDelayTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(MouseEnterDelay), Tag = this };
        _mouseEnterDelayTimer.Tick += HandleMouseEnterTimerTick;
        _mouseEnterDelayTimer.Start();
    }

    private void HandleMouseEnterTimerTick(object? sender, EventArgs e)
    {
        if (_mouseEnterDelayTimer != null)
        {
            StopMouseEnterTimer();
            if (Flyout is null || AnchorTarget is null)
            {
                return;
            }

            FlyoutAboutToShow?.Invoke(this, EventArgs.Empty);
            Flyout.ShowAt(AnchorTarget);
        }
    }

    private void StopMouseEnterTimer()
    {
        if (_mouseEnterDelayTimer != null)
        {
            _mouseEnterDelayTimer.Stop();
            _mouseEnterDelayTimer.Tick -= HandleMouseEnterTimerTick;
            _mouseEnterDelayTimer = null;
        }
    }

    private void StopMouseLeaveTimer()
    {
        if (_mouseLeaveDelayTimer != null)
        {
            _mouseLeaveDelayTimer.Stop();
            _mouseLeaveDelayTimer.Tick -= HandleMouseLeaveTimerTick;
            _mouseLeaveDelayTimer = null;
        }
    }

    private void StartMouseLeaveTimer()
    {
        StopMouseLeaveTimer();
        _mouseLeaveDelayTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(MouseLeaveDelay), Tag = this };
        _mouseLeaveDelayTimer.Tick += HandleMouseLeaveTimerTick;
        _mouseLeaveDelayTimer.Start();
    }

    private void HandleMouseLeaveTimerTick(object? sender, EventArgs e)
    {
        if (_mouseLeaveDelayTimer != null)
        {
            StopMouseLeaveTimer();
            if (Flyout is null)
            {
                return;
            }
    
            FlyoutAboutToClose?.Invoke(this, EventArgs.Empty);
            Flyout.Hide();
        }
    }

    #endregion

    #region Scope helpers

    private bool IsVisualInFlyoutScope(Visual visual)
    {
        if (AnchorTarget is not null &&
            (visual == AnchorTarget || AnchorTarget.IsVisualAncestorOf(visual)))
        {
            return true;
        }

        if (Flyout is PopupFlyoutBase { Popup.Child: Visual popupChild } &&
            (visual == popupChild || popupChild.IsLogicalAncestorOf(visual)))
        {
            return true;
        }

        return false;
    }

    private bool IsFocusWithinFlyoutScope()
    {
        if (AnchorTarget is null)
        {
            return false;
        }

        var topLevel = TopLevel.GetTopLevel(AnchorTarget);
        return topLevel?.FocusManager.GetFocusedElement() is Visual focused
               && IsVisualInFlyoutScope(focused);
    }

    private bool IsAnchorReady =>
        Flyout is not null && AnchorTarget is { IsEnabled: true, IsVisible: true };

    #endregion

    public void NotifyAttachedToVisualTree()
    {
        SetupTriggerHandler();
    }

    public void NotifyDetachedFromVisualTree()
    {
        StopMouseLeaveTimer();
        StopMouseEnterTimer();
        UnregisterRootPointerHandler();
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;
        _subscriptions?.Dispose();
        _subscriptions = null;
    }

    private void SetupTriggerHandler()
    {
        _subscriptions?.Dispose();

        if (AnchorTarget is null)
        {
            return;
        }
        _subscriptions = new CompositeDisposable();

        if (Flyout is Flyout atomFlyout)
        {
            atomFlyout.IsLightDismissEnabled = TriggerType == FlyoutTriggerType.Click;
        }
        else if (Flyout != null)
        {
            Flyout.Popup.IsLightDismissEnabled = TriggerType == FlyoutTriggerType.Click;
        }

        switch (TriggerType)
        {
            case FlyoutTriggerType.Hover:
                SetupHoverTrigger();
                break;
            case FlyoutTriggerType.Click:
                SetupClickTrigger();
                break;
            case FlyoutTriggerType.Focus:
                SetupFocusTrigger();
                break;
        }
    }

    #region Hover trigger

    private void SetupHoverTrigger()
    {
        var anchor = AnchorTarget!;
        _subscriptions!.Add(anchor.GetObservable(InputElement.IsPointerOverProperty).Subscribe(isPointerOver =>
        {
            if (anchor.IsEnabled && anchor.IsVisible)
            {
                HandleAnchorTargetHover(isPointerOver);
            }
        }));
    }

    private void SubscribeToPopupPointer()
    {
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;

        if (AnchorTarget != null)
        {
            var inputManager = AvaloniaLocator.Current.GetService<IInputManager>();
            if (inputManager != null)
            {
                _popupPointerSubscription = inputManager.Process.Subscribe(HandleGlobalPointerMove);
            }
        }
    }

    private void HandleGlobalPointerMove(RawInputEventArgs e)
    {
        if (e is RawPointerEventArgs pointerEvent && pointerEvent.Type == RawPointerEventType.Move)
        {
            var hitElement = pointerEvent.GetInputHitTestResult().element;
            if (hitElement is Visual visual)
            {
                // Check if the pointer is over the flyout scope
                if (IsVisualInFlyoutScope(visual))
                {
                    StopMouseLeaveTimer();
                }
                else if (AnchorTarget is not { IsPointerOver: true })
                {
                    // Pointer left both anchor and popup, start close timer if not already started
                    if (_mouseLeaveDelayTimer == null && Flyout is { IsOpen: true })
                    {
                        StartMouseLeaveTimer();
                    }
                }
            }
        }
    }

    private void HandleAnchorTargetHover(bool isPointerOver)
    {
        if (Flyout is not null)
        {
            if (isPointerOver)
            {
                ShowFlyout();
            }
            else
            {
                StopMouseEnterTimer();
                if (Flyout is PopupFlyoutBase { IsOpen: true, Popup.Child: InputElement popupChild }
                    && popupChild.IsPointerOver)
                {
                    return;
                }
                StartMouseLeaveTimer();
            }
        }
    }

    #endregion

    #region Click trigger

    private void SetupClickTrigger()
    {
        var anchor = AnchorTarget!;
        anchor.AddHandler(InputElement.PointerPressedEvent, HandleClickModePointerPressed,
            handledEventsToo: true);
        anchor.AddHandler(InputElement.KeyDownEvent, HandleClickModeKeyDown,
            handledEventsToo: true);
        _subscriptions!.Add(Disposable.Create(() =>
        {
            anchor.RemoveHandler(InputElement.PointerPressedEvent, HandleClickModePointerPressed);
            anchor.RemoveHandler(InputElement.KeyDownEvent, HandleClickModeKeyDown);
        }));
    }

    private void HandleClickModePointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsAnchorReady)
        {
            return;
        }

        ToggleClickModeFlyout();
    }

    private void HandleClickModeKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Enter or Key.Space) ||
            !IsAnchorReady ||
            !IsAnchorKeyboardFocused())
        {
            return;
        }

        ToggleClickModeFlyout();
        e.Handled = true;
    }

    private void ToggleClickModeFlyout()
    {
        if (Flyout!.IsOpen)
        {
            HideFlyout(immediately: true);
        }
        else
        {
            ShowFlyout(immediately: true);
        }
    }

    private bool IsAnchorKeyboardFocused()
    {
        if (AnchorTarget is null)
        {
            return false;
        }

        var topLevel = TopLevel.GetTopLevel(AnchorTarget);
        return ReferenceEquals(topLevel?.FocusManager?.GetFocusedElement(), AnchorTarget);
    }

    #endregion

    #region Focus trigger

    private void SetupFocusTrigger()
    {
        var anchor = AnchorTarget!;
        anchor.GotFocus  += HandleFocusModeGotFocus;
        anchor.LostFocus += HandleFocusModeLostFocus;
        anchor.AddHandler(InputElement.PointerPressedEvent, HandleFocusModeAnchorPointerPressed,
            handledEventsToo: true);
        _subscriptions!.Add(Disposable.Create(() =>
        {
            anchor.GotFocus  -= HandleFocusModeGotFocus;
            anchor.LostFocus -= HandleFocusModeLostFocus;
            anchor.RemoveHandler(InputElement.PointerPressedEvent, HandleFocusModeAnchorPointerPressed);
            UnregisterRootPointerHandler();
        }));
    }

    private void HandleFocusModeGotFocus(object? sender, FocusChangedEventArgs e)
    {
        TryShowIfClosed();
    }

    private void HandleFocusModeAnchorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryShowIfClosed();
    }

    private void TryShowIfClosed()
    {
        if (IsAnchorReady && !Flyout!.IsOpen)
        {
            ShowFlyout(immediately: true);
        }
    }

    private void HandleFocusModeLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (Flyout is not { IsOpen: true } || _isFlyoutShowing)
        {
            return;
        }

        Dispatcher.Post(() =>
        {
            if (Flyout is { IsOpen: true } && !_isFlyoutShowing && !IsFocusWithinFlyoutScope())
            {
                var topLevel = AnchorTarget != null ? TopLevel.GetTopLevel(AnchorTarget) : null;
                var currentFocused = topLevel?.FocusManager.GetFocusedElement();

                if (currentFocused is Control focusedControl)
                {
                    var focusedParent = focusedControl.FindLogicalAncestorOfType<FlyoutHost>();
                    if (focusedParent != null && focusedParent.Flyout != null && focusedParent.Flyout.IsOpen)
                    {
                        return;
                    }
                }

                HideFlyout(immediately: true);
            }
        });
    }

    #endregion

    public void ShowFlyout(bool immediately = false)
    {
        if (Flyout is null || AnchorTarget is null)
        {
            return;
        }

        _isFlyoutShowing = true;
        StopMouseEnterTimer();
        StopMouseLeaveTimer();
        if (Flyout.IsOpen)
        {
            if (Flyout.Popup is Popup popup && popup.IsPlayingCloseMotion)
            {
                popup.CancelCloseAnimation();
                _isFlyoutShowing = false;
                return;
            }
            Flyout.Hide();
        }

        if (immediately || MouseEnterDelay == 0)
        {
            FlyoutAboutToShow?.Invoke(this, EventArgs.Empty);
            Flyout.ShowAt(AnchorTarget);
        }
        else
        {
            StartMouseEnterTimer();
        }
    }

    public void HideFlyout(bool immediately = false)
    {
        if (Flyout is null)
        {
            return;
        }

        StopMouseEnterTimer();
        if (immediately || MouseLeaveDelay == 0)
        {
            FlyoutAboutToClose?.Invoke(this, EventArgs.Empty);
            Flyout.Hide();
        }
        else
        {
            StartMouseLeaveTimer();
        }
    }
}
