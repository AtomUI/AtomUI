using AtomUI.Controls.Primitives;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Rendering.Composition;

namespace AtomUI.Controls.Commons;

using AvaloniaButton = Avalonia.Controls.Button;

/// <summary>
/// Base class of float buttons: a themed icon button that can carry a description, a badge and a tooltip.
/// When a host places the button in the scoped overlay layer, <see cref="Placement" /> and the float offsets
/// position it inside that layer. An embedded button, such as an item of a float button group, is laid out
/// by its parent instead.
/// </summary>
[TemplatePart("PART_BadgeLayout", typeof(Canvas))]
[PseudoClasses(ButtonPseudoClass.IconOnly, ":description", ":plain-description")]
public abstract class AbstractFloatButton : AvaloniaButton, IMotionAwareControl
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonPlacement> PlacementProperty =
        AvaloniaProperty.Register<AbstractFloatButton, FloatButtonPlacement>(nameof(Placement));
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetX" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetXProperty =
        AvaloniaProperty.Register<AbstractFloatButton, double>(nameof(FloatOffsetX));
    
    /// <summary>
    /// Identifies the <see cref="FloatOffsetY" /> property.
    /// </summary>
    public static readonly StyledProperty<double> FloatOffsetYProperty =
        AvaloniaProperty.Register<AbstractFloatButton, double>(nameof(FloatOffsetY));
    
    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractFloatButton, PathIcon?>(nameof(Icon));
    
    /// <summary>
    /// Identifies the <see cref="DragTranslation" /> property.
    /// </summary>
    public static readonly StyledProperty<Vector> DragTranslationProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Vector>(nameof(DragTranslation),
            validate: static value => double.IsFinite(value.X) && double.IsFinite(value.Y));

    /// <summary>
    /// Identifies the <see cref="Tooltip" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TooltipProperty =
        AvaloniaProperty.Register<AbstractFloatButton, string?>(nameof(Tooltip));
    
    /// <summary>
    /// Identifies the <see cref="TooltipColor" /> property.
    /// </summary>
    public static readonly StyledProperty<Color?> TooltipColorProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Color?>(nameof(TooltipColor));
    
    /// <summary>
    /// Identifies the <see cref="ButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonType> ButtonTypeProperty =
        AvaloniaProperty.Register<AbstractFloatButton, FloatButtonType>(nameof(ButtonType), FloatButtonType.Default);
    
    /// <summary>
    /// Identifies the <see cref="Shape" /> property.
    /// </summary>
    public static readonly StyledProperty<FloatButtonShape> ShapeProperty =
        AvaloniaProperty.Register<AbstractFloatButton, FloatButtonShape>(nameof(Shape), FloatButtonShape.Circle);
    
    /// <summary>
    /// Identifies the <see cref="Href" /> property.
    /// </summary>
    public static readonly StyledProperty<Uri?> HrefProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Uri?>(nameof(Href));
    
    /// <summary>
    /// Identifies the <see cref="BoxShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<AbstractFloatButton>();
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractFloatButton>();
    
    /// <summary>
    /// Identifies the <see cref="IsBadgeEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBadgeEnabledProperty =
        AvaloniaProperty.Register<AbstractFloatButton, bool>(nameof(IsBadgeEnabled));
    
    /// <summary>
    /// Identifies the <see cref="IsDotBadge" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDotBadgeProperty =
        AvaloniaProperty.Register<AbstractFloatButton, bool>(nameof(IsDotBadge));
    
    /// <summary>
    /// Identifies the <see cref="BadgeCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> BadgeCountProperty =
        AvaloniaProperty.Register<AbstractFloatButton, int>(nameof(BadgeCount));
    
    /// <summary>
    /// Identifies the <see cref="BadgeColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<AbstractFloatButton, string?>(
            nameof(BadgeColor));
    
    /// <summary>
    /// Identifies the <see cref="BadgeOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> BadgeOffsetProperty =
        AvaloniaProperty.Register<AbstractFloatButton, Point>(nameof(BadgeOffset));

    /// <summary>
    /// Identifies the <see cref="BadgeOverflowCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> BadgeOverflowCountProperty =
        AvaloniaProperty.Register<AbstractFloatButton, int>(nameof(BadgeOverflowCount), 99,
            coerce: (o, v) => Math.Max(0, v));
    
    /// <summary>
    /// Gets or sets the edge or corner of the scoped overlay layer that the button is anchored to.
    /// The value positions the button only while its parent is that layer or a surface host inside it, which is
    /// where float button hosts put their button; otherwise the parent lays the button out and the value is ignored.
    /// Unset means <see cref="FloatButtonPlacement.Top" />.
    /// </summary>
    public FloatButtonPlacement Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the horizontal offset, in device-independent pixels, from the position chosen by
    /// <see cref="Placement" />. A positive value moves the button right, or left when the placement is
    /// <see cref="FloatButtonPlacement.Right" />, <see cref="FloatButtonPlacement.TopRight" /> or
    /// <see cref="FloatButtonPlacement.BottomRight" />; a negative value moves it the other way.
    /// It applies under the same condition as <see cref="Placement" />.
    /// </summary>
    public double FloatOffsetX
    {
        get => GetValue(FloatOffsetXProperty);
        set => SetValue(FloatOffsetXProperty, value);
    }

    /// <summary>
    /// Gets or sets the vertical offset, in device-independent pixels, from the position chosen by
    /// <see cref="Placement" />. A positive value moves the button down, or up when the placement is
    /// <see cref="FloatButtonPlacement.Bottom" />, <see cref="FloatButtonPlacement.BottomLeft" /> or
    /// <see cref="FloatButtonPlacement.BottomRight" />; a negative value moves it the other way.
    /// It applies under the same condition as <see cref="Placement" />.
    /// </summary>
    public double FloatOffsetY
    {
        get => GetValue(FloatOffsetYProperty);
        set => SetValue(FloatOffsetYProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the icon shown in the center of the button, above the description.
    /// A value that is <see langword="null" /> when the button is initialized is replaced by a default icon under
    /// the conditions of <see cref="ConfigureDefaultIcon" />. Setting <see langword="null" /> afterwards leaves the
    /// button without an icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a temporary render translation without changing the anchor position.
    /// The vector, in device-independent pixels, is applied as a composition translation on top of the position
    /// computed from <see cref="Placement" />, <see cref="FloatOffsetX" /> and <see cref="FloatOffsetY" />, which
    /// lets a caller preview a drag. Both components must be finite. <see cref="CommitDragTranslation" /> folds
    /// the translation into the float offsets and resets it to zero.
    /// </summary>
    public Vector DragTranslation
    {
        get => GetValue(DragTranslationProperty);
        set => SetValue(DragTranslationProperty, value);
    }

    /// <summary>
    /// Gets or sets the text of the tooltip shown when the pointer rests on the button.
    /// <see langword="null" /> shows no tooltip.
    /// </summary>
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the background color of the tooltip. <see langword="null" /> keeps the background of the theme.
    /// </summary>
    public Color? TooltipColor
    {
        get => GetValue(TooltipColorProperty);
        set => SetValue(TooltipColorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the visual type, which selects the icon color and the background of the button in the normal,
    /// hovered and pressed states. Unset means <see cref="FloatButtonType.Default" />.
    /// </summary>
    public FloatButtonType ButtonType
    {
        get => GetValue(ButtonTypeProperty);
        set => SetValue(ButtonTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the outline of the button. A circle is sized to the theme's float button size and shows only
    /// an icon. A square is a rounded rectangle. When it has a description, a square button shows the description
    /// under the icon and grows in height to fit it, unless it has an outline and is not embedded in a group; such
    /// a button shows the description beside the icon and is sized to its content. Unset means
    /// <see cref="FloatButtonShape.Circle" />.
    /// </summary>
    public FloatButtonShape Shape
    {
        get => GetValue(ShapeProperty);
        set => SetValue(ShapeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets a uniform resource identifier. Nothing in this library reads the value, so setting it has no
    /// effect on rendering or on the click behavior.
    /// </summary>
    public Uri? Href
    {
        get => GetValue(HrefProperty);
        set => SetValue(HrefProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the shadow painted around the button face. When unset, a square button embedded in a group gets
    /// no shadow; any other button takes the explicit primary shadow of the theme when it is a primary button and the
    /// theme provides one, and otherwise the float button shadow token.
    /// </summary>
    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the button animates. <see langword="true" /> transitions the background color when the
    /// pointer hovers or presses the button and animates the badge when it appears or disappears;
    /// <see langword="false" /> shows the end state at once. When unset, the theme takes the value from the
    /// <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether a badge is shown over the top-right corner of the button. <see langword="true" /> shows
    /// a count badge, or a dot badge when <see cref="IsDotBadge" /> is <see langword="true" />.
    /// </summary>
    public bool IsBadgeEnabled
    {
        get => GetValue(IsBadgeEnabledProperty);
        set => SetValue(IsBadgeEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the badge is a plain dot instead of a number. The value applies only while
    /// <see cref="IsBadgeEnabled" /> is <see langword="true" />.
    /// </summary>
    public bool IsDotBadge
    {
        get => GetValue(IsDotBadgeProperty);
        set => SetValue(IsDotBadgeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the number shown by the count badge. The value applies only while the badge is enabled and
    /// <see cref="IsDotBadge" /> is <see langword="false" />.
    /// </summary>
    public int BadgeCount
    {
        get => GetValue(BadgeCountProperty);
        set => SetValue(BadgeCountProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the color of the badge: a color string, a preset color name such as <c>blue</c>, or a status
    /// name such as <c>error</c>. Preset and status names follow the current theme. When the value is unset or
    /// cannot be resolved, the theme's error color is used.
    /// </summary>
    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the offset of the badge from the top-right corner of the button, in device-independent pixels.
    /// A positive value moves the badge right and down. When unset, the theme takes an offset tuned for the shape
    /// and for a dot or a count badge.
    /// </summary>
    public Point BadgeOffset
    {
        get => GetValue(BadgeOffsetProperty);
        set => SetValue(BadgeOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the number above which the count badge shows the overflow count followed by a plus sign instead
    /// of the actual count. Values below zero are treated as zero. Unset means 99.
    /// </summary>
    public int BadgeOverflowCount
    {
        get => GetValue(BadgeOverflowCountProperty);
        set => SetValue(BadgeOverflowCountProperty, value);
    }
    #endregion

    #region 内部属性定义
    internal static readonly DirectProperty<AbstractFloatButton, bool> IsEmbedModeProperty =
        AvaloniaProperty.RegisterDirect<AbstractFloatButton, bool>(
            nameof(IsEmbedMode),
            o => o.IsEmbedMode,
            (o, v) => o.IsEmbedMode = v);
    
    internal static readonly StyledProperty<IBrush?> BadgeEffectiveColorProperty =
        AvaloniaProperty.Register<AbstractFloatButton, IBrush?>(
            nameof(BadgeEffectiveColor));

    internal static readonly StyledProperty<bool> UseContentMeasureProperty =
        AvaloniaProperty.Register<AbstractFloatButton, bool>(nameof(UseContentMeasure));

    private bool _isEmbedMode;

    /// <summary>
    /// Whether the button is laid out inside a float button group instead of floating over its scope. The group sets
    /// it to <see langword="true" /> on its child buttons and back to <see langword="false" /> when a button
    /// leaves the group. While <see langword="true" />, the button does not use an overlay layer, and the theme
    /// drops the floating shadow and adjusts the background and the outline of a square button; a circle keeps the
    /// shadow and the background of a floating button.
    /// </summary>
    internal bool IsEmbedMode
    {
        get => _isEmbedMode;
        set => SetAndRaise(IsEmbedModeProperty, ref _isEmbedMode, value);
    }
    
    /// <summary>
    /// Color the badge of the button is painted with. The button writes the brush resolved from
    /// <see cref="BadgeColor" /> into it, and clears it when <see cref="BadgeColor" /> is unset, invalid or a status
    /// name without a theme, so that the <c>ColorError</c> of the theme applies then.
    /// </summary>
    internal IBrush? BadgeEffectiveColor
    {
        get => GetValue(BadgeEffectiveColorProperty);
        set => SetValue(BadgeEffectiveColorProperty, value);
    }

    /// <summary>
    /// Switches the button from a square measured to its longest side to a content-sized layout that shows the icon
    /// and the description side by side. While <see langword="true" />, the natural size is measured and the
    /// available width is limited so that the description fits into the overlay layer. The theme sets it for a
    /// square button with an outline that shows a description.
    /// </summary>
    internal bool UseContentMeasure
    {
        get => GetValue(UseContentMeasureProperty);
        set => SetValue(UseContentMeasureProperty, value);
    }

    #endregion
    
    private bool _isCommittingDrag;
    private protected ScopeAwareOverlayLayer? _overlayLayer;
    private protected Canvas? _badgeLayout;
    private protected Control? _badge;
    private IDisposable? _themeSubscription;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    static AbstractFloatButton()
    {
        AffectsMeasure<AbstractFloatButton>(UseContentMeasureProperty);
    }

    internal Size? OverlayAvailableSize => _overlayLayer?.Bounds.Size;

    /// <summary>
    /// Folds the temporary render translation into the anchored position and clears the translation.
    /// <see cref="FloatOffsetX" /> and <see cref="FloatOffsetY" /> change so that the button stays where the
    /// translation moved it. They are written as current values, so bindings on them stay attached.
    /// The method does nothing while the button is not positioned in the scoped overlay layer or the translation
    /// is zero.
    /// </summary>
    public void CommitDragTranslation()
    {
        var translation = DragTranslation;
        if (_overlayLayer is not { } layer || translation == default)
        {
            return;
        }

        var xSign = Placement is FloatButtonPlacement.Right or FloatButtonPlacement.TopRight or
            FloatButtonPlacement.BottomRight ? -1 : 1;
        var ySign = Placement is FloatButtonPlacement.Bottom or FloatButtonPlacement.BottomLeft or
            FloatButtonPlacement.BottomRight ? -1 : 1;
        _isCommittingDrag = true;
        try
        {
            SetCurrentValue(FloatOffsetXProperty, FloatOffsetX + xSign * translation.X);
            SetCurrentValue(FloatOffsetYProperty, FloatOffsetY + ySign * translation.Y);
        }
        finally
        {
            _isCommittingDrag = false;
        }
        CalculatePosition(this, layer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
        SetCurrentValue(DragTranslationProperty, default);
    }

    private void UpdateDragVisual()
    {
        if (ElementComposition.GetElementVisual(this) is { } visual)
        {
            var translation = DragTranslation;
            visual.Translation = new Vector3D(translation.X, translation.Y, 0);
        }
    }

    /// <summary>
    /// Called when the size of the button changes. A circle gets half of its height as corner radius, and the
    /// position in the scoped overlay layer and the badge position are recalculated. Overrides must call the base
    /// implementation to keep the SizeChanged event and these updates working.
    /// </summary>
    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        if (Shape == FloatButtonShape.Circle)
        {
            SetValue(CornerRadiusProperty, new CornerRadius(e.NewSize.Height / 2), BindingPriority.Template);
        }

        if (_overlayLayer != null)
        {
            CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
        }
        CalculateBadgePosition();
    }

    /// <summary>
    /// Called when the control template is applied. It looks up the badge layout part of the template and creates
    /// or removes the badge so that it matches <see cref="IsBadgeEnabled" />. Overrides must call the base
    /// implementation, otherwise the button shows no badge.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _badgeLayout = e.NameScope.Find<Canvas>("PART_BadgeLayout");
        ConfigureBadge();
    }

    /// <summary>
    /// Measures the button and returns a square whose side is the larger of the measured width and height.
    /// When the theme switches the button to content measurement, through the internal <c>UseContentMeasure</c>
    /// flag, the natural size is returned instead and the available width is limited so that the description fits
    /// into the scoped overlay layer. Overrides must call the base implementation to keep this behavior.
    /// </summary>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (UseContentMeasure)
        {
            if (!double.IsFinite(Width))
            {
                availableSize = availableSize.WithWidth(Math.Min(availableSize.Width, GetDescriptionWidthLimit()));
            }
            return base.MeasureOverride(availableSize);
        }

        var size       = base.MeasureOverride(availableSize);
        var effectSize = Math.Max(size.Width, size.Height);
        return new Size(effectSize, effectSize);
    }

    private double GetDescriptionWidthLimit()
    {
        var width = double.IsFinite(MaxWidth) ? MaxWidth : MinWidth * 2.5;
        if (_overlayLayer is { Bounds.Width: > 0 } layer)
        {
            var offset = Placement is FloatButtonPlacement.Top or FloatButtonPlacement.Bottom or FloatButtonPlacement.Center
                ? 2 * Math.Abs(FloatOffsetX)
                : Math.Max(0, FloatOffsetX);
            width = Math.Min(width, layer.Bounds.Width - offset);
        }
        return Math.Max(MinWidth, width);
    }

    /// <summary>
    /// Called when the value of a property of the button changes. It updates the pseudo classes, the layer the
    /// button is positioned in, the position and the drag translation, and the badge, its offset and its color.
    /// Overrides must call the base implementation, otherwise these updates are skipped.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IconProperty ||
            change.Property == ContentProperty ||
            change.Property == ContentTemplateProperty ||
            change.Property == ShapeProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == IsEmbedModeProperty ||
            change.Property == ParentProperty)
        {
            SetupParentLayer(Parent);
            if (_overlayLayer != null)
            {
                CalculatePosition(this, _overlayLayer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == FloatOffsetXProperty ||
                 change.Property == FloatOffsetYProperty ||
                 change.Property == PlacementProperty)
        {
            if (UseContentMeasure &&
                (change.Property == FloatOffsetXProperty || change.Property == PlacementProperty))
            {
                InvalidateMeasure();
            }
            if (!_isCommittingDrag && _overlayLayer is { } layer)
            {
                CalculatePosition(this, layer.Bounds.Size, Placement, FloatOffsetX, FloatOffsetY);
            }
        }
        else if (change.Property == DragTranslationProperty)
        {
            UpdateDragVisual();
        }
        else if (change.Property == IsBadgeEnabledProperty ||
                 change.Property == IsDotBadgeProperty)

        {
            ConfigureBadge();
        }
        else if (change.Property == BadgeOffsetProperty)
        {
            CalculateBadgePosition();
        }
        else if (change.Property == BadgeColorProperty)
        {
            ConfigureBadgeColor();
        }
    }

    /// <summary>
    /// Called when the button is attached to a visual tree. It resolves the badge color against the theme of that
    /// tree, keeps resolving it whenever the theme changes, and re-applies a non-zero
    /// <see cref="DragTranslation" />. Overrides must call the base implementation, otherwise the badge color stays
    /// unresolved.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Preset badge colors come from the snapshot's palettes: resolve them for this attach's context (a XAML value
        // set before attach has none) and again whenever the theme publishes.
        _themeSubscription?.Dispose();
        _themeSubscription = s_themeTokenResolver.Subscribe(this, _ => ConfigureBadgeColor());
        ConfigureBadgeColor();
        if (DragTranslation != default)
        {
            UpdateDragVisual();
        }
    }

    /// <summary>
    /// Called when the button is detached from its visual tree. It disposes the subscription that keeps the badge
    /// color resolved. Overrides must call the base implementation, otherwise the subscription is not disposed.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Writes the resolved <see cref="BadgeColor" /> into the internal <see cref="BadgeEffectiveColor" /> the badge
    /// binds, or clears it so the theme's ColorError applies when the color is unset, invalid or a status name
    /// without a theme.
    /// </summary>
    private void ConfigureBadgeColor()
    {
        if (BadgeColorUtils.CalculateColor(this, BadgeColor) is { } brush)
        {
            BadgeEffectiveColor = brush;
        }
        else
        {
            ClearValue(BadgeEffectiveColorProperty);
        }
    }

    /// <summary>
    /// Called when the button is initialized. It installs the default icon through
    /// <see cref="ConfigureDefaultIcon" />. Overrides must call the base implementation, otherwise a button without
    /// an explicit icon shows no icon.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        ConfigureDefaultIcon();

    }

    /// <summary>
    /// Updates the <c>:description</c> and <c>:plain-description</c> pseudo classes from the content and its
    /// template, and the <c>:icononly</c> pseudo class from the shape, the icon and the content. This class calls
    /// it whenever the icon, the content, the content template or the shape changes. Overrides must call the base
    /// implementation to keep the theme selectors working.
    /// </summary>
    protected virtual void UpdatePseudoClasses()
    {
        PseudoClasses.Set(":description", Content is not null);
        PseudoClasses.Set(":plain-description", Content is string && ContentTemplate is null);
        if (Shape == FloatButtonShape.Square)
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, Icon is not null && Content is null);
        }
        else
        {
            PseudoClasses.Set(ButtonPseudoClass.IconOnly, true);
        }
    }

    /// <summary>
    /// Sets the default icon when the button is initialized without one: <c>FileTextOutlined</c> for a circle, and
    /// for a square only when it has no description either. The icon is written as a current value, so a binding on
    /// <see cref="Icon" /> stays attached. Override it to install another default icon.
    /// </summary>
    protected virtual void ConfigureDefaultIcon()
    {
        if ((Shape == FloatButtonShape.Circle && Icon == null) ||
            (Shape == FloatButtonShape.Square && Content == null && Icon == null))
        {
            SetCurrentValue(IconProperty, new FileTextOutlined());
        }
    }

    private void SetupParentLayer(StyledElement? parent)
    {
        if (_overlayLayer != null)
        {
            _overlayLayer.SizeChanged -= HandleLayerSizeChanged;
        }
        _overlayLayer = IsEmbedMode ? null : FloatingSurfaceHost.ResolveLayer(parent);
        if (_overlayLayer is not null)
        {
            _overlayLayer.SizeChanged += HandleLayerSizeChanged;
        }
        if (UseContentMeasure)
        {
            InvalidateMeasure();
        }
    }

    private void HandleLayerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (UseContentMeasure && e.PreviousSize.Width != e.NewSize.Width)
        {
            InvalidateMeasure();
        }
        CalculatePosition(this, e.NewSize, Placement, FloatOffsetX, FloatOffsetY);
    }

    /// <summary>
    /// Creates, updates or removes the badge of the button so that it matches <c>IsBadgeEnabled</c> and
    /// <c>IsDotBadge</c>; <c>FloatButton</c> and <c>BackTopFloatButton</c> of AtomUI.Desktop.Controls
    /// override it.
    /// </summary>
    /// <remarks>
    /// The base class calls it after the control template is applied, once <c>PART_BadgeLayout</c> has been looked
    /// up, and whenever <c>IsBadgeEnabled</c> or <c>IsDotBadge</c> changes. The color of the badge is refreshed
    /// separately through <c>ConfigureBadgeColor</c>.
    /// </remarks>
    private protected abstract void ConfigureBadge();
    /// <summary>
    /// Positions the badge at the top-right corner of the button, moved by <see cref="BadgeOffset" />. It does
    /// nothing while the badge is disabled or absent. This class calls it when the size of the button or the badge
    /// offset changes. The base implementation only sets the position of the badge in its canvas, so an override
    /// that positions the badge itself need not call it.
    /// </summary>
    private protected virtual void CalculateBadgePosition()
    {
        if (IsBadgeEnabled && _badge != null)
        {
            var targetOffsetX = DesiredSize.Width;
            var targetOffsetY = 0.0d;
            targetOffsetX += BadgeOffset.X;
            targetOffsetY += BadgeOffset.Y;
            Canvas.SetLeft(_badge, targetOffsetX);
            Canvas.SetTop(_badge, targetOffsetY);
        }
    }

    #region 内部协作 API

    internal static void CalculatePosition(Control floatControl, 
                                           Size layerSize, 
                                           FloatButtonPlacement placement,
                                           double offsetX,
                                           double offsetY)
    {
        var width         = floatControl.DesiredSize.Width;
        var height        = floatControl.DesiredSize.Height;
        var targetOffsetX = 0.0d;
        var targetOffsetY = 0.0d;
        if (placement == FloatButtonPlacement.Left)
        {
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Right)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX -= offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Top)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.Bottom)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX += offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.TopLeft)
        {
            targetOffsetX =  0.0d;
            targetOffsetY =  0.0d;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.TopRight)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  0.0d;
            targetOffsetX -= offsetX;
            targetOffsetY += offsetY;
        }
        else if (placement == FloatButtonPlacement.BottomLeft)
        {
            targetOffsetX =  0.0d;
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX += offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.BottomRight)
        {
            targetOffsetX =  (layerSize.Width - width);
            targetOffsetY =  (layerSize.Height - height);
            targetOffsetX -= offsetX;
            targetOffsetY -= offsetY;
        }
        else if (placement == FloatButtonPlacement.Center)
        {
            targetOffsetX =  (layerSize.Width - width) / 2;
            targetOffsetY =  (layerSize.Height - height) / 2;
            targetOffsetX += offsetX;
            targetOffsetY += offsetY;
        }
        Canvas.SetLeft(floatControl, targetOffsetX);
        Canvas.SetTop(floatControl, targetOffsetY);
    }

    #endregion
}
