using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using AtomUI.Reflection;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Base class of tag controls. It resolves the color scheme selected by <see cref="TagColor" /> and
/// <see cref="Variant" /> into the brushes that the theme applies, and runs the close button that raises
/// <see cref="Closed" />.
/// </summary>
[TemplatePart(CloseButtonPartName, typeof(AbstractIconButton), IsRequired = false)]
[TemplatePart(CloseButtonHostPartName, typeof(Decorator), IsRequired = false)]
public abstract class AbstractTag : TemplatedControl
{
    /// <summary>
    /// Name of the close button. Declared by the template, or created on demand into the host.
    /// </summary>
    protected const string CloseButtonPartName = "PART_CloseButton";

    /// <summary>
    /// Optional name of a <see cref="Decorator" /> in the template that hosts the close button. When the template
    /// has no <c>PART_CloseButton</c> but has this host, the button is created while <see cref="IsClosable" /> is true.
    /// </summary>
    protected const string CloseButtonHostPartName = "PART_CloseButtonHost";

    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="TagColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TagColorProperty =
        AvaloniaProperty.Register<AbstractTag, string?>(
            nameof(TagColor));

    /// <summary>
    /// Identifies the <see cref="IsClosable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<AbstractTag, bool>(nameof(IsClosable));

    /// <summary>
    /// Identifies the <see cref="Variant" /> property.
    /// </summary>
    public static readonly StyledProperty<TagVariant> VariantProperty =
        AvaloniaProperty.Register<AbstractTag, TagVariant>(
            nameof(Variant),
            TagVariant.Filled);

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractTag, PathIcon?>(nameof(Icon));

    /// <summary>
    /// Identifies the <see cref="CloseIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<AbstractTag, PathIcon?>(nameof(CloseIcon));

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractTag, string?>(
            nameof(Text));

