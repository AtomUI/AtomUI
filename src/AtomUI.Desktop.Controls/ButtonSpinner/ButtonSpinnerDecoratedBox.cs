using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Theme.Styling;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

internal class ButtonSpinnerDecoratedBox : AddOnDecoratedBox
{
    #region 公共属性定义

    public static readonly StyledProperty<object?> SpinnerContentProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, object?>(nameof(SpinnerContent));

    public static readonly StyledProperty<bool> IsShowHandleProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, bool>(nameof(IsShowHandle), true);

    public static readonly StyledProperty<bool> IsHandleFloatableProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, bool>(nameof(IsHandleFloatable), true);

    public static readonly StyledProperty<ButtonSpinnerLocation> ButtonSpinnerLocationProperty =
        ButtonSpinner.ButtonSpinnerLocationProperty.AddOwner<ButtonSpinnerDecoratedBox>();

    public object? SpinnerContent
    {
        get => GetValue(SpinnerContentProperty);
        set => SetValue(SpinnerContentProperty, value);
    }

    public bool IsShowHandle
    {
        get => GetValue(IsShowHandleProperty);
        set => SetValue(IsShowHandleProperty, value);
    }

    public bool IsHandleFloatable
    {
        get => GetValue(IsHandleFloatableProperty);
        set => SetValue(IsHandleFloatableProperty, value);
    }

    public ButtonSpinnerLocation ButtonSpinnerLocation
    {
        get => GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }

    #endregion
    
    #region 内部属性定义

    internal static readonly DirectProperty<ButtonSpinnerDecoratedBox, bool> ShowButtonSpinnerProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerDecoratedBox, bool>(nameof(ShowButtonSpinner),
            o => o.ShowButtonSpinner,
            (o, v) => o.ShowButtonSpinner = v);
    
    internal static readonly DirectProperty<ButtonSpinnerDecoratedBox, Thickness> SpinnerBorderThicknessProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerDecoratedBox, Thickness>(nameof(SpinnerBorderThickness),
            o => o.SpinnerBorderThickness,
            (o, v) => o.SpinnerBorderThickness = v);
    
    public static readonly StyledProperty<double> HandleOpacityProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, double>(nameof(HandleOpacity));

    public static readonly StyledProperty<double> HandleOffsetProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, double>(nameof(HandleOffset));

    public static readonly StyledProperty<double> ContentLeftShiftProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, double>(nameof(ContentLeftShift));

    public static readonly StyledProperty<double> ContentRightShiftProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, double>(nameof(ContentRightShift));

    internal static readonly DirectProperty<ButtonSpinnerDecoratedBox, double> SpinnerHandleWidthProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerDecoratedBox, double>(nameof(SpinnerHandleWidth),
            o => o.SpinnerHandleWidth,
            (o, v) => o.SpinnerHandleWidth = v);
    
    public static readonly StyledProperty<Thickness> ContentPaddingProperty =
        AvaloniaProperty.Register<ButtonSpinnerDecoratedBox, Thickness>(nameof(ContentPadding));
    
    internal static readonly DirectProperty<ButtonSpinnerDecoratedBox, Thickness> EffectiveContentPaddingProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerDecoratedBox, Thickness>(nameof(EffectiveContentPadding),
            o => o.EffectiveContentPadding,
            (o, v) => o.EffectiveContentPadding = v);
    
    internal static readonly DirectProperty<ButtonSpinnerDecoratedBox, bool> IsSpinnerContentHoverProperty =
        AvaloniaProperty.RegisterDirect<ButtonSpinnerDecoratedBox, bool>(nameof(IsSpinnerContentHover),
            o => o.IsSpinnerContentHover,
            (o, v) => o.IsSpinnerContentHover = v);

    private bool _showButtonSpinner;

    internal bool ShowButtonSpinner
    {
        get => _showButtonSpinner;
        set => SetAndRaise(ShowButtonSpinnerProperty, ref _showButtonSpinner, value);
    }
    
