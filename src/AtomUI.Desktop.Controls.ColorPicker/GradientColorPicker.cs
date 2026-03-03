using System.Reactive.Disposables;
using AtomUI.Desktop.Controls.Themes;
using AtomUI.Data;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

public class GradientColorPicker : AbstractColorPicker
{
    #region 公共属性定义
    public static readonly StyledProperty<LinearGradientBrush?> DefaultValueProperty =
        GradientColorPickerView.DefaultValueProperty.AddOwner<GradientColorPicker>();
    
    public static readonly StyledProperty<LinearGradientBrush?> ValueProperty =
        GradientColorPickerView.ValueProperty.AddOwner<GradientColorPicker>();
    
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
        private set => SetValue(ValueProperty, value);
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
    private CompositeDisposable? _flyoutBindingDisposables;
    private WrapPanel? _textPanel;
    private IDisposable? _activeStopIndexChangedDisposable;
    private int? _latestActivatedStopIndex;
    private IDisposable? _emptyTextBindingDisposable;
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
            NotifyValueChanged(new GradientColorChangedEventArgs(change.GetOldValue<LinearGradientBrush?>(),
                change.GetNewValue<LinearGradientBrush?>()));
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
                if (Value != null)
                {
                    Value.StartPoint = new RelativePoint(0, 0.5, RelativeUnit.Relative);
                    Value.EndPoint   = new RelativePoint(1, 0.5, RelativeUnit.Relative);
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
        if (_textPanel == null || !IsShowText)
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
            var stops        = Value.GradientStops;
            var stopsCount   = stops.Count;
            if (stopsCount < textRunCount)
            {
                // 删除多于的
                var delta = textRunCount - stopsCount;
                for (var i = 0; i < delta; i++)
                {
                    _textPanel.Children.Remove(_textPanel.Children.Last());
                }
            } 
            else if (stopsCount > textRunCount)
            {
                var delta   = stopsCount - textRunCount;
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
            _emptyTextBindingDisposable?.Dispose();
            _emptyTextBindingDisposable = BindUtils.RelayBind(this, EmptyColorTextProperty, emptyTextBlock, AvaloniaTextBlock.TextProperty);
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
                    var stop      = Value.GradientStops[i];
                    var textBlock       = _textPanel.Children[i] as AvaloniaTextBlock;
                    var colorText = FormatColor(stop.Color, Format);
                    var percent   = $"{stop.Offset * 100:0}%";
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
        if (_colorIndicator != null)
        {
            _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, false);
        }
        if (Value == null)
        {
            SetCurrentValue(ColorBlockBackgroundProperty, new SolidColorBrush(Colors.Transparent));
        }
        else
        {
            SetCurrentValue(ColorBlockBackgroundProperty, Value);
        }
    }

    protected override Flyout CreatePickerFlyout()
    {
        var flyout = new GradientColorPickerFlyout();
        flyout.IsDetectMouseClickEnabled = false;
        _flyoutBindingDisposables?.Dispose();
        _flyoutBindingDisposables = new CompositeDisposable(6);
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsMotionEnabledProperty, flyout, GradientColorPickerFlyout.IsMotionEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsClearEnabledProperty, flyout, GradientColorPickerFlyout.IsClearEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, FormatProperty, flyout, GradientColorPickerFlyout.FormatProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsAlphaEnabledProperty, flyout, GradientColorPickerFlyout.IsAlphaEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsFormatEnabledProperty, flyout, GradientColorPickerFlyout.IsFormatEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, IsPaletteGroupEnabledProperty, flyout, GradientColorPickerFlyout.IsPaletteGroupEnabledProperty));
        _flyoutBindingDisposables.Add(BindUtils.RelayBind(this, PaletteGroupProperty, flyout, GradientColorPickerFlyout.PaletteGroupProperty));
        
        return flyout;
    }
    
    protected override void NotifyFlyoutPresenterCreated(Control control)
    {
        if (control is FlyoutPresenter flyoutPresenter && flyoutPresenter.Content is GradientColorPickerView presenter)
        {
            _presenter                      =  presenter;
        }
    }

    private void HandleColorPickerViewValueChanged(object? sender, GradientColorChangedEventArgs args)
    {
        if (IsFlyoutOpen)
        {
            SetCurrentValue(ValueProperty, args.NewColor);
        }
    }

    protected override void NotifyFlyoutOpened()
    {
        if (_presenter != null)
        {
            _presenter.GradientValueChanged += HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared    += HandleColorCleared;
            var effectiveColor = Value ?? DefaultValue;
            _activeStopIndexChangedDisposable = BindUtils.RelayBind(_presenter, GradientColorPickerView.ActivatedStopIndexProperty, this, ActivatedStopIndexProperty);
            if (effectiveColor != null)
            {
                _presenter.SetCurrentValue(GradientColorPickerView.ValueProperty, effectiveColor);
                _presenter.SetCurrentValue(GradientColorPickerView.ActivatedStopIndexProperty, _latestActivatedStopIndex ?? 0);
            }
        }
    }

    protected override void NotifyFlyoutClosed()
    {
        if (_presenter != null)
        {
            if (Value != null)
            {
                ValueSelected?.Invoke(this, new GradientColorSelectedEventArgs(Value));
            }
            _latestActivatedStopIndex       =  ActivatedStopIndex;
            _presenter.GradientValueChanged -= HandleColorPickerViewValueChanged;
            _presenter.ColorValueCleared    -= HandleColorCleared;
            _activeStopIndexChangedDisposable?.Dispose();
            _activeStopIndexChangedDisposable = null;
            SetCurrentValue(ActivatedStopIndexProperty, null);
        }
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
    
    internal void NotifyValueChanged(GradientColorChangedEventArgs e)
    {
        GradientValueChanged?.Invoke(this, e);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Value           ??= DefaultValue;
        _textPanel      =   e.NameScope.Find<WrapPanel>(ColorPickerThemeConstants.ColorTextPanelPart);
        _colorIndicator =   e.NameScope.Find<ColorBlock>(ColorPickerThemeConstants.ColorIndicatorPart);
        GenerateValueText();
        GenerateColorBlockBackground();
    }
    
    private void HandleColorCleared(object? sender, EventArgs args)
    {
        ClearColor();
    }
    
    private void ClearColor()
    {
        if (_colorIndicator != null)
        {
            _colorIndicator.SetCurrentValue(ColorBlock.IsEmptyColorModeProperty, true);
            _textPanel?.Children.Clear();
            _emptyTextBindingDisposable?.Dispose();
            var emptyTextBlock = new AvaloniaTextBlock();
            _emptyTextBindingDisposable = BindUtils.RelayBind(this, EmptyColorTextProperty, emptyTextBlock, AvaloniaTextBlock.TextProperty);
            _textPanel?.Children.Add(emptyTextBlock);
        }
    }
    
    #region 实现 FormItem 接口
    protected override void NotifySetFormValue(object? value)
    {
        Value = value as LinearGradientBrush;
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