    /// <summary>
    /// Gets or sets the color scheme of the tag, which paints the background, the text and the border in every
    /// <see cref="Variant" />. The value is a preset color name such as <c>red</c> or <c>geekblue</c> (a member of
    /// <c>PresetColorType</c>), a status name (<c>success</c>, <c>info</c>, <c>processing</c>, <c>warning</c>,
    /// <c>error</c>, <c>danger</c> or <c>primary</c>), or a color string for a custom color. Names ignore case, and
    /// surrounding whitespace is ignored.
    /// </summary>
    /// <remarks>
    /// <see langword="null" />, an empty string and <c>default</c> select no color, as does a value that the theme
    /// cannot resolve. The colors come from the theme that applies to the tag while it is attached to the visual
    /// tree, and they follow theme changes.
    /// </remarks>
    public string? TagColor
    {
        get => GetValue(TagColorProperty);
        set => SetValue(TagColorProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the tag shows a close button. Clicking the button raises <see cref="Closed" />.
    /// </summary>
    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    /// <summary>
    /// Gets or sets how the tag paints its background, text and border. The default is
    /// <see cref="TagVariant.Filled" />.
    /// </summary>
    /// <remarks>
    /// With a <see cref="TagColor" /> the variant selects among the colors of that scheme; without one it selects the
    /// neutral look that the theme defines for the variant.
    /// </remarks>
    public TagVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon shown before the text of the tag. <see langword="null" /> shows no icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// Gets or sets the icon of the close button. While <see cref="IsClosable" /> is <see langword="true" />,
    /// <see langword="null" /> is replaced by the default <see cref="CloseOutlined" /> icon. This happens when the
    /// template is applied, when this property changes, and when <see cref="IsClosable" /> changes while the tag is
    /// attached to the visual tree.
    /// </summary>
    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    /// <summary>
    /// Gets or sets the text of the tag. It is the XAML content property, so text written between the opening and
    /// closing tags of the tag is assigned here.
    /// </summary>
    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Identifies the <see cref="Closed" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<AbstractTag, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    /// <summary>
    /// Occurs when the close button of the tag is clicked. The event bubbles up the visual tree and cannot be
    /// canceled, and the tag is neither hidden nor removed, so the handler decides what happens to the tag.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Thickness> TagTextPaddingInlineProperty =
        AvaloniaProperty.Register<AbstractTag, Thickness>(nameof(TagTextPaddingInline));

    internal static readonly DirectProperty<AbstractTag, Thickness> RenderScaleAwareBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<AbstractTag, Thickness>(nameof(RenderScaleAwareBorderThickness),
            o => o.RenderScaleAwareBorderThickness,
            (o, v) => o.RenderScaleAwareBorderThickness = v);

    // Runtime palette colors are projected by untriggered ControlTheme setters (unset = the Token defaults the theme
    // gives these inputs). Keeping them in internal inputs instead of writing Background/Foreground/BorderBrush lets
    // app Styles, Semantic Part Styles and local values override the visuals natively.
    internal static readonly StyledProperty<IBrush?> CalculatedBackgroundProperty =
        AvaloniaProperty.Register<AbstractTag, IBrush?>(nameof(CalculatedBackground));

    internal static readonly StyledProperty<IBrush?> CalculatedForegroundProperty =
        AvaloniaProperty.Register<AbstractTag, IBrush?>(nameof(CalculatedForeground));

    internal static readonly StyledProperty<IBrush?> CalculatedBorderBrushProperty =
        AvaloniaProperty.Register<AbstractTag, IBrush?>(nameof(CalculatedBorderBrush));

    /// <summary>
    /// Identifies the <see cref="IsFilledStatusSolid" /> property.
    /// </summary>
    internal static readonly StyledProperty<bool> IsFilledStatusSolidProperty =
        AvaloniaProperty.Register<AbstractTag, bool>(nameof(IsFilledStatusSolid));

    /// <summary>
    /// Gets or sets the brush that the theme binds to <see cref="TemplatedControl.Background" />, which paints
    /// the frame of the tag. The theme sets it from the <c>DefaultBg</c> token, or, while <see cref="TagColor" />
    /// selects no color, to the neutral default of the <see cref="Variant" />.
    /// </summary>
    /// <remarks>
    /// While <see cref="TagColor" /> resolves to a color, the control overrides this value with the calculated
    /// color scheme, and clears the override when the color is empty or cannot be resolved, so the theme value
    /// applies again. A <see langword="null" /> brush paints nothing. A style or a local value on
    /// <see cref="TemplatedControl.Background" /> keeps precedence over this value.
    /// </remarks>
    internal IBrush? CalculatedBackground
    {
        get => GetValue(CalculatedBackgroundProperty);
        set => SetValue(CalculatedBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush that the theme binds to <see cref="TemplatedControl.Foreground" />, which paints
    /// the text, the icon and the close icon of the tag. The theme sets it from the <c>DefaultColor</c> token,
    /// or, while <see cref="TagColor" /> selects no color, to the neutral default of the <see cref="Variant" />.
    /// The theme of <c>SelectTag</c> sets it from the <c>ColorText</c> token.
    /// </summary>
    /// <remarks>
    /// While <see cref="TagColor" /> resolves to a color, the control overrides this value with the calculated
    /// color scheme, and clears the override when the color is empty or cannot be resolved, so the theme value
    /// applies again. A <see langword="null" /> brush paints nothing. A style or a local value on
    /// <see cref="TemplatedControl.Foreground" /> keeps precedence over this value.
    /// </remarks>
    internal IBrush? CalculatedForeground
    {
        get => GetValue(CalculatedForegroundProperty);
        set => SetValue(CalculatedForegroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush that the theme binds to <see cref="TemplatedControl.BorderBrush" />, which paints
    /// the outline of the frame of the tag. While <see cref="TagColor" /> selects no color, the theme sets it
    /// from the <c>ColorBorder</c> token for <see cref="TagVariant.Outlined" />, from the <c>ColorPrimary</c>
    /// token for <see cref="TagVariant.Outline" />, and to <c>Transparent</c> for the other variants.
    /// </summary>
    /// <remarks>
    /// While <see cref="TagColor" /> resolves to a color, the control overrides this value with the calculated
    /// color scheme, and clears the override when the color is empty or cannot be resolved, so the theme value
    /// applies again. A <see langword="null" /> brush paints nothing. A style or a local value on
    /// <see cref="TemplatedControl.BorderBrush" /> keeps precedence over this value.
    /// </remarks>
    internal IBrush? CalculatedBorderBrush
    {
        get => GetValue(CalculatedBorderBrushProperty);
        set => SetValue(CalculatedBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a <see cref="TagVariant.Filled" /> tag whose <see cref="TagColor" /> is a status
    /// color is painted with the status color as background and the <c>ColorTextLightSolid</c> token as text.
    /// The default, <see langword="false" />, keeps the light status background with the status color as text.
    /// The theme sets the value from the <c>FilledStatusSolid</c> token.
    /// </summary>
    /// <remarks>
    /// The value changes nothing for preset and custom colors and for the other variants. A change while the tag
    /// is attached to the visual tree recalculates <see cref="CalculatedBackground" />,
    /// <see cref="CalculatedForeground" /> and <see cref="CalculatedBorderBrush" />.
    /// </remarks>
    internal bool IsFilledStatusSolid
    {
        get => GetValue(IsFilledStatusSolidProperty);
        set => SetValue(IsFilledStatusSolidProperty, value);
    }

    /// <summary>
    /// Gets or sets the padding around the text of the tag, inside the <see cref="TemplatedControl.Padding" />
    /// of the tag. The theme sets the value from the <c>TagTextPaddingInline</c> token and binds it to the
    /// <c>Padding</c> of the text label. Without a value the label has no padding of its own.
    /// </summary>
    internal Thickness TagTextPaddingInline
    {
        get => GetValue(TagTextPaddingInlineProperty);
        set => SetValue(TagTextPaddingInlineProperty, value);
    }

    private Thickness _renderScaleAwareBorderThickness;

    /// <summary>
    /// Gets or sets the border thickness of the frame of the tag in device-independent pixels. The theme sets
    /// the value from the <c>BorderThickness</c> token. The control applies it to
    /// <see cref="TemplatedControl.BorderThickness" /> through a held value when the template is applied and
    /// whenever this property changes, so the frame follows Token changes.
    /// </summary>
    internal Thickness RenderScaleAwareBorderThickness
    {
        get => _renderScaleAwareBorderThickness;
        set => SetAndRaise(RenderScaleAwareBorderThicknessProperty, ref _renderScaleAwareBorderThickness, value);
    }

    #endregion
    
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();
    /// <summary>
    /// The close button in use: the <c>PART_CloseButton</c> of the template, or the button created into
    /// <c>PART_CloseButtonHost</c> while <see cref="IsClosable" /> is true. <see langword="null" />
    /// until the template is applied and whenever the tag has no close button.
    /// </summary>
    protected AbstractIconButton? CloseButton;
    private Decorator? _closeButtonHost;
    private bool _closeButtonCreatedOnDemand;
    private IDisposable? _closeIconBinding;
    private IDisposable? _themeSubscription;
    private (Color Background, Color Foreground, Color Border)? _calculatedColors;
    private IDisposable? _borderThicknessValue;
    private TagColorTable? _colorTable;

    // Null while the tag has no color to resolve: tags without a color never touch the theme snapshot.
    internal TagColorTable? ColorTable => _colorTable;
    internal bool IsThemeTracked => _themeSubscription is not null;

    static AbstractTag()
    {
        AffectsMeasure<AbstractTag>(IconProperty,
            IsClosableProperty,
            TextProperty);
        AffectsRender<AbstractTag>(TagColorProperty,
            VariantProperty,
            ForegroundProperty,
            BackgroundProperty,
            BorderBrushProperty);
    }

    /// <summary>
    /// Creates the close button for a template that hosts it in <c>PART_CloseButtonHost</c>. The base class
    /// names it <c>PART_CloseButton</c>, binds its icon to <see cref="CloseIcon" /> and handles its click;
    /// the subclass adds classes and other configuration. Not called for templates that declare the button.
    /// </summary>
    protected virtual AbstractIconButton? CreateCloseButton()
    {
        return null;
    }

    /// <summary>
    /// Resolves the color table for the current theme and applies the colors of <see cref="TagColor" />. A derived
    /// override must call the base method, otherwise the colors of <see cref="TagColor" /> are not applied.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Subscriptions do not survive a detach and do not fire on subscribe, so the table is re-resolved here.
        RefreshThemeTracking();
        UpdateTagColorVisualState();
    }

    /// <summary>
    /// Releases the theme subscription and the color table of the tag. A derived override must call the base method,
    /// otherwise the tag stays subscribed to the theme while it is detached.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseThemeTracking();
        base.OnDetachedFromVisualTree(e);
    }

    private static bool IsColorEmpty(string? color)
    {
        var span = color.AsSpan().Trim();
        return span.IsEmpty || span.Equals("default".AsSpan(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Keeps the theme subscription and color table in line with the current color: a tag without a color
    /// holds neither, and a colored tag resolves the table of the snapshot that applies to it right now.
    /// </summary>
    private void RefreshThemeTracking()
    {
        if (!this.IsAttachedToVisualTree() || IsColorEmpty(TagColor))
        {
            ReleaseThemeTracking();
            return;
        }

        _themeSubscription ??= s_themeTokenResolver.Subscribe(this, HandleThemeSnapshotChanged);
        _colorTable = ThemeScope.ResolveContext(this) is { } context ? TagColorTable.For(context.Snapshot) : null;
    }

    private void ReleaseThemeTracking()
    {
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        _colorTable = null;
    }

    private void HandleThemeSnapshotChanged(ThemeSnapshot snapshot)
    {
        _colorTable = TagColorTable.For(snapshot);
        UpdateTagColorVisualState();
    }

    /// <summary>
    /// Connects the close button of the new template, prepares the default close icon, and updates the colors and the
    /// border thickness of the tag. A derived override must call the base method first, before it uses
    /// <see cref="CloseButton" />.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        ReleaseCloseButton();
        CloseButton = e.NameScope.Find<AbstractIconButton>(CloseButtonPartName);
        if (CloseButton != null)
        {
            CloseButton.Click += HandleCloseRequest;
        }
        else
        {
            _closeButtonHost = e.NameScope.Find<Decorator>(CloseButtonHostPartName);
        }
        SetupDefaultCloseIcon();
        SyncCloseButton();
        RefreshThemeTracking();
        UpdateTagColorVisualState();
        ConfigureBorderThickness();
    }

    /// <summary>
    /// Updates the close button, the default close icon and the calculated colors after <see cref="CloseIcon" />,
    /// <see cref="IsClosable" />, <see cref="TagColor" />, <see cref="Variant" /> or
    /// <see cref="IsFilledStatusSolid" /> changed, and the border thickness after the theme input for it changed.
    /// A derived override must call the base method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CloseIconProperty)
        {
            SetupDefaultCloseIcon();
        }
        else if (change.Property == IsClosableProperty)
        {
            if (this.IsAttachedToVisualTree())
            {
                SetupDefaultCloseIcon();
            }
            SyncCloseButton();
        }
        else if (change.Property == TagColorProperty)
        {
            RefreshThemeTracking();
            if (this.IsAttachedToVisualTree())
            {
                UpdateTagColorVisualState();
            }
        }
        else if ((change.Property == VariantProperty || change.Property == IsFilledStatusSolidProperty) &&
                 this.IsAttachedToVisualTree())
        {
            UpdateTagColorVisualState();
        }
        else if (change.Property == RenderScaleAwareBorderThicknessProperty)
        {
            // The theme's BorderThickness Token reaches the border through this input; follow Token changes.
            ConfigureBorderThickness();
        }
    }

    // Written on every template application. The hold handle is replaced instead of stacked, so property frames
    // do not pile up.
    private void ConfigureBorderThickness() =>
        this.SetHeldValue(BorderThicknessProperty, RenderScaleAwareBorderThickness, ref _borderThicknessValue);

    private void UpdateTagColorVisualState()
    {
        if (!TryApplyTagColor() && _calculatedColors is not null)
        {
            // Fall back to the ControlTheme Token defaults of the Calculated* inputs.
            _calculatedColors = null;
            ClearValue(CalculatedBackgroundProperty);
            ClearValue(CalculatedForegroundProperty);
            ClearValue(CalculatedBorderBrushProperty);
            InvalidateVisual();
        }
    }

    private bool TryApplyTagColor()
    {
        PseudoClasses.Set(TagPseudoClass.PresetColor, false);
        PseudoClasses.Set(TagPseudoClass.StatusColor, false);
        PseudoClasses.Set(TagPseudoClass.CustomColor, false);

        var colorSpan = TagColor.AsSpan().Trim();
        if (IsColorEmpty(TagColor) || _colorTable is not { } table)
        {
            // Off-tree tags get their colors once the theme subscription attaches a context.
            return false;
        }

        // Status and solid text colors are the Tag's effective global Tokens, so Controls[Tag] and scoped overrides
        // reach them; preset palettes belong to the snapshot. A subclass without a registered Token identity reads the
        // snapshot's globals.
        var snapshot    = s_themeTokenResolver.Capture(this);
        int? controlSlot = snapshot.Registry.TryGetControl(GetType(), out var descriptor) ? descriptor.Slot : null;
        Color Global(SharedTokenKind kind) => controlSlot is { } slot
            ? s_themeTokenResolver.GetEffectiveGlobal<Color>(snapshot, slot, (int)kind)
            : s_themeTokenResolver.GetGlobal<Color>(snapshot, (int)kind);

        var solidTextColor = Global(SharedTokenKind.ColorTextLightSolid);
        if (table.TryGetPreset(colorSpan, out var presetColor))
        {
            ApplyPresetColor(presetColor, solidTextColor);
            PseudoClasses.Set(TagPseudoClass.PresetColor, true);
            return true;
        }

        if (table.TryGetStatus(colorSpan, out var statusColor))
        {
            ApplyStatusColor(ResolveEffectiveStatusColor(colorSpan, Global, controlSlot is { } own
                    ? status => s_themeTokenResolver.GetOwn<Color>(snapshot, own, StatusColorTokenKey(status)!)
                    : null) ?? statusColor,
                solidTextColor);
            PseudoClasses.Set(TagPseudoClass.StatusColor, true);
            return true;
        }

        if (Color.TryParse(colorSpan, out var color))
        {
            // antd lightens a custom color to a fixed HSL lightness, which stays near-white under a dark theme; the
            // palette algorithm of the snapshot's appearance gives the same step 1 color preset tags use.
            ApplyCustomColor(color, PresetColorPalette.ResolveCustom(snapshot, color).LightColor, solidTextColor);
            PseudoClasses.Set(TagPseudoClass.CustomColor, true);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns the key of the control token that holds the main color of a status tag, which lets a design language
    /// give a control its own status colors; <see langword="null" /> makes the tag read the global status color
    /// instead. Consulted when the colors of <see cref="TagColor" /> are resolved, and only for a derived tag
    /// type with a registered token identity.
    /// </summary>
    private protected virtual object? StatusColorTokenKey(TagStatus status) => null;

    private TagStatusCalcColor? ResolveEffectiveStatusColor(ReadOnlySpan<char> name, Func<SharedTokenKind, Color> global,
        Func<TagStatus, Color>? own)
    {
        (SharedTokenKind Color, SharedTokenKind Background, SharedTokenKind Border)? kinds =
            name.Equals(nameof(TagStatus.Success), StringComparison.OrdinalIgnoreCase)
                ? (SharedTokenKind.ColorSuccess, SharedTokenKind.ColorSuccessBg, SharedTokenKind.ColorSuccessBorder)
                : name.Equals(nameof(TagStatus.Info), StringComparison.OrdinalIgnoreCase) ||
                  name.Equals("processing", StringComparison.OrdinalIgnoreCase)
                    ? (SharedTokenKind.ColorInfo, SharedTokenKind.ColorInfoBg, SharedTokenKind.ColorInfoBorder)
                    : name.Equals(nameof(TagStatus.Warning), StringComparison.OrdinalIgnoreCase)
                        ? (SharedTokenKind.ColorWarning, SharedTokenKind.ColorWarningBg, SharedTokenKind.ColorWarningBorder)
                        : name.Equals(nameof(TagStatus.Error), StringComparison.OrdinalIgnoreCase)
                            ? (SharedTokenKind.ColorError, SharedTokenKind.ColorErrorBg, SharedTokenKind.ColorErrorBorder)
                            : null;
        if (kinds is not { } k)
        {
            return null;
        }

        var status = k.Color switch
        {
            SharedTokenKind.ColorSuccess => TagStatus.Success,
            SharedTokenKind.ColorInfo    => TagStatus.Info,
            SharedTokenKind.ColorWarning => TagStatus.Warning,
            _                            => TagStatus.Error
        };
        return new TagStatusCalcColor
        {
            Color       = own is not null && StatusColorTokenKey(status) is not null ? own(status) : global(k.Color),
            Background  = global(k.Background),
            BorderColor = global(k.Border)
        };
    }

    // Outline: transparent background, the main color for border and text.
    private void ApplyPresetColor(TagCalcColor colorInfo, Color solidText)
    {
        var (background, foreground, border) = Variant switch
        {
            TagVariant.Solid    => (colorInfo.DarkColor, solidText, colorInfo.DarkColor),
            TagVariant.Outlined => (colorInfo.LightColor, colorInfo.TextColor, colorInfo.LightBorderColor),
            TagVariant.Outline  => (Colors.Transparent, colorInfo.DarkColor, colorInfo.DarkColor),
            _                   => (colorInfo.LightColor, colorInfo.TextColor, Colors.Transparent)
        };
        ApplyCalculatedColors(background, foreground, border);
    }

    private void ApplyStatusColor(TagStatusCalcColor colorInfo, Color solidText)
    {
        var (background, foreground, border) = Variant switch
        {
            TagVariant.Solid    => (colorInfo.Color, solidText, colorInfo.Color),
            TagVariant.Outlined => (colorInfo.Background, colorInfo.Color, colorInfo.BorderColor),
            TagVariant.Outline  => (Colors.Transparent, colorInfo.Color, colorInfo.Color),
            _ when IsFilledStatusSolid => (colorInfo.Color, solidText, Colors.Transparent),
            _                   => (colorInfo.Background, colorInfo.Color, Colors.Transparent)
        };
        ApplyCalculatedColors(background, foreground, border);
    }

    private void ApplyCustomColor(Color color, Color lightBackground, Color solidText)
    {
        var (background, foreground, border) = Variant switch
        {
            TagVariant.Solid    => (color, solidText, Colors.Transparent),
            TagVariant.Outlined => (lightBackground, color, color),
            TagVariant.Outline  => (Colors.Transparent, color, color),
            _                   => (lightBackground, color, Colors.Transparent)
        };
        ApplyCalculatedColors(background, foreground, border);
    }

    private void ApplyCalculatedColors(Color background, Color foreground, Color border)
    {
        // Theme publications and variant refreshes re-run this projection; unchanged colors keep their brushes and
        // raise no property change.
        if (_calculatedColors == (background, foreground, border))
        {
            return;
        }
        _calculatedColors     = (background, foreground, border);
        CalculatedBackground  = new ImmutableSolidColorBrush(background);
        CalculatedForeground  = new ImmutableSolidColorBrush(foreground);
        CalculatedBorderBrush = new ImmutableSolidColorBrush(border);
        InvalidateVisual();
    }

    private void SetupDefaultCloseIcon()
    {
        if (!IsClosable)
        {
            return;
        }
        if (CloseIcon is null)
        {
            ClearValue(CloseIconProperty);
            SetValue(CloseIconProperty, new CloseOutlined());
        }
    }

    // Creates the close button into the template host while closable, removes it otherwise.
    private void SyncCloseButton()
    {
        if (_closeButtonHost is null)
        {
            return;
        }

        if (!IsClosable)
        {
            ReleaseCloseButton(keepHost: true);
            return;
        }

        if (CloseButton is not null || CreateCloseButton() is not { } button)
        {
            return;
        }

        button.Name = CloseButtonPartName;
        button.SetTemplatedParent(this);
        _closeIconBinding = button.Bind(AbstractIconButton.IconProperty, this.GetObservable(CloseIconProperty));
        button.Click += HandleCloseRequest;
        CloseButton = button;
        _closeButtonCreatedOnDemand = true;
        _closeButtonHost.Child = button;
    }

    private void ReleaseCloseButton(bool keepHost = false)
    {
        if (CloseButton is { } button)
        {
            button.Click -= HandleCloseRequest;
            if (_closeButtonCreatedOnDemand)
            {
                _closeIconBinding?.Dispose();
                if (ReferenceEquals(_closeButtonHost?.Child, button))
                {
                    _closeButtonHost.Child = null;
                }
                button.SetTemplatedParent(null);
            }
        }
        CloseButton = null;
        _closeIconBinding = null;
        _closeButtonCreatedOnDemand = false;
        if (!keepHost)
        {
            _closeButtonHost = null;
        }
    }

    private void HandleCloseRequest(object? sender, EventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }
}
