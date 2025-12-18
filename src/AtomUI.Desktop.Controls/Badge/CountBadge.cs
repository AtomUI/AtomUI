using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Theme.Palette;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum CountBadgeSize
{
    Default,
    Small
}

public class CountBadge : Control,
                          IControlSharedTokenResourcesHost,
                          IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<CountBadge, string?>(
            nameof(BadgeColor));

    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<CountBadge, int>(nameof(Count),
            coerce: (o, v) => Math.Max(0, v));

    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<CountBadge, Control?>(nameof(DecoratedTarget));

    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<CountBadge, Point>(nameof(Offset));

    public static readonly StyledProperty<int> OverflowCountProperty =
        AvaloniaProperty.Register<CountBadge, int>(nameof(OverflowCount), 99,
            coerce: (o, v) => Math.Max(0, v));

    public static readonly StyledProperty<bool> ShowZeroProperty =
        AvaloniaProperty.Register<CountBadge, bool>(nameof(ShowZero));

    public static readonly StyledProperty<CountBadgeSize> SizeProperty =
        AvaloniaProperty.Register<CountBadge, CountBadgeSize>(nameof(Size));

    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<CountBadge, bool>(nameof(BadgeIsVisible), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<CountBadge>();

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

    public bool ShowZero
    {
        get => GetValue(ShowZeroProperty);
        set => SetValue(ShowZeroProperty, value);
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

    #region 内部属性定义

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => BadgeToken.ID;

    #endregion

    private CountBadgeAdorner? _badgeAdorner;
    private AdornerLayer? _adornerLayer;
    private CompositeDisposable? _adornerBindingDisposables;

    static CountBadge()
    {
        AffectsMeasure<CountBadge>(DecoratedTargetProperty,
            CountProperty,
            OverflowCountProperty,
            SizeProperty);
        AffectsRender<CountBadge>(BadgeColorProperty, OffsetProperty);
    }

    public CountBadge()
    {
        this.RegisterResources();
        this.ConfigureMotionBindingStyle();
    }

    private CountBadgeAdorner CreateBadgeAdorner()
    {
        if (_badgeAdorner is null)
        {
            _badgeAdorner = new CountBadgeAdorner();
            SetupTokenBindings();
            HandleDecoratedTargetChanged();
            if (BadgeColor is not null)
            {
                SetupBadgeColor(BadgeColor);
            }
        }

        return _badgeAdorner;
    }

    private void PrepareAdorner()
    {
        var badgeAdorner = CreateBadgeAdorner();
        if (DecoratedTarget is not null)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            // 这里需要抛出异常吗？
            if (_adornerLayer == null)
            {
                return;
            }

            badgeAdorner.ApplyToTarget(_adornerLayer, this);
        }
        else
        {
            IsVisible = true;
            badgeAdorner.ApplyToTarget(null, this);
        }
    }

    private void HideAdorner(bool enableMotion)
    {
        // 这里需要抛出异常吗？
        if (_badgeAdorner is null)
        {
            return;
        }

        _badgeAdorner.DetachFromTarget(_adornerLayer, enableMotion);
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
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        if (DecoratedTarget is null)
        {
            CreateBadgeAdorner();
        }

        SetupShowZero();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        HideAdorner(false);
    }

    private void SetupTokenBindings()
    {
        if (_badgeAdorner is not null)
        {
            _adornerBindingDisposables?.Dispose();
            _adornerBindingDisposables = new CompositeDisposable(5);
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, OffsetProperty, _badgeAdorner, CountBadgeAdorner.OffsetProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, SizeProperty, _badgeAdorner, CountBadgeAdorner.SizeProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, OverflowCountProperty, _badgeAdorner, CountBadgeAdorner.OverflowCountProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, CountProperty, _badgeAdorner, CountBadgeAdorner.CountProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, _badgeAdorner,
                CountBadgeAdorner.IsMotionEnabledProperty));
        }
    }

    private void HandleDecoratedTargetChanged()
    {
        if (_badgeAdorner is not null)
        {
            if (DecoratedTarget is null)
            {
                _badgeAdorner.SetLogicalParent(this);
                VisualChildren.Add(_badgeAdorner);
                LogicalChildren.Add(_badgeAdorner);
                _badgeAdorner.IsAdornerMode = false;
            }
            else if (DecoratedTarget is not null)
            {
                _badgeAdorner.IsAdornerMode = true;
                DecoratedTarget.SetLogicalParent(this);
                VisualChildren.Add(DecoratedTarget);
                LogicalChildren.Add(DecoratedTarget);
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
                HideAdorner(IsMotionEnabled);
            }
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DecoratedTargetProperty)
            {
                HandleDecoratedTargetChanged();
            }

            if (change.Property == BadgeColorProperty)
            {
                SetupBadgeColor(change.GetNewValue<string>());
            }
        }

        if (change.Property == CountProperty ||
            change.Property == ShowZeroProperty)
        {
            SetupShowZero();
        }
    }

    private void SetupShowZero()
    {
        if (Count == 0 && !ShowZero)
        {
            BadgeIsVisible = false;
        }
        else if (Count > 0)
        {
            BadgeIsVisible = true;
        }
    }

    private void SetupBadgeColor(string colorStr)
    {
        colorStr = colorStr.Trim().ToLower();

        foreach (var presetColor in PresetPrimaryColor.AllColorTypes())
        {
            if (presetColor.Type.ToString().ToLower() == colorStr)
            {
                _badgeAdorner!.BadgeColor = new SolidColorBrush(presetColor.Color());
                return;
            }
        }

        if (Color.TryParse(colorStr, out var color))
        {
            _badgeAdorner!.BadgeColor = new SolidColorBrush(color);
        }
    }
}