    private Thickness _spinnerBorderThickness;

    internal Thickness SpinnerBorderThickness
    {
        get => _spinnerBorderThickness;
        set => SetAndRaise(SpinnerBorderThicknessProperty, ref _spinnerBorderThickness, value);
    }

    internal double HandleOpacity
    {
        get => GetValue(HandleOpacityProperty);
        set => SetValue(HandleOpacityProperty, value);
    }
    
    internal double HandleOffset
    {
        get => GetValue(HandleOffsetProperty);
        set => SetValue(HandleOffsetProperty, value);
    }

    internal double ContentLeftShift
    {
        get => GetValue(ContentLeftShiftProperty);
        set => SetValue(ContentLeftShiftProperty, value);
    }

    internal double ContentRightShift
    {
        get => GetValue(ContentRightShiftProperty);
        set => SetValue(ContentRightShiftProperty, value);
    }

    private double _spinnerHandleWidth;

    internal double SpinnerHandleWidth
    {
        get => _spinnerHandleWidth;
        set => SetAndRaise(SpinnerHandleWidthProperty, ref _spinnerHandleWidth, value);
    }
    
    internal Thickness ContentPadding
    {
        get => GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }
    
    private Thickness _effectiveContentPadding;

    internal Thickness EffectiveContentPadding
    {
        get => _effectiveContentPadding;
        set => SetAndRaise(EffectiveContentPaddingProperty, ref _effectiveContentPadding, value);
    }
    
    private bool _isSpinnerContentHover;

    internal bool IsSpinnerContentHover
    {
        get => _isSpinnerContentHover;
        set => SetAndRaise(IsSpinnerContentHoverProperty, ref _isSpinnerContentHover, value);
    }
    
    #endregion
    
    private IDisposable? _mouseMoveDisposable;
    
    protected void ConfigureEffectiveContentPadding()
    {
        if (IsShowHandle && !IsHandleFloatable)
        {
            var padding = SpinnerHandleWidth * 1.1;
            if (ButtonSpinnerLocation == ButtonSpinnerLocation.Right)
            {
                SetCurrentValue(EffectiveContentPaddingProperty, new Thickness(ContentPadding.Left, ContentPadding.Top, padding,
                    ContentPadding.Bottom));
            }
            else
            {
                SetCurrentValue(EffectiveContentPaddingProperty, new Thickness(padding, ContentPadding.Top, ContentPadding.Right,
                    ContentPadding.Bottom));
            }
        }
        else
        {
            SetCurrentValue(EffectiveContentPaddingProperty, ContentPadding);
        }
        UpdateHandleVisualState();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureMoveProcessor();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _mouseMoveDisposable?.Dispose();
    }

    private void ConfigureMoveProcessor()
    {
        if (IsEffectivelyEnabled && IsShowHandle && IsHandleFloatable)
        {
            _mouseMoveDisposable?.Dispose();
            var inputManager = AvaloniaLocator.Current.GetService<IInputManager>()!;
            _mouseMoveDisposable = inputManager.Process.Subscribe(HandleMouseMove);
        }
    }

    private void HandleMouseMove(RawInputEventArgs args)
    {
        if (!IsEffectivelyEnabled)
        {
            return;
        }

        if (args is RawPointerEventArgs pointerEventArgs)
        {
            var pos = this.TranslatePoint(new Point(0, 0), TopLevel.GetTopLevel(this)!);
            if (!pos.HasValue)
            {
                return;
            }

            var bounds = new Rect(pos.Value, Bounds.Size);
            if (bounds.Contains(pointerEventArgs.Position))
            {
                if (IsShowHandle && IsHandleFloatable)
                {
                    IsSpinnerContentHover = true;
                    UpdateHandleVisualState();
                }
            }
            else
            {
                if (IsShowHandle && IsHandleFloatable)
                {
                    IsSpinnerContentHover = false;
                    UpdateHandleVisualState();
                }
            }
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsShowHandleProperty ||
            change.Property == IsHandleFloatableProperty)
        {
            UpdateHandleVisualState();
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == IsHandleFloatableProperty)
            {
                if (change.OldValue is bool oldValue)
                {
                    if (oldValue)
                    {
                        _mouseMoveDisposable?.Dispose();
                    }
                }

                if (IsHandleFloatable)
                {
                    ConfigureMoveProcessor();
                }
            }
        }

