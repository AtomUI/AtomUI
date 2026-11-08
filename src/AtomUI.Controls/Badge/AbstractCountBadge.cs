using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public enum CountBadgeSize
{
    Default,
    Small
}

public abstract class AbstractCountBadge : Control, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<AbstractCountBadge, string?>(
            nameof(BadgeColor));

    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<AbstractCountBadge, int>(nameof(Count),
            coerce: (o, v) => Math.Max(0, v));

    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<AbstractCountBadge, Control?>(nameof(DecoratedTarget));

    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractCountBadge, Point>(nameof(Offset));

    public static readonly StyledProperty<int> OverflowCountProperty =
        AvaloniaProperty.Register<AbstractCountBadge, int>(nameof(OverflowCount), 99,
            coerce: (o, v) => Math.Max(0, v));

    public static readonly StyledProperty<bool> IsZeroVisibleProperty =
        AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(IsZeroVisible));

    public static readonly StyledProperty<CountBadgeSize> SizeProperty =
        AvaloniaProperty.Register<AbstractCountBadge, CountBadgeSize>(nameof(Size));

    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<AbstractCountBadge, object?>(nameof(Content));

    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(IsBordered), true);

    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(BadgeIsVisible), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCountBadge>();

    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
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

    public int OverflowCount
    {
        get => GetValue(OverflowCountProperty);
        set => SetValue(OverflowCountProperty, value);
    }

    public bool IsZeroVisible
    {
        get => GetValue(IsZeroVisibleProperty);
        set => SetValue(IsZeroVisibleProperty, value);
    }

    public CountBadgeSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
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

    private protected AbstractCountBadgeAdorner? _badgeAdorner;
    private protected AdornerLayer? _adornerLayer;
    private const int MaxAdornerLayerRetryCount = 30;
    private bool _adornerLayerRetryScheduled;
    private int _adornerLayerRetryCount;
    private IDisposable? _motionBinding;
    private AncestorVisibilityTracker? _ancestorVisibilityTracker;
    private IDisposable? _themeSubscription;
    private Panel? _standaloneContentHost;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    static AbstractCountBadge()
    {
        AffectsMeasure<AbstractCountBadge>(DecoratedTargetProperty,
            ContentProperty,
            CountProperty,
            OverflowCountProperty,
            SizeProperty);
        AffectsRender<AbstractCountBadge>(BadgeColorProperty, OffsetProperty);
        // 装饰型控件必须包裹目标而不是拉伸占满父槽（与 AbstractDotBadge 一致）：
        // 拉伸会把指示器锚点和 root 语义部件标记拖到父槽右缘。
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractCountBadge>(Avalonia.Layout.HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractCountBadge>(Avalonia.Layout.VerticalAlignment.Top);
    }

    // Live ancestor-visibility subscriptions; 0 when the tracker is not active.
    internal int TrackedAncestorCount => _ancestorVisibilityTracker?.SubscriptionCount ?? 0;

    private protected abstract AbstractCountBadgeAdorner CreateBadgeAdorner();

    private void PrepareAdorner()
    {
        var badgeAdorner = CreateBadgeAdorner();
        if (DecoratedTarget is not null)
        {
            DetachStandaloneAdorner(badgeAdorner);
            AttachChild(DecoratedTarget);
            badgeAdorner.IsAdornerMode = true;
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            // 这里需要抛出异常吗？
            if (_adornerLayer == null)
            {
                ScheduleAdornerLayerRetry();
                return;
            }

            _adornerLayerRetryCount     = 0;
            _adornerLayerRetryScheduled = false;
            badgeAdorner.ApplyToTarget(_adornerLayer, this);
            UpdateAncestorVisibilityTracking();
            SyncLayerAdornerVisibility();
        }
        else
        {
            DetachLayerAdorner();
            badgeAdorner.IsAdornerMode = false;
            AttachStandaloneAdorner(badgeAdorner);
            badgeAdorner.ApplyToTarget(null, this, () => IsVisible = true);
        }
    }

    // 指示器渲染在跨根的 AdornerLayer 中，祖先仅以 IsVisible=false 隐藏宿主子树时，
    // 控件不触发 detach 清理、Avalonia 也不会隐藏装饰层中的残留视觉，
    // 必须按有效可见性显式同步，否则指示器会以退化布局坐标漂移在页面上。
    private void SyncLayerAdornerVisibility()
    {
        if (_badgeAdorner is { IsAdornerMode: true } && _adornerLayer is not null)
        {
            _badgeAdorner.SetCurrentValue(IsVisibleProperty, IsEffectivelyVisible && BadgeIsVisible);
        }
    }

    private void HideAdorner(bool enableMotion)
    {
        // 这里需要抛出异常吗？
        if (_badgeAdorner is null)
        {
            return;
        }

        if (DecoratedTarget is null)
        {
            if (enableMotion)
            {
                var badgeAdorner = _badgeAdorner;
                badgeAdorner.DetachFromTarget(null,
                    true,
                    () =>
                    {
                        if (_badgeAdorner == badgeAdorner)
                        {
                            DetachStandaloneAdorner(badgeAdorner);
                            IsVisible = false;
                        }
                    });
            }
            else
            {
                DetachStandaloneAdorner(_badgeAdorner);
                _badgeAdorner.DetachFromTarget(null, false);
            }
        }
        else
        {
            if (enableMotion)
            {
                var badgeAdorner = _badgeAdorner;
                var adornerLayer = _adornerLayer;
                if (adornerLayer is null)
                {
                    badgeAdorner.DetachFromTarget(null,
                        true,
                        () =>
                        {
                            if (_badgeAdorner == badgeAdorner)
                            {
                                DetachStandaloneAdorner(badgeAdorner);
                            }
                        });
                }
                else
                {
                    badgeAdorner.DetachFromTarget(adornerLayer,
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
                DetachStandaloneAdorner(_badgeAdorner);
                _badgeAdorner.DetachFromTarget(_adornerLayer, false);
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
        // 目标挂载与指示器可见性无关：初始隐藏（零计数、空内容、BadgeIsVisible=false）时也必须挂上目标
        SyncDecoratedTargetChild(null);
        SetupShowZero();
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        // The subscription does not fire for the first context, and a re-attached badge reuses its adorner: resolve
        // against the context of this attach (the theme may have changed, or the badge moved to another provider).
        ConfigureBadgeColor();

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
        if (this.IsAttachedToVisualTree() && BadgeColorUtils.IsThemeDependent(BadgeColor))
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
        if (_badgeAdorner is not null && BadgeColor is { } badgeColor)
        {
            ConfigureDotColor(badgeColor);
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        _ancestorVisibilityTracker?.Dispose();
        _ancestorVisibilityTracker = null;
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
        if (_badgeAdorner is not null)
        {
            _badgeAdorner[!AbstractCountBadgeAdorner.OffsetProperty]        = this[!OffsetProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.SizeProperty]          = this[!SizeProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.OverflowCountProperty] = this[!OverflowCountProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.CountProperty]         = this[!CountProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.ContentProperty]       = this[!ContentProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.IsBorderedProperty]    = this[!IsBorderedProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
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
        if (_badgeAdorner is null)
        {
            return;
        }

        _badgeAdorner.DetachFromTarget(_adornerLayer, false);
        _adornerLayer = null;
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
        if (_badgeAdorner is not null)
        {
            DetachChild(oldDecoratedTarget);
            if (DecoratedTarget is null)
            {
                DetachLayerAdorner();
                _badgeAdorner.IsAdornerMode = false;
                if (BadgeIsVisible)
                {
                    AttachStandaloneAdorner(_badgeAdorner);
                }
            }
            else if (DecoratedTarget is not null)
            {
                DetachStandaloneAdorner(_badgeAdorner);
                _badgeAdorner.IsAdornerMode = true;
                AttachChild(DecoratedTarget);
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BadgeIsVisibleProperty)
        {
            if (BadgeIsVisible)
            {
                SetupShowZero();
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
                NotifyDecoratedTargetChanged(change.GetOldValue<Control?>());
                if (BadgeIsVisible)
                {
                    PrepareAdorner();
                }

                UpdateAncestorVisibilityTracking();
            }

            if (change.Property == BadgeColorProperty)
            {
                ConfigureDotColor(change.GetNewValue<string>());
                UpdateThemeSubscription();
            }
        }

        if (change.Property == ContentProperty)
        {
            SetupShowZero();
        }

        if (change.Property == CountProperty ||
            change.Property == IsZeroVisibleProperty)
        {
            if (change.Property == CountProperty)
            {
                var oldCount = change.GetOldValue<int>();
                var newCount = change.GetNewValue<int>();
                if (oldCount > 0 && newCount == 0 && !IsZeroVisible)
                {
                    _badgeAdorner?.PreserveCountTextForHide();
                }
            }
            else if (Count == 0 && IsZeroVisible)
            {
                _badgeAdorner?.RefreshCountText();
            }

            SetupShowZero();
        }
    }

    private void SetupShowZero()
    {
        // Explicit content decides on its own: an empty string means nothing to show, any other value is
        // always shown whatever the count says. Null falls through to the count logic.
        if (Content is string { Length: 0 })
        {
            BadgeIsVisible = false;
        }
        else if (Content is not null)
        {
            BadgeIsVisible = true;
        }
        else if (Count == 0 && !IsZeroVisible)
        {
            BadgeIsVisible = false;
        }
        else if (Count > 0 || IsZeroVisible)
        {
            BadgeIsVisible = true;
        }
    }
    
    /// <summary>
    /// Writes the resolved colour into the internal indicator, or clears it so the indicator falls back to the
    /// theme's indicator colour when the colour is unset or invalid.
    /// </summary>
    private protected virtual void ConfigureDotColor(string? colorStr)
    {
        if (_badgeAdorner is null)
        {
            return;
        }

        if (BadgeColorUtils.CalculateColor(this, colorStr) is { } brush)
        {
            _badgeAdorner.BadgeColor = brush;
        }
        else
        {
            _badgeAdorner.ClearValue(AbstractCountBadgeAdorner.BadgeColorProperty);
        }
    }
}
