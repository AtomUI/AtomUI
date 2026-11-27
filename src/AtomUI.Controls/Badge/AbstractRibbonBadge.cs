using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// The corner of the decorated target that a ribbon hangs over.
/// </summary>
public enum RibbonBadgePlacement
{
    /// <summary>
    /// The ribbon hangs over the top-left corner of the target.
    /// </summary>
    Start,
    /// <summary>
    /// The ribbon hangs over the top-right corner of the target. This is the default placement.
    /// </summary>
    End
}

/// <summary>
/// Base class of badges that show a label on a coloured ribbon with a folded corner. The ribbon hangs over a top
/// corner of a <see cref="DecoratedTarget" />, or stands alone without one. Derived classes create the ribbon
/// in <c>CreateBadgeAdorner</c>.
/// </summary>
public abstract class AbstractRibbonBadge : Control
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="RibbonColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> RibbonColorProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, string?>(nameof(RibbonColor));

    /// <summary>
    /// Identifies the <see cref="DecoratedTarget" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, Control?>(nameof(DecoratedTarget));

    /// <summary>
    /// Identifies the <see cref="Offset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, Point>(nameof(Offset));

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, string?>(nameof(Text));

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<RibbonBadgePlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, RibbonBadgePlacement>(
            nameof(Placement),
            RibbonBadgePlacement.End);

    /// <summary>
    /// Identifies the <see cref="BadgeIsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<AbstractRibbonBadge, bool>(nameof(BadgeIsVisible), true);

    /// <summary>
    /// The control that the ribbon decorates. It becomes the badge's child, receives the whole layout slot and decides
    /// the badge's size; the ribbon is laid over it.
    /// When it is <see langword="null" />, the badge shows the ribbon alone and is sized to the ribbon.
    /// </summary>
    [Content]
    public Control? DecoratedTarget
    {
        get => GetValue(DecoratedTargetProperty);
        set => SetValue(DecoratedTargetProperty, value);
    }

    /// <summary>
    /// Background of the ribbon body; the fold below it uses a darkened shade of the same color. The value is a
    /// preset color name such as <c>Pink</c> or <c>Cyan</c>, a status color name, or a color string that
    /// <c>Color.TryParse</c> accepts. Preset names resolve to the palette of the current theme.
    /// <see langword="null" />, an empty string or an unknown value makes the ribbon use the theme's primary color.
    /// </summary>
    public string? RibbonColor
    {
        get => GetValue(RibbonColorProperty);
        set => SetValue(RibbonColorProperty, value);
    }

    /// <summary>
    /// Additional shift of the ribbon from its default position at the target's corner, in device-independent pixels.
    /// A positive <c>X</c> moves the ribbon right and a positive <c>Y</c> moves it down. It applies only when
    /// <see cref="DecoratedTarget" /> is set.
    /// </summary>
    public Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// The label shown on the ribbon body. The body is sized to fit the label.
    /// <see langword="null" /> leaves the body without a label.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// The corner of <see cref="DecoratedTarget" /> that the ribbon hangs over. The default is
    /// <see cref="RibbonBadgePlacement.End" />. Without a target it only selects which end of the ribbon body carries
    /// the fold.
    /// </summary>
    public RibbonBadgePlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Shows or hides the ribbon while a <see cref="DecoratedTarget" /> stays in place. The default is
    /// <see langword="true" />. Without a target the badge hides and shows the whole control by writing its own
    /// <see cref="Visual.IsVisible" />, so that property is owned by the badge rather than by the application.
    /// </summary>
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
        // A decorating control wraps its target instead of stretching to fill the parent slot, as in AbstractDotBadge.
        // Stretching would drag the root semantic part marker to the width of the parent slot.
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractRibbonBadge>(Avalonia.Layout.HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractRibbonBadge>(Avalonia.Layout.VerticalAlignment.Top);
    }
    
    private protected AbstractRibbonBadgeAdorner? _ribbonBadgeAdorner;
    private IDisposable? _themeSubscription;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

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

    /// <summary>
    /// Writes the resolved <see cref="RibbonColor" /> into the internal ribbon, or clears it so the ribbon falls
    /// back to the theme's ColorPrimary when the color is unset or invalid.
    /// </summary>
    private protected void ConfigureRibbonColor()
    {
        if (_ribbonBadgeAdorner is null)
        {
            return;
        }

        if (BadgeColorUtils.CalculateColor(this, RibbonColor) is { } brush)
        {
            _ribbonBadgeAdorner.RibbonColor = brush;
        }
        else
        {
            _ribbonBadgeAdorner.ClearValue(AbstractRibbonBadgeAdorner.RibbonColorProperty);
        }
    }

    /// <summary>
    /// Binds the internal ribbon stored in <c>_ribbonBadgeAdorner</c> to <see cref="Text" />,
    /// <see cref="Offset" /> and <see cref="Placement" />, so that later changes of those properties reach the ribbon.
    /// It does nothing while that field is <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// The base class does not call it. An override of <see cref="CreateBadgeAdorner" /> calls it once for each new
    /// instance it stores in <c>_ribbonBadgeAdorner</c>, as <c>RibbonBadge</c> does.
    /// Overrides must call the base member, which creates the three bindings.
    /// </remarks>
    private protected virtual void SetupTokenBindings()
    {
        if (_ribbonBadgeAdorner is not null)
        {
            _ribbonBadgeAdorner[!AbstractRibbonBadgeAdorner.TextProperty]      = this[!TextProperty];
            _ribbonBadgeAdorner[!AbstractRibbonBadgeAdorner.OffsetProperty]    = this[!OffsetProperty];
            _ribbonBadgeAdorner[!AbstractRibbonBadgeAdorner.PlacementProperty] = this[!PlacementProperty];
        }
    }

    /// <summary>
    /// Shows or removes the ribbon when <see cref="Visual.IsVisible" /> or <see cref="BadgeIsVisible" /> changes.
    /// While the badge is attached to a visual tree, it reacts to a <see cref="DecoratedTarget" /> change by
    /// replacing the decorated child, and to a <see cref="RibbonColor" /> change by resolving the color again.
    /// Overrides must call the base member first.
    /// </summary>
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
                ConfigureRibbonColor();
            }
        }
    }

    /// <summary>
    /// Creates the ribbon indicator of the badge; <c>RibbonBadge</c> in AtomUI.Desktop.Controls overrides it.
    /// </summary>
    /// <remarks>
    /// The base class calls it each time it prepares the indicator. It then sets <c>IsAdornerMode</c> on the returned
    /// instance and attaches it as its own child, together with <see cref="DecoratedTarget" /> or alone when there is
    /// no decorated target. It removes only the instance stored in <c>_ribbonBadgeAdorner</c>, so an override creates
    /// the instance only while that field is <see langword="null" /> and returns that same instance on later calls.
    /// For a new instance the override calls <see cref="SetupTokenBindings" /> and <see cref="ConfigureRibbonColor" />,
    /// so that the values of the badge reach the ribbon.
    /// </remarks>
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

    /// <summary>
    /// When the badge is attached to a visual tree, shows the ribbon if <see cref="BadgeIsVisible" /> is
    /// <see langword="true" /> and removes it otherwise, resolves the ribbon color for the current theme, and
    /// subscribes to theme changes so that a preset <see cref="RibbonColor" /> follows the theme. Overrides must
    /// call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Preset ribbon colors come from the snapshot's palettes: re-resolve them whenever the theme publishes.
        _themeSubscription?.Dispose();
        _themeSubscription = s_themeTokenResolver.Subscribe(this, _ => ConfigureRibbonColor());
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        else
        {
            HideAdorner();
        }
        // The subscription does not fire for the first context, and a re-attached badge reuses its adorner: resolve
        // against the context of this attach (the theme may have changed, or the badge moved to another provider).
        ConfigureRibbonColor();
    }

    /// <summary>
    /// When the badge is detached from a visual tree, stops the theme subscription and removes the ribbon.
    /// Overrides must call the base member first.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        HideAdorner();
    }

    /// <summary>
    /// Measures the decorated target and reports its desired size, so the ribbon never enlarges the badge. Without a
    /// target it reports the ribbon's desired size, or an empty size when no ribbon is shown.
    /// </summary>
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

    /// <summary>
    /// Gives the whole final size to the decorated target and lays the ribbon out over the same area. Without a
    /// target it arranges the ribbon in that area alone.
    /// </summary>
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
