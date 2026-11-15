using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace AtomUI.Controls;

public abstract class AbstractNumericUpDown : Avalonia.Controls.NumericUpDown, IFormItemAware, IInputControlStatusAware
{
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractNumericUpDown>();

    public static readonly StyledProperty<bool> IsStringModeProperty =
        AvaloniaProperty.Register<AbstractNumericUpDown, bool>(nameof(IsStringMode));

    public static readonly StyledProperty<string?> StringValueProperty =
        AvaloniaProperty.Register<AbstractNumericUpDown, string?>(nameof(StringValue));

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    public bool IsStringMode
    {
        get => GetValue(IsStringModeProperty);
        set => SetValue(IsStringModeProperty, value);
    }
    public string? StringValue
    {
        get => GetValue(StringValueProperty);
        set => SetValue(StringValueProperty, value);
    }

    public static readonly StyledProperty<int?> DigitsProperty =
        AvaloniaProperty.Register<AbstractNumericUpDown, int?>(nameof(Digits),
            validate: digits => digits is null or >= 0 and <= 28);

    /// <summary>Gets or sets the number of fractional digits to display and retain on commit.</summary>
    public int? Digits
    {
        get => GetValue(DigitsProperty);
        set => SetValue(DigitsProperty, value);
    }

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractNumericUpDown>();
    
    internal FormValidateStatus FormStatus => GetValue(FormStatusProperty);
    
    private readonly NumericUpDownTextConverter _textConverter;
    private IValueConverter? _userTextConverter;
    private bool _suppressTextConverterTracking;
    private bool _isUpdatingFromText;
    private bool _isUpdatingFromValue;
    private bool _isUpdatingText;
    private bool _isParsingText;
    private EventHandler? _formValueChanged;
    private TextBox? _input;
    private decimal? _lastCommittedValue;
    private FormatException? _parseError;
    private bool _isCommitting;

    protected AbstractNumericUpDown()
    {
        _textConverter = new NumericUpDownTextConverter(this);
    }

    protected virtual bool AllowsEmptyText => true;

    protected virtual bool ReportsTextConversionErrors => true;

