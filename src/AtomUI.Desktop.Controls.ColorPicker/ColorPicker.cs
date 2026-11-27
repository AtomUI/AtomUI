using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Color picker that edits a single color. The popup hosts a <see cref="ColorPickerView" />, and the picker
/// itself shows the color in its color indicator and, when <see cref="AbstractColorPicker.IsTextVisible" />
/// is <see langword="true" />, as formatted text. While <see cref="AbstractColorPicker.IsNeedConfirm" /> is
/// <see langword="true" /> (confirm mode below), the popup edits a draft, and only
/// <see cref="CommitConfirmationDraft" /> writes that draft to <see cref="Value" />.
/// </summary>
public partial class ColorPicker : AbstractColorPicker
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="DefaultValue" /> property.
    /// </summary>
    public static readonly StyledProperty<Color?> DefaultValueProperty =
        AvaloniaProperty.Register<ColorPicker, Color?>(nameof(DefaultValue));

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<Color?> ValueProperty =
        AvaloniaProperty.Register<ColorPicker, Color?>(
            nameof(Value),
            defaultBindingMode: BindingMode.TwoWay,
            enableDataValidation: true);

    /// <summary>
    /// Identifies the ColorTextFormatter attached property, which holds a formatter that replaces the built-in
    /// color text. Any control can carry the value, it is not inherited, and the picker reads it only from itself.
    /// <see langword="null" /> selects the text built from the picker's color format; a formatter receives the
    /// selected color and the format and returns the text.
    /// </summary>
    public static readonly AttachedProperty<Func<Color, ColorFormat, string>?> ColorTextFormatterProperty =
        AvaloniaProperty.RegisterAttached<ColorPicker, Control, Func<Color, ColorFormat, string>?>("ColorTextFormatter");

    /// <summary>
    /// Identifies the <see cref="ValueSyncStrategy" /> property.
    /// </summary>
    public static readonly StyledProperty<ColorPickerValueSyncMode> ValueSyncStrategyProperty =
        AvaloniaProperty.Register<ColorPicker, ColorPickerValueSyncMode>(nameof(ValueSyncStrategy), ColorPickerValueSyncMode.Immediate);

    /// <summary>
    /// Color that stands in for <see cref="Value" /> while that is <see langword="null" />. It becomes
    /// <see cref="Value" /> when the control is loaded, so a <see cref="Value" /> that is already set is not
    /// replaced. Outside confirm mode the popup starts from it when <see cref="Value" /> is
    /// <see langword="null" />, and from white when both are <see langword="null" />.
    /// </summary>
    public Color? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// The selected color, or <see langword="null" /> for no color, which shows the empty-color indicator and the
    /// empty text. Binds two-way by default. Outside confirm mode the popup writes to it as
    /// <see cref="ValueSyncStrategy" /> decides, and its clear button empties it at once; in confirm mode only a
    /// committed draft is written.
    /// </summary>
    public Color? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Returns the formatter that replaces the built-in color text, or <see langword="null" /> when the built-in
    /// text is used.
    /// </summary>
    /// <param name="colorPicker">
    /// The picker that carries the value.
    /// </param>
    public static Func<Color, ColorFormat, string>? GetColorTextFormatter(ColorPicker colorPicker)
    {
        return colorPicker.GetValue(ColorTextFormatterProperty);
    }

    /// <summary>
    /// Sets the formatter that replaces the built-in color text. The text is regenerated at once through
    /// <see cref="GenerateValueText" />, which does nothing while the text is hidden.
    /// </summary>
    /// <param name="colorPicker">
    /// The picker that carries the value.
    /// </param>
    /// <param name="formatter">
    /// Returns the text for the selected color in the picker's color format.
    /// </param>
    public static void SetColorTextFormatter(ColorPicker colorPicker, Func<Color, ColorFormat, string> formatter)
    {
        colorPicker.SetValue(ColorTextFormatterProperty, formatter);
    }

    /// <summary>
    /// Decides when a change made in the popup becomes <see cref="Value" /> outside confirm mode: at once with
    /// <see cref="ColorPickerValueSyncMode.Immediate" />, the default, or once when the popup closes with
    /// <see cref="ColorPickerValueSyncMode.OnCompleted" />. In confirm mode the committed draft decides instead.
    /// The popup's clear button empties <see cref="Value" /> at once, whatever the strategy.
    /// </summary>
    public ColorPickerValueSyncMode ValueSyncStrategy
    {
        get => GetValue(ValueSyncStrategyProperty);
        set => SetValue(ValueSyncStrategyProperty, value);
    }
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after <see cref="Value" /> changes, whatever the source: the popup, a binding, a form item or the
    /// clear action. The arguments carry the previous and the new color.
    /// </summary>
    public event EventHandler<ColorChangedEventArgs>? ValueChanged;
    /// <summary>
    /// Raised with the chosen color once it is committed: when the popup closes outside confirm mode and a
    /// confirmation operation, and when a confirmation draft is committed. It is not raised while
    /// <see cref="Value" /> is <see langword="null" />.
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

    /// <summary>
    /// Applies the fallbacks once the control is loaded: an empty <see cref="Value" /> takes
    /// <see cref="DefaultValue" /> when that is set, and a <see cref="Value" /> that is still
    /// <see langword="null" /> is cleared through <see cref="ClearColor" />, which also resets the indicator.
    /// </summary>
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

    /// <summary>
    /// Reacts to changes of the picker's properties. A changed <see cref="Value" /> resets an open confirmation
    /// draft, regenerates the text and the color indicator, raises <see cref="ValueChanged" /> and notifies the
    /// owning form. A changed <see cref="ValueSyncStrategy" /> resets an open confirmation draft while the
    /// popup is open in confirm mode, and a changed formatter regenerates the text.
    /// </summary>
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

    /// <summary>
    /// Builds the text shown for <see cref="Value" /> from the custom formatter or, in the picker's color format,
    /// from the built-in formatting, or shows the empty text when <see cref="Value" /> is
    /// <see langword="null" />. Does nothing while the text is hidden.
    /// </summary>
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

    /// <summary>
    /// Updates the color indicator for <see cref="Value" />: the color becomes its background, or, when
    /// <see cref="Value" /> is <see langword="null" />, the background is transparent and the indicator switches
    /// to empty-color mode.
    /// </summary>
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

    /// <summary>
    /// Looks up the template part <c>PART_ColorIndicator</c>, the color indicator that <see cref="Value" /> drives.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _colorIndicator = e.NameScope.Find<ColorBlock>("PART_ColorIndicator");
    }

    /// <summary>
    /// Creates the <see cref="ColorPickerView" /> that edits the color in the popup and forwards the
    /// host's motion switch and its clear, format, alpha and palette settings to it through bindings.
    /// The base class calls it to obtain the popup's presenter.
    /// </summary>
    /// <returns>
    /// The view shown in the popup.
    /// </returns>
    protected override Control CreatePresenter()
    {
        // The runtime-created presenter is outside the owner's template selector chain.
        // Its independent Content subtree cannot receive these bindings from ControlTheme.
        // Forward motion only as a host input; the View theme owns its default and the host-false gate.
        var presenter = new ColorPickerView();
        presenter[!AbstractColorPickerView.HostIsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        presenter[!ColorPickerView.IsClearEnabledProperty] = this[!IsClearEnabledProperty];
        presenter[!ColorPickerView.FormatProperty] = this[!FormatProperty];
        presenter[!ColorPickerView.IsAlphaEnabledProperty] = this[!IsAlphaEnabledProperty];
        presenter[!ColorPickerView.IsFormatEnabledProperty] = this[!IsFormatEnabledProperty];
        presenter[!ColorPickerView.IsPaletteGroupEnabledProperty] = this[!IsPaletteGroupEnabledProperty];
        presenter[!ColorPickerView.PaletteGroupProperty] = this[!PaletteGroupProperty];

        return presenter;
    }

    /// <summary>
    /// Stores the presenter created by <see cref="CreatePresenter" /> as the view whose value and clear events drive
    /// the popup. Nothing is stored when the presenter is not a <see cref="ColorPickerView" />.
    /// </summary>
    /// <param name="pickerPresenter">
    /// The presenter the base class created for the popup.
    /// </param>
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

    /// <summary>
    /// Prepares the popup view when the picker opens, after the base method. In confirm mode the view's draft is
    /// reset to <see cref="Value" />; outside confirm mode the view shows <see cref="Value" />, falling back to
    /// <see cref="DefaultValue" /> and then to white, and the value to sync on close starts from that color. The
    /// view's value and clear events are subscribed.
    /// </summary>
    protected override void NotifyPickerOpened()
    {
        base.NotifyPickerOpened();
        if (_presenter != null)
        {
            if (IsNeedConfirm)
            {
                _presenter.ResetDraft(Value);
            }
#pragma warning disable ATOMUIGEN044 // Color-model data endpoints/default values, independent of theme chrome.
            var effectiveColor = Value ?? DefaultValue ?? Colors.White;
#pragma warning restore ATOMUIGEN044
            if (!IsNeedConfirm)
            {
                _presenter.SetCurrentValue(ColorPickerView.ValueProperty, effectiveColor);
            }
            _latestSyncValue = effectiveColor;
            _presenter.ValueChanged += HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared += HandleColorCleared;
        }
    }

    /// <summary>
    /// Ends the view's edit session and unsubscribes the view's value and clear events before the base method runs.
    /// Outside confirm mode and a confirmation operation, the change collected in the popup is written to
    /// <see cref="Value" /> when <see cref="ValueSyncStrategy" /> is
    /// <see cref="ColorPickerValueSyncMode.OnCompleted" />, and <see cref="ValueSelected" /> is raised when
    /// <see cref="Value" /> is set.
    /// </summary>
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
    /// <summary>
    /// Sets <see cref="Value" /> from the value the owning form item provides. A value that is not a
    /// <see cref="Color" />, including <see langword="null" />, empties <see cref="Value" />.
    /// </summary>
    /// <param name="value">
    /// The color supplied by the form item.
    /// </param>
    protected override void NotifySetFormValue(object? value)
    {
        SetCurrentValue(ValueProperty, value as Color?);
    }

    /// <summary>
    /// Returns <see cref="Value" /> as the value reported to the owning form item.
    /// </summary>
    /// <returns>
    /// The selected color, or <see langword="null" /> when none is selected.
    /// </returns>
    protected override object? NotifyGetFormValue()
    {
        return Value;
    }

    /// <summary>
    /// Empties the color for the owning form item through <see cref="ClearColor" />, which also switches the color
    /// indicator to empty-color mode and shows the empty text.
    /// </summary>
    protected override void NotifyClearFormValue()
    {
        ClearColor();
    }
    #endregion
}