        if (change.Property == IsEffectivelyEnabledProperty)
        {
            if (!IsEffectivelyEnabled)
            {
                _mouseMoveDisposable?.Dispose();
                Transitions = null;
            }
            else
            {
                ConfigureMoveProcessor();
                ConfigureTransitionsForEnabledState();
            }
            UpdateHandleVisualState();
        }

        if (change.Property == IsHandleFloatableProperty ||
            change.Property == IsShowHandleProperty ||
            change.Property == SpinnerHandleWidthProperty ||
            change.Property == ButtonSpinnerLocationProperty ||
            change.Property == ContentPaddingProperty)
        {
            ConfigureEffectiveContentPadding();
        }

        if (change.Property == SpinnerHandleWidthProperty ||
            change.Property == ButtonSpinnerLocationProperty)
        {
            UpdateHandleVisualState();
        }
    }
    
    protected override void NotifyCreateTransitions(Transitions transitions)
    {
        base.NotifyCreateTransitions(transitions);
        transitions.Add(TransitionUtils.CreateTransition<DoubleTransition>(HandleOpacityProperty));
        transitions.Add(TransitionUtils.CreateTransition<DoubleTransition>(HandleOffsetProperty, SharedTokenKey.MotionDurationMid));
        transitions.Add(TransitionUtils.CreateTransition<DoubleTransition>(ContentLeftShiftProperty, SharedTokenKey.MotionDurationMid));
        transitions.Add(TransitionUtils.CreateTransition<DoubleTransition>(ContentRightShiftProperty, SharedTokenKey.MotionDurationMid));
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        ConfigureEffectiveContentPadding();
        UpdateHandleVisualState();
    }

    private double GetHiddenOffset()
    {
        if (SpinnerHandleWidth <= 0)
        {
            return 0.0;
        }

        return ButtonSpinnerLocation == ButtonSpinnerLocation.Right ? SpinnerHandleWidth : -SpinnerHandleWidth;
    }

    private void UpdateHandleVisualState()
    {
        if (!IsEffectivelyEnabled)
        {
            IsSpinnerContentHover = false;
            HandleOpacity = IsShowHandle ? 1.0 : 0.0;
            HandleOffset = 0.0;
            ContentLeftShift = 0.0;
            ContentRightShift = 0.0;
            return;
        }

        var visible = IsShowHandle && (!IsHandleFloatable || IsSpinnerContentHover);
        HandleOpacity = visible ? 1.0 : 0.0;
        HandleOffset = visible ? 0.0 : GetHiddenOffset();
        if (!IsHandleFloatable)
        {
            IsSpinnerContentHover = false;
        }

        if (IsHandleFloatable && IsSpinnerContentHover)
        {
            if (ButtonSpinnerLocation == ButtonSpinnerLocation.Right)
            {
                ContentRightShift = -EffectiveContentPadding.Right * 1.5;
                ContentLeftShift = 0.0;
            }
            else
            {
                ContentLeftShift = EffectiveContentPadding.Left * 1.5;
                ContentRightShift = 0.0;
            }
        }
        else
        {
            ContentLeftShift = 0.0;
            ContentRightShift = 0.0;
        }
    }

    private void ConfigureTransitionsForEnabledState()
    {
        if (IsMotionEnabled)
        {
            if (Transitions == null)
            {
                Transitions =
                [
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BorderBrushProperty),
                    TransitionUtils.CreateTransition<SolidColorBrushTransition>(Border.BackgroundProperty)
                ];
                NotifyCreateTransitions(Transitions);
            }
        }
    }
}
