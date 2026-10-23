using AtomUI.Animations;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Shared visual state owner for controls that render an AtomUI input surface.
/// Layout-specific decorated boxes derive from this type; they do not own a
/// second validation or variant state machine.
/// </summary>
internal class InputControlFrame : ContentControl,
                                   ICustomizableSizeTypeAware,
                                   IMotionAwareControl,
                                   IInputControlStatusAware,
                                   IInputControlStyleVariantAware
{
    #region 公共属性定义

    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<InputControlFrame>();

    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<InputControlFrame>();

    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<InputControlFrame>();

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<InputControlFrame>();

    public static readonly StyledProperty<BoxShadows> BoxShadowProperty =
        Border.BoxShadowProperty.AddOwner<InputControlFrame>();

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

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    public BoxShadows BoxShadow
    {
        get => GetValue(BoxShadowProperty);
        set => SetValue(BoxShadowProperty, value);
    }


    public FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        internal set => SetCurrentValue(FormStatusProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<InputControlFrame>();

    internal static readonly DirectProperty<InputControlFrame, InputControlStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, InputControlStatus>(
            nameof(EffectiveStatus),
            o => o.EffectiveStatus,
            (o, value) => o.EffectiveStatus = value);

    internal static readonly DirectProperty<InputControlFrame, bool> IsInnerBoxHoverProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, bool>(
            nameof(IsInnerBoxHover),
            o => o.IsInnerBoxHover,
            (o, value) => o.IsInnerBoxHover = value);

    internal static readonly DirectProperty<InputControlFrame, bool> IsInnerBoxPressedProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, bool>(
            nameof(IsInnerBoxPressed),
            o => o.IsInnerBoxPressed,
            (o, value) => o.IsInnerBoxPressed = value);

    internal static readonly DirectProperty<InputControlFrame, bool> IsInputFocusWithinProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, bool>(
            nameof(IsInputFocusWithin),
            o => o.IsInputFocusWithin,
            (o, value) => o.IsInputFocusWithin = value);

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<InputControlFrame>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<InputControlFrame>();

    // Size presets supply defaults; owner template inputs retain native precedence.
    internal static readonly StyledProperty<CornerRadius> SizeCornerRadiusProperty =
        AvaloniaProperty.Register<InputControlFrame, CornerRadius>(nameof(SizeCornerRadius));

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<InputControlFrame>();


    private InputControlStatus _effectiveStatus;

    internal InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
    }

    private bool _isInnerBoxHover;

    internal bool IsInnerBoxHover
    {
        get => _isInnerBoxHover;
        set => SetAndRaise(IsInnerBoxHoverProperty, ref _isInnerBoxHover, value);
    }

    private bool _isInnerBoxPressed;

    internal bool IsInnerBoxPressed
    {
        get => _isInnerBoxPressed;
        set => SetAndRaise(IsInnerBoxPressedProperty, ref _isInnerBoxPressed, value);
    }

    private bool _isInputFocusWithin;

    internal bool IsInputFocusWithin
    {
        get => _isInputFocusWithin;
        set => SetAndRaise(IsInputFocusWithinProperty, ref _isInputFocusWithin, value);
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

    internal CornerRadius SizeCornerRadius
    {
        get => GetValue(SizeCornerRadiusProperty);
        set => SetValue(SizeCornerRadiusProperty, value);
    }

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    #endregion

    private readonly StateMotionSession _stateMotion;

    static InputControlFrame()
    {
        AffectsRender<InputControlFrame>(BorderBrushProperty, BackgroundProperty);
    }

    public InputControlFrame()
    {
        _stateMotion = new StateMotionSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _stateMotion.CompleteInitialization();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _stateMotion.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _stateMotion?.Reset();
        base.OnTemplateChanged(e);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == StyleVariantProperty)
        {
            UpdateVariantPseudoClasses();
        }

        if (change.Property == IsKeyboardFocusWithinProperty ||
            change.Property == IsInputFocusWithinProperty)
        {
            UpdateFocusWithinPseudoClass();
        }

        if (change.Property == IsInnerBoxPressedProperty)
        {
            PseudoClasses.Set(StdPseudoClass.Pressed, IsInnerBoxPressed);
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateEffectiveStatus();
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateVariantPseudoClasses();
        UpdateFocusWithinPseudoClass();
        PseudoClasses.Set(StdPseudoClass.Pressed, IsInnerBoxPressed);
        UpdateEffectiveStatus();
        _stateMotion.CompleteInitialization();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        SetInnerBoxHover(true);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        SetInnerBoxHover(false);
        SetInnerBoxPressed(false);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        SetInnerBoxHover(true);
        SetInnerBoxPressed(true);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        SetInnerBoxPressed(false);
        SetInnerBoxHover(true);
    }

    protected virtual void UpdateVariantPseudoClasses()
    {
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Outline,
            StyleVariant == InputControlStyleVariant.Outlined);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Filled,
            StyleVariant == InputControlStyleVariant.Filled);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Borderless,
            StyleVariant == InputControlStyleVariant.Borderless);
        PseudoClasses.Set(AddOnDecoratedBoxPseudoClass.Underlined,
            StyleVariant == InputControlStyleVariant.Underlined);
    }

    private void UpdateEffectiveStatus()
    {
        var effectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);

        if (EffectiveStatus != effectiveStatus)
        {
            EffectiveStatus = effectiveStatus;
        }

        PseudoClasses.Set(StdPseudoClass.Warning, effectiveStatus == InputControlStatus.Warning);
    }

    private void SetInnerBoxHover(bool value)
    {
        if (IsInnerBoxHover != value)
        {
            IsInnerBoxHover = value;
        }
    }

    private void SetInnerBoxPressed(bool value)
    {
        if (IsInnerBoxPressed != value)
        {
            IsInnerBoxPressed = value;
        }
    }

    private void UpdateFocusWithinPseudoClass()
    {
        PseudoClasses.Set(
            StdPseudoClass.FocusWithIn,
            IsKeyboardFocusWithin || IsInputFocusWithin);
    }
}
