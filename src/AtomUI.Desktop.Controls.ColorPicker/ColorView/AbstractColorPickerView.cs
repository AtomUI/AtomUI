using AtomUI.Controls;
using AtomUI.Theme.Algorithms;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Interactivity;

namespace AtomUI.Desktop.Controls;

public abstract class AbstractColorPickerView : TemplatedControl, IMotionAwareControl
{
    #region 公共属性定义

    public static readonly StyledProperty<ColorSpectrumComponents> ColorSpectrumComponentsProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, ColorSpectrumComponents>(
            nameof(ColorSpectrumComponents),
            ColorSpectrumComponents.HueSaturation);

    public static readonly StyledProperty<ColorFormat> FormatProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, ColorFormat>(nameof(Format), ColorFormat.Hex);

    public static readonly StyledProperty<bool> IsFormatEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsFormatEnabled), true);

    public static readonly StyledProperty<bool> IsClearEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsClearEnabled));

    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsAlphaEnabled),
            true);

    public static readonly StyledProperty<bool> IsAlphaVisibleProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsAlphaVisible),
            true);

    public static readonly StyledProperty<bool> IsPaletteGroupEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsPaletteGroupEnabled),
            true);

    public static readonly StyledProperty<bool> IsColorSpectrumSliderVisibleProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(
            nameof(IsColorSpectrumSliderVisible),
            true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractColorPickerView>();

    public static readonly StyledProperty<int> MaxHueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MaxHue),
            359);

    public static readonly StyledProperty<int> MaxSaturationProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MaxSaturation),
            100);

    public static readonly StyledProperty<int> MaxValueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MaxValue),
            100);

    public static readonly StyledProperty<int> MinHueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MinHue),
            0);

    public static readonly StyledProperty<int> MinSaturationProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MinSaturation),
            0);

    public static readonly StyledProperty<int> MinValueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, int>(
            nameof(MinValue),
            0);

    public static readonly StyledProperty<List<ColorPickerPalette>?> PaletteGroupProperty =
        ColorPickerPaletteGroup.PaletteGroupProperty.AddOwner<AbstractColorPickerView>();

    public ColorSpectrumComponents ColorSpectrumComponents
    {
        get => GetValue(ColorSpectrumComponentsProperty);
        set => SetValue(ColorSpectrumComponentsProperty, value);
    }

    public ColorFormat Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    public bool IsFormatEnabled
    {
        get => GetValue(IsFormatEnabledProperty);
        set => SetValue(IsFormatEnabledProperty, value);
    }

    public bool IsClearEnabled
    {
        get => GetValue(IsClearEnabledProperty);
        set => SetValue(IsClearEnabledProperty, value);
    }

    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    public bool IsAlphaVisible
    {
        get => GetValue(IsAlphaVisibleProperty);
        set => SetValue(IsAlphaVisibleProperty, value);
    }

    public bool IsPaletteGroupEnabled
    {
        get => GetValue(IsPaletteGroupEnabledProperty);
        set => SetValue(IsPaletteGroupEnabledProperty, value);
    }

    public bool IsColorSpectrumSliderVisible
    {
        get => GetValue(IsColorSpectrumSliderVisibleProperty);
        set => SetValue(IsColorSpectrumSliderVisibleProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public int MaxHue
    {
        get => GetValue(MaxHueProperty);
        set => SetValue(MaxHueProperty, value);
    }

    public int MaxSaturation
    {
        get => GetValue(MaxSaturationProperty);
        set => SetValue(MaxSaturationProperty, value);
    }

    public int MaxValue
    {
        get => GetValue(MaxValueProperty);
        set => SetValue(MaxValueProperty, value);
    }

    public int MinHue
    {
        get => GetValue(MinHueProperty);
        set => SetValue(MinHueProperty, value);
    }

    public int MinSaturation
    {
        get => GetValue(MinSaturationProperty);
        set => SetValue(MinSaturationProperty, value);
    }

    public int MinValue
    {
        get => GetValue(MinValueProperty);
        set => SetValue(MinValueProperty, value);
    }

    public List<ColorPickerPalette>? PaletteGroup
    {
        get => GetValue(PaletteGroupProperty);
        set => SetValue(PaletteGroupProperty, value);
    }
    #endregion

    #region 公共事件定义

    public event EventHandler? ColorValueCleared;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<HsvColor> HsvValueProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, HsvColor>(
            nameof(HsvValue),
            Colors.White.ToHsv(),
            defaultBindingMode: BindingMode.TwoWay,
            coerce: CoerceHsvColor);

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

    internal static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsNeedConfirm));

    internal static readonly StyledProperty<bool> CanConfirmProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(CanConfirm), true);

    internal static readonly StyledProperty<bool> IsEmptyDraftProperty =
        AvaloniaProperty.Register<AbstractColorPickerView, bool>(nameof(IsEmptyDraft));

    internal HsvColor HsvValue
    {
        get => GetValue(HsvValueProperty);
        set => SetValue(HsvValueProperty, value);
    }

    private bool _isAlphaEffectiveVisible;
    internal bool IsAlphaEffectiveVisible
    {
        get => _isAlphaEffectiveVisible;
        set => SetAndRaise(IsAlphaEffectiveVisibleProperty, ref _isAlphaEffectiveVisible, value);
    }

    private readonly SolidColorBrush _hsvColorBrush = new(Colors.White);
    internal IBrush HsvColorBrush => _hsvColorBrush;

    private readonly SolidColorBrush _hsvColorWithoutAlphaBrush = new(Colors.White);
    internal IBrush HsvColorWithoutAlphaBrush => _hsvColorWithoutAlphaBrush;

    internal bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

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

    internal bool IsEditing { get; private set; }

    #endregion

    #region 内部事件定义

    internal event EventHandler? ConfirmRequested;
    internal event EventHandler? CancelRequested;

    #endregion

    #region 内部协作 API

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

    internal virtual void NotifyInputColorEdited()
    {
    }

    #endregion

    private Avalonia.Controls.Button? _confirmButton;
    private Avalonia.Controls.Button? _cancelButton;
    private ColorPickerInput? _colorInput;

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ReleaseFooterHandlers();
        base.OnDetachedFromVisualTree(e);
    }

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

    #region 内部协作 API

    internal static Color CoerceColor(AvaloniaObject instance, Color value)
    {
        if (instance is AbstractColorPickerView colorView)
        {
            return colorView.NotifyCoerceColor(value);
        }

        return value;
    }

    #endregion

    private static HsvColor CoerceHsvColor(AvaloniaObject instance, HsvColor value)
    {
        if (instance is AbstractColorPickerView colorView)
        {
            return colorView.NotifyCoerceHsvColor(value);
        }

        return value;
    }

    protected virtual Color NotifyCoerceColor(Color value)
    {
        if (IsAlphaEnabled == false)
        {
            return new Color(255, value.R, value.G, value.B);
        }

        return value;
    }

    protected virtual HsvColor NotifyCoerceHsvColor(HsvColor value)
    {
        if (IsAlphaEnabled == false)
        {
            return new HsvColor(1.0, value.H, value.S, value.V);
        }
        return value;
    }

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

    protected virtual void NotifyColorClearRequest()
    {
        HsvValue = new HsvColor(0.0, HsvValue.H, HsvValue.S, HsvValue.V);
        InvokeColorValueClearedEvent();
    }

    protected void InvokeColorValueClearedEvent()
    {
        ColorValueCleared?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void NotifyPaletteColorSelected(Color color)
    {
    }

}
