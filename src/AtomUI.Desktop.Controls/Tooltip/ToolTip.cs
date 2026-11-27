using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using AtomUI.Theme.Derivation;
using AtomUI.Theme.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A tool tip: a content control with an arrow, shown by a popup next to its host. A host sets its
/// tooltip through the attached properties of this class, and the tooltip service or the app opens it
/// through <see cref="IsOpenProperty" />. Plain tip content gets a tooltip created for it; a
/// <see cref="ToolTip" /> instance set as the tip is used as it is and also supplies the popup with
/// the mask and arrow geometry that the shadow follows.
/// </summary>
[PseudoClasses(":open")]
public partial class ToolTip : ContentControl,
                               IMotionAwareControl,
                               IArrowAwareShadowMaskInfoProvider
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<ToolTip>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<ToolTip>();

    /// <summary>
    /// Identifies the <see cref="ToolTipShadows" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> ToolTipShadowsProperty =
        AvaloniaProperty.Register<ToolTip, BoxShadows>(nameof(ToolTipShadows));

    /// <summary>
    /// Identifies the Tip attached property: the tool tip of the host element. The value is plain
    /// content, shown by a tooltip that is created for it, or a <see cref="ToolTip" /> instance, which is
    /// shown as it is. The tooltip cannot open while the value is null.
    /// </summary>
    public static readonly AttachedProperty<object?> TipProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, object?>("Tip");

    /// <summary>
    /// Identifies the TipHostWidth attached property: the width of the tooltip's content area, in
    /// device-independent pixels. NaN lets the content choose its width. The value is applied to the
    /// content presenter when the tooltip opens; the Slider sets it on its thumbs.
    /// </summary>
    public static readonly AttachedProperty<double> TipHostWidthProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("TipHostWidth", double.NaN);

    /// <summary>
    /// Identifies the PresetColor attached property: the preset color of the tooltip background,
    /// resolved from the theme's palette. It takes precedence over <see cref="ColorProperty" />. The value
    /// is read from the host, or from the <see cref="ToolTip" /> instance used as the tip when that sets it.
    /// </summary>
    public static readonly AttachedProperty<PresetColorType?> PresetColorProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, PresetColorType?>("PresetColor");

    /// <summary>
    /// Identifies the Color attached property: the custom background color of the tooltip. A preset set
    /// through <see cref="PresetColorProperty" /> takes precedence. The value is read from the host, or
    /// from the <see cref="ToolTip" /> instance used as the tip when that sets it.
    /// </summary>
    public static readonly AttachedProperty<Color?> ColorProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, Color?>("Color");

    /// <summary>
    /// Identifies the IsUseOverlayHost attached property: when true, the tooltip is shown in the overlay
    /// layer of the host's window instead of a popup of its own. Only the host's value is used, and it is
    /// relayed to the popup each time the tooltip opens.
    /// </summary>
    public static readonly AttachedProperty<bool> IsUseOverlayHostProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsUseOverlayHost");

    /// <summary>
    /// Identifies the IsArrowVisible attached property: whether the tooltip draws the arrow that points
    /// at the host; true is the default. The value has no effect for
    /// <see cref="PlacementMode.Center" /> and <see cref="PlacementMode.Pointer" />, which have no arrow.
    /// The value is read from the host, or from the <see cref="ToolTip" /> instance used as the tip when
    /// that sets it.
    /// </summary>
    public static readonly AttachedProperty<bool> IsArrowVisibleProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsArrowVisible", true);

    /// <summary>
    /// Identifies the IsPointAtCenter attached property: when true, the tooltip points at the center of
    /// the host instead of its edge. The value is read from the host, or from the <see cref="ToolTip" />
    /// instance used as the tip when that sets it.
    /// </summary>
    public static readonly AttachedProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsPointAtCenter");

    /// <summary>
    /// Identifies the IsOpen attached property: the requested open state of the tooltip of the host.
    /// true opens the tooltip once the host has a tip and is attached to the visual tree; false closes
    /// it. The tooltip service writes the value as the pointer moves, and it is written back to false
    /// when the popup closes for a reason other than the host being detached. A false value is ignored
    /// while the popup is pinned open by framework code.
    /// </summary>
    public static readonly AttachedProperty<bool> IsOpenProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsOpen", coerce: CoerceIsOpen);

    /// <summary>
    /// Identifies the Placement attached property: where the tooltip popup is placed relative to the host;
    /// the default is Top. <see cref="PlacementMode.Center" /> and <see cref="PlacementMode.Pointer" /> cannot
    /// show an arrow. The value is read from the host, or from the <see cref="ToolTip" /> instance used as
    /// the tip when that sets it.
    /// </summary>
    public static readonly AttachedProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, PlacementMode>("Placement",
            defaultValue: PlacementMode.Top);

    /// <summary>
    /// Identifies the HorizontalOffset attached property: the horizontal offset of the tooltip popup from
    /// its placed position, in device-independent pixels. The value is read from the host, or from the
    /// <see cref="ToolTip" /> instance used as the tip when that sets it.
    /// </summary>
    public static readonly AttachedProperty<double> HorizontalOffsetProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("HorizontalOffset");

    /// <summary>
    /// Identifies the VerticalOffset attached property: the vertical offset of the tooltip popup from its
    /// placed position, in device-independent pixels. The value is read from the host, or from the
    /// <see cref="ToolTip" /> instance used as the tip when that sets it.
    /// </summary>
    public static readonly AttachedProperty<double> VerticalOffsetProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("VerticalOffset");

    /// <summary>
    /// Identifies the ShowDelay attached property: the time in milliseconds that the pointer rests on the
    /// host before the tooltip service opens its tooltip; 0 opens it at once.
    /// </summary>
    public static readonly AttachedProperty<int> ShowDelayProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, int>("ShowDelay", 400);

    /// <summary>
    /// Identifies the BetweenShowDelay attached property: the time window in milliseconds after a tooltip
    /// closes within which the tooltip of the next host opens without waiting for
    /// <see cref="ShowDelayProperty" />. A negative value disables the window.
    /// </summary>
    public static readonly AttachedProperty<int> BetweenShowDelayProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, int>("BetweenShowDelay", 100);

    /// <summary>
    /// Identifies the ShowOnDisabled attached property: when true, a disabled element still gets its
    /// tooltip from the tooltip service. The value is inherited, so it can be set on a container for all
    /// of its descendants.
    /// </summary>
    public static readonly AttachedProperty<bool> ShowOnDisabledProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("ShowOnDisabled", defaultValue: false,
            inherits: true);

    /// <summary>
    /// Identifies the ServiceEnabled attached property: when false, the tooltip service does not show
    /// tooltips for the element. The value is inherited, so setting it on a container disables the
    /// tooltips of the descendants that do not set their own.
    /// </summary>
    public static readonly AttachedProperty<bool> ServiceEnabledProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("ServiceEnabled", defaultValue: true, inherits: true);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<ToolTip>();

    /// <summary>
    /// Identifies the MarginToAnchor attached property: the gap between the tooltip and its host, in
    /// device-independent pixels. An explicit value on the <see cref="ToolTip" /> instance used as the tip
    /// wins over the host's value; when neither sets it, the theme's MarginToAnchor Token applies.
    /// </summary>
    public static readonly AttachedProperty<double> MarginToAnchorProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, double>("MarginToAnchor", 4);

    /// <summary>
    /// Identifies the IsCustomShowAndHide attached property: when true, the tooltip service neither shows
    /// nor hides the tooltip of the host. The app opens and closes it through <see cref="IsOpenProperty" />.
    /// </summary>
    public static readonly AttachedProperty<bool> IsCustomShowAndHideProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsCustomShowAndHide");

    /// <summary>
    /// Identifies the TextWrapping attached property: how the tooltip text wraps inside its content
    /// area. The value is read from the host, or from the <see cref="ToolTip" /> instance used as the tip
    /// when that sets it.
    /// </summary>
    public static readonly AttachedProperty<TextWrapping> TextWrappingProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, TextWrapping>("TextWrapping", TextWrapping.Wrap);

    /// <summary>
    /// Identifies the TextTrimming attached property: how the tooltip text is trimmed when it does not
    /// fit. The value is read from the host, or from the <see cref="ToolTip" /> instance used as the tip
    /// when that sets it.
    /// </summary>
    public static readonly AttachedProperty<TextTrimming> TextTrimmingProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, TextTrimming>("TextTrimming", TextTrimming.None);

    /// <summary>
    /// The motion the popup plays when the tooltip opens. The ControlTheme supplies the default, and the
    /// value is relayed to the popup that shows this control.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// The motion the popup plays when the tooltip closes. The ControlTheme supplies the default, and the
    /// value is relayed to the popup that shows this control.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// The shadow of the tooltip popup. The popup is a runtime-created cross-root node outside this control's
    /// ControlTheme, so the theme writes the token here and the code relays it to the popup (a ControlTheme
    /// setter cannot reach the popup's tree). A cleared value leaves the popup without a shadow.
    /// </summary>
    public BoxShadows ToolTipShadows
    {
        get => GetValue(ToolTipShadowsProperty);
        set => SetValue(ToolTipShadowsProperty, value);
    }

    /// <summary>
    /// Whether the tooltip plays its <see cref="OpenMotion" /> and <see cref="CloseMotion" />; false
    /// disables both. The ControlTheme binds the value to the EnableMotion Token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised on the host element before its tooltip opens, after the tip and the attachment of the host
    /// were checked and before the tooltip instance is created or shown. Cancelling the event keeps the
    /// tooltip closed and sets the host's <see cref="IsOpenProperty" /> back to false.
    /// </summary>
    public static readonly RoutedEvent<CancelRoutedEventArgs> ToolTipOpeningEvent =
        RoutedEvent.Register<ToolTip, CancelRoutedEventArgs>("ToolTipOpening", RoutingStrategies.Direct);

    /// <summary>
    /// Raised on the host element before its tooltip closes. That happens whenever the tooltip is closed
    /// while the host's <see cref="IsOpenProperty" /> is still true, as when the tip is cleared, the host
    /// leaves the visual tree or the tooltip is about to reopen; a close that follows IsOpen changing to
    /// false raises no event. The event carries plain <see cref="RoutedEventArgs" />, so the close cannot
    /// be canceled.
    /// </summary>
    public static readonly RoutedEvent ToolTipClosingEvent =
        RoutedEvent.Register<ToolTip, RoutedEventArgs>("ToolTipClosing", RoutingStrategies.Direct);

    #endregion

    #region 内部属性定义


    internal static readonly AttachedProperty<ToolTip?> ToolTipProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, ToolTip?>("ToolTip");

    internal static readonly AttachedProperty<bool> IsPopupPinnedOpenProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, bool>("IsPopupPinnedOpen");

    internal static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<ToolTip>();

    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<ToolTip>();

    /// <summary>
    /// The effective tooltip background: the ControlTheme gives it the ToolTipBackground Token and projects
    /// it onto Background through an untriggered setter, so app styles and local values keep precedence; a
    /// preset or custom color requested for the tooltip is written here.
    /// </summary>
    internal static readonly StyledProperty<IBrush?> ColorBackgroundProperty =
        AvaloniaProperty.Register<ToolTip, IBrush?>(nameof(ColorBackground));

    /// <summary>
    /// The theme's gap to the anchor (the <c>MarginToAnchor</c> token), used while neither the host nor this tooltip
    /// sets <see cref="MarginToAnchorProperty" />.
    /// </summary>
    internal static readonly StyledProperty<double> ThemeMarginToAnchorProperty =
        AvaloniaProperty.Register<ToolTip, double>(nameof(ThemeMarginToAnchor), 4);

    private static readonly AttachedProperty<EventHandler<VisualTreeAttachmentEventArgs>?> PendingAttachHandlerProperty =
        AvaloniaProperty.RegisterAttached<ToolTip, Control, EventHandler<VisualTreeAttachmentEventArgs>?>("PendingAttachHandler");

    internal TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    internal IBrush? ColorBackground
    {
        get => GetValue(ColorBackgroundProperty);
        set => SetValue(ColorBackgroundProperty, value);
    }

    internal double ThemeMarginToAnchor
    {
        get => GetValue(ThemeMarginToAnchorProperty);
        set => SetValue(ThemeMarginToAnchorProperty, value);
    }

    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    internal Control? AdornedControl { get; private set; }

    #endregion

    #region 内部事件定义

    internal event EventHandler? Closed;

    #endregion

    #region 运行时字段

    private Popup? _popup;

    private CompositeDisposable? _subscriptions;

    private ArrowDecoratedBox? _arrowDecoratedBox;

    private ContentPresenter? _contentPresenter;

    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    private IDisposable? _themeSubscription;

    // The preset / custom color requested by the host (or the tip instance) of the current opening.
    private PresetColorType? _requestedPresetColor;

    private Color? _requestedColor;

    #endregion

    #region 构造函数

    static ToolTip()
    {
        IsOpenProperty.Changed.Subscribe(IsOpenChanged);
        TipProperty.Changed.Subscribe(TipChanged);
        IsPopupPinnedOpenProperty.Changed.Subscribe(IsPopupPinnedOpenChanged);
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Reads <see cref="TipProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that shows the tip.
    /// </param>
    /// <returns>
    /// The tip content or the <see cref="ToolTip" /> instance that is shown for the element, or
    /// <see langword="null" /> when it has no tip.
    /// </returns>
    public static object? GetTip(Control element)
    {
        return element.GetValue(TipProperty);
    }

    /// <summary>
    /// Writes <see cref="TipProperty" /> on <paramref name="element" />. Setting a tip while the
    /// tooltip of the element is open replaces what is shown; clearing it closes the tooltip.
    /// </summary>
    /// <param name="element">
    /// The element that shows the tip.
    /// </param>
    /// <param name="value">
    /// The tip content, or a <see cref="ToolTip" /> instance that carries its own presentation settings.
    /// Pass <see langword="null" /> to remove the tip.
    /// </param>
    public static void SetTip(Control element, object? value)
    {
        element.SetValue(TipProperty, value);
    }

    /// <summary>
    /// Reads <see cref="TipHostWidthProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The width of the tooltip's content area in device-independent pixels, or <see cref="double.NaN" />
    /// when it is not set.
    /// </returns>
    public static double GetTipHostWidth(Control element)
    {
        return element.GetValue(TipHostWidthProperty);
    }

    /// <summary>
    /// Writes <see cref="TipHostWidthProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// The width of the tooltip's content area in device-independent pixels; <see cref="double.NaN" />
    /// lets the content choose its width.
    /// </param>
    public static void SetTipHostWidth(Control element, double value)
    {
        element.SetValue(TipHostWidthProperty, value);
    }

    /// <summary>
    /// Reads <see cref="PresetColorProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The preset color of the tooltip background, or <see langword="null" /> when none is set.
    /// </returns>
    public static PresetColorType? GetPresetColor(Control element)
    {
        return element.GetValue(PresetColorProperty);
    }

    /// <summary>
    /// Writes <see cref="PresetColorProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="color">
    /// The preset color of the tooltip background. It takes precedence over <see cref="ColorProperty" />.
    /// </param>
    public static void SetPresetColor(Control element, PresetColorType color)
    {
        element.SetValue(PresetColorProperty, color);
    }

    /// <summary>
    /// Reads <see cref="ColorProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The custom background color of the tooltip, or <see langword="null" /> when none is set.
    /// </returns>
    public static Color? GetColor(Control element)
    {
        return element.GetValue(ColorProperty);
    }

    /// <summary>
    /// Writes <see cref="ColorProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="color">
    /// The custom background color of the tooltip, or <see langword="null" /> to use the preset color
    /// or the theme's background.
    /// </param>
    public static void SetColor(Control element, Color? color)
    {
        element.SetValue(ColorProperty, color);
    }

    /// <summary>
    /// Reads <see cref="IsUseOverlayHostProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Whether the tooltip is shown in the overlay layer of the host's window instead of a popup of its
    /// own; false by default.
    /// </returns>
    public static bool GetIsUseOverlayHost(Control element)
    {
        return element.GetValue(IsUseOverlayHostProperty);
    }

    /// <summary>
    /// Writes <see cref="IsUseOverlayHostProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// <see langword="true" /> to show the tooltip in the overlay layer of the host's window instead of
    /// a popup of its own.
    /// </param>
    public static void SetIsUseOverlayHost(Control element, bool value)
    {
        element.SetValue(IsUseOverlayHostProperty, value);
    }

    /// <summary>
    /// Reads <see cref="IsArrowVisibleProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Whether the tooltip shows the arrow that points at the host; true by default. A placement that
    /// has no arrow ignores the value.
    /// </returns>
    public static bool GetIsArrowVisible(Control element)
    {
        return element.GetValue(IsArrowVisibleProperty);
    }

    /// <summary>
    /// Writes <see cref="IsArrowVisibleProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="flag">
    /// <see langword="false" /> to hide the arrow that points at the host.
    /// </param>
    public static void SetIsArrowVisible(Control element, bool flag)
    {
        element.SetValue(IsArrowVisibleProperty, flag);
    }

    /// <summary>
    /// Reads <see cref="IsPointAtCenterProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Whether the tooltip points at the center of the host instead of its edge; false by default.
    /// </returns>
    public static bool GetIsPointAtCenter(Control element)
    {
        return element.GetValue(IsPointAtCenterProperty);
    }

    /// <summary>
    /// Writes <see cref="IsPointAtCenterProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="flag">
    /// <see langword="true" /> to make the tooltip point at the center of the host instead of its edge.
    /// </param>
    public static void SetIsPointAtCenter(Control element, bool flag)
    {
        element.SetValue(IsPointAtCenterProperty, flag);
    }

    /// <summary>
    /// Reads <see cref="IsOpenProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that shows the tooltip.
    /// </param>
    /// <returns>
    /// The requested open state of the tooltip of the element. It can be true while the tooltip is not
    /// shown, when the element has no tip or is detached from the visual tree.
    /// </returns>
    public static bool GetIsOpen(Control element)
    {
        return element.GetValue(IsOpenProperty);
    }

    /// <summary>
    /// Writes <see cref="IsOpenProperty" /> on <paramref name="element" />. true opens the tooltip of the
    /// host, false closes it; the tooltip service writes this value as the pointer moves.
    /// </summary>
    /// <param name="element">
    /// The element that shows the tooltip.
    /// </param>
    /// <param name="value">
    /// <see langword="true" /> opens the tooltip once the host has a tip and is attached to the visual
    /// tree; <see langword="false" /> closes it.
    /// </param>
    public static void SetIsOpen(Control element, bool value)
    {
        element.SetValue(IsOpenProperty, value);
    }

    /// <summary>
    /// Reads <see cref="PlacementProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Where the tooltip is placed relative to the host. The default is <see cref="PlacementMode.Top" />.
    /// </returns>
    public static PlacementMode GetPlacement(Control element)
    {
        return element.GetValue(PlacementProperty);
    }

    /// <summary>
    /// Writes <see cref="PlacementProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// Where the tooltip is placed relative to the host. <see cref="PlacementMode.Center" /> and
    /// <see cref="PlacementMode.Pointer" /> cannot show an arrow.
    /// </param>
    public static void SetPlacement(Control element, PlacementMode value)
    {
        element.SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// Reads <see cref="HorizontalOffsetProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The horizontal offset of the popup from its placed position in device-independent pixels; the
    /// default is 0.
    /// </returns>
    public static double GetHorizontalOffset(Control element)
    {
        return element.GetValue(HorizontalOffsetProperty);
    }

    /// <summary>
    /// Writes <see cref="HorizontalOffsetProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// The horizontal offset of the popup from its placed position, in device-independent pixels.
    /// </param>
    public static void SetHorizontalOffset(Control element, double value)
    {
        element.SetValue(HorizontalOffsetProperty, value);
    }

    /// <summary>
    /// Reads <see cref="VerticalOffsetProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The vertical offset of the popup from its placed position in device-independent pixels; the
    /// default is 0.
    /// </returns>
    public static double GetVerticalOffset(Control element)
    {
        return element.GetValue(VerticalOffsetProperty);
    }

    /// <summary>
    /// Writes <see cref="VerticalOffsetProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// The vertical offset of the popup from its placed position, in device-independent pixels.
    /// </param>
    public static void SetVerticalOffset(Control element, double value)
    {
        element.SetValue(VerticalOffsetProperty, value);
    }

    /// <summary>
    /// Reads <see cref="ShowDelayProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The time in milliseconds that the pointer rests on the host before the tooltip service opens its
    /// tooltip; the default is 400.
    /// </returns>
    public static int GetShowDelay(Control element)
    {
        return element.GetValue(ShowDelayProperty);
    }

    /// <summary>
    /// Writes <see cref="ShowDelayProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// The time in milliseconds that the pointer rests on the host before the tooltip service opens its
    /// tooltip; 0 opens it at once.
    /// </param>
    public static void SetShowDelay(Control element, int value)
    {
        element.SetValue(ShowDelayProperty, value);
    }

    /// <summary>
    /// Reads <see cref="BetweenShowDelayProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The time window in milliseconds after a tooltip closes within which the tooltip of the next host
    /// opens without waiting for <see cref="ShowDelayProperty" />; the default is 100.
    /// </returns>
    public static int GetBetweenShowDelay(Control element)
    {
        return element.GetValue(BetweenShowDelayProperty);
    }

    /// <summary>
    /// Writes <see cref="BetweenShowDelayProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// The time window in milliseconds after a tooltip closes within which the tooltip of the next host
    /// opens at once; a negative value disables the window.
    /// </param>
    public static void SetBetweenShowDelay(Control element, int value)
    {
        element.SetValue(BetweenShowDelayProperty, value);
    }

    /// <summary>
    /// Reads <see cref="ShowOnDisabledProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Whether a disabled element still gets its tooltip. The value is inherited from the nearest
    /// ancestor that sets it; <see langword="false" /> when none does.
    /// </returns>
    public static bool GetShowOnDisabled(Control element)
    {
        return element.GetValue(ShowOnDisabledProperty);
    }

    /// <summary>
    /// Writes <see cref="ShowOnDisabledProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// <see langword="true" /> to let a disabled element still get its tooltip. The value is inherited by
    /// the descendants that do not set their own.
    /// </param>
    public static void SetShowOnDisabled(Control element, bool value)
    {
        element.SetValue(ShowOnDisabledProperty, value);
    }

    /// <summary>
    /// Reads <see cref="ServiceEnabledProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Whether the tooltip service shows tooltips for the element. The value is inherited from the nearest
    /// ancestor that sets it; <see langword="true" /> when none does.
    /// </returns>
    public static bool GetServiceEnabled(Control element)
    {
        return element.GetValue(ServiceEnabledProperty);
    }

    /// <summary>
    /// Writes <see cref="ServiceEnabledProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// <see langword="false" /> to stop the tooltip service from showing tooltips for the element and for the
    /// descendants that inherit the value.
    /// </param>
    public static void SetServiceEnabled(Control element, bool value)
    {
        element.SetValue(ServiceEnabledProperty, value);
    }

    /// <summary>
    /// Reads <see cref="MarginToAnchorProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// The gap between the tooltip and its host in device-independent pixels; the default is 4.
    /// </returns>
    public static double GetMarginToAnchor(Control element)
    {
        return element.GetValue(MarginToAnchorProperty);
    }

    /// <summary>
    /// Writes <see cref="MarginToAnchorProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="margin">
    /// The gap between the tooltip and its host, in device-independent pixels.
    /// </param>
    public static void SetMarginToAnchor(Control element, double margin)
    {
        element.SetValue(MarginToAnchorProperty, margin);
    }

    /// <summary>
    /// Reads <see cref="IsCustomShowAndHideProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// Whether the app shows and hides the tooltip itself, not the tooltip service.
    /// </returns>
    public static bool GetIsCustomShowAndHide(Control element)
    {
        return element.GetValue(IsCustomShowAndHideProperty);
    }

    /// <summary>
    /// Writes <see cref="IsCustomShowAndHideProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="flag">
    /// <see langword="true" /> when the app shows and hides the tooltip itself, not the tooltip service.
    /// </param>
    public static void SetIsCustomShowAndHide(Control element, bool flag)
    {
        element.SetValue(IsCustomShowAndHideProperty, flag);
    }

    /// <summary>
    /// Reads <see cref="TextWrappingProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// How the tooltip text wraps inside its content area; <see cref="TextWrapping.Wrap" /> by default.
    /// </returns>
    public static TextWrapping GetTextWrapping(Control element)
    {
        return element.GetValue(TextWrappingProperty);
    }

    /// <summary>
    /// Writes <see cref="TextWrappingProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// How the tooltip text wraps inside its content area.
    /// </param>
    public static void SetTextWrapping(Control element, TextWrapping value)
    {
        element.SetValue(TextWrappingProperty, value);
    }

    /// <summary>
    /// Reads <see cref="TextTrimmingProperty" /> from <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <returns>
    /// How the tooltip text is trimmed when it does not fit; <see cref="TextTrimming.None" /> by default.
    /// </returns>
    public static TextTrimming GetTextTrimming(Control element)
    {
        return element.GetValue(TextTrimmingProperty);
    }

    /// <summary>
    /// Writes <see cref="TextTrimmingProperty" /> on <paramref name="element" />.
    /// </summary>
    /// <param name="element">
    /// The element that carries the value.
    /// </param>
    /// <param name="value">
    /// How the tooltip text is trimmed when it does not fit.
    /// </param>
    public static void SetTextTrimming(Control element, TextTrimming value)
    {
        element.SetValue(TextTrimmingProperty, value);
    }

    /// <summary>
    /// Adds a handler for <see cref="ToolTipOpeningEvent" />. The event is raised on the host element only, so
    /// <paramref name="element" /> must be the host.
    /// </summary>
    /// <param name="element">
    /// The host element that shows the tooltip.
    /// </param>
    /// <param name="handler">
    /// The handler to add.
    /// </param>
    public static void AddToolTipOpeningHandler(Control element, EventHandler<CancelRoutedEventArgs> handler) =>
        element.AddHandler(ToolTipOpeningEvent, handler);

    /// <summary>
    /// Removes a <see cref="ToolTipOpeningEvent" /> handler that <see cref="AddToolTipOpeningHandler" /> added.
    /// </summary>
    /// <param name="element">
    /// The host element that carries the handler.
    /// </param>
    /// <param name="handler">
    /// The handler to remove.
    /// </param>
    public static void RemoveToolTipOpeningHandler(Control element, EventHandler<CancelRoutedEventArgs> handler) =>
        element.RemoveHandler(ToolTipOpeningEvent, handler);

    /// <summary>
    /// Adds a handler for <see cref="ToolTipClosingEvent" />. The event is raised on the host element only, so
    /// <paramref name="element" /> must be the host.
    /// </summary>
    /// <param name="element">
    /// The host element that shows the tooltip.
    /// </param>
    /// <param name="handler">
    /// The handler to add.
    /// </param>
    public static void AddToolTipClosingHandler(Control element, EventHandler<RoutedEventArgs> handler) =>
        element.AddHandler(ToolTipClosingEvent, handler);

    /// <summary>
    /// Removes a <see cref="ToolTipClosingEvent" /> handler that <see cref="AddToolTipClosingHandler" /> added.
    /// </summary>
    /// <param name="element">
    /// The host element that carries the handler.
    /// </param>
    /// <param name="handler">
    /// The handler to remove.
    /// </param>
    public static void RemoveToolTipClosingHandler(Control element, EventHandler<RoutedEventArgs> handler) =>
        element.RemoveHandler(ToolTipClosingEvent, handler);

    /// <summary>
    /// Called when the tooltip enters the visual tree: subscribes to theme token changes and resolves the color
    /// for the current theme at once. An override must call the base method, or the color never tracks theme
    /// changes.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        // Preset colors come from the snapshot's palettes: re-resolve them whenever the theme publishes. The
        // subscription does not fire for the first context, so resolve against the context of this attach too.
        _themeSubscription?.Dispose();
        _themeSubscription = s_themeTokenResolver.Subscribe(this, _ => UpdateColorBackground());
        UpdateColorBackground();
    }

    /// <summary>
    /// Called when the tooltip leaves the visual tree: releases the theme token subscription, so that a detached
    /// tooltip no longer refreshes its color on theme changes. An override must call the base method, or the
    /// subscription outlives the attachment.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _themeSubscription?.Dispose();
        _themeSubscription = null;
    }

    /// <summary>
    /// Looks up the template parts: the arrow decorator (<c>PART_ArrowDecorator</c>) and the content presenter
    /// (<c>PART_ContentPresenter</c>). Either is left null when the template lacks it; without the arrow
    /// decorator the tooltip reports no arrow to the popup.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _arrowDecoratedBox = e.NameScope.Find<ArrowDecoratedBox>(ArrowDecoratedBox.ArrowDecoratorPart);
        _contentPresenter  = e.NameScope.Find<ContentPresenter>("PART_ContentPresenter");
    }

    #endregion

    #region 内部协作 API

    internal static bool GetIsPopupPinnedOpen(Control element)
    {
        return element.GetValue(IsPopupPinnedOpenProperty);
    }

    internal static void SetIsPopupPinnedOpen(Control element, bool value)
    {
        element.SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    #endregion

    #region 实现 IShadowMaskInfoProvider 接口

    CornerRadius IShadowMaskInfoProvider.GetMaskCornerRadius()
    {
        return ResolveArrowDecoratedBox()?.GetMaskCornerRadius() ?? new CornerRadius(0);
    }

    Rect IShadowMaskInfoProvider.GetMaskBounds()
    {
        return ResolveArrowDecoratedBox()?.GetMaskBounds() ?? new Rect(Bounds.Size);
    }

    IBrush? IShadowMaskInfoProvider.GetMaskBackground()
    {
        return Background;
    }

    #endregion

    #region 实现 IArrowAwareShadowMaskInfoProvider 接口

    ArrowPosition IArrowAwareShadowMaskInfoProvider.GetArrowPosition()
    {
        return ResolveArrowDecoratedBox()?.ArrowPosition ?? default;
    }

    bool IArrowAwareShadowMaskInfoProvider.IsArrowVisible()
    {
        // Report no arrow when the arrow part is unavailable; the popup degrades its shadow and placement to match.
        return ResolveArrowDecoratedBox()?.IsArrowVisible ?? false;
    }

    void IArrowAwareShadowMaskInfoProvider.SetArrowOpacity(double opacity)
    {
        if (ResolveArrowDecoratedBox() is { } arrowDecoratedBox)
        {
            arrowDecoratedBox.ArrowOpacity = opacity;
        }
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorBounds()
    {
        return ResolveArrowDecoratedBox()?.ArrowIndicatorBounds ?? default;
    }

    Rect IArrowAwareShadowMaskInfoProvider.GetArrowIndicatorLayoutBounds()
    {
        return ResolveArrowDecoratedBox()?.ArrowIndicatorLayoutBounds ?? default;
    }

    AbstractArrowDecoratedBox? IArrowAwareShadowMaskInfoProvider.GetArrowDecoratedBox()
    {
        return ResolveArrowDecoratedBox();
    }

    #endregion

    #region 内部实现

    private static void IsOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var control = (Control)e.Sender;
        ReconcileOpenState(control);
    }

    private static void TipChanged(AvaloniaPropertyChangedEventArgs e)
    {
        ReconcileOpenState((Control)e.Sender);
    }

    private static void IsPopupPinnedOpenChanged(AvaloniaPropertyChangedEventArgs e)
    {
        var control = (Control)e.Sender;
        if (e.GetNewValue<bool>())
        {
            if (!GetIsOpen(control))
            {
                control.SetCurrentValue(IsOpenProperty, true);
                return;
            }
        }
        else if (GetIsOpen(control))
        {
            var toolTip = control.GetValue(ToolTipProperty);
            control.SetCurrentValue(IsOpenProperty, toolTip?._popup?.IsOpen == true);
            return;
        }

        ReconcileOpenState(control);
    }

    private static bool CoerceIsOpen(AvaloniaObject sender, bool value)
    {
        return !value && sender is Control control && GetIsPopupPinnedOpen(control)
            ? true
            : value;
    }

    /// <summary>
    /// Converges the popup of the host to the requested state. <see cref="IsOpenProperty" /> is the requested
    /// state: the tooltip is meant to be open when it is true, the tip is ready and the host is attached to the
    /// visual tree. Every input that affects the open state (a change of IsOpen or Tip, the host attaching)
    /// goes through this method, and no second path may open or close the popup directly. The method must
    /// stay idempotent.
    /// </summary>
    private static void ReconcileOpenState(Control control)
    {
        var toolTip          = control.GetValue(ToolTipProperty);
        var isPhysicallyOpen = toolTip?._popup?.IsOpen == true;
        var isAttached       = control.IsAttachedToVisualTree();
        var wantOpen         = GetIsOpen(control) && GetTip(control) is not null && isAttached;

        if (!GetIsOpen(control) || wantOpen)
        {
            ClearPendingAttachHandler(control);
        }
        else if (!isAttached)
        {
            // Open is requested but the host is not attached to the visual tree yet: subscribe once and reconcile
            // again after it attaches.
            EnsurePendingAttachHandler(control);
        }

        if (wantOpen == isPhysicallyOpen)
        {
            return;
        }

        if (wantOpen)
        {
            var args = new CancelRoutedEventArgs(ToolTipOpeningEvent);
            control.RaiseEvent(args);
            if (args.Cancel)
            {
                control.SetCurrentValue(IsOpenProperty, false);
                return;
            }

            var tip = GetTip(control)!;
            if (toolTip == null || (tip != toolTip && tip != toolTip.Content))
            {
                toolTip?.Close();
                toolTip = tip as ToolTip ?? new ToolTip() { Content = tip };
                control.SetValue(ToolTipProperty, toolTip);
            }

            toolTip.AdornedControl = control;
            toolTip.Open(control);
        }
        else if (toolTip is not null)
        {
            // Only the popup closes here. IsOpen stays true (the tip is not ready or the host was detached), so the
            // requested state is kept and the reconciliation reopens the tooltip once the conditions hold.
            if (GetIsPopupPinnedOpen(control))
            {
                toolTip.CloseForLifecycle();
            }
            else
            {
                toolTip.Close();
            }
        }
    }

    private static void EnsurePendingAttachHandler(Control control)
    {
        if (control.GetValue(PendingAttachHandlerProperty) is not null)
        {
            return;
        }

        EventHandler<VisualTreeAttachmentEventArgs> handler = (s, _) =>
        {
            if (s is Control attachedControl)
            {
                ClearPendingAttachHandler(attachedControl);
                ReconcileOpenState(attachedControl);
            }
        };
        control.SetValue(PendingAttachHandlerProperty, handler);
        control.AttachedToVisualTree += handler;
    }

    private static void ClearPendingAttachHandler(Control control)
    {
        var handler = control.GetValue(PendingAttachHandlerProperty);
        if (handler is not null)
        {
            control.AttachedToVisualTree -= handler;
            control.ClearValue(PendingAttachHandlerProperty);
        }
    }

    private void Open(Control control)
    {
        Close();
        ApplyTemplate();

        if (_popup is null)
        {
            // This cross-root Popup is created at runtime, outside ToolTip's
            // ControlTheme; its pure default projection shares the popup lifetime.
            _popup = new Popup();
            _popup.Child = this;
            _popup.TakesFocusFromNativeControl = false;
            _popup.WindowManagerAddShadowHint = false;

            _popup.Opened                       += HandlePopupOpened;
            _popup.Closed                       += HandlePopupClosed;
            _popup.PositionFlipped              += HandlePositionFlipped;
        }

        _subscriptions = new CompositeDisposable([
            Bind(ShouldUseOverlayPopupProperty, control.GetBindingObservable(IsUseOverlayHostProperty)),
            _popup.Bind(Popup.ShouldUseOverlayLayerProperty, this.GetObservable(ShouldUseOverlayPopupProperty)),
            _popup.Bind(Popup.OpenMotionProperty, this.GetObservable(OpenMotionProperty)),
            _popup.Bind(Popup.CloseMotionProperty, this.GetObservable(CloseMotionProperty)),
            _popup.Bind(Popup.MotionDurationProperty, this.GetObservable(MotionDurationProperty)),
            _popup.Bind(Popup.IsMotionEnabledProperty, this.GetObservable(IsMotionEnabledProperty)),
            // Cross-root relay of the tooltip's own shadow tokens (see ToolTipShadowsProperty).
            _popup.Bind(Popup.PopupRootShadowProperty, this.GetObservable(ToolTipShadowsProperty)),
            _popup.Bind(Popup.OverlayHostShadowProperty, this.GetObservable(ToolTipShadowsProperty)),
            _popup.Bind(Popup.HorizontalOffsetProperty, ResolveValueSource(control, HorizontalOffsetProperty).GetBindingObservable(HorizontalOffsetProperty)),
            _popup.Bind(Popup.VerticalOffsetProperty, ResolveValueSource(control, VerticalOffsetProperty).GetBindingObservable(VerticalOffsetProperty)),
            _popup.Bind(Popup.RequestedPlacementProperty, ResolveValueSource(control, PlacementProperty).GetBindingObservable(PlacementProperty, v => (PlacementMode?)v)),
            _popup.Bind(Popup.MarginToAnchorProperty, ResolveMarginToAnchor(control)),
            _popup.Bind(Popup.IsPointAtCenterProperty, ResolveValueSource(control, IsPointAtCenterProperty).GetBindingObservable(IsPointAtCenterProperty)),
            _popup.Bind(Popup.IsPopupPinnedOpenProperty, control.GetBindingObservable(IsPopupPinnedOpenProperty)),
        ]);

        _popup.PlacementTarget = control;
        _popup.SetPopupParent(control);

        if (_arrowDecoratedBox is not null)
        {
            SetupArrowDecoratedBox(control);
        }
        else
        {
            TemplateApplied += DeferSetupArrowDecoratedBox;
        }

        _popup.IsOpen = true;
    }

    /// <summary>
    /// Picks the source of a presentation attached property. When the tip is a <see cref="ToolTip" /> instance,
    /// a value set on the instance itself wins over the host, and one left unset falls back to the host. That
    /// makes the instance a complete customization surface without a second configuration language on the host.
    /// </summary>
    private AvaloniaObject ResolveValueSource(Control host, AvaloniaProperty property)
    {
        return IsSet(property) ? this : host;
    }

    /// <summary>
    /// The gap to the anchor: an explicit value on this tooltip, then one on the host, then the theme's token.
    /// </summary>
    private IObservable<double> ResolveMarginToAnchor(Control host)
    {
        if (IsSet(MarginToAnchorProperty))
        {
            return this.GetObservable(MarginToAnchorProperty);
        }

        return host.IsSet(MarginToAnchorProperty)
            ? host.GetObservable(MarginToAnchorProperty)
            : this.GetObservable(ThemeMarginToAnchorProperty);
    }

    private T GetToolTipValue<T>(Control host, AttachedProperty<T> property)
    {
        return IsSet(property) ? GetValue(property) : host.GetValue(property);
    }

    private void Close()
    {
        if (AdornedControl is { } adornedControl
            && GetIsOpen(adornedControl))
        {
            var args = new RoutedEventArgs(ToolTipClosingEvent);
            adornedControl.RaiseEvent(args);
        }

        if (_popup != null)
        {
            _popup.IsOpen = false;
        }
        else
        {
            _subscriptions?.Dispose();
        }
    }

    private void CloseForLifecycle()
    {
        if (_popup != null)
        {
            _popup.CloseForLifecycle();
        }
        else
        {
            _subscriptions?.Dispose();
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        if (AdornedControl is { } adornedControl
            && GetIsOpen(adornedControl))
        {
            // Closed can fire synchronously while the host is detached from the visual tree. The host's
            // PresentationSource is still set then, so a detach cannot be told apart reliably at this point.
            // Defer the convergence of the requested state until the detach or attach pass has ended, and
            // judge by the real attachment state then.
            Dispatcher.UIThread.Post(() => ConvergeIsOpenAfterPopupClosed(adornedControl), DispatcherPriority.Background);
        }

        UpdatePseudoClasses(false);
        Closed?.Invoke(this, EventArgs.Empty);
        if (sender is Popup popup)
        {
            popup.SetPopupParent(null);
            popup.PlacementTarget = null;
            _subscriptions?.Dispose();
            _subscriptions = null;
        }
    }

    private void ConvergeIsOpenAfterPopupClosed(Control adornedControl)
    {
        if (!GetIsOpen(adornedControl) || _popup?.IsOpen == true)
        {
            // The requested state was already converged by the reconciliation, or the popup has reopened.
            return;
        }

        if (adornedControl.IsAttachedToVisualTree())
        {
            // The popup closed for a reason other than a detached host: write the requested state back as closed.
            adornedControl.SetCurrentValue(IsOpenProperty, false);
        }
        else
        {
            // A lifecycle close caused by the host being detached: keep the requested IsOpen state. The reconciliation
            // reopens the tooltip once the host is attached again.
            EnsurePendingAttachHandler(adornedControl);
        }
    }

    private void HandlePositionFlipped(object? sender, PopupFlippedEventArgs args)
    {
        if (sender is Popup popup && popup.PlacementTarget != null)
        {
            SetupArrowPosition(GetToolTipValue(popup.PlacementTarget, PlacementProperty), args.HorizontalFlipped, args.VerticalFlipped);
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        UpdatePseudoClasses(true);
    }

    private void UpdatePseudoClasses(bool newValue)
    {
        PseudoClasses.Set(ToolTipPseudoClass.Open, newValue);
    }

    private void DeferSetupArrowDecoratedBox(object? sender, TemplateAppliedEventArgs args)
    {
        if (_arrowDecoratedBox is null)
        {
            // The template applied this time still has no arrow part: stay subscribed and finish the arrow
            // setup after a later template application.
            // later template application.
            return;
        }
        TemplateApplied -= DeferSetupArrowDecoratedBox;
        Debug.Assert(_popup != null && _popup.PlacementTarget != null);
        SetupArrowDecoratedBox(_popup.PlacementTarget);
    }

    private void SetupArrowDecoratedBox(Control control)
    {
        if (_arrowDecoratedBox is not null)
        {
            SetToolTipColor(control);
            if (_contentPresenter != null)
            {
                _contentPresenter.Width = GetToolTipValue(control, TipHostWidthProperty);
                _subscriptions?.Add(_contentPresenter.Bind(ContentPresenter.TextWrappingProperty,
                    ResolveValueSource(control, TextWrappingProperty).GetBindingObservable(TextWrappingProperty)));
                _subscriptions?.Add(_contentPresenter.Bind(ContentPresenter.TextTrimmingProperty,
                    ResolveValueSource(control, TextTrimmingProperty).GetBindingObservable(TextTrimmingProperty)));
            }

            _arrowDecoratedBox.Bind(ArrowDecoratedBox.IsArrowVisibleProperty,
                ResolveValueSource(control, IsArrowVisibleProperty).GetBindingObservable(IsArrowVisibleProperty, flag =>
                {
                    // Some placements do not allow the arrow pointer to be shown.
                    if (flag && _popup is not null)
                    {
                        return PopupUtils.CanEnabledArrow(GetToolTipValue(control, PlacementProperty));
                    }

                    return flag;
                }));
            if (_popup is not null)
            {
                SetupArrowPosition(GetToolTipValue(control, PlacementProperty), false, false);
            }
        }
    }

    private ArrowDecoratedBox? ResolveArrowDecoratedBox()
    {
        if (_arrowDecoratedBox is null)
        {
            // Build the template at once while the control is attached and styled. The result is null when the
            // template is not ready or the theme provides no PART_ArrowDecorator part; callers then degrade to
            // no arrow.
            ApplyTemplate();
        }

        return _arrowDecoratedBox;
    }

    private void SetupArrowPosition(PlacementMode placement, bool isHorizontalFlipped, bool isVerticalFlipped, PopupAnchor? anchor = null, PopupGravity? gravity = null)
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(placement, anchor, gravity);
        if (_arrowDecoratedBox is not null && arrowPosition is not null)
        {
            _arrowDecoratedBox.ArrowPosition = ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, isHorizontalFlipped, isVerticalFlipped);
        }
    }

    private void SetToolTipColor(Control control)
    {
        // The preset color takes precedence over the custom color.
        _requestedPresetColor = GetToolTipValue(control, PresetColorProperty);
        _requestedColor       = GetToolTipValue(control, ColorProperty);
        UpdateColorBackground();
    }

    private void UpdateColorBackground()
    {
        Color? background = null;
        if (_requestedPresetColor is { } presetColorType)
        {
            // antd genPresetColor darkColor: palette step 6 of the snapshot (dark palettes under a dark theme).
            if (ThemeScope.ResolveContext(this) is { } context &&
                PresetColorPalette.TryResolve(context.Snapshot, PresetPrimaryColor.GetColor(presetColorType), out var colors))
            {
                background = colors.DarkColor;
            }
        }
        else if (_requestedColor is { } color)
        {
            background = color;
        }

        if (background is { } effective)
        {
            if (ColorBackground is not ISolidColorBrush { Opacity: 1.0 } current || current.Color != effective)
            {
                ColorBackground = new ImmutableSolidColorBrush(effective);
            }
        }
        else
        {
            // Falls back to the ToolTipBackground Token the ControlTheme gives ColorBackground.
            ClearValue(ColorBackgroundProperty);
        }
    }

    #endregion
}
