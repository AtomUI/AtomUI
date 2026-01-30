using System;
using System.Globalization;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaNumericUpDown = Avalonia.Controls.NumericUpDown;

public class NumericUpDown : AvaloniaNumericUpDown, IMotionAwareControl, IControlSharedTokenResourcesHost
{
    #region 公共属性定义
    public static readonly StyledProperty<PathIcon?> ClearIconProperty =
        AvaloniaProperty.Register<NumericUpDown, PathIcon?>(nameof(ClearIcon));

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<NumericUpDown>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<NumericUpDown>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<NumericUpDown>();
    
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<NumericUpDown, IDataTemplate?>(nameof(InnerLeftContentTemplate));
    
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<NumericUpDown, IDataTemplate?>(nameof(InnerRightContentTemplate));

    public static readonly StyledProperty<SizeType> SizeTypeProperty =
        SizeTypeControlProperty.SizeTypeProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<AddOnDecoratedVariant> StyleVariantProperty =
        AddOnDecoratedBox.StyleVariantProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<AddOnDecoratedStatus> StatusProperty =
        AddOnDecoratedBox.StatusProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsEnableClearButtonProperty =
        TextBox.IsEnableClearButtonProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> StringModeProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(StringMode));
    
    public static readonly StyledProperty<bool> KeyboardProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(Keyboard), true);

    public static readonly StyledProperty<bool> MouseWheelProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(MouseWheel), true);
    
    public static readonly StyledProperty<string?> StringValueProperty =
        AvaloniaProperty.Register<NumericUpDown, string?>(nameof(StringValue));
    
    public PathIcon? ClearIcon
    {
        get => GetValue(ClearIconProperty);
        set => SetValue(ClearIconProperty, value);
    }

    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    public object? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    public object? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    public object? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }
    
    public object? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    public SizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public AddOnDecoratedVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public AddOnDecoratedStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public bool IsEnableClearButton
    {
        get => GetValue(IsEnableClearButtonProperty);
        set => SetValue(IsEnableClearButtonProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool StringMode
    {
        get => GetValue(StringModeProperty);
        set => SetValue(StringModeProperty, value);
    }


    public bool Keyboard
    {
        get => GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public bool MouseWheel
    {
        get => GetValue(MouseWheelProperty);
        set => SetValue(MouseWheelProperty, value);
    }

    public string? StringValue
    {
        get => GetValue(StringValueProperty);
        set => SetValue(StringValueProperty, value);
    }
    
    #endregion
    
    #region 内部属性定义

    internal static readonly StyledProperty<double> SpinnerHandleWidthProperty =
        ButtonSpinner.SpinnerHandleWidthProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<bool> IsCustomFontSizeProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(IsCustomFontSize));
    
    internal static readonly DirectProperty<NumericUpDown, bool> IsEffectiveShowClearButtonProperty =
        AvaloniaProperty.RegisterDirect<NumericUpDown, bool>(nameof(IsEffectiveShowClearButton),
            o => o.IsEffectiveShowClearButton,
            (o, v) => o.IsEffectiveShowClearButton = v);
    
    internal double SpinnerHandleWidth
    {
        get => GetValue(SpinnerHandleWidthProperty);
        set => SetValue(SpinnerHandleWidthProperty, value);
    }
    
    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
    }
    
    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }
    
    Control IControlSharedTokenResourcesHost.HostControl => this;
    string IControlSharedTokenResourcesHost.TokenId => NumericUpDownToken.ID;
    
    #endregion
    
    private IconButton? _clearButton;
    private TextBox? _textBoxPart;
    private readonly NumericUpDownTextConverter _textConverter;
    private IValueConverter? _userTextConverter;
    private bool _suppressTextConverterTracking;
    private bool _isUpdatingFromText;
    private bool _isUpdatingFromValue;
    private bool _isUpdatingText;
    private bool _isParsingText;
    
    public NumericUpDown()
    {
        this.RegisterResources();
        _textConverter = new NumericUpDownTextConverter(this);
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (ClearIcon == null)
        {
            SetCurrentValue(ClearIconProperty, new CloseCircleFilled());
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _clearButton      = e.NameScope.Find<IconButton>(NumericUpDownThemeConstants.ClearButtonPart);
        if (_clearButton is not null)
        {
            _clearButton.Click += (sender, args) => { NotifyClearButtonClicked(); };
        }
        SetTextBoxPart(e.NameScope.Find<TextBox>("PART_TextBox"));
        ConfigureEffectiveShowClearButton();
    }
    
    protected virtual void NotifyClearButtonClicked()
    {
        Value = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsEnableClearButtonProperty)
        {
            ConfigureEffectiveShowClearButton();
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

        if (change.Property == StringModeProperty)
        {
            if (change.Property == StringModeProperty && StringMode)
            {
                UpdateStringValueFromValue(Value, CultureInfo.CurrentCulture);
            }
            UpdateTextConverter();
            RefreshDisplayedText();
        }

        if (change.Property == StringValueProperty && StringMode && !_isUpdatingFromText && !_isUpdatingFromValue)
        {
            ApplyStringValue(change.NewValue as string);
        }
    }

    protected override void OnTextChanged(string? oldValue, string? newValue)
    {
        _isParsingText = true;
        base.OnTextChanged(oldValue, newValue);
        _isParsingText = false;

        if (StringMode)
        {
            UpdateStringValueFromText(newValue);
        }
    }

    protected override void OnValueChanged(decimal? oldValue, decimal? newValue)
    {
        base.OnValueChanged(oldValue, newValue);
        if (!StringMode || _isUpdatingFromText || _isParsingText)
        {
            return;
        }
        UpdateStringValueFromValue(newValue, CultureInfo.CurrentCulture);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!Keyboard)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = true;
                    return;
            }
        }

        base.OnKeyDown(e);
    }

    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        if (!MouseWheel)
        {
            e.Handled = true;
            return;
        }

        base.OnPointerWheelChanged(e);
    }

    private void SetTextBoxPart(TextBox? textBox)
    {
        if (_textBoxPart != null)
        {
            _textBoxPart.KeyDown -= HandleTextBoxKeyDown;
            _textBoxPart.PointerWheelChanged -= HandleTextBoxPointerWheelChanged;
        }
        _textBoxPart = textBox;
        if (_textBoxPart != null)
        {
            _textBoxPart.KeyDown += HandleTextBoxKeyDown;
            _textBoxPart.PointerWheelChanged += HandleTextBoxPointerWheelChanged;
        }
    }

    private void HandleTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (!Keyboard)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.PageUp:
                case Key.PageDown:
                    e.Handled = true;
                    break;
            }
        }
    }

    private void HandleTextBoxPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!MouseWheel)
        {
            e.Handled = true;
        }
    }

    private void ConfigureEffectiveShowClearButton()
    {
        if (!IsEnableClearButton)
        {
            SetCurrentValue(IsEffectiveShowClearButtonProperty, false);
            return;
        }
        
        SetCurrentValue(IsEffectiveShowClearButtonProperty, !IsReadOnly && !string.IsNullOrEmpty(Text));
    }

    private void UpdateTextConverter()
    {
        var needsConverter = StringMode;
        if (needsConverter)
        {
            if (TextConverter != _textConverter)
            {
                _suppressTextConverterTracking = true;
                SetCurrentValue(TextConverterProperty, _textConverter);
                _suppressTextConverterTracking = false;
            }
        }
        else if (TextConverter == _textConverter)
        {
            _suppressTextConverterTracking = true;
            SetCurrentValue(TextConverterProperty, _userTextConverter);
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

        _isUpdatingText = true;
        SetCurrentValue(TextProperty, displayText);
        _isUpdatingText = false;
    }

    private void ApplyStringValue(string? raw)
    {
        if (!StringMode)
        {
            return;
        }

        var displayText = FormatDisplayText(raw, CultureInfo.CurrentCulture);
        if (!_isUpdatingText && displayText != Text)
        {
            _isUpdatingText = true;
            SetCurrentValue(TextProperty, displayText);
            _isUpdatingText = false;
        }

        if (TextConverter == _textConverter)
        {
            return;
        }

        if (TryParseDecimal(raw, CultureInfo.CurrentCulture, out var value))
        {
            _isUpdatingFromValue = true;
            SetCurrentValue(ValueProperty, value);
            _isUpdatingFromValue = false;
        }
        else
        {
            _isUpdatingFromValue = true;
            SetCurrentValue(ValueProperty, null);
            _isUpdatingFromValue = false;
        }
    }

    private string FormatDisplayText(string? raw, CultureInfo culture)
    {
        if (string.IsNullOrEmpty(raw))
        {
            return string.Empty;
        }

        if (StringMode)
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
        var format = FormatString;
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
        if (!StringMode)
        {
            return;
        }

        _isUpdatingFromText = true;
        SetCurrentValue(StringValueProperty, string.IsNullOrEmpty(raw) ? null : raw);
        _isUpdatingFromText = false;
    }

    private void UpdateStringValueFromValue(decimal? value, CultureInfo culture)
    {
        if (!StringMode)
        {
            return;
        }

        _isUpdatingFromValue = true;
        SetCurrentValue(StringValueProperty, value.HasValue ? FormatRawValue(value.Value, culture) : null);
        _isUpdatingFromValue = false;
    }

    private bool IsTextInputFocused()
    {
        return _textBoxPart?.IsFocused == true;
    }

    private sealed class NumericUpDownTextConverter : IValueConverter
    {
        private readonly NumericUpDown _owner;

        public NumericUpDownTextConverter(NumericUpDown owner)
        {
            _owner = owner;
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            var text = value as string ?? value?.ToString();
            if (_owner.TryParseDecimal(text, culture, out var result))
            {
                return result;
            }
            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            string? raw = null;

            if (_owner.StringMode)
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
            if (_owner.IsTextInputFocused())
            {
                return raw ?? string.Empty;
            }

            return _owner.FormatDisplayText(raw, culture);
        }
    }
}
