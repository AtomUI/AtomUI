using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
using Avalonia.Input;

namespace AtomUI.Controls;

/// <summary>
/// Base class of the AtomUI numeric up-down controls. It adds the AtomUI input status, a string mode that keeps
/// the text as entered, a fixed display precision and the form value contract to the Avalonia numeric up-down.
/// </summary>
public abstract class AbstractNumericUpDown : Avalonia.Controls.NumericUpDown, IFormItemAware, IInputControlStatusAware
{
    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractNumericUpDown>();

    /// <summary>
    /// Identifies the <see cref="IsStringMode" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsStringModeProperty =
        AvaloniaProperty.Register<AbstractNumericUpDown, bool>(nameof(IsStringMode));

    /// <summary>
    /// Identifies the <see cref="StringValue" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> StringValueProperty =
        AvaloniaProperty.Register<AbstractNumericUpDown, string?>(nameof(StringValue));

    /// <summary>
    /// <see cref="InputControlStatus.Warning" /> and <see cref="InputControlStatus.Error" /> paint the frame in that
    /// state, and <see cref="InputControlStatus.Default" /> leaves it neutral. Data validation errors and an error
    /// reported by the form take precedence and force the error state; a warning reported by the form forces the
    /// warning state.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    /// <summary>
    /// <see langword="true" /> keeps the text as entered in <see cref="StringValue" />, next to the parsed
    /// <see cref="NumericUpDown.Value" />. The default is <see langword="false" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In string mode the typed text is copied to <see cref="StringValue" />, a <see cref="StringValue" /> assigned
    /// from outside updates the displayed text and the parsed <see cref="NumericUpDown.Value" />, and a
    /// <see cref="NumericUpDown.Value" /> that did not come from typed text is written to <see cref="StringValue" />
    /// in general format. Typed text that cannot be parsed leaves
    /// <see cref="NumericUpDown.Value" /> unchanged and is kept in <see cref="StringValue" />.
    /// </para>
    /// <para>
    /// While string mode or <see cref="Digits" /> is active, the control installs its own converter in
    /// <see cref="NumericUpDown.TextConverter" /> and restores the converter assigned by the application when neither
    /// is active.
    /// </para>
    /// </remarks>
    public bool IsStringMode
    {
        get => GetValue(IsStringModeProperty);
        set => SetValue(IsStringModeProperty, value);
    }
    /// <summary>
    /// The text as it was entered, next to the parsed <see cref="NumericUpDown.Value" />. It is used only while
    /// <see cref="IsStringMode" /> is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Setting it replaces the displayed text and sets <see cref="NumericUpDown.Value" /> to the parsed number, or to
    /// <see langword="null" /> when the text cannot be parsed.
    /// </para>
    /// <para>
    /// It follows the typed text, and it follows <see cref="NumericUpDown.Value" /> when the value does not come from
    /// typed text. It is <see langword="null" /> while the text is empty.
    /// </para>
    /// </remarks>
    public string? StringValue
    {
        get => GetValue(StringValueProperty);
        set => SetValue(StringValueProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="Digits" /> property.
    /// </summary>
    public static readonly StyledProperty<int?> DigitsProperty =
        AvaloniaProperty.Register<AbstractNumericUpDown, int?>(nameof(Digits),
            validate: digits => digits is null or >= 0 and <= 28);

    /// <summary>
    /// The number of fractional digits kept when text is committed: the value is rounded half away from zero to this
    /// many digits and clamped into the range of <see cref="NumericUpDown.Minimum" /> and <see
    /// cref="NumericUpDown.Maximum" />. Outside string mode, the text also shows this many fractional digits in place
    /// of <see cref="NumericUpDown.FormatString" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Valid values are 0 to 28. <see langword="null" />, the default, rounds nothing and leaves the text to
    /// <see cref="NumericUpDown.FormatString" /> and <see cref="NumericUpDown.NumberFormat" />.
    /// </para>
    /// <para>
    /// While a value is set, the control installs its own converter in <see cref="NumericUpDown.TextConverter" />. A
    /// converter assigned by the application is still used to parse the text and to format the value, and is restored
    /// when <see cref="Digits" /> is cleared and <see cref="IsStringMode" /> is off.
    /// </para>
    /// </remarks>
    public int? Digits
    {
        get => GetValue(DigitsProperty);
        set => SetValue(DigitsProperty, value);
    }

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<AbstractNumericUpDown>();
    
    /// <summary>
    /// Validation status that the form system last reported for the control; <see cref="NotifyValidateStatus" />
    /// stores it when the status differs from the stored value. The theme passes it to the decorated input frames
    /// of the template, so that their outline follows the form.
    /// </summary>
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

    /// <summary>
    /// Whether an empty or whitespace text counts as valid input. With <see langword="true" />, the default, committing
    /// an empty text sets <see cref="NumericUpDown.Value" /> to <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// With <see langword="false" />, an empty text is never passed to the base class, and committing it restores the
    /// last value that was committed or assigned directly.
    /// </para>
    /// <para>
    /// It is read each time text is parsed or committed, so an override may return a value that changes at runtime.
    /// </para>
    /// </remarks>
    protected virtual bool AllowsEmptyText => true;

    /// <summary>
    /// Whether the text commit policy is active even when <see cref="Digits" /> is not set and empty text is allowed.
    /// The default is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While the policy is active, text that cannot be parsed is kept as typed, <see cref="NumericUpDown.Value" /> is
    /// not updated and a data validation error is reported. The Enter key and the loss of focus commit the text through
    /// <see cref="TryCommitText" />.
    /// </para>
    /// <para>
    /// With <see langword="false" />, the policy is active only while <see cref="Digits" /> is set or
    /// <see cref="AllowsEmptyText" /> is <see langword="false" />. Otherwise the base class parses the text: input that
    /// fails is not committed and raises no error.
    /// </para>
    /// </remarks>
    protected virtual bool ReportsTextConversionErrors => true;

    private bool UsesCommitPolicy => Digits.HasValue || !AllowsEmptyText || ReportsTextConversionErrors;

    /// <summary>
    /// Resolves the inner text box of the template and, when <see cref="Digits" /> is set and the control has no
    /// keyboard focus, rewrites the displayed text with the fixed precision. Call the base member first.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _input = e.NameScope.Find<TextBox>("PART_TextBox");
        if (Digits.HasValue && !IsKeyboardFocusWithin)
        {
            RefreshPrecisionText();
        }
    }

    /// <summary>
    /// Under the text commit policy, commits the text before the base member runs and keeps the commit guard active
    /// while the base member re-parses the text. An input that fails to parse therefore keeps the last committed value
    /// and its conversion error.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Without the policy, or when the control is read-only, only the base member runs.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Under the text commit policy, the Return key commits the text, and the key is handled when the commit fails so
    /// that it is not passed on. Every other key, and every key when the control is read-only, goes to the base
    /// member.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!e.Handled && e.Key == Key.Return && UsesCommitPolicy && !IsReadOnly)
        {
            e.Handled = !TryCommitText(Text);
            return;
        }

        base.OnKeyDown(e);
    }

