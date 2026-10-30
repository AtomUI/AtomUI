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
    private Panel? _standaloneContentHost;

    static AbstractCountBadge()
    {
        AffectsMeasure<AbstractCountBadge>(DecoratedTargetProperty,
            CountProperty,
            OverflowCountProperty,
            SizeProperty);
        AffectsRender<AbstractCountBadge>(BadgeColorProperty, OffsetProperty);
        // 装饰型控件必须包裹目标而不是拉伸占满父槽（与 AbstractDotBadge 一致）：
        // 拉伸会把指示器锚点和 root 语义部件标记拖到父槽右缘。
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractCountBadge>(Avalonia.Layout.HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractCountBadge>(Avalonia.Layout.VerticalAlignment.Top);
    }

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
        SetupShowZero();
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }

        _ancestorVisibilityTracker?.Dispose();
        _ancestorVisibilityTracker = new AncestorVisibilityTracker(this, SyncLayerAdornerVisibility);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
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

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DecoratedTargetProperty)
            {
                NotifyDecoratedTargetChanged(change.GetOldValue<Control?>());
                if (BadgeIsVisible)
                {
                    PrepareAdorner();
                }
            }

            if (change.Property == BadgeColorProperty)
            {
                ConfigureDotColor(change.GetNewValue<string>());
            }
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
        if (Count == 0 && !IsZeroVisible)
        {
            BadgeIsVisible = false;
        }
        else if (Count > 0 || IsZeroVisible)
        {
            BadgeIsVisible = true;
        }
    }
    
    private protected virtual void ConfigureDotColor(string colorStr)
    {
        _badgeAdorner!.BadgeColor = BadgeColorUtils.CalculateColor(colorStr);
    }
}
