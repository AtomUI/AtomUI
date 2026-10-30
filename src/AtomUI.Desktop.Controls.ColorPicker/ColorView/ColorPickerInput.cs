using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ColorPickerInput : TemplatedControl
{
    #region 公共属性定义
    public static readonly StyledProperty<ColorFormat> FormatProperty =
        AbstractColorPickerView.FormatProperty.AddOwner<ColorPickerInput>();

    public static readonly StyledProperty<bool> IsFormatEnabledProperty =
        AbstractColorPickerView.IsFormatEnabledProperty.AddOwner<ColorPickerInput>();

    public static readonly StyledProperty<HsvColor> ColorValueProperty =
        AvaloniaProperty.Register<ColorPickerInput, HsvColor>(nameof(ColorValue),
            Colors.White.ToHsv());

    public static readonly StyledProperty<bool> IsClearEnabledProperty =
        AbstractColorPickerView.IsClearEnabledProperty.AddOwner<ColorPickerInput>();

    public static readonly StyledProperty<bool> IsAlphaVisibleProperty =
        AbstractColorPickerView.IsAlphaVisibleProperty.AddOwner<ColorPickerInput>();

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

    public HsvColor ColorValue
    {
        get => GetValue(ColorValueProperty);
        set => SetValue(ColorValueProperty, value);
    }

    public bool IsClearEnabled
    {
        get => GetValue(IsClearEnabledProperty);
        set => SetValue(IsClearEnabledProperty, value);
    }

    public bool IsAlphaVisible
    {
        get => GetValue(IsAlphaVisibleProperty);
        set => SetValue(IsAlphaVisibleProperty, value);
    }
    #endregion

    internal static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AbstractColorPickerView.IsNeedConfirmProperty.AddOwner<ColorPickerInput>();

    internal static readonly StyledProperty<bool> IsInputValidProperty =
        AvaloniaProperty.Register<ColorPickerInput, bool>(nameof(IsInputValid), true);

    internal bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

    internal bool IsInputValid
    {
        get => GetValue(IsInputValidProperty);
        private set => SetCurrentValue(IsInputValidProperty, value);
    }

    internal event EventHandler? ColorInputEdited;

    private bool _hasPendingInput;

    internal void ResetInput()
    {
        using var scope = BeginIgnoringConfigureValues();
        ConfigureColorValues();
        foreach (var input in GetInputs().OfType<NumericUpDown>())
        {
            // Value equality must not preserve discarded invalid text.
            input.SetCurrentValue(NumericUpDown.TextProperty,
                input.Value?.ToString(input.FormatString, input.NumberFormat ?? CultureInfo.CurrentCulture.NumberFormat));
        }
        _hasPendingInput = false;
        IsInputValid = true;
    }

    internal bool TryCommitInput()
    {
        if (!TryReadInput(out var color))
        {
            IsInputValid = false;
            return false;
        }
        IsInputValid = true;
        if (_hasPendingInput)
        {
            using var scope = BeginIgnoringConfigureValues();
            SetCurrentValue(ColorValueProperty, color);
            ColorInputEdited?.Invoke(this, EventArgs.Empty);
            _hasPendingInput = false;
        }
        return true;
    }

    private bool TryReadInput(out HsvColor color)
    {
        color = ColorValue;
        if (_hexValueInput == null)
        {
            return true;
        }
        var alpha = ColorValue.A * 100;
        if (IsAlphaVisible && !TryReadNumeric(_alphaInput, out alpha))
        {
            return false;
        }
        if (Format == ColorFormat.Hex)
        {
            if (!Color.TryParse((_hexValueInput.Text ?? string.Empty).AsSpan().Trim(), out var rgb))
            {
                return false;
            }
            color = Color.FromArgb((byte)((decimal)alpha * 2.55m), rgb.R, rgb.G, rgb.B).ToHsv();
            return true;
        }
        if (Format == ColorFormat.Hsva)
        {
            if (!TryReadNumeric(_hValueInput, out var hue) || !TryReadNumeric(_sValueInput, out var saturation) ||
                !TryReadNumeric(_vValueInput, out var value))
            {
                return false;
            }
            color = HsvColor.FromAhsv(alpha / 100, hue, saturation / 100, value / 100);
            return true;
        }
        if (!TryReadNumeric(_rValueInput, out var red) || !TryReadNumeric(_gValueInput, out var green) ||
            !TryReadNumeric(_bValueInput, out var blue))
        {
            return false;
        }
        color = Color.FromArgb((byte)((decimal)alpha * 2.55m), (byte)red, (byte)green, (byte)blue).ToHsv();
        return true;
    }

    private static bool TryReadNumeric(NumericUpDown? input, out double value)
    {
        value = 0;
        if (input == null)
        {
            return false;
        }
        var text = input.Text?.Trim().TrimEnd('%').Trim();
        if (!decimal.TryParse(text, input.ParsingNumberStyle, input.NumberFormat ?? CultureInfo.CurrentCulture.NumberFormat, out var number) ||
            number < input.Minimum || number > input.Maximum)
        {
            return false;
        }
        value = (double)number;
        return true;
    }

    private void HandleConfirmationInputChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (!IsNeedConfirm || _ignoringConfigureValues ||
            (args.Property != LineEdit.TextProperty && args.Property != NumericUpDown.TextProperty))
        {
            return;
        }
        _hasPendingInput = true;
        IsInputValid = TryReadInput(out var color);
        if (IsInputValid)
        {
            using var scope = BeginIgnoringConfigureValues();
            SetCurrentValue(ColorValueProperty, color);
            ColorInputEdited?.Invoke(this, EventArgs.Empty);
        }
    }

    private IEnumerable<Control> GetInputs()
    {
        return new Control?[] { _hexValueInput, _alphaInput, _hValueInput, _sValueInput, _vValueInput,
            _rValueInput, _gValueInput, _bValueInput }.OfType<Control>();
    }

    private ComboBox? _colorFormatComboBox;
    private NumericUpDown? _alphaInput;
    private LineEdit? _hexValueInput;
    private NumericUpDown? _hValueInput;
    private NumericUpDown? _sValueInput;
    private NumericUpDown? _vValueInput;
    private NumericUpDown? _rValueInput;
    private NumericUpDown? _gValueInput;
    private NumericUpDown? _bValueInput;
    private bool _ignoringConfigureValues;
    private bool _alphaInputPassiveChanged;
    private bool _hexValueInputPassiveChanged;
    private bool _hValueInputPassiveChanged;
    private bool _sValueInputPassiveChanged;
    private bool _vValueInputPassiveChanged;
    private bool _rValueInputPassiveChanged;
    private bool _gValueInputPassiveChanged;
    private bool _bValueInputPassiveChanged;

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        foreach (var input in GetInputs())
        {
            input.PropertyChanged -= HandleConfirmationInputChanged;
        }
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        foreach (var input in GetInputs())
        {
            input.PropertyChanged -= HandleConfirmationInputChanged;
            input.PropertyChanged += HandleConfirmationInputChanged;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == ColorValueProperty)
            {
                if (_ignoringConfigureValues)
                {
                    return;
                }
                ResetInput();
            }
            else if (change.Property == FormatProperty)
            {
                ConfigureComboBoxSelected();
            }
        }
    }

    private void ConfigureComboBoxSelected()
    {
        if (_colorFormatComboBox != null)
        {
            if (Format == ColorFormat.Hex)
            {
                _colorFormatComboBox.SelectedIndex = 0;
            }
            else if (Format == ColorFormat.Hsva)
            {
                _colorFormatComboBox.SelectedIndex = 1;
            }
            else if (Format == ColorFormat.Rgba)
            {
                _colorFormatComboBox.SelectedIndex = 2;
            }
        }
    }

    private void HandleAlphaInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_alphaInputPassiveChanged)
        {
            _alphaInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleHexValueInputTextChanged(object? sender, TextChangedEventArgs args)
    {
        if (_hexValueInputPassiveChanged)
        {
            _hexValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleHValueInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_hValueInputPassiveChanged)
        {
            _hValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleSValueInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_sValueInputPassiveChanged)
        {
            _sValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleVValueInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_vValueInputPassiveChanged)
        {
            _vValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleRValueInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_rValueInputPassiveChanged)
        {
            _rValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleGValueInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_gValueInputPassiveChanged)
        {
            _gValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    private void HandleBValueInputValueChanged(object? sender, NumericUpDownValueChangedEventArgs args)
    {
        if (_bValueInputPassiveChanged)
        {
            _bValueInputPassiveChanged = false;
            return;
        }
        if (IsNeedConfirm)
        {
            return;
        }
        SyncInputValue();
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        foreach (var input in GetInputs())
        {
            input.PropertyChanged -= HandleConfirmationInputChanged;
        }
        if (_alphaInput != null)
        {
            _alphaInput.ValueChanged -= HandleAlphaInputValueChanged;
        }
        if (_hexValueInput != null)
        {
            _hexValueInput.TextChanged -= HandleHexValueInputTextChanged;
        }
        if (_hValueInput != null)
        {
            _hValueInput.ValueChanged -= HandleHValueInputValueChanged;
        }
        if (_sValueInput != null)
        {
            _sValueInput.ValueChanged -= HandleSValueInputValueChanged;
        }
        if (_vValueInput != null)
        {
            _vValueInput.ValueChanged -= HandleVValueInputValueChanged;
        }
        if (_rValueInput != null)
        {
            _rValueInput.ValueChanged -= HandleRValueInputValueChanged;
        }
        if (_gValueInput != null)
        {
            _gValueInput.ValueChanged -= HandleGValueInputValueChanged;
        }
        if (_bValueInput != null)
        {
            _bValueInput.ValueChanged -= HandleBValueInputValueChanged;
        }

        if (_colorFormatComboBox != null)
        {
            _colorFormatComboBox.SelectionChanged -= HandleFormatChanged;
        }

        base.OnApplyTemplate(e);
        _colorFormatComboBox = e.NameScope.Find<ComboBox>("PART_ColorFormatComboBox");
        _alphaInput = e.NameScope.Find<NumericUpDown>("PART_AlphaInput");
        _hexValueInput = e.NameScope.Find<LineEdit>("PART_HexValueInput");
        _hValueInput = e.NameScope.Find<NumericUpDown>("PART_HValueInput");
        _sValueInput = e.NameScope.Find<NumericUpDown>("PART_SValueInput");
        _vValueInput = e.NameScope.Find<NumericUpDown>("PART_VValueInput");
        _rValueInput = e.NameScope.Find<NumericUpDown>("PART_RValueInput");
        _gValueInput = e.NameScope.Find<NumericUpDown>("PART_GValueInput");
        _bValueInput = e.NameScope.Find<NumericUpDown>("PART_BValueInput");

        if (_colorFormatComboBox != null)
        {
            _colorFormatComboBox.SelectionChanged += HandleFormatChanged;
            ConfigureComboBoxSelected();
        }

        if (_alphaInput != null)
        {
            _alphaInput.ValueChanged += HandleAlphaInputValueChanged;
        }

        if (_hexValueInput != null)
        {
            _hexValueInput.TextChanged += HandleHexValueInputTextChanged;
        }

        if (_hValueInput != null)
        {
            _hValueInput.ValueChanged += HandleHValueInputValueChanged;
        }

        if (_sValueInput != null)
        {
            _sValueInput.ValueChanged += HandleSValueInputValueChanged;
        }

        if (_vValueInput != null)
        {
            _vValueInput.ValueChanged += HandleVValueInputValueChanged;
        }

        if (_rValueInput != null)
        {
            _rValueInput.ValueChanged += HandleRValueInputValueChanged;
        }

        if (_gValueInput != null)
        {
            _gValueInput.ValueChanged += HandleGValueInputValueChanged;
        }

        if (_bValueInput != null)
        {
            _bValueInput.ValueChanged += HandleBValueInputValueChanged;
        }
        foreach (var input in GetInputs())
        {
            input.PropertyChanged += HandleConfirmationInputChanged;
        }
        ResetInput();
    }

    private void HandleFormatChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (_colorFormatComboBox?.SelectedItem is ComboBoxItem comboBoxItem &&
            comboBoxItem.Tag is ColorFormat colorFormat)
        {
            Format = colorFormat;
            using var scope = BeginIgnoringConfigureValues();
            ResetInput();
        }
    }

    private void ConfigureColorValues()
    {
        _alphaInputPassiveChanged = false;
        _hexValueInputPassiveChanged = false;
        _hValueInputPassiveChanged = false;
        _sValueInputPassiveChanged = false;
        _vValueInputPassiveChanged = false;
        _rValueInputPassiveChanged = false;
        _gValueInputPassiveChanged = false;
        _bValueInputPassiveChanged = false;
        if (Format == ColorFormat.Hex)
        {
            var rgbValue = ColorValue.ToRgb();
            if (_hexValueInput != null)
            {
                var colorText = ColorToHexConverter.ToHexString(rgbValue, AlphaComponentPosition.Leading, false, true);
                _hexValueInputPassiveChanged = _hexValueInput.Text != colorText;
                _hexValueInput.Text = colorText;
            }
        }
        else if (Format == ColorFormat.Hsva)
        {
            if (_hValueInput != null)
            {
                var inputValue = (int)ColorValue.H;
                _hValueInputPassiveChanged = _hValueInput.Value != inputValue;
                _hValueInput.Value = inputValue;
            }
            if (_sValueInput != null)
            {
                var inputValue = new decimal(ColorValue.S * 100);
                _sValueInputPassiveChanged = _sValueInput.Value != inputValue;
                _sValueInput.Value = inputValue;
            }
            if (_vValueInput != null)
            {
                var inputValue = new decimal(ColorValue.V * 100);
                _vValueInputPassiveChanged = _vValueInput.Value != inputValue;
                _vValueInput.Value = inputValue;
            }
        }
        else if (Format == ColorFormat.Rgba)
        {
            var rgbValue = ColorValue.ToRgb();
            if (_rValueInput != null)
            {
                var inputValue = rgbValue.R;
                _rValueInputPassiveChanged = _rValueInput.Value != inputValue;
                _rValueInput.Value = inputValue;
            }

            if (_gValueInput != null)
            {
                var inputValue = rgbValue.G;
                _gValueInputPassiveChanged = _gValueInput.Value != inputValue;
                _gValueInput.Value = inputValue;
            }

            if (_bValueInput != null)
            {
                var inputValue = rgbValue.B;
                _bValueInputPassiveChanged = _bValueInput.Value != inputValue;
                _bValueInput.Value = inputValue;
            }
        }
        if (_alphaInput != null)
        {
            var alpha = (int)(ColorValue.A * 100);
            _alphaInputPassiveChanged = _alphaInput.Value != alpha;
            _alphaInput.Value = alpha;
        }
    }

    private void SyncInputValue()
    {
        if (Format == ColorFormat.Hex)
        {
            if (_hexValueInput?.Text is { } colorText)
            {
                if (Color.TryParse(colorText.AsSpan().Trim(), out var value))
                {
                    var percentage = _alphaInput?.Value ?? 100m;
                    byte alphaValue = (byte)(percentage * 2.55m);
                    var combineValue = Color.FromArgb(alphaValue, value.R, value.G, value.B);
                    using var scope = BeginIgnoringConfigureValues();
                    SetCurrentValue(ColorValueProperty, combineValue.ToHsv());
                }
            }
        }
        else if (Format == ColorFormat.Hsva)
        {
            var hValue = (double?)_hValueInput?.Value;
            var sValue = (double?)_sValueInput?.Value;
            var vValue = (double?)_vValueInput?.Value;
            var percentage = _alphaInput?.Value ?? 100m;
            var alphaValue = (double)percentage / 100;
            if (hValue != null && sValue != null && vValue != null)
            {
                using var scope = BeginIgnoringConfigureValues();
                var newColorValue = HsvColor.FromAhsv(alphaValue, hValue.Value, sValue.Value / 100, vValue.Value / 100);
                SetCurrentValue(ColorValueProperty, newColorValue);
            }
        }
        else if (Format == ColorFormat.Rgba)
        {
            var rValue = _rValueInput?.Value;
            var gValue = _gValueInput?.Value;
            var bValue = _bValueInput?.Value;
            var percentage = _alphaInput?.Value ?? 100m;
            byte alphaValue = (byte)(percentage * 2.55m);
            if (rValue != null && gValue != null && bValue != null)
            {
                using var scope = BeginIgnoringConfigureValues();
                var newColorValue = Color.FromArgb(alphaValue, (byte)rValue.Value, (byte)gValue.Value, (byte)bValue.Value);
                SetCurrentValue(ColorValueProperty, newColorValue.ToHsv());
            }
        }
    }

    private IgnoreConfigureValues BeginIgnoringConfigureValues() => new IgnoreConfigureValues(this);

    private readonly struct IgnoreConfigureValues : IDisposable
    {
        private readonly ColorPickerInput _owner;

        public IgnoreConfigureValues(ColorPickerInput owner)
        {
            _owner = owner;
            _owner._ignoringConfigureValues = true;
        }

        public void Dispose() => _owner._ignoringConfigureValues = false;
    }
}
