using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data.Converters;
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

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<InputControlFrame>();

    /// <summary>
    /// Border brush set on the root of the hosting input control, template-bound by the host. When set, the theme
    /// projects it onto <see cref="TemplatedControl.BorderBrush"/> after every state brush (antd <c>styles.root</c>);
    /// unset hands the border back to the state machine.
    /// </summary>
    internal static readonly StyledProperty<IBrush?> HostBorderBrushProperty =
        AvaloniaProperty.Register<InputControlFrame, IBrush?>(nameof(HostBorderBrush));

    /// <summary>Background set on the root of the hosting input control; projected like <see cref="HostBorderBrush"/>.</summary>
    internal static readonly StyledProperty<IBrush?> HostBackgroundProperty =
        AvaloniaProperty.Register<InputControlFrame, IBrush?>(nameof(HostBackground));

    // Theme variables: the surface values of the current StyleVariant and EffectiveStatus, read from the owner's effective
    // global Tokens (the same resources OwnerTokenResource reads). The ControlTheme maps the interaction states (hover,
    // focus, pressed) onto them, so the variant x status matrix is not expressed as one style per combination. They are
    // direct properties fed by one OwnerTokenSet: a styled property with a binding per variable would add value-store
    // entries to every frame, and one owner subscription per variable would multiply the attach cost.
    internal static readonly DirectProperty<InputControlFrame, object?> FrameBorderBrushProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, object?>(nameof(FrameBorderBrush), o => o.FrameBorderBrush);

    internal static readonly DirectProperty<InputControlFrame, object?> FrameBackgroundProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, object?>(nameof(FrameBackground), o => o.FrameBackground);

    internal static readonly DirectProperty<InputControlFrame, object?> FrameHoverBorderBrushProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, object?>(nameof(FrameHoverBorderBrush), o => o.FrameHoverBorderBrush);

    internal static readonly DirectProperty<InputControlFrame, object?> FrameHoverBackgroundProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, object?>(nameof(FrameHoverBackground), o => o.FrameHoverBackground);

    internal static readonly DirectProperty<InputControlFrame, object?> FrameActiveBorderBrushProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, object?>(nameof(FrameActiveBorderBrush), o => o.FrameActiveBorderBrush);

    internal static readonly DirectProperty<InputControlFrame, object?> FrameActiveShadowProperty =
        AvaloniaProperty.RegisterDirect<InputControlFrame, object?>(nameof(FrameActiveShadow), o => o.FrameActiveShadow);

    internal object? FrameBorderBrush => FrameTokens.GetValue(0);

    internal object? FrameBackground => FrameTokens.GetValue(1);

    internal object? FrameHoverBorderBrush => FrameTokens.GetValue(2);

    internal object? FrameHoverBackground => FrameTokens.GetValue(3);

    internal object? FrameActiveBorderBrush => FrameTokens.GetValue(4);

    internal object? FrameActiveShadow => FrameTokens.GetValue(5);

    /// <summary>
    /// Converter of the ControlTheme setters that project a theme variable: an unresolved variable (no value yet, e.g.
    /// before the frame is attached) yields no value, so the target falls back exactly as an unresolved
    /// <c>OwnerTokenResource</c> setter does.
    /// </summary>
    public static readonly IValueConverter ThemeVariableConverter =
        new FuncValueConverter<object?, object?>(value => value ?? AvaloniaProperty.UnsetValue);

    internal IBrush? HostBorderBrush
    {
        get => GetValue(HostBorderBrushProperty);
        set => SetValue(HostBorderBrushProperty, value);
    }

    internal IBrush? HostBackground
    {
        get => GetValue(HostBackgroundProperty);
        set => SetValue(HostBackgroundProperty, value);
    }

    public FormValidateStatus FormStatus
    {
        get => GetValue(FormStatusProperty);
        internal set => SetCurrentValue(FormStatusProperty, value);
    }

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

    internal bool IsUsedInCompactSpace
    {
        get => GetValue(IsUsedInCompactSpaceProperty);
        set => SetValue(IsUsedInCompactSpaceProperty, value);
    }

    #endregion

    private readonly TransitionInitializationSession _transitionInitialization;
    private OwnerTokenSet? _frameTokens;

    static InputControlFrame()
    {
        AffectsRender<InputControlFrame>(BorderBrushProperty, BackgroundProperty);
    }

    public InputControlFrame()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _transitionInitialization.CompleteInitialization();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
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

        if (change.Property == HostBorderBrushProperty ||
            change.Property == HostBackgroundProperty)
        {
            UpdateHostBrushPseudoClasses();
        }

        if (_frameTokens is not null &&
            (change.Property == StyleVariantProperty || change.Property == EffectiveStatusProperty))
        {
            UpdateFrameTokenKeys(_frameTokens);
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
        UpdateHostBrushPseudoClasses();
        UpdateEffectiveStatus();
        _transitionInitialization.CompleteInitialization();
    }

    private void UpdateHostBrushPseudoClasses()
    {
        PseudoClasses.Set(HostBorderBrushPseudoClass, HostBorderBrush is not null);
        PseudoClasses.Set(HostBackgroundPseudoClass, HostBackground is not null);
    }

    internal const string HostBorderBrushPseudoClass = ":host-border-brush";
    internal const string HostBackgroundPseudoClass = ":host-background";

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

    private const int FrameVariableCount = 6;

    private static readonly DirectPropertyBase<object?>[] s_frameVariableProperties =
    [
        FrameBorderBrushProperty, FrameBackgroundProperty, FrameHoverBorderBrushProperty, FrameHoverBackgroundProperty,
        FrameActiveBorderBrushProperty, FrameActiveShadowProperty
    ];

    /// <summary>
    /// Points the theme variables at the Tokens of the current StyleVariant and EffectiveStatus; variables the variant's
    /// interaction styles do not read have no Token. An unchanged variable is not resolved again.
    /// </summary>
    /// <summary>
    /// The Tokens behind the theme variables, created and pointed at the current StyleVariant and EffectiveStatus on the
    /// first read, so a style that reads a variable always finds it configured. It only listens to this frame, so it
    /// lives and dies with it; entries resolve when a style first reads them.
    /// </summary>
    private OwnerTokenSet FrameTokens
    {
        get
        {
            if (_frameTokens is null)
            {
                _frameTokens = new OwnerTokenSet(this, FrameVariableCount, OnFrameVariableChanged);
                UpdateFrameTokenKeys(_frameTokens);
            }

            return _frameTokens;
        }
    }

    private void UpdateFrameTokenKeys(OwnerTokenSet tokens)
    {
        var status = EffectiveStatus;
        SharedTokenKind? border          = null;
        SharedTokenKind? background      = null;
        SharedTokenKind? hoverBorder     = null;
        SharedTokenKind? hoverBackground = null;
        SharedTokenKind? activeBorder    = null;
        SharedTokenKind? activeShadow    = null;
        switch (StyleVariant)
        {
            case InputControlStyleVariant.Outlined:
                border = ByStatus(status, SharedTokenKind.ColorBorder, SharedTokenKind.ColorError,
                    SharedTokenKind.ColorWarning);
                background = SharedTokenKind.ColorBgContainer;
                hoverBorder = ByStatus(status, SharedTokenKind.ColorPrimaryHover, SharedTokenKind.ColorErrorBorderHover,
                    SharedTokenKind.ColorWarningBorderHover);
                activeBorder = ByStatus(status, SharedTokenKind.ColorPrimary, SharedTokenKind.ColorError,
                    SharedTokenKind.ColorWarning);
                activeShadow = ByStatus(status, SharedTokenKind.InputActiveShadow,
                    SharedTokenKind.InputErrorActiveShadow, SharedTokenKind.InputWarningActiveShadow);
                break;
            case InputControlStyleVariant.Filled:
                border = ByStatus(status, SharedTokenKind.ColorFillTertiary, SharedTokenKind.ColorErrorBg,
                    SharedTokenKind.ColorWarningBg);
                background = border;
                hoverBackground = ByStatus(status, SharedTokenKind.ColorFillSecondary, SharedTokenKind.ColorErrorBgHover,
                    SharedTokenKind.ColorWarningBgHover);
                activeBorder = ByStatus(status, SharedTokenKind.ColorPrimary, SharedTokenKind.ColorError,
                    SharedTokenKind.ColorWarning);
                break;
            case InputControlStyleVariant.Underlined:
                border = ByStatus(status, SharedTokenKind.ColorBorder, SharedTokenKind.ColorError,
                    SharedTokenKind.ColorWarning);
                hoverBorder = ByStatus(status, SharedTokenKind.ColorPrimaryHover, SharedTokenKind.ColorErrorBorderHover,
                    SharedTokenKind.ColorWarningBorderHover);
                activeBorder = ByStatus(status, SharedTokenKind.ColorPrimary, SharedTokenKind.ColorError,
                    SharedTokenKind.ColorWarning);
                break;
        }

        tokens.SetKind(0, border);
        tokens.SetKind(1, background);
        tokens.SetKind(2, hoverBorder);
        tokens.SetKind(3, hoverBackground);
        tokens.SetKind(4, activeBorder);
        tokens.SetKind(5, activeShadow);
    }

    private static SharedTokenKind ByStatus(InputControlStatus status,
                                            SharedTokenKind normal,
                                            SharedTokenKind error,
                                            SharedTokenKind warning)
    {
        return status switch
        {
            InputControlStatus.Error => error,
            InputControlStatus.Warning => warning,
            _ => normal
        };
    }

    private void OnFrameVariableChanged(int index, object? oldValue, object? newValue)
    {
        RaisePropertyChanged(s_frameVariableProperties[index], oldValue, newValue);
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
