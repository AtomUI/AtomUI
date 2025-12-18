using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Theme;
using AtomUI.Theme.Palette;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum DotBadgeStatus
{
    Default,
    Success,
    Processing,
    Error,
    Warning
}

public class DotBadge : Control,
                        IControlSharedTokenResourcesHost,
                        IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> DotColorProperty
        = AvaloniaProperty.Register<DotBadge, string?>(
            nameof(DotColor));

    public static readonly StyledProperty<DotBadgeStatus?> StatusProperty
        = AvaloniaProperty.Register<DotBadge, DotBadgeStatus?>(
            nameof(Status));

    public static readonly StyledProperty<string?> TextProperty
        = AvaloniaProperty.Register<DotBadge, string?>(
            nameof(Text));

    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<DotBadge, Control?>(nameof(DecoratedTarget));

    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<DotBadge, Point>(nameof(Offset));

    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<DotBadge, bool>(nameof(BadgeIsVisible));
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty
        = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<DotBadge>();

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
    
    #region 内部属性定义
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => BadgeToken.ID;

    #endregion

    private DotBadgeAdorner? _dotBadgeAdorner;
    private AdornerLayer? _adornerLayer;
    private CompositeDisposable? _adornerBindingDisposables;

    static DotBadge()
    {
        AffectsMeasure<DotBadge>(DecoratedTargetProperty, TextProperty);
        AffectsRender<DotBadge>(DotColorProperty, StatusProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<DotBadge>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<DotBadge>(VerticalAlignment.Top);
    }

    public DotBadge()
    {
        this.RegisterResources();
        this.ConfigureMotionBindingStyle();
    }

    private DotBadgeAdorner CreateDotBadgeAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            _dotBadgeAdorner = new DotBadgeAdorner();
            SetupTokenBindings();
            HandleDecoratedTargetChanged();
            if (DotColor is not null)
            {
                SetupDotColor(DotColor);
            }
        }

        return _dotBadgeAdorner;
    }

    private void PrepareAdorner()
    {
        if (DecoratedTarget is not null)
        {
            var dotBadgeAdorner = CreateDotBadgeAdorner();
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            // 这里需要抛出异常吗？
            if (_adornerLayer == null)
            {
                return;
            }

            dotBadgeAdorner.ApplyToTarget(_adornerLayer, this);
        } 
        else
        {
            IsVisible = true;
        }
    }

    private void HideAdorner(bool enableMotion)
    {
        // 这里需要抛出异常吗？
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        _dotBadgeAdorner.DetachFromTarget(_adornerLayer, enableMotion);
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
        if (DecoratedTarget is null)
        {
            CreateDotBadgeAdorner();
        }
        else
        {
            PrepareAdorner();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        HideAdorner(false);
    }

    private void SetupTokenBindings()
    {
        if (_dotBadgeAdorner is not null)
        {
            _adornerBindingDisposables?.Dispose();
            _adornerBindingDisposables = new CompositeDisposable(4);
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, StatusProperty, _dotBadgeAdorner, DotBadgeAdorner.StatusProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, TextProperty, _dotBadgeAdorner, DotBadgeAdorner.TextProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, OffsetProperty, _dotBadgeAdorner, DotBadgeAdorner.OffsetProperty));
            _adornerBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, _dotBadgeAdorner, DotBadgeAdorner.IsMotionEnabledProperty));
        }
    }

    private void HandleDecoratedTargetChanged()
    {
        if (_dotBadgeAdorner is not null)
        {
            if (DecoratedTarget is null)
            {
                _dotBadgeAdorner.IsAdornerMode = false;
                _dotBadgeAdorner.SetLogicalParent(this);
                VisualChildren.Add(_dotBadgeAdorner);
                LogicalChildren.Add(_dotBadgeAdorner);
            }
            else
            {
                _dotBadgeAdorner.IsAdornerMode = true;
                DecoratedTarget.SetLogicalParent(this);
                VisualChildren.Add(DecoratedTarget);
                LogicalChildren.Add(DecoratedTarget);
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
                HideAdorner(IsMotionEnabled);
            }
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DecoratedTargetProperty)
            {
                HandleDecoratedTargetChanged();
            }

            if (change.Property == DotColorProperty)
            {
                SetupDotColor(change.GetNewValue<string>());
            }
        }
    }

    private void SetupDotColor(string colorStr)
    {
        colorStr = colorStr.Trim().ToLower();

        foreach (var presetColor in PresetPrimaryColor.AllColorTypes())
        {
            if (presetColor.Type.ToString().ToLower() == colorStr)
            {
                _dotBadgeAdorner!.BadgeDotColor = new SolidColorBrush(presetColor.Color());
                return;
            }
        }

        if (Color.TryParse(colorStr, out var color))
        {
            _dotBadgeAdorner!.BadgeDotColor = new SolidColorBrush(color);
        }
    }
}