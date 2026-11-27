using AtomUI.Controls;
using AtomUI.Theme.Derivation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base class of <see cref="ColorPickerView" /> and <see cref="GradientColorPickerView" />, the views a color
/// picker shows for editing a color. It holds the settings that the color spectrum, the sliders, the palette
/// group and the color input of the template are bound to, keeps the color being edited as an HSV value, and
/// runs the confirm, cancel and clear flow between those parts.
/// </summary>
/// <remarks>
/// A derived view hooks in through <see cref="NotifyCoerceColor" />, <see cref="NotifyCoerceHsvColor" />,
/// <see cref="NotifyColorClearRequest" /> and <see cref="NotifyPaletteColorSelected" />.
/// </remarks>
public abstract class AbstractColorPickerView : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="ColorSpectrumComponents" /> property.
    /// </summary>
    public static readonly StyledProperty<ColorSpectrumComponents> ColorSpectrumComponentsProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, ColorSpectrumComponents>(
            nameof(ColorSpectrumComponents),
            ColorSpectrumComponents.HueSaturation);

    /// <summary>
    /// Identifies the <see cref="Format" /> property.
    /// </summary>
    public static readonly StyledProperty<ColorFormat> FormatProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, ColorFormat>(nameof(Format), ColorFormat.Hex);

    /// <summary>
    /// Identifies the <see cref="IsFormatEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFormatEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsFormatEnabled), true);

    /// <summary>
    /// Identifies the <see cref="IsClearEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClearEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsClearEnabled));

    /// <summary>
    /// Identifies the <see cref="IsAlphaEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsAlphaEnabled),
            true);

    /// <summary>
    /// Identifies the <see cref="IsAlphaVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaVisibleProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsAlphaVisible),
            true);

    /// <summary>
    /// Identifies the <see cref="IsPaletteGroupEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPaletteGroupEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsPaletteGroupEnabled),
            true);

    /// <summary>
    /// Identifies the <see cref="IsColorSpectrumSliderVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsColorSpectrumSliderVisibleProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsColorSpectrumSliderVisible),
            true);

    /// <summary>
    /// Identifies the <see cref="SliderTrackHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> SliderTrackHeightProperty =
        ColorPickerAttachedProperties.SliderTrackHeightProperty.AddOwner<AbstractColorPickerView>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractColorPickerView>();

    /// <summary>
    /// Identifies the <see cref="MaxHue" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxHueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MaxHue),
            359);

    /// <summary>
    /// Identifies the <see cref="MaxSaturation" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxSaturationProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MaxSaturation),
            100);

    /// <summary>
    /// Identifies the <see cref="MaxValue" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MaxValue),
            100);

    /// <summary>
    /// Identifies the <see cref="MinHue" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinHueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MinHue),
            0);

    /// <summary>
    /// Identifies the <see cref="MinSaturation" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinSaturationProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MinSaturation),
            0);

    /// <summary>
    /// Identifies the <see cref="MinValue" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MinValueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MinValue),
            0);

    /// <summary>
    /// Identifies the <see cref="PaletteGroup" /> property.
    /// </summary>
    public static readonly StyledProperty<List<ColorPickerPalette>?> PaletteGroupProperty =
        ColorPickerPaletteGroup.PaletteGroupProperty.AddOwner<AbstractColorPickerView>();

    /// <summary>
    /// The pair of HSV components that the two axes of the color spectrum represent, for a template that binds it to
    /// <c>Components</c> of the <see cref="ColorSpectrum" />. The built-in templates do not bind it, so there it has
    /// no effect. The default is <see cref="ColorSpectrumComponents.HueSaturation" />.
    /// </summary>
    public ColorSpectrumComponents ColorSpectrumComponents
    {
        get => GetValue(ColorSpectrumComponentsProperty);
        set => SetValue(ColorSpectrumComponentsProperty, value);
    }

    /// <summary>
    /// The text format the color input uses to show and read the color: <see cref="ColorFormat.Hex" />, <see
    /// cref="ColorFormat.Rgba" /> or <see cref="ColorFormat.Hsva" />. The default is <see cref="ColorFormat.Hex" />.
    /// </summary>
    /// <remarks>
    /// The value is bound two-way to the color input in the template, so a format selected in the format selector of
    /// the input is written back to this property.
    /// </remarks>
    public ColorFormat Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    /// <summary>
    /// Whether the color input shows its format selector, with which the user switches the text format. When
    /// <see langword="false" />, the selector is hidden. The default is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// The value is bound two-way to the color input in the template. When it changes to <see langword="false" />,
    /// the view also calls <c>SetCurrentValue</c> on <see cref="Format" />.
    /// </remarks>
    public bool IsFormatEnabled
    {
        get => GetValue(IsFormatEnabledProperty);
        set => SetValue(IsFormatEnabledProperty, value);
    }

    /// <summary>
    /// Whether the clear block is shown. A clear request from it makes the color fully transparent and raises
    /// <see cref="ColorValueCleared" />. The default is <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// The value is also passed on to the color input in the template.
    /// </remarks>
    public bool IsClearEnabled
    {
        get => GetValue(IsClearEnabledProperty);
        set => SetValue(IsClearEnabledProperty, value);
    }

    /// <summary>
    /// Whether the palette group of the view plays its motion. The value is passed to <c>PART_PaletteGroup</c> in the
    /// template.
    /// </summary>
    /// <remarks>
    /// The theme sets it from the <c>EnableMotion</c> motion token and to <see langword="false" /> while the internal
    /// <c>HostIsMotionEnabled</c> is <see langword="false" />. A value set on the view replaces both; clearing it
    /// falls back to the theme.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Whether the edited color may be transparent. While <see langword="false" />, an assigned color gets an opaque
    /// alpha, and the alpha slider and the alpha input are hidden. The default is <see langword="true" />.
    /// </summary>
    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    /// <summary>
    /// Whether the alpha slider and the alpha input of the color input are shown. It has no effect while
    /// <see cref="IsAlphaEnabled" /> is <see langword="false" />, because both are hidden then. The default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsAlphaVisible
    {
        get => GetValue(IsAlphaVisibleProperty);
        set => SetValue(IsAlphaVisibleProperty, value);
    }

    /// <summary>
    /// Whether the palette group is shown. The default is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// While it is <see langword="true" /> and <see cref="PaletteGroup" /> is <see langword="null" />, the view
    /// assigns the built-in palette group when it applies its template and whenever this property changes.
    /// </remarks>
    public bool IsPaletteGroupEnabled
    {
        get => GetValue(IsPaletteGroupEnabledProperty);
        set => SetValue(IsPaletteGroupEnabledProperty, value);
    }

    /// <summary>
    /// Whether the slider for the third component of the color spectrum, the one its two axes do not show, is visible
    /// below the spectrum. The default is <see langword="true" />.
    /// </summary>
    public bool IsColorSpectrumSliderVisible
    {
        get => GetValue(IsColorSpectrumSliderVisibleProperty);
        set => SetValue(IsColorSpectrumSliderVisibleProperty, value);
    }

    /// <summary>
    /// The height of the tracks of the sliders inside the view, in device-independent pixels. The value is set on the
    /// view and is inherited by each slider.
    /// </summary>
    /// <remarks>
    /// The theme of each view sets it from its <c>ColorSliderTrackSize</c> token. A value set here replaces that
    /// setting; clearing it falls back to the token.
    /// </remarks>
    public double SliderTrackHeight
    {
        get => GetValue(SliderTrackHeightProperty);
        set => SetValue(SliderTrackHeightProperty, value);
    }

    /// <summary>
    /// The largest hue the color spectrum offers, in degrees (0..359). The value is passed to the spectrum in the
    /// template, which requires it to be greater than <see cref="MinHue" />. The default is 359.
    /// </summary>
    public int MaxHue
    {
        get => GetValue(MaxHueProperty);
        set => SetValue(MaxHueProperty, value);
    }

    /// <summary>
    /// The largest saturation the color spectrum offers, in percent (0..100). The value is passed to the spectrum in
    /// the template, which requires it to be greater than <see cref="MinSaturation" />. The default is 100.
    /// </summary>
    public int MaxSaturation
    {
        get => GetValue(MaxSaturationProperty);
        set => SetValue(MaxSaturationProperty, value);
    }

    /// <summary>
    /// The largest value (brightness) the color spectrum offers, in percent (0..100). The value is passed to the
    /// spectrum in the template, which requires it to be greater than <see cref="MinValue" />. The default is 100.
    /// </summary>
    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    /// <summary>
    /// The smallest hue the color spectrum offers, in degrees (0..359). The value is passed to the spectrum in the
    /// template. The default is 0.
    /// </summary>
    public int MinHue
    {
        get => GetValue(MinHueProperty);
        set => SetValue(MinHueProperty, value);
    }

    /// <summary>
    /// The smallest saturation the color spectrum offers, in percent (0..100). The value is passed to the spectrum
    /// in the template. The default is 0.
    /// </summary>
    public int MinSaturation
    {
        get => GetValue(MinSaturationProperty);
        set => SetValue(MinSaturationProperty, value);
    }

    /// <summary>
    /// The smallest value (brightness) the color spectrum offers, in percent (0..100). The value is passed to the
    /// spectrum in the template. The default is 0.
    /// </summary>
    public int MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    /// <summary>
    /// The palettes listed in the palette group, or <see langword="null" /> to get the built-in group of the preset
    /// primary, red, green and cyan palettes.
    /// </summary>
    /// <remarks>
    /// The built-in group is assigned to this property when the template is applied and whenever
    /// <see cref="IsPaletteGroupEnabled" /> changes, while that property is <see langword="true" />.
    /// </remarks>
    public List<ColorPickerPalette>? PaletteGroup
    {
        get => GetValue(PaletteGroupProperty);
        set => SetValue(PaletteGroupProperty, value);
    }
    #endregion

    /// <summary>
    /// Raised after the clear block of the template (<c>PART_ClearColor</c>) asked the view to clear its color.
    /// The color is fully transparent when the handlers run.
    /// </summary>
    public event EventHandler? ColorValueCleared;

    #region 内部属性定义

    internal static readonly StyledProperty<bool> HostIsMotionEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(HostIsMotionEnabled), true);

