using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

public partial class ColorPicker : AbstractColorPicker
{
    #region 公共属性定义
    public static readonly StyledProperty<Color?> DefaultValueProperty =
        AvaloniaProperty.Register<ColorPicker, Color?>(nameof(DefaultValue));

    public static readonly StyledProperty<Color?> ValueProperty =
        AvaloniaProperty.Register<ColorPicker, Color?>(
            nameof(Value),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    public static readonly AttachedProperty<Func<Color, ColorFormat, string>?> ColorTextFormatterProperty =
        AvaloniaProperty.RegisterAttached<ColorPicker, Control, Func<Color, ColorFormat, string>?>("ColorTextFormatter");

    public static readonly StyledProperty<ColorPickerValueSyncMode> ValueSyncStrategyProperty =
        AvaloniaProperty.Register<ColorPicker, ColorPickerValueSyncMode>(nameof(ValueSyncStrategy), ColorPickerValueSyncMode.Immediate);

    public Color? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public Color? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static Func<Color, ColorFormat, string>? GetColorTextFormatter(ColorPicker colorPicker)
    {
        return colorPicker.GetValue(ColorTextFormatterProperty);
    }

    public static void SetColorTextFormatter(ColorPicker colorPicker, Func<Color, ColorFormat, string> formatter)
    {
        colorPicker.SetValue(ColorTextFormatterProperty, formatter);
    }

    public ColorPickerValueSyncMode ValueSyncStrategy
    {
        get => GetValue(ValueSyncStrategyProperty);
        set => SetValue(ValueSyncStrategyProperty, value);
    }
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Keep distributing as long as there are changes
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ValueChanged;
    /// <summary>
    /// Dispatched once when Flyout is closed
    /// </summary>
    public event EventHandler<ColorSelectedEventArgs>? ValueSelected;
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<ColorPicker, string?> ColorTextProperty =
        AvaloniaProperty.RegisterDirect<ColorPicker, string?>(
            nameof(ColorText),
            o => o.ColorText,
            (o, v) => o.ColorText = v);

    private string? _colorText;

    internal string? ColorText
    {
        get => _colorText;
        set => SetAndRaise(ColorTextProperty, ref _colorText, value);
    }

    #endregion

    private ColorPickerView? _presenter;
    private Color? _latestSyncValue;
    private ColorBlock? _colorIndicator;

    static ColorPicker()
    {
        AffectsMeasure<ColorPicker>(ColorTextFormatterProperty);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (DefaultValue != null && Value == null)
        {
            SetCurrentValue(ValueProperty, DefaultValue);
        }

        if (Value == null)
        {
            ClearColor();
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty)
        {
            if (IsNeedConfirm && _presenter is { IsEditing: true })
            {
                ResetConfirmationDraft();
            }
            GenerateValueText();
            GenerateColorBlockBackground();
            NotifyValueChanged(new ColorChangedEventArgs(change.GetOldValue<Color?>(), change.GetNewValue<Color?>()));
            NotifyFormValueChanged();
        }

        if (change.Property == ValueSyncStrategyProperty && IsNeedConfirm && IsPickerOpen && !IsConfirmationOperationActive)
        {
            ResetConfirmationDraft();
        }

        if (change.Property == ColorTextFormatterProperty)
        {
            GenerateValueText();
        }
    }

    protected override void GenerateValueText()
    {
        if (IsTextVisible)
        {
            if (Value != null)
            {
                var customFormatter = GetColorTextFormatter(this);
                if (customFormatter != null)
                {
                    SetCurrentValue(ColorTextProperty, customFormatter(Value.Value, Format));
                }
                else
                {
                    SetCurrentValue(ColorTextProperty, FormatColor(Value.Value, Format));
                }
            }
            else
            {
                SetCurrentValue(ColorTextProperty, EmptyColorText);
                this[!ColorTextProperty] = this[!EmptyColorTextProperty];
            }
        }
    }

    protected override void GenerateColorBlockBackground()
    {
        if (Value == null)
        {
            SetColorBlockBackgroundBrushIfChanged(Brushes.Transparent);
            ClearColor();
        }
        else
        {
            if (_colorIndicator != null)
            {
                _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, false);
            }
            SetColorBlockBackgroundColorIfChanged(Value.Value);
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _colorIndicator = e.NameScope.Find<ColorBlock>("PART_ColorIndicator");
    }

    protected override Control CreatePresenter()
    {
        // The runtime-created presenter is outside the owner's template selector chain.
        // Its independent Content subtree cannot receive these bindings from ControlTheme.
        var presenter = new ColorPickerView();
        presenter[!ColorPickerView.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        presenter[!ColorPickerView.IsClearEnabledProperty] = this[!IsClearEnabledProperty];
        presenter[!ColorPickerView.FormatProperty] = this[!FormatProperty];
        presenter[!ColorPickerView.IsAlphaEnabledProperty] = this[!IsAlphaEnabledProperty];
        presenter[!ColorPickerView.IsFormatEnabledProperty] = this[!IsFormatEnabledProperty];
        presenter[!ColorPickerView.IsPaletteGroupEnabledProperty] = this[!IsPaletteGroupEnabledProperty];
        presenter[!ColorPickerView.PaletteGroupProperty] = this[!PaletteGroupProperty];

        return presenter;
    }

    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        _presenter = pickerPresenter as ColorPickerView;
    }

    private void HandleColorPickerViewValueChanged(object? sender, ColorChangedEventArgs args)
    {
        if (!IsNeedConfirm && ValueSyncStrategy == ColorPickerValueSyncMode.Immediate)
        {
            SetCurrentValue(ValueProperty, args.NewColor);
        }
        else if (!IsNeedConfirm)
        {
            _latestSyncValue = args.NewColor;
        }
    }

    protected override void NotifyPickerOpened()
    {
        base.NotifyPickerOpened();
        if (_presenter != null)
        {
            if (IsNeedConfirm)
            {
                _presenter.ResetDraft(Value);
            }
            var effectiveColor = Value ?? DefaultValue ?? Colors.White;
            if (!IsNeedConfirm)
            {
                _presenter.SetCurrentValue(ColorPickerView.ValueProperty, effectiveColor);
            }
            _latestSyncValue = effectiveColor;
            _presenter.ValueChanged += HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared += HandleColorCleared;
        }
    }

    protected override void NotifyPickerClosed()
    {
        if (_presenter != null)
        {
            _presenter.EndEdit();
            if (!IsNeedConfirm && !IsConfirmationOperationActive && ValueSyncStrategy == ColorPickerValueSyncMode.OnCompleted)
            {
                SetCurrentValue(ValueProperty, _latestSyncValue);
            }

            if (!IsNeedConfirm && !IsConfirmationOperationActive && Value != null)
            {
                ValueSelected?.Invoke(this, new ColorSelectedEventArgs(Value.Value));
            }
            _presenter.ValueChanged -= HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared -= HandleColorCleared;
        }
        base.NotifyPickerClosed();
    }

    internal override void ResetConfirmationDraft()
    {
        if (_presenter == null)
        {
            return;
        }
        _presenter.ValueChanged -= HandleColorPickerViewValueChanged;
        _presenter.ResetDraft(Value);
        _latestSyncValue = Value;
        if (IsPickerOpen)
        {
            _presenter.ValueChanged += HandleColorPickerViewValueChanged;
        }
    }

    internal override void CommitConfirmationDraft()
    {
        SetCurrentValue(ValueProperty, _presenter?.DraftValue);
        if (Value is { } value)
        {
            ValueSelected?.Invoke(this, new ColorSelectedEventArgs(value));
        }
    }

    internal void NotifyValueChanged(ColorChangedEventArgs e)
    {
        ValueChanged?.Invoke(this, e);
    }

    private void HandleColorCleared(object? sender, EventArgs args)
    {
        if (!IsNeedConfirm)
        {
            ClearColor();
        }
    }

    private void ClearColor()
    {
        SetCurrentValue(ValueProperty, null);
        _latestSyncValue = null;

        if (_colorIndicator != null)
        {
            _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, true);
            this[!ColorTextProperty] = this[!EmptyColorTextProperty];
        }
    }

    #region 实现 FormItem 接口
    protected override void NotifySetFormValue(object? value)
    {
        SetCurrentValue(ValueProperty, value as Color?);
    }

    protected override object? NotifyGetFormValue()
    {
        return Value;
    }

    protected override void NotifyClearFormValue()
    {
        ClearColor();
    }
    #endregion
}
