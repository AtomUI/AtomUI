using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public enum DotBadgeStatus
{
    Default,
    Success,
    Processing,
    Error,
    Warning
}

public abstract class AbstractDotBadge : Control, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> DotColorProperty =
        AvaloniaProperty.Register<AbstractDotBadge, string?>(
            nameof(DotColor));

    public static readonly StyledProperty<DotBadgeStatus?> StatusProperty =
        AvaloniaProperty.Register<AbstractDotBadge, DotBadgeStatus?>(
            nameof(Status));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractDotBadge, string?>(nameof(Text));

    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<AbstractDotBadge, Control?>(nameof(DecoratedTarget));

    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractDotBadge, Point>(nameof(Offset));

    // Defaults to false: the Desktop dot has no ring. Subclasses that draw one style on the adorner's IsBordered.
    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<AbstractDotBadge, bool>(nameof(IsBordered));

    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<AbstractDotBadge, bool>(nameof(BadgeIsVisible), true);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty
        = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractDotBadge>();

    public string? DotColor
    {
        get => GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    public DotBadgeStatus? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    [Content]
    public Control? DecoratedTarget
    {
        get => GetValue(DecoratedTargetProperty);
        set => SetValue(DecoratedTargetProperty, value);
    }

    public Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    public bool BadgeIsVisible
    {
        get => GetValue(BadgeIsVisibleProperty);
        set => SetValue(BadgeIsVisibleProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    private protected AbstractDotBadgeAdorner? _dotBadgeAdorner;
    private protected AdornerLayer? _adornerLayer;
    private const int MaxAdornerLayerRetryCount = 30;
    private bool _adornerLayerRetryScheduled;
    private int _adornerLayerRetryCount;
    private IDisposable? _motionBinding;
    private AncestorVisibilityTracker? _ancestorVisibilityTracker;
    private IDisposable? _themeSubscription;
    private Panel? _standaloneContentHost;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    static AbstractDotBadge()
    {
        AffectsMeasure<AbstractDotBadge>(DecoratedTargetProperty, TextProperty);
        AffectsRender<AbstractDotBadge>(DotColorProperty, StatusProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractDotBadge>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractDotBadge>(VerticalAlignment.Top);
    }

    // Live ancestor-visibility subscriptions; 0 when the tracker is not active.
    internal int TrackedAncestorCount => _ancestorVisibilityTracker?.SubscriptionCount ?? 0;

    private protected abstract AbstractDotBadgeAdorner CreateDotBadgeAdorner();

    private void PrepareAdorner()
    {
        var dotBadgeAdorner = CreateDotBadgeAdorner();
        if (DecoratedTarget is not null)
        {
            DetachStandaloneAdorner(dotBadgeAdorner);
            AttachChild(DecoratedTarget);
            dotBadgeAdorner.IsAdornerMode = true;
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            // 这里需要抛出异常吗？
            if (_adornerLayer == null)
            {
                ScheduleAdornerLayerRetry();
                return;
            }

            _adornerLayerRetryCount     = 0;
            _adornerLayerRetryScheduled = false;
            dotBadgeAdorner.ApplyToTarget(_adornerLayer, this);
            UpdateAncestorVisibilityTracking();
            SyncLayerAdornerVisibility();
        }
        else
        {
            DetachLayerAdorner();
            dotBadgeAdorner.IsAdornerMode = false;
            AttachStandaloneAdorner(dotBadgeAdorner);
            dotBadgeAdorner.ShowWithoutMotion();
            IsVisible = true;
        }
    }

    // 指示器渲染在跨根的 AdornerLayer 中，祖先仅以 IsVisible=false 隐藏宿主子树时，
    // 控件不触发 detach 清理、Avalonia 也不会隐藏装饰层中的残留视觉，
    // 必须按有效可见性显式同步，否则指示器会以退化布局坐标漂移在页面上。
    private void SyncLayerAdornerVisibility()
    {
        if (_dotBadgeAdorner is { IsAdornerMode: true } && _adornerLayer is not null)
        {
            _dotBadgeAdorner.SetCurrentValue(IsVisibleProperty, IsEffectivelyVisible && BadgeIsVisible);
        }
    }

    private void HideAdorner(bool enableMotion)
    {
        // 这里需要抛出异常吗？
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        if (DecoratedTarget is null)
        {
            if (enableMotion)
            {
                var dotBadgeAdorner = _dotBadgeAdorner;
                dotBadgeAdorner.DetachFromTargetAsync(null,
                    true,
                    () =>
                    {
                        if (_dotBadgeAdorner == dotBadgeAdorner)
                        {
                            DetachStandaloneAdorner(dotBadgeAdorner);
                            IsVisible = false;
                        }
                    });
            }
            else
            {
                DetachStandaloneAdorner(_dotBadgeAdorner);
                _dotBadgeAdorner.DetachFromTargetAsync(null, false);
            }
        }
        else
        {
            if (enableMotion)
            {
                var dotBadgeAdorner = _dotBadgeAdorner;
                var adornerLayer    = _adornerLayer;
                if (adornerLayer is null)
                {
                    dotBadgeAdorner.DetachFromTargetAsync(null,
                        true,
                        () =>
                        {
                            if (_dotBadgeAdorner == dotBadgeAdorner)
                            {
                                DetachStandaloneAdorner(dotBadgeAdorner);
                            }
                        });
                }
                else
                {
                    dotBadgeAdorner.DetachFromTargetAsync(adornerLayer,
                        true,
                        () =>
                        {
                            if (_adornerLayer == adornerLayer)
                            {
                                _adornerLayer = null;
                            }
                        });
                }
            }
            else
            {
                DetachStandaloneAdorner(_dotBadgeAdorner);
                _dotBadgeAdorner.DetachFromTargetAsync(_adornerLayer, false);
                _adornerLayer = null;
            }
        }

        if (!enableMotion)
        {
            if (DecoratedTarget is null)
            {
                IsVisible = false;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        _adornerLayerRetryCount = 0;
        // 目标挂载与指示器可见性无关：BadgeIsVisible 初始为 false 时也必须挂上目标
        SyncDecoratedTargetChild(null);
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }

        UpdateAncestorVisibilityTracking();
        UpdateThemeSubscription();
    }

    // Only a layer adorner can drift when an ancestor is hidden; an inline adorner is a plain child
    // of this control and hides with it, so inline mode needs no subscriptions at all.
    private void UpdateAncestorVisibilityTracking()
    {
        if (DecoratedTarget is not null && this.IsAttachedToVisualTree())
        {
            // The adorner layer's host hides the layer with its adorners, so tracking stops below it.
            var boundary = _adornerLayer?.GetVisualParent();
            if (_ancestorVisibilityTracker is null || !ReferenceEquals(_ancestorVisibilityTracker.Boundary, boundary))
            {
                _ancestorVisibilityTracker?.Dispose();
                _ancestorVisibilityTracker = new AncestorVisibilityTracker(this, SyncLayerAdornerVisibility, boundary);
            }
        }
        else
        {
            _ancestorVisibilityTracker?.Dispose();
            _ancestorVisibilityTracker = null;
        }
    }

    // Named colors (presets and statuses) depend on the theme snapshot; a hex value or no color needs no subscription.
    private void UpdateThemeSubscription()
    {
        if (this.IsAttachedToVisualTree() && BadgeColorUtils.IsThemeDependent(DotColor))
        {
            if (_themeSubscription is null)
            {
                _themeSubscription = s_themeTokenResolver.Subscribe(this, _ => RefreshThemeColor());
                // Subscribing does not report the current snapshot, and a theme switch while detached was
                // missed. A reused adorner still carries the old color, so resolve once on (re)subscription.
                RefreshThemeColor();
            }
        }
        else
        {
            _themeSubscription?.Dispose();
            _themeSubscription = null;
        }
    }

    private void RefreshThemeColor()
    {
        if (_dotBadgeAdorner is not null && DotColor is { } dotColor)
        {
            ConfigureDotColor(dotColor);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
        _ancestorVisibilityTracker?.Dispose();
        _ancestorVisibilityTracker = null;
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        Loaded -= HandleAdornerLayerRetryLoaded;
        _adornerLayerRetryScheduled = false;
        _adornerLayerRetryCount     = 0;
        HideAdorner(false);
    }

    private void ScheduleAdornerLayerRetry()
    {
        if (_adornerLayerRetryScheduled ||
            !this.IsAttachedToVisualTree() ||
            _adornerLayerRetryCount >= MaxAdornerLayerRetryCount)
        {
            return;
        }

        _adornerLayerRetryScheduled = true;
        if (IsLoaded)
        {
            if (_adornerLayerRetryCount == 0)
            {
                Dispatcher.UIThread.Post(RetryPrepareAdorner, DispatcherPriority.Loaded);
            }
            else
            {
                DispatcherTimer.RunOnce(RetryPrepareAdorner, TimeSpan.FromMilliseconds(16));
            }
        }
        else
        {
            Loaded += HandleAdornerLayerRetryLoaded;
        }
    }

    private void HandleAdornerLayerRetryLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= HandleAdornerLayerRetryLoaded;
        Dispatcher.UIThread.Post(RetryPrepareAdorner, DispatcherPriority.Loaded);
    }

    private void RetryPrepareAdorner()
    {
        if (!_adornerLayerRetryScheduled)
        {
            return;
        }

        _adornerLayerRetryScheduled = false;
        if (!this.IsAttachedToVisualTree() ||
            !BadgeIsVisible ||
            DecoratedTarget is null)
        {
            return;
        }

        _adornerLayerRetryCount++;
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer is null)
        {
            ScheduleAdornerLayerRetry();
            return;
        }

        PrepareAdorner();
    }

    private protected virtual void SetupTokenBindings()
    {
        if (_dotBadgeAdorner is not null)
        {
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.StatusProperty] = this[!StatusProperty];
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.TextProperty]   = this[!TextProperty];
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.OffsetProperty] = this[!OffsetProperty];
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.IsBorderedProperty] = this[!IsBorderedProperty];
            _dotBadgeAdorner[!IsMotionEnabledProperty]                = this[!IsMotionEnabledProperty];
        }
    }

    private void AttachChild(Control child)
    {
        if (child.GetVisualParent() == this)
        {
            return;
        }

        child.SetLogicalParent(this);
        VisualChildren.Add(child);
        LogicalChildren.Add(child);
    }

    // standalone 指示器不能直接挂 Control.VisualChildren：普通控件集合缺少 Panel
    // 式的逻辑挂载联动，adorner 子树会停留在样式未应用状态（ControlTheme 的
    // Setter 全部失效、desired=0，宿主布局退化为空白）。经内部 Panel 宿主挂载补齐联动。
    private void AttachStandaloneAdorner(Control adorner)
    {
        if (_standaloneContentHost is null)
        {
            _standaloneContentHost = new Panel();
            AttachChild(_standaloneContentHost);
        }

        if (!_standaloneContentHost.Children.Contains(adorner))
        {
            _standaloneContentHost.Children.Add(adorner);
        }

        // 首次挂载可能发生在宿主 attach 级联内部：逻辑根尚未贯通到本 Panel，
        // 子级收不到 AttachedToLogicalTree，ControlTheme 与模板 Setter 全部
        // 不应用、desired 停留 0。补设逻辑父（SetParent → FindLogicalRoot →
        // ApplyStyling）；不能用视觉重挂——detach 会取消待播的 show 动画，
        // 让指示器停留在 opacity 0。级联内同步设置可能失败，Post 兜底重试。
        if (((StyledElement)adorner).Parent is null)
        {
            var hostPanel = _standaloneContentHost;
            ((ISetLogicalParent)adorner).SetParent(hostPanel);
            if (((StyledElement)adorner).Parent is null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_standaloneContentHost == hostPanel &&
                        hostPanel.Children.Contains(adorner) &&
                        ((StyledElement)adorner).Parent is null)
                    {
                        ((ISetLogicalParent)adorner).SetParent(hostPanel);
                    }
                }, DispatcherPriority.Loaded);
            }
        }
    }

    private void DetachStandaloneAdorner(Control? adorner)
    {
        if (adorner is null)
        {
            return;
        }

        if (_standaloneContentHost is not { } host)
        {
            return;
        }

        host.Children.Remove(adorner);
        if (host.Children.Count == 0)
        {
            DetachChild(host);
            _standaloneContentHost = null;
        }
    }

    private void DetachChild(Control? child)
    {
        if (child is null)
        {
            return;
        }

        if (child.GetVisualParent() == this)
        {
            VisualChildren.Remove(child);
        }

        if (LogicalChildren.Contains(child))
        {
            LogicalChildren.Remove(child);
        }

        child.SetLogicalParent(null);
    }

    private void DetachLayerAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        _dotBadgeAdorner.DetachFromTargetAsync(_adornerLayer, false);
        _adornerLayer = null;
    }

    private void ResetDotBadgeAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        DetachStandaloneAdorner(_dotBadgeAdorner);
        DetachLayerAdorner();
        _dotBadgeAdorner = null;
    }

    // 目标的挂载与替换独立于指示器：指示器未创建或处于隐藏时，旧目标仍要移除、新目标仍要挂上。
    private void SyncDecoratedTargetChild(Control? oldDecoratedTarget)
    {
        if (oldDecoratedTarget is not null &&
            !ReferenceEquals(oldDecoratedTarget, DecoratedTarget) &&
            oldDecoratedTarget.GetVisualParent() == this)
        {
            DetachChild(oldDecoratedTarget);
        }

        if (DecoratedTarget is not null && this.IsAttachedToVisualTree())
        {
            AttachChild(DecoratedTarget);
        }
    }

    private protected virtual void NotifyDecoratedTargetChanged(Control? oldDecoratedTarget = null)
    {
        if (_dotBadgeAdorner is not null)
        {
            DetachChild(oldDecoratedTarget);
            if (DecoratedTarget is null)
            {
                _dotBadgeAdorner.IsAdornerMode = false;
                DetachLayerAdorner();
                if (BadgeIsVisible)
                {
                    AttachStandaloneAdorner(_dotBadgeAdorner);
                }
            }
            else
            {
                DetachStandaloneAdorner(_dotBadgeAdorner);
                _dotBadgeAdorner.IsAdornerMode = true;
                AttachChild(DecoratedTarget);
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty ||
            change.Property == BadgeIsVisibleProperty)
        {
            var badgeIsVisible = change.GetNewValue<bool>();
            if (badgeIsVisible)
            {
                PrepareAdorner();
            }
            else
            {
                HideAdorner(IsMotionEnabled && IsLoaded);
            }
        }

        if (change.Property == DecoratedTargetProperty)
        {
            SyncDecoratedTargetChild(change.GetOldValue<Control?>());
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DecoratedTargetProperty)
            {
                var oldDecoratedTarget = change.GetOldValue<Control?>();
                DetachChild(oldDecoratedTarget);
                if ((oldDecoratedTarget is null) != (DecoratedTarget is null))
                {
                    ResetDotBadgeAdorner();
                }
                NotifyDecoratedTargetChanged();
                if (BadgeIsVisible)
                {
                    PrepareAdorner();
                }

                UpdateAncestorVisibilityTracking();
            }

            if (change.Property == DotColorProperty)
            {
                ConfigureDotColor(change.GetNewValue<string>());
                UpdateThemeSubscription();
            }
        }
    }

    private protected virtual void ConfigureDotColor(string colorStr)
    {
        if (_dotBadgeAdorner is not null)
        {
            _dotBadgeAdorner.BadgeDotColor = BadgeColorUtils.CalculateColor(this, colorStr);
        }
    }
    
}