#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
    internal static readonly StyledProperty<HsvColor> HsvValueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, HsvColor>(
            nameof(HsvValue),
            Colors.White.ToHsv(),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceHsvColor);
#pragma warning restore ATOMUIGEN044

    internal static readonly DirectProperty<AbstractColorPickerView, bool> IsAlphaEffectiveVisibleProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPickerView, bool>(
            nameof(IsAlphaEffectiveVisible),
            o => o.IsAlphaEffectiveVisible,
            (o, v) => o.IsAlphaEffectiveVisible = v);

    internal static readonly DirectProperty<AbstractColorPickerView, IBrush> HsvColorBrushProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPickerView, IBrush>(
            nameof(HsvColorBrush),
            o => o.HsvColorBrush);

    internal static readonly DirectProperty<AbstractColorPickerView, IBrush> HsvColorWithoutAlphaBrushProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPickerView, IBrush>(
            nameof(HsvColorWithoutAlphaBrush),
            o => o.HsvColorWithoutAlphaBrush);

    internal bool HostIsMotionEnabled
    {
        get => GetValue(HostIsMotionEnabledProperty);
        set => SetValue(HostIsMotionEnabledProperty, value);
    }

    internal HsvColor HsvValue
    {
        get => GetValue(HsvValueProperty);
        set => SetValue(HsvValueProperty, value);
    }

    private bool _isAlphaEffectiveVisible;
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
    private readonly SolidColorBrush _hsvColorBrush = new(Colors.White);
    private readonly SolidColorBrush _hsvColorWithoutAlphaBrush = new(Colors.White);
