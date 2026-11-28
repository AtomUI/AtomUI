using AtomUI.Animations;
using System.Reactive.Disposables;
using System.Windows.Input;
using AtomUI.Controls;
using AtomUI.Controls.Utils;
using AtomUI.Data;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Icons.AntDesign;
using AtomUI.Theme;
using AtomUI.Theme.Compilation;
using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// A button whose primary part acts as a regular button and whose secondary part opens a <see cref="Flyout" />
/// anchored at it. The primary part shows <see cref="ContentControl.Content" /> and <see cref="Icon" />, the
/// secondary part shows <see cref="OpenIndicator" />.
/// </summary>
public partial class SplitButton : ContentControl,
                           ICommandSource, 
                           ICustomizableSizeTypeAware,
                           IInteractionEffectAwareControl,
                           ICompactSpaceAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="Click" /> routed event.
    /// </summary>
    public static readonly RoutedEvent<RoutedEventArgs> ClickEvent =
        RoutedEvent.Register<SplitButton, RoutedEventArgs>(
            nameof(Click),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Identifies the <see cref="Command" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand?> CommandProperty =
        Avalonia.Controls.Button.CommandProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="CommandParameter" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> CommandParameterProperty =
        Avalonia.Controls.Button.CommandParameterProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="Flyout" /> property.
    /// </summary>
    public static readonly StyledProperty<Flyout?> FlyoutProperty =
        AvaloniaProperty.Register<SplitButton, Flyout?>(nameof(Flyout));

    /// <summary>
    /// Identifies the <see cref="HotKey" /> property.
    /// </summary>
    public static readonly StyledProperty<KeyGesture?> HotKeyProperty =
        Avalonia.Controls.Button.HotKeyProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="TriggerType" /> property.
    /// </summary>
    public static readonly StyledProperty<FlyoutTriggerType> TriggerTypeProperty =
        FlyoutStateHelper.TriggerTypeProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Flyout.IsPointAtCenterProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="Placement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> PlacementProperty =
        Popup.PlacementProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="PlacementAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupAnchor> PlacementAnchorProperty =
        Popup.PlacementAnchorProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="PlacementGravity" /> property.
    /// </summary>
    public static readonly StyledProperty<PopupGravity> PlacementGravityProperty =
        Popup.PlacementGravityProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="GutterToFlyout" /> property.
    /// </summary>
    public static readonly StyledProperty<double> GutterToFlyoutProperty =
        AvaloniaProperty.Register<SplitButton, double>(nameof(GutterToFlyout));

    /// <summary>
    /// Identifies the <see cref="MouseEnterDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseEnterDelayProperty =
        FlyoutStateHelper.MouseEnterDelayProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="MouseLeaveDelay" /> property.
    /// </summary>
    public static readonly StyledProperty<int> MouseLeaveDelayProperty =
        FlyoutStateHelper.MouseLeaveDelayProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="Icon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> IconProperty =
        Button.IconProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="OpenIndicator" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> OpenIndicatorProperty =
        AvaloniaProperty.Register<SplitButton, PathIcon?>(nameof(OpenIndicator));

    /// <summary>
    /// Identifies the <see cref="IsDanger" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsDangerProperty =
        Button.IsDangerProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="IsPrimaryButtonType" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPrimaryButtonTypeProperty =
        AvaloniaProperty.Register<SplitButton, bool>(nameof(IsPrimaryButtonType));
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="ShouldUseOverlayPopup" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        Flyout.ShouldUseOverlayPopupProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Flyout.IsPopupPinnedOpenProperty.AddOwner<SplitButton>();

    /// <summary>
    /// Identifies the <see cref="IsInteractionEffectEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInteractionEffectEnabledProperty =
        InteractionEffectAwareControlProperty.IsInteractionEffectEnabledProperty.AddOwner<SplitButton>();
    
    /// <summary>
    /// The motion the flyout plays when it opens. The value is relayed to the flyout; without a local value the
    /// theme supplies <c>PopupMotionDefaults.DropdownOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// The motion the flyout plays when it closes. The value is relayed to the flyout; without a local value the
    /// theme supplies <c>PopupMotionDefaults.DropdownClose</c>. It is skipped while
    /// <see cref="IsMotionEnabled" /> is <see langword="false" />.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Occurs when the primary part is activated, by a click or by releasing Space or Enter, while the control is
    /// effectively enabled. It is raised before <see cref="Command" /> runs, and a handler that marks the event
    /// handled prevents the command from running.
    /// </summary>
    public event EventHandler<RoutedEventArgs>? Click
    {
        add => AddHandler(ClickEvent, value);
        remove => RemoveHandler(ClickEvent, value);
    }

    /// <summary>
    /// The command that runs when the primary part is activated and the <see cref="Click" /> event was not marked
    /// handled. It runs only while it can execute with <see cref="CommandParameter" />, and while it cannot the
    /// control is effectively disabled.
    /// </summary>
    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    /// <summary>
    /// Gets or sets a parameter to be passed to the <see cref="Command" />.
    /// </summary>
    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    /// <summary>
    /// The flyout that opens at the secondary part when the input chosen by <see cref="TriggerType" /> occurs. Its
    /// motion, placement and arrow are relayed from the properties of this control.
    /// </summary>
    public Flyout? Flyout
    {
        get => GetValue(FlyoutProperty);
        set => SetValue(FlyoutProperty, value);
    }

    /// <summary>
    /// Gets or sets an <see cref="KeyGesture" /> associated with this control
    /// </summary>
    public KeyGesture? HotKey
    {
        get => GetValue(HotKeyProperty);
        set => SetValue(HotKeyProperty, value);
    }

    /// <summary>
    /// The input that opens and closes the flyout at the secondary part: a click on it, which is the default, the
    /// pointer resting on it and leaving it, timed by <see cref="MouseEnterDelay" /> and
    /// <see cref="MouseLeaveDelay" />, or its keyboard focus. Alt+Down and F4 open the flyout regardless of this
    /// value.
    /// </summary>
    public FlyoutTriggerType TriggerType
    {
        get => GetValue(TriggerTypeProperty);
        set => SetValue(TriggerTypeProperty, value);
    }

    /// <summary>
    /// Whether the flyout draws an arrow that points at the secondary part. The value is relayed to the flyout,
    /// and the arrow is drawn only while <see cref="Placement" />, <see cref="PlacementAnchor" /> and
    /// <see cref="PlacementGravity" /> allow one. The default is <see langword="false" />.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Moves the flyout so that its arrow points at the center of the secondary part. The value is relayed to the
    /// flyout and has an effect only while the arrow is drawn.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// Where the flyout is placed relative to the secondary part. The value is relayed to the flyout as its
    /// requested placement. The default is <c>BottomEdgeAlignedRight</c>.
    /// </summary>
    public PlacementMode Placement
    {
        get => GetValue(PlacementProperty);
        set => SetValue(PlacementProperty, value);
    }

    /// <summary>
    /// The direction in which the flyout opens from <see cref="PlacementAnchor" />, refining
    /// <see cref="Placement" />. The value is relayed to the flyout and also decides whether the arrow can be
    /// drawn.
    /// </summary>
    public PopupGravity PlacementGravity
    {
        get => GetValue(PlacementGravityProperty);
        set => SetValue(PlacementGravityProperty, value);
    }

    /// <summary>
    /// The point on the secondary part at which the flyout is placed, refining <see cref="Placement" />. The value
    /// is relayed to the flyout and also decides whether the arrow can be drawn.
    /// </summary>
    public PopupAnchor PlacementAnchor
    {
        get => GetValue(PlacementAnchorProperty);
        set => SetValue(PlacementAnchorProperty, value);
    }

    /// <summary>
    /// The gap between the control and its flyout, in DIPs. The theme takes the value from the
    /// <c>GutterToFlyout</c> token and the value is relayed to the flyout as its margin to the anchor.
    /// </summary>
    public double GutterToFlyout
    {
        get => GetValue(GutterToFlyoutProperty);
        set => SetValue(GutterToFlyoutProperty, value);
    }

    /// <summary>
    /// The time in milliseconds the pointer must rest on the secondary part before the flyout opens. It applies to
    /// the hover trigger only; 0 opens the flyout at once. The default is 200.
    /// </summary>
    public int MouseEnterDelay
    {
        get => GetValue(MouseEnterDelayProperty);
        set => SetValue(MouseEnterDelayProperty, value);
    }

    /// <summary>
    /// The time in milliseconds the flyout stays open after the pointer has left both the secondary part and the
    /// flyout. It applies to the hover trigger only; 0 closes the flyout at once. The default is 200.
    /// </summary>
    public int MouseLeaveDelay
    {
        get => GetValue(MouseLeaveDelayProperty);
        set => SetValue(MouseLeaveDelayProperty, value);
    }

    /// <summary>
    /// The size both parts are drawn at: it is forwarded to the two buttons of the template and selects the corner
    /// radius of the control. With <see cref="CustomizableSizeType.Custom" />, the <c>Height</c>,
    /// <c>MinHeight</c>, <c>Width</c>, <c>MinWidth</c>, <c>Padding</c> and <c>FontSize</c> set on this control are
    /// applied to both buttons as well.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// The icon of the primary part, forwarded to its button. <see langword="null" /> shows no icon.
    /// </summary>
    public PathIcon? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>
    /// The icon of the secondary part, forwarded to its button. A value that is still <see langword="null" />
    /// when the control is initialized is replaced with the default ellipsis icon.
    /// </summary>
    public PathIcon? OpenIndicator
    {
        get => GetValue(OpenIndicatorProperty);
        set => SetValue(OpenIndicatorProperty, value);
    }

    /// <summary>
    /// Draws both parts in the danger style of their button type. In the primary form it also switches the seam
    /// separator to the danger separator color.
    /// </summary>
    public bool IsDanger
    {
        get => GetValue(IsDangerProperty);
        set => SetValue(IsDangerProperty, value);
    }

    /// <summary>
    /// Draws both parts as primary buttons joined by a seam separator, instead of two adjacent default buttons.
    /// The default is <see langword="false" />.
    /// </summary>
    public bool IsPrimaryButtonType
    {
        get => GetValue(IsPrimaryButtonTypeProperty);
        set => SetValue(IsPrimaryButtonTypeProperty, value);
    }
    
    /// <summary>
    /// Switches the motion of the control on or off: the color transitions and the wave animation of both parts,
    /// and the open and close motion of the flyout. The value is forwarded to both buttons and relayed to the
    /// flyout. The theme takes it from the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Enables the interaction feedback that both parts play when they are interacted with; it runs only while
    /// <see cref="IsMotionEnabled" /> is <see langword="true" />. The value is forwarded to both buttons, and the
    /// theme takes it from the <c>EnableInteractionEffect</c> token.
    /// </summary>
    public bool IsInteractionEffectEnabled
    {
        get => GetValue(IsInteractionEffectEnabledProperty);
        set => SetValue(IsInteractionEffectEnabledProperty, value);
    }

    /// <summary>
    /// Whether the flyout is shown in the overlay layer of the window instead of a native popup window. The value
    /// is relayed to the flyout, which also uses the overlay layer on platforms without native popup windows. The
    /// default is <see langword="true" />.
    /// </summary>
    public bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the flyout popup stays pinned open. Used by
    /// Gallery semantic part previews to keep the cross-visual-root popup materialized.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    /// <summary>
    /// Identifies the <see cref="SplitSeparatorBrush" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SplitSeparatorBrushProperty =
        AvaloniaProperty.Register<SplitButton, IBrush?>(nameof(SplitSeparatorBrush));

    /// <summary>
    /// Identifies the <see cref="EffectiveButtonType" /> property.
    /// </summary>
    public static readonly DirectProperty<SplitButton, ButtonType> EffectiveButtonTypeProperty =
        AvaloniaProperty.RegisterDirect<SplitButton, ButtonType>(nameof(EffectiveButtonType),
            o => o.EffectiveButtonType,
            (o, v) => o.EffectiveButtonType = v);

    /// <summary>
    /// Brush of the vertical divider between the primary and secondary action buttons of the split button.
    /// </summary>
    public IBrush? SplitSeparatorBrush
    {
        get => GetValue(SplitSeparatorBrushProperty);
        set => SetValue(SplitSeparatorBrushProperty, value);
    }

    private ButtonType _effectiveButtonType;

    /// <summary>
    /// Effective <see cref="ButtonType" /> forwarded to the child action buttons in the split button template.
    /// </summary>
    public ButtonType EffectiveButtonType
    {
        get => _effectiveButtonType;
        internal set => SetAndRaise(EffectiveButtonTypeProperty, ref _effectiveButtonType, value);
    }

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<bool> UseInsetSeparatorProperty =
        AvaloniaProperty.Register<SplitButton, bool>(nameof(UseInsetSeparator));
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<SplitButton>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<SplitButton>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty =
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<SplitButton>();

    internal bool UseInsetSeparator => GetValue(UseInsetSeparatorProperty);
    
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

    /// <summary>
    /// Whether the seam separator of the primary form is currently painted in its contrast color. It follows the
    /// solid combination rule of upstream Ant Design: the separator takes the hover background token of the
    /// secondary button (<c>colorPrimaryHover</c> or <c>colorErrorHover</c>). While the pointer is over the
    /// secondary button, which holds the separator, the seam therefore has the same color as the hover background
    /// of the button and the separator is visually hidden. The hiding must emerge from these colors alone: never
    /// change the geometry of the secondary button for it. A left shift that covered the separator was tried once
    /// and caused the button to shift on hover and its edges to flicker.
    /// </summary>
    internal bool IsSeparatorVisible =>
        IsPrimaryButtonType && _secondaryButton is not null &&
        (UseInsetSeparator
            ? SplitSeparatorBrush is not (null or ISolidColorBrush { Color.A: 0 })
            : !_isPointerOverSecondaryButton);

    #endregion
    
    private Button? _primaryButton;
    private Button? _secondaryButton;
    private KeyGesture? _hotkey;

    private bool _commandCanExecute = true;
    private bool _isFlyoutOpen;
    private bool _isKeyboardPressed;
    private bool _isPointerOverSecondaryButton;
    private bool _isPinnedOpenRetryArmed;
    private readonly FlyoutStateHelper _flyoutStateHelper;
    
    private CompositeDisposable? _flyoutBindingDisposables;
    private int _pinnedOpenGeneration;
    private Flyout? _registeredFlyout;
    private IDisposable? _themeSubscription;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    static SplitButton()
    {
        PlacementProperty.OverrideDefaultValue<SplitButton>(PlacementMode.BottomEdgeAlignedRight);
        IsArrowVisibleProperty.OverrideDefaultValue<SplitButton>(false);
        HorizontalAlignmentProperty.OverrideDefaultValue<SplitButton>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<SplitButton>(VerticalAlignment.Top);
        AffectsMeasure<SplitButton>(SizeTypeProperty);
        AffectsArrange<SplitButton>(IsPrimaryButtonTypeProperty, BorderThicknessProperty, UseLayoutRoundingProperty, UseInsetSeparatorProperty);
    }

    public SplitButton()
    {
        _flyoutStateHelper = new FlyoutStateHelper();
        _flyoutStateHelper[!FlyoutStateHelper.FlyoutProperty]          = this[!FlyoutProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseEnterDelayProperty] = this[!MouseEnterDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.MouseLeaveDelayProperty] = this[!MouseLeaveDelayProperty];
        _flyoutStateHelper[!FlyoutStateHelper.TriggerTypeProperty]     = this[!TriggerTypeProperty];
    }

    internal virtual bool InternalIsChecked => false;
    /// <summary>
    /// Also disables the control while <see cref="Command" /> cannot execute with
    /// <see cref="CommandParameter" />.
    /// </summary>
    protected override bool IsEnabledCore => base.IsEnabledCore && _commandCanExecute;

    /// <summary>
    /// Calls the base method and then replaces a <see langword="null" /> <see cref="OpenIndicator" /> with the
    /// default ellipsis icon.
    /// </summary>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (OpenIndicator == null)
        {
            SetCurrentValue(OpenIndicatorProperty, new EllipsisOutlined());
        }
    }

    void ICommandSource.CanExecuteChanged(object sender, EventArgs e)
    {
        CanExecuteChanged(sender, e);
    }
    
    private void CanExecuteChanged(object? sender, EventArgs e)
    {
        var (command, parameter) = (Command, CommandParameter);
        CanExecuteChanged(command, parameter);
    }

    private void CanExecuteChanged(ICommand? command, object? parameter)
    {
        if (!this.IsAttachedToLogicalTree())
        {
            return;
        }

        var canExecute = command is null || command.CanExecute(parameter);

        if (canExecute != _commandCanExecute)
        {
            _commandCanExecute = canExecute;
            UpdateIsEffectivelyEnabled();
        }
    }

    /// <summary>
    /// Updates the visual state of the control by applying latest PseudoClasses.
    /// </summary>
    protected void UpdatePseudoClasses()
    {
        PseudoClasses.Set(StdPseudoClass.FlyoutOpen, _isFlyoutOpen);
        PseudoClasses.Set(StdPseudoClass.Pressed, _isKeyboardPressed);
        PseudoClasses.Set(StdPseudoClass.Checked, InternalIsChecked);
    }

    /// <summary>
    /// Opens the flyout of the secondary button; Alt+Down and F4 call it.
    /// </summary>
    protected void OpenFlyout()
    {
        _flyoutStateHelper.ShowFlyout();
    }

    /// <summary>
    /// Closes the secondary button's flyout.
    /// </summary>
    protected void CloseFlyout()
    {
        _flyoutStateHelper.HideFlyout();
    }

    /// <summary>
    /// Registers all flyout events.
    /// </summary>
    /// <param name="flyout">
    /// The flyout to connect events to.
    /// </param>
    private void RegisterFlyoutEvents(Flyout? flyout)
    {
        if (flyout is null || ReferenceEquals(_registeredFlyout, flyout) || _flyoutBindingDisposables?.IsDisposed == true)
        {
            return;
        }
        UnregisterFlyoutEvents(_registeredFlyout);
        if (!ReferenceEquals(flyout, Flyout))
        {
            RegisterFlyoutEvents(Flyout);
            return;
        }
        var bindings = new CompositeDisposable(14);
        var registration = new CompositeDisposable(1);
        registration.Add(Disposable.Create(() =>
        {
            try
            {
                flyout.CloseForLifecycle();
            }
            finally
            {
                try { bindings.Dispose(); }
                finally { flyout.ClearMotionConfigurationOwner(this); }
            }
        }));
        _registeredFlyout = flyout;
        _flyoutBindingDisposables = registration;
        try
        {
            flyout.SetMotionConfigurationOwner(this);
            if (registration.IsDisposed)
            {
                return;
            }
            flyout.Opened += HandleFlyoutOpened;
            flyout.Closed += HandleFlyoutClosed;
            Bind(() => BindUtils.RelayBind(this, OpenMotionProperty, flyout, AtomUI.Desktop.Controls.Flyout.OpenMotionProperty));
            Bind(() => BindUtils.RelayBind(this, CloseMotionProperty, flyout, AtomUI.Desktop.Controls.Flyout.CloseMotionProperty));
            Bind(() => BindUtils.RelayBind(this, PlacementProperty, flyout, Flyout.RequestedPlacementProperty));
            Bind(() => BindUtils.RelayBind(this, PlacementAnchorProperty, flyout));
            Bind(() => BindUtils.RelayBind(this, PlacementGravityProperty, flyout));
            Bind(() => BindUtils.RelayBind(this, IsArrowVisibleProperty, flyout));
            Bind(() => BindUtils.RelayBind(this, IsPointAtCenterProperty, flyout));
            Bind(() => BindUtils.RelayBind(this, GutterToFlyoutProperty, flyout, MenuFlyout.MarginToAnchorProperty));
            Bind(() => BindUtils.RelayBind(this, IsMotionEnabledProperty, flyout));
            Bind(() => BindUtils.RelayBind(this, ShouldUseOverlayPopupProperty, flyout));
            Bind(() => BindUtils.RelayBind(this, IsPopupPinnedOpenProperty, flyout, Flyout.IsPopupPinnedOpenProperty));
            Bind(() => flyout.GetPropertyChangedObservable(Popup.RequestedPlacementProperty).Subscribe(HandleFlyoutPlacementPropertyChanged));
            if (!registration.IsDisposed && IsPopupPinnedOpen && this.IsAttachedToVisualTree())
            {
                QueuePinnedOpen();
            }
        }
        catch
        {
            if (ReferenceEquals(_flyoutBindingDisposables, registration)) { UnregisterFlyoutEvents(flyout); }
            else { registration.Dispose(); }
            throw;
        }

        // Only replaceable runtime targets need a scoped code relay. A retired
        // lease cannot acquire more bindings after a synchronous user callback.
        void Bind(Func<IDisposable> acquire)
        {
            if (!registration.IsDisposed)
            {
                bindings.Add(acquire());
            }
        }
    }

    private void UnregisterFlyoutEvents(Flyout? flyout)
    {
        if (flyout is null || !ReferenceEquals(_registeredFlyout, flyout))
        {
            return;
        }
        flyout.Opened -= HandleFlyoutOpened;
        flyout.Closed -= HandleFlyoutClosed;
        ++_pinnedOpenGeneration;
        var registration = _flyoutBindingDisposables!;
        _registeredFlyout = null;
        _isFlyoutOpen = false;
        try
        {
            registration.Dispose();
        }
        finally
        {
            if (ReferenceEquals(_flyoutBindingDisposables, registration))
            {
                _flyoutBindingDisposables = null;
            }
        }
    }
    
    /// <summary>
    /// Binds the buttons of the new template: the handlers of the buttons of the previous template are removed
    /// and attached to <c>PART_PrimaryButton</c> and <c>PART_SecondaryButton</c>, the flyout is anchored at the
    /// secondary button, and the corner radius and the custom size values are applied to both buttons.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        
        if (_primaryButton != null)
        {
            _primaryButton.Click -= HandlePrimaryButtonClick;
        }
        if (_secondaryButton != null)
        {
            _secondaryButton.PointerEntered -= HandleSecondaryButtonPointerEntered;
            _secondaryButton.PointerExited  -= HandleSecondaryButtonPointerExited;
        }
        _primaryButton                  = e.NameScope.Find<Button>("PART_PrimaryButton");
        _secondaryButton                = e.NameScope.Find<Button>("PART_SecondaryButton");
        _flyoutStateHelper.AnchorTarget = _secondaryButton;
        ConfigureButtonCornerRadius(Bounds.Height);
        ConfigureButtonCustomSizeOverrides();
        if (_primaryButton != null)
        {
            _primaryButton.Click += HandlePrimaryButtonClick;
        }
        if (_secondaryButton != null)
        {
            _secondaryButton.PointerEntered += HandleSecondaryButtonPointerEntered;
            _secondaryButton.PointerExited  += HandleSecondaryButtonPointerExited;
        }

        if (IsPopupPinnedOpen && this.IsAttachedToVisualTree())
        {
            QueuePinnedOpen();
        }
    }
    
    /// <summary>
    /// Calls the base method and then lets the flyout trigger follow the attachment, applies the current state as
    /// pseudo classes and hooks the events and property relays on the <see cref="Flyout" />.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _flyoutStateHelper.NotifyAttachedToVisualTree();
        UpdatePseudoClasses();
        RegisterFlyoutEvents(Flyout);
    }

    /// <summary>
    /// Calls the base method and then releases the flyout hooked on attach: it is closed and its events and
    /// property relays are removed. The flyout trigger then stops following the control.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        UnregisterFlyoutEvents(_registeredFlyout);
        _flyoutStateHelper.NotifyDetachedFromVisualTree();
    }

    /// <summary>
    /// Calls the base method and then updates <c>UseInsetSeparator</c> from the theme token, restores the hot key
    /// that was cleared on detach and starts tracking whether <see cref="Command" /> can execute.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _themeSubscription = s_themeTokenResolver.Subscribe(this, ConfigureSeparator);
        ConfigureSeparator(ThemeScope.ResolveContext(this)?.Snapshot);
        
        // Control attached again, set Hotkey to create a hotkey manager for this control
        SetCurrentValue(HotKeyProperty, _hotkey);

        if (Command != null)
        {
            Command.CanExecuteChanged += CanExecuteChanged;
            CanExecuteChanged(this, EventArgs.Empty);
        }
    }
    
    /// <summary>
    /// Stops the theme subscription that maintains <c>UseInsetSeparator</c> and calls the base method; then
    /// clears the hot key so that the hot key manager releases this control and stops tracking whether
    /// <see cref="Command" /> can execute. The hot key is restored on the next attach.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        ClearValue(UseInsetSeparatorProperty);
        base.OnDetachedFromLogicalTree(e);

        // This will cause the hotkey manager to dispose the observer and the reference to this control
        _hotkey = HotKey;
        SetCurrentValue(HotKeyProperty, null);

        if (Command != null)
        {
            Command.CanExecuteChanged -= CanExecuteChanged;
        }
    }
    
    /// <summary>
    /// Reacts to property changes and then calls the base method: the enabled state follows <see cref="Command" />
    /// and <see cref="CommandParameter" />, the hooked flyout follows <see cref="Flyout" />,
    /// <see cref="IsPrimaryButtonType" /> selects the button type of both parts, and a change of the corner
    /// radius, the separator mode, the compact space position or a size property updates the two buttons of the
    /// template.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == CommandProperty)
        {
            // Must unregister events here while a reference to the old command still exists
            var (oldValue, newValue) = change.GetOldAndNewValue<ICommand?>();

            if (this.IsAttachedToLogicalTree())
            {
                if (oldValue is not null)
                {
                    oldValue.CanExecuteChanged -= CanExecuteChanged;
                }

                if (newValue is not null)
                {
                    newValue.CanExecuteChanged += CanExecuteChanged;
                }
            }

            CanExecuteChanged(newValue, CommandParameter);
        }
        else if (change.Property == CommandParameterProperty && IsLoaded)
        {
            CanExecuteChanged(Command, change.NewValue);
        }
        else if (change.Property == FlyoutProperty)
        {
            var (oldFlyout, newFlyout) = change.GetOldAndNewValue<Flyout?>();

            // Must unregister events here while a reference to the old flyout still exists
            UnregisterFlyoutEvents(oldFlyout);
            if (this.IsAttachedToVisualTree())
            {
                RegisterFlyoutEvents(newFlyout);
            }
            UpdatePseudoClasses();
        }
        else if (change.Property == IsPopupPinnedOpenProperty && this.IsAttachedToVisualTree())
        {
            if (change.GetNewValue<bool>())
            {
                QueuePinnedOpen();
            }
            else
            {
                ++_pinnedOpenGeneration;
            }
        }
        else if (change.Property == IsPrimaryButtonTypeProperty)
        {
            SetupEffectiveButtonType();
        }

        if (change.Property == CornerRadiusProperty ||
            change.Property == UseInsetSeparatorProperty ||
            change.Property == CompactSpaceItemPositionProperty ||
            change.Property == CompactSpaceOrientationProperty)
        {
            ConfigureButtonCornerRadius(Bounds.Height);
        }

        if (ShouldConfigureCustomSizeOverrides(change.Property))
        {
            ConfigureButtonCustomSizeOverrides();
        }

        base.OnPropertyChanged(change);
    }

    private void SetupEffectiveButtonType()
    {
        if (IsPrimaryButtonType)
        {
            EffectiveButtonType = ButtonType.Primary;
        }
        else
        {
            EffectiveButtonType = ButtonType.Default;
        }
    }

    private void ConfigureSeparator(ThemeSnapshot? snapshot)
    {
        SetValue(UseInsetSeparatorProperty, snapshot is not null &&
            snapshot.Registry.TryGetControl(SplitButtonTokens.Identity, out var descriptor) &&
            s_themeTokenResolver.GetOwn<bool>(snapshot, descriptor.Slot, SplitButtonTokenKind.InsetSeparator));
    }

    private void ConfigureButtonCornerRadius(double height)
    {
        var effectiveCornerRadius = CompactSpace.CalculateEffectiveCornerRadius(
            CornerRadius, 
            IsUsedInCompactSpace, 
            CompactSpaceItemPosition,
            CompactSpaceOrientation);

        if (UseInsetSeparator && height > 0)
        {
            var cap = height / 2;
            effectiveCornerRadius = new CornerRadius(
                Math.Min(effectiveCornerRadius.TopLeft, cap),
                Math.Min(effectiveCornerRadius.TopRight, cap),
                Math.Min(effectiveCornerRadius.BottomRight, cap),
                Math.Min(effectiveCornerRadius.BottomLeft, cap));
        }

        var primaryButtonCornerRadius = new CornerRadius(effectiveCornerRadius.TopLeft,
            0,
            0,
            effectiveCornerRadius.BottomLeft);
        
        var secondaryButtonCornerRadius = new CornerRadius(0,
            effectiveCornerRadius.TopRight,
            effectiveCornerRadius.BottomRight,
            0);
        
        // The split radii go to the inner Buttons' internal host radius: their public CornerRadius keeps the Button
        // theme and user values, and the template frames follow the split shape.
        _primaryButton?.SetValue(Button.HostCornerRadiusProperty, primaryButtonCornerRadius);
        _secondaryButton?.SetValue(Button.HostCornerRadiusProperty, secondaryButtonCornerRadius);
    }

    private void ConfigureButtonCustomSizeOverrides()
    {
        var useCustomOverrides = SizeType == CustomizableSizeType.Custom;

        ConfigureButtonCustomSizeOverride(_primaryButton, HeightProperty, Height, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, HeightProperty, Height, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, MinHeightProperty, MinHeight, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, MinHeightProperty, MinHeight, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, WidthProperty, Width, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, WidthProperty, Width, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, MinWidthProperty, MinWidth, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, MinWidthProperty, MinWidth, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, PaddingProperty, Padding, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, PaddingProperty, Padding, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_primaryButton, FontSizeProperty, FontSize, useCustomOverrides);
        ConfigureButtonCustomSizeOverride(_secondaryButton, FontSizeProperty, FontSize, useCustomOverrides);
    }

    private void ConfigureButtonCustomSizeOverride<T>(
        Button? button,
        StyledProperty<T> property,
        T value,
        bool useCustomOverrides)
    {
        if (button is null)
        {
            return;
        }

        if (useCustomOverrides && IsLocalValue(property))
        {
            button.SetValue(property, value);
        }
        else
        {
            button.ClearValue(property);
        }
    }

    private bool IsLocalValue(AvaloniaProperty property)
    {
        return IsSet(property);
    }

    private static bool ShouldConfigureCustomSizeOverrides(AvaloniaProperty property)
    {
        return property == SizeTypeProperty ||
               property == HeightProperty ||
               property == MinHeightProperty ||
               property == WidthProperty ||
               property == MinWidthProperty ||
               property == PaddingProperty ||
               property == FontSizeProperty;
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        var flyout     = _registeredFlyout;
        var anchor     = _secondaryButton;
        Dispatcher.Post(() =>
        {
            if (generation == _pinnedOpenGeneration &&
                IsPopupPinnedOpen &&
                this.IsAttachedToVisualTree() &&
                ReferenceEquals(flyout, _registeredFlyout) &&
                ReferenceEquals(anchor, _secondaryButton))
            {
                _flyoutStateHelper.ShowFlyout(immediately: true);
            }
        });
    }
    
    /// <summary>
    /// Calls the base method after entering the pressed state while Space or Enter is held down.
    /// </summary>
    protected override void OnKeyDown(KeyEventArgs e)
    {
        var key = e.Key;

        if (key == Key.Space || key == Key.Enter)
        {
            _isKeyboardPressed = true;
            UpdatePseudoClasses();
        }

        base.OnKeyDown(e);
    }
    
    /// <summary>
    /// Calls the base method after handling the keys of this control: releasing Space or Enter activates the
    /// primary part, Alt+Down and F4 open the flyout, and Escape closes the flyout while it is open. Each of
    /// these keys is marked handled.
    /// </summary>
    protected override void OnKeyUp(KeyEventArgs e)
    {
        var key = e.Key;

        if (key == Key.Space || key == Key.Enter)
        {
            _isKeyboardPressed = false;
            UpdatePseudoClasses();

            // Consider this a click on the primary button
            if (IsEffectivelyEnabled)
            {
                OnClickPrimary(null);
                e.Handled = true;
            }
        }
        else if (key == Key.Down && e.KeyModifiers.HasAllFlags(KeyModifiers.Alt) && IsEffectivelyEnabled)
        {
            OpenFlyout();
            e.Handled = true;
        }
        else if (key == Key.F4 && IsEffectivelyEnabled)
        {
            OpenFlyout();
            e.Handled = true;
        }
        else if (e.Key == Key.Escape && _isFlyoutOpen)
        {
            // If Flyout doesn't have focusable content, close the flyout here
            // This is the same behavior as Button
            CloseFlyout();
            e.Handled = true;
        }

        base.OnKeyUp(e);
    }

    /// <summary>
    /// Raises <see cref="Click" /> and then runs <see cref="Command" /> with <see cref="CommandParameter" />,
    /// unless a handler marked the event handled. It is called when the primary button part is clicked and when
    /// Space or Enter is released, and does nothing while the control is not effectively enabled.
    /// </summary>
    /// <param name="e">
    /// The event args of the click of the internal primary button, or <see langword="null" /> when the keyboard
    /// activated the primary part.
    /// </param>
    protected virtual void OnClickPrimary(RoutedEventArgs? e)
    {
        var (command, parameter) = (Command, CommandParameter);
        // Note: It is not currently required to check enabled status; however, this is a failsafe
        if (IsEffectivelyEnabled)
        {
            var eventArgs = new RoutedEventArgs(ClickEvent);
            RaiseEvent(eventArgs);

            if (!eventArgs.Handled && command?.CanExecute(parameter) == true)
            {
                command.Execute(parameter);
                eventArgs.Handled = true;
            }
        }
    }

    /// <summary>
    /// Invoked when the split button's flyout is opened.
    /// </summary>
    protected virtual void OnFlyoutOpened()
    {
        // Available for derived types
    }

    /// <summary>
    /// Invoked when the split button's flyout is closed.
    /// </summary>
    protected virtual void OnFlyoutClosed()
    {
        // Available for derived types
    }

    /// <summary>
    /// Event handler for when the internal primary button part is clicked.
    /// </summary>
    private void HandlePrimaryButtonClick(object? sender, RoutedEventArgs e)
    {
        // Handle internal button click, so it won't bubble outside together with SplitButton.ClickEvent.
        e.Handled = true;
        OnClickPrimary(e);
    }

    /// <summary>
    /// Refreshes the pseudo classes when the <c>RequestedPlacement</c> of the flyout changes.
    /// </summary>
    private void HandleFlyoutPlacementPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Event handler for when the split button's flyout is opened.
    /// </summary>
    private void HandleFlyoutOpened(object? sender, EventArgs e)
    {
        var flyout = sender as Flyout;

        // It is possible to share flyouts among multiple controls including SplitButton.
        // This can cause a problem here since all controls that share a flyout receive
        // the same Opened/Closed events at the same time.
        // For SplitButton that means they all would be updating their pseudoclasses accordingly.
        // In other words, all SplitButtons with a shared Flyout would have the backgrounds changed together.
        // To fix this, only continue here if the Flyout target belongs to this SplitButton.
        if (IsOwnFlyoutTarget(flyout))
        {
            _isFlyoutOpen = true;
            UpdatePseudoClasses();
            OnFlyoutOpened();
        }
    }

    /// <summary>
    /// Event handler for when the split button's flyout is closed.
    /// </summary>
    private void HandleFlyoutClosed(object? sender, EventArgs e)
    {
        var flyout = sender as Flyout;

        // See comments in HandleFlyoutOpened
        if (IsOwnFlyoutTarget(flyout))
        {
            _isFlyoutOpen = false;
            UpdatePseudoClasses();
            OnFlyoutClosed();
            if (IsPopupPinnedOpen && this.IsAttachedToVisualTree())
            {
                QueuePinnedOpenRetry();
            }
        }
    }

    // Closing the flyout (the upstream HideCore) clears its Target and the PlacementTarget and the logical parent
    // of the popup, which also tears down the pinned-open recovery monitoring on the popup side. A pinned flyout
    // can only be closed because its placement target left the lifecycle (the tab was switched away or it
    // scrolled out of the viewport). The owner keeps the pin intent: once the anchor is visible again, a new
    // ShowAt rebuilds the whole chain.
    private void QueuePinnedOpenRetry()
    {
        if (_isPinnedOpenRetryArmed)
        {
            return;
        }

        _isPinnedOpenRetryArmed = true;
        LayoutUpdated += HandlePinnedOpenRetryLayoutUpdated;
    }

    private void HandlePinnedOpenRetryLayoutUpdated(object? sender, EventArgs e)
    {
        if (_isFlyoutOpen ||
            !IsPopupPinnedOpen ||
            !this.IsAttachedToVisualTree())
        {
            DisarmPinnedOpenRetry();
            return;
        }

        if (IsEffectivelyVisible && _secondaryButton is { IsEffectivelyVisible: true })
        {
            DisarmPinnedOpenRetry();
            QueuePinnedOpen();
        }
    }

    private void DisarmPinnedOpenRetry()
    {
        if (!_isPinnedOpenRetryArmed)
        {
            return;
        }

        _isPinnedOpenRetryArmed = false;
        LayoutUpdated -= HandlePinnedOpenRetryLayoutUpdated;
    }

    private bool IsOwnFlyoutTarget(Flyout? flyout)
    {
        var target = flyout?.Target;
        return ReferenceEquals(target, this) ||
               ReferenceEquals(target, _secondaryButton);
    }

    /// <summary>
    /// Arranges the content with the base method and then places the secondary button against the right edge of
    /// the primary button. Without <see cref="IsPrimaryButtonType" /> it overlaps the primary button by its own
    /// left border thickness; with it, a gap as wide as the left border of this control is left between the two
    /// for the seam that <see cref="Render" /> paints. With the inset separator in the primary form the template
    /// already reserves that gap, so only the corner radius of both buttons is updated.
    /// </summary>
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = base.ArrangeOverride(finalSize);

        if (UseInsetSeparator)
        {
            ConfigureButtonCornerRadius(size.Height);
            if (IsPrimaryButtonType)
            {
                // The template already reserves the width for the visible separator.
                return size;
            }
        }

        if (_primaryButton is not null && _secondaryButton is not null)
        {
            var originRect = _secondaryButton.Bounds;
            var secondaryLeft = _primaryButton.Bounds.Right;
            if (!IsPrimaryButtonType)
            {
                secondaryLeft -= _secondaryButton.BorderThickness.Left;
            }
            else
            {
                secondaryLeft += BorderUtils.BuildRenderScaleAwareThickness(this, BorderThickness.Left);
            }

            _secondaryButton.Arrange(new Rect(
                secondaryLeft,
                originRect.Y,
                Math.Max(0, originRect.Right - secondaryLeft),
                originRect.Height));
        }

        return size;
    }

    private void HandleSecondaryButtonPointerEntered(object? sender, PointerEventArgs e)
    {
        SetSeparatorVisibility(false);
    }

    private void HandleSecondaryButtonPointerExited(object? sender, PointerEventArgs e)
    {
        SetSeparatorVisibility(true);
    }

    private void SetSeparatorVisibility(bool visible)
    {
        // The hover hiding of the separator emerges from the colors (the separator color equals the hover background
        // token of the secondary button), so this state flip drives neither layout nor a redraw. Changing the
        // geometry of the secondary button for it would cause a visible shift and edge flicker.
        _isPointerOverSecondaryButton = !visible;
    }

    void ICompactSpaceAware.NotifyPositionChange(SpaceItemPosition? position)
    {
        IsUsedInCompactSpace     = position != null;
        CompactSpaceItemPosition = position;
    }

    void ICompactSpaceAware.NotifyOrientationChange(Orientation orientation)
    {
        CompactSpaceOrientation = orientation;
    }
    
    double ICompactSpaceAware.GetBorderThickness() => GetBorderThicknessForCompactSpace();

    /// <summary>
    /// Called through <c>ICompactSpaceAware</c> while the control sits in a compact <c>Space</c>: it returns the
    /// left border thickness in a horizontal space and the top border thickness in a vertical one, and 0 while
    /// the control is not used in a compact space.
    /// </summary>
    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        return CompactSpaceOrientation == Orientation.Horizontal ? BorderThickness.Left : BorderThickness.Top;
    }
}
