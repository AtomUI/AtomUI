using System.Reactive.Disposables;
using AtomUI.Data;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

public enum RibbonBadgePlacement
{
    Start,
    End
}

public abstract class AbstractRibbonBadge : Control, IThemeInheritanceSource
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> RibbonColorProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, string?>(nameof(RibbonColor));

    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, Control?>(nameof(DecoratedTarget));

    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, Point>(nameof(Offset));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, string?>(nameof(Text));

    public static readonly StyledProperty<RibbonBadgePlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, RibbonBadgePlacement>(
            nameof(Placement),
            RibbonBadgePlacement.End);

    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, bool>(nameof(BadgeIsVisible), true);

    public string? RibbonColor
    {
        get => GetValue(RibbonColorProperty);
        set => SetValue(RibbonColorProperty, value);
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

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public RibbonBadgePlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    public bool BadgeIsVisible
    {
        get => GetValue(BadgeIsVisibleProperty);
        set => SetValue(BadgeIsVisibleProperty, value);
    }

    #endregion

    #region 运行时字段

    private CompositeDisposable? _adornerBindings;

    private IDisposable? _instanceColorValue;

    private protected AbstractRibbonBadgeAdorner? _ribbonBadgeAdorner;

    #endregion

    static AbstractRibbonBadge()
    {
        AffectsMeasure<AbstractRibbonBadge>(DecoratedTargetProperty,
            TextProperty);
        AffectsRender<AbstractRibbonBadge>(RibbonColorProperty, PlacementProperty);
        // 装饰型控件必须包裹目标而不是拉伸占满父槽（与 AbstractDotBadge 一致）：
        // 拉伸会把 root 语义部件标记拖到父槽宽度。
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractRibbonBadge>(Avalonia.Layout.HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractRibbonBadge>(Avalonia.Layout.VerticalAlignment.Top);
    }

    #region 公共与受保护 API

    private protected void HandleDecoratedTargetChanged(Control? oldDecoratedTarget = null)
    {
        DetachChild(oldDecoratedTarget);
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        else
        {
            HideAdorner();
        }
    }

    private protected void SetupRibbonColor(string? colorStr)
    {
        _instanceColorValue?.Dispose();
        _instanceColorValue = null;
        var brush = BadgeColorUtils.CalculateColor(colorStr);
        if (_ribbonBadgeAdorner is not null && brush is not null)
        {
            // Preset/instance strings require runtime parsing. A removable value
            // frame preserves the underlying AXAML token binding for clear/restore.
            _instanceColorValue = _ribbonBadgeAdorner.SetValue(AbstractRibbonBadgeAdorner.RibbonColorProperty,
                brush, Avalonia.Data.BindingPriority.Animation);
        }
    }

    private protected virtual void SetupTokenBindings()
    {
        _adornerBindings?.Dispose();
        _adornerBindings = null;
        if (_ribbonBadgeAdorner is not null)
        {
            // Runtime adorner projections are LocalValue and are established
            // before template/visual attachment; AXAML setters cannot preserve
            // this preparation contract. The owner holds their release handles.
            _adornerBindings = new CompositeDisposable
            {
                BindUtils.RelayBind(this, TextProperty, _ribbonBadgeAdorner, AbstractRibbonBadgeAdorner.TextProperty),
                BindUtils.RelayBind(this, OffsetProperty, _ribbonBadgeAdorner, AbstractRibbonBadgeAdorner.OffsetProperty),
                BindUtils.RelayBind(this, PlacementProperty, _ribbonBadgeAdorner, AbstractRibbonBadgeAdorner.PlacementProperty)
            };
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
                HideAdorner();
            }
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DecoratedTargetProperty)
            {
                HandleDecoratedTargetChanged(change.GetOldValue<Control?>());
            }

            if (change.Property == RibbonColorProperty)
            {
                SetupRibbonColor(change.GetNewValue<string>());
            }
        }
    }

    private protected abstract AbstractRibbonBadgeAdorner CreateBadgeAdorner();

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SetupTokenBindings();
        if (_ribbonBadgeAdorner is not null)
        {
            SetupRibbonColor(RibbonColor);
        }
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        else
        {
            HideAdorner();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _adornerBindings?.Dispose();
        _adornerBindings = null;
        _instanceColorValue?.Dispose();
        _instanceColorValue = null;
        HideAdorner();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (DecoratedTarget is not null)
        {
            DecoratedTarget.Measure(availableSize);
            var targetSize = DecoratedTarget.DesiredSize;
            if (BadgeIsVisible && _ribbonBadgeAdorner is not null)
            {
                _ribbonBadgeAdorner.Measure(targetSize);
            }

            return targetSize;
        }

        if (BadgeIsVisible && _ribbonBadgeAdorner is not null)
        {
            _ribbonBadgeAdorner.Measure(availableSize);
            return _ribbonBadgeAdorner.DesiredSize;
        }

        return default;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        if (DecoratedTarget is not null)
        {
            DecoratedTarget.Arrange(new Rect(finalSize));
            if (BadgeIsVisible && _ribbonBadgeAdorner is not null)
            {
                _ribbonBadgeAdorner.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        if (BadgeIsVisible && _ribbonBadgeAdorner is not null)
        {
            _ribbonBadgeAdorner.Arrange(new Rect(finalSize));
        }

        return finalSize;
    }

    #endregion

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    #region 私有实现

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

    private void DetachRibbonAdorner()
    {
        DetachChild(_ribbonBadgeAdorner);
    }

    private void PrepareAdorner()
    {
        var ribbonBadgeAdorner = CreateBadgeAdorner();
        if (DecoratedTarget is not null)
        {
            AttachChild(DecoratedTarget);
            ribbonBadgeAdorner.IsAdornerMode = true;
            AttachChild(ribbonBadgeAdorner);
        }
        else
        {
            DetachRibbonAdorner();
            ribbonBadgeAdorner.IsAdornerMode = false;
            AttachChild(ribbonBadgeAdorner);
            IsVisible = true;
        }

        InvalidateMeasure();
    }

    private void HideAdorner()
    {
        if (DecoratedTarget is null)
        {
            DetachRibbonAdorner();
            IsVisible = false;
        }
        else
        {
            AttachChild(DecoratedTarget);
            DetachRibbonAdorner();
        }

        InvalidateMeasure();
    }

    #endregion
}
