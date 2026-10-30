using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

public enum ButtonSpinnerLocation
{
    Left,
    Right
}

[PseudoClasses(ButtonSpinnerPseudoClass.Left, ButtonSpinnerPseudoClass.Right)]
public partial class ButtonSpinner : Spinner,
                                     IMotionAwareControl,
                                     ICompactSpaceAware,
                                     ICustomizableSizeTypeAware,
                                     IInputControlStatusAware,
                                     IInputControlStyleVariantAware
{
    #region 公共属性定义
    public static readonly StyledProperty<bool> IsSpinEnabledProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof(IsSpinEnabled), true);
    
    public static readonly StyledProperty<bool> IsButtonSpinnerVisibleProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof(IsButtonSpinnerVisible), true);
    
    public static readonly StyledProperty<ButtonSpinnerLocation> ButtonSpinnerLocationProperty =
        AvaloniaProperty.Register<ButtonSpinner, ButtonSpinnerLocation>(nameof(ButtonSpinnerLocation), ButtonSpinnerLocation.Right);

    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<ButtonSpinner>();
    
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<ButtonSpinner>();
    
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        AvaloniaProperty.Register<ButtonSpinner, object?>(nameof(InnerLeftContent));
    
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<ButtonSpinner, IDataTemplate?>(nameof(InnerLeftContentTemplate));

    public static readonly StyledProperty<object?> InnerRightContentProperty =
        AvaloniaProperty.Register<ButtonSpinner, object?>(nameof(InnerRightContent));
    
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<ButtonSpinner, IDataTemplate?>(nameof(InnerRightContentTemplate));

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<ButtonSpinner>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<ButtonSpinner>();
    
    public static readonly StyledProperty<bool> IsButtonSpinnerFloatableProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof (IsButtonSpinnerFloatable), false);
    
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ButtonSpinner>();

    public bool IsSpinEnabled
    {
        get => GetValue(IsSpinEnabledProperty);
        set => SetValue(IsSpinEnabledProperty, value);
    }

    public bool IsButtonSpinnerVisible
    {
        get => GetValue(IsButtonSpinnerVisibleProperty);
        set => SetValue(IsButtonSpinnerVisibleProperty, value);
    }

    public ButtonSpinnerLocation ButtonSpinnerLocation
    {
        get => GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }
    
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    public IDataTemplate? LeftAddOnTemplate
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
    
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    [DependsOn(nameof(InnerLeftContentTemplate))]
    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }
    
    public IDataTemplate? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }
    
    [DependsOn(nameof(InnerRightContentTemplate))]
    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    public IDataTemplate? InnerRightContentTemplate
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
    
    public bool IsButtonSpinnerFloatable
    {
        get => GetValue(IsButtonSpinnerFloatableProperty);
        set => SetValue(IsButtonSpinnerFloatableProperty, value);
    }
    
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义

    public static readonly StyledProperty<double> SpinnerHandleWidthProperty =
        AvaloniaProperty.Register<ButtonSpinner, double>(nameof (SpinnerHandleWidth));

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<ButtonSpinner>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<ButtonSpinner>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<ButtonSpinner>();

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<ButtonSpinner>();

    public double SpinnerHandleWidth
    {
        get => GetValue(SpinnerHandleWidthProperty);
        set => SetValue(SpinnerHandleWidthProperty, value);
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
        set => SetValue(FormStatusProperty, value);
    }

    #endregion
    
    internal ButtonSpinnerDecoratedBox? DecoratedBox;
    private ButtonSpinnerHandle? _spinnerHandle;
    private bool _isFrameBorderBrushRelayed;

    static ButtonSpinner()
    {
        IsSpinEnabledProperty.Changed.Subscribe(IsSpinEnabledChanged);
    }
    
    public ButtonSpinner()
    {
    }
    
    private IconButton? _decreaseButton;
    private IconButton? DecreaseButton
    {
        get => _decreaseButton;
        set
        {
            if (_decreaseButton != null)
            {
                _decreaseButton.Click -= HandleButtonClick;
            }
            _decreaseButton = value;
            if (_decreaseButton != null)
            {
                _decreaseButton.Click += HandleButtonClick;
            }
        }
    }
    
    private IconButton? _increaseButton;
    private IconButton? IncreaseButton
    {
        get => _increaseButton;
        set
        {
            if (_increaseButton != null)
            {
                _increaseButton.Click -= HandleButtonClick;
            }
            _increaseButton = value;
            if (_increaseButton != null)
            {
                _increaseButton.Click += HandleButtonClick;
            }
        }
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        var borderThickness = DecoratedBox?.BorderThickness ?? default;
        return base.ArrangeOverride(finalSize).Inflate(borderThickness);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == LeftAddOnProperty ||
                change.Property == RightAddOnProperty ||
                change.Property == InnerLeftContentProperty ||
                change.Property == InnerRightContentProperty)
            {
                ConfigureAddOns();
            }
        }
        
        if (change.Property == ButtonSpinnerLocationProperty)
        {
            UpdatePseudoClasses();
        }

        if (change.Property == BorderBrushProperty)
        {
            RelayFrameBorderBrush();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_spinnerHandle != null)
        {
            _spinnerHandle.ButtonsCreated -= HandleButtonCreated;
        }
        _spinnerHandle = null;
        DecoratedBox = e.NameScope.Find<ButtonSpinnerDecoratedBox>("PART_DecoratedBox");
        _isFrameBorderBrushRelayed = false;
        base.OnApplyTemplate(e);
        var increaseButton = e.NameScope.Find<IconButton>("PART_IncreaseButton");
        var decreaseButton = e.NameScope.Find<IconButton>("PART_DecreaseButton");
        if (increaseButton is not null || decreaseButton is not null)
        {
            IncreaseButton = increaseButton;
            DecreaseButton = decreaseButton;
        }
        else if (DecoratedBox?.SpinnerContent is ButtonSpinnerHandle spinnerHandle)
        {
            _spinnerHandle                =  spinnerHandle;
            _spinnerHandle.ButtonsCreated += HandleButtonCreated;
            IncreaseButton                =  _spinnerHandle.IncreaseButton;
            DecreaseButton                =  _spinnerHandle.DecreaseButton;
        }
        else
        {
            IncreaseButton = null;
            DecreaseButton = null;
        }
        SetButtonUsage();
        ConfigureAddOns();
        RelayFrameBorderBrush();
    }

    /// <summary>
    /// Relays the owner's root border brush onto the input frame as a local value so
    /// application-level customization wins over the frame state machine, mirroring antd
    /// inline styles.root semantics and the shared AbstractTextInput behavior. An unset
    /// owner value restores the state machine.
    /// </summary>
    private void RelayFrameBorderBrush()
    {
        var decoratedBox = DecoratedBox;
        if (decoratedBox is null)
        {
            return;
        }

        var value = GetValue(BorderBrushProperty);
        if (value is null || ReferenceEquals(value, AvaloniaProperty.UnsetValue))
        {
            // Only hand the property back when this control is the one that took it over:
            // a nested owner (for example NumericUpDown, whose spinner is a ButtonSpinner)
            // may hold its own relay on the same frame node.
            if (_isFrameBorderBrushRelayed)
            {
                decoratedBox.ClearValue(BorderBrushProperty);
                _isFrameBorderBrushRelayed = false;
            }
            return;
        }

        decoratedBox.SetValue(BorderBrushProperty, value, BindingPriority.LocalValue);
        _isFrameBorderBrushRelayed = true;
    }

    private void HandleButtonCreated(object? sender, EventArgs e)
    {
        if (_spinnerHandle != null)
        {
            IncreaseButton = _spinnerHandle.IncreaseButton;
            DecreaseButton = _spinnerHandle.DecreaseButton;
        }
    }

    private void ConfigureAddOns()
    {
        if (LeftAddOn is PathIcon leftAddOnIcon)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter
            {
                Icon = leftAddOnIcon
            };
            iconPresenter[!SizeTypeProperty] = this[!SizeTypeProperty];
            LeftAddOn                        = iconPresenter;
        }
        if (InnerLeftContent is PathIcon innerLeftContent)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter
            {
                Icon = innerLeftContent
            };
            iconPresenter[!SizeTypeProperty] = this[!SizeTypeProperty];
            InnerLeftContent                 = iconPresenter;
        }
        if (RightAddOn is PathIcon rightAddOnIcon)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter
            {
                Icon = rightAddOnIcon
            };
            iconPresenter[!SizeTypeProperty] = this[!SizeTypeProperty];
            RightAddOn                       = iconPresenter;
        }
        if (InnerRightContent is PathIcon innerRightContent)
        {
            var iconPresenter = new SizeTypeAwareIconPresenter
            {
                Icon = innerRightContent
            };
            iconPresenter[!SizeTypeProperty] = this[!SizeTypeProperty];
            InnerRightContent                = iconPresenter;
        }
    }
    
    private void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ButtonSpinnerPseudoClass.Left, ButtonSpinnerLocation == ButtonSpinnerLocation.Left);
        PseudoClasses.Set(ButtonSpinnerPseudoClass.Right, ButtonSpinnerLocation == ButtonSpinnerLocation.Right);
    }
    
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        Point mousePosition;
        if (IncreaseButton != null && IncreaseButton.IsEnabled == false)
        {
            mousePosition = e.GetPosition(IncreaseButton);
            if (mousePosition.X > 0 && mousePosition.X < IncreaseButton.Width &&
                mousePosition.Y > 0 && mousePosition.Y < IncreaseButton.Height)
            {
                e.Handled = true;
            }
        }

        if (DecreaseButton != null && DecreaseButton.IsEnabled == false)
        {
            mousePosition = e.GetPosition(DecreaseButton);
            if (mousePosition.X > 0 && mousePosition.X < DecreaseButton.Width &&
                mousePosition.Y > 0 && mousePosition.Y < DecreaseButton.Height)
            {
                e.Handled = true;
            }
        }
    }
    
    protected override void OnPointerWheelChanged(PointerWheelEventArgs e)
    {
        base.OnPointerWheelChanged(e);

        if (IsSpinEnabled && IsKeyboardFocusWithin && e.Delta.Y != 0)
        {
            var direction = e.Delta.Y < 0 ? SpinDirection.Decrease : SpinDirection.Increase;
            OnSpin(new SpinEventArgs(SpinEvent, direction, true));
            e.Handled = true;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Up:
            {
                if (IsSpinEnabled)
                {
                    OnSpin(new SpinEventArgs(SpinEvent, SpinDirection.Increase));
                    e.Handled = true;
                }
                break;
            }
            case Key.Down:
            {
                if (IsSpinEnabled)
                {
                    OnSpin(new SpinEventArgs(SpinEvent, SpinDirection.Decrease));
                    e.Handled = true;
                }
                break;
            }
            case Key.Enter:
            {
                //Do not Spin on enter Key when spinners have focus
                if ((IncreaseButton != null && IncreaseButton.IsFocused)
                    || (DecreaseButton != null && DecreaseButton.IsFocused))
                {
                    e.Handled = true;
                }
                break;
            }
        }
    }
    
    protected override void OnValidSpinDirectionChanged(ValidSpinDirections oldValue, ValidSpinDirections newValue)
    {
        SetButtonUsage();
    }
    
    protected virtual void OnIsSpinEnabledChanged(bool oldValue, bool newValue)
    {
        SetButtonUsage();
    }
    
    private static void IsSpinEnabledChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Sender is ButtonSpinner spinner)
        {
            var oldValue = (bool)change.OldValue!;
            var newValue = (bool)change.NewValue!;
            spinner.OnIsSpinEnabledChanged(oldValue, newValue);
        }
    }
    
    private void SetButtonUsage()
    {
        if (IncreaseButton != null)
        {
            IncreaseButton.IsEnabled = IsSpinEnabled && ((ValidSpinDirection & ValidSpinDirections.Increase) == ValidSpinDirections.Increase);
        }

        if (DecreaseButton != null)
        {
            DecreaseButton.IsEnabled = IsSpinEnabled && ((ValidSpinDirection & ValidSpinDirections.Decrease) == ValidSpinDirections.Decrease);
        }
    }
    
    private void HandleButtonClick(object? sender, RoutedEventArgs e)
    {
        if (IsSpinEnabled)
        {
            var direction = sender == IncreaseButton ? SpinDirection.Increase : SpinDirection.Decrease;
            OnSpin(new SpinEventArgs(SpinEvent, direction));
        }
    }
    
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

    double ICompactSpaceAware.GetBorderThickness() => GetBorderThicknessForCompactSpace();
    
    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }
    
        if (DecoratedBox == null || DecoratedBox.StyleVariant != InputControlStyleVariant.Outlined)
        {
            return 0.0;
        }
    
        // 都一样宽
        return DecoratedBox.InnerBoxBorderThickness.Left;
    }
}
