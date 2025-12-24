using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Palette;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum TagStatus
{
    // 状态
    Success,
    Info,
    Error,
    Warning
}

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

public class Tag : TemplatedControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义

    public static readonly StyledProperty<string?> TagColorProperty =
        AvaloniaProperty.Register<Tag, string?>(
            nameof(Color));

    public static readonly StyledProperty<bool> IsClosableProperty =
        AvaloniaProperty.Register<Tag, bool>(nameof(IsClosable));

    public static readonly StyledProperty<bool> BorderedProperty =
        AvaloniaProperty.Register<Tag, bool>(nameof(Bordered), true);

    public static readonly StyledProperty<PathIcon?> IconProperty =
        AvaloniaProperty.Register<Tag, PathIcon?>(nameof(Icon));

    public static readonly StyledProperty<PathIcon?> CloseIconProperty =
        AvaloniaProperty.Register<Tag, PathIcon?>(nameof(CloseIcon));

    public static readonly StyledProperty<string?> TagTextProperty =
        AvaloniaProperty.Register<Tag, string?>(
            nameof(TagText));

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

    public bool Bordered
    {
        get => GetValue(BorderedProperty);
        set => SetValue(BorderedProperty, value);
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
    public string? TagText
    {
        get => GetValue(TagTextProperty);
        set => SetValue(TagTextProperty, value);
    }

    #endregion

    #region 公共事件定义

    public static readonly RoutedEvent<RoutedEventArgs> ClosedEvent =
        RoutedEvent.Register<Tag, RoutedEventArgs>(nameof(Closed), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs>? Closed
    {
        add => AddHandler(ClosedEvent, value);
        remove => RemoveHandler(ClosedEvent, value);
    }
    
    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<Thickness> TagTextPaddingInlineProperty =
        AvaloniaProperty.Register<Tag, Thickness>(nameof(TagTextPaddingInline));

    internal static readonly DirectProperty<Tag, Thickness> RenderScaleAwareBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<Tag, Thickness>(nameof(RenderScaleAwareBorderThickness),
            o => o.RenderScaleAwareBorderThickness,
            (o, v) => o.RenderScaleAwareBorderThickness = v);

    internal static readonly DirectProperty<Tag, bool> IsPresetColorTagProperty =
        AvaloniaProperty.RegisterDirect<Tag, bool>(nameof(IsPresetColorTag),
            o => o.IsPresetColorTag,
            (o, v) => o.IsPresetColorTag = v);

    internal static readonly DirectProperty<Tag, bool> IsColorSetProperty =
        AvaloniaProperty.RegisterDirect<Tag, bool>(nameof(IsColorSet),
            o => o.IsColorSet,
            (o, v) => o.IsColorSet = v);

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

    private bool _isPresetColorTag;

    internal bool IsPresetColorTag
    {
        get => _isPresetColorTag;
        set => SetAndRaise(IsPresetColorTagProperty, ref _isPresetColorTag, value);
    }

    private bool _isColorSet;

    internal bool IsColorSet
    {
        get => _isColorSet;
        set => SetAndRaise(IsColorSetProperty, ref _isColorSet, value);
    }

    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => TagToken.ID;

    #endregion
    
    private static readonly Dictionary<PresetColorType, TagCalcColor> PresetColorMap;
    private static readonly Dictionary<TagStatus, TagStatusCalcColor> StatusColorMap;
    protected IconButton? CloseButton;
    
    static Tag()
    {
        PresetColorMap = new Dictionary<PresetColorType, TagCalcColor>();
        StatusColorMap = new Dictionary<TagStatus, TagStatusCalcColor>();
        AffectsMeasure<Tag>(BorderedProperty,
            IconProperty,
            IsClosableProperty,
            TagTextProperty);
        AffectsRender<Tag>(TagColorProperty,
            ForegroundProperty,
            BackgroundProperty,
            BorderBrushProperty);
    }

    public Tag()
    {
        this.RegisterResources();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (ThemeManager.Current != null)
        {
            ThemeManager.Current.ThemeChanged += HandleActualThemeVariantChanged;
        }
    }
    
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        if (ThemeManager.Current != null)
        {
            ThemeManager.Current.ThemeChanged -= HandleActualThemeVariantChanged;
        }
    }
    
    private void HandleActualThemeVariantChanged(object? sender, ThemeChangedEventArgs e)
    {
        SetupStatusColorMap(true);
        SetupPresetColorMap(true);
        if (TagColor is not null)
        {
            SetupTagColorInfo(TagColor);
        }
        InvalidateVisual();
    }
    
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (CloseButton != null)
        {
            CloseButton.Click -= HandleCloseRequest;
        }
        
        CloseButton = e.NameScope.Find<IconButton>(TagThemeConstants.CloseButtonPart);
        if (CloseButton != null)
        {
            CloseButton.Click += HandleCloseRequest;
        }
        SetupDefaultCloseIcon();
        SetupPresetColorMap();
        SetupStatusColorMap();
        if (TagColor is not null)
        {
            SetupTagColorInfo(TagColor);
        }
        ConfigureBorderThickness();
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
            if (change.Property == TagColorProperty)
            {
                if (TagColor is not null)
                {
                    SetupTagColorInfo(TagColor); 
                }
            }
            else if (change.Property == BorderedProperty)
            {
                ConfigureBorderThickness();
            }
        }
    }

    private void ConfigureBorderThickness()
    {
        if (Bordered)
        {
            SetValue(BorderThicknessProperty, RenderScaleAwareBorderThickness, BindingPriority.Template);
        }
        else
        {
            SetValue(BorderThicknessProperty, new Thickness(), BindingPriority.Template);
        }
    }

    // TODO 优化成静态变量
    private static void SetupPresetColorMap(bool force = false)
    {
        if (PresetColorMap.Count == 0 || force)
        {
            if (force)
            {
                PresetColorMap.Clear();
            }
            var activatedTheme = ThemeManager.Current?.ActivatedTheme;
            var sharedToken    = activatedTheme?.SharedToken;
            if (sharedToken == null)
            {
                // 是否需要输出日志
                return;
            }

            foreach (var entry in PresetPrimaryColor.AllColorTypes())
            {
                var colorMap = sharedToken.GetColorPalette(entry)!;
                var calcColor = new TagCalcColor
                {
                    LightColor       = colorMap.Color1,
                    LightBorderColor = colorMap.Color3,
                    DarkColor        = colorMap.Color6,
                    TextColor        = colorMap.Color7
                };
                PresetColorMap.Add(entry.Type, calcColor);
            }
        }
    }

    private static void SetupStatusColorMap(bool force = false)
    {
        if (StatusColorMap.Count == 0 || force)
        {
            if (force)
            {
                StatusColorMap.Clear();
            }
            var activatedTheme = ThemeManager.Current?.ActivatedTheme;
            var sharedToken    = activatedTheme?.SharedToken;
            if (sharedToken == null)
            {
                // 是否需要输出日志
                return;
            }

            StatusColorMap.Add(TagStatus.Success, new TagStatusCalcColor
            {
                Color       = sharedToken.ColorSuccess,
                Background  = sharedToken.ColorSuccessBg,
                BorderColor = sharedToken.ColorSuccessBorder
            });

            StatusColorMap.Add(TagStatus.Info, new TagStatusCalcColor
            {
                Color       = sharedToken.ColorInfo,
                Background  = sharedToken.ColorInfoBg,
                BorderColor = sharedToken.ColorInfoBorder
            });

            StatusColorMap.Add(TagStatus.Warning, new TagStatusCalcColor
            {
                Color       = sharedToken.ColorWarning,
                Background  = sharedToken.ColorWarningBg,
                BorderColor = sharedToken.ColorWarningBorder
            });

            StatusColorMap.Add(TagStatus.Error, new TagStatusCalcColor
            {
                Color       = sharedToken.ColorError,
                Background  = sharedToken.ColorErrorBg,
                BorderColor = sharedToken.ColorErrorBorder
            });
        }
    }

    private void SetupTagColorInfo(string colorStr)
    {
        IsPresetColorTag = false;
        IsColorSet       = false;
        colorStr          = colorStr.Trim().ToLower();

        foreach (var entry in PresetColorMap)
        {
            if (entry.Key.ToString().ToLower() == colorStr)
            {
                var colorInfo = PresetColorMap[entry.Key];
                Foreground        = new SolidColorBrush(colorInfo.TextColor);
                BorderBrush       = new SolidColorBrush(colorInfo.LightBorderColor);
                Background        = new SolidColorBrush(colorInfo.LightColor);
                IsPresetColorTag = true;
                PseudoClasses.Set(TagPseudoClass.PresetColor, true);
                PseudoClasses.Set(TagPseudoClass.StatusColor, false);
                PseudoClasses.Set(TagPseudoClass.CustomColor, false);
                return;
            }
        }

        foreach (var entry in StatusColorMap)
        {
            if (entry.Key.ToString().ToLower() == colorStr)
            {
                var colorInfo = StatusColorMap[entry.Key];
                Foreground        = new SolidColorBrush(colorInfo.Color);
                BorderBrush       = new SolidColorBrush(colorInfo.BorderColor);
                Background        = new SolidColorBrush(colorInfo.Background);
                IsPresetColorTag = true;
                PseudoClasses.Set(TagPseudoClass.PresetColor, false);
                PseudoClasses.Set(TagPseudoClass.StatusColor, true);
                PseudoClasses.Set(TagPseudoClass.CustomColor, false);
                return;
            }
        }

        if (Color.TryParse(colorStr, out var color))
        {
            Bordered   = false;
            IsColorSet = true;
            Background = new SolidColorBrush(color);
            PseudoClasses.Set(TagPseudoClass.PresetColor, false);
            PseudoClasses.Set(TagPseudoClass.StatusColor, false);
            PseudoClasses.Set(TagPseudoClass.CustomColor, true);
        }
    }

    private void SetupDefaultCloseIcon()
    {
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