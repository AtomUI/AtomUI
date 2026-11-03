using System.Reactive.Disposables;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Algorithms;
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

internal struct TagCalcColor
{
    public Color LightColor { get; set; } // 1 号色
    public Color LightBorderColor { get; set; } // 3 号色
    public Color DarkColor { get; set; } // 6 号色
    public Color TextColor { get; set; } // 7 号色
}

internal struct TagStatusCalcColor
{
    public Color Color { get; set; }
    public Color Background { get; set; }
    public Color BorderColor { get; set; }
}

[TemplatePart("PART_CloseButton", typeof(AbstractIconButton))]
public abstract class AbstractTag : TemplatedControl, IThemeInheritanceSource
{
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
    private static readonly (string Name, PresetPrimaryColor Color)[] PresetColorEntries =
    [
        (nameof(PresetColorType.Red), PresetPrimaryColor.Red),
        (nameof(PresetColorType.Volcano), PresetPrimaryColor.Volcano),
        (nameof(PresetColorType.Orange), PresetPrimaryColor.Orange),
        (nameof(PresetColorType.Gold), PresetPrimaryColor.Gold),
        (nameof(PresetColorType.Yellow), PresetPrimaryColor.Yellow),
        (nameof(PresetColorType.Lime), PresetPrimaryColor.Lime),
        (nameof(PresetColorType.Green), PresetPrimaryColor.Green),
        (nameof(PresetColorType.Cyan), PresetPrimaryColor.Cyan),
        (nameof(PresetColorType.Blue), PresetPrimaryColor.Blue),
        (nameof(PresetColorType.GeekBlue), PresetPrimaryColor.GeekBlue),
        (nameof(PresetColorType.Purple), PresetPrimaryColor.Purple),
        (nameof(PresetColorType.Pink), PresetPrimaryColor.Pink),
        (nameof(PresetColorType.Magenta), PresetPrimaryColor.Magenta),
        (nameof(PresetColorType.Grey), PresetPrimaryColor.Grey)
    ];
    private static readonly (string Name, TagStatus Status)[] StatusColorEntries =
    [
        (nameof(TagStatus.Success), TagStatus.Success),
        (nameof(TagStatus.Info), TagStatus.Info),
        ("processing", TagStatus.Info),
        (nameof(TagStatus.Warning), TagStatus.Warning),
        (nameof(TagStatus.Error), TagStatus.Error)
    ];
    protected AbstractIconButton? CloseButton;
    private IReadOnlyDictionary<PresetColorType, TagCalcColor> _presetColorMap =
        new Dictionary<PresetColorType, TagCalcColor>();
    private IReadOnlyDictionary<TagStatus, TagStatusCalcColor> _statusColorMap =
        new Dictionary<TagStatus, TagStatusCalcColor>();
    private IDisposable? _themeSubscription;
    private CompositeDisposable? _calculatedVisualValues;
    private Color _solidTextColor;
    
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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _themeSubscription?.Dispose();
        _themeSubscription = s_themeTokenResolver.Subscribe(this, ApplyThemeSnapshot);
        if (GetValue(ThemeScope.ContextProperty) is { } context)
        {
            ApplyThemeSnapshot(context.Snapshot);
        }
        else
        {
            UpdateTagColorVisualState();
        }
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        _calculatedVisualValues?.Dispose();
        _calculatedVisualValues = null;
        base.OnDetachedFromVisualTree(e);
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (CloseButton != null)
        {
            CloseButton.Click -= HandleCloseRequest;
        }
        
