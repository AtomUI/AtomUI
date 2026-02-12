using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Diagnostics;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
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

    public event EventHandler<EventArgs>? FlyoutPassiveAboutToClose;
    public event EventHandler<EventArgs>? FlyoutAboutToClose;
    public event EventHandler<EventArgs>? FlyoutClosed;
    public event EventHandler<EventArgs>? FlyoutAboutToShow;
    public event EventHandler<EventArgs>? FlyoutOpened;

    public Func<Point, bool>? OpenFlyoutPredicate;
    public Func<IPopupHostProvider, RawPointerEventArgs, bool>? ClickHideFlyoutPredicate;

    private DispatcherTimer? _mouseEnterDelayTimer;
    private DispatcherTimer? _mouseLeaveDelayTimer;
    private IDisposable? _flyoutCloseDetectDisposable;
    private CompositeDisposable? _subscriptions;

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
        if (Flyout is IPopupHostProvider popupHostProvider)
        {
            var host = popupHostProvider.PopupHost;
            if (host is PopupRoot popupRoot)
            {
                // 这里 PopupRoot 关闭的时候会被关闭，所以这里的事件处理器是不是不需要删除
                if (TriggerType == FlyoutTriggerType.Hover)
                {
                    popupRoot.PointerMoved += (o, args) =>
                    {
                        StopMouseLeaveTimer();
                        if (_flyoutCloseDetectDisposable is null)
                        {
                            var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
                            _flyoutCloseDetectDisposable = inputManager.Process.Subscribe(DetectWhenToClosePopup);
                        }
                    };
                }
            }
        }
        FlyoutOpened?.Invoke(this, EventArgs.Empty);
    }

    private void HandleFlyoutClosed(object? sender, EventArgs e)
    {
        FlyoutClosed?.Invoke(this, EventArgs.Empty); 
        // 处理被动关闭
        _flyoutCloseDetectDisposable?.Dispose();
        _flyoutCloseDetectDisposable = null;
        StopMouseEnterTimer();
    }

    private void StartMouseEnterTimer()
    {
        _mouseEnterDelayTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(MouseEnterDelay), Tag = this };
        _mouseEnterDelayTimer.Tick += (sender, args) =>
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
        };
        _mouseEnterDelayTimer.Start();
    }

    private void StopMouseEnterTimer()
    {
        _mouseEnterDelayTimer?.Stop();
        _mouseEnterDelayTimer = null;
    }

    private void StopMouseLeaveTimer()
    {
        _mouseLeaveDelayTimer?.Stop();
        _mouseLeaveDelayTimer = null;
    }

    private void StartMouseLeaveTimer()
    {
        _mouseLeaveDelayTimer = new DispatcherTimer
            { Interval = TimeSpan.FromMilliseconds(MouseLeaveDelay), Tag = this };
        _mouseLeaveDelayTimer.Tick += (sender, args) =>
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
        };
        _mouseLeaveDelayTimer.Start();
    }

    public void NotifyAttachedToVisualTree()
    {
        SetupTriggerHandler();
    }

    public void NotifyDetachedFromVisualTree()
    {
        StopMouseLeaveTimer();
        StopMouseEnterTimer();
        _subscriptions?.Dispose();
    }

    private void SetupTriggerHandler()
    {
        if (AnchorTarget is null)
        {
            return;
        }
        _subscriptions?.Dispose();
        _subscriptions = new CompositeDisposable();
        if (TriggerType == FlyoutTriggerType.Hover)
        {
            _subscriptions.Add(InputElement.IsPointerOverProperty.Changed.Subscribe(args =>
            {
                if (args.Sender == AnchorTarget && 
                    AnchorTarget.IsEnabled &&
                    AnchorTarget.IsVisible)
                {
                    HandleAnchorTargetHover(args);
                }
            }));
        }
        else if (TriggerType == FlyoutTriggerType.Click ||
                 TriggerType == FlyoutTriggerType.Focus)
        {
            var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
            _subscriptions.Add(inputManager.Process.Subscribe(HandleAnchorTargetClick));
            if (TriggerType == FlyoutTriggerType.Focus)
            {
                _subscriptions.Add(InputElement.IsFocusedProperty.Changed.Subscribe(args =>
                {
                    if (args.Sender == AnchorTarget &&
                        AnchorTarget.IsEnabled &&
                        AnchorTarget.IsVisible)
                    {
                        HandleAnchorTargetFocus(args);
                    }
                }));
            }
        }
    }

    private void HandleAnchorTargetHover(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (Flyout is not null)
        {
            if (e.GetNewValue<bool>())
            {
                ShowFlyout();
            }
            else
            {
                StopMouseEnterTimer();
                StartMouseLeaveTimer();
            }
        }
    }

    public void ShowFlyout(bool immediately = false)
    {
        if (Flyout is null || AnchorTarget is null)
        {
            return;
        }

        _flyoutCloseDetectDisposable?.Dispose();
        StopMouseEnterTimer();
        StopMouseLeaveTimer();
        if (Flyout.IsOpen)
        {
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
        _flyoutCloseDetectDisposable?.Dispose();
        _flyoutCloseDetectDisposable = null;
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

    private void HandleAnchorTargetClick(RawInputEventArgs args)
    {
        if (args is RawPointerEventArgs pointerEventArgs && pointerEventArgs.Type == RawPointerEventType.LeftButtonUp)
        {
            var hitTestResult = pointerEventArgs.GetInputHitTestResult();
            if (AnchorTarget is not null && 
                AnchorTarget.IsEnabled && 
                AnchorTarget.IsVisible)
            {
                if (Flyout is null)
                {
                    return;
                }
       
                if (TriggerType == FlyoutTriggerType.Click)
                {
                    if (!Flyout.IsOpen && AnchorTarget.IsVisualAncestorOf(hitTestResult.firstEnabledAncestor as Visual))
                    {
                        if (OpenFlyoutPredicate is not null)
                        {
                            if (OpenFlyoutPredicate(pointerEventArgs.Position))
                            {
                                ShowFlyout();
                            }
                        }
                        else
                        {
                            var pos = AnchorTarget.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(AnchorTarget)!);
                            if (!pos.HasValue)
                            {
                                return;
                            }

                            var bounds = new Rect(pos.Value, AnchorTarget.Bounds.Size);
                            if (bounds.Contains(pointerEventArgs.Position))
                            {
                                ShowFlyout();
                            }
                        }
                    }
                    else
                    {
                        if (Flyout is IPopupHostProvider popupHostProvider)
                        {
                            if (ClickHideFlyoutPredicate is not null)
                            {
                                if (ClickHideFlyoutPredicate(popupHostProvider, pointerEventArgs))
                                {
                                    HideFlyout();
                                }
                            }
                            else
                            {
                                if (popupHostProvider.PopupHost != pointerEventArgs.Root)
                                {
                                    HideFlyout();
                                }
                            }
                        }
                    }
                }
                else
                {
                    var pos = AnchorTarget.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(AnchorTarget)!);
                    if (!pos.HasValue)
                    {
                        return;
                    }
                    var anchorBounds = new Rect(pos.Value, AnchorTarget.Bounds.Size);
                    if (!anchorBounds.Contains(pointerEventArgs.Position))
                    {
                        if (Flyout is IPopupHostProvider popupHostProvider)
                        {
                            if (ClickHideFlyoutPredicate is not null)
                            {
                                if (ClickHideFlyoutPredicate(popupHostProvider, pointerEventArgs))
                                {
                                    if (AnchorTarget.IsFocused)
                                    {
                                        FocusUtils.GetFocusManager(AnchorTarget)?.ClearFocus();
                                    }
                                    HideFlyout(true);
                                }
                            }
                            else
                            {
                                if (popupHostProvider.PopupHost != pointerEventArgs.Root)
                                {
                                    if (AnchorTarget.IsFocused)
                                    {
                                        FocusUtils.GetFocusManager(AnchorTarget)?.ClearFocus();
                                    }
                                    HideFlyout(true);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void HandleAnchorTargetFocus(AvaloniaPropertyChangedEventArgs<bool> e)
    {
        if (Flyout is null)
        {
            return;
        }

        if (e.GetNewValue<bool>())
        {
            ShowFlyout(true);
        }
        else
        {
            HideFlyout(true);
        }
    }
    
    private void DetectWhenToClosePopup(RawInputEventArgs args)
    {
        if (TriggerType == FlyoutTriggerType.Hover)
        {
            if (args is RawPointerEventArgs pointerEventArgs)
            {
                if (Flyout is null)
                {
                    return;
                }

                if (Flyout.IsOpen)
                {
                    var        found     = false;
                    PopupRoot? popupRoot = null;
                    if (pointerEventArgs.Root is PopupBuddyLayer buddyLayer)
                    {
                        if (buddyLayer.BuddyPopup.Host != null)
                        {
                            popupRoot = buddyLayer.BuddyPopup.Host as PopupRoot;
                        }
             
                    }
                    popupRoot ??= pointerEventArgs.Root as PopupRoot;
                    if (popupRoot is not null)
                    {
                        var current = popupRoot.Parent;
                        while (current is not null)
                        {
                            if (current == AnchorTarget)
                            {
                                found = true;
                                break;
                            }

                            current = current.Parent;
                        }
                    }
                    else if (CheckRootAncestor(pointerEventArgs.Root))
                    {
                        found = true;
                    }

                    if (!found)
                    {
                        FlyoutPassiveAboutToClose?.Invoke(this, EventArgs.Empty);
                        HideFlyout();
                    }
                }
            }
        }
    }

    private bool CheckRootAncestor(IInputRoot root)
    {
        if (root.Equals(AnchorTarget))
        {
            return true;
        }
        if (root is ILogical logical)
        {
            ILogical? current = logical;
            while (current != null)
            {
                if (current.Equals(AnchorTarget))
                {
                    return true;
                }
                current = current.LogicalParent;
            }
        }

        return false;
    }
}
