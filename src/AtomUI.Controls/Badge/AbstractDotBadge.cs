using System.Reactive.Disposables;
using AtomUI.Data;
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

public abstract class AbstractDotBadge : Control, IMotionAwareControl, IThemeInheritanceSource
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

    private protected AbstractDotBadgeAdorner? _dotBadgeAdorner;

    private protected AdornerLayer? _adornerLayer;

    private const int MaxAdornerLayerRetryCount = 30;

    private bool _adornerLayerRetryScheduled;

    private int _adornerLayerRetryCount;

    private IDisposable? _motionBinding;

    private AncestorVisibilityTracker? _ancestorVisibilityTracker;

    private Panel? _standaloneContentHost;

    #endregion

    static AbstractDotBadge()
    {
        AffectsMeasure<AbstractDotBadge>(DecoratedTargetProperty, TextProperty);
        AffectsRender<AbstractDotBadge>(DotColorProperty, StatusProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractDotBadge>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractDotBadge>(VerticalAlignment.Top);
    }

    #region 公共与受保护 API

    private protected abstract AbstractDotBadgeAdorner CreateDotBadgeAdorner();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupTokenBindings();
        if (_dotBadgeAdorner is not null)
        {
            ConfigureDotColor(DotColor);
        }
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        _adornerLayerRetryCount = 0;
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
        if (_dotBadgeAdorner is not null)
        {
            // Runtime adorner projections are LocalValue and are established
            // before template/visual attachment; AXAML setters cannot preserve
            // this preparation contract. The owner holds their release handles.
            _adornerBindings = new CompositeDisposable
            {
                BindUtils.RelayBind(this, StatusProperty, _dotBadgeAdorner, AbstractDotBadgeAdorner.StatusProperty),
                BindUtils.RelayBind(this, TextProperty, _dotBadgeAdorner, AbstractDotBadgeAdorner.TextProperty),
                BindUtils.RelayBind(this, OffsetProperty, _dotBadgeAdorner, AbstractDotBadgeAdorner.OffsetProperty),
                BindUtils.RelayBind(this, IsMotionEnabledProperty, _dotBadgeAdorner, AbstractDotBadgeAdorner.IsMotionEnabledProperty)
            };
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
            if (badgeIsVisible &&
                (change.Property == BadgeIsVisibleProperty || BadgeIsVisible))
            {
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
                else if (DecoratedTarget is not null)
                {
                    AttachChild(DecoratedTarget);
                }
            }

            if (change.Property == DotColorProperty)
            {
                ConfigureDotColor(change.GetNewValue<string>());
            }
        }
    }

    private protected virtual void ConfigureDotColor(string? colorStr)
    {
        _instanceColorValue?.Dispose();
        _instanceColorValue = null;
        var brush = BadgeColorUtils.CalculateColor(colorStr);
        if (_dotBadgeAdorner is not null && brush is not null)
        {
            // Preset/instance strings require runtime parsing. A removable value
            // frame preserves the underlying AXAML token binding for clear/restore.
            _instanceColorValue = _dotBadgeAdorner.SetValue(AbstractDotBadgeAdorner.BadgeDotColorProperty,
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

        host.Children.Remove(adorner);
        LogicalChildren.Remove(adorner);
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
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        _dotBadgeAdorner.DetachFromTargetAsync(_adornerLayer, false);
        _adornerLayer = null;
    }

    private void ResetDotBadgeAdorner()
    {
        _instanceColorValue?.Dispose();
        _instanceColorValue = null;
        _adornerBindings?.Dispose();
        _adornerBindings = null;
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        DetachStandaloneAdorner(_dotBadgeAdorner);
        DetachLayerAdorner();
        _dotBadgeAdorner = null;
    }

    #endregion
}