        CloseButton = e.NameScope.Find<AbstractIconButton>("PART_CloseButton");
        if (CloseButton != null)
        {
            CloseButton.Click += HandleCloseRequest;
        }
        SetupDefaultCloseIcon();
        if (GetValue(ThemeScope.ContextProperty) is { } context)
        {
            ApplyThemeSnapshot(context.Snapshot);
        }
        else
        {
            UpdateTagColorVisualState();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CloseIconProperty)
        {
            SetupDefaultCloseIcon();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == TagColorProperty || change.Property == VariantProperty)
            {
                UpdateTagColorVisualState();
            }
            else if (change.Property == IsClosableProperty)
            {
                SetupDefaultCloseIcon();
            }
        }
    }

    #region 实现 IThemeInheritanceSource 接口

    AvaloniaObject? IThemeInheritanceSource.ThemeInheritanceParent => InheritanceParent;

    #endregion

    private void ApplyThemeSnapshot(ThemeSnapshot snapshot)
    {
        SetupStatusColorMap(snapshot);
        SetupPresetColorMap(snapshot);
        _solidTextColor = GetEffectiveGlobal<Color>(snapshot, SharedTokenKind.ColorTextLightSolid);
        UpdateTagColorVisualState();
    }

    private T GetEffectiveGlobal<T>(ThemeSnapshot snapshot, SharedTokenKind token)
    {
        // Color/variant calculations require runtime palette data rather than a
        // ControlTheme binding. Match the registered owner used by AXAML resources.
        var tokenSlot = snapshot.Registry.GetSharedTokenSlot(token);
        if (snapshot.Registry.TryGetControl(GetType(), out var descriptor) ||
            snapshot.Registry.TryGetControl(StyleKey, out descriptor))
        {
            return s_themeTokenResolver.GetEffectiveGlobal<T>(snapshot, descriptor.Slot, tokenSlot);
        }
        return s_themeTokenResolver.GetGlobal<T>(snapshot, tokenSlot);
    }

    private void SetupPresetColorMap(ThemeSnapshot snapshot)
    {
        var dict = new Dictionary<PresetColorType, TagCalcColor>(PresetColorEntries.Length);
        foreach (var entry in PresetColorEntries)
        {
            if (!snapshot.PresetColorPalettes.TryGetValue(entry.Color, out var palette))
            {
                continue;
            }

            dict[entry.Color.Type] = new TagCalcColor
            {
                LightColor       = palette.ColorSequence[0],
                LightBorderColor = palette.ColorSequence[2],
                DarkColor        = palette.ColorSequence[5],
                TextColor        = palette.ColorSequence[6]
            };
        }
        _presetColorMap = dict;
    }

    private void SetupStatusColorMap(ThemeSnapshot snapshot)
    {
        Color Global(SharedTokenKind kind)
        {
            return GetEffectiveGlobal<Color>(snapshot, kind);
        }

        _statusColorMap = new Dictionary<TagStatus, TagStatusCalcColor>(4)
        {
            [TagStatus.Success] = new TagStatusCalcColor
            {
                Color       = Global(SharedTokenKind.ColorSuccess),
                Background  = Global(SharedTokenKind.ColorSuccessBg),
                BorderColor = Global(SharedTokenKind.ColorSuccessBorder)
            },
            [TagStatus.Info] = new TagStatusCalcColor
            {
                Color       = Global(SharedTokenKind.ColorInfo),
                Background  = Global(SharedTokenKind.ColorInfoBg),
                BorderColor = Global(SharedTokenKind.ColorInfoBorder)
            },
            [TagStatus.Warning] = new TagStatusCalcColor
            {
                Color       = Global(SharedTokenKind.ColorWarning),
                Background  = Global(SharedTokenKind.ColorWarningBg),
                BorderColor = Global(SharedTokenKind.ColorWarningBorder)
            },
            [TagStatus.Error] = new TagStatusCalcColor
            {
                Color       = Global(SharedTokenKind.ColorError),
                Background  = Global(SharedTokenKind.ColorErrorBg),
                BorderColor = Global(SharedTokenKind.ColorErrorBorder)
            }
        };
    }

    private void UpdateTagColorVisualState()
    {
        _calculatedVisualValues?.Dispose();
        _calculatedVisualValues = null;
        PseudoClasses.Set(TagPseudoClass.PresetColor, false);
        PseudoClasses.Set(TagPseudoClass.StatusColor, false);
        PseudoClasses.Set(TagPseudoClass.CustomColor, false);

        var colorSpan = TagColor.AsSpan().Trim();
        if (colorSpan.IsEmpty || colorSpan.Equals("default".AsSpan(), StringComparison.OrdinalIgnoreCase))
        {
            InvalidateVisual();
            return;
        }

        foreach (var entry in PresetColorEntries)
        {
            if (entry.Name.AsSpan().Equals(colorSpan, StringComparison.OrdinalIgnoreCase) &&
                _presetColorMap.TryGetValue(entry.Color.Type, out var colorInfo))
            {
                ApplyPresetColor(colorInfo);
                PseudoClasses.Set(TagPseudoClass.PresetColor, true);
                return;
            }
        }

        foreach (var entry in StatusColorEntries)
        {
            if (entry.Name.AsSpan().Equals(colorSpan, StringComparison.OrdinalIgnoreCase) &&
                _statusColorMap.TryGetValue(entry.Status, out var colorInfo))
            {
                ApplyStatusColor(colorInfo);
                PseudoClasses.Set(TagPseudoClass.StatusColor, true);
                return;
            }
        }

        if (Color.TryParse(colorSpan, out var color))
        {
            ApplyCustomColor(color);
            PseudoClasses.Set(TagPseudoClass.CustomColor, true);
            return;
        }

        InvalidateVisual();
    }

    private void ApplyPresetColor(TagCalcColor colorInfo)
    {
        var background = Variant == TagVariant.Solid ? colorInfo.DarkColor : colorInfo.LightColor;
        var foreground = Variant == TagVariant.Solid ? _solidTextColor : colorInfo.TextColor;
        var border = Variant switch
        {
            TagVariant.Solid    => colorInfo.DarkColor,
            TagVariant.Outlined => colorInfo.LightBorderColor,
            _                   => Colors.Transparent
        };
        ApplyCalculatedColors(background, foreground, border);
    }

    private void ApplyStatusColor(TagStatusCalcColor colorInfo)
    {
        var background = Variant == TagVariant.Solid ? colorInfo.Color : colorInfo.Background;
        var foreground = Variant == TagVariant.Solid ? _solidTextColor : colorInfo.Color;
        var border = Variant switch
        {
            TagVariant.Solid    => colorInfo.Color,
            TagVariant.Outlined => colorInfo.BorderColor,
            _                   => Colors.Transparent
        };
        ApplyCalculatedColors(background, foreground, border);
    }

    private void ApplyCustomColor(Color color)
    {
        var hsl            = new HslColor(color);
        var lightBackground = new HslColor(hsl.A, hsl.H, hsl.S, 0.95).ToRgb();
        var background     = Variant == TagVariant.Solid ? color : lightBackground;
        var foreground     = Variant == TagVariant.Solid ? _solidTextColor : color;
        var border         = Variant == TagVariant.Outlined ? color : Colors.Transparent;
        ApplyCalculatedColors(background, foreground, border);
    }

    private void ApplyCalculatedColors(Color background, Color foreground, Color border)
    {
        var values = new CompositeDisposable(3);
        try
        {
            AddTemplateValue(values, BackgroundProperty, new ImmutableSolidColorBrush(background));
            AddTemplateValue(values, ForegroundProperty, new ImmutableSolidColorBrush(foreground));
            AddTemplateValue(values, BorderBrushProperty, new ImmutableSolidColorBrush(border));
            _calculatedVisualValues = values;
        }
        catch
        {
            values.Dispose();
            throw;
        }
        InvalidateVisual();
    }

    private void AddTemplateValue<T>(CompositeDisposable values, StyledProperty<T> property, T value)
    {
        if (SetValue(property, value, BindingPriority.Template) is { } disposable)
        {
            values.Add(disposable);
        }
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

    private void HandleCloseRequest(object? sender, EventArgs e)
    {
        RaiseEvent(new RoutedEventArgs(ClosedEvent, this));
    }
}
