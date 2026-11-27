using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

/// <summary>
/// Color picker that edits a linear gradient. The popup hosts a <see cref="GradientColorPickerView" />, and
/// the picker itself shows the gradient in its color indicator and, when
/// <see cref="AbstractColorPicker.IsTextVisible" /> is <see langword="true" />, as one text run per stop.
/// While <see cref="AbstractColorPicker.IsNeedConfirm" /> is <see langword="true" /> (confirm mode below),
/// the popup edits a draft, and only <see cref="CommitConfirmationDraft" /> writes that draft to
/// <see cref="Value" />.
/// </summary>
public partial class GradientColorPicker : AbstractColorPicker
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="DefaultValue" /> property.
    /// </summary>
    public static readonly StyledProperty<LinearGradientBrush?> DefaultValueProperty =
        GradientColorPickerView.DefaultValueProperty.AddOwner<GradientColorPicker>();

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<LinearGradientBrush?> ValueProperty =
        GradientColorPickerView.ValueProperty.AddOwner<GradientColorPicker>(
            new(defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true));

    /// <summary>
    /// Identifies the ColorTextFormatter attached property, which holds a formatter that replaces the built-in
    /// text of the gradient stops. Any control can carry the value, it is not inherited, and the picker reads it
    /// only from itself. <see langword="null" /> selects the built-in text; a formatter receives the gradient, the
    /// picker's color format and the text runs of the stop list, and fills the runs itself.
    /// </summary>
    public static readonly AttachedProperty<Action<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls>?> ColorTextFormatterProperty =
        AvaloniaProperty.RegisterAttached<GradientColorPicker, Control, Action<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls>?>("ColorTextFormatter");

    /// <summary>
    /// Gradient that stands in for <see cref="Value" /> while that is <see langword="null" />. It becomes
    /// <see cref="Value" /> when the template is applied and when this property changes on an attached control;
    /// a <see cref="Value" /> that is already set is not replaced.
    /// </summary>
    public LinearGradientBrush? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    /// <summary>
    /// The selected gradient, or <see langword="null" /> for no color, which shows the empty-color indicator and
    /// the empty text. Binds two-way by default. Outside confirm mode, every change made in the open popup,
    /// including clearing, is written here at once; in confirm mode only the committed draft is written. On an
    /// attached control outside confirm mode, a gradient assigned here has its start and end points set to a
    /// horizontal line from left to right.
    /// </summary>
    public LinearGradientBrush? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    /// Returns the formatter that replaces the built-in text of the gradient stops, or
    /// <see langword="null" /> when the built-in text is used.
    /// </summary>
    /// <param name="colorPicker">
    /// The picker that carries the value.
    /// </param>
    public static Action<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls>? GetColorTextFormatter(GradientColorPicker colorPicker)
    {
        return colorPicker.GetValue(ColorTextFormatterProperty);
    }

    /// <summary>
    /// Sets the formatter that replaces the built-in text of the gradient stops. Despite its name, this method
    /// sets the ColorTextFormatter attached value and no color.
    /// </summary>
    /// <param name="colorPicker">
    /// The picker that carries the value.
    /// </param>
    /// <param name="formatter">
    /// Receives the gradient, the picker's color format and the text runs of the stop list, and fills the runs.
    /// </param>
    public static void SetPresetColor(GradientColorPicker colorPicker, Func<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls> formatter)
    {
        colorPicker.SetValue(ColorTextFormatterProperty, formatter);
    }
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Raised after <see cref="Value" /> changes, whatever the source: the popup, a binding, a form item or the
    /// clear action. The arguments carry the previous and the new gradient.
    /// </summary>
    public event EventHandler<GradientColorChangedEventArgs>? GradientValueChanged;

    /// <summary>
    /// Raised with the chosen gradient once it is committed: when the popup closes outside confirm mode and a
    /// confirmation operation, and when a confirmation draft is committed. It is not raised while
    /// <see cref="Value" /> is <see langword="null" />.
    /// </summary>
    public event EventHandler<GradientColorSelectedEventArgs>? ValueSelected;
    #endregion

    #region 内部事件定义

    internal static readonly StyledProperty<IBrush?> InactiveStopTextColorProperty =
        AvaloniaProperty.Register<GradientColorPicker, IBrush?>(nameof(InactiveStopTextColor));

    internal IBrush? InactiveStopTextColor
    {
        get => GetValue(InactiveStopTextColorProperty);
        set => SetValue(InactiveStopTextColorProperty, value);
    }
    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<GradientColorPicker, int?> ActivatedStopIndexProperty =
        AvaloniaProperty.RegisterDirect<GradientColorPicker, int?>(
            nameof(ActivatedStopIndex),
            o => o.ActivatedStopIndex,
            (o, v) => o.ActivatedStopIndex = v);

    private int? _activatedStopIndex;

    internal int? ActivatedStopIndex
    {
        get => _activatedStopIndex;
        set => SetAndRaise(ActivatedStopIndexProperty, ref _activatedStopIndex, value);
    }

    #endregion

    private GradientColorPickerView? _presenter;
    private WrapPanel? _textPanel;
    private int? _latestActivatedStopIndex;
    private ColorBlock? _colorIndicator;

    static GradientColorPicker()
    {
        AffectsMeasure<GradientColorPicker>(ColorTextFormatterProperty);
    }

    /// <summary>
    /// Reacts to changes of the picker's properties. A changed <see cref="Value" /> resets an open confirmation
    /// draft, raises <see cref="GradientValueChanged" /> and notifies the owning form. A changed formatter
    /// regenerates the stop text. On an attached control, a changed <see cref="DefaultValue" /> fills an empty
    /// <see cref="Value" />, a changed <see cref="Value" /> regenerates the stop text and the color indicator,
    /// and a changed <see cref="ActivatedStopIndex" /> moves the active-stop class of the stop text.
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
            NotifyValueChanged(new GradientColorChangedEventArgs(change.GetOldValue<LinearGradientBrush?>(),
                change.GetNewValue<LinearGradientBrush?>()));
            NotifyFormValueChanged();
        }

        if (change.Property == ColorTextFormatterProperty)
        {
            GenerateValueText();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DefaultValueProperty)
            {
                Value ??= DefaultValue;
            }
            else if (change.Property == ValueProperty)
            {
                if (!IsNeedConfirm && Value != null)
                {
                    Value.StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative);
                    Value.EndPoint = new RelativePoint(1, 0.5, RelativeUnit.Relative);
                }
                GenerateValueText();
                GenerateColorBlockBackground();
            }

            if (change.Property == ActivatedStopIndexProperty)
            {
                HandleGradientActiveStopChanged(ActivatedStopIndex);
            }
        }

    }

    /// <summary>
    /// Rebuilds the stop text of the template's <c>PART_ColorTextPanel</c>: one text run per gradient stop with
    /// its formatted color and its offset as a percentage, or a single run with the empty text when
    /// <see cref="Value" /> is <see langword="null" /> or has no stops. A custom formatter fills the runs itself.
    /// Does nothing while the text is hidden or the template part is missing.
    /// </summary>
    protected override void GenerateValueText()
    {
        if (_textPanel == null || !IsTextVisible)
        {
            return;
        }
        var textRunCount = _textPanel.Children.Count;
        if (Value == null || Value.GradientStops.Count == 0)
        {
            _textPanel.Children.Clear();
        }
        else
        {
            var stops = Value.GradientStops;
            var stopsCount = stops.Count;
            if (stopsCount < textRunCount)
            {
                // Remove the text runs that exceed the number of stops.
                var delta = textRunCount - stopsCount;
                for (var i = 0; i < delta; i++)
                {
                    _textPanel.Children.RemoveAt(_textPanel.Children.Count - 1);
                }
            }
            else if (stopsCount > textRunCount)
            {
                var delta = stopsCount - textRunCount;
                for (var i = 0; i < delta; i++)
                {
                    var textBlock = new AvaloniaTextBlock();
                    _textPanel.Children.Add(textBlock);
                }
            }
        }
        textRunCount = _textPanel.Children.Count;
        if (Value == null || Value.GradientStops.Count == 0)
        {
            var emptyTextBlock = new AvaloniaTextBlock();
            emptyTextBlock[!AvaloniaTextBlock.TextProperty] = this[!EmptyColorTextProperty];
            _textPanel.Children.Add(emptyTextBlock);
        }
        else
        {
            var customFormatter = GetColorTextFormatter(this);
            if (customFormatter != null)
            {
                customFormatter(Value, Format, _textPanel.Children);
            }
            else
            {
                for (var i = 0; i < textRunCount; i++)
                {
                    var stop = Value.GradientStops[i];
                    var textBlock = _textPanel.Children[i] as AvaloniaTextBlock;
                    var colorText = FormatColor(stop.Color, Format);
                    var percent = $"{stop.Offset * 100:0}%";
                    if (textBlock != null)
                    {
                        if (i < textRunCount - 1)
                        {
                            textBlock.Text = $"{colorText} {percent}, ";
                        }
                        else
                        {

                            textBlock.Text = $"{colorText} {percent}";
                        }
                    }
                }
            }
        }
        HandleGradientActiveStopChanged(ActivatedStopIndex);
    }

    /// <summary>
    /// Updates the color indicator for <see cref="Value" />: the gradient becomes its background, or, when
    /// <see cref="Value" /> is <see langword="null" />, the background is transparent and the indicator switches
    /// to empty-color mode.
    /// </summary>
    protected override void GenerateColorBlockBackground()
    {
        if (Value == null)
        {
            if (_colorIndicator != null)
            {
                _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, true);
            }
            SetColorBlockBackgroundBrushIfChanged(Brushes.Transparent);
        }
        else
        {
            if (_colorIndicator != null)
            {
                _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, false);
            }
            SetColorBlockBackgroundBrushIfChanged(Value);
        }
    }

    /// <summary>
    /// Creates the <see cref="GradientColorPickerView" /> that edits the gradient in the popup and forwards the
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
        var presenter = new GradientColorPickerView();
        presenter[!AbstractColorPickerView.HostIsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        presenter[!GradientColorPickerView.IsClearEnabledProperty] = this[!IsClearEnabledProperty];
        presenter[!GradientColorPickerView.FormatProperty] = this[!FormatProperty];
        presenter[!GradientColorPickerView.IsAlphaEnabledProperty] = this[!IsAlphaEnabledProperty];
        presenter[!GradientColorPickerView.IsFormatEnabledProperty] = this[!IsFormatEnabledProperty];
        presenter[!GradientColorPickerView.IsPaletteGroupEnabledProperty] = this[!IsPaletteGroupEnabledProperty];
        presenter[!GradientColorPickerView.PaletteGroupProperty] = this[!PaletteGroupProperty];

        return presenter;
    }

    /// <summary>
    /// Stores the presenter created by <see cref="CreatePresenter" /> as the view whose value and clear events drive
    /// the popup. Nothing is stored when the presenter is not a <see cref="GradientColorPickerView" />.
    /// </summary>
    /// <param name="pickerPresenter">
    /// The presenter the base class created for the popup.
    /// </param>
    protected override void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        _presenter = pickerPresenter as GradientColorPickerView;
    }

    private void HandleColorPickerViewValueChanged(object? sender, GradientColorChangedEventArgs args)
    {
        if (IsPickerOpen && !IsNeedConfirm)
        {
            SetCurrentValue(ValueProperty, args.NewColor);
        }
    }

    /// <summary>
    /// Prepares the popup view when the picker opens, after the base method. In confirm mode the view's draft is
    /// reset to <see cref="Value" />; outside confirm mode the view shows <see cref="Value" />, or
    /// <see cref="DefaultValue" /> when that is <see langword="null" />, with the stop that was active at the
    /// previous close. The picker's active stop follows the view's, and the view's value and clear events are
    /// subscribed.
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
            _presenter.GradientValueChanged += HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared += HandleColorCleared;
            var effectiveColor = Value ?? DefaultValue;
            this[!ActivatedStopIndexProperty] = _presenter[!GradientColorPickerView.ActivatedStopIndexProperty];
            if (!IsNeedConfirm && effectiveColor != null)
            {
                _presenter.SetCurrentValue(GradientColorPickerView.ValueProperty, effectiveColor);
                _presenter.SetCurrentValue(GradientColorPickerView.ActivatedStopIndexProperty, _latestActivatedStopIndex ?? 0);
            }
        }
    }

    /// <summary>
    /// Ends the view's edit session and unsubscribes the view's value and clear events before the base method runs.
    /// Outside confirm mode and a confirmation operation it raises <see cref="ValueSelected" /> when
    /// <see cref="Value" /> is set. The active stop is remembered for the next open and then cleared.
    /// </summary>
    protected override void NotifyPickerClosed()
    {
        if (_presenter != null)
        {
            _presenter.EndEdit();
            if (!IsNeedConfirm && !IsConfirmationOperationActive && Value != null)
            {
                ValueSelected?.Invoke(this, new GradientColorSelectedEventArgs(Value));
            }
            _latestActivatedStopIndex = ActivatedStopIndex;
            _presenter.GradientValueChanged -= HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared -= HandleColorCleared;
            SetCurrentValue(ActivatedStopIndexProperty, null);
        }
        base.NotifyPickerClosed();
    }

    private void HandleGradientActiveStopChanged(int? index)
    {
        if (_textPanel != null)
        {
            for (var i = 0; i < _textPanel.Children.Count; i++)
            {
                if (_textPanel.Children[i] is AvaloniaTextBlock textBlock)
                {
                    textBlock.Classes.Set("active-stop", i == index);
                }
            }
        }
    }

    internal override void ResetConfirmationDraft()
    {
        if (_presenter == null)
        {
            return;
        }
        _presenter.GradientValueChanged -= HandleColorPickerViewValueChanged;
        _presenter.ResetDraft(Value);
        if (IsPickerOpen)
        {
            _presenter.GradientValueChanged += HandleColorPickerViewValueChanged;
        }
    }

    internal override void CommitConfirmationDraft()
    {
        var draft = _presenter?.DraftValue;
        if (!GradientColorPickerView.GradientEquals(Value, draft))
        {
            SetCurrentValue(ValueProperty, GradientColorPickerView.CopyGradient(draft));
        }
        if (Value is { } value)
        {
            ValueSelected?.Invoke(this, new GradientColorSelectedEventArgs(value));
        }
    }

    internal void NotifyValueChanged(GradientColorChangedEventArgs e)
    {
        GradientValueChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Looks up the template parts <c>PART_ColorTextPanel</c> and <c>PART_ColorIndicator</c>, fills an empty
    /// <see cref="Value" /> from <see cref="DefaultValue" />, and builds the stop text and the color indicator.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Value ??= DefaultValue;
        _textPanel = e.NameScope.Find<WrapPanel>("PART_ColorTextPanel");
        _colorIndicator = e.NameScope.Find<ColorBlock>("PART_ColorIndicator");
        GenerateValueText();
        GenerateColorBlockBackground();
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

        if (_colorIndicator != null)
        {
            _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, true);
            _textPanel?.Children.Clear();
            var emptyTextBlock = new AvaloniaTextBlock();
            emptyTextBlock[!AvaloniaTextBlock.TextProperty] = this[!EmptyColorTextProperty];
            _textPanel?.Children.Add(emptyTextBlock);
        }
    }

    #region 实现 FormItem 接口
    /// <summary>
    /// Sets <see cref="Value" /> from the value the owning form item provides. A value that is not a
    /// <see cref="LinearGradientBrush" />, including <see langword="null" />, empties <see cref="Value" />.
    /// </summary>
    /// <param name="value">
    /// The gradient supplied by the form item.
    /// </param>
    protected override void NotifySetFormValue(object? value)
    {
        SetCurrentValue(ValueProperty, value as LinearGradientBrush);
    }

    /// <summary>
    /// Returns <see cref="Value" /> as the value reported to the owning form item.
    /// </summary>
    /// <returns>
    /// The selected gradient, or <see langword="null" /> when none is selected.
    /// </returns>
    protected override object? NotifyGetFormValue()
    {
        return Value;
    }

    /// <summary>
    /// Empties the gradient for the owning form item through <see cref="ClearColor" />, which also switches the
    /// color indicator to empty-color mode and shows the empty text.
    /// </summary>
    protected override void NotifyClearFormValue()
    {
        ClearColor();
    }
    #endregion
}
