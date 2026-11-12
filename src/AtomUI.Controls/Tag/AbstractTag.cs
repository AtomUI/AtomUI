using System.Reactive.Disposables;
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
    private CompositeDisposable? _calculatedVisualValues;
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
        _calculatedVisualValues?.Dispose();
        _calculatedVisualValues = null;
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
        _calculatedVisualValues?.Dispose();
        _calculatedVisualValues = null;
        PseudoClasses.Set(TagPseudoClass.PresetColor, false);
        PseudoClasses.Set(TagPseudoClass.StatusColor, false);
        PseudoClasses.Set(TagPseudoClass.CustomColor, false);

        var colorSpan = TagColor.AsSpan().Trim();
        if (IsColorEmpty(TagColor))
        {
            InvalidateVisual();
            return;
        }

        var table = _colorTable;
        if (table is not null)
        {
            if (table.TryGetPreset(colorSpan, out var presetColor))
            {
                ApplyPresetColor(presetColor, table.SolidTextColor);
                PseudoClasses.Set(TagPseudoClass.PresetColor, true);
                return;
            }

            if (table.TryGetStatus(colorSpan, out var statusColor))
            {
                ApplyStatusColor(statusColor, table.SolidTextColor);
                PseudoClasses.Set(TagPseudoClass.StatusColor, true);
                return;
            }
        }

        if (Color.TryParse(colorSpan, out var color))
        {
            ApplyCustomColor(color, table?.SolidTextColor ?? default);
            PseudoClasses.Set(TagPseudoClass.CustomColor, true);
            return;
        }

        InvalidateVisual();
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

    private void ApplyCustomColor(Color color, Color solidText)
    {
        var hsl             = new HslColor(color);
        var lightBackground = new HslColor(hsl.A, hsl.H, hsl.S, 0.95).ToRgb();
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
