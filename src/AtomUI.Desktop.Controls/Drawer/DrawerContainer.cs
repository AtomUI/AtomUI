using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class DrawerContainer : ContentControl
{
    #region 内部属性定义

    internal static readonly DirectProperty<DrawerContainer, DrawerPlacement> PlacementProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, DrawerPlacement>(nameof(Placement),
            o => o.Placement,
            (o, v) => o.Placement = v);

    internal static readonly DirectProperty<DrawerContainer, bool> IsShowCloseButtonProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, bool>(nameof(IsShowCloseButton),
            o => o.IsShowCloseButton,
            (o, v) => o.IsShowCloseButton = v);

    internal static readonly DirectProperty<DrawerContainer, bool> IsShowMaskProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, bool>(nameof(IsShowMask),
            o => o.IsShowMask,
            (o, v) => o.IsShowMask = v);

    internal static readonly DirectProperty<DrawerContainer, string> TitleProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, string>(nameof(Title),
            o => o.Title,
            (o, v) => o.Title = v);

    internal static readonly DirectProperty<DrawerContainer, object?> FooterProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, object?>(nameof(Footer),
            o => o.Footer,
            (o, v) => o.Footer = v);

    internal static readonly DirectProperty<DrawerContainer, IDataTemplate?> FooterTemplateProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, IDataTemplate?>(nameof(FooterTemplate),
            o => o.FooterTemplate,
            (o, v) => o.FooterTemplate = v);

    internal static readonly DirectProperty<DrawerContainer, Thickness> ContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, Thickness>(nameof(ContentPadding),
            o => o.ContentPadding,
            (o, v) => o.ContentPadding = v);

    internal static readonly DirectProperty<DrawerContainer, object?> ExtraProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, object?>(nameof(Extra),
            o => o.Extra,
            (o, v) => o.Extra = v);

    internal static readonly DirectProperty<DrawerContainer, IDataTemplate?> ExtraTemplateProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, IDataTemplate?>(nameof(ExtraTemplate),
            o => o.ExtraTemplate,
            (o, v) => o.ExtraTemplate = v);

    internal static readonly DirectProperty<DrawerContainer, double> DialogSizeProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, double>(nameof(DialogSize),
            o => o.DialogSize,
            (o, v) => o.DialogSize = v);

    internal static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DrawerContainer>();

    internal static readonly DirectProperty<DrawerContainer, bool> IsCloseOnMaskClickProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, bool>(nameof(IsCloseOnMaskClick),
            o => o.IsCloseOnMaskClick,
            (o, v) => o.IsCloseOnMaskClick = v);
    
    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<DrawerContainer>();
    
    internal static readonly DirectProperty<DrawerContainer, double> PushOffsetPercentProperty =
        AvaloniaProperty.RegisterDirect<DrawerContainer, double>(nameof(PushOffsetPercent),
            o => o.PushOffsetPercent,
            (o, v) => o.PushOffsetPercent = v);
    
    private DrawerPlacement _placement = DrawerPlacement.Right;

    internal DrawerPlacement Placement
    {
        get => _placement;
        set
        {
            if (SetAndRaise(PlacementProperty, ref _placement, value))
            {
                RefreshActiveChildDrawerPushTransform();
            }
        }
    }

    private bool _isShowMask;

    internal bool IsShowMask
    {
        get => _isShowMask;
        set => SetAndRaise(IsShowMaskProperty, ref _isShowMask, value);
    }

    private bool _isShowCloseButton = true;

    internal bool IsShowCloseButton
    {
        get => _isShowCloseButton;
        set => SetAndRaise(IsShowCloseButtonProperty, ref _isShowCloseButton, value);
    }

    private string _title = string.Empty;

    internal string Title
    {
        get => _title;
        set => SetAndRaise(TitleProperty, ref _title, value);
    }

    private object? _footer;

    internal object? Footer
    {
        get => _footer;
        set => SetAndRaise(FooterProperty, ref _footer, value);
    }

    private IDataTemplate? _footerTemplate;

    internal IDataTemplate? FooterTemplate
    {
        get => _footerTemplate;
        set => SetAndRaise(FooterTemplateProperty, ref _footerTemplate, value);
    }

    private Thickness _contentPadding;

    internal Thickness ContentPadding
    {
        get => _contentPadding;
        set => SetAndRaise(ContentPaddingProperty, ref _contentPadding, value);
    }

    private object? _extra;

    internal object? Extra
    {
        get => _extra;
        set => SetAndRaise(ExtraProperty, ref _extra, value);
    }

    private IDataTemplate? _extraTemplate;

    internal IDataTemplate? ExtraTemplate
    {
        get => _extraTemplate;
        set => SetAndRaise(ExtraTemplateProperty, ref _extraTemplate, value);
    }

    private double _dialogSize;

    internal double DialogSize
    {
        get => _dialogSize;
        set => SetAndRaise(DialogSizeProperty, ref _dialogSize, value);
    }

    internal bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    private bool _closeWhenClickOnMask;

    internal bool IsCloseOnMaskClick
    {
        get => _closeWhenClickOnMask;
        set => SetAndRaise(IsCloseOnMaskClickProperty, ref _closeWhenClickOnMask, value);
    }
    
    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private double _pushOffsetPercent;

    internal double PushOffsetPercent
    {
        get => _pushOffsetPercent;
        set
        {
            if (SetAndRaise(PushOffsetPercentProperty, ref _pushOffsetPercent, value))
            {
                RefreshActiveChildDrawerPushTransform();
            }
        }
    }

    #endregion

    internal WeakReference<Drawer>? Drawer { get; set; }
    
    private Border? _surface;
    private Border? _mask;
    private CompositeDisposable? _entryPreparation;
    private DrawerInfoContainer? _infoContainer;
    private ITransform? _originInfoContainerTransform;
    private WeakReference<Drawer>? _activeChildDrawer;
    private bool _isChildDrawerPushed;
    private bool _closeAnimating;
    private bool _presentationPending;
    private int _lifecycleVersion;
    private int _pushTransformVersion;
    private CompositeDisposable? _drawerBindingDisposables;
    private IDisposable? _hostMarginSubscription;
    private IDisposable? _drawnChromeOverlaySuppression;

    internal void BindToDrawer(Drawer drawer)
    {
        _drawerBindingDisposables?.Dispose();
        _drawerBindingDisposables = new CompositeDisposable
        {
            Bind(DataContextProperty, drawer[!DataContextProperty]),
            Bind(ContentProperty, drawer[!AtomUI.Desktop.Controls.Drawer.ContentProperty]),
            Bind(ContentTemplateProperty, drawer[!AtomUI.Desktop.Controls.Drawer.ContentTemplateProperty]),
            Bind(ContentPaddingProperty, drawer[!AtomUI.Desktop.Controls.Drawer.ContentPaddingProperty]),
            Bind(FooterProperty, drawer[!AtomUI.Desktop.Controls.Drawer.FooterProperty]),
            Bind(FooterTemplateProperty, drawer[!AtomUI.Desktop.Controls.Drawer.FooterTemplateProperty]),
            Bind(ExtraProperty, drawer[!AtomUI.Desktop.Controls.Drawer.ExtraProperty]),
            Bind(ExtraTemplateProperty, drawer[!AtomUI.Desktop.Controls.Drawer.ExtraTemplateProperty]),
            Bind(DialogSizeProperty, drawer[!AtomUI.Desktop.Controls.Drawer.EffectiveDialogSizeProperty]),
            Bind(PlacementProperty, drawer[!AtomUI.Desktop.Controls.Drawer.PlacementProperty]),
            Bind(TitleProperty, drawer[!AtomUI.Desktop.Controls.Drawer.TitleProperty]),
            Bind(IsShowMaskProperty, drawer[!AtomUI.Desktop.Controls.Drawer.IsShowMaskProperty]),
            Bind(IsShowCloseButtonProperty, drawer[!AtomUI.Desktop.Controls.Drawer.IsShowCloseButtonProperty]),
            Bind(IsMotionEnabledProperty, drawer[!AtomUI.Desktop.Controls.Drawer.IsMotionEnabledProperty]),
            Bind(IsCloseOnMaskClickProperty, drawer[!AtomUI.Desktop.Controls.Drawer.IsCloseOnMaskClickProperty]),
            Bind(PushOffsetPercentProperty, drawer[!AtomUI.Desktop.Controls.Drawer.PushOffsetPercentProperty]),
            Bind(ThemeVariantScope.ActualThemeVariantProperty,
                drawer.GetBindingObservable(ThemeVariantScope.ActualThemeVariantProperty))
        };
    }

    internal void Open(ScopeAwareAdornerLayer layer)
    {
        if (Drawer is null || !Drawer.TryGetTarget(out var drawer))
        {
            return;
        }
        var initialize = !this.IsAttachedToVisualTree();
        var version = ++_lifecycleVersion;
        _closeAnimating = false;
        if (initialize)
        {
            PrepareOpenVisualState(drawer.IsMotionEnabled);
        }
        ScopeAwareAdornerLayer.SetAdornedElement(this, drawer.OpenOn);
        ConfigureHostMargin(drawer.OpenOn);
        AttachToLayer(layer);
        ApplyTemplate();
        if (initialize)
        {
            PrepareOpenVisualState(drawer.IsMotionEnabled);
        }
        SchedulePresentation(version, drawer, layer, true);
    }

    internal void UpdateOpenOn(Control? openOn, ScopeAwareAdornerLayer? layer)
    {
        ScopeAwareAdornerLayer.SetAdornedElement(this, openOn);
        ConfigureHostMargin(openOn);
        if (layer is not null)
        {
            AttachToLayer(layer);
        }
    }

    private void PrepareOpenVisualState(bool isMotionEnabled)
    {
        ClearValue(BackgroundProperty);
        if (!isMotionEnabled)
        {
            ClearEntryPreparation();
            ResetPresentation();
            return;
        }
        if (_surface is null || _entryPreparation is not null)
        {
            return;
        }
        _entryPreparation = new CompositeDisposable
        {
            _surface.SetValue(OpacityProperty, 0, Avalonia.Data.BindingPriority.Animation) ?? Disposable.Empty
        };
        if (_mask is { } mask)
        {
            _entryPreparation.Add(mask.SetValue(OpacityProperty, 0, Avalonia.Data.BindingPriority.Animation) ?? Disposable.Empty);
        }
    }

    internal void Close(ScopeAwareAdornerLayer layer)
    {
        if (Drawer is null || !Drawer.TryGetTarget(out var drawer) || _closeAnimating)
        {
            return;
        }
        var version = ++_lifecycleVersion;
        _closeAnimating = true;
        ClearEntryPreparation();
        SchedulePresentation(version, drawer, layer, false);
    }

    private void SchedulePresentation(int version, Drawer drawer, ScopeAwareAdornerLayer layer, bool opening)
    {
        if (version != _lifecycleVersion)
        {
            return;
        }
        _presentationPending = true;
        var surface = _surface;
        var mask = _mask;
        Dispatcher.Post(() => _ = PlayPresentationAsync(version, surface, mask, drawer, layer, opening),
            Avalonia.Threading.DispatcherPriority.Loaded);
    }

    private async Task PlayPresentationAsync(int version, Border? surface, Border? mask,
        Drawer drawer, ScopeAwareAdornerLayer layer, bool opening)
    {
        if (!IsCurrentPresentation(version, surface, drawer, opening))
        {
            return;
        }
        try
        {
            if (surface is not null && IsMotionEnabled)
            {
                UpdateLayout();
                Task<MotionResult> surfacePlayback;
                Task<MotionResult>? maskPlayback = null;
                var preparation = _entryPreparation;
                _entryPreparation = null;
                try
                {
                    surfacePlayback = new DrawerPanelMotion(Placement, opening)
                        .PlayAsync(surface, MotionDuration, CancellationToken.None);
                    if (mask is not null && IsShowMask && IsCurrentPresentation(version, surface, drawer, opening))
                    {
                        Motion maskMotion = opening ? new FadeInMotion() : new FadeOutMotion();
                        maskMotion.Easing = MotionCurves.Ease;
                        maskPlayback = maskMotion.PlayAsync(mask, MotionDuration, CancellationToken.None);
                    }
                }
                finally
                {
                    preparation?.Dispose();
                }
                if (maskPlayback is not null)
                {
                    await Task.WhenAll(surfacePlayback, maskPlayback);
                }
                var surfaceResult = await surfacePlayback;
                var maskResult = maskPlayback is not null ? await maskPlayback : (MotionResult?)null;
                if (!IsCurrentPresentation(version, surface, drawer, opening) ||
                    !CanComplete(surface, surfaceResult) ||
                    (maskResult is { } result && !CanComplete(mask!, result)))
                {
                    return;
                }
                if (surfaceResult.Outcome == MotionOutcome.NotReady || maskResult?.Outcome == MotionOutcome.NotReady)
                {
                    ResetPresentation();
                }
            }
            else
            {
                ClearEntryPreparation();
                ResetPresentation();
            }
        }
        catch (Exception exception)
        {
            Avalonia.Logging.Logger.TryGet(Avalonia.Logging.LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                this, "Drawer motion failed: {Exception}", exception);
            if (!IsCurrentPresentation(version, surface, drawer, opening))
            {
                return;
            }
            ClearEntryPreparation();
            ResetPresentation();
        }
        if (!IsCurrentPresentation(version, surface, drawer, opening))
        {
            return;
        }
        CompletePresentation(drawer, layer, opening);
    }

    private void CompletePresentation(Drawer drawer, ScopeAwareAdornerLayer layer, bool opening)
    {
        _presentationPending = false;
        if (opening)
        {
            drawer.NotifyOpened();
        }
        else
        {
            _closeAnimating = false;
            DetachFromLayer(layer);
            drawer.NotifyClosed();
        }
    }

    private bool IsCurrentPresentation(int version, Border? surface, Drawer drawer, bool opening) =>
        version == _lifecycleVersion && ReferenceEquals(surface, _surface) &&
        Drawer is not null && Drawer.TryGetTarget(out var currentDrawer) && ReferenceEquals(currentDrawer, drawer) &&
        drawer.IsOpen == opening && this.IsAttachedToVisualTree();

    private static bool CanComplete(Control target, MotionResult result) =>
        result.Outcome == MotionOutcome.NotReady || (result.IsCompleted && Motion.IsCurrent(target, result));

    private void ClearEntryPreparation()
    {
        var preparation = _entryPreparation;
        _entryPreparation = null;
        preparation?.Dispose();
    }

    private void ResetPresentation()
    {
        var version = _lifecycleVersion;
        var surface = _surface;
        var mask = _mask;
        List<Exception>? failures = null;
        if (surface is not null)
        {
            TryCleanup(() => MotionPlayback.CancelAndReset(surface), ref failures);
        }
        if (mask is not null && version == _lifecycleVersion)
        {
            TryCleanup(() => MotionPlayback.CancelAndReset(mask), ref failures);
        }
        if (failures is not null)
        {
            throw new AggregateException("Drawer presentation cleanup failed.", failures);
        }
    }

    internal void Release()
    {
        ++_lifecycleVersion;
        ++_pushTransformVersion;
        _closeAnimating = false;
        _presentationPending = false;
        var preparation = _entryPreparation;
        var surface = _surface;
        var mask = _mask;
        var bindings = _drawerBindingDisposables;
        var hostMargin = _hostMarginSubscription;
        var overlaySuppression = _drawnChromeOverlaySuppression;
        var infoContainer = _infoContainer;
        _entryPreparation = null;
        _surface = null;
        _mask = null;
        _drawerBindingDisposables = null;
        _hostMarginSubscription = null;
        _drawnChromeOverlaySuppression = null;
        _infoContainer = null;
        _originInfoContainerTransform = null;
        _activeChildDrawer            = null;
        _isChildDrawerPushed          = false;
        Drawer                        = null;

        List<Exception>? failures = null;
        DisposeOwned(preparation, ref failures);
        if (surface is not null)
        {
            TryCleanup(() => MotionPlayback.CancelAndReset(surface), ref failures);
        }
        if (mask is not null)
        {
            TryCleanup(() => MotionPlayback.CancelAndReset(mask), ref failures);
        }
        DisposeOwned(bindings, ref failures);
        TryCleanup(() => hostMargin?.Dispose(), ref failures);
        TryCleanup(() => overlaySuppression?.Dispose(), ref failures);
        TryCleanup(() => DetachFromLayer(null), ref failures);
        if (infoContainer is not null)
        {
            TryCleanup(() => infoContainer.CloseRequested -= HandleCloseRequested, ref failures);
        }
        ReleaseTemplateContent(infoContainer, ref failures);
        if (failures is not null)
        {
            throw new AggregateException("Drawer release cleanup failed.", failures);
        }
    }

    private void ReleaseTemplateContent(DrawerInfoContainer? infoContainer, ref List<Exception>? failures)
    {
        TryCleanup(() => ClearValue(DataContextProperty), ref failures);
        TryCleanup(() => ClearValue(ContentProperty), ref failures);
        TryCleanup(() => ClearValue(ContentTemplateProperty), ref failures);
        TryCleanup(() => ClearValue(ContentPaddingProperty), ref failures);
        TryCleanup(() => ClearValue(FooterProperty), ref failures);
        TryCleanup(() => ClearValue(FooterTemplateProperty), ref failures);
        TryCleanup(() => ClearValue(ExtraProperty), ref failures);
        TryCleanup(() => ClearValue(ExtraTemplateProperty), ref failures);
        TryCleanup(() => ClearValue(DialogSizeProperty), ref failures);
        TryCleanup(() => ClearValue(PlacementProperty), ref failures);
        TryCleanup(() => ClearValue(TitleProperty), ref failures);
        TryCleanup(() => ClearValue(IsShowMaskProperty), ref failures);
        TryCleanup(() => ClearValue(IsShowCloseButtonProperty), ref failures);
        TryCleanup(() => ClearValue(IsMotionEnabledProperty), ref failures);
        TryCleanup(() => ClearValue(IsCloseOnMaskClickProperty), ref failures);
        TryCleanup(() => ClearValue(PushOffsetPercentProperty), ref failures);

        if (infoContainer == null)
        {
            return;
        }

        TryCleanup(() => infoContainer.Content         = null, ref failures);
        TryCleanup(() => infoContainer.ContentTemplate = null, ref failures);
        TryCleanup(() => infoContainer.Footer          = null, ref failures);
        TryCleanup(() => infoContainer.FooterTemplate  = null, ref failures);
        TryCleanup(() => infoContainer.Extra           = null, ref failures);
        TryCleanup(() => infoContainer.ExtraTemplate   = null, ref failures);
    }

    private void AttachToLayer(ScopeAwareAdornerLayer layer)
    {
        if (this.GetVisualParent() is Panel currentParent)
        {
            if (ReferenceEquals(currentParent, layer))
            {
                return;
            }

            currentParent.Children.Remove(this);
        }

        // 语义部件契约：容器必须逻辑挂到 Drawer owner 下，owner 嵌套的生成
        // Semantic Style 才能命中容器子树。必须在 Children.Add 之前设置：
        // Panel 收集子项不会覆盖已显式设置的逻辑父（先例：ImagePreviewer 的 OverlayHost）。
        if (Drawer is not null && Drawer.TryGetTarget(out var drawer))
        {
            ((ISetLogicalParent)this).SetParent(drawer);
        }

        if (!layer.Children.Contains(this))
        {
            layer.Children.Add(this);
        }
    }

    private void DetachFromLayer(ScopeAwareAdornerLayer? layer)
    {
        List<Exception>? failures = null;
        if (this.GetVisualParent() is Panel currentParent)
        {
            TryCleanup(() => currentParent.Children.Remove(this), ref failures);
        }
        else
        {
            TryCleanup(() => layer?.Children.Remove(this), ref failures);
        }

        TryCleanup(() => ((ISetLogicalParent)this).SetParent(null), ref failures);
        TryCleanup(() => ScopeAwareAdornerLayer.SetAdornedElement(this, null), ref failures);
        TryCleanup(ClearHostMargin, ref failures);
        if (failures is not null)
        {
            throw new AggregateException("Drawer host detachment failed.", failures);
        }
    }

    private void ConfigureHostMargin(Control? host)
    {
        _hostMarginSubscription?.Dispose();
        _drawnChromeOverlaySuppression?.Dispose();
        _drawnChromeOverlaySuppression = host is Window window
            ? window.SuppressDrawnChromeOverlay()
            : null;
        _hostMarginSubscription = host is null
            ? null
            : TopLevelMarginBinder.BindWindowFrameGeometry(
                host,
                ApplyHostGeometry);

        if (host is null)
        {
            ApplyHostGeometry(default, default);
        }
    }

    private void ApplyHostGeometry(
        Thickness margin,
        CornerRadius cornerRadius)
    {
        Margin       = margin;
        CornerRadius = cornerRadius;
    }

    private void ClearHostMargin()
    {
        var subscription = _hostMarginSubscription;
        var suppression = _drawnChromeOverlaySuppression;
        _hostMarginSubscription = null;
        _drawnChromeOverlaySuppression = null;
        List<Exception>? failures = null;
        TryCleanup(() => subscription?.Dispose(), ref failures);
        TryCleanup(() => suppression?.Dispose(), ref failures);
        TryCleanup(() => Margin = default, ref failures);
        TryCleanup(() => CornerRadius = default, ref failures);
        if (failures is not null)
        {
            throw new AggregateException("Drawer host geometry cleanup failed.", failures);
        }
    }

    private static void DisposeOwned(CompositeDisposable? subscriptions, ref List<Exception>? failures)
    {
        if (subscriptions is null)
        {
            return;
        }
        foreach (var subscription in subscriptions.ToArray())
        {
            // Remove retires the handle before invoking it; one throwing binding
            // must not prevent the remaining owned bindings from being released.
            TryCleanup(() => subscriptions.Remove(subscription), ref failures);
        }
        TryCleanup(subscriptions.Dispose, ref failures);
    }

    private static void TryCleanup(Action cleanup, ref List<Exception>? failures)
    {
        try
        {
            cleanup();
        }
        catch (Exception exception)
        {
            (failures ??= []).Add(exception);
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (IsCloseOnMaskClick && e.InitialPressMouseButton == MouseButton.Left)
        {
            if (Drawer != null && Drawer.TryGetTarget(out var drawer))
            {
                if (drawer.IsPinnedOpen)
                {
                    return;
                }

                drawer.IsOpen = false;
            }
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        var previousSurface = _surface;
        var previousMask = _mask;
        base.OnApplyTemplate(e);
        _surface = e.NameScope.Find<Border>("PART_Surface");
        _mask = e.NameScope.Find<Border>("PART_Mask");
        
        if (_infoContainer != null)
        {
            _infoContainer.CloseRequested -= HandleCloseRequested;
        }

        _infoContainer = e.NameScope.Find<DrawerInfoContainer>("PART_InfoContainer");
        if (_infoContainer != null)
        {
            _infoContainer.CloseRequested += HandleCloseRequested;
        }
        RefreshActiveChildDrawerPushTransform();

        if (previousSurface is not null && !ReferenceEquals(previousSurface, _surface))
        {
            var version = ++_lifecycleVersion;
            ClearEntryPreparation();
            MotionPlayback.CancelAndReset(previousSurface);
            if (previousMask is not null)
            {
                MotionPlayback.CancelAndReset(previousMask);
            }
            if (version == _lifecycleVersion && _presentationPending && this.IsAttachedToVisualTree() &&
                Drawer is not null && Drawer.TryGetTarget(out var drawer) &&
                this.GetVisualParent() is ScopeAwareAdornerLayer layer)
            {
                _closeAnimating = !drawer.IsOpen;
                if (drawer.IsOpen)
                {
                    PrepareOpenVisualState(IsMotionEnabled);
                }
                SchedulePresentation(version, drawer, layer, drawer.IsOpen);
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsMotionEnabledProperty && !IsMotionEnabled && _presentationPending &&
            this.IsAttachedToVisualTree() && Drawer is not null && Drawer.TryGetTarget(out var drawer) &&
            this.GetVisualParent() is ScopeAwareAdornerLayer layer)
        {
            var version = ++_lifecycleVersion;
            var surface = _surface;
            var opening = drawer.IsOpen;
            ClearEntryPreparation();
            ResetPresentation();
            if (IsCurrentPresentation(version, surface, drawer, opening))
            {
                CompletePresentation(drawer, layer, opening);
            }
        }
    }

    private void HandleCloseRequested(object? sender, EventArgs e)
    {
        if (Drawer != null && Drawer.TryGetTarget(out var drawer))
        {
            if (drawer.IsPinnedOpen)
            {
                return;
            }

            drawer.IsOpen = false;
        }
    }

    internal void NotifyChildDrawerAboutToOpen(Drawer childDrawer)
    {
        _activeChildDrawer = new WeakReference<Drawer>(childDrawer);
        RefreshActiveChildDrawerPushTransform();
    }

    internal void CloseActiveChildDrawer()
    {
        if (_activeChildDrawer == null)
        {
            return;
        }

        if (!_activeChildDrawer.TryGetTarget(out var childDrawer) || !childDrawer.IsOpen)
        {
            _activeChildDrawer = null;
            RestoreChildDrawerPushTransform();
            return;
        }

        childDrawer.IsOpen = false;
        if (IsActiveChildDrawer(childDrawer))
        {
            _activeChildDrawer = null;
            RestoreChildDrawerPushTransform();
        }
    }

    internal void NotifyChildDrawerAboutToClose(Drawer childDrawer)
    {
        if (IsActiveChildDrawer(childDrawer))
        {
            _activeChildDrawer = null;
            RestoreChildDrawerPushTransform();
        }
    }

    internal void NotifyChildDrawerPlacementChanged(Drawer childDrawer)
    {
        if (IsActiveChildDrawer(childDrawer))
        {
            RefreshActiveChildDrawerPushTransform();
        }
    }

    private bool IsActiveChildDrawer(Drawer childDrawer)
    {
        return _activeChildDrawer != null &&
               _activeChildDrawer.TryGetTarget(out var activeChildDrawer) &&
               ReferenceEquals(activeChildDrawer, childDrawer);
    }

    private void RefreshActiveChildDrawerPushTransform()
    {
        if (_activeChildDrawer == null || !_activeChildDrawer.TryGetTarget(out var childDrawer))
        {
            _activeChildDrawer = null;
            RestoreChildDrawerPushTransform();
            return;
        }

        if (_infoContainer == null)
        {
            return;
        }

        if (Placement != childDrawer.Placement)
        {
            // Parent and child Placement bindings can settle in separate notifications.
            // Keep the current push for this dispatcher turn so no zero-offset frame is shown.
            ScheduleChildDrawerPushRestore(childDrawer);
            return;
        }

        ++_pushTransformVersion;
        if (!_isChildDrawerPushed)
        {
            _originInfoContainerTransform = _infoContainer.RenderTransform;
            _isChildDrawerPushed          = true;
        }

        double offsetX = 0d;
        double offsetY = 0d;

        if (Placement == DrawerPlacement.Left)
        {
            offsetX = DesiredSize.Width * PushOffsetPercent;
        }
        else if (Placement == DrawerPlacement.Right)
        {
            offsetX = -DesiredSize.Width * PushOffsetPercent;
        }
        else if (Placement == DrawerPlacement.Top)
        {
            offsetY = DesiredSize.Height * PushOffsetPercent;
        }
        else
        {
            offsetY = -DesiredSize.Height * PushOffsetPercent;
        }

        var builder = new TransformOperations.Builder(1);
        builder.AppendTranslate(offsetX, offsetY);
        _infoContainer.RenderTransform = builder.Build();
    }

    private void ScheduleChildDrawerPushRestore(Drawer childDrawer)
    {
        var pushTransformVersion = ++_pushTransformVersion;
        Dispatcher.Post(() =>
        {
            if (pushTransformVersion != _pushTransformVersion ||
                _infoContainer == null ||
                !IsActiveChildDrawer(childDrawer) ||
                Placement == childDrawer.Placement)
            {
                return;
            }

            RestoreChildDrawerPushTransform();
        });
    }

    private void RestoreChildDrawerPushTransform()
    {
        ++_pushTransformVersion;
        if (_infoContainer != null && _isChildDrawerPushed)
        {
            _infoContainer.RenderTransform = _originInfoContainerTransform;
        }

        _originInfoContainerTransform = null;
        _isChildDrawerPushed          = false;
    }
}
