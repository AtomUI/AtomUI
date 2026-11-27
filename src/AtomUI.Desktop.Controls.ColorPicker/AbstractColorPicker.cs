using AtomUI.Animations;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Media;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Converters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Input.Raw;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AtomUIFlyout = AtomUI.Desktop.Controls.Flyout;
using AvaloniaButton = Avalonia.Controls.Button;

/// <summary>
/// Decides when a change made in the popup becomes the value of the picker outside confirm mode. In confirm
/// mode only a committed draft is written, whatever the mode.
/// </summary>
public enum ColorPickerValueSyncMode
{
    /// <summary>
    /// A change made in the popup is written to the value at once. This is the default.
    /// </summary>
    Immediate,
    /// <summary>
    /// A change made in the popup is written to the value once, when the popup closes.
    /// </summary>
    OnCompleted
}

/// <summary>
/// Base class of the color picker controls: a button that shows the picked color in a color block, with
/// optional text, and opens a popup for editing it. It owns the popup with its trigger, delays and
/// placement, the confirm mode and the form item integration. <see cref="ColorPicker" /> and
/// <see cref="GradientColorPicker" /> derive from it. They supply the content of the popup through
/// <see cref="CreatePresenter" />, and the text and the color block of the trigger through
/// <see cref="GenerateValueText" /> and <see cref="GenerateColorBlockBackground" />.
/// </summary>
public abstract class AbstractColorPicker : AvaloniaButton,
                                            ICustomizableSizeTypeAware,
                                            IMotionAwareControl,
                                            ICompactSpaceAware,
                                            IFormItemAware,
                                            IInputControlStatusAware,
                                            IInputControlStyleVariantAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="IsNeedConfirm" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsNeedConfirmProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsNeedConfirm));

    /// <summary>
    /// Identifies the <see cref="Format" /> property.
    /// </summary>
    public static readonly StyledProperty<ColorFormat> FormatProperty =
        AbstractColorPickerView.FormatProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="TriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        AvaloniaProperty.Register<AbstractColorPicker, FlyoutTriggerType>(nameof(TriggerType), FlyoutTriggerType.Click);

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AtomUIFlyout.IsPointAtCenterProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="PlacementGravity" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Popup.PlacementGravityProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="PlacementAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Popup.PlacementAnchorProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="MarginToAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="IsAlphaEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAlphaEnabledProperty =
        AbstractColorPickerView.IsAlphaEnabledProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="IsFormatEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsFormatEnabledProperty =
        AbstractColorPickerView.IsFormatEnabledProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="IsTextVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsTextVisibleProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsTextVisible));

    /// <summary>
    /// Identifies the <see cref="IsClearEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsClearEnabledProperty =
        AbstractColorPickerView.IsClearEnabledProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="EmptyColorText" /> property.
    /// </summary>
    public static readonly StyledProperty<string> EmptyColorTextProperty =
        AvaloniaProperty.Register<AbstractColorPicker, string>(nameof(EmptyColorText), string.Empty);

    /// <summary>
    /// Identifies the <see cref="IsPaletteGroupEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPaletteGroupEnabledProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsPaletteGroupEnabled));

    /// <summary>
    /// Identifies the <see cref="PaletteGroup" /> property.
    /// </summary>
    public static readonly StyledProperty<List<ColorPickerPalette>?> PaletteGroupProperty =
        ColorPickerPaletteGroup.PaletteGroupProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// Identifies the <see cref="MouseEnterDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        AvaloniaProperty.Register<AbstractColorPicker, int>(nameof(MouseEnterDelay), 200);

    /// <summary>
    /// Identifies the <see cref="MouseLeaveDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        AvaloniaProperty.Register<AbstractColorPicker, int>(nameof(MouseLeaveDelay), 200);

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<AbstractColorPicker>();

    /// <summary>
    /// The motion that plays when the popup opens. The value is forwarded to the popup. Without an explicit
    /// value the theme supplies <c>PopupMotionDefaults.PopoverOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// The motion that plays when the popup closes. The value is forwarded to the popup. Without an explicit
    /// value the theme supplies <c>PopupMotionDefaults.PopoverClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Switches the popup to confirm mode: the popup shows Cancel and Confirm buttons and edits a draft instead
    /// of the value of the picker. Confirm writes the draft to the value and closes the popup; cancel, or closing
    /// the popup any other way, leaves the value unchanged. While the popup is pinned open, confirm and cancel
    /// reset the draft and keep it open. Changing this property while the popup is open resets the draft. The
    /// default is <see langword="false" />.
    /// </summary>
    public bool IsNeedConfirm
    {
        get => GetValue(IsNeedConfirmProperty);
        set => SetValue(IsNeedConfirmProperty, value);
    }

    /// <summary>
    /// The color format of the value text and of the input in the popup: <see cref="ColorFormat.Hex" />,
    /// <see cref="ColorFormat.Rgba" /> or <see cref="ColorFormat.Hsva" />. The default is
    /// <see cref="ColorFormat.Hex" />. While <see cref="IsFormatEnabled" /> is <see langword="true" />, the
    /// format picked in the popup is written back to this property.
    /// </summary>
    public ColorFormat Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }

    /// <summary>
    /// The user action that opens and closes the popup: clicking the trigger, the pointer entering and leaving
    /// it, or the trigger receiving focus. The click trigger toggles the popup on a press and lets a press
    /// outside close it. The hover trigger opens the popup after <see cref="MouseEnterDelay" /> and closes it
    /// after <see cref="MouseLeaveDelay" /> once the pointer has left the trigger and the popup. The focus
    /// trigger opens it when the trigger receives focus or is pressed, and closes it when the focus leaves the
    /// trigger and the popup or the pointer is pressed outside them. The default is
    /// <see cref="FlyoutTriggerType.Click" />.
    /// </summary>
    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    /// <summary>
    /// Draws an arrow on the popup that points at the trigger. The arrow is drawn only while this property is
    /// <see langword="true" /> and <see cref="Placement" />, <see cref="PlacementAnchor" /> and
    /// <see cref="PlacementGravity" /> allow one. The theme sets it to <see langword="true" />.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Moves the popup so that its arrow points at the center of the trigger. The value is forwarded to the
    /// popup and has an effect only while the arrow is drawn.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// The position of the popup relative to the trigger. The value is forwarded to the popup as its requested
    /// placement. Without an explicit value the theme supplies
    /// <see cref="PlacementMode.BottomEdgeAlignedLeft" />. The placement is also one of the values that decide
    /// whether the arrow is drawn and where it sits.
    /// </summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// The direction in which the popup extends from its anchor point on the trigger, as a refinement of
    /// <see cref="Placement" />. The value is forwarded to the popup and is one of the values that decide
    /// whether the arrow is drawn and where it sits.
    /// </summary>
    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    /// <summary>
    /// The point on the trigger at which the popup is anchored, as a refinement of <see cref="Placement" />.
    /// The value is forwarded to the popup and is one of the values that decide whether the arrow is drawn and
    /// where it sits.
    /// </summary>
    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    /// <summary>
    /// The gap between the popup and the trigger, in device-independent pixels. The value is forwarded to the
    /// popup. Without an explicit value the theme supplies the <c>UniformlyMarginXXS</c> token.
    /// </summary>
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    /// <summary>
    /// Enables the alpha channel of the picked color. While it is <see langword="false" />, the alpha channel
    /// of a value is coerced to 255, so the color is opaque.
    /// </summary>
    public bool IsAlphaEnabled
    {
        get => GetValue(IsAlphaEnabledProperty);
        set => SetValue(IsAlphaEnabledProperty, value);
    }

    /// <summary>
    /// Shows the control in the popup that switches the color format. The format picked there is written back
    /// to <see cref="Format" />. While it is <see langword="false" />, only <see cref="Format" /> decides the
    /// format. The default is <see langword="true" />.
    /// </summary>
    public bool IsFormatEnabled
    {
        get => GetValue(IsFormatEnabledProperty);
        set => SetValue(IsFormatEnabledProperty, value);
    }

    /// <summary>
    /// Shows text next to the color block of the trigger. The text is produced by
    /// <see cref="GenerateValueText" />, which runs when this property or <see cref="Format" /> changes. The
    /// default is <see langword="false" />.
    /// </summary>
    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }

    /// <summary>
    /// Shows the clear control in the popup, which empties the value of the picker as soon as it is used. The
    /// default is <see langword="false" />.
    /// </summary>
    public bool IsClearEnabled
    {
        get => GetValue(IsClearEnabledProperty);
        set => SetValue(IsClearEnabledProperty, value);
    }

    /// <summary>
    /// The size preset of the picker. It sets the corner radius and the font size of the trigger text; the
    /// theme treats <see cref="CustomizableSizeType.Custom" /> like <see cref="CustomizableSizeType.Middle" />.
    /// The color block follows the preset too, unless the size type is
    /// <see cref="CustomizableSizeType.Custom" /> and <c>Height</c> is set: then the color block fills the
    /// height that is left by the vertical padding and border.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// Switches the open and close motions of the popup on or off; the value is forwarded to the popup. While
    /// it is <see langword="true" />, the theme also lets the border color of the trigger change with a
    /// transition. Without an explicit value the theme supplies the EnableMotion token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// The text that the trigger shows while the picker has no color. Without an explicit value the theme
    /// supplies the localized empty-color text of the color picker, which is "Transparent" in English.
    /// </summary>
    public string EmptyColorText
    {
        get => GetValue(EmptyColorTextProperty);
        set => SetValue(EmptyColorTextProperty, value);
    }

    /// <summary>
    /// Shows the palette group of preset colors at the side of the popup. While it is
    /// <see langword="true" /> and <see cref="PaletteGroup" /> is <see langword="null" />, the popup uses its
    /// built-in default palettes. The default is <see langword="false" />.
    /// </summary>
    public bool IsPaletteGroupEnabled
    {
        get => GetValue(IsPaletteGroupEnabledProperty);
        set => SetValue(IsPaletteGroupEnabledProperty, value);
    }

    /// <summary>
    /// The palettes of the palette group in the popup, or <see langword="null" /> to use the built-in default
    /// palettes. The group is shown only while <see cref="IsPaletteGroupEnabled" /> is <see langword="true" />.
    /// </summary>
    public List<ColorPickerPalette>? PaletteGroup
    {
        get => GetValue(PaletteGroupProperty);
        set => SetValue(PaletteGroupProperty, value);
    }

    /// <summary>
    /// The style variant that the picker accepts as an input control. The default picker themes do not select
    /// by it, so it does not change the trigger.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// The validation state of the picker. The theme paints the border of the trigger in the error color while
    /// it is <see cref="InputControlStatus.Error" /> and in the warning color while it is
    /// <see cref="InputControlStatus.Warning" />. The validation result of a form item is mapped onto it by
    /// <see cref="NotifyValidateStatus" />.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// The time, in milliseconds, that the pointer must rest on the trigger before the popup opens. <c>0</c>
    /// opens it at once. It takes effect with the hover trigger. The default is <c>200</c>.
    /// </summary>
    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    /// <summary>
    /// The time, in milliseconds, between the pointer leaving the trigger and the popup and the popup closing.
    /// <c>0</c> closes it at once. It takes effect with the hover trigger. The default is <c>200</c>.
    /// </summary>
    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }

    /// <summary>
    /// Shows the popup in the overlay layer of the window instead of a native popup window. The value is
    /// forwarded to the popup. The default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the picker popup remains open while its placement target is available.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 内部属性定义


    internal static readonly DirectProperty<AbstractColorPicker, IBrush?> ColorBlockBackgroundProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, IBrush?>(
            nameof(ColorBlockBackground),
            o => o.ColorBlockBackground,
            (o, v) => o.ColorBlockBackground = v);

    internal static readonly DirectProperty<AbstractColorPicker, CornerRadius> EffectiveCornerRadiusProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, CornerRadius>(nameof(EffectiveCornerRadius),
            o => o.EffectiveCornerRadius,
            (o, v) => o.EffectiveCornerRadius = v);

    internal static readonly DirectProperty<AbstractColorPicker, double> ColorBlockSizeProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, double>(nameof(ColorBlockSize),
            o => o.ColorBlockSize,
            (o, v) => o.ColorBlockSize = v);

    internal static readonly StyledProperty<Thickness> TriggerPaddingProperty =
        AvaloniaProperty.Register<AbstractColorPicker, Thickness>(nameof(TriggerPadding));

    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty =
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<AbstractColorPicker>();

    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty =
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<AbstractColorPicker>();

    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<AbstractColorPicker>();

    internal static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<AbstractColorPicker, bool>(nameof(IsPickerOpen), coerce: CoerceIsPickerOpen);

    internal static readonly StyledProperty<Control?> PickerPresenterProperty =
        AvaloniaProperty.Register<AbstractColorPicker, Control?>(nameof(PickerPresenter));

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsArrowVisibleEffectiveProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsArrowVisibleEffective),
            o => o.IsArrowVisibleEffective,
            (o, v) => o.IsArrowVisibleEffective = v);

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);

    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<AbstractColorPicker>();

    internal static readonly DirectProperty<AbstractColorPicker, bool> IsLightDismissEnabledProperty =
        AvaloniaProperty.RegisterDirect<AbstractColorPicker, bool>(nameof(IsLightDismissEnabled),
            o => o.IsLightDismissEnabled,
            (o, v) => o.IsLightDismissEnabled = v);

    private IBrush? _colorBlockBackground;

    internal IBrush? ColorBlockBackground
    {
        get => _colorBlockBackground;
        set => SetAndRaise(ColorBlockBackgroundProperty, ref _colorBlockBackground, value);
    }

    private CornerRadius _effectiveCornerRadius;

    internal CornerRadius EffectiveCornerRadius
    {
        get => _effectiveCornerRadius;
        set => SetAndRaise(EffectiveCornerRadiusProperty, ref _effectiveCornerRadius, value);
    }

    private double _colorBlockSize = double.NaN;

    internal double ColorBlockSize
    {
        get => _colorBlockSize;
        private set => SetAndRaise(ColorBlockSizeProperty, ref _colorBlockSize, value);
    }

    internal Thickness TriggerPadding
    {
        get => GetValue(TriggerPaddingProperty);
        set => SetValue(TriggerPaddingProperty, value);
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

    internal bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    internal Control? PickerPresenter
    {
        get => GetValue(PickerPresenterProperty);
        set => SetValue(PickerPresenterProperty, value);
    }

    private bool _isArrowVisibleEffective;

    internal bool IsArrowVisibleEffective
    {
        get => _isArrowVisibleEffective;
        private set => SetAndRaise(IsArrowVisibleEffectiveProperty, ref _isArrowVisibleEffective, value);
    }

    private bool _isPopupHorizontalFlipped;

    internal bool IsPopupHorizontalFlipped
    {
        get => _isPopupHorizontalFlipped;
        private set => SetAndRaise(IsPopupHorizontalFlippedProperty, ref _isPopupHorizontalFlipped, value);
    }

    private bool _isPopupVerticalFlipped;

    internal bool IsPopupVerticalFlipped
    {
        get => _isPopupVerticalFlipped;
        private set => SetAndRaise(IsPopupVerticalFlippedProperty, ref _isPopupVerticalFlipped, value);
    }

    internal ArrowPosition ArrowPosition
    {
        get => GetValue(ArrowPositionProperty);
        set => SetValue(ArrowPositionProperty, value);
    }

    private bool _isLightDismissEnabled;

    internal bool IsLightDismissEnabled
    {
        get => _isLightDismissEnabled;
        private set => SetAndRaise(IsLightDismissEnabledProperty, ref _isLightDismissEnabled, value);
    }

    internal bool IsConfirmationOperationActive => _confirmationOperation != ConfirmationOperation.None;

    #endregion

    #region 运行时字段

    private TopLevel? _deactivationTopLevel;

    private IDisposable? _deactivationSubscription;

    private Popup? _popup;

    private CompositeDisposable? _triggerSubscriptions;

    private DispatcherTimer? _mouseEnterDelayTimer;

    private DispatcherTimer? _mouseLeaveDelayTimer;

    private IDisposable? _popupPointerSubscription;

    private TopLevel? _registeredTopLevel;

    private bool _isPickerShowing;

    private int _popupLifecycleCloseDepth;

    // IsAttachedToVisualTree() still reports true inside OnDetachedFromVisualTree, so track attachment explicitly.
    private bool _isAttachedToVisualTree;

    private ConfirmationOperation _confirmationOperation;

    private EventHandler? _formValueChanged;

    #endregion

    #region 构造函数

    static AbstractColorPicker()
    {
        AffectsMeasure<AbstractColorPicker>(IsTextVisibleProperty,
            FormatProperty,
            ColorBlockBackgroundProperty,
            ColorBlockSizeProperty,
            TriggerPaddingProperty);
        IsPickerOpenProperty.Changed.AddClassHandler<AbstractColorPicker>((picker, args) => picker.HandleIsPickerOpenChanged(args));
        TriggerTypeProperty.Changed.AddClassHandler<AbstractColorPicker>((picker, _) => picker.SetupTriggerHandler());
        InputElement.PointerPressedEvent.AddClassHandler<AbstractColorPicker>(
            (picker, args) => picker.HandleClickModePointerPressed(args),
            RoutingStrategies.Tunnel,
            handledEventsToo: true);
    }

    public AbstractColorPicker()
    {
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Sets the brush that is painted behind the color block of the trigger. Nothing happens when the same brush
    /// instance is already set, so no property change is raised.
    /// </summary>
    /// <param name="brush">
    /// The new brush, or <see langword="null" /> for none.
    /// </param>
    protected void SetColorBlockBackgroundBrushIfChanged(IBrush? brush)
    {
        if (ReferenceEquals(ColorBlockBackground, brush))
        {
            return;
        }

        SetCurrentValue(ColorBlockBackgroundProperty, brush);
    }

    /// <summary>
    /// Sets the background of the color block of the trigger to an opaque solid brush of the given color.
    /// Nothing happens when an opaque solid brush of the same color is already set.
    /// </summary>
    /// <param name="color">
    /// The color to show.
    /// </param>
    protected void SetColorBlockBackgroundColorIfChanged(Color color)
    {
        if (ColorBlockBackground is ISolidColorBrush { Opacity: 1.0 } solidColorBrush &&
            solidColorBrush.Color == color)
        {
            return;
        }

        SetCurrentValue(ColorBlockBackgroundProperty, new SolidColorBrush(color));
    }

    /// <summary>
    /// Regenerates the text that the trigger shows for the current value of the picker, in the picker's
    /// <see cref="Format" />. It is called when <see cref="IsTextVisible" /> or <see cref="Format" /> changes;
    /// a derived picker also calls it when its value changes.
    /// </summary>
    protected abstract void GenerateValueText();

    /// <summary>
    /// Rebuilds the background of the color block of the trigger from the current value of the picker, through
    /// <see cref="SetColorBlockBackgroundColorIfChanged" /> or
    /// <see cref="SetColorBlockBackgroundBrushIfChanged" />. The base class does not call it; a derived picker
    /// calls it when its value changes.
    /// </summary>
    protected abstract void GenerateColorBlockBackground();

    /// <summary>
    /// Keeps the derived state of the picker in step with its properties: it regenerates the value text,
    /// recalculates the effective corner radius, the arrow, the color block size and the popup trigger settings,
    /// resets the confirmation draft when <see cref="IsNeedConfirm" /> changes while the popup is open, and opens
    /// or closes the popup when <see cref="IsPopupPinnedOpen" /> changes. Derived overrides must call the base
    /// method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsTextVisibleProperty || change.Property == FormatProperty)
        {
            GenerateValueText();
        }
        else if (change.Property == CornerRadiusProperty ||
                 change.Property == CompactSpaceItemPositionProperty ||
                 change.Property == CompactSpaceOrientationProperty)
        {
            ConfigureCornerRadius();
        }

        if (change.Property == IsArrowVisibleProperty ||
            change.Property == PlacementProperty ||
            change.Property == PlacementAnchorProperty ||
            change.Property == PlacementGravityProperty)
        {
            ConfigureShowArrowEffective();
        }

        if (change.Property == PlacementProperty ||
            change.Property == PlacementAnchorProperty ||
            change.Property == PlacementGravityProperty ||
            change.Property == IsPopupHorizontalFlippedProperty ||
            change.Property == IsPopupVerticalFlippedProperty)
        {
            ConfigureArrowPosition();
        }

        if (change.Property == SizeTypeProperty ||
            change.Property == HeightProperty ||
            change.Property == TriggerPaddingProperty ||
            change.Property == BorderThicknessProperty)
        {
            ConfigureColorBlockSize();
        }

        if (change.Property == IsNeedConfirmProperty && IsPickerOpen && !IsConfirmationOperationActive)
        {
            ResetConfirmationDraft();
        }

        if (change.Property == IsPopupPinnedOpenProperty)
        {
            ApplyPopupTriggerSettings();

            if (change.GetNewValue<bool>())
            {
                SetCurrentValue(IsPickerOpenProperty, true);
            }
            else if (IsPickerOpen)
            {
                if (_popup is { IsOpen: true })
                {
                    SetCurrentValue(IsPickerOpenProperty, true);
                }
                else
                {
                    ClosePickerForLifecycle();
                }
            }
        }
    }

    /// <summary>
    /// Locates the <c>PART_Popup</c> template part and wires it up: its open and close events, its flip state as
    /// an input of the arrow position, and the trigger settings. It then recalculates the arrow and the color
    /// block size and opens the popup when the picker is already open. A presenter that is still editing in
    /// confirm mode is taken out of editing before the template is applied. Derived overrides must call the base
    /// method.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        if (IsNeedConfirm && PickerPresenter is AbstractColorPickerView view)
        {
            view.EndEdit();
        }
        base.OnApplyTemplate(e);
        DetachPopupHandlers();
        _popup = e.NameScope.Find<Popup>("PART_Popup");
        if (_popup != null)
        {
            if (!IsPopupPinnedOpen)
            {
                _popup.OverlayInputPassThroughElement = this;
            }
            AttachPopupHandlers();

            // Popup's computed flip properties are read-only binding sources.
            // TemplateBinding cannot target them or express OneWayToSource.
            this[!IsPopupHorizontalFlippedProperty] = _popup[!Popup.IsHorizontalFlippedProperty];
            this[!IsPopupVerticalFlippedProperty] = _popup[!Popup.IsVerticalFlippedProperty];


            ApplyPopupTriggerSettings();
        }
        SetupPopupProperties();
        ConfigureShowArrowEffective();
        ConfigureArrowPosition();
        ConfigureColorBlockSize();
        // The business state can become open before the template has produced PART_Popup.
        // Defer the physical open until placement, pinning and dismiss behavior are ready.
        if (IsPickerOpen && _popup != null && !_popup.IsOpen)
        {
            _popup.IsOpen = true;
        }
    }

    /// <summary>
    /// Sets up the trigger handlers and the popup events when the picker is attached, and subscribes to the
    /// window deactivation while the picker is open. A picker that is pinned open is opened when it is not open
    /// yet. Derived overrides must call the base method.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        SetupTriggerHandler();
        AttachPopupHandlers();
        if (IsPickerOpen)
        {
            RegisterWindowDeactivatedHandler();
        }

        if (IsPopupPinnedOpen && !IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, true);
        }
    }

    /// <summary>
    /// Closes the popup and releases the trigger subscriptions, the hover delay timers, the pointer handlers and
    /// the window deactivation subscription when the picker is detached. Derived overrides must call the base
    /// method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        ClosePickerForLifecycle();
        base.OnDetachedFromVisualTree(e);
        UnregisterWindowDeactivatedHandler();

        StopMouseEnterTimer();
        StopMouseLeaveTimer();
        UnregisterRootPointerHandler();
        UnsubscribeFromPopupPointer();
        DetachPopupHandlers();
        _triggerSubscriptions?.Dispose();
        _triggerSubscriptions = null;
    }

    /// <summary>
    /// Called at the end of <see cref="OnApplyTemplate" />, as a hook for a derived picker to configure the
    /// popup. The base method does nothing; the default open and close motions come from the theme, not from
    /// local values.
    /// </summary>
    protected virtual void SetupPopupProperties()
    {
        // Retained as the existing derived-picker template hook. Default motion
        // relationships are declared by AbstractColorPickerTheme, not local values.
    }

    /// <summary>
    /// Closes the popup by clearing the open state of the picker. In confirm mode an edit that is still in
    /// progress is ended, so its draft is not written to the value of the picker.
    /// </summary>
    public void ClosePicker()
    {
        SetCurrentValue(IsPickerOpenProperty, false);
    }

    /// <summary>
    /// Called after the open state of the picker changes to open and before the popup is shown. It creates the
    /// presenter through <see cref="CreatePresenter" /> when there is none yet, forwards the confirm mode to it,
    /// attaches the confirmation handlers and subscribes to the window deactivation. With the focus trigger it
    /// also subscribes the root pointer handler that closes the popup on a press outside it. Derived overrides
    /// must call the base method.
    /// </summary>
    protected virtual void NotifyPickerOpened()
    {
        RegisterWindowDeactivatedHandler();

        if (PickerPresenter is null)
        {
            PickerPresenter = CreatePresenter();
            // The presenter is created at runtime in a Content subtree; the owner's
            // ControlTheme cannot bind through this independent template owner.
            if (PickerPresenter is AbstractColorPickerView view)
            {
                view[!AbstractColorPickerView.IsNeedConfirmProperty] = this[!IsNeedConfirmProperty];
            }
            NotifyPickerPresenterCreated(PickerPresenter);
        }

        AttachConfirmationHandlers();
        if (TriggerType == FlyoutTriggerType.Focus)
        {
            RegisterRootPointerHandler();
            Dispatcher.Post(() => _isPickerShowing = false);
        }
        else
        {
            _isPickerShowing = false;
        }
    }

    /// <summary>
    /// Called after the open state of the picker changes to closed and the popup has been told to close. It
    /// releases the confirmation handlers, the window deactivation subscription, the root pointer handler and
    /// the popup pointer subscription. Derived overrides must call the base method.
    /// </summary>
    protected virtual void NotifyPickerClosed()
    {
        ReleaseConfirmationHandlers();
        UnregisterWindowDeactivatedHandler();
        UnregisterRootPointerHandler();
        UnsubscribeFromPopupPointer();
    }

    /// <summary>
    /// Called from <see cref="NotifyPickerOpened" /> right after the presenter has been created by
    /// <see cref="CreatePresenter" /> and before the confirmation handlers are attached. The base method does
    /// nothing.
    /// </summary>
    /// <param name="pickerPresenter">
    /// The presenter that was just created.
    /// </param>
    protected virtual void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
    }

    /// <summary>
    /// Creates the control that the popup shows, such as the color view of the picker. It is called by
    /// <see cref="NotifyPickerOpened" /> the first time the popup opens. The result is kept as the presenter
    /// and reused, so the method is not called again.
    /// </summary>
    /// <returns>
    /// The new presenter.
    /// </returns>
    protected abstract Control CreatePresenter();

    /// <summary>
    /// Updates the pseudo-classes of the picker from its current state: the <c>flyout-open</c> pseudo-class
    /// follows the open state. It is called after the open state changes. Derived overrides must call the base
    /// method.
    /// </summary>
    protected virtual void UpdatePseudoClasses()
    {
        PseudoClasses.Set(ColorPickerPseudoClass.FlyoutOpen, IsPickerOpen);
    }

    /// <summary>
    /// Formats a color as text in a color format of the picker. <see cref="ColorFormat.Hex" /> gives a hash
    /// followed by the six hexadecimal digits of the RGB channels, without the alpha channel.
    /// <see cref="ColorFormat.Rgba" /> gives <c>rgba(r, g, b, a)</c> with channels from 0 to 255 and an alpha
    /// from 0.00 to 1.00. <see cref="ColorFormat.Hsva" /> gives <c>hsva(h, s%, v%, a)</c> with the hue in
    /// degrees, saturation and value in whole percent, and an alpha from 0.00 to 1.00.
    /// </summary>
    /// <param name="color">
    /// The color to format.
    /// </param>
    /// <param name="format">
    /// The color format to use.
    /// </param>
    /// <returns>
    /// The formatted text.
    /// </returns>
    public static string FormatColor(Color color, ColorFormat format)
    {
        if (format == ColorFormat.Hex)
        {
            return ColorToHexConverter.ToHexString(color, AlphaComponentPosition.Leading, false, true);
        }
        if (format == ColorFormat.Rgba)
        {
            return $"rgba({(int)color.R}, {(int)color.G}, {(int)color.B}, {color.GetAlphaF():0.00})";
        }

        var hsvColor = color.ToHsv();
        return $"hsva({hsvColor.H:0}, {hsvColor.S * 100:0}%, {hsvColor.V * 100:0}%,  {hsvColor.A:0.00})";
    }

    #endregion

    #region 内部协作 API

    internal void ShowPicker(bool immediately = false)
    {
        if (!IsEnabled || !IsVisible)
        {
            return;
        }

        StopMouseLeaveTimer();

        if (immediately || MouseEnterDelay == 0)
        {
            ShowPickerCore();
        }
        else
        {
            StartMouseEnterTimer();
        }
    }

    internal void HidePicker(bool immediately = false)
    {
        StopMouseEnterTimer();

        if (immediately || MouseLeaveDelay == 0)
        {
            HidePickerCore();
        }
        else
        {
            StartMouseLeaveTimer();
        }
    }

    internal void NotifyFormValueChanged()
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    internal virtual void ResetConfirmationDraft()
    {
    }

    internal virtual void CommitConfirmationDraft()
    {
    }

    #endregion

    #region 实现 ICompactSpaceAware 接口

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace = position != null;
        CompactSpaceItemPosition = position;
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }

    double ICompactSpaceAware.GetBorderThickness()
    {
        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }

    #endregion

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();

    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();

    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    /// <summary>
    /// Called when a form item sets the value of the picker through <see cref="IFormItemAware.SetFormValue" />.
    /// The base method does nothing; a picker that has a value overrides it and applies the value.
    /// </summary>
    /// <param name="value">
    /// The value handed over by the form item.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
    }

    /// <summary>
    /// Called when a form item reads the value of the picker through <see cref="IFormItemAware.GetFormValue" />.
    /// The base method returns <see langword="null" />; a picker that has a value overrides it and returns that
    /// value.
    /// </summary>
    /// <returns>
    /// The value for the form item, or <see langword="null" /> when the picker has none.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    /// <summary>
    /// Called when a form item clears the picker through <see cref="IFormItemAware.ClearFormValue" />. The base
    /// method does nothing; a picker that has a value overrides it and resets the value.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
    }

    /// <summary>
    /// Called when a form item reports its validation status through
    /// <see cref="IFormItemAware.NotifyValidateStatus" />. <see cref="FormValidateStatus.Error" /> and
    /// <see cref="FormValidateStatus.Warning" /> are mapped to the matching <see cref="Status" />; every other
    /// status sets <see cref="InputControlStatus.Default" />.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form item.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (status == FormValidateStatus.Error)
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Error);
        }
        else if (status == FormValidateStatus.Warning)
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Warning);
        }
        else
        {
            SetCurrentValue(StatusProperty, InputControlStatus.Default);
        }
    }

    #endregion

    #region 内部实现

    private void SetupTriggerHandler()
    {
        _triggerSubscriptions?.Dispose();
        StopMouseEnterTimer();
        StopMouseLeaveTimer();
        UnregisterRootPointerHandler();
        UnsubscribeFromPopupPointer();

        IsLightDismissEnabled = TriggerType == FlyoutTriggerType.Click;
        ApplyPopupTriggerSettings();

        _triggerSubscriptions = null;

        switch (TriggerType)
        {
            case FlyoutTriggerType.Hover:
                _triggerSubscriptions = new CompositeDisposable();
                SetupHoverTrigger();
                if (IsPickerOpen)
                {
                    SubscribeToPopupPointer();
                }
                break;
            case FlyoutTriggerType.Click:
                break;
            case FlyoutTriggerType.Focus:
                _triggerSubscriptions = new CompositeDisposable();
                SetupFocusTrigger();
                if (IsPickerOpen)
                {
                    RegisterRootPointerHandler();
                }
                break;
        }
    }

    private void ApplyPopupTriggerSettings()
    {
        if (_popup == null)
        {
            return;
        }
        _popup.OverlayDismissEventPassThrough = TriggerType != FlyoutTriggerType.Click;
        _popup.SetCurrentValue(
            Popup.IsLightDismissEnabledProperty,
            IsLightDismissEnabled);
    }

    private void SetupHoverTrigger()
    {
        _triggerSubscriptions!.Add(this.GetObservable(IsPointerOverProperty).Subscribe(isPointerOver =>
        {
            if (IsEnabled && IsVisible)
            {
                HandleAnchorHover(isPointerOver);
            }
        }));
    }

    private void HandleAnchorHover(bool isPointerOver)
    {
        if (isPointerOver)
        {
            ShowPicker();
        }
        else
        {
            StopMouseEnterTimer();
            if (IsPickerOpen && _popup?.Child is InputElement popupChild && popupChild.IsPointerOver)
            {
                return;
            }
            StartMouseLeaveTimer();
        }
    }

    private void SubscribeToPopupPointer()
    {
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;

        // IInputManager.Process is app-wide; subscribing while detached (e.g. TriggerType changed with
        // IsPickerOpen already true) would never be released and would root this picker.
        if (!_isAttachedToVisualTree)
        {
            return;
        }

        var inputManager = AvaloniaLocator.Current.GetService<IInputManager>();
        if (inputManager != null)
        {
            _popupPointerSubscription = inputManager.Process.Subscribe(HandleGlobalPointerMove);
        }
    }

    private void UnsubscribeFromPopupPointer()
    {
        _popupPointerSubscription?.Dispose();
        _popupPointerSubscription = null;
    }

    private void HandleGlobalPointerMove(RawInputEventArgs e)
    {
        if (e is not RawPointerEventArgs pe || pe.Type != RawPointerEventType.Move)
        {
            return;
        }

        var hitElement = pe.GetInputHitTestResult().element;
        if (hitElement is not Visual visual)
        {
            return;
        }

        if (IsVisualInPickerScope(visual))
        {
            StopMouseLeaveTimer();
        }
        else if (!IsPointerOver)
        {
            if (_mouseLeaveDelayTimer == null && IsPickerOpen)
            {
                StartMouseLeaveTimer();
            }
        }
    }

    private void HandleClickModePointerPressed(PointerPressedEventArgs e)
    {
        if (TriggerType != FlyoutTriggerType.Click)
        {
            return;
        }

        if (!IsEnabled || !IsVisible)
        {
            return;
        }

        if (e.Source is Visual source && IsSourceInsidePickerPopup(source))
        {
            return;
        }

        if (IsPickerOpen)
        {
            HidePicker(immediately: true);
        }
        else
        {
            ShowPicker(immediately: true);
        }
    }

    private bool IsSourceInsidePickerPopup(Visual visual)
    {
        if (visual == this || this.IsVisualAncestorOf(visual))
        {
            return false;
        }
        return IsVisualInPickerScope(visual);
    }

    private void SetupFocusTrigger()
    {
        GotFocus += HandleFocusModeGotFocus;
        LostFocus += HandleFocusModeLostFocus;
        AddHandler(InputElement.PointerPressedEvent, HandleFocusModeAnchorPointerPressed,
            handledEventsToo: true);
        _triggerSubscriptions!.Add(Disposable.Create(() =>
        {
            GotFocus -= HandleFocusModeGotFocus;
            LostFocus -= HandleFocusModeLostFocus;
            RemoveHandler(InputElement.PointerPressedEvent, HandleFocusModeAnchorPointerPressed);
            UnregisterRootPointerHandler();
        }));
    }

    private void HandleFocusModeGotFocus(object? sender, FocusChangedEventArgs e)
    {
        TryShowIfClosed();
    }

    private void HandleFocusModeAnchorPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        TryShowIfClosed();
    }

    private void TryShowIfClosed()
    {
        if (IsEnabled && IsVisible && !IsPickerOpen)
        {
            ShowPicker(immediately: true);
        }
    }

    private void HandleFocusModeLostFocus(object? sender, FocusChangedEventArgs e)
    {
        if (!IsPickerOpen || _isPickerShowing)
        {
            return;
        }

        Dispatcher.Post(() =>
        {
            if (IsPickerOpen && !_isPickerShowing && !IsFocusWithinPickerScope())
            {
                HidePicker(immediately: true);
            }
        });
    }

    private void RegisterRootPointerHandler()
    {
        UnregisterRootPointerHandler();
        _registeredTopLevel = TopLevel.GetTopLevel(this);
        _registeredTopLevel?.AddHandler(PointerPressedEvent, HandleFocusModeRootPointerPressed,
            RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    private void UnregisterRootPointerHandler()
    {
        if (_registeredTopLevel != null)
        {
            _registeredTopLevel.RemoveHandler(PointerPressedEvent, HandleFocusModeRootPointerPressed);
            _registeredTopLevel = null;
        }
    }

    private void HandleFocusModeRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsPickerOpen)
        {
            return;
        }

        if (e.Source is Visual source && !IsVisualInPickerScope(source))
        {
            HidePicker(immediately: true);
        }
    }

    private void StartMouseEnterTimer()
    {
        StopMouseEnterTimer();
        _mouseEnterDelayTimer = new DispatcherTimer
        { Interval = TimeSpan.FromMilliseconds(MouseEnterDelay) };
        _mouseEnterDelayTimer.Tick += HandleMouseEnterTimerTick;
        _mouseEnterDelayTimer.Start();
    }

    private void HandleMouseEnterTimerTick(object? sender, EventArgs e)
    {
        StopMouseEnterTimer();
        ShowPickerCore();
    }

    private void StopMouseEnterTimer()
    {
        if (_mouseEnterDelayTimer != null)
        {
            _mouseEnterDelayTimer.Stop();
            _mouseEnterDelayTimer.Tick -= HandleMouseEnterTimerTick;
            _mouseEnterDelayTimer = null;
        }
    }

    private void StartMouseLeaveTimer()
    {
        StopMouseLeaveTimer();
        _mouseLeaveDelayTimer = new DispatcherTimer
        { Interval = TimeSpan.FromMilliseconds(MouseLeaveDelay) };
        _mouseLeaveDelayTimer.Tick += HandleMouseLeaveTimerTick;
        _mouseLeaveDelayTimer.Start();
    }

    private void HandleMouseLeaveTimerTick(object? sender, EventArgs e)
    {
        StopMouseLeaveTimer();
        HidePickerCore();
    }

    private void StopMouseLeaveTimer()
    {
        if (_mouseLeaveDelayTimer != null)
        {
            _mouseLeaveDelayTimer.Stop();
            _mouseLeaveDelayTimer.Tick -= HandleMouseLeaveTimerTick;
            _mouseLeaveDelayTimer = null;
        }
    }

    private bool IsVisualInPickerScope(Visual visual)
        => PopupUtils.IsVisualInPopupScope(visual, this, _popup?.Child as Visual);

    private bool IsFocusWithinPickerScope()
    {
        var topLevel = TopLevel.GetTopLevel(this);
        return topLevel?.FocusManager.GetFocusedElement() is Visual focused
               && IsVisualInPickerScope(focused);
    }

    private void ShowPickerCore()
    {
        _isPickerShowing = true;
        StopMouseEnterTimer();
        StopMouseLeaveTimer();

        if (IsPickerOpen && _popup is { IsPlayingCloseMotion: true })
        {
            _popup.CancelCloseAnimation();
            _isPickerShowing = false;
            return;
        }

        SetCurrentValue(IsPickerOpenProperty, true);
    }

    private void HidePickerCore()
    {
        StopMouseLeaveTimer();
        SetCurrentValue(IsPickerOpenProperty, false);
    }

    private void ConfigureShowArrowEffective()
    {
        if (!IsArrowVisible)
        {
            IsArrowVisibleEffective = false;
        }
        else
        {
            IsArrowVisibleEffective = PopupUtils.CanEnabledArrow(Placement, PlacementAnchor, PlacementGravity);
        }
    }

    private void ConfigureArrowPosition()
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(Placement, PlacementAnchor, PlacementGravity);
        if (arrowPosition.HasValue)
        {
            arrowPosition = ArrowPositionUtils.FlipArrowPosition(
                arrowPosition.Value, IsPopupHorizontalFlipped, IsPopupVerticalFlipped);
            SetCurrentValue(ArrowPositionProperty, arrowPosition);
        }
    }

    private void ConfigureColorBlockSize()
    {
        if (SizeType != CustomizableSizeType.Custom || double.IsNaN(Height))
        {
            ColorBlockSize = double.NaN;
            return;
        }

        var colorBlockSize = Height -
                             TriggerPadding.Top -
                             TriggerPadding.Bottom -
                             BorderThickness.Top -
                             BorderThickness.Bottom;
        ColorBlockSize = Math.Max(0, colorBlockSize);
    }

    private void DetachPopupHandlers()
    {
        if (_popup != null)
        {
            _popup.Opened -= HandlePopupOpened;
            _popup.Closed -= HandlePopupClosed;
        }
    }

    private void AttachPopupHandlers()
    {
        if (_popup == null)
        {
            return;
        }

        _popup.Opened -= HandlePopupOpened;
        _popup.Closed -= HandlePopupClosed;
        _popup.Opened += HandlePopupOpened;
        _popup.Closed += HandlePopupClosed;
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        if (IsNeedConfirm && PickerPresenter is AbstractColorPickerView { IsEditing: false })
        {
            ResetConfirmationDraft();
        }
        AttachConfirmationHandlers();
        if (TriggerType == FlyoutTriggerType.Hover)
        {
            SubscribeToPopupPointer();
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        ReleaseConfirmationHandlers();
        if (IsNeedConfirm && PickerPresenter is AbstractColorPickerView view)
        {
            view.EndEdit();
        }
        UnsubscribeFromPopupPointer();
        // Keep the business state synchronized with light-dismiss and Escape closes.
        if (IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, false);
        }
    }

    private void ClosePickerForLifecycle()
    {
        try
        {
            ++_popupLifecycleCloseDepth;
            _popup?.CloseForLifecycle();
            SetCurrentValue(IsPickerOpenProperty, false);
        }
        finally
        {
            --_popupLifecycleCloseDepth;
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetCurrentValue(IsPickerOpenProperty, false);
    }

    private void RegisterWindowDeactivatedHandler()
    {
        RegisterWindowDeactivatedHandler(TopLevel.GetTopLevel(this));
    }

    private void RegisterWindowDeactivatedHandler(TopLevel? topLevel)
    {
        if (ReferenceEquals(_deactivationTopLevel, topLevel))
        {
            return;
        }

        UnregisterWindowDeactivatedHandler();
        if (topLevel is null)
        {
            return;
        }

        _deactivationTopLevel = topLevel;
        _deactivationSubscription = TopLevelDeactivation.Subscribe(topLevel, HandleWindowDeactivated);
    }

    private void UnregisterWindowDeactivatedHandler()
    {
        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;
        _deactivationTopLevel = null;
    }

    private void AttachConfirmationHandlers()
    {
        ReleaseConfirmationHandlers();
        if (PickerPresenter is AbstractColorPickerView view)
        {
            view.ConfirmRequested += HandleConfirmationRequested;
            view.CancelRequested += HandleCancellationRequested;
        }
    }

    private void ReleaseConfirmationHandlers()
    {
        if (PickerPresenter is AbstractColorPickerView view)
        {
            view.ConfirmRequested -= HandleConfirmationRequested;
            view.CancelRequested -= HandleCancellationRequested;
        }
    }

    private enum ConfirmationOperation
    {
        None,
        Confirming,
        Cancelling
    }

    private void HandleConfirmationRequested(object? sender, EventArgs args)
    {
        if (!IsNeedConfirm || !IsPickerOpen || IsConfirmationOperationActive)
        {
            return;
        }
        _confirmationOperation = ConfirmationOperation.Confirming;
        try
        {
            CommitConfirmationDraft();
            CompleteConfirmationOperation();
        }
        finally
        {
            _confirmationOperation = ConfirmationOperation.None;
        }
    }

    private void HandleCancellationRequested(object? sender, EventArgs args)
    {
        if (!IsNeedConfirm || !IsPickerOpen || IsConfirmationOperationActive)
        {
            return;
        }
        _confirmationOperation = ConfirmationOperation.Cancelling;
        try
        {
            CompleteConfirmationOperation();
        }
        finally
        {
            _confirmationOperation = ConfirmationOperation.None;
        }
    }

    private void CompleteConfirmationOperation()
    {
        if (IsPopupPinnedOpen)
        {
            ResetConfirmationDraft();
        }
        else
        {
            ClosePicker();
        }
    }

    private void HandleIsPickerOpenChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.NewValue is true)
        {
            NotifyPickerOpened();
            if (_popup != null && !_popup.IsOpen)
            {
                _popup.IsOpen = true;
            }
        }
        else
        {
            if (_popup is { IsOpen: true })
            {
                _popup.IsOpen = false;
            }
            NotifyPickerClosed();
        }

        UpdatePseudoClasses();
    }

    private static bool CoerceIsPickerOpen(AvaloniaObject sender, bool value)
    {
        return value ||
               sender is AbstractColorPicker { IsPopupPinnedOpen: true, _popupLifecycleCloseDepth: 0 };
    }

    private void ConfigureCornerRadius()
    {
        EffectiveCornerRadius = CompactSpace.CalculateEffectiveCornerRadius(
            CornerRadius,
            IsUsedInCompactSpace,
            CompactSpaceItemPosition,
            CompactSpaceOrientation);
    }

    #endregion
}