    /// <summary>
    /// Parses the text and commits it as the <see cref="NumericUpDown.Value" />. When <see cref="Digits" /> is set, the
    /// value is rounded half away from zero to that many digits and clamped into the range of <see
    /// cref="NumericUpDown.Minimum" /> and <see cref="NumericUpDown.Maximum" />.
    /// </summary>
    /// <param name="text">
    /// The text to commit. An empty text commits <see langword="null" />, or the last committed value when
    /// <see cref="AllowsEmptyText" /> is <see langword="false" />.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the text was committed. <see langword="false" /> when the text cannot be parsed, or
    /// lies outside the range of <see cref="NumericUpDown.Minimum" /> and <see cref="NumericUpDown.Maximum" /> while
    /// <see cref="NumericUpDown.ClipValueToMinMax" /> is <see langword="false" />; the value and the text are left unchanged and a
    /// data validation error is reported.
    /// </returns>
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

    /// <summary>
    /// Reacts to related property changes. The displayed text is rebuilt when <see cref="Digits" /> changes, or when
    /// <see cref="NumericUpDown.NumberFormat" /> or <see cref="NumericUpDown.FormatString" /> changes while <see
    /// cref="Digits" /> is set. Turning <see cref="IsStringMode" /> on seeds <see cref="StringValue" /> from the value
    /// and swaps the text converter, and a <see cref="StringValue" /> assigned from outside is applied to the text and
    /// the value. The base member runs first.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A <see cref="NumericUpDown.TextConverter" /> assigned by the application is remembered as such; the converter installed by the
    /// control is not.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Under the text commit policy, text that cannot be parsed is kept as typed, does not reach <see
    /// cref="NumericUpDown.Value" /> and is reported as a data validation error. Text that parses clears the error.
    /// Text that reaches the base member is parsed into <see cref="NumericUpDown.Value" />.
    /// </summary>
    /// <param name="oldValue">
    /// The previous text.
    /// </param>
    /// <param name="newValue">
    /// The new text.
    /// </param>
    /// <remarks>
    /// <para>
    /// An empty text is not passed on while <see cref="AllowsEmptyText" /> is <see langword="false" />, text changes
    /// made by a commit in progress are ignored, and in string mode <see cref="StringValue" /> follows the text.
    /// </para>
    /// </remarks>
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

    /// <summary>
    /// Stores the new value as the last committed one when it did not come from parsing typed text, and in string
    /// mode writes a value that did not come from typed text to <see cref="StringValue" />. It then calls the base
    /// member and raises <see cref="IFormItemAware.ValueChanged" /> once the value is committed.
    /// </summary>
    /// <param name="oldValue">
    /// The previous value.
    /// </param>
    /// <param name="newValue">
    /// The new value.
    /// </param>
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

        // A converter assigned by the application takes precedence over the built-in parsing.
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

    /// <summary>
    /// Applies a value assigned by the form system. The default sets <see cref="NumericUpDown.Value" /> with
    /// <c>SetCurrentValue</c>, so a binding on <see cref="NumericUpDown.Value" /> is kept. The form system passes the
    /// argument as <c>object</c>: only a boxed <c>decimal</c> is assigned, any other argument clears the value.
    /// </summary>
    /// <param name="value">
    /// The value to assign, or <see langword="null" /> to clear.
    /// </param>
    protected virtual void NotifySetFormValue(decimal? value)
    {
        SetCurrentValue(ValueProperty, value);
    }

    /// <summary>
    /// Returns the value reported to the form system.
    /// </summary>
    /// <returns>
    /// The current <see cref="NumericUpDown.Value" />.
    /// </returns>
    protected virtual decimal? NotifyGetFormValue()
    {
        return Value;
    }

    /// <summary>
    /// Resets the value on behalf of the form system. The default sets <see cref="NumericUpDown.Value" /> to <see
    /// langword="null" /> with <c>SetCurrentValue</c>, so a binding on <see cref="NumericUpDown.Value" /> is kept.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(ValueProperty, null);
    }

    /// <summary>
    /// Called when the form system reports a new validation status. The default stores a status that differs in
    /// <c>FormStatus</c>, which lets the frame follow the form. An override must call the base method to keep that
    /// behavior.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form system.
    /// </param>
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

    // The converter installed in TextConverter while string mode or Digits is active. During a commit it returns the
    // current value, so that the base class re-parsing the text leaves the committed value and its error alone.
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