    private bool UsesCommitPolicy => Digits.HasValue || !AllowsEmptyText || ReportsTextConversionErrors;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _input = e.NameScope.Find<TextBox>("PART_TextBox");
        if (Digits.HasValue && !IsKeyboardFocusWithin)
        {
            RefreshPrecisionText();
        }
    }

    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        if (UsesCommitPolicy && !IsReadOnly)
        {
            TryCommitText(Text);
            // Native NumericUpDown re-parses text on lost focus even for an invalid edit.
            // Keep the shared policy active so the value and the conversion error survive.
            _isCommitting = true;
            try
            {
                base.OnLostFocus(e);
                RefreshPrecisionText();
            }
            finally
            {
                _isCommitting = false;
            }
            return;
        }

        base.OnLostFocus(e);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Return && UsesCommitPolicy && !IsReadOnly)
        {
            e.Handled = !TryCommitText(Text);
            return;
        }

        base.OnKeyDown(e);
    }

    protected bool TryCommitText(string? text)
    {
        if (!TryReadValue(text, out var value))
        {
            SetTextConversionError(true);
            return false;
        }

        SetTextConversionError(false);
        _isCommitting = true;
        try
        {
            if (value is { } parsed && Digits is { } digits)
            {
                value = Math.Clamp(decimal.Round(parsed, digits, MidpointRounding.AwayFromZero), Minimum, Maximum);
            }

            _lastCommittedValue = value;
            SetCurrentValue(ValueProperty, value);
            if (Digits.HasValue || !AllowsEmptyText)
            {
                RefreshPrecisionText();
            }
            return true;
        }
        finally
        {
            _isCommitting = false;
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == DigitsProperty ||
            (Digits.HasValue && (change.Property == NumberFormatProperty || change.Property == FormatStringProperty)))
        {
            UpdateTextConverter();
            RefreshPrecisionText();
        }

        if (change.Property == TextConverterProperty && !_suppressTextConverterTracking)
        {
            if (change.NewValue is IValueConverter converter && converter != _textConverter)
            {
                _userTextConverter = converter;
            }
            else if (change.NewValue is null)
            {
                _userTextConverter = null;
            }
        }

        if (change.Property == IsStringModeProperty)
        {
            if (IsStringMode)
            {
                UpdateStringValueFromValue(Value, CultureInfo.CurrentCulture);
            }
            UpdateTextConverter();
            RefreshDisplayedText();
        }

        if (change.Property == StringValueProperty && IsStringMode && !_isUpdatingFromText && !_isUpdatingFromValue)
        {
            ApplyStringValue(change.NewValue as string);
        }
    }

    protected override void OnTextChanged(string? oldValue, string? newValue)
    {
        if (_isCommitting)
        {
            return;
        }

        if (UsesCommitPolicy && IsInitialized && !TryReadValue(newValue, out _))
        {
            SetTextConversionError(true);
            if (IsStringMode)
            {
                UpdateStringValueFromText(newValue);
            }
            return;
        }

        if (UsesCommitPolicy)
        {
            SetTextConversionError(false);
            if (!AllowsEmptyText && string.IsNullOrWhiteSpace(newValue))
            {
                return;
            }
        }

        _isParsingText = true;
        try
        {
            base.OnTextChanged(oldValue, newValue);
        }
        finally
        {
            _isParsingText = false;
        }

        if (IsStringMode)
        {
            UpdateStringValueFromText(newValue);
        }
    }

    protected override void OnValueChanged(decimal? oldValue, decimal? newValue)
    {
        if (!_isParsingText && newValue is not null)
        {
            _lastCommittedValue = newValue;
        }
        if (IsStringMode && !_isUpdatingFromText && !_isParsingText)
        {
            UpdateStringValueFromValue(newValue, CultureInfo.CurrentCulture);
        }
        base.OnValueChanged(oldValue, newValue);
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool TryReadValue(string? text, out decimal? value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = AllowsEmptyText ? null : _lastCommittedValue;
            return true;
        }

        // A custom converter retains precedence in both directions.
        var converter = TextConverter == _textConverter ? _userTextConverter : TextConverter;
        if (converter is not null)
        {
            try
            {
                value = converter.Convert(text, typeof(decimal?), null, CultureInfo.CurrentCulture) as decimal?;
            }
            catch (Exception exception) when (exception is FormatException or OverflowException or ArgumentException)
            {
                value = null;
            }
        }
        else
        {
            value = TryParseDecimal(text, CultureInfo.CurrentCulture, out var parsed) ? parsed : null;
        }

        if (value is not { } number)
        {
            return false;
        }
        if (ClipValueToMinMax)
        {
            value = Math.Clamp(number, Minimum, Maximum);
            return true;
        }
        return number >= Minimum && number <= Maximum;
    }

    private void SetTextConversionError(bool hasError)
    {
        if (hasError == (_parseError is not null))
        {
            return;
        }

        var errors = DataValidationErrors.GetErrors(this)?.Where(error => !ReferenceEquals(error, _parseError)).ToList() ?? [];
        _parseError = hasError ? new FormatException() : null;
        if (_parseError is not null)
        {
            errors.Add(_parseError);
        }
        DataValidationErrors.SetErrors(this, errors.Count == 0 ? null : errors);
    }

    private void RefreshPrecisionText()
    {
        var culture = CultureInfo.CurrentCulture;
        var converter = TextConverter == _textConverter ? _userTextConverter : TextConverter;
        var display = converter is not null
            ? converter.ConvertBack(Value, typeof(string), null, culture)?.ToString()
            : Value is { } value ? FormatDisplayValue(value, culture) : string.Empty;
        if (Text != display)
        {
            var isCommitting = _isCommitting;
            _isCommitting = true;
            try
            {
                SetCurrentValue(TextProperty, display);
                if (_input is not null)
                {
                    _input.SetCurrentValue(TextBox.TextProperty, display);
                }
            }
            finally
            {
                _isCommitting = isCommitting;
            }
        }
    }

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as decimal?);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    protected virtual void NotifySetFormValue(decimal? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    protected virtual decimal? NotifyGetFormValue()
    {
        return Value;
    }

    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, null);
    }

    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }

    private void UpdateTextConverter()
    {
        var needsConverter = IsStringMode || Digits.HasValue;
        if (needsConverter)
        {
            if (TextConverter != _textConverter)
            {
                SetTextConverterWithoutTracking(_textConverter);
            }
        }
        else if (TextConverter == _textConverter)
        {
            SetTextConverterWithoutTracking(_userTextConverter);
        }
    }

    private void SetTextConverterWithoutTracking(IValueConverter? converter)
    {
        _suppressTextConverterTracking = true;
        try
        {
            SetCurrentValue(TextConverterProperty, converter);
        }
        finally
        {
            _suppressTextConverterTracking = false;
        }
    }

    private void RefreshDisplayedText()
    {
        if (TextConverter != _textConverter)
        {
            return;
        }

        var displayText = _textConverter.ConvertBack(Value, typeof(string), null, CultureInfo.CurrentCulture)?.ToString();
        if (displayText is null || displayText == Text)
        {
            return;
        }

        SetTextWhileUpdating(displayText);
    }

    private void ApplyStringValue(string? raw)
    {
        if (!IsStringMode)
        {
            return;
        }

        var displayText = FormatDisplayText(raw, CultureInfo.CurrentCulture);
        if (!_isUpdatingText && displayText != Text)
        {
            SetTextWhileUpdating(displayText);
        }

        if (TryParseDecimal(raw, CultureInfo.CurrentCulture, out var value))
        {
            SetValueWhileUpdatingFromString(value);
        }
        else
        {
            SetValueWhileUpdatingFromString(null);
        }
    }

    private void SetTextWhileUpdating(string text)
    {
        _isUpdatingText = true;
        try
        {
            SetCurrentValue(TextProperty, text);
        }
        finally
        {
            _isUpdatingText = false;
        }
    }

    private void SetValueWhileUpdatingFromString(decimal? value)
    {
        _isUpdatingFromValue = true;
        try
        {
            SetCurrentValue(ValueProperty, value);
        }
        finally
        {
            _isUpdatingFromValue = false;
        }
    }

    private string FormatDisplayText(string? raw, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        if (IsStringMode)
        {
            return raw;
        }

        if (TryParseDecimal(raw, culture, out var value))
        {
            return FormatDisplayValue(value, culture);
        }

        return raw;
    }

    private string FormatDisplayValue(decimal value, CultureInfo culture)
    {
        var format = Digits is { } digits ? $"F{digits}" : FormatString;
        var numberFormat = NumberFormat ?? culture.NumberFormat;
        if (!string.IsNullOrEmpty(format))
        {
            if (format.Contains("{0", StringComparison.Ordinal))
            {
                return string.Format(culture, format, value);
            }
            return value.ToString(format, numberFormat);
        }

        return value.ToString(numberFormat);
    }

    private string FormatRawValue(decimal value, CultureInfo culture)
    {
        var numberFormat = NumberFormat ?? culture.NumberFormat;
        return value.ToString("G", numberFormat);
    }

    private bool TryParseDecimal(string? text, CultureInfo culture, out decimal value)
    {
        value = 0m;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }
        var numberFormat = NumberFormat ?? culture.NumberFormat;
        return decimal.TryParse(text, ParsingNumberStyle, numberFormat, out value);
    }


    private void UpdateStringValueFromText(string? raw)
    {
        if (!IsStringMode)
        {
            return;
        }

        SetStringValueWhileUpdatingFromText(string.IsNullOrEmpty(raw) ? null : raw);
    }

    private void UpdateStringValueFromValue(decimal? value, CultureInfo culture)
    {
        if (!IsStringMode)
        {
            return;
        }

        SetStringValueWhileUpdatingFromValue(value.HasValue ? FormatRawValue(value.Value, culture) : null);
    }

    private void SetStringValueWhileUpdatingFromText(string? value)
    {
        _isUpdatingFromText = true;
        try
        {
            SetCurrentValue(StringValueProperty, value);
        }
        finally
        {
            _isUpdatingFromText = false;
        }
    }

    private void SetStringValueWhileUpdatingFromValue(string? value)
    {
        _isUpdatingFromValue = true;
        try
        {
            SetCurrentValue(StringValueProperty, value);
        }
        finally
        {
            _isUpdatingFromValue = false;
        }
    }

    private sealed class NumericUpDownTextConverter : IValueConverter
    {
        private readonly AbstractNumericUpDown _owner;

        public NumericUpDownTextConverter(AbstractNumericUpDown owner)
        {
            _owner = owner;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var text = value as string ?? value?.ToString();
            if (_owner._isCommitting)
            {
                return _owner.Value;
            }
            if (_owner._userTextConverter is { } converter && _owner.Digits.HasValue)
            {
                return converter.Convert(value, targetType, parameter, culture);
            }
            if (_owner.TryParseDecimal(text, culture, out var result))
            {
                return result;
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (_owner.Digits.HasValue && !_owner.IsStringMode)
            {
                if (_owner._userTextConverter is { } converter)
                {
                    return converter.ConvertBack(value, targetType, parameter, culture);
                }
                return value is decimal number ? _owner.FormatDisplayValue(number, culture) : string.Empty;
            }

            string? raw = null;

            if (_owner.IsStringMode)
            {
                raw = _owner.StringValue;
            }

            if (raw is null)
            {
                if (value is decimal decimalValue)
                {
                    raw = _owner.FormatRawValue(decimalValue, culture);
                }
                else
                {
                    raw = value?.ToString();
                }
            }
            if (_owner._input?.IsFocused == true)
            {
                return raw ?? string.Empty;
            }

            return _owner.FormatDisplayText(raw, culture);
        }
    }
}
