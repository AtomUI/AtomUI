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

public abstract class AbstractRibbonBadge : Control
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

    [Content]
    public Control? DecoratedTarget
    {
        get => GetValue(DecoratedTargetProperty);
        set => SetValue(DecoratedTargetProperty, value);
    }

    public string? RibbonColor
    {
        get => GetValue(RibbonColorProperty);
        set => SetValue(RibbonColorProperty, value);
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
    
    private protected AbstractRibbonBadgeAdorner? _ribbonBadgeAdorner;

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

    private protected void SetupRibbonColor(string colorStr)
    {
        var colorSpan = colorStr.AsSpan().Trim();

        if (BadgeColorUtils.TryGetPresetColor(colorSpan, out var presetColor))
        {
            _ribbonBadgeAdorner!.RibbonColor = new SolidColorBrush(presetColor);
            return;
        }

        if (Color.TryParse(colorSpan, out var color))
        {
            _ribbonBadgeAdorner!.RibbonColor = new SolidColorBrush(color);
        }
    }

    private protected virtual void SetupTokenBindings()
    {
        if (_ribbonBadgeAdorner is not null)
        {
            _ribbonBadgeAdorner[!AbstractRibbonBadgeAdorner.TextProperty]      = this[!TextProperty];
            _ribbonBadgeAdorner[!AbstractRibbonBadgeAdorner.OffsetProperty]    = this[!OffsetProperty];
            _ribbonBadgeAdorner[!AbstractRibbonBadgeAdorner.PlacementProperty] = this[!PlacementProperty];
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
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
}
