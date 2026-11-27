using AtomUI.Animations;
using AtomUI.Controls;
using AtomUI.Data;
using AtomUI.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls.Primitives;

/// <summary>
/// Base class of input controls that open a picker in a popup, such as the date, time and color pickers.
/// It provides the input box, the add-on decorated box, the popup and its open state, and the form item
/// integration; a derived control supplies the content of the popup by implementing
/// <see cref="CreatePickerPresenter" />.
/// </summary>
[PseudoClasses(InfoPickerPseudoClass.Choosing, InfoPickerPseudoClass.FlyoutOpen)]
public abstract class InfoPickerInput : TemplatedControl,
                                        IMotionAwareControl,
                                        ICompactSpaceAware,
                                        IFormItemAware,
                                        IInputControlStatusAware,
                                        IInputControlStyleVariantAware,
                                        ICustomizableSizeTypeAware,
                                        IFormItemFeedbackAware
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        Popup.OpenMotionProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        Popup.CloseMotionProperty.AddOwner<InfoPickerInput>();
    /// <summary>
    /// Identifies the <see cref="LeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> LeftAddOnProperty =
        AddOnDecoratedBox.LeftAddOnProperty.AddOwner<InfoPickerInput>();
    
    /// <summary>
    /// Identifies the <see cref="LeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> LeftAddOnTemplateProperty =
        AddOnDecoratedBox.LeftAddOnTemplateProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="RightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> RightAddOnProperty =
        AddOnDecoratedBox.RightAddOnProperty.AddOwner<InfoPickerInput>();
    
    /// <summary>
    /// Identifies the <see cref="RightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> RightAddOnTemplateProperty =
        AddOnDecoratedBox.RightAddOnTemplateProperty.AddOwner<InfoPickerInput>();
    
    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentLeftAddOnProperty =
        AddOnDecoratedBox.ContentLeftAddOnProperty.AddOwner<InfoPickerInput>();
    
    /// <summary>
    /// Identifies the <see cref="ContentLeftAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentLeftAddOnTemplateProperty =
        AddOnDecoratedBox.ContentLeftAddOnTemplateProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="ContentRightAddOn" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentRightAddOnProperty =
        AddOnDecoratedBox.ContentRightAddOnProperty.AddOwner<InfoPickerInput>();
    
    /// <summary>
    /// Identifies the <see cref="ContentRightAddOnTemplate" /> property.
    /// </summary>
    public static readonly StyledProperty<IDataTemplate?> ContentRightAddOnTemplateProperty =
        AddOnDecoratedBox.ContentRightAddOnTemplateProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="SizeType" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomizableSizeType> SizeTypeProperty =
        CustomizableSizeTypeControlProperty.SizeTypeProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="StyleVariant" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStyleVariant> StyleVariantProperty =
        InputControlStyleVariantProperty.StyleVariantProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<InputControlStatus> StatusProperty =
        InputControlStatusProperty.StatusProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="PlaceholderText" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty =
        AvaloniaProperty.Register<InfoPickerInput, string?>(nameof(PlaceholderText));
    
    /// <summary>
    /// Identifies the <see cref="InfoIcon" /> property.
    /// </summary>
    public static readonly StyledProperty<PathIcon?> InfoIconProperty =
        AvaloniaProperty.Register<InfoPickerInput, PathIcon?>(nameof(InfoIcon));

    /// <summary>
    /// Identifies the <see cref="PickerPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode> PickerPlacementProperty =
        AvaloniaProperty.Register<InfoPickerInput, PlacementMode>(nameof(PickerPlacement),
            PlacementMode.BottomEdgeAlignedLeft,
            coerce:CoercePickerPlacement);

    /// <summary>
    /// Identifies the <see cref="IsArrowVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsArrowVisibleProperty =
        ArrowDecoratedBox.IsArrowVisibleProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        Flyout.IsPointAtCenterProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="MarginToAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        Popup.MarginToAnchorProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="IsReadOnly" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsReadOnlyProperty =
        Avalonia.Controls.TextBox.IsReadOnlyProperty.AddOwner<InfoPickerInput>();

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<InfoPickerInput>();
    
    /// <summary>
    /// Identifies the <see cref="PickerPresenter" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> PickerPresenterProperty =
        AvaloniaProperty.Register<InfoPickerInput, Control?>(nameof(PickerPresenter));
    
    /// <summary>
    /// The motion played when the picker popup opens. The value is set on the picker and forwarded to the
    /// popup by the theme; without a local value the theme supplies <c>PopupMotionDefaults.SelectOpen</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// The motion played when the picker popup closes. The value is set on the picker and forwarded to the
    /// popup by the theme; without a local value the theme supplies <c>PopupMotionDefaults.SelectClose</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// The content of the add-on attached to the left of the input frame, outside the frame.
    /// <see langword="null" /> shows no add-on.
    /// </summary>
    public object? LeftAddOn
    {
        get => GetValue(LeftAddOnProperty);
        set => SetValue(LeftAddOnProperty, value);
    }
    
    /// <summary>
    /// The data template used to present <see cref="LeftAddOn" />.
    /// </summary>
    public IDataTemplate? LeftAddOnTemplate
    {
        get => GetValue(LeftAddOnTemplateProperty);
        set => SetValue(LeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// The content of the add-on attached to the right of the input frame, outside the frame.
    /// <see langword="null" /> shows no add-on.
    /// </summary>
    public object? RightAddOn
    {
        get => GetValue(RightAddOnProperty);
        set => SetValue(RightAddOnProperty, value);
    }
    
    /// <summary>
    /// The data template used to present <see cref="RightAddOn" />.
    /// </summary>
    public IDataTemplate? RightAddOnTemplate
    {
        get => GetValue(RightAddOnTemplateProperty);
        set => SetValue(RightAddOnTemplateProperty, value);
    }
    
    /// <summary>
    /// The content shown inside the input frame, to the left of the input text. A control set here is not
    /// part of the area where a click opens the picker. <see langword="null" /> shows nothing.
    /// </summary>
    public object? ContentLeftAddOn
    {
        get => GetValue(ContentLeftAddOnProperty);
        set => SetValue(ContentLeftAddOnProperty, value);
    }
    
    /// <summary>
    /// The data template used to present <see cref="ContentLeftAddOn" />. It is forwarded to the decorated
    /// box, but the default template of this class renders the left add-on through a presenter of its own
    /// that does not read this value.
    /// </summary>
    public IDataTemplate? ContentLeftAddOnTemplate
    {
        get => GetValue(ContentLeftAddOnTemplateProperty);
        set => SetValue(ContentLeftAddOnTemplateProperty, value);
    }

    /// <summary>
    /// The content shown inside the input frame, to the right of the input text and after the suffix icon
    /// button. <see langword="null" /> shows nothing.
    /// </summary>
    public object? ContentRightAddOn
    {
        get => GetValue(ContentRightAddOnProperty);
        set => SetValue(ContentRightAddOnProperty, value);
    }
    
    /// <summary>
    /// The data template used to present <see cref="ContentRightAddOn" />.
    /// </summary>
    public IDataTemplate? ContentRightAddOnTemplate
    {
        get => GetValue(ContentRightAddOnTemplateProperty);
        set => SetValue(ContentRightAddOnTemplateProperty, value);
    }

    /// <summary>
    /// The size preset of the input. The theme sets the font size of the input text (<c>Large</c>,
    /// <c>Middle</c> and <c>Small</c> each use their own, <c>Custom</c> uses the <c>Middle</c> one) and, for
    /// the three named presets, the maximum height of the input box. The value is also forwarded to the
    /// decorated box.
    /// </summary>
    public CustomizableSizeType SizeType
    {
        get => GetValue(SizeTypeProperty);
        set => SetValue(SizeTypeProperty, value);
    }

    /// <summary>
    /// The style variant of the input frame. The value is forwarded to the decorated box that paints the
    /// frame; only <c>Outlined</c> reports a border width to a container that uses the picker in a compact
    /// space.
    /// </summary>
    public InputControlStyleVariant StyleVariant
    {
        get => GetValue(StyleVariantProperty);
        set => SetValue(StyleVariantProperty, value);
    }

    /// <summary>
    /// The status of the input, painted by the input frame. It is forwarded to the decorated box, together
    /// with the status reported by the form item through <see cref="NotifyValidateStatus" />, and both feed
    /// <see cref="EffectiveStatus" />.
    /// </summary>
    public InputControlStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// The text shown by the input box while it is empty. <see langword="null" /> shows no placeholder.
    /// </summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }
    
    /// <summary>
    /// The icon shown at the right end of the input, inside the frame, by the suffix button. That button
    /// turns into the clear button while the input can be cleared, and it is visible only when this value
    /// is set, so <see langword="null" /> hides both the icon and the clear button.
    /// </summary>
    public PathIcon? InfoIcon
    {
        get => GetValue(InfoIconProperty);
        set => SetValue(InfoIconProperty, value);
    }

    /// <summary>
    /// Where the popup is placed relative to the picker. It also decides the position of the arrow and
    /// whether an arrow can be shown. <c>Pointer</c>, <c>Custom</c>, <c>AnchorAndGravity</c> and
    /// <c>Center</c> are replaced by <c>BottomEdgeAlignedLeft</c>, the default.
    /// </summary>
    public PlacementMode PickerPlacement
    {
        get => GetValue(PickerPlacementProperty);
        set => SetValue(PickerPlacementProperty, value);
    }

    /// <summary>
    /// Whether the popup shows an arrow that points to the picker. The arrow also needs a placement that
    /// supports one; <see cref="IsArrowVisibleEffective" /> holds the result. The theme's default is
    /// <see langword="true" />.
    /// </summary>
    public bool IsArrowVisible
    {
        get => GetValue(IsArrowVisibleProperty);
        set => SetValue(IsArrowVisibleProperty, value);
    }

    /// <summary>
    /// Whether the popup is anchored to the center of the picker rather than to a point on its edge. The
    /// value is forwarded to the popup.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// The distance between the popup and the picker, in device-independent pixels. The value is forwarded
    /// to the popup; without a local value the theme supplies the <c>MarginToAnchor</c> popup token.
    /// </summary>
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    /// <summary>
    /// Makes the input read-only: the text cannot be edited, typing and Enter no longer open or submit the
    /// picker, a click on the input no longer opens it, and the clear button stays hidden. The value is
    /// forwarded to the input box.
    /// </summary>
    public bool IsReadOnly
    {
        get => GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    /// <summary>
    /// Whether the popup and the arrow box play their open and close motion. The value is forwarded to
    /// both; without a local value the theme supplies the <c>EnableMotion</c> token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// The control shown inside the picker popup. When it is <see langword="null" /> as the picker opens,
    /// the control returned by <see cref="CreatePickerPresenter" /> is created and assigned; that instance
    /// is released when the picker leaves the visual tree, while a control assigned from outside is kept.
    /// </summary>
    public Control? PickerPresenter
    {
        get => GetValue(PickerPresenterProperty);
        set => SetValue(PickerPresenterProperty, value);
    }
    #endregion

    #region 内部属性定义


    internal static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<InfoPickerInput, string?>(nameof(Text));
    
    internal static readonly DirectProperty<InfoPickerInput, double> PreferredInputWidthProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, double>(nameof(PreferredInputWidth),
            o => o.PreferredInputWidth,
            (o, v) => o.PreferredInputWidth = v);
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsClearButtonVisibleProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsClearButtonVisible),
            o => o.IsClearButtonVisible,
            (o, v) => o.IsClearButtonVisible = v);
    
    internal static readonly StyledProperty<SpaceItemPosition?> CompactSpaceItemPositionProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceItemPositionProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly StyledProperty<Orientation> CompactSpaceOrientationProperty = 
        CompactSpaceAwareControlProperty.CompactSpaceOrientationProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly StyledProperty<bool> IsUsedInCompactSpaceProperty = 
        CompactSpaceAwareControlProperty.IsUsedInCompactSpaceProperty.AddOwner<InfoPickerInput>();

    internal static readonly StyledProperty<FormValidateStatus> FormStatusProperty =
        InputControlState.FormStatusProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly StyledProperty<IFormValidateFeedback?> FormFeedbackProperty =
        AvaloniaProperty.Register<InfoPickerInput, IFormValidateFeedback?>(nameof (FormFeedback));
    
    internal static readonly StyledProperty<bool> ShouldUseOverlayPopupProperty =
        AvaloniaProperty.Register<InfoPickerInput, bool>(nameof(ShouldUseOverlayPopup), true);

    /// <summary>
    /// Identifies the <see cref="IsPickerOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPickerOpenProperty =
        AvaloniaProperty.Register<InfoPickerInput, bool>(nameof(IsPickerOpen), coerce: CoerceIsPickerOpen);

    /// <summary>
    /// Identifies the <see cref="IsPopupPinnedOpen" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        Popup.IsPopupPinnedOpenProperty.AddOwner<InfoPickerInput>();
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsArrowVisibleEffectiveProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsArrowVisibleEffective),
            o => o.IsArrowVisibleEffective,
            (o, v) => o.IsArrowVisibleEffective = v);

    internal static readonly DirectProperty<InfoPickerInput, InputControlStatus> EffectiveStatusProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, InputControlStatus>(
            nameof(EffectiveStatus),
            o => o.EffectiveStatus,
            (o, v) => o.EffectiveStatus = v);
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsPopupHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsPopupHorizontalFlipped),
            o => o.IsPopupHorizontalFlipped,
            (o, v) => o.IsPopupHorizontalFlipped = v);
    
    internal static readonly DirectProperty<InfoPickerInput, bool> IsPopupVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<InfoPickerInput, bool>(nameof(IsPopupVerticalFlipped),
            o => o.IsPopupVerticalFlipped,
            (o, v) => o.IsPopupVerticalFlipped = v);
    
    internal static readonly StyledProperty<ArrowPosition> ArrowPositionProperty =
        ArrowDecoratedBox.ArrowPositionProperty.AddOwner<InfoPickerInput>();

    internal string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private double _preferredInputWidth = double.NaN;

    internal double PreferredInputWidth
    {
        get => _preferredInputWidth;
        set => SetAndRaise(PreferredInputWidthProperty, ref _preferredInputWidth, value);
    }

    private bool _isClearButtonVisible;

    internal bool IsClearButtonVisible
    {
        get => _isClearButtonVisible;
        set => SetAndRaise(IsClearButtonVisibleProperty, ref _isClearButtonVisible, value);
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
        private set => SetCurrentValue(FormStatusProperty, value);
    }
    
    internal IFormValidateFeedback? FormFeedback
    {
        get => GetValue(FormFeedbackProperty);
        set => SetValue(FormFeedbackProperty, value);
    }
    
    internal bool ShouldUseOverlayPopup
    {
        get => GetValue(ShouldUseOverlayPopupProperty);
        set => SetValue(ShouldUseOverlayPopupProperty, value);
    }

    /// <summary>
    /// Whether the picker popup is open. Setting it to <see langword="true" /> creates the presenter when
    /// needed and then shows the popup; setting it to <see langword="false" /> raises the close
    /// notifications and hides the popup. A popup closed by light dismiss resets the value to
    /// <see langword="false" />. While <see cref="IsPopupPinnedOpen" /> is <see langword="true" />,
    /// <see langword="false" /> is coerced back to <see langword="true" /> except during a lifecycle close.
    /// </summary>
    public bool IsPickerOpen
    {
        get => GetValue(IsPickerOpenProperty);
        set => SetValue(IsPickerOpenProperty, value);
    }

    /// <summary>
    /// Keeps the popup open: setting it to <see langword="true" /> opens the picker, and while it is set
    /// the picker ignores requests to close, such as focus leaving the picker. Setting it back does not
    /// close an open popup. The value is relayed to the popup.
    /// </summary>
    public bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }
    
    private bool _isArrowVisibleEffective;

    internal bool IsArrowVisibleEffective
    {
        get => _isArrowVisibleEffective;
        private set => SetAndRaise(IsArrowVisibleEffectiveProperty, ref _isArrowVisibleEffective, value);
    }

    private InputControlStatus _effectiveStatus;

    internal InputControlStatus EffectiveStatus
    {
        get => _effectiveStatus;
        private set => SetAndRaise(EffectiveStatusProperty, ref _effectiveStatus, value);
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

    #endregion

    private protected AddOnDecoratedBox? DecoratedBox;
    private protected PickerClearUpButton? PickerClearUpButton;
    private protected Popup? PickerPopup;
    /// <summary>
    /// Whether the selection of the current open session is valid. It is reset to <see langword="false" />
    /// when the picker opens and passed to <see cref="NotifyFlyoutAboutToClose" /> when the picker closes.
    /// </summary>
    protected bool CurrentValidSelected;
    /// <summary>
    /// The input box of the template (<c>PART_InfoInputBox</c>), or <see langword="null" /> until the
    /// template is applied or when the template has no such part.
    /// </summary>
    protected TextBox? InfoInputBox;
    /// <summary>
    /// The frame of the decorated box that holds the input box and the inner add-ons, or
    /// <see langword="null" /> until the decorated box has applied its template. It delimits the area where
    /// a click opens the picker.
    /// </summary>
    protected Control? PickerInnerBox;

    private protected bool IsChoosing;
    private bool _isPreviewing;
    private AddOnDecoratedBox? _addOnDecoratedBox;
    private IDisposable? _deactivationSubscription;
    private IDisposable? _popupPinnedOpenBinding;
    private int _popupLifecycleCloseDepth;
    private Control? _ownedPickerPresenter;
    private IDisposable? _presenterOwnerBinding;
    

    static InfoPickerInput()
    {
        AffectsMeasure<InfoPickerInput>(PreferredInputWidthProperty, SizeTypeProperty);
        IsPickerOpenProperty.Changed.AddClassHandler<InfoPickerInput>((picker, args) => picker.HandleIsPickerOpenChanged(args));
    }

    public InfoPickerInput()
    {
        // Tunnel, so the picker sees navigation keys before the input's caret handling consumes them.
        AddHandler(KeyDownEvent, HandleInputKeyDown, RoutingStrategies.Tunnel);
    }

    /// <summary>
    /// Clears the text of the input box. The clear button calls it through
    /// <see cref="NotifyClearButtonClicked" />.
    /// </summary>
    public virtual void Clear()
    {
        InfoInputBox?.Clear();
    }

    /// <summary>
    /// Closes the picker popup when it is open, through the same path as setting
    /// <see cref="IsPickerOpen" /> to <see langword="false" />. The request is ignored while
    /// <see cref="IsPopupPinnedOpen" /> is <see langword="true" />.
    /// </summary>
    public void ClosePickerFlyout()
    {
        SetPickerOpenIfChanged(false);
    }

    private void HandleIsPickerOpenChanged(AvaloniaPropertyChangedEventArgs args)
    {
        var isOpen = args.NewValue is true;
        if (isOpen)
        {
            EnsurePickerPresenter();
            CurrentValidSelected = false;
            NotifyFlyoutAboutToShow();
            NotifyPickerOpened();
        }
        else
        {
            NotifyFlyoutAboutToClose(CurrentValidSelected);
            NotifyPickerClosed();
        }
        UpdatePseudoClasses();
        ApplyPopupOpenState(isOpen);
    }

    private void HandleFlyoutAboutToShow(object? sender, EventArgs args)
    {
    }

    /// <summary>
    /// Called when the picker is about to open, after the presenter was created if it was missing, and
    /// before <see cref="NotifyPickerOpened" /> and the popup is shown. The base method does nothing.
    /// </summary>
    protected virtual void NotifyFlyoutAboutToShow()
    {
    }

    private void HandleFlyoutAboutToClose(object? sender, EventArgs args)
    {
    }

    /// <summary>
    /// Called when <see cref="IsPickerOpen" /> changes to <see langword="false" />, before
    /// <see cref="NotifyPickerClosed" />. The base method does nothing.
    /// </summary>
    /// <param name="selectedIsValid">
    /// The value of <see cref="CurrentValidSelected" /> at the time of closing.
    /// </param>
    protected virtual void NotifyFlyoutAboutToClose(bool selectedIsValid)
    {
    }

    private void HandleFlyoutOpened(object? sender, EventArgs args)
    {
    }

    /// <summary>
    /// Called when <see cref="IsPickerOpen" /> changes to <see langword="true" />, after
    /// <see cref="NotifyFlyoutAboutToShow" /> and before the popup is shown. The base method calls
    /// <see cref="NotifyFlyoutOpened" />; call it from an override to keep that.
    /// </summary>
    protected virtual void NotifyPickerOpened()
    {
        NotifyFlyoutOpened();
    }

    /// <summary>
    /// Called by <see cref="NotifyPickerOpened" /> before the popup is shown. The base method does nothing.
    /// </summary>
    protected virtual void NotifyFlyoutOpened()
    {
    }

    private void HandleFlyoutClosed(object? sender, EventArgs args)
    {
    }

    /// <summary>
    /// Called when <see cref="IsPickerOpen" /> changes to <see langword="false" />, after
    /// <see cref="NotifyFlyoutAboutToClose" />. The base method ends the preview state and then calls
    /// <see cref="NotifyFlyoutClosed" />; call it from an override to keep both.
    /// </summary>
    protected virtual void NotifyPickerClosed()
    {
        SetPreviewing(false);
        NotifyFlyoutClosed();
    }

    private protected bool IsPreviewing => _isPreviewing;

    /// <summary>
    /// Marks whether the input text is a hover preview rather than the candidate, which dims it.
    /// </summary>
    private protected void SetPreviewing(bool isPreviewing)
    {
        if (_isPreviewing == isPreviewing)
        {
            return;
        }

        _isPreviewing = isPreviewing;
        UpdatePseudoClasses();
    }

    /// <summary>
    /// Called by <see cref="NotifyPickerClosed" /> after the preview state was cleared, when
    /// <see cref="IsPickerOpen" /> changes to <see langword="false" />. The base method does nothing.
    /// </summary>
    protected virtual void NotifyFlyoutClosed()
    {
    }

    /// <summary>
    /// Resolves the template parts (the popup, the add-on decorated box, the input box and the clear button)
    /// and subscribes to them, then applies the popup setup, the arrow state and the open state.
    /// Call the base method before the parts are used in an override.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        _popupPinnedOpenBinding?.Dispose();
        _popupPinnedOpenBinding = null;

        if (PickerPopup != null)
        {
            PickerPopup.Closed -= HandlePickerPopupClosed;
        }

        if (DecoratedBox != null)
        {
            DecoratedBox.TemplateApplied -= HandleDecoratedBoxTemplateApplied;
            DecoratedBox.PropertyChanged -= HandleDecoratedBoxPropertyChanged;
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest -= HandleClearRequest;
        }

        base.OnApplyTemplate(e);

        PickerPopup = e.NameScope.Find<Popup>("PART_Popup");

        DecoratedBox = e.NameScope.Get<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);
        InfoInputBox = e.NameScope.Get<TextBox>("PART_InfoInputBox");
        PickerClearUpButton = e.NameScope.Find<PickerClearUpButton>("PART_ClearUpButton");
        if (PickerPopup != null && InfoInputBox != null)
        {
            // As in AutoComplete and Select: always name the pass-through element, so the input box stays
            // interactive while the overlay is shown.
            PickerPopup.OverlayInputPassThroughElement = InfoInputBox;
        }
        if (PickerPopup != null)
        {
            _popupPinnedOpenBinding = BindUtils.RelayBind(
                this,
                IsPopupPinnedOpenProperty,
                PickerPopup,
                Popup.IsPopupPinnedOpenProperty);
            PickerPopup.Closed += HandlePickerPopupClosed;

        }
        if (DecoratedBox != null)
        {
            DecoratedBox.TemplateApplied += HandleDecoratedBoxTemplateApplied;
            DecoratedBox.PropertyChanged += HandleDecoratedBoxPropertyChanged;
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest += HandleClearRequest;
        }

        _addOnDecoratedBox = e.NameScope.Find<AddOnDecoratedBox>(AddOnDecoratedBox.AddOnDecoratedBoxPart);

        SetupPopupProperties();
        ConfigureArrowPosition();
        ConfigureShowArrowEffective();
        if (IsPickerOpen)
        {
            EnsurePickerPresenter();
            ApplyPopupOpenState(true);
        }
    }


    /// <summary>
    /// Handles the replacement of the template: closes the picker as a lifecycle close (see
    /// <see cref="IsLifecycleClosing" />) and releases the resources of the retired template, while
    /// <see cref="PickerPresenter" /> is kept for the new template.
    /// </summary>
    protected override void OnTemplateChanged(AvaloniaPropertyChangedEventArgs e)
    {
        if (e.OldValue is IControlTemplate)
        {
            // Retired template theme activators can subscribe to this owner.
            // Logical detach only releases ordinary styles in the framework.
            var ownerParts = this.GetVisualDescendants().OfType<Control>()
                .Where(part => ReferenceEquals(part.TemplatedParent, this)).ToArray();
            // This owner intentionally preserves the presenter across templates.
            // It may already be absent from the retired Popup.Child subtree.
            var borders = PickerPresenter?.GetSelfAndVisualDescendants().OfType<Border>().ToArray();
            ClosePickerForLifecycle();
            if (borders is not null)
            {
                foreach (var border in borders)
                {
                    border.ReleaseDetachedCompositionCache();
                }
            }
            if (ReferenceEquals(Template, e.NewValue))
            {
                foreach (var part in ownerParts)
                {
                    part.ReleaseTemplatedParentTheme();
                }
            }
        }
        base.OnTemplateChanged(e);
    }

    private void HandleDecoratedBoxTemplateApplied(object? sender, TemplateAppliedEventArgs args)
    {
        PickerInnerBox = DecoratedBox!.ContentFrame;
    }

    /// <summary>
    /// Toggles the picker on a pointer release outside the popup: a release in the input area that does not
    /// hit the clear button opens the picker, any other release closes it, and the release is marked handled.
    /// Releases inside the popup, releases already handled and a read-only picker are left alone.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs args)
    {
        base.OnPointerReleased(args);

        if (!args.Handled && !IsReadOnly && args.Source is Visual source)
        {
            // Check if click is inside the input area (not in popup)
            if (PickerPopup?.IsInsidePopup(source) != true)
            {
                if (IsPointerInInfoInputBox(args.GetPosition(this)) &&
                    !ClickInClearUpButtonWithClearMode(args))
                {
                    SetPickerOpenIfChanged(true);
                }
                else
                {
                    SetPickerOpenIfChanged(false);
                }
                args.Handled = true;
            }
        }
    }
    
    /// <summary>
    /// The element belongs to the picker or to its popup.
    /// </summary>
    private protected bool IsPickerElement(IInputElement? element)
    {
        return element is Visual visual &&
               (ReferenceEquals(visual, this) || this.IsVisualAncestorOf(visual) ||
                PickerPopup?.IsInsidePopup(visual) == true);
    }

    #region Keyboard

    // Keyboard semantics follow Ant Design pickers: focus never opens the popup and typing does (each picker parses
    // its own text), Enter submits, Escape reverts (and closes unless the popup is pinned), and focus leaving the
    // picker closes it, committing or discarding the edit by the picker's confirmation rule. While the popup is open
    // and the input holds no typed text, navigation keys drive the popup panel instead of the caret; Ant Design has
    // no such forwarding.

    private bool _isCancelingEdit;

    /// <summary>
    /// The picker closes because the user reverted the edit (Escape), so nothing is committed.
    /// </summary>
    private protected bool IsCancelingEdit => _isCancelingEdit;

    /// <summary>
    /// The picker closes because it leaves the visual tree or replaces its template; the draft is discarded so
    /// nothing is written back during teardown.
    /// </summary>
    private protected bool IsLifecycleClosing => _popupLifecycleCloseDepth > 0;

    /// <summary>
    /// The input holds text typed in this open session rather than a formatted value.
    /// </summary>
    private protected virtual bool HasPendingInputText => false;

    /// <summary>
    /// Determines whether an event source is the info input box (<c>InfoInputBox</c>) or one of its visual
    /// descendants. The key-down handler of the picker ignores keys that come from any other element. Call the
    /// base method from an override to keep the input box recognized.
    /// </summary>
    /// <param name="source">
    /// The source of the event, such as <c>KeyEventArgs.Source</c>.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when <paramref name="source" /> is the info input box or lies within it; otherwise
    /// <see langword="false" />.
    /// </returns>
    private protected virtual bool IsPickerInputElement(object? source) =>
        source is Visual visual && InfoInputBox is not null &&
        (ReferenceEquals(visual, InfoInputBox) || InfoInputBox.IsVisualAncestorOf(visual));

    private void HandleInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Handled || IsReadOnly || !IsPickerInputElement(e.Source))
        {
            return;
        }

        var noModifiers = e.KeyModifiers == KeyModifiers.None;
        switch (e.Key)
        {
            case Key.Enter when noModifiers:
                if (IsPickerOpen)
                {
                    NotifyInputSubmit();
                }
                else
                {
                    SetPickerOpenIfChanged(true);
                }
                e.Handled = true;
                break;
            case Key.Escape when noModifiers && IsPickerOpen:
                CancelPickerEdit();
                e.Handled = true;
                break;
            case Key.Tab:
                e.Handled = NotifyInputTab(e.KeyModifiers.HasFlag(KeyModifiers.Shift));
                break;
            default:
                if (IsPickerOpen && !HasPendingInputText && NotifyNavigationKey(e.Key, e.KeyModifiers))
                {
                    e.Handled = true;
                }
                break;
        }
    }

    /// <summary>
    /// Enter in an open picker. The default closes the popup.
    /// </summary>
    private protected virtual void NotifyInputSubmit() => ClosePickerFlyout();

    /// <summary>
    /// Reverts the edit of this open session, including the input text; the picker closes afterwards unless the
    /// popup is pinned open.
    /// </summary>
    private protected virtual void NotifyEditCanceled()
    {
    }

    /// <summary>
    /// Tab in an input; returns true when the picker moved focus itself.
    /// </summary>
    private protected virtual bool NotifyInputTab(bool isBackward) => false;

    /// <summary>
    /// A navigation key in an open picker; returns true when the popup panel used it.
    /// </summary>
    private protected virtual bool NotifyNavigationKey(Key key, KeyModifiers modifiers) => false;

    private protected void CancelPickerEdit()
    {
        _isCancelingEdit = true;
        try
        {
            NotifyEditCanceled();
            ClosePickerFlyout();
        }
        finally
        {
            _isCancelingEdit = false;
        }
    }

    /// <summary>
    /// Closes the picker when focus moves to an element outside the picker and its popup. A popup that is
    /// pinned open stays open.
    /// </summary>
    protected override void OnLostFocus(FocusChangedEventArgs e)
    {
        base.OnLostFocus(e);
        CloseIfFocusLeftPicker(e.NewFocusedElement);
    }

    /// <summary>
    /// Closes the popup once focus is outside both the picker and its popup.
    /// </summary>
    private protected void CloseIfFocusLeftPicker(IInputElement? newFocusedElement)
    {
        if (IsPickerOpen && !IsPopupPinnedOpen && !IsPickerElement(newFocusedElement))
        {
            ClosePickerFlyout();
        }
    }

    #endregion

    /// <summary>
    /// Whether the source of a pointer release lies in the clear icon button of the template's clear button
    /// part. Such a release must not open the picker.
    /// </summary>
    /// <param name="args">
    /// The pointer release whose source is tested.
    /// </param>
    /// <returns>
    /// <see langword="true" /> when the template has a clear button part and the source is, or lies in, an
    /// <c>InputClearIconButton</c>; otherwise <see langword="false" />.
    /// </returns>
    protected bool ClickInClearUpButtonWithClearMode(PointerReleasedEventArgs args)
    {
        if (PickerClearUpButton != null)
        {
            var sourceControl = args.Source as Control;
            return sourceControl.FindLogicalAncestorOfType<InputClearIconButton>() is not null;
        }
        return false;
    }

    private bool IsPointerInInfoInputBox(Point position)
    {
        if (PickerInnerBox is not null)
        {
            var pos = PickerInnerBox.TranslatePoint(new Point(0, 0), this);
            if (!pos.HasValue)
            {
                return false;
            }
        
            var targetWidth  = PickerInnerBox.Bounds.Width;
            var targetHeight = PickerInnerBox.Bounds.Height;
            var startOffsetX = pos.Value.X;
            var endOffsetX   = startOffsetX + targetWidth;
            var offsetY      = pos.Value.Y;
            if (ContentLeftAddOn is Control leftContent)
            {
                var leftContentPos = leftContent.TranslatePoint(new Point(0, 0), this);
                if (leftContentPos.HasValue)
                {
                    startOffsetX = leftContentPos.Value.X + leftContent.Bounds.Width;
                }
            }
            
            targetWidth = endOffsetX - startOffsetX;
            var bounds = new Rect(new Point(startOffsetX, offsetY), new Size(targetWidth, targetHeight));
            return bounds.Contains(position);
        }
    
        return false;
    }

    // Ant Design shows the clear button on hover and while focus is inside the picker, so keyboard users can reach it.
    private protected bool IsClearButtonRevealed => DecoratedBox is { IsInnerBoxHover: true } || IsKeyboardFocusWithin;

    /// <summary>
    /// Switches the suffix button to clear mode: <see cref="IsClearButtonVisible" /> becomes
    /// <see langword="true" /> when the input box is hovered or the picker has keyboard focus, the input is
    /// editable and it holds text, and <see langword="false" /> otherwise. It runs when the hover state or the
    /// keyboard focus within changes. An override sets the value through
    /// <see cref="SetClearButtonVisibleIfChanged" />.
    /// </summary>
    protected virtual void ConfigureIsClearButtonVisible()
    {
        if (DecoratedBox is not null)
        {
            SetClearButtonVisibleIfChanged(
                IsClearButtonRevealed &&
                InfoInputBox?.IsReadOnly == false &&
                InfoInputBox.Text?.Length > 0);
        }
    }

    /// <summary>
    /// Handles a click on the clear button. The base method calls <see cref="Clear" /> and hides the button
    /// through <see cref="SetClearButtonVisibleIfChanged" />; call it from an override to keep both.
    /// When the clear button held keyboard focus, focus returns to the input box after this method returns.
    /// </summary>
    protected virtual void NotifyClearButtonClicked()
    {
        Clear();
        SetClearButtonVisibleIfChanged(false);
    }

    /// <summary>
    /// Called after the picker created its presenter, which happens when the picker opens and
    /// <see cref="PickerPresenter" /> is <see langword="null" />. The created control is already assigned to
    /// <see cref="PickerPresenter" />. The base method calls <see cref="NotifyFlyoutPresenterCreated" />.
    /// </summary>
    /// <param name="pickerPresenter">
    /// The control returned by <see cref="CreatePickerPresenter" />.
    /// </param>
    protected virtual void NotifyPickerPresenterCreated(Control pickerPresenter)
    {
        NotifyFlyoutPresenterCreated(pickerPresenter);
    }

    /// <summary>
    /// Called before the picker releases the presenter it created itself, which happens when the picker
    /// leaves the visual tree; <see cref="PickerPresenter" /> is reset to <see langword="null" /> afterwards.
    /// A presenter assigned from outside is not released. The base method calls
    /// <see cref="NotifyFlyoutPresenterCleared" />.
    /// </summary>
    /// <param name="pickerPresenter">
    /// The presenter about to be released.
    /// </param>
    protected virtual void NotifyPickerPresenterCleared(Control pickerPresenter)
    {
        NotifyFlyoutPresenterCleared(pickerPresenter);
    }

    /// <summary>
    /// Called by <see cref="NotifyPickerPresenterCreated" /> with the newly created presenter. The base
    /// method does nothing.
    /// </summary>
    /// <param name="flyoutPresenter">
    /// The control returned by <see cref="CreatePickerPresenter" />.
    /// </param>
    protected virtual void NotifyFlyoutPresenterCreated(Control flyoutPresenter)
    {
    }

    /// <summary>
    /// Called by <see cref="NotifyPickerPresenterCleared" /> with the presenter that is about to be released.
    /// The base method does nothing.
    /// </summary>
    /// <param name="flyoutPresenter">
    /// The presenter about to be released.
    /// </param>
    protected virtual void NotifyFlyoutPresenterCleared(Control flyoutPresenter)
    {
    }

    private void EnsurePickerPresenter()
    {
        if (PickerPresenter is not null)
        {
            return;
        }

        var pickerPresenter = CreatePickerPresenter();
        _ownedPickerPresenter = pickerPresenter;
        SetCurrentValue(PickerPresenterProperty, pickerPresenter);
        NotifyPickerPresenterCreated(pickerPresenter);
    }

    private void ClearOwnedPickerPresenter()
    {
        var pickerPresenter = _ownedPickerPresenter;
        if (pickerPresenter is null)
        {
            return;
        }

        if (IsPickerOpen)
        {
            SetPickerOpenIfChanged(false);
        }

        NotifyPickerPresenterCleared(pickerPresenter);
        if (ReferenceEquals(PickerPresenter, pickerPresenter))
        {
            SetCurrentValue(PickerPresenterProperty, null);
        }
        _ownedPickerPresenter = null;
    }

    /// <summary>
    /// Called from <see cref="OnApplyTemplate" /> once the template parts are resolved and subscribed, to
    /// configure the popup. The base method calls <see cref="SetupFlyoutProperties" />.
    /// </summary>
    protected virtual void SetupPopupProperties()
    {
        SetupFlyoutProperties();
    }

    /// <summary>
    /// Called by <see cref="SetupPopupProperties" /> when the template is applied. The base method does
    /// nothing.
    /// </summary>
    protected virtual void SetupFlyoutProperties()
    {
    }

    /// <summary>
    /// Creates the control shown inside the picker popup. It is called when the picker opens and
    /// <see cref="PickerPresenter" /> is <see langword="null" />; the picker owns the returned control and
    /// releases it when it leaves the visual tree.
    /// </summary>
    /// <returns>
    /// The content of the popup.
    /// </returns>
    protected abstract Control CreatePickerPresenter();

    /// <summary>
    /// Sets <see cref="IsPickerOpen" /> to the requested state without replacing a binding or a local value
    /// on the property, and only when the value differs. The coercion for <see cref="IsPopupPinnedOpen" />
    /// still applies.
    /// </summary>
    /// <param name="isOpen">
    /// The requested open state.
    /// </param>
    protected void SetPickerOpenIfChanged(bool isOpen)
    {
        if (IsPickerOpen != isOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, isOpen);
        }
    }

    /// <summary>
    /// Sets the clear mode of the suffix button (<see cref="IsClearButtonVisible" />) without replacing a
    /// binding or a local value on the property, and only when the value differs.
    /// </summary>
    /// <param name="isVisible">
    /// Whether the button is in clear mode.
    /// </param>
    protected void SetClearButtonVisibleIfChanged(bool isVisible)
    {
        if (IsClearButtonVisible != isVisible)
        {
            SetCurrentValue(IsClearButtonVisibleProperty, isVisible);
        }
    }

    /// <summary>
    /// Not supported: a picker shows a presenter, so override <see cref="CreatePickerPresenter" /> instead.
    /// </summary>
    /// <returns>
    /// Never returns.
    /// </returns>
    /// <exception cref="NotImplementedException">
    /// Always thrown.
    /// </exception>
    protected virtual Flyout CreatePickerFlyout()
    {
        throw new NotImplementedException("Use CreatePickerPresenter instead");
    }

    /// <summary>
    /// Syncs the <c>Choosing</c>, <c>FlyoutOpen</c> and <c>Previewing</c> pseudo-classes with
    /// <see cref="IsChoosing" />, <see cref="IsPickerOpen" /> and the preview state. It runs when the open
    /// state or the preview state changes. Call the base method from an override.
    /// </summary>
    protected virtual void UpdatePseudoClasses()
    {
        PseudoClasses.Set(InfoPickerPseudoClass.FlyoutOpen, IsPickerOpen);
        PseudoClasses.Set(InfoPickerPseudoClass.Choosing, IsChoosing);
        PseudoClasses.Set(InfoPickerPseudoClass.Previewing, _isPreviewing);
    }

    /// <summary>
    /// Subscribes to the deactivation of the owning top level, which closes the picker, and opens the picker
    /// when the popup is pinned open.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _deactivationSubscription =
            TopLevelDeactivation.Subscribe(TopLevel.GetTopLevel(this), HandleWindowDeactivated);
        if (IsPopupPinnedOpen)
        {
            SetPickerOpenIfChanged(true);
        }
    }

    private void HandleWindowDeactivated(object? sender, EventArgs e)
    {
        SetPickerOpenIfChanged(false);
    }

    private void HandleDecoratedBoxPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property == AddOnDecoratedBox.IsInnerBoxHoverProperty)
        {
            ConfigureIsClearButtonVisible();
        }
    }

    private void HandleClearRequest(object? sender, EventArgs args)
    {
        // The clear button hides once the value is gone; keep keyboard focus in the picker, as Ant Design returns it
        // to the input.
        var hadFocus = TopLevel.GetTopLevel(this)?.FocusManager?.GetFocusedElement() is Visual focused &&
                       PickerClearUpButton?.IsVisualAncestorOf(focused) == true;
        NotifyClearButtonClicked();
        if (hadFocus)
        {
            InfoInputBox?.Focus(NavigationMethod.Tab);
        }
    }

    /// <summary>
    /// Closes the picker as a lifecycle close before the base call, so that the close is counted as a
    /// lifecycle close and not as a user close (see <see cref="IsLifecycleClosing" />).
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        // The template popup closes itself when it leaves the logical tree, before the visual detach; close it here
        // first so that close counts as a lifecycle close.
        ClosePickerForLifecycle();
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    /// Closes the picker as a lifecycle close, then releases what was attached with the template: the
    /// top-level deactivation subscription, the handlers on the decorated box and the clear button, and the
    /// presenter that the picker created. Call the base method from an override.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ClosePickerForLifecycle();
        base.OnDetachedFromVisualTree(e);

        _deactivationSubscription?.Dispose();
        _deactivationSubscription = null;

        if (DecoratedBox != null)
        {
            DecoratedBox.PropertyChanged -= HandleDecoratedBoxPropertyChanged;
            DecoratedBox.TemplateApplied -= HandleDecoratedBoxTemplateApplied;
        }

        if (PickerClearUpButton is not null)
        {
            PickerClearUpButton.ClearRequest -= HandleClearRequest;
        }

        ClearOwnedPickerPresenter();
    }

    private void ClosePickerForLifecycle()
    {
        try
        {
            ++_popupLifecycleCloseDepth;
            PickerPopup?.CloseForLifecycle();
            SetPickerOpenIfChanged(false);
        }
        finally
        {
            --_popupLifecycleCloseDepth;
        }
    }

    /// <summary>
    /// Not called by this class: the clear button is driven by
    /// <see cref="ConfigureIsClearButtonVisible" /> instead. The base method returns
    /// <see langword="false" />; derived pickers override it.
    /// </summary>
    /// <returns>
    /// Always <see langword="false" /> in the base method.
    /// </returns>
    protected virtual bool ShowClearButtonPredicate()
    {
        return false;
    }
    
    private static PlacementMode CoercePickerPlacement(object o, PlacementMode value)
    {
        if (value == PlacementMode.Pointer ||
            value == PlacementMode.Custom ||
            value == PlacementMode.AnchorAndGravity ||
            value == PlacementMode.Center) 
        {
            return PlacementMode.BottomEdgeAlignedLeft;
        }

        return value;
    }

    private static bool CoerceIsPickerOpen(AvaloniaObject sender, bool value)
    {
        return !value &&
               sender is InfoPickerInput { IsPopupPinnedOpen: true, _popupLifecycleCloseDepth: 0 }
            ? true
            : value;
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

    double ICompactSpaceAware.GetBorderThickness()
    {
        return GetBorderThicknessForCompactSpace();
    }
    
    /// <summary>
    /// The border width reported to the container that uses the picker in a compact space, through
    /// <c>ICompactSpaceAware</c>: the border width of the input frame when the style variant is
    /// <c>Outlined</c>, otherwise <c>0</c>.
    /// </summary>
    /// <returns>
    /// The border width in device-independent pixels.
    /// </returns>
    protected virtual double GetBorderThicknessForCompactSpace()
    {
        if (!IsUsedInCompactSpace)
        {
            return 0.0;
        }

        if (_addOnDecoratedBox == null || _addOnDecoratedBox.StyleVariant != InputControlStyleVariant.Outlined)
        {
            return 0.0;
        }

        // All four borders are equally wide, so the left width stands for all of them.
        return _addOnDecoratedBox.InnerBoxBorderThickness.Left;
    }
    
    /// <summary>
    /// Keeps the derived state in step with property changes: the token owner of a new presenter, the
    /// effective arrow visibility, the effective status, the arrow position, the clear button when the
    /// keyboard focus within changes, and the open state when <see cref="IsPopupPinnedOpen" /> changes.
    /// Call the base method from an override.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == PickerPresenterProperty)
        {
            ForwardTokenOwner(change.GetNewValue<Control?>());
        }

        if (change.Property == IsArrowVisibleProperty ||
            change.Property == PickerPlacementProperty)
        {
            ConfigureShowArrowEffective();
        }

        if (change.Property == StatusProperty ||
            change.Property == FormStatusProperty ||
            change.Property == DataValidationErrors.HasErrorsProperty ||
            change.Property == DataValidationErrors.ErrorsProperty)
        {
            UpdateEffectiveStatus();
        }

        if (change.Property == PickerPlacementProperty ||
            change.Property == IsPopupHorizontalFlippedProperty ||
            change.Property == IsPopupVerticalFlippedProperty)
        {
            ConfigureArrowPosition();
        }

        if (change.Property == IsKeyboardFocusWithinProperty)
        {
            ConfigureIsClearButtonVisible();
        }

        if (change.Property == IsPopupPinnedOpenProperty)
        {

            if (change.GetNewValue<bool>())
            {
                SetPickerOpenIfChanged(true);
            }
            else if (IsPickerOpen)
            {
                if (PickerPopup is { IsOpen: true })
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


    private void ApplyPopupOpenState(bool isOpen)
    {
        if (PickerPopup is null)
        {
            return;
        }

        if (PickerPopup.IsOpen != isOpen)
        {
            PickerPopup.IsOpen = isOpen;
        }
    }

    private void HandlePickerPopupClosed(object? sender, EventArgs e)
    {
        // The popup closed on its own (light dismiss included): write the state back so IsPickerOpen follows it.
        if (IsPickerOpen)
        {
            SetCurrentValue(IsPickerOpenProperty, false);
        }
    }

    /// <summary>
    /// The picker presenter is shown inside the popup's registered ArrowDecoratedBox, whose identity would otherwise
    /// become the presenter's Token owner. The presenter belongs to this picker, so its owner follows the picker's own:
    /// the identity-less presenter parts (time view, buttons frame) read the picker's effective globals, and
    /// <c>Controls[picker]</c> reaches them. The date presenter forwards the same owner to its DateViewer (the picker takes
    /// over that viewer's globals); other registered controls inside the presenter (Button, IconButton) keep their own
    /// identity.
    /// </summary>
    private void ForwardTokenOwner(Control? newPresenter)
    {
        _presenterOwnerBinding?.Dispose();
        _presenterOwnerBinding = newPresenter is null ? null : TokenOwnerForwarding.Forward(this, newPresenter);
    }

    private void UpdateEffectiveStatus()
    {
        var effectiveStatus = InputControlState.ResolveEffectiveStatus(this, Status, FormStatus);
        if (EffectiveStatus != effectiveStatus)
        {
            EffectiveStatus = effectiveStatus;
        }
    }
    
    /// <summary>
    /// Updates <see cref="IsArrowVisibleEffective" />, which allows the popup arrow only when
    /// <see cref="IsArrowVisible" /> is set and <see cref="PickerPlacement" /> supports an arrow. It runs
    /// when the template is applied and when either property changes.
    /// </summary>
    protected void ConfigureShowArrowEffective()
    {
        var isArrowVisibleEffective = IsArrowVisible && PopupUtils.CanEnabledArrow(PickerPlacement);
        if (IsArrowVisibleEffective != isArrowVisibleEffective)
        {
            SetCurrentValue(IsArrowVisibleEffectiveProperty, isArrowVisibleEffective);
        }
    }

    /// <summary>
    /// Updates <see cref="ArrowPosition" /> from <see cref="PickerPlacement" />, mirrored when the popup is
    /// flipped horizontally or vertically (<see cref="IsPopupHorizontalFlipped" /> and
    /// <see cref="IsPopupVerticalFlipped" />). A placement that yields no arrow position leaves the value
    /// unchanged. It runs when the template is applied and when the placement or a flip state changes.
    /// </summary>
    protected void ConfigureArrowPosition()
    {
        var arrowPosition = PopupUtils.CalculateArrowPosition(PickerPlacement, null, null);
        if (arrowPosition.HasValue)
        {
            arrowPosition = ArrowPositionUtils.FlipArrowPosition(arrowPosition.Value, IsPopupHorizontalFlipped, IsPopupVerticalFlipped);
            if (ArrowPosition != arrowPosition)
            {
                SetCurrentValue(ArrowPositionProperty, arrowPosition);
            }
        }
    }
    
    #region 实现 FormItem 接口
    
    private EventHandler? _formValueChanged;
    event EventHandler? IFormItemAware.ValueChanged
    {
        add => _formValueChanged += value;
        remove => _formValueChanged -= value;
    }

    void IFormItemAware.SetFormValue(object? value) => NotifySetFormValue(value);

    object? IFormItemAware.GetFormValue() => NotifyGetFormValue();
    void IFormItemAware.ClearFormValue() => NotifyClearFormValue();
    void IFormItemAware.NotifyValidateStatus(FormValidateStatus status) => NotifyValidateStatus(status);
    void IFormItemFeedbackAware.SetFeedbackControl(FormValidateFeedback? value) => NotifySetFeedBackControl(value);
    
    /// <summary>
    /// Raises <c>IFormItemAware.ValueChanged</c> to tell the owning form item that the value of the picker
    /// changed. A derived picker calls it after its value changed; the base method ignores the argument.
    /// </summary>
    /// <param name="value">
    /// The new value of the picker.
    /// </param>
    protected virtual void NotifyFormValueChanged(object? value)
    {
        _formValueChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Applies a value passed in by the owning form item (<c>IFormItemAware.SetFormValue</c>). The base
    /// method does nothing; a derived picker overrides it to set its value.
    /// </summary>
    /// <param name="value">
    /// The value supplied by the form item.
    /// </param>
    protected virtual void NotifySetFormValue(object? value)
    {
    }

    /// <summary>
    /// Returns the value of the picker for the owning form item (<c>IFormItemAware.GetFormValue</c>). The
    /// base method returns <see langword="null" />; a derived picker overrides it.
    /// </summary>
    /// <returns>
    /// The value of the picker, or <see langword="null" /> when it provides none.
    /// </returns>
    protected virtual object? NotifyGetFormValue()
    {
        return null;
    }

    /// <summary>
    /// Clears the value of the picker at the request of the owning form item
    /// (<c>IFormItemAware.ClearFormValue</c>). The base method does nothing; a derived picker overrides it.
    /// </summary>
    protected virtual void NotifyClearFormValue()
    {
    }

    /// <summary>
    /// Applies the validation status reported by the owning form item: stores it in
    /// <see cref="FormStatus" />, which feeds <see cref="EffectiveStatus" />, when the status differs.
    /// </summary>
    /// <param name="status">
    /// The validation status reported by the form item.
    /// </param>
    protected virtual void NotifyValidateStatus(FormValidateStatus status)
    {
        if (FormStatus != status)
        {
            SetCurrentValue(FormStatusProperty, status);
        }
    }
    
    /// <summary>
    /// Stores the feedback control passed in by the owning form item
    /// (<c>IFormItemFeedbackAware.SetFeedbackControl</c>) in <see cref="FormFeedback" />, when it differs.
    /// </summary>
    /// <param name="value">
    /// The feedback control to show, or <see langword="null" /> to remove it.
    /// </param>
    protected virtual void NotifySetFeedBackControl(FormValidateFeedback? value)
    {
        if (!ReferenceEquals(FormFeedback, value))
        {
            FormFeedback = value;
        }
    }
    #endregion
}
