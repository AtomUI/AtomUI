using System.Reactive.Disposables;
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
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

[TemplatePart(CloseButtonPartName, typeof(AbstractIconButton), IsRequired = false)]
[TemplatePart(CloseButtonHostPartName, typeof(Decorator), IsRequired = false)]
public abstract class AbstractTag : TemplatedControl
{
    /// <summary>Name of the close button. Declared by the template, or created on demand into the host.</summary>
    protected const string CloseButtonPartName = "PART_CloseButton";

    /// <summary>
    /// Optional name of a <see cref="Decorator" /> in the template that hosts the close button. When the template
    /// has no <c>PART_CloseButton</c> but has this host, the button is created while <see cref="IsClosable" /> is true.
    /// </summary>
    protected const string CloseButtonHostPartName = "PART_CloseButtonHost";

    #region 公共属性定义

    public static readonly StyledProperty<string?> TagColorProperty =
        AvaloniaProperty.Register<AbstractTag, string?>(
            nameof(TagColor));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<AbstractTag, bool>(nameof(IsClosable));

    public static readonly StyledProperty<TagVariant> VariantProperty =
        AvaloniaProperty.Register<AbstractTag, TagVariant>(
            nameof(Variant),
            TagVariant.Filled);

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<AbstractTag, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<AbstractTag, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractTag, string?>(
            nameof(Text));

    public string? TagColor
    {
        get => GetValue(TagColorProperty);
        set => SetValue(TagColorProperty, value);
    }

    public bool IsClosable
    {
        get => GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public TagVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public PathIcon? CloseIcon
    {
        get => GetValue(CloseIconProperty);
        set => SetValue(CloseIconProperty, value);
    }

    [Content]
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<AbstractTag, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

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

    // Runtime palette colours are projected by untriggered ControlTheme setters (unset = the Token defaults the theme
    // gives these inputs). Keeping them in internal inputs instead of writing Background/Foreground/BorderBrush lets
    // app Styles, Semantic Part Styles and local values override the visuals natively (consumption rule 7).
    internal static readonly StyledProperty<IBrush?> CalculatedBackgroundProperty =
        AvaloniaProperty.Register<AbstractTag, IBrush?>(nameof(CalculatedBackground));

    internal static readonly StyledProperty<IBrush?> CalculatedForegroundProperty =
        AvaloniaProperty.Register<AbstractTag, IBrush?>(nameof(CalculatedForeground));

    internal static readonly StyledProperty<IBrush?> CalculatedBorderBrushProperty =
        AvaloniaProperty.Register<AbstractTag, IBrush?>(nameof(CalculatedBorderBrush));

    internal IBrush? CalculatedBackground
    {
        get => GetValue(CalculatedBackgroundProperty);
        set => SetValue(CalculatedBackgroundProperty, value);
    }

    internal IBrush? CalculatedForeground
    {
        get => GetValue(CalculatedForegroundProperty);
        set => SetValue(CalculatedForegroundProperty, value);
    }

    internal IBrush? CalculatedBorderBrush
    {
        get => GetValue(CalculatedBorderBrushProperty);
        set => SetValue(CalculatedBorderBrushProperty, value);
    }

    internal Thickness TagTextPaddingInline
    {
        get => GetValue(TagTextPaddingInlineProperty);
        set => SetValue(TagTextPaddingInlineProperty, value);
    }

    private Thickness _renderScaleAwareBorderThickness;

    internal Thickness RenderScaleAwareBorderThickness
    {
        get => _renderScaleAwareBorderThickness;
        set => SetAndRaise(RenderScaleAwareBorderThicknessProperty, ref _renderScaleAwareBorderThickness, value);
    }

    #endregion
    
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Subscriptions do not survive a detach and do not fire on subscribe, so the table is re-resolved here.
        RefreshThemeTracking();
        UpdateTagColorVisualState();
    }

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
        else if (change.Property == VariantProperty && this.IsAttachedToVisualTree())
        {
            UpdateTagColorVisualState();
        }
    }

    // 每次套模板都会写入，持有句柄替换，避免累积属性帧
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
            // Off-tree tags get their colours once the theme subscription attaches a context.
            return false;
        }

        // Status and solid text colours are the Tag's effective global Tokens, so Controls[Tag] and scoped overrides
        // reach them; preset palettes belong to the snapshot.
        var (snapshot, controlSlot) = s_themeTokenResolver.CaptureControl(this);
        Color Global(SharedTokenKind kind) =>
            s_themeTokenResolver.GetEffectiveGlobal<Color>(snapshot, controlSlot, (int)kind);

        var solidTextColor = Global(SharedTokenKind.ColorTextLightSolid);
        if (table.TryGetPreset(colorSpan, out var presetColor))
        {
            ApplyPresetColor(presetColor, solidTextColor);
            PseudoClasses.Set(TagPseudoClass.PresetColor, true);
            return true;
        }

        if (table.TryGetStatus(colorSpan, out var statusColor))
        {
            ApplyStatusColor(ResolveEffectiveStatusColor(colorSpan, Global) ?? statusColor, solidTextColor);
            PseudoClasses.Set(TagPseudoClass.StatusColor, true);
            return true;
        }

        if (Color.TryParse(colorSpan, out var color))
        {
            // antd lightens a custom colour to a fixed HSL lightness, which stays near-white under a dark theme; the
            // palette algorithm of the snapshot's appearance gives the same step 1 colour preset tags use.
            ApplyCustomColor(color, PresetColorPalette.ResolveCustom(snapshot, color).LightColor, solidTextColor);
            PseudoClasses.Set(TagPseudoClass.CustomColor, true);
            return true;
        }

        return false;
    }

    private static TagStatusCalcColor? ResolveEffectiveStatusColor(ReadOnlySpan<char> name, Func<SharedTokenKind, Color> global)
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

        return new TagStatusCalcColor
        {
            Color       = global(k.Color),
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
        // Theme publications and variant refreshes re-run this projection; unchanged colours keep their brushes and
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