#pragma warning restore ATOMUIGEN044

    internal bool IsAlphaEffectiveVisible
    {
        get => _isAlphaEffectiveVisible;
        set => SetAndRaise(IsAlphaEffectiveVisibleProperty, ref _isAlphaEffectiveVisible, value);
    }

    internal IBrush HsvColorBrush => _hsvColorBrush;

    internal IBrush HsvColorWithoutAlphaBrush => _hsvColorWithoutAlphaBrush;
    #endregion

    internal static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsNeedConfirm));

    internal bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

    internal static readonly StyledProperty<bool> CanConfirmProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(CanConfirm), true);

    internal static readonly StyledProperty<bool> IsEmptyDraftProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsEmptyDraft));

    internal bool CanConfirm
    {
        get => GetValue(CanConfirmProperty);
        set => SetValue(CanConfirmProperty, value);
    }

    internal bool IsEmptyDraft
    {
        get => GetValue(IsEmptyDraftProperty);
        set => SetValue(IsEmptyDraftProperty, value);
    }

    internal event EventHandler? ConfirmRequested;
    internal event EventHandler? CancelRequested;
    internal bool IsEditing { get; private set; }

    internal void BeginEdit()
    {
        IsEditing = true;
        _colorInput?.ResetInput();
    }
    internal void EndEdit() => IsEditing = false;

    internal void RequestConfirm()
    {
        if (!IsNeedConfirm || !IsEditing || !(_colorInput?.TryCommitInput() ?? true))
        {
            return;
        }
        EndEdit();
        ConfirmRequested?.Invoke(this, EventArgs.Empty);
    }

    internal void RequestCancel()
    {
        if (!IsNeedConfirm || !IsEditing)
        {
            return;
        }
        EndEdit();
        CancelRequested?.Invoke(this, EventArgs.Empty);
    }

    private Avalonia.Controls.Button? _confirmButton;
    private Avalonia.Controls.Button? _cancelButton;
    private ColorPickerInput? _colorInput;

    internal virtual void NotifyInputColorEdited()
    {
    }

    private void HandleInputColorEdited(object? sender, EventArgs args)
    {
        if (IsNeedConfirm && IsEditing)
        {
            NotifyInputColorEdited();
        }
    }

    private void HandleConfirmClicked(object? sender, RoutedEventArgs args) => RequestConfirm();
    private void HandleCancelClicked(object? sender, RoutedEventArgs args) => RequestCancel();

    private void ReleaseFooterHandlers()
    {
        if (_colorInput != null)
        {
            _colorInput.ColorInputEdited -= HandleInputColorEdited;
        }
        if (_confirmButton != null)
        {
            _confirmButton.Click -= HandleConfirmClicked;
        }
        if (_cancelButton != null)
        {
            _cancelButton.Click -= HandleCancelClicked;
        }
    }

    private void AttachFooterHandlers()
    {
        ReleaseFooterHandlers();
        if (_colorInput != null)
        {
            _colorInput.ColorInputEdited += HandleInputColorEdited;
        }
        if (_confirmButton != null)
        {
            _confirmButton.Click += HandleConfirmClicked;
        }
        if (_cancelButton != null)
        {
            _cancelButton.Click += HandleCancelClicked;
        }
    }

    /// <summary>
    /// Releases the handlers of the color input and of the confirm and cancel buttons before the base member detaches
    /// the view, so that a detached view stops reacting to them.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseFooterHandlers();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Subscribes the view to the color input and to the confirm and cancel buttons when it is attached to a visual
    /// tree. This restores the handlers that <see cref="OnDetachedFromVisualTree" /> released.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AttachFooterHandlers();
    }

    private ColorBlock? _clearColorButton;
    private ColorPickerPaletteGroup? _paletteGroup;
    private protected bool IgnorePropertyChanged = false;

    public AbstractColorPickerView()
    {
        ConfigureHsvColorBrushes();
    }

    /// <summary>
    /// Keeps the view in step with changes of <see cref="IsAlphaEnabled" />, <see cref="IsAlphaVisible" />,
    /// <see cref="IsFormatEnabled" />, <see cref="IsPaletteGroupEnabled" /> and the internal <c>HsvValue</c>.
    /// </summary>
    /// <remarks>
    /// It recalculates <c>IsAlphaEffectiveVisible</c>, calls <c>SetCurrentValue</c> on <see cref="Format" /> when
    /// <see cref="IsFormatEnabled" /> becomes <see langword="false" />, creates the default palette group when
    /// <see cref="IsPaletteGroupEnabled" /> changes while <see cref="PaletteGroup" /> is <see langword="null" />, and
    /// calls <see cref="ConfigureHsvColorBrushes" /> when <c>HsvValue</c> changes.
    /// The base member runs first. An override must call it to keep this behavior.
    /// </remarks>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsAlphaEnabledProperty || change.Property == IsAlphaVisibleProperty)
        {
            ConfigureAlphaEffectiveVisible();
        }
        else if (change.Property == IsFormatEnabledProperty)
        {
            if (!IsFormatEnabled)
            {
                SetCurrentValue(FormatProperty, Format == ColorFormat.Hex);
            }
        }
        else if (change.Property == HsvValueProperty)
        {
            ConfigureHsvColorBrushes();
        }

        if (change.Property == IsPaletteGroupEnabledProperty)
        {
            if (IsPaletteGroupEnabled && PaletteGroup == null)
            {
                ConfigureDefaultPaletteGroup();
            }
        }
    }

    /// <summary>
    /// Recolors the two brushes the template binds to from the current <c>HsvValue</c>: <c>HsvColorBrush</c> takes
    /// the color with its alpha, and <c>HsvColorWithoutAlphaBrush</c> takes it as opaque. The brushes change in
    /// place, so the bound parts of the template follow.
    /// </summary>
    /// <remarks>
    /// The constructor calls it, and so does <see cref="OnPropertyChanged" /> whenever <c>HsvValue</c> changes.
    /// </remarks>
    protected void ConfigureHsvColorBrushes()
    {
        var hsvValue = HsvValue;
        _hsvColorBrush.Color = hsvValue.ToRgb();
        _hsvColorWithoutAlphaBrush.Color = HsvColor.ToRgb(hsvValue.H, hsvValue.S, hsvValue.V);
    }

    private void ConfigureAlphaEffectiveVisible()
    {
        if (!IsAlphaEnabled)
        {
            IsAlphaEffectiveVisible = false;
        }
        else
        {
            IsAlphaEffectiveVisible = IsAlphaVisible;
        }
    }

    internal static Color CoerceColor(AvaloniaObject instance, Color value)
    {
        if (instance is AbstractColorPickerView colorView)
        {
            return colorView.NotifyCoerceColor(value);
        }

        return value;
    }

    private static HsvColor CoerceHsvColor(AvaloniaObject instance, HsvColor value)
    {
        if (instance is AbstractColorPickerView colorView)
        {
            return colorView.NotifyCoerceHsvColor(value);
        }

        return value;
    }

    /// <summary>
    /// Coerces a color on its way into the color value of the view; <see cref="ColorPickerView" /> registers the
    /// internal <c>CoerceColor</c> callback for it. While <see cref="IsAlphaEnabled" /> is <see langword="false" />,
    /// the alpha channel is replaced with 255 (opaque); otherwise the color is returned unchanged.
    /// </summary>
    /// <param name="value">
    /// The color to coerce.
    /// </param>
    /// <returns>
    /// The color to store.
    /// </returns>
    /// <remarks>
    /// An override that must keep the alpha rule calls the base member.
    /// </remarks>
    protected virtual Color NotifyCoerceColor(Color value)
    {
        if (IsAlphaEnabled == false)
        {
            return new Color(255, value.R, value.G, value.B);
        }

        return value;
    }

    /// <summary>
    /// Coerces an HSV color on its way into <c>HsvValue</c>. While <see cref="IsAlphaEnabled" /> is
    /// <see langword="false" />, its alpha is replaced with 1.0 (opaque); otherwise the value is returned unchanged.
    /// </summary>
    /// <param name="value">
    /// The HSV color to coerce.
    /// </param>
    /// <returns>
    /// The HSV color to store.
    /// </returns>
    /// <remarks>
    /// An override that must keep the alpha rule calls the base member.
    /// </remarks>
    protected virtual HsvColor NotifyCoerceHsvColor(HsvColor value)
    {
        if (IsAlphaEnabled == false)
        {
            return new HsvColor(1.0, value.H, value.S, value.V);
        }
        return value;
    }

    /// <summary>
    /// Locates the parts of the template and wires them up: the color input (<c>PART_ColorInput</c>), the confirm
    /// and cancel buttons (<c>PART_ConfirmButton</c>, <c>PART_CancelButton</c>), the clear block
    /// (<c>PART_ClearColor</c>) and the palette group (<c>PART_PaletteGroup</c>). It also recalculates
    /// <c>IsAlphaEffectiveVisible</c> and creates the default palette group when it is missing.
    /// </summary>
    /// <remarks>
    /// An override must call the base member; without it the view does not react to these parts.
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ReleaseFooterHandlers();
        base.OnApplyTemplate(e);
        _confirmButton = e.NameScope.Find<Avalonia.Controls.Button>("PART_ConfirmButton");
        _cancelButton = e.NameScope.Find<Avalonia.Controls.Button>("PART_CancelButton");
        _colorInput = e.NameScope.Find<ColorPickerInput>("PART_ColorInput");
        AttachFooterHandlers();
        ConfigureAlphaEffectiveVisible();

        if (_clearColorButton != null)
        {
            _clearColorButton.ClearRequest -= HandleColorClearRequest;
        }
        if (_paletteGroup != null)
        {
            _paletteGroup.ColorSelected -= HandlePaletteColorSelected;
        }

        _clearColorButton = e.NameScope.Find<ColorBlock>("PART_ClearColor");
        if (_clearColorButton != null)
        {
            _clearColorButton.ClearRequest += HandleColorClearRequest;
        }
        if (IsPaletteGroupEnabled && PaletteGroup == null)
        {
            ConfigureDefaultPaletteGroup();
        }

        _paletteGroup = e.NameScope.Find<ColorPickerPaletteGroup>("PART_PaletteGroup");
        if (_paletteGroup != null)
        {
            _paletteGroup.ColorSelected += HandlePaletteColorSelected;
        }
    }

    private void HandlePaletteColorSelected(object? sender, ColorPickerPaletteColorSelectedEventArgs args)
    {
        NotifyPaletteColorSelected(args.SelectedColor);
    }

    private void ConfigureDefaultPaletteGroup()
    {
        PaletteGroup = new List<ColorPickerPalette>(4);
        {
            var paletteInfo = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Blue);
            PaletteGroup.Add(new ColorPickerPalette("primary", true, paletteInfo.ColorSequence));
        }
        {
            var paletteInfo = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Red);
            PaletteGroup.Add(new ColorPickerPalette("red", true, paletteInfo.ColorSequence));
        }
        {
            var paletteInfo = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Green);
            PaletteGroup.Add(new ColorPickerPalette("green", true, paletteInfo.ColorSequence));
        }
        {
            var paletteInfo = PresetPalettes.GetPresetPalette(PresetPrimaryColor.Cyan);
            PaletteGroup.Add(new ColorPickerPalette("cyan", true, paletteInfo.ColorSequence));
        }
    }

    private void HandleColorClearRequest(object? sender, EventArgs args)
    {
        NotifyColorClearRequest();
    }

    /// <summary>
    /// Called when the clear block of the template (<c>PART_ClearColor</c>) raises its <c>ClearRequest</c>. It makes
    /// <c>HsvValue</c> fully transparent, keeping hue, saturation and value, and then raises
    /// <see cref="ColorValueCleared" />.
    /// </summary>
    /// <remarks>
    /// An override that does not call the base member raises the event itself through
    /// <see cref="InvokeColorValueClearedEvent" />.
    /// </remarks>
    protected virtual void NotifyColorClearRequest()
    {
        HsvValue = new HsvColor(0.0, HsvValue.H, HsvValue.S, HsvValue.V);
        InvokeColorValueClearedEvent();
    }

    /// <summary>
    /// Raises <see cref="ColorValueCleared" />. A derived view that handles <see cref="NotifyColorClearRequest" />
    /// without calling the base member calls it once the color is fully transparent.
    /// </summary>
    protected void InvokeColorValueClearedEvent()
    {
        ColorValueCleared?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called with the color the user selected in the palette group of the template. The base implementation does
    /// nothing; a view that adopts palette colors overrides it.
    /// </summary>
    /// <param name="color">
    /// The color that was selected in the palette group.
    /// </param>
    protected virtual void NotifyPaletteColorSelected(Color color)
    {
    }

}
