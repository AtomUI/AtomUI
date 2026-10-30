using System.Globalization;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Icons.AntDesign;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;

namespace AtomUI.Desktop.Controls;

using AvaloniaNumericUpDown = Avalonia.Controls.NumericUpDown;

public enum NumericUpDownMode
{
    Input,
    Spinner
}

public partial class NumericUpDown : AvaloniaNumericUpDown,
                             IMotionAwareControl,
                             ICompactSpaceAware,
                             IFormItemAware,
                             IInputControlStatusAware,
                             IInputControlStyleVariantAware,
                             ICustomizableSizeTypeAware
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

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsAllowClearProperty =
        TextBox.IsAllowClearProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<NumericUpDown>();

    public static readonly StyledProperty<bool> IsStringModeProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(IsStringMode));
    
    public static readonly StyledProperty<bool> IsKeyboardEnabledProperty =
        AvaloniaProperty.Register<NumericUpDown, bool>(nameof(IsKeyboardEnabled), true);

    public static readonly StyledProperty<NumericUpDownMode> ModeProperty =
        AvaloniaProperty.Register<NumericUpDown, NumericUpDownMode>(nameof(Mode), NumericUpDownMode.Input);

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

    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public bool IsAllowClear
    {
        get => GetValue(IsAllowClearProperty);
        set => SetValue(IsAllowClearProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public bool IsStringMode
    {
        get => GetValue(IsStringModeProperty);
        set => SetValue(IsStringModeProperty, value);
    }

    public bool IsKeyboardEnabled
    {
        get => GetValue(IsKeyboardEnabledProperty);
        set => SetValue(IsKeyboardEnabledProperty, value);
    }

    public NumericUpDownMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public string? StringValue
    {
        get => GetValue(StringValueProperty);
        set => SetValue(StringValueProperty, value);
    }

    public bool IsCustomFontSize
    {
        get => GetValue(IsCustomFontSizeProperty);
        set => SetValue(IsCustomFontSizeProperty, value);
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
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<NumericUpDown>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<NumericUpDown>();

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<NumericUpDown>();
    
    internal double SpinnerHandleWidth
    {
        get => GetValue(SpinnerHandleWidthProperty);
        set => SetValue(SpinnerHandleWidthProperty, value);
    }
    
    private bool _isEffectiveShowClearButton;

    internal bool IsEffectiveShowClearButton
    {
        get => _isEffectiveShowClearButton;
        set => SetAndRaise(IsEffectiveShowClearButtonProperty, ref _isEffectiveShowClearButton, value);
    }
    
    internal SpaceItemPosition? CompactSpaceItemPosition
    {
        get => GetValue(CompactSpaceItemPositionProperty);
        set => SetValue(CompactSpaceItemPositionProperty, value);
    }
    
    internal Orientation CompactSpaceOrientation
    {
        get => GetValue(CompactSpaceOrientationProperty);
        set => SetValue(CompactSpaceOrientationProperty, value);
    }
    
    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    internal FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        private set => SetCurrentValue(FormStatusProperty, value);
    }
    
    #endregion
    
    private EventHandler? _formValueChanged;
    private IconButton? _clearButton;
    private TextBox? _textBoxPart;
    private CompositeDisposable? _templatePartBindings;
    private readonly NumericUpDownTextConverter _textConverter;
    private IValueConverter? _userTextConverter;
    private bool _suppressTextConverterTracking;
    private bool _isUpdatingFromText;
    private bool _isUpdatingFromValue;
    private bool _isUpdatingText;
    private bool _isParsingText;
    private ButtonSpinner? _buttonSpinner;

    private IconButton? ClearButtonPart
    {
        get => _clearButton;
        set
        {
            if (ReferenceEquals(_clearButton, value))
            {
                return;
            }

            if (_clearButton is not null)
            {
                _clearButton.Click -= HandleClearButtonClicked;
            }

            _clearButton = value;

            if (_clearButton is not null)
            {
                _clearButton.Click += HandleClearButtonClicked;
            }
        }
    }

    private TextBox? TextBoxPart
    {
        get => _textBoxPart;
        set
        {
            if (ReferenceEquals(_textBoxPart, value))
            {
                return;
            }

            if (_textBoxPart is not null)
            {
                _textBoxPart.KeyDown -= HandleTextBoxKeyDown;
            }

            _textBoxPart = value;

            if (_textBoxPart is not null)
            {
                _textBoxPart.KeyDown += HandleTextBoxKeyDown;
            }
        }
    }

    static NumericUpDown()
    {
        ValueChangedEvent.AddClassHandler<NumericUpDown>((textBox, args) => textBox.HandleValueChanged());
    }
    
    public NumericUpDown()
    {
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
        ClearButtonPart = e.NameScope.Find<IconButton>("PART_ClearButton");
        TextBoxPart     = e.NameScope.Find<TextBox>("PART_TextBox");
        ConfigureEffectiveShowClearButton();
        if (e.NameScope.Find<ButtonSpinner>("PART_Spinner") is { } buttonSpinner)
        {
            _buttonSpinner = buttonSpinner;
            // The frame node lives inside the spinner's own template; apply it now so the
            // border relay below can reach the decorated box instead of silently no-oping.
            buttonSpinner.ApplyTemplate();
        }
        RelayFrameBorderBrush();
        SetupTemplatePartBindings(e);
    }

    protected virtual void NotifyClearButtonClicked()
    {
        Value = null;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BorderBrushProperty)
        {
            RelayFrameBorderBrush();
        }

        if (change.Property == IsReadOnlyProperty ||
            change.Property == TextProperty ||
            change.Property == IsAllowClearProperty)
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
        if (IsStringMode && !_isUpdatingFromText && !_isParsingText)
        {
            UpdateStringValueFromValue(newValue, CultureInfo.CurrentCulture);
        }
        base.OnValueChanged(oldValue, newValue);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (!IsKeyboardEnabled)
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

    #region 实现 CompactSpace 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        var isUsedInCompactSpace = position != null;
        if (IsUsedInCompactSpace != isUsedInCompactSpace)
        {
            IsUsedInCompactSpace = isUsedInCompactSpace;
        }

        if (CompactSpaceItemPosition != position)
        {
            CompactSpaceItemPosition = position;
        }
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        if (CompactSpaceOrientation != orientation)
        {
            CompactSpaceOrientation = orientation;
        }
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        var addOnDecoratedBox = _buttonSpinner?.DecoratedBox;
        if (addOnDecoratedBox == null)
        {
            var topLevel = TopLevel.GetTopLevel(this);
            if (topLevel != null)
            {
                topLevel.UpdateLayout();
                addOnDecoratedBox = _buttonSpinner?.DecoratedBox;
            }
        }

        if (addOnDecoratedBox == null || StyleVariant != InputControlStyleVariant.Outlined)
        {
            return 0.0;
        }

        // 都一样宽
        return addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }

    #endregion

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as decimal?);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    private void HandleValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

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

    #endregion

    /// <summary>
    /// Relays the owner's root border brush onto the input frame as a local value so
    /// application-level customization wins over the frame state machine, mirroring antd
    /// inline styles.root semantics and the shared AbstractTextInput behavior. An unset
    /// owner value restores the state machine.
    /// </summary>
    private void RelayFrameBorderBrush()
    {
        var decoratedBox = _buttonSpinner?.DecoratedBox;
        if (decoratedBox is null)
        {
            return;
        }

        var value = GetValue(BorderBrushProperty);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            decoratedBox.ClearValue(BorderBrushProperty);
        }
        else
        {
            decoratedBox.SetValue(BorderBrushProperty, value, BindingPriority.LocalValue);
        }
    }

    private void HandleClearButtonClicked(object? sender, RoutedEventArgs args)
    {
        NotifyClearButtonClicked();
    }

    private void SetupTemplatePartBindings(TemplateAppliedEventArgs e)
    {
        _templatePartBindings?.Dispose();
        _templatePartBindings = new CompositeDisposable();

        if (e.NameScope.Find<InputClearIconButton>("PART_ClearButton") is { } clearButton)
        {
            // Only the visibility relay stays in code: IsEffectiveShowClearButton is an internal
            // direct property, invisible to compiled template bindings. Icon and IsMotionEnabled
            // are template-bound in the ControlTheme.
            _templatePartBindings.Add(BindUtils.RelayBind(this, IsEffectiveShowClearButtonProperty, clearButton,
                Visual.IsVisibleProperty));
        }

        if (e.NameScope.Find<ContentPresenter>("PART_InnerLeftContentPresenter") is { } innerLeftContent)
        {
            // The presenter lives in a ButtonSpinner.InnerLeftContent property-element subtree, where
            // TemplatedParent never propagates, so template bindings stay dead there; relay in code.
            _templatePartBindings.Add(BindUtils.RelayBind(this, InnerLeftContentProperty, innerLeftContent,
                ContentPresenter.ContentProperty));
            _templatePartBindings.Add(BindUtils.RelayBind(this, InnerLeftContentTemplateProperty,
                innerLeftContent, ContentPresenter.ContentTemplateProperty));
        }

        // InnerRightContent / InnerRightContentTemplate and the text box IsCustomFontSize are
        // template-bound in the ControlTheme (the parts sit in direct template content where
        // TemplatedParent propagates), so no code relay is needed for them.

        if (e.NameScope.Find<NumericUpDownInputPanel>("PART_InputPanel") is { } inputPanel &&
            _buttonSpinner?.DecoratedBox is { } decoratedBox)
        {
            // The animated source is internal state of the spinner's nested decorated-box
            // template, outside this Content subtree's TemplateBinding scope. Keep this
            // binding scoped to the current template and consume the existing animation;
            // the panel must not start a second transition or translate the suffix again.
            _templatePartBindings.Add(inputPanel.Bind(NumericUpDownInputPanel.RightInsetProperty,
                decoratedBox.GetObservable(ButtonSpinnerDecoratedBox.ContentRightShiftProperty)
                            .Select(static shift => Math.Max(0d, -shift))));
        }
    }

    private void HandleTextBoxKeyDown(object? sender, KeyEventArgs e)
    {
        if (!IsKeyboardEnabled)
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

    private void ConfigureEffectiveShowClearButton()
    {
        if (!IsAllowClear)
        {
            SetCurrentValue(IsEffectiveShowClearButtonProperty, false);
            return;
        }
        
        SetCurrentValue(IsEffectiveShowClearButtonProperty, !IsReadOnly && !string.IsNullOrEmpty(Text));
    }

    private void UpdateTextConverter()
    {
        var needsConverter = IsStringMode;
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
            if (_owner.IsTextInputFocused())
            {
                return raw ?? string.Empty;
            }

            return _owner.FormatDisplayText(raw, culture);
        }
    }
    
}
