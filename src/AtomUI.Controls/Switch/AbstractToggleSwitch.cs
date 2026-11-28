using System.Globalization;
using System.Reactive.Disposables;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using AtomUI.Data;
using AtomUI.Media;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Media.Transformation;
using Avalonia.Metadata;
using Avalonia.Rendering;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls;

/// <summary>
/// Base class of the toggle switch: a two-state switch with a knob that slides inside a groove.
/// </summary>
/// <remarks>
///     <para>
///     The control measures and arranges the parts of its template itself and paints the groove itself.
///     Besides the knob it looks up <c>PART_OnContentPresenter</c>, <c>PART_OffContentPresenter</c>,
///     <c>PART_WaveSpirit</c> and <c>PART_MainContainer</c>.
///     </para>
///     <para>
///     The width of the switch grows with its content but never falls below <see cref="TrackMinWidth" />;
///     the height is <see cref="TrackHeight" />.
///     </para>
/// </remarks>
[TemplatePart("PART_SwitchKnob", typeof(SwitchKnob))]
[TemplatePart("PART_Track", typeof(Control))]
public abstract class AbstractToggleSwitch : ToggleButton,
                                             IFocusOutlineShapeProvider,
                                             ICustomizableSizeTypeAware,
                                             ICustomHitTest,
                                             IInteractionEffectAwareControl,
                                             IFormItemAware,
                                             ITransitionInitializationOwner
{
    #region 公共属性定义
    
    /// <summary>
    /// Identifies the <see cref="GrooveBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> GrooveBackgroundProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, IBrush?>(
            nameof(GrooveBackground));

    /// <summary>
    /// Identifies the <see cref="GrooveBorderBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> GrooveBorderBrushProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, IBrush?>(nameof(GrooveBorderBrush));

    /// <summary>
    /// Identifies the <see cref="GrooveBorderThickness" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GrooveBorderThicknessProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(GrooveBorderThickness));

    /// <summary>
    /// Identifies the <see cref="OnContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> OnContentProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, object?>(nameof(OnContent));
    
    /// <summary>
    /// Identifies the <see cref="OnContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OnContentTemplateProperty = 
        AvaloniaProperty.Register<AbstractToggleSwitch, IDataTemplate?>(nameof(OnContentTemplate));
    
    /// <summary>
    /// Identifies the <see cref="OffContent" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> OffContentProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, object?>(nameof(OffContent));

    /// <summary>
    /// Identifies the <see cref="OffContentTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> OffContentTemplateProperty = 
        AvaloniaProperty.Register<AbstractToggleSwitch, IDataTemplate?>(nameof(OffContentTemplate));
    
    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<AbstractToggleSwitch>();
    
    /// <summary>
    /// Identifies the <see cref="IsLoading" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, bool>(nameof(IsLoading));

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractToggleSwitch>();

    /// <summary>
    /// Identifies the <see cref="IsInteractionEffectEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.AddOwner<AbstractToggleSwitch>();

    /// <summary>
    /// Identifies the <see cref="TrackHeight" /> property.
    /// </summary>
    public static readonly StyledProperty<double> TrackHeightProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(TrackHeight));

    /// <summary>
    /// Identifies the <see cref="TrackMinWidth" /> property.
    /// </summary>
    public static readonly StyledProperty<double> TrackMinWidthProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(TrackMinWidth));

    /// <summary>
    /// Identifies the <see cref="TrackPadding" /> property.
    /// </summary>
    public static readonly StyledProperty<double> TrackPaddingProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(TrackPadding));

    /// <summary>
    /// Identifies the <see cref="KnobSize" /> property.
    /// </summary>
    public static readonly StyledProperty<Size> KnobSizeProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, Size>(nameof(KnobSize));

    /// <summary>
    /// Identifies the <see cref="TrackCornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius?> TrackCornerRadiusProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, CornerRadius?>(nameof(TrackCornerRadius));

    /// <summary>
    /// Identifies the <see cref="HandleCornerRadius" /> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius?> HandleCornerRadiusProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, CornerRadius?>(nameof(HandleCornerRadius));

    /// <summary>
    /// Identifies the <see cref="KnobRenderTransform" /> property.
    /// </summary>
    public static readonly StyledProperty<ITransform?> KnobRenderTransformProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, ITransform?>(nameof(KnobRenderTransform));

    /// <summary>
    /// Identifies the <see cref="KnobWidthExtension" /> property.
    /// </summary>
    public static readonly StyledProperty<double> KnobWidthExtensionProperty =
        SwitchKnob.KnobWidthExtensionProperty.AddOwner<AbstractToggleSwitch>();

    /// <summary>
    /// Identifies the <see cref="OnContentOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OnContentOffsetProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, Point>(nameof(OnContentOffset));

    /// <summary>
    /// Identifies the <see cref="OffContentOffset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OffContentOffsetProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, Point>(nameof(OffContentOffset));
    
    /// <summary>
    /// Gets or sets the brush that fills the groove of the switch.
    /// </summary>
    /// <remarks>
    /// The theme sets it from the state of the switch, for example <c>TrackBg</c> while it is unchecked and
    /// <c>SwitchColor</c> while it is checked, and animates a change while motion is enabled.
    /// The click wave uses this brush as well.
    /// </remarks>
    public IBrush? GrooveBackground
    {
        get => GetValue(GrooveBackgroundProperty);
        set => SetValue(GrooveBackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets the brush of the groove's outline.
    /// </summary>
    /// <remarks>
    /// The outline lies inside the groove. Nothing is drawn while this value is <see langword="null" /> or
    /// <see cref="GrooveBorderThickness" /> is <c>0</c> or less. The theme sets the value from the
    /// <c>TrackBorderColor</c> token, or from <c>TrackCheckedBorderColor</c> while the switch is checked.
    /// </remarks>
    public IBrush? GrooveBorderBrush
    {
        get => GetValue(GrooveBorderBrushProperty);
        set => SetValue(GrooveBorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of the groove's outline in device-independent pixels.
    /// </summary>
    /// <remarks>
    /// The outline lies inside the groove, so it changes neither the layout nor the knob position.
    /// Nothing is drawn while the value is <c>0</c> or less or <see cref="GrooveBorderBrush" /> is
    /// <see langword="null" />. The theme sets the value from the <c>TrackBorderThickness</c> token.
    /// </remarks>
    public double GrooveBorderThickness
    {
        get => GetValue(GrooveBorderThicknessProperty);
        set => SetValue(GrooveBorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the content shown inside the groove while the switch is checked.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The default is <see langword="null" />, which shows nothing. The wider of <see cref="OnContent" /> and
    ///     <see cref="OffContent" />, plus the inner margins, sets the width of the switch.
    ///     </para>
    ///     <para>
    ///     A <c>PathIcon</c> follows the icon size of the <see cref="SizeType" /> and the
    ///     <see cref="TemplatedControl.Foreground" /> of the switch.
    ///     </para>
    /// </remarks>
    [DependsOn(nameof(OnContentTemplate))]
    public object? OnContent
    {
        get => GetValue(OnContentProperty);
        set => SetValue(OnContentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the <see cref="OnContent" />; <see langword="null" />, the default,
    /// leaves the content presenter to present the content in its default form.
    /// </summary>
    public IDataTemplate? OnContentTemplate
    {
        get => GetValue(OnContentTemplateProperty);
        set => SetValue(OnContentTemplateProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the content shown inside the groove while the switch is unchecked.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The default is <see langword="null" />, which shows nothing. The wider of <see cref="OnContent" /> and
    ///     <see cref="OffContent" />, plus the inner margins, sets the width of the switch.
    ///     </para>
    ///     <para>
    ///     A <c>PathIcon</c> follows the icon size of the <see cref="SizeType" /> and the
    ///     <see cref="TemplatedControl.Foreground" /> of the switch.
    ///     </para>
    /// </remarks>
    [DependsOn(nameof(OffContentTemplate))]
    public object? OffContent
    {
        get => GetValue(OffContentProperty);
        set => SetValue(OffContentProperty, value);
    }
    
    /// <summary>
    /// Gets or sets the template that builds the <see cref="OffContent" />; <see langword="null" />, the default,
    /// leaves the content presenter to present the content in its default form.
    /// </summary>
    public IDataTemplate? OffContentTemplate
    {
        get => GetValue(OffContentTemplateProperty);
        set => SetValue(OffContentTemplateProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the switch. The default is <see cref="CustomizableSizeType.Middle" />.
    /// </summary>
    /// <remarks>
    /// The theme takes the knob size, track height, minimum track width, font size and icon size from the size:
    /// <c>Small</c> uses the small tokens and every other value the regular ones. The inner content margins
    /// follow the same split.
    /// </remarks>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }
    
    /// <summary>
    /// Gets or sets whether the switch is loading.
    /// </summary>
    /// <remarks>
    /// While loading, an arc rotates on the knob, the switch ignores pointer input and the theme dims it.
    /// The arc rotates only while <see cref="IsMotionEnabled" /> is <see langword="true" />.
    /// </remarks>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// Gets or sets whether state changes animate.
    /// </summary>
    /// <remarks>
    /// While <see langword="true" />, the theme animates the knob position, the content slide, the groove
    /// background and the opacity, and the click wave and the loading arc are allowed to play.
    /// The theme sets the value from the <c>EnableMotion</c> token.
    /// </remarks>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether interaction feedback plays from the groove when the checked state changes.
    /// </summary>
    /// <remarks>
    /// Feedback plays only while <see cref="IsMotionEnabled" /> is also <see langword="true" />.
    /// The theme sets the value from the <c>EnableInteractionEffect</c> token.
    /// </remarks>
    public bool IsInteractionEffectEnabled
    {
        get => GetValue(IsInteractionEffectEnabledProperty);
        set => SetValue(IsInteractionEffectEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of the groove in device-independent pixels; it is also the height of the switch.
    /// </summary>
    /// <remarks>
    /// The theme sets the value from the track height token of the <see cref="SizeType" />.
    /// </remarks>
    public double TrackHeight
    {
        get => GetValue(TrackHeightProperty);
        set => SetValue(TrackHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the smallest width of the groove in device-independent pixels.
    /// </summary>
    /// <remarks>
    /// The width of the switch is the wider content plus the inner margins, but never less than this value; without
    /// content it is this value. The value is also the lower bound of an explicit
    /// <see cref="Avalonia.Layout.Layoutable.Width" />. The theme sets the value from the minimum track width token of
    /// the <see cref="SizeType" />.
    /// </remarks>
    public double TrackMinWidth
    {
        get => GetValue(TrackMinWidthProperty);
        set => SetValue(TrackMinWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the inset of the knob from the groove edge that it rests on, in device-independent pixels.
    /// </summary>
    /// <remarks>
    /// The knob rests on the left edge while the switch is unchecked and on the right edge while it is checked;
    /// the value is also the inset from the top edge. A negative value moves the knob outside the groove.
    /// The theme sets the value from the <c>TrackPadding</c> token.
    /// </remarks>
    public double TrackPadding
    {
        get => GetValue(TrackPaddingProperty);
        set => SetValue(TrackPaddingProperty, value);
    }

    /// <summary>
    /// Gets or sets the size of the knob in device-independent pixels.
    /// </summary>
    /// <remarks>
    /// The theme sets the value from the handle size token of the <see cref="SizeType" />. While the switch is
    /// pressed, the knob can be drawn wider than this size; the layout does not change.
    /// </remarks>
    public Size KnobSize
    {
        get => GetValue(KnobSizeProperty);
        set => SetValue(KnobSizeProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of the switch track, or <see langword="null" /> to draw a full capsule pill.
    /// </summary>
    public CornerRadius? TrackCornerRadius
    {
        get => GetValue(TrackCornerRadiusProperty);
        set => SetValue(TrackCornerRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the corner radius of the switch handle, or <see langword="null" /> to draw a circle or capsule pill.
    /// </summary>
    public CornerRadius? HandleCornerRadius
    {
        get => GetValue(HandleCornerRadiusProperty);
        set => SetValue(HandleCornerRadiusProperty, value);
    }

    // The knob target is the base value its transition animates to. SetCurrentValue would write it into the active
    // animation layer during a transition, and the end of that transition would drop it.
    /// <summary>
    /// Gets the transform of the knob: a translation to the top-left corner of the knob rectangle, which lies
    /// <see cref="TrackPadding" /> from the left edge of the switch while the switch is unchecked, from the
    /// right edge while it is checked, and from the top edge. The theme binds <c>RenderTransform</c> of
    /// <c>PART_SwitchKnob</c> to it and animates it while <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// Only the control sets the value: when the switch is arranged, when the checked state changes and, for a
    /// knob of the template, when the pressed state changes. A custom template that leaves the
    /// <c>RenderTransform</c> of <c>PART_SwitchKnob</c> unset gets the same binding from
    /// <see cref="OnApplyTemplate" />.
    /// </remarks>
    public ITransform? KnobRenderTransform
    {
        get => GetValue(KnobRenderTransformProperty);
        private set => SetValue(KnobRenderTransformProperty, value);
    }

    /// <summary>
    /// Gets how much wider in device-independent pixels the knob is drawn than <see cref="KnobSize" />: 30
    /// percent of the knob width while the switch is pressed and press stretch is enabled, otherwise <c>0</c>.
    /// The theme binds <c>KnobWidthExtension</c> of <c>PART_SwitchKnob</c> to it and animates it while
    /// <see cref="IsMotionEnabled" /> is <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// The layout is not affected: the knob is measured and arranged with <see cref="KnobSize" />.
    /// </remarks>
    public double KnobWidthExtension
    {
        get => GetValue(KnobWidthExtensionProperty);
        private set => SetValue(KnobWidthExtensionProperty, value);
    }

    /// <summary>
    /// Gets or sets the offset in device-independent pixels from the top-left corner of the switch at which the
    /// <see cref="OnContent" /> is laid out. While the switch is checked, the content starts from the left inner
    /// margin and is centered vertically; a stretched knob shifts it further left. While the switch is unchecked,
    /// the content lies left of the groove and is not visible.
    /// </summary>
    /// <remarks>
    /// The control sets the value when the switch is arranged and when the checked or pressed state changes.
    /// The theme binds <c>OnContentOffset</c> of <c>PART_MainContainer</c> (<c>ToggleSwitchContentPanel</c>) to
    /// it, which applies it to <c>PART_OnContentPresenter</c>; while <see cref="IsMotionEnabled" /> is
    /// <see langword="true" /> the theme animates the change.
    /// </remarks>
    public Point OnContentOffset
    {
        get => GetValue(OnContentOffsetProperty);
        set => SetValue(OnContentOffsetProperty, value);
    }

    /// <summary>
    /// Gets or sets the offset in device-independent pixels from the top-left corner of the switch at which the
    /// <see cref="OffContent" /> is laid out. While the switch is unchecked, the content starts from the maximum
    /// inner margin and is centered vertically; a stretched knob shifts it further right. While the switch is
    /// checked, the content lies right of the groove and is not visible.
    /// </summary>
    /// <remarks>
    /// The control sets the value when the switch is arranged and when the checked or pressed state changes.
    /// The theme binds <c>OffContentOffset</c> of <c>PART_MainContainer</c> (<c>ToggleSwitchContentPanel</c>)
    /// to it, which applies it to <c>PART_OffContentPresenter</c>; while <see cref="IsMotionEnabled" /> is
    /// <see langword="true" /> the theme animates the change.
    /// </remarks>
    public Point OffContentOffset
    {
        get => GetValue(OffContentOffsetProperty);
        set => SetValue(OffContentOffsetProperty, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<double> InnerMaxMarginProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(InnerMaxMargin));

    internal static readonly StyledProperty<double> InnerMinMarginProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(InnerMinMargin));

    internal static readonly StyledProperty<bool> UseContentMarginTokenResourcesProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, bool>(nameof(UseContentMarginTokenResources));

    internal static readonly StyledProperty<bool> EnableHandlePressStretchProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, bool>(nameof(EnableHandlePressStretch), true);

    internal static readonly StyledProperty<double> IconSizeProperty =
        AvaloniaProperty.Register<AbstractToggleSwitch, double>(nameof(IconSize));

    internal double InnerMaxMargin
    {
        get => IsSet(InnerMaxMarginProperty) ? GetValue(InnerMaxMarginProperty) : _contentMargins.Max;
        set => SetValue(InnerMaxMarginProperty, value);
    }

    internal double InnerMinMargin
    {
        get => IsSet(InnerMinMarginProperty) ? GetValue(InnerMinMarginProperty) : _contentMargins.Min;
        set => SetValue(InnerMinMarginProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the switch reads the Tokens that bound the inner content margins. The default is
    /// <see langword="false" />; the theme sets <see langword="true" />.
    /// </summary>
    /// <remarks>
    /// While <see langword="true" /> and attached to the visual tree, the switch subscribes to the keys of
    /// <see cref="ContentMarginKeys" />. Their values feed <see cref="InnerMinMargin" /> and
    /// <see cref="InnerMaxMargin" /> where those are not set explicitly: the small keys for the <c>Small</c>
    /// <see cref="SizeType" />, the normal keys for every other size. <see langword="false" /> releases the
    /// subscription and resets the margins that are not set explicitly to <c>0</c>. The base class provides no
    /// keys, so only a derived class that overrides <see cref="ContentMarginKeys" /> reads Tokens here.
    /// </remarks>
    internal bool UseContentMarginTokenResources
    {
        get => GetValue(UseContentMarginTokenResourcesProperty);
        set => SetValue(UseContentMarginTokenResourcesProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the knob is drawn 30 percent wider than <see cref="KnobSize" /> while the switch
    /// is pressed. The default is <see langword="true" />; the theme sets the value from the
    /// <c>EnableHandlePressStretch</c> token.
    /// </summary>
    /// <remarks>
    /// The stretch changes neither the layout nor the size the knob is arranged with. It is reported through
    /// <see cref="KnobWidthExtension" />, and the content of the switch shifts away from the knob. A change while
    /// the switch is pressed recalculates the knob and the content offsets.
    /// </remarks>
    internal bool EnableHandlePressStretch
    {
        get => GetValue(EnableHandlePressStretchProperty);
        set => SetValue(EnableHandlePressStretchProperty, value);
    }

    private OwnerTokenSet? _contentMarginTokens;
    private (double Min, double Max) _contentMargins;

    /// <summary>
    /// Resource keys of the Tokens that bound the content margin of the switch; <c>ToggleSwitch</c> in
    /// AtomUI.Desktop.Controls returns the keys of its Tokens.
    /// </summary>
    /// <remarks>
    /// The margin is read as a minimum and a maximum, each for the normal and for the small size. The keys
    /// are subscribed only while <c>UseContentMarginTokenResources</c> is <see langword="true" />. The base
    /// class returns <see langword="null" />, so no Tokens are read.
    /// </remarks>
    private protected virtual (object NormalMin, object NormalMax, object SmallMin, object SmallMax)? ContentMarginKeys => null;

    /// <summary>
    /// Gets or sets the width and height in device-independent pixels of a <see cref="PathIcon" /> that is
    /// assigned as <see cref="OnContent" /> or <see cref="OffContent" />. The theme sets the value from the
    /// <c>ContentIconSizeSM</c> token for the <c>Small</c> <see cref="SizeType" /> and from the
    /// <c>ContentIconSize</c> token for every other size.
    /// </summary>
    internal double IconSize
    {
        get => GetValue(IconSizeProperty);
        set => SetValue(IconSizeProperty, value);
    }

    #endregion

    private const double STRETCH_FACTOR = 1.3d;
    private ContentPresenter? _onContentPresenter;
    private ContentPresenter? _offContentPresenter;
    
    private Rect _knobTarget;
    private SwitchKnob? _switchKnob;
    private Control? _track;
    private CompositeDisposable? _onBindingDisposables;
    private CompositeDisposable? _offBindingDisposables;
    private Canvas? _mainLayout;
    private IDisposable? _loadingCursorValue;
    private double _onContentInkOffsetY;
    private double _offContentInkOffsetY;
    private EventHandler? _formValueChanged;
    
    private readonly TransitionInitializationSession _transitionInitialization;

    static AbstractToggleSwitch()
    {
        AffectsMeasure<AbstractToggleSwitch>(
            SizeTypeProperty,
            OnContentProperty,
            OnContentTemplateProperty,
            OffContentProperty,
            OffContentTemplateProperty,
            TrackHeightProperty,
            TrackMinWidthProperty,
            TrackPaddingProperty,
            KnobSizeProperty,
            InnerMinMarginProperty,
            InnerMaxMarginProperty);
        ToggleInteractionFeedback.Configure<AbstractToggleSwitch>(static owner => owner, static owner => !owner.IsLoading,
            canStart: static owner => !owner.IsLoading);
        IsCheckedProperty.Changed.AddClassHandler<AbstractToggleSwitch>((toggleSwitch, args) => toggleSwitch.NotifyFormValueChanged(args.NewValue as bool?));
    }

    protected AbstractToggleSwitch()
    {
        _transitionInitialization = new TransitionInitializationSession(this);
    }

    /// <summary>
    /// Completes the transition initialization when the switch is already arranged.
    /// Until then, state changes are not animated.
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (IsArrangeValid)
        {
            CompleteTransitionInitializationMode(Bounds.Size);
        }
    }

    /// <summary>
    /// Resets the transition initialization and starts the token subscription that provides the inner content
    /// margins.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _transitionInitialization.Reset();
        base.OnAttachedToVisualTree(e);
        UpdateContentMarginTokenSubscription();
    }

    /// <summary>
    /// Releases the content margin token subscription and a pending click wave, and resets the transition
    /// initialization.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ToggleInteractionFeedback.Cancel(this);
        InteractionEffectHost.CancelFor(this);
        _contentMarginTokens?.Dispose();
        _contentMarginTokens = null;
        _transitionInitialization.Reset();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Resets the transition initialization when the template changes.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        _transitionInitialization?.Reset();
        base.OnTemplateChanged(e);
    }

    /// <summary>
    /// Looks up the template parts and applies the loading state to the knob.
    /// </summary>
    /// <remarks>
    /// A custom template can omit the bindings that the default theme declares for the knob. This method adds
    /// template bindings for the render transform and the width extension of the knob when they are not set.
    /// </remarks>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        ToggleInteractionFeedback.Cancel(this);
        InteractionEffectHost.CancelFor(this);
        _transitionInitialization.Reset();
        base.OnApplyTemplate(e);
        var scope = e.NameScope;
        _track = scope.Find<Control>("PART_Track");
        _switchKnob = scope.Find<SwitchKnob>("PART_SwitchKnob");
        if (_switchKnob is not null)
        {
            _switchKnob.KnobSize = KnobSize;
            // Custom templates can omit the bindings declared by the default theme. These fallback
            // bindings follow TemplatedParent so they release this owner when the part is replaced.
            if (!_switchKnob.IsSet(RenderTransformProperty))
            {
                _switchKnob.Bind(RenderTransformProperty, new TemplateBinding(KnobRenderTransformProperty));
                _switchKnob.SetValue(RenderTransformOriginProperty, new RelativePoint(0, 0, RelativeUnit.Relative), BindingPriority.Template);
            }
            if (!_switchKnob.IsSet(SwitchKnob.KnobWidthExtensionProperty))
            {
                _switchKnob.Bind(SwitchKnob.KnobWidthExtensionProperty, new TemplateBinding(KnobWidthExtensionProperty));
            }
        }

        _onContentPresenter  = scope.Find<ContentPresenter>("PART_OnContentPresenter");
        _offContentPresenter = scope.Find<ContentPresenter>("PART_OffContentPresenter");
        _mainLayout          = scope.Find<Canvas>("PART_MainContainer");
        HandleLoadingState(IsLoading);
    }

    /// <summary>
    /// Keeps the switch in step with property changes: it starts or stops loading, binds icon content to the
    /// switch, updates the knob size, recalculates the offsets when the checked state or the pressed state
    /// changes, and plays the click wave when the checked state changes.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsLoadingProperty)
        {
            HandleLoadingState(IsLoading);
        }
        else if (change.Property == UseContentMarginTokenResourcesProperty && this.IsAttachedToVisualTree())
        {
            UpdateContentMarginTokenSubscription();
        }
        else if (change.Property == SizeTypeProperty && _contentMarginTokens is not null)
        {
            RefreshContentMargins();
        }
        else if ((change.Property == IsPointerOverProperty && !IsLoading) ||
                 change.Property == IsCheckedProperty ||
                 change.Property == IsEnabledProperty)
        {
            if (change.Property == IsCheckedProperty)
            {
                CalculateElementsOffset(GrooveRect().Size);
            }
        }
        else if (change.Property == KnobSizeProperty)
        {
            if (_switchKnob is not null)
            {
                _switchKnob.KnobSize = KnobSize;
            }
        }

        if (change.Property == OffContentProperty ||
            change.Property == OnContentProperty)
        {
            SetupContent(change.OldValue, change.NewValue, change.Property == OnContentProperty);
        }

        if (_switchKnob is not null &&
            (change.Property == IsPressedProperty || (change.Property == EnableHandlePressStretchProperty && IsPressed)))
        {
            CalculateElementsOffset(GrooveRect().Size);
        }

        if (_mainLayout is not ToggleSwitchContentPanel)
        {
            if (change.Property == OnContentOffsetProperty)
            {
                ToggleSwitchContentPanel.SetContentOffset(_onContentPresenter, OnContentOffset);
            }
            else if (change.Property == OffContentOffsetProperty)
            {
                ToggleSwitchContentPanel.SetContentOffset(_offContentPresenter, OffContentOffset);
            }
        }
    }

    /// <summary>
    /// Measures both content presenters and the knob and returns the size of the groove.
    /// </summary>
    /// <remarks>
    /// The height is <see cref="TrackHeight" />. The width is the wider content plus the inner margins, and
    /// <see cref="TrackMinWidth" /> without content; the result is never narrower than <see cref="TrackMinWidth" />. An
    /// explicit <see cref="Avalonia.Layout.Layoutable.Width" /> takes the place of the content width.
    /// </remarks>
    protected override Size MeasureOverride(Size availableSize)
    {
        base.MeasureOverride(availableSize);
        double extraInfoWidth = 0;

        if (_offContentPresenter != null)
        {
            _offContentPresenter.Measure(availableSize);
            extraInfoWidth = Math.Max(extraInfoWidth, _offContentPresenter.DesiredSize.Width);
        }

        if (_onContentPresenter != null)
        {
            _onContentPresenter.Measure(availableSize);
            extraInfoWidth = Math.Max(extraInfoWidth, _onContentPresenter.DesiredSize.Width);
        }

        _onContentInkOffsetY = CalculateContentInkOffsetY(_onContentPresenter);
        _offContentInkOffsetY = CalculateContentInkOffsetY(_offContentPresenter);

        var switchHeight  = TrackHeight;
        // Without content the inner margins do not add to the width, so the width falls back to TrackMinWidth.
        var switchWidth   = extraInfoWidth > 0
            ? extraInfoWidth + InnerMinMargin + InnerMaxMargin
            : extraInfoWidth;
        // An explicit Width takes the place of the measured content width, so that the knob and the groove follow the
        // width that is finally arranged.
        switchWidth = !double.IsNaN(Width)
            ? Math.Max(Width, TrackMinWidth)
            : Math.Max(switchWidth, TrackMinWidth);
        var targetSize = new Size(switchWidth, switchHeight);
        if (_mainLayout is null)
        {
            base.MeasureOverride(targetSize);
        }
        _track?.Measure(targetSize);
        if (_switchKnob is not null)
        {
            _switchKnob.Measure(KnobSize);
        }
        
        return targetSize;
    }

    /// <summary>
    /// Arranges the parts of the template, computes the knob position and the content offsets from the final
    /// size, and completes the transition initialization.
    /// </summary>
    /// <remarks>
    /// With the default <c>ToggleSwitchContentPanel</c> the panel arranges the content presenters.
    /// Only when the template provides a plain <c>Canvas</c> does the switch arrange them itself.
    /// </remarks>
    protected override Size ArrangeOverride(Size finalSize)
    {
        // The arrange size already has Width, MinWidth and MaxWidth applied and excludes the outer margin. The knob
        // and the groove must use this final geometry, not the target size that the measure pass computed before those
        // constraints were applied.
        CalculateElementsOffset(finalSize);
        if (_mainLayout is null)
        {
            base.ArrangeOverride(finalSize);
        }
        _track?.Arrange(new Rect(finalSize));
        _mainLayout?.Arrange(new Rect(finalSize));
        // A template that an application already uses may still provide a plain Canvas, whose layout the library
        // ControlTheme cannot take over declaratively. Only on this compatibility path does the switch arrange the
        // presenters; with the default dedicated canvas the canvas stays the only one that arranges them.
        if (_mainLayout is not ToggleSwitchContentPanel)
        {
            if (_offContentPresenter is { } offPresenter)
            {
                offPresenter.Arrange(new Rect(offPresenter.DesiredSize));
                ToggleSwitchContentPanel.SetContentOffset(offPresenter, OffContentOffset);
            }
            if (_onContentPresenter is { } onPresenter)
            {
                onPresenter.Arrange(new Rect(onPresenter.DesiredSize));
                ToggleSwitchContentPanel.SetContentOffset(onPresenter, OnContentOffset);
            }
        }
        _switchKnob?.Arrange(new Rect(KnobSize));


        CompleteTransitionInitializationMode(finalSize);
        
        return finalSize;
    }

    // The focus outline follows the explicit track corner radius when set, or defaults to a full capsule pill.
    CornerRadius IFocusOutlineShapeProvider.FocusOutlineCornerRadius =>
        TrackCornerRadius is { } trackCornerRadius
            ? CornerRadiusUtils.ClampToSize(trackCornerRadius, Bounds.Width, Bounds.Height)
            : new CornerRadius(Math.Min(Bounds.Width, Bounds.Height) / 2);


    /// <summary>
    /// Returns whether a point lies inside the groove of the switch.
    /// The result is always <see langword="false" /> while the switch is disabled or loading.
    /// </summary>
    /// <param name="point">
    /// The point to test, relative to the top-left corner of the switch.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the point lies inside the groove; otherwise <see langword="false" />.
    /// </returns>
    public bool HitTest(Point point)
    {
        if (!IsEnabled || IsLoading)
        {
            return false;
        }

        var grooveRect = GrooveRect();
        return grooveRect.Contains(point);
    }

    /// <summary>
    /// Ignores the pointer press while the switch is loading; otherwise it runs the Avalonia handling.
    /// </summary>
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        if (!IsLoading)
        {
            base.OnPointerPressed(e);
        }
    }

    /// <summary>
    /// Ignores the pointer release while the switch is loading; otherwise it runs the Avalonia handling.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        if (!IsLoading)
        {
            base.OnPointerReleased(e);
        }
    }

    #region 实现 FormItem 接口

    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value as bool?);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);

    /// <summary>
    /// Called after <see cref="ToggleButton.IsChecked" /> has changed.
    /// </summary>
    /// <param name="value">
    /// The new checked state.
    /// </param>
    /// <remarks>
    /// The default implementation raises the <c>ValueChanged</c> event of the form item that hosts the switch.
    /// An override that keeps the form item informed must call the base method.
    /// </remarks>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the switch sets or resets its value.
    /// The default implementation assigns <paramref name="value" /> to <see cref="ToggleButton.IsChecked" /> while
    /// keeping an existing binding on it.
    /// </summary>
    /// <param name="value">
    /// The checked state to apply; <see langword="null" /> clears the checked state. A form value that is not a
    /// <see cref="bool" /> arrives here as <see langword="null" />.
    /// </param>
    protected virtual void NotifySetFormValue(bool? value)
    {
        SetCurrentValue(IsCheckedProperty, value);
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the switch reads its value.
    /// </summary>
    /// <returns>
    /// The value of <see cref="ToggleButton.IsChecked" />.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return IsChecked;
    }

    /// <summary>
    /// Called when the <c>FormItem</c> that hosts the switch resets and has no initial value.
    /// The default implementation sets <see cref="ToggleButton.IsChecked" /> to <see langword="null" /> while
    /// keeping an existing binding on it.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
        SetCurrentValue(IsCheckedProperty, null);
    }

    /// <summary>
    /// Called when the validation status of the <c>FormItem</c> that hosts the switch changes, for example
    /// after a validation run. The default implementation does nothing.
    /// </summary>
    /// <param name="status">
    /// The status that the form item now shows.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
    }

    #endregion

    #region 实现 ITransitionInitializationOwner 接口

    TransitionInitializationSession ITransitionInitializationOwner.TransitionInitialization => _transitionInitialization;

    #endregion

    private void CompleteTransitionInitializationMode(Size size)
    {
        if (_switchKnob is { IsArrangeValid: true } &&
            double.IsFinite(size.Width) && double.IsFinite(size.Height) &&
            MathUtils.GreaterThan(size.Width, 0) && MathUtils.GreaterThan(size.Height, 0))
        {
            _transitionInitialization.CompleteInitialization();
        }
    }

    private void SetupContent(object? oldContent, object? newContent, bool isOnContent)
    {
        if (oldContent != null)
        {
            if (isOnContent)
            {
                _onBindingDisposables?.Dispose();
                _onBindingDisposables = null;
            }
            else
            {
                _offBindingDisposables?.Dispose();
                _offBindingDisposables = null;
            }
        }
        if (newContent is PathIcon newPathIcon)
        {
            var disposables = new CompositeDisposable(4);
            disposables.Add(BindUtils.RelayBind(this, IconSizeProperty, newPathIcon, WidthProperty));
            disposables.Add(BindUtils.RelayBind(this, IconSizeProperty, newPathIcon, HeightProperty));
            disposables.Add(BindUtils.RelayBind(this, ForegroundProperty, newPathIcon, ForegroundProperty));
            if (newPathIcon is Icon icon)
            {
                disposables.Add(BindUtils.RelayBind(this, ForegroundProperty, icon, Icon.FillBrushProperty));
                disposables.Add(BindUtils.RelayBind(this, ForegroundProperty, icon, Icon.StrokeBrushProperty));
            }
            if (isOnContent)
            {
                _onBindingDisposables = disposables;
            }
            else
            {
                _offBindingDisposables = disposables;
            }
        }
    }

    private void CalculateElementsOffset(Size controlSize)
    {
        var isChecked = IsChecked.HasValue && IsChecked.Value;
        var stretchHandle = IsPressed && EnableHandlePressStretch;
        var handleWidth = stretchHandle ? KnobSize.Width * STRETCH_FACTOR : KnobSize.Width;
        var knobTarget = HandleRect(isChecked, controlSize, handleWidth);
        KnobWidthExtension = handleWidth - KnobSize.Width;
        if (KnobRenderTransform is null || _knobTarget != knobTarget)
        {
            _knobTarget = knobTarget;
            var builder = new TransformOperations.Builder(1);
            builder.AppendTranslate(knobTarget.X, knobTarget.Y);
            KnobRenderTransform = builder.Build();
        }

        var onExtraInfoRect  = ExtraInfoRect(true, controlSize);
        var offExtraInfoRect = ExtraInfoRect(false, controlSize);
        var pressedOffset = stretchHandle ? knobTarget.Width * (STRETCH_FACTOR - 1) : 0;
        if (isChecked)
        {
            OnContentOffset  = new Point(onExtraInfoRect.X - pressedOffset, onExtraInfoRect.Y);
            OffContentOffset = new Point(controlSize.Width + 1, onExtraInfoRect.Top);
        }
        else
        {
            OffContentOffset = new Point(offExtraInfoRect.X + pressedOffset, offExtraInfoRect.Y);
            OnContentOffset  = new Point(-offExtraInfoRect.Width, offExtraInfoRect.Top);
        }
    }


    private void UpdateContentMarginTokenSubscription()
    {
        if (UseContentMarginTokenResources && ContentMarginKeys is { } keys)
        {
            if (_contentMarginTokens is null)
            {
                _contentMarginTokens = new OwnerTokenSet(this, 4, RefreshContentMargins);
                _contentMarginTokens.SetResourceKey(0, keys.NormalMin);
                _contentMarginTokens.SetResourceKey(1, keys.NormalMax);
                _contentMarginTokens.SetResourceKey(2, keys.SmallMin);
                _contentMarginTokens.SetResourceKey(3, keys.SmallMax);
            }
        }
        else if (_contentMarginTokens is not null)
        {
            _contentMarginTokens.Dispose();
            _contentMarginTokens = null;
        }
        RefreshContentMargins();
    }

    private void RefreshContentMargins()
    {
        var previous = (InnerMinMargin, InnerMaxMargin);
        var index = SizeType == CustomizableSizeType.Small ? 2 : 0;
        _contentMargins = (ReadMargin(index, _contentMargins.Min), ReadMargin(index + 1, _contentMargins.Max));
        if (!previous.Equals((InnerMinMargin, InnerMaxMargin)))
        {
            InvalidateMeasure();
        }
    }

    private double ReadMargin(int index, double previous)
    {
        var value = _contentMarginTokens?.GetValue(index);
        if (value is double margin)
        {
            return margin;
        }
        if (ReferenceEquals(value, BindingOperations.DoNothing))
        {
            return previous;
        }
        try
        {
            return value is IConvertible convertible ? convertible.ToDouble(CultureInfo.CurrentCulture) : 0;
        }
        catch (Exception error) when (error is FormatException or InvalidCastException or OverflowException)
        {
            return 0;
        }
    }

    private void HandleLoadingState(bool isLoading)
    {
        if (isLoading)
        {
            _loadingCursorValue ??= SetValue(CursorProperty, Cursor.Default, BindingPriority.Template);
            _switchKnob?.NotifyStartLoading();
        }
        else
        {
            _loadingCursorValue?.Dispose();
            _loadingCursorValue = null;
            _switchKnob?.NotifyStopLoading();
        }
    }

    private Rect GrooveRect()
    {
        return new Rect(Bounds.Size);
    }

    private Rect HandleRect(bool isChecked, Size controlSize, double handleWidth)
    {
        double handlePosX;
        double handlePosY;
        var    offsetX    = TrackPadding;
        var    offsetY    = TrackPadding;

        if (!isChecked)
        {
            handlePosX = offsetX;
            handlePosY = offsetY;
        }
        else
        {
            handlePosX = controlSize.Width - offsetX - handleWidth;
            handlePosY = offsetY;
        }

        return new Rect(handlePosX, handlePosY, handleWidth, KnobSize.Height);
    }

    private Rect ExtraInfoRect(bool isChecked, Size controlSize)
    {
        var    innerMinMargin = InnerMinMargin;
        var    innerMaxMargin = InnerMaxMargin;
        double yAdjustValue   = 0;
        var    targetRect     = new Rect(new Point(0, 0), controlSize);
        if (isChecked)
        {
            if (_onContentPresenter != null)
            {
                yAdjustValue = (controlSize.Height - _onContentPresenter.DesiredSize.Height) / 2 + _onContentInkOffsetY;
            }

            targetRect = targetRect.Inflate(new Thickness(-innerMinMargin, -yAdjustValue, innerMaxMargin, 0));
        }
        else
        {
            if (_offContentPresenter != null)
            {
                yAdjustValue = (controlSize.Height - _offContentPresenter.DesiredSize.Height) / 2 + _offContentInkOffsetY;
            }

            targetRect = targetRect.Inflate(new Thickness(-innerMaxMargin, -yAdjustValue, innerMinMargin, 0));
        }

        return targetRect;
    }

    private static double CalculateContentInkOffsetY(ContentPresenter? presenter)
    {
        if (presenter is not { Content: string, ContentTemplate: null, Child: TextBlock textBlock } ||
            presenter.FindDataTemplate(presenter.Content) is not null)
        {
            return 0;
        }

        // A plain string label is centered on its visible glyphs: the line box of the font reserves room for
        // descenders, so centering the line box puts labels such as On / Off too high. A custom template keeps its
        // own layout. The real glyphs, the fallback fonts and the shaping offsets are results of the run-time text
        // layout, which a ControlTheme cannot express. The existing text layout is reused and only the ink bounds are
        // extracted in the measure pass; the arrange pass of every animation frame builds no Geometry.
        // GlyphRun.InkBounds is a conservative bounding box and GlyphMetrics may provide only the advance, so neither
        // can replace the visible outline.
        var layout = textBlock.TextLayout;
        var inkTop = double.PositiveInfinity;
        var inkBottom = double.NegativeInfinity;
        var lineTop = 0d;
        foreach (var line in layout.TextLines)
        {
            foreach (var textRun in line.TextRuns)
            {
                if (textRun is not ShapedTextRun run)
                {
                    continue;
                }

                var inkBounds = CalculateGlyphRunInkBounds(run.GlyphRun);
                if (MathUtils.GreaterThan(inkBounds.Height, 0))
                {
                    var baselineOffset = lineTop + line.Baseline - run.Baseline;
                    inkTop = Math.Min(inkTop, baselineOffset + inkBounds.Top);
                    inkBottom = Math.Max(inkBottom, baselineOffset + inkBounds.Bottom);
                }
            }

            lineTop += line.Height;
        }

        if (double.IsFinite(inkTop) && double.IsFinite(inkBottom))
        {
            return (layout.Height - inkTop - inkBottom) / 2;
        }

        return 0;
    }

    private static Rect CalculateGlyphRunInkBounds(GlyphRun glyphRun)
    {
        if (glyphRun.GlyphInfos.All(glyph => MathUtils.IsZero(glyph.GlyphOffset.Y)))
        {
            return glyphRun.BuildGeometry().Bounds;
        }

        // In Avalonia 12 the Skia outline is built without the GlyphOffset, while drawing applies it. With a vertical
        // shaping offset the baseline is therefore set per glyph, so that the outline bounds match the drawn position.
        var bounds = default(Rect);
        foreach (var glyph in glyphRun.GlyphInfos)
        {
            using var single = new GlyphRun(glyphRun.GlyphTypeface, glyphRun.FontRenderingEmSize,
                glyphRun.Characters, [new GlyphInfo(glyph.GlyphIndex, 0, glyph.GlyphAdvance)],
                new Point(0, glyphRun.BaselineOrigin.Y + glyph.GlyphOffset.Y));
            var glyphBounds = single.BuildGeometry().Bounds;
            if (MathUtils.GreaterThan(glyphBounds.Height, 0))
            {
                bounds = bounds.Union(glyphBounds);
            }
        }

        return bounds;
    }
}
