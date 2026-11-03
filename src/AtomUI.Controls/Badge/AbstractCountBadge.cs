using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public enum CountBadgeSize
{
    Default,
    Small
}

public abstract class AbstractCountBadge : Control, IMotionAwareControl, IThemeInheritanceSource
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

    #region 运行时字段

    private CompositeDisposable? _adornerBindings;

    private IDisposable? _instanceColorValue;

    private protected AbstractCountBadgeAdorner? _badgeAdorner;

    private protected AdornerLayer? _adornerLayer;

    private const int MaxAdornerLayerRetryCount = 30;

    private bool _adornerLayerRetryScheduled;

    private int _adornerLayerRetryCount;

    private IDisposable? _motionBinding;

    private AncestorVisibilityTracker? _ancestorVisibilityTracker;

    private Panel? _standaloneContentHost;

    #endregion

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

    #region 公共与受保护 API

    private protected abstract AbstractCountBadgeAdorner CreateBadgeAdorner();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupTokenBindings();
        if (_badgeAdorner is not null)
        {
            ConfigureDotColor(BadgeColor);
        }
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        _adornerLayerRetryCount = 0;
        SetupShowZero();
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        else if (DecoratedTarget is not null)
        {
            AttachChild(DecoratedTarget);
        }

        _ancestorVisibilityTracker?.Dispose();
        _ancestorVisibilityTracker = new AncestorVisibilityTracker(this, SyncLayerAdornerVisibility);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _adornerBindings?.Dispose();
        _adornerBindings = null;
        _instanceColorValue?.Dispose();
        _instanceColorValue = null;
        _motionBinding?.Dispose();
        _motionBinding = null;
        _ancestorVisibilityTracker?.Dispose();
        _ancestorVisibilityTracker = null;
        Loaded -= HandleAdornerLayerRetryLoaded;
        _adornerLayerRetryScheduled = false;
        _adornerLayerRetryCount     = 0;
        HideAdorner(false);
    }

    private protected virtual void SetupTokenBindings()
    {
        _adornerBindings?.Dispose();
        _adornerBindings = null;
        if (_badgeAdorner is not null)
        {
            // Runtime adorner projections are LocalValue and are established
            // before template/visual attachment; AXAML setters cannot preserve
            // this preparation contract. The owner holds their release handles.
            _adornerBindings = new CompositeDisposable
            {
                BindUtils.RelayBind(this, OffsetProperty, _badgeAdorner, AbstractCountBadgeAdorner.OffsetProperty),
                BindUtils.RelayBind(this, SizeProperty, _badgeAdorner, AbstractCountBadgeAdorner.SizeProperty),
                BindUtils.RelayBind(this, OverflowCountProperty, _badgeAdorner, AbstractCountBadgeAdorner.OverflowCountProperty),
                BindUtils.RelayBind(this, CountProperty, _badgeAdorner, AbstractCountBadgeAdorner.CountProperty),
                BindUtils.RelayBind(this, IsMotionEnabledProperty, _badgeAdorner, AbstractCountBadgeAdorner.IsMotionEnabledProperty)
            };
        }
    }

    private protected virtual void NotifyDecoratedTargetChanged(Control? oldDecoratedTarget = null)
    {
        DetachChild(oldDecoratedTarget);
        if (_badgeAdorner is not null)
        {
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
                else if (DecoratedTarget is not null)
                {
                    AttachChild(DecoratedTarget);
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

    private protected virtual void ConfigureDotColor(string? colorStr)
    {
        _instanceColorValue?.Dispose();
        _instanceColorValue = null;
        var brush = BadgeColorUtils.CalculateColor(colorStr);
        if (_badgeAdorner is not null && brush is not null)
        {
            // Preset/instance strings require runtime parsing. A removable value
            // frame preserves the underlying AXAML token binding for clear/restore.
            _instanceColorValue = _badgeAdorner.SetValue(AbstractCountBadgeAdorner.BadgeColorProperty,
                brush, Avalonia.Data.BindingPriority.Animation);
        }
    }

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    #region 私有实现

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

    private void AttachChild(Control child)
    {
        if (child.GetVisualParent() == this)
        {
            return;
        }

        child.SetLogicalParent(this);
        VisualChildren.Add(child);
        LogicalChildren.Add(child);
        InvalidateMeasure();
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

        // The Panel provides the visual attach cascade; the public Badge remains
        // the direct logical/style owner required by its generated Part route.
        if (!LogicalChildren.Contains(adorner))
        {
            adorner.SetLogicalParent(this);
            LogicalChildren.Add(adorner);
        }
        if (!_standaloneContentHost.Children.Contains(adorner))
        {
            _standaloneContentHost.Children.Add(adorner);
        }
        if (((StyledElement)adorner).Parent is null)
        {
            var hostPanel = _standaloneContentHost;
            ((ISetLogicalParent)adorner).SetParent(this);
            if (((StyledElement)adorner).Parent is null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_standaloneContentHost == hostPanel &&
                        hostPanel.Children.Contains(adorner) &&
                        LogicalChildren.Contains(adorner) &&
                        ((StyledElement)adorner).Parent is null)
                    {
                        ((ISetLogicalParent)adorner).SetParent(this);
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

        var hostVisual = ElementComposition.GetElementVisual(host) as CompositionContainerVisual;
        var childVisual = ElementComposition.GetElementVisual(adorner);
        host.Children.Remove(adorner);
        LogicalChildren.Remove(adorner);
        if (childVisual is not null)
        {
            // This runtime host belongs to Badge. Once its visual membership is
            // retired, an off-tree renderer may never synchronize its children.
            // Release that public composition membership with the same lifetime.
            hostVisual?.Children.Remove(childVisual);
        }
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
        InvalidateMeasure();
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

    #endregion
}
