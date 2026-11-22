using System.Reactive.Disposables;
using AtomUI.Controls.Primitives;
using AtomUI.Native;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

namespace AtomUI.Desktop.Controls;

// The full-screen title bar: mode resolution, title-bar slot arbitration, the AutoHide reveal state machine, the
// platform's own full-screen chrome and the Docked content inset. Window is the single owner of all of them; the
// host only presents what is projected here.
public partial class Window
{
    private const double FullScreenTitleBarTriggerBandHeight = 2;
    private const double FullScreenTitleBarTouchTriggerBandHeight = 12;
    private const double FullScreenTitleBarTouchRevealDistance = 16;
    private const double FullScreenTitleBarKeepZoneExtension = 8;

    private readonly HashSet<AvaloniaPopup> _fullScreenTitleBarPopups = new();
    private FullScreenTitleBarHost? _fullScreenTitleBarHost;
    private WindowChromeLayer? _windowChromeLayer;
    private CompositeDisposable? _fullScreenTitleBarHostSubscriptions;
    private FullScreenTitleBarRevealController? _fullScreenTitleBarRevealController;
    private IDisposable? _fullScreenTitleBarPointerLeaveSubscription;
    private IFullScreenNativeChrome? _fullScreenNativeChrome;
    private IInputElement? _fullScreenTitleBarFocusReturnTarget;
    private Point? _fullScreenTitleBarTouchOrigin;

    private static bool CoerceIsFullScreenTitleBarOpen(AvaloniaObject sender, bool value)
    {
        return ((Window)sender).EffectiveFullScreenTitleBarMode switch
        {
            FullScreenTitleBarMode.Pinned or FullScreenTitleBarMode.Docked => true,
            FullScreenTitleBarMode.AutoHide => value,
            _ => false
        };
    }

    private void HandleFullScreenTitleBarPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == WindowStateProperty ||
            change.Property == FullScreenTitleBarModeProperty ||
            change.Property == FullScreenTitleBarProperty ||
            change.Property == FullScreenTitleBarTemplateProperty ||
            change.Property == IsTitleBarVisibleProperty)
        {
            UpdateFullScreenTitleBar();
        }
        else if (change.Property == FullScreenTitleBarMouseEnterDelayProperty ||
                 change.Property == FullScreenTitleBarMouseLeaveDelayProperty)
        {
            ApplyFullScreenTitleBarDelays();
        }
        else if (change.Property == IsFullScreenTitleBarOpenProperty)
        {
            HandleFullScreenTitleBarOpenRequested(change.GetNewValue<bool>());
        }
        else if (change.Property == IsActiveProperty)
        {
            UpdateFullScreenTitleBarSuppression();
        }
    }

    private void AttachFullScreenTitleBarHost(FullScreenTitleBarHost? host)
    {
        _fullScreenTitleBarHostSubscriptions?.Dispose();
        _fullScreenTitleBarHostSubscriptions = null;
        _fullScreenTitleBarHost              = host;
        if (host is null)
        {
            return;
        }

        host.DismissRequested += HandleFullScreenTitleBarDismissRequested;
        var subscriptions = new CompositeDisposable
        {
            Disposable.Create(() => host.DismissRequested -= HandleFullScreenTitleBarDismissRequested)
        };
        _fullScreenTitleBarHostSubscriptions = subscriptions;
        subscriptions.Add(host.GetObservable(FullScreenTitleBarHost.SurfaceHeightProperty)
                              .Subscribe(_ => UpdateFullScreenTitleBarGeometry()));
        subscriptions.Add(host.GetObservable(IsKeyboardFocusWithinProperty)
                              .Subscribe(isFocusWithin =>
                                  _fullScreenTitleBarRevealController?.SetHold(
                                      FullScreenTitleBarHolds.Focus,
                                      isFocusWithin)));
    }

    /// <summary>
    /// Moves the template-declared full-screen title bar host into a fresh window chrome layer (T6) of the template's
    /// <c>PART_VisualLayerManager</c>, releasing the layer of the previous template first.
    /// </summary>
    private void AttachWindowChromeLayer(VisualLayerManager? visualLayerManager, FullScreenTitleBarHost? host)
    {
        if (_windowChromeLayer is { } current &&
            ReferenceEquals(current.GetVisualParent(), visualLayerManager) &&
            ReferenceEquals(current.HostedTitleBar, host))
        {
            return;
        }

        ReleaseWindowChromeLayer();
        if (visualLayerManager is null || host is null)
        {
            return;
        }

        // WindowChromeLayerTheme projects the manager's Padding (OffScreenMargin) onto the layer's Margin.
        var layer = new WindowChromeLayer();
        LayerLadder.AddLayer(visualLayerManager, layer, LayerLadder.WindowChrome);
        layer.Host(host);
        _windowChromeLayer = layer;
    }

    /// <summary>
    /// Gives up the current window chrome layer: the title bar host leaves it.
    /// Avalonia's VisualLayerManager has no API to remove an added layer (AddLayer is private and has no inverse), so
    /// the emptied layer stays in its manager and is discarded together with the template that owns that manager.
    /// </summary>
    private void ReleaseWindowChromeLayer()
    {
        _windowChromeLayer?.Release();
        _windowChromeLayer = null;
    }

    private void DetachFullScreenTitleBar()
    {
        StopFullScreenTitleBarReveal();
        _fullScreenTitleBarRevealController?.Dispose();
        _fullScreenTitleBarRevealController = null;
        ReleaseFullScreenNativeChrome();
        AttachFullScreenTitleBarHost(null);
        ReleaseWindowChromeLayer();
    }

    /// <summary>
    /// Re-resolves the full-screen title bar after any of its inputs changed: window state, mode, content source,
    /// default title bar or template.
    /// </summary>
    private void UpdateFullScreenTitleBar()
    {
        var mode         = ResolveEffectiveFullScreenTitleBarMode();
        var hostsDefault = mode != FullScreenTitleBarMode.Hidden && !HasCustomFullScreenTitleBar();
        UpdateTitleBarSlots(mode, hostsDefault);
        EffectiveFullScreenTitleBarMode = mode;
        UpdateFullScreenNativeChromeSession(mode);
        UpdateFullScreenTitleBarReveal(mode);
        CoerceValue(IsFullScreenTitleBarOpenProperty);
        if (mode == FullScreenTitleBarMode.AutoHide &&
            _fullScreenTitleBarRevealController is { } controller &&
            IsFullScreenTitleBarOpen != controller.IsOpen)
        {
            SetCurrentValue(IsFullScreenTitleBarOpenProperty, controller.IsOpen);
        }

        UpdateFullScreenTitleBarGeometry();
    }

    private FullScreenTitleBarMode ResolveEffectiveFullScreenTitleBarMode()
    {
        if (WindowState != WindowState.FullScreen ||
            _fullScreenTitleBarHost is null ||
            FullScreenTitleBarMode == FullScreenTitleBarMode.Hidden)
        {
            return FullScreenTitleBarMode.Hidden;
        }

        var hasContent = HasCustomFullScreenTitleBar() || (IsTitleBarVisible && TitleBar is not null);
        return hasContent ? FullScreenTitleBarMode : FullScreenTitleBarMode.Hidden;
    }

    private bool HasCustomFullScreenTitleBar()
    {
        return FullScreenTitleBar is not null || FullScreenTitleBarTemplate is not null;
    }

    private void UpdateTitleBarSlots(FullScreenTitleBarMode mode, bool hostsDefault)
    {
        var titleBar = TitleBar;
        var normal   = hostsDefault ? null : titleBar;
        object? fullScreenContent = mode == FullScreenTitleBarMode.Hidden
            ? null
            : hostsDefault ? titleBar : FullScreenTitleBar;
        IDataTemplate? fullScreenTemplate = mode == FullScreenTitleBarMode.Hidden || hostsDefault
            ? null
            : FullScreenTitleBarTemplate;
        var titleBarMoves = !ReferenceEquals(NormalSlotTitleBar, normal) ||
                            (hostsDefault && !ReferenceEquals(FullScreenSlotContent, titleBar));

        // The slot that gives the title bar up is cleared before the slot that receives it, so the instance never
        // has two parents. The template follows the content it belongs to.
        if (normal is null)
        {
            NormalSlotTitleBar            = null;
            FullScreenSlotContentTemplate = fullScreenTemplate;
            FullScreenSlotContent         = fullScreenContent;
        }
        else
        {
            FullScreenSlotContent         = fullScreenContent;
            FullScreenSlotContentTemplate = fullScreenTemplate;
            NormalSlotTitleBar            = normal;
        }

        // Moving between slots detaches the title bar from the logical tree, which releases its host projection.
        if (titleBarMoves && titleBar is not null)
        {
            titleBar.AttachHost(this);
        }
    }

    private void UpdateFullScreenNativeChromeSession(FullScreenTitleBarMode mode)
    {
        if (mode == FullScreenTitleBarMode.Hidden)
        {
            ReleaseFullScreenNativeChrome();
            return;
        }

        _fullScreenNativeChrome ??= _platformChromeManager?.AttachFullScreenNativeChrome(
            HandleFullScreenNativeChromeChanged);
        ConfigureFullScreenTitleStripFusion();
    }

    private void ReleaseFullScreenNativeChrome()
    {
        _fullScreenNativeChrome?.Dispose();
        _fullScreenNativeChrome = null;
    }

    private void HandleFullScreenNativeChromeChanged()
    {
        // AppKit raises the notification inside its own window move; react once it has returned.
        Dispatcher.UIThread.Post(ApplyFullScreenNativeChromeMetrics, DispatcherPriority.Render);
    }

    private void ApplyFullScreenNativeChromeMetrics()
    {
        if (_fullScreenNativeChrome is not { } nativeChrome)
        {
            return;
        }

        _fullScreenTitleBarRevealController?.SetHold(
            FullScreenTitleBarHolds.NativeChrome,
            nativeChrome.Metrics.IsChromePresented);
        UpdateFullScreenTitleBarGeometry();
    }

    private void ConfigureFullScreenTitleStripFusion()
    {
        if (_fullScreenNativeChrome is not { } nativeChrome)
        {
            return;
        }

        var mode = EffectiveFullScreenTitleBarMode;
        var isEnabled = mode is FullScreenTitleBarMode.Pinned or FullScreenTitleBarMode.Docked ||
                        (mode == FullScreenTitleBarMode.AutoHide && !IsFullScreenTitleBarSuppressed());
        var measuredHeight = _fullScreenTitleBarHost?.SurfaceHeight ?? 0;
        var surfaceHeight  = measuredHeight > 0 ? measuredHeight : TitleBarHeight;
        var buttonOffsetX = OperatingSystem.IsMacOS() && IsSet(MacStandardWindowButtons.OffsetXProperty)
            ? MacStandardWindowButtons.GetOffsetX(this)
            : _macOsCachedButtonX ?? double.NaN;
        nativeChrome.ConfigureTitleStripFusion(new FullScreenTitleStripFusion(isEnabled, surfaceHeight, buttonOffsetX));
    }

    private void UpdateFullScreenTitleBarGeometry()
    {
        var mode          = EffectiveFullScreenTitleBarMode;
        var metrics       = _fullScreenNativeChrome?.Metrics ?? default;
        var surfaceHeight = _fullScreenTitleBarHost?.SurfaceHeight ?? 0;
        var hostsDefault  = TitleBar is not null && ReferenceEquals(FullScreenSlotContent, TitleBar);

        FullScreenTitleBarSurfaceMargin = mode == FullScreenTitleBarMode.Hidden
            ? default
            : new Thickness(0, Math.Max(metrics.Extent, metrics.PersistentInset), 0, 0);
        // Only the persistent platform chrome moves the content; chrome sliding in temporarily pushes the surface
        // over the content instead of reflowing it.
        FullScreenContentInset = mode == FullScreenTitleBarMode.Docked
            ? new Thickness(0, metrics.PersistentInset + surfaceHeight, 0, 0)
            : default;
        FullScreenTitleBarContentPadding = mode != FullScreenTitleBarMode.Hidden && !hostsDefault
            ? metrics.ControlInset
            : default;
        if (WindowState == WindowState.FullScreen && _fullScreenNativeChrome is not null)
        {
            NativeChromeInsets = hostsDefault ? metrics.ControlInset : default;
        }

        UpdateEffectiveContentFrameMargin();
        ConfigureFullScreenTitleStripFusion();
    }

    private void UpdateFullScreenTitleBarReveal(FullScreenTitleBarMode mode)
    {
        if (mode != FullScreenTitleBarMode.AutoHide)
        {
            StopFullScreenTitleBarReveal();
            return;
        }

        var controller = _fullScreenTitleBarRevealController ??=
            new FullScreenTitleBarRevealController(HandleFullScreenTitleBarRevealChanged);
        ApplyFullScreenTitleBarDelays();
        if (controller.IsRunning)
        {
            return;
        }

        controller.Start();
        AddHandler(PointerMovedEvent, HandleFullScreenTitleBarPointerMoved, RoutingStrategies.Tunnel, true);
        AddHandler(PointerPressedEvent, HandleFullScreenTitleBarPointerPressed, RoutingStrategies.Tunnel, true);
        AddHandler(PointerReleasedEvent, HandleFullScreenTitleBarPointerReleased, RoutingStrategies.Tunnel, true);
        // Showing the surface can invalidate hit testing and raise PointerExited with the pointer still in the window.
        _fullScreenTitleBarPointerLeaveSubscription = AvaloniaLocator.Current.GetService<IInputManager>()?
            .Process.Subscribe(HandleFullScreenTitleBarPointerLeave);
        controller.SetSuppressed(IsFullScreenTitleBarSuppressed());
        controller.SetHold(
            FullScreenTitleBarHolds.Focus,
            _fullScreenTitleBarHost?.IsKeyboardFocusWithin == true);
        controller.SetHold(
            FullScreenTitleBarHolds.NativeChrome,
            _fullScreenNativeChrome?.Metrics.IsChromePresented == true);
    }

    private void StopFullScreenTitleBarReveal()
    {
        if (_fullScreenTitleBarRevealController is not { IsRunning: true } controller)
        {
            return;
        }

        RemoveHandler(PointerMovedEvent, HandleFullScreenTitleBarPointerMoved);
        RemoveHandler(PointerPressedEvent, HandleFullScreenTitleBarPointerPressed);
        RemoveHandler(PointerReleasedEvent, HandleFullScreenTitleBarPointerReleased);
        _fullScreenTitleBarPointerLeaveSubscription?.Dispose();
        _fullScreenTitleBarPointerLeaveSubscription = null;
        foreach (var popup in _fullScreenTitleBarPopups)
        {
            popup.Closed -= HandleFullScreenTitleBarPopupClosed;
        }

        _fullScreenTitleBarPopups.Clear();
        _fullScreenTitleBarTouchOrigin      = null;
        _fullScreenTitleBarFocusReturnTarget = null;
        controller.Stop();
    }

    private void ApplyFullScreenTitleBarDelays()
    {
        if (_fullScreenTitleBarRevealController is not { } controller)
        {
            return;
        }

        controller.EnterDelay = TimeSpan.FromMilliseconds(Math.Max(0, FullScreenTitleBarMouseEnterDelay));
        controller.LeaveDelay = TimeSpan.FromMilliseconds(Math.Max(0, FullScreenTitleBarMouseLeaveDelay));
    }

    private void HandleFullScreenTitleBarRevealChanged(bool isOpen)
    {
        if (isOpen)
        {
            var focused = FocusManager?.GetFocusedElement();
            if (!IsInsideFullScreenTitleBar(focused))
            {
                _fullScreenTitleBarFocusReturnTarget = focused;
            }
        }

        SetCurrentValue(IsFullScreenTitleBarOpenProperty, isOpen);
    }

    private void HandleFullScreenTitleBarOpenRequested(bool isOpen)
    {
        if (EffectiveFullScreenTitleBarMode != FullScreenTitleBarMode.AutoHide ||
            _fullScreenTitleBarRevealController is not { IsRunning: true } controller ||
            controller.IsOpen == isOpen)
        {
            return;
        }

        if (isOpen)
        {
            controller.RequestOpen();
        }
        else
        {
            controller.RequestClose();
        }

        // A rejected request (suppressed, or held open by the platform chrome) reports the actual state back.
        if (IsFullScreenTitleBarOpen != controller.IsOpen)
        {
            SetCurrentValue(IsFullScreenTitleBarOpenProperty, controller.IsOpen);
        }
    }

    private bool IsFullScreenTitleBarSuppressed()
    {
        return _drawnChromeOverlaySuppressionCount > 0 ||
               (!IsActive && _fullScreenTitleBarPopups.Count == 0);
    }

    private void UpdateFullScreenTitleBarSuppression()
    {
        _fullScreenTitleBarRevealController?.SetSuppressed(IsFullScreenTitleBarSuppressed());
        ConfigureFullScreenTitleStripFusion();
    }

    private Rect GetFullScreenTitleBarKeepZone()
    {
        var surfaceHeight = _fullScreenTitleBarHost?.SurfaceHeight ?? 0;
        return new Rect(
            0,
            FullScreenTitleBarSurfaceMargin.Top,
            Bounds.Width,
            surfaceHeight + FullScreenTitleBarKeepZoneExtension);
    }

    private bool IsInsideFullScreenTitleBar(object? element)
    {
        return element is Visual visual &&
               _fullScreenTitleBarHost is { } host &&
               (ReferenceEquals(host, visual) || host.IsVisualAncestorOf(visual));
    }

    private void HandleFullScreenTitleBarPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_fullScreenTitleBarRevealController is not { IsRunning: true } controller)
        {
            return;
        }

        var position   = e.GetPosition(this);
        var surfaceTop = FullScreenTitleBarSurfaceMargin.Top;
        if (e.Pointer.Type == PointerType.Mouse)
        {
            controller.SetPointerInTriggerBand(
                position.Y >= surfaceTop && position.Y <= surfaceTop + FullScreenTitleBarTriggerBandHeight);
        }
        else if (_fullScreenTitleBarTouchOrigin is { } origin &&
                 position.Y - origin.Y >= FullScreenTitleBarTouchRevealDistance)
        {
            // A swipe down from the top edge opens the surface until the next press outside it.
            _fullScreenTitleBarTouchOrigin = null;
            controller.RequestOpen();
        }

        controller.SetHold(FullScreenTitleBarHolds.Pointer, GetFullScreenTitleBarKeepZone().Contains(position));
        controller.SetHold(FullScreenTitleBarHolds.Capture, IsInsideFullScreenTitleBar(e.Pointer.Captured));
    }

    private void HandleFullScreenTitleBarPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_fullScreenTitleBarRevealController is not { IsRunning: true } controller)
        {
            return;
        }

        var position   = e.GetPosition(this);
        var surfaceTop = FullScreenTitleBarSurfaceMargin.Top;
        if (e.Pointer.Type != PointerType.Mouse &&
            !controller.IsOpen &&
            position.Y >= surfaceTop &&
            position.Y <= surfaceTop + FullScreenTitleBarTouchTriggerBandHeight)
        {
            _fullScreenTitleBarTouchOrigin = position;
        }

        if (controller.IsOpen &&
            _fullScreenTitleBarPopups.Count == 0 &&
            !GetFullScreenTitleBarKeepZone().Contains(position))
        {
            controller.CloseImmediately();
        }
    }

    private void HandleFullScreenTitleBarPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _fullScreenTitleBarTouchOrigin = null;
        _fullScreenTitleBarRevealController?.SetHold(FullScreenTitleBarHolds.Capture, false);
    }

    private void HandleFullScreenTitleBarPointerLeave(RawInputEventArgs e)
    {
        if (e is not RawPointerEventArgs { Type: RawPointerEventType.LeaveWindow } ||
            !ReferenceEquals(e.Root, this.GetPresentationSource()) ||
            _fullScreenTitleBarRevealController is not { IsRunning: true } controller)
        {
            return;
        }

        controller.SetPointerInTriggerBand(false);
        controller.SetHold(FullScreenTitleBarHolds.Pointer, false);
    }

    private void HandleFullScreenTitleBarDismissRequested(object? sender, EventArgs e)
    {
        if (_fullScreenTitleBarRevealController is not { IsRunning: true } controller)
        {
            return;
        }

        var returnTarget = _fullScreenTitleBarFocusReturnTarget;
        controller.CloseImmediately();
        if (!controller.IsOpen && returnTarget is Visual visual && visual.IsAttachedToVisualTree())
        {
            returnTarget.Focus();
        }
    }

    private static void HandleAnyPopupIsOpenChanged(AvaloniaPopup popup, AvaloniaPropertyChangedEventArgs e)
    {
        if (!e.GetNewValue<bool>() ||
            popup.FindLogicalAncestorOfType<FullScreenTitleBarHost>() is not { TemplatedParent: Window window })
        {
            return;
        }

        window.TrackFullScreenTitleBarPopup(popup);
    }

    private void TrackFullScreenTitleBarPopup(AvaloniaPopup popup)
    {
        if (_fullScreenTitleBarRevealController is not { IsRunning: true } ||
            !_fullScreenTitleBarPopups.Add(popup))
        {
            return;
        }

        popup.Closed += HandleFullScreenTitleBarPopupClosed;
        UpdateFullScreenTitleBarPopupHold();
    }

    private void HandleFullScreenTitleBarPopupClosed(object? sender, EventArgs e)
    {
        if (sender is not AvaloniaPopup popup || !_fullScreenTitleBarPopups.Remove(popup))
        {
            return;
        }

        popup.Closed -= HandleFullScreenTitleBarPopupClosed;
        UpdateFullScreenTitleBarPopupHold();
    }

    private void UpdateFullScreenTitleBarPopupHold()
    {
        _fullScreenTitleBarRevealController?.SetHold(
            FullScreenTitleBarHolds.Popup,
            _fullScreenTitleBarPopups.Count > 0);
        UpdateFullScreenTitleBarSuppression();
    }
}
