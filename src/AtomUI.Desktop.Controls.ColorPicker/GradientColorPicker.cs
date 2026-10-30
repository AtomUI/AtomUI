using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

public partial class GradientColorPicker : AbstractColorPicker
{
    #region 公共属性定义
    public static readonly StyledProperty<LinearGradientBrush?> DefaultValueProperty =
        GradientColorPickerView.DefaultValueProperty.AddOwner<GradientColorPicker>();

    public static readonly StyledProperty<LinearGradientBrush?> ValueProperty =
        GradientColorPickerView.ValueProperty.AddOwner<GradientColorPicker>(
            new(defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true));

    public static readonly AttachedProperty<Action<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls>?> ColorTextFormatterProperty =
        AvaloniaProperty.RegisterAttached<GradientColorPicker, Control, Action<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls>?>("ColorTextFormatter");

    public LinearGradientBrush? DefaultValue
    {
        get => GetValue(DefaultValueProperty);
        set => SetValue(DefaultValueProperty, value);
    }

    public LinearGradientBrush? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static Action<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls>? GetColorTextFormatter(GradientColorPicker colorPicker)
    {
        return colorPicker.GetValue(ColorTextFormatterProperty);
    }

    public static void SetPresetColor(GradientColorPicker colorPicker, Func<LinearGradientBrush?, ColorFormat, Avalonia.Controls.Controls> formatter)
    {
        colorPicker.SetValue(ColorTextFormatterProperty, formatter);
    }
    #endregion

    #region 公共事件定义
    /// <summary>
    /// Keep distributing as long as there are changes
    /// </summary>
    public event EventHandler<GradientColorChangedEventArgs>? GradientValueChanged;

    /// <summary>
    /// Dispatched once when Flyout is closed
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
                // 删除多于的
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
    }

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

    protected override Control CreatePresenter()
    {
        // The runtime-created presenter is outside the owner's template selector chain.
        // Its independent Content subtree cannot receive these bindings from ControlTheme.
        var presenter = new GradientColorPickerView();
        presenter[!GradientColorPickerView.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
        presenter[!GradientColorPickerView.IsClearEnabledProperty] = this[!IsClearEnabledProperty];
        presenter[!GradientColorPickerView.FormatProperty] = this[!FormatProperty];
        presenter[!GradientColorPickerView.IsAlphaEnabledProperty] = this[!IsAlphaEnabledProperty];
        presenter[!GradientColorPickerView.IsFormatEnabledProperty] = this[!IsFormatEnabledProperty];
        presenter[!GradientColorPickerView.IsPaletteGroupEnabledProperty] = this[!IsPaletteGroupEnabledProperty];
        presenter[!GradientColorPickerView.PaletteGroupProperty] = this[!PaletteGroupProperty];

        return presenter;
    }

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
                var textBlock = _textPanel.Children[i] as AvaloniaTextBlock;
                if (textBlock != null)
                {
                    if (i == index)
                    {
                        textBlock.Foreground = InactiveStopTextColor;
                    }
                    else
                    {
                        textBlock.Foreground = Foreground;
                    }
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
    protected override void NotifySetFormValue(object? value)
    {
        SetCurrentValue(ValueProperty, value as LinearGradientBrush);
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
