using Avalonia.Media;
using AtomUI.Controls;
using AtomUI.Desktop.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Edge of a <see cref="ButtonSpinner" /> where the spinner handle is placed.
/// </summary>
public enum ButtonSpinnerLocation
{
    /// <summary>
    /// The spinner handle is placed at the left edge of the control.
    /// </summary>
    Left,
    /// <summary>
    /// The spinner handle is placed at the right edge of the control. This is the default.
    /// </summary>
    Right
}

/// <summary>
/// An input frame with a handle of increase and decrease buttons at its left or right edge.
/// The control holds no value itself: it only raises <see cref="Spinner.SpinEvent" />, and a handler of
/// that event changes the value.
/// </summary>
[PseudoClasses(ButtonSpinnerPseudoClass.Left, ButtonSpinnerPseudoClass.Right)]
public partial class ButtonSpinner : Spinner,
                                     IMotionAwareControl,
                                     ICompactSpaceAware,
                                     ICustomizableSizeTypeAware,
                                     IInputControlStatusAware,
                                     IInputControlStyleVariantAware
{
    #region 公共属性定义
    /// <summary>
    /// Identifies the <see cref="IsSpinEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSpinEnabledProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof(IsSpinEnabled), true);
    
    /// <summary>
    /// Identifies the <see cref="IsButtonSpinnerVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsButtonSpinnerVisibleProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof(IsButtonSpinnerVisible), true);
    
    /// <summary>
    /// Identifies the <see cref="ButtonSpinnerLocation" /> property.
    /// </summary>
    public static readonly StyledProperty<ButtonSpinnerLocation> ButtonSpinnerLocationProperty =
        AvaloniaProperty.Register<ButtonSpinner, ButtonSpinnerLocation>(nameof(ButtonSpinnerLocation), ButtonSpinnerLocation.Right);

    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<ButtonSpinner>();
    
    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<ButtonSpinner>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<ButtonSpinner>();
    
    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<ButtonSpinner>();

    /// <summary>
    /// Identifies the <see cref="InnerLeftContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> InnerLeftContentProperty =
        AvaloniaProperty.Register<ButtonSpinner, object?>(nameof(InnerLeftContent));
    
    /// <summary>
    /// Identifies the <see cref="InnerLeftContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerLeftContentTemplateProperty =
        AvaloniaProperty.Register<ButtonSpinner, IDataTemplate?>(nameof(InnerLeftContentTemplate));

    /// <summary>
    /// Identifies the <see cref="InnerRightContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> InnerRightContentProperty =
        AvaloniaProperty.Register<ButtonSpinner, object?>(nameof(InnerRightContent));
    
    /// <summary>
    /// Identifies the <see cref="InnerRightContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> InnerRightContentTemplateProperty =
        AvaloniaProperty.Register<ButtonSpinner, IDataTemplate?>(nameof(InnerRightContentTemplate));

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<ButtonSpinner>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<ButtonSpinner>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<ButtonSpinner>();
    
    /// <summary>
    /// Identifies the <see cref="IsButtonSpinnerFloatable" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsButtonSpinnerFloatableProperty =
        AvaloniaProperty.Register<ButtonSpinner, bool>(nameof (IsButtonSpinnerFloatable), false);
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty = 
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ButtonSpinner>();

    /// <summary>
    /// Gets or sets whether the increase and decrease buttons, the Up and Down keys and the mouse wheel raise
    /// <see cref="Spinner.SpinEvent" />. When <see langword="false" />, both buttons are disabled whatever the
    /// valid spin direction allows.
    /// </summary>
    public bool IsSpinEnabled
    {
        get => GetValue(IsSpinEnabledProperty);
        set => SetValue(IsSpinEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the spinner handle is shown.
    /// When <see langword="false" />, the buttons and the divider next to them are transparent and the content
    /// area reserves no room for them. Spinning by keyboard and mouse wheel does not depend on this property.
    /// </summary>
    public bool IsButtonSpinnerVisible
    {
        get => GetValue(IsButtonSpinnerVisibleProperty);
        set => SetValue(IsButtonSpinnerVisibleProperty, value);
    }

    /// <summary>
    /// Gets or sets the edge of the input frame that holds the spinner handle. The right edge is the default.
    /// Changing the value also switches the <c>left</c> and <c>right</c> pseudo-classes of the control.
    /// </summary>
    public ButtonSpinnerLocation ButtonSpinnerLocation
    {
        get => GetValue(ButtonSpinnerLocationProperty);
        set => SetValue(ButtonSpinnerLocationProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content of the add-on box attached to the left of the input frame. The box is absent
    /// while this value and <see cref="LeftAddOnTemplate" /> are both <see langword="null" />.
    /// A <see cref="PathIcon" /> value is replaced by a presenter that sizes the icon according to
    /// <see cref="SizeType" />.
    /// </summary>
    [DependsOn(nameof(LeftAddOnTemplate))]
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds <see cref="LeftAddOn" /> when it is not a control.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }
        
    /// <summary>
    /// Gets or sets the content of the add-on box attached to the right of the input frame. The box is absent
    /// while this value and <see cref="RightAddOnTemplate" /> are both <see langword="null" />.
    /// A <see cref="PathIcon" /> value is wrapped in the same way as for <see cref="LeftAddOn" />.
    /// </summary>
    [DependsOn(nameof(RightAddOnTemplate))]
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds <see cref="RightAddOn" /> when it is not a control.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown inside the input frame, to the left of the main content. The slot is hidden
    /// while this value and <see cref="InnerLeftContentTemplate" /> are both <see langword="null" />.
    /// A <see cref="PathIcon" /> value is wrapped in the same way as for <see cref="LeftAddOn" />.
    /// </summary>
    [DependsOn(nameof(InnerLeftContentTemplate))]
    public object? InnerLeftContent
    {
        get => GetValue(InnerLeftContentProperty);
        set => SetValue(InnerLeftContentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds <see cref="InnerLeftContent" /> when it is not a control.
    /// </summary>
    public IDataTemplate? InnerLeftContentTemplate
    {
        get => GetValue(InnerLeftContentTemplateProperty);
        set => SetValue(InnerLeftContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content shown inside the input frame, to the right of the main content. The slot is hidden
    /// while this value and <see cref="InnerRightContentTemplate" /> are both <see langword="null" />.
    /// A <see cref="PathIcon" /> value is wrapped in the same way as for <see cref="LeftAddOn" />.
    /// </summary>
    [DependsOn(nameof(InnerRightContentTemplate))]
    public object? InnerRightContent
    {
        get => GetValue(InnerRightContentProperty);
        set => SetValue(InnerRightContentProperty, value);
    }

    /// <summary>
    /// Gets or sets the template that builds <see cref="InnerRightContent" /> when it is not a control.
    /// </summary>
    public IDataTemplate? InnerRightContentTemplate
    {
        get => GetValue(InnerRightContentTemplateProperty);
        set => SetValue(InnerRightContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the preset size of the control; <c>Middle</c> is the default. It selects the corner radius of
    /// the frame and the handle and the size of the icons assigned to the add-on and inner content properties.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Gets or sets the frame style of the control; <c>Outlined</c> is the default.
    /// The input frame and the spinner handle both follow it.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// Gets or sets the validation state painted by the input frame; <c>Default</c> is the default value.
    /// Any other value recolors the frame with the colors of that state.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the spinner handle stays out of sight until the pointer is over the control;
    /// the default is <see langword="false" />. The handle then slides in on hover and the content shifts aside
    /// for it. A disabled control keeps the handle in view. The property has no visible effect while
    /// <see cref="IsButtonSpinnerVisible" /> is <see langword="false" />.
    /// </summary>
    public bool IsButtonSpinnerFloatable
    {
        get => GetValue(IsButtonSpinnerFloatableProperty);
        set => SetValue(IsButtonSpinnerFloatableProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the input frame and the spinner handle play their transitions, such as the frame
    /// colors and the fading of the handle. When the value is not set on the control, the theme supplies the
    /// <c>EnableMotion</c> token of the owning control.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }
    
    #endregion

    #region 内部属性定义
    internal static readonly StyledProperty<IBrush?> HandleBgProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandleBg));

    internal static readonly StyledProperty<IBrush?> FilledHandleBgProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(FilledHandleBg));

    internal static readonly StyledProperty<IBrush?> HandleBorderColorProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandleBorderColor));

    internal static readonly StyledProperty<IBrush?> HandleHoverColorProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandleHoverColor));

    internal static readonly StyledProperty<IBrush?> HandleHoverBgProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandleHoverBg));

    internal static readonly StyledProperty<IBrush?> HandleIconColorProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandleIconColor));

    internal static readonly StyledProperty<IBrush?> HandleActiveBgProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandleActiveBg));

    internal static readonly StyledProperty<double> HandleIconSizeProperty =
        AvaloniaProperty.Register<ButtonSpinner, double>(nameof(HandleIconSize));

    internal static readonly StyledProperty<IBrush?> HandlePressedColorProperty =
        AvaloniaProperty.Register<ButtonSpinner, IBrush?>(nameof(HandlePressedColor));



    /// <summary>
    /// Identifies the <see cref="SpinnerHandleWidth" /> property.
    /// </summary>
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

    internal IBrush? HandleBg
    {
        get => GetValue(HandleBgProperty);
        set => SetValue(HandleBgProperty, value);
    }

    internal IBrush? FilledHandleBg
    {
        get => GetValue(FilledHandleBgProperty);
        set => SetValue(FilledHandleBgProperty, value);
    }

    internal IBrush? HandleBorderColor
    {
        get => GetValue(HandleBorderColorProperty);
        set => SetValue(HandleBorderColorProperty, value);
    }

    internal IBrush? HandleHoverColor
    {
        get => GetValue(HandleHoverColorProperty);
        set => SetValue(HandleHoverColorProperty, value);
    }

    internal IBrush? HandleHoverBg
    {
        get => GetValue(HandleHoverBgProperty);
        set => SetValue(HandleHoverBgProperty, value);
    }

    internal IBrush? HandleIconColor
    {
        get => GetValue(HandleIconColorProperty);
        set => SetValue(HandleIconColorProperty, value);
    }

    internal IBrush? HandleActiveBg
    {
        get => GetValue(HandleActiveBgProperty);
        set => SetValue(HandleActiveBgProperty, value);
    }

    internal double HandleIconSize
    {
        get => GetValue(HandleIconSizeProperty);
        set => SetValue(HandleIconSizeProperty, value);
    }

    internal IBrush? HandlePressedColor
    {
        get => GetValue(HandlePressedColorProperty);
        set => SetValue(HandlePressedColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the spinner handle in DIPs. While the handle is shown and not floatable, the
    /// frame reserves this width beside the content. When the value is not set on the control, the theme supplies
    /// the ButtonSpinner <c>HandleWidth</c> token.
    /// </summary>
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

    /// <summary>
    /// Arranges the template and returns the arranged size enlarged on every side by the border thickness of the
    /// decorated box. An override must call the base member to keep the frame border in the returned size.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var borderThickness = DecoratedBox?.BorderThickness ?? default;
        return base.ArrangeOverride(finalSize).Inflate(borderThickness);
    }

    /// <summary>
    /// Applies the <see cref="PathIcon" /> wrapping again when <see cref="LeftAddOn" />, <see cref="RightAddOn" />,
    /// <see cref="InnerLeftContent" /> or <see cref="InnerRightContent" /> changes while the control is attached
    /// to the visual tree, and updates the <c>left</c> and <c>right</c> pseudo-classes when
    /// <see cref="ButtonSpinnerLocation" /> changes. An override must call the base member first.
    /// </summary>
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
    }

    /// <summary>
    /// Finds the decorated box and the increase and decrease buttons, then enables the buttons for the current
    /// spin state and wraps the icon add-ons. The buttons come from the template parts <c>PART_IncreaseButton</c>
    /// and <c>PART_DecreaseButton</c> or, when the template has none, from the handle inside the decorated box,
    /// which reports its buttons once they are created. An override must call the base member.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (_spinnerHandle != null)
        {
            _spinnerHandle.ButtonsCreated -= HandleButtonCreated;
        }
        _spinnerHandle = null;
        DecoratedBox = e.NameScope.Find<ButtonSpinnerDecoratedBox>("PART_DecoratedBox");
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
        // The root BorderBrush reaches the input frame through the template's HostBorderBrush binding, which the
        // frame theme projects over its state brushes (antd styles.root).
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
    
    /// <summary>
    /// Marks the pointer release as handled when it lies inside a disabled increase or decrease button,
    /// so the release stops at this control. The base member is called first and sees the release unhandled.
    /// </summary>
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
    
    /// <summary>
    /// Raises <see cref="Spinner.SpinEvent" /> with the mouse-wheel flag set when the wheel is turned while
    /// <see cref="IsSpinEnabled" /> is set and the keyboard focus is inside the control: a negative vertical delta
    /// decreases, a positive one increases. The wheel event is then marked handled.
    /// </summary>
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

    /// <summary>
    /// Raises <see cref="Spinner.SpinEvent" /> for the Up and Down keys while <see cref="IsSpinEnabled" /> is set
    /// and marks those key presses handled. Enter is marked handled while a spinner button has the focus.
    /// </summary>
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
    
    /// <summary>
    /// Called when <see cref="Spinner.ValidSpinDirection" /> changes; enables or disables the increase and decrease
    /// buttons for that value and the current <see cref="IsSpinEnabled" />.
    /// </summary>
    protected override void OnValidSpinDirectionChanged(ValidSpinDirections oldValue, ValidSpinDirections newValue)
    {
        SetButtonUsage();
    }
    
    /// <summary>
    /// Called when <see cref="IsSpinEnabled" /> changes; enables or disables the increase and decrease buttons
    /// for the new value and the valid spin direction. An override must call the base member to keep the buttons
    /// in sync.
    /// </summary>
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
    
    /// <summary>
    /// Returns the thickness of the border that a compact <c>Space</c> layout gets from this control. The result
    /// is 0 unless the control is used in compact space and its style variant is <c>Outlined</c>; otherwise it is
    /// the frame border thickness, which is the same on all sides. An override that does not compute its own value
    /// must call the base member.
    /// </summary>
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
    
        // The frame border is as thick on every side, so the left value stands for all of them.
        return DecoratedBox.InnerBoxBorderThickness.Left;
    }
}
