using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Disposables;
using AtomUI.Controls;
using AtomUI.Controls.Primitives;
using AtomUI.Media;
using AtomUI.Animations;
using AtomUI.Native;
using AtomUI.Reflection;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace AtomUI.Desktop.Controls;

using AvaloniaPopup = Avalonia.Controls.Primitives.Popup;

// A Token-owner transparent container: popup content (drop-down options, popup frames) inherits the host's Token
// owner; the Popup's own visuals read PopupTokenResource explicitly.
/// <summary>
/// A popup with open and close motions, a shadow that follows its host kind, and anchor placement that reports
/// when it flips. The content is shown in its own window or in the overlay layer of the parent window, and
/// inherits the Token owner of the control that hosts it.
/// </summary>
[TokenOwnerTransparent]
public class Popup : AvaloniaPopup, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="PopupRootShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> PopupRootShadowProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(PopupRootShadow));

    /// <summary>
    /// Identifies the <see cref="OverlayHostShadow" /> property.
    /// </summary>
    public static readonly StyledProperty<BoxShadows> OverlayHostShadowProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(OverlayHostShadow));

    /// <summary>
    /// Identifies the <see cref="SurfaceBackground" /> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> SurfaceBackgroundProperty =
        AvaloniaProperty.Register<Popup, IBrush?>(nameof(SurfaceBackground), defaultValue: null);

    /// <summary>
    /// Identifies the <see cref="MotionDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> MotionDurationProperty =
        MotionAwareControlProperty.MotionDurationProperty.AddOwner<Popup>();

    /// <summary>
    /// Identifies the <see cref="IsHorizontalFlipped" /> property.
    /// </summary>
    public static readonly DirectProperty<Popup, bool> IsHorizontalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Popup, bool>(nameof(IsHorizontalFlipped),
            o => o.IsHorizontalFlipped,
            (o, v) => o.IsHorizontalFlipped = v);

    /// <summary>
    /// Identifies the <see cref="IsVerticalFlipped" /> property.
    /// </summary>
    public static readonly DirectProperty<Popup, bool> IsVerticalFlippedProperty =
        AvaloniaProperty.RegisterDirect<Popup, bool>(nameof(IsVerticalFlipped),
            o => o.IsVerticalFlipped,
            (o, v) => o.IsVerticalFlipped = v);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<Popup>();

    /// <summary>
    /// Identifies the <see cref="IsWaveSpiritEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsWaveSpiritEnabledProperty =
        WaveSpiritAwareControlProperty.IsWaveSpiritEnabledProperty.AddOwner<Popup>();

    /// <summary>
    /// Identifies the <see cref="OpenMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> OpenMotionProperty =
        AvaloniaProperty.Register<Popup, AbstractMotion?>(nameof(OpenMotion));

    /// <summary>
    /// Identifies the <see cref="CloseMotion" /> property.
    /// </summary>
    public static readonly StyledProperty<AbstractMotion?> CloseMotionProperty =
        AvaloniaProperty.Register<Popup, AbstractMotion?>(nameof(CloseMotion));

    /// <summary>
    /// Identifies the <see cref="RequestedPlacement" /> property.
    /// </summary>
    public static readonly StyledProperty<PlacementMode?> RequestedPlacementProperty =
        AvaloniaProperty.Register<Popup, PlacementMode?>(nameof(RequestedPlacement));

    /// <summary>
    /// Identifies the <see cref="MarginToAnchor" /> property.
    /// </summary>
    public static readonly StyledProperty<double> MarginToAnchorProperty =
        AvaloniaProperty.Register<Popup, double>(nameof(MarginToAnchor));

    /// <summary>
    /// Identifies the <see cref="IsPointAtCenter" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsPointAtCenterProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof(IsPointAtCenter));

    /// <summary>
    /// Identifies the <see cref="CustomPlacementCallback" /> property.
    /// </summary>
    public static readonly StyledProperty<CustomPlacementCallback?> CustomPlacementCallbackProperty =
        AvaloniaProperty.Register<Popup, CustomPlacementCallback?>(nameof(CustomPlacementCallback));

    /// <summary>
    /// Shadow painted around the frame of the popup surface while the popup is hosted in its own window; the
    /// overlay layer host uses <see cref="OverlayHostShadow" /> instead. The placement pass also takes its
    /// thickness into account when it offsets the popup from the placement target. The Popup theme supplies the
    /// Popup token value.
    /// </summary>
    public BoxShadows PopupRootShadow
    {
        get => GetValue(PopupRootShadowProperty);
        set => SetValue(PopupRootShadowProperty, value);
    }

    /// <summary>
    /// Shadow used in place of <see cref="PopupRootShadow" /> while the popup is hosted in the overlay layer of
    /// its parent window. It feeds the same frame painting and placement offset.
    /// </summary>
    public BoxShadows OverlayHostShadow
    {
        get => GetValue(OverlayHostShadowProperty);
        set => SetValue(OverlayHostShadowProperty, value);
    }

    /// <summary>
    /// Background that the frame of the popup surface paints as a rounded rectangle together with the shadow. The
    /// value is relayed to the shadow-aware container of the content. <see langword="null" />, the default, paints
    /// the surface transparent.
    /// </summary>
    public IBrush? SurfaceBackground
    {
        get => GetValue(SurfaceBackgroundProperty);
        set => SetValue(SurfaceBackgroundProperty, value);
    }

    /// <summary>
    /// Duration of the <see cref="OpenMotion" /> and the <see cref="CloseMotion" /> playback. The Popup theme
    /// supplies the Popup token value; clearing a local value falls back to it.
    /// </summary>
    public TimeSpan MotionDuration
    {
        get => GetValue(MotionDurationProperty);
        set => SetValue(MotionDurationProperty, value);
    }

    private bool _isHorizontalFlipped;

    /// <summary>
    /// Whether the last placement pass showed the popup on the other side of its target along the X axis. It is
    /// updated before <see cref="PositionFlipped" /> is raised, and a centered popup reports
    /// <see langword="false" />.
    /// </summary>
    public bool IsHorizontalFlipped
    {
        get => _isHorizontalFlipped;
        private set => SetAndRaise(IsHorizontalFlippedProperty, ref _isHorizontalFlipped, value);
    }

    private bool _isVerticalFlipped;

    /// <summary>
    /// Whether the last placement pass showed the popup on the other side of its target along the Y axis. It is
    /// updated before <see cref="PositionFlipped" /> is raised, and a centered popup reports
    /// <see langword="false" />.
    /// </summary>
    public bool IsVerticalFlipped
    {
        get => _isVerticalFlipped;
        private set => SetAndRaise(IsVerticalFlippedProperty, ref _isVerticalFlipped, value);
    }

    /// <summary>
    /// Enables the playback of <see cref="OpenMotion" /> and <see cref="CloseMotion" />. Playback additionally
    /// requires <c>Motion.IsAnimationEnabled</c> for the motion target. Setting it to <see langword="false" />
    /// while a motion is in progress cancels the playback and completes a close that waits for its exit motion.
    /// The Popup theme supplies the Popup token value.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Switches the wave spirit effect. <see cref="Popup" /> has no wave spirit of its own and does not read this
    /// value; it only exposes the property. The default is <see langword="false" /> and the Popup theme sets no
    /// value.
    /// </summary>
    public bool IsWaveSpiritEnabled
    {
        get => GetValue(IsWaveSpiritEnabledProperty);
        set => SetValue(IsWaveSpiritEnabledProperty, value);
    }

    /// <summary>
    /// Motion played on the popup surface when the popup opens, or <see langword="null" /> to show it without an
    /// entry animation. It starts after the <c>Opened</c> event once the surface is ready, and plays for
    /// <see cref="MotionDuration" /> only while <see cref="IsMotionEnabled" /> is true. Owning controls set it in
    /// their ControlTheme, typically from <c>PopupMotionDefaults</c>.
    /// </summary>
    public AbstractMotion? OpenMotion
    {
        get => GetValue(OpenMotionProperty);
        set => SetValue(OpenMotionProperty, value);
    }

    /// <summary>
    /// Motion played on the popup surface before the popup closes, or <see langword="null" /> to close without an
    /// exit animation. While it plays the close is put off; the popup closes once the motion has ended. It is
    /// skipped when the entry motion has not presented the popup yet or the placement target can no longer be
    /// located. Owning controls set it in their ControlTheme, typically from <c>PopupMotionDefaults</c>.
    /// </summary>
    public AbstractMotion? CloseMotion
    {
        get => GetValue(CloseMotionProperty);
        set => SetValue(CloseMotionProperty, value);
    }

    /// <summary>
    /// Placement of the popup relative to its placement target, resolved by this class through Avalonia's custom
    /// placement callback. Setting <c>Placement</c> to a value other than <c>Custom</c> is redirected here and
    /// leaves <c>Placement</c> at <c>Custom</c>. <c>Center</c> centers the popup in the window and <c>Pointer</c>
    /// anchors it at the last pointer position. <see langword="null" />, the default, clears the custom placement
    /// callback and leaves <c>Placement</c> untouched.
    /// </summary>
    public PlacementMode? RequestedPlacement
    {
        get => GetValue(RequestedPlacementProperty);
        set => SetValue(RequestedPlacementProperty, value);
    }

    /// <summary>
    /// Gap in device-independent pixels kept between the popup and its placement target. A centered popup ignores
    /// it. The default is <c>0</c>; the Popup theme supplies the Popup token value.
    /// </summary>
    public double MarginToAnchor
    {
        get => GetValue(MarginToAnchorProperty);
        set => SetValue(MarginToAnchorProperty, value);
    }

    /// <summary>
    /// Moves the popup so that its arrow points at the center of the placement target. It takes effect only when
    /// the content shows an arrow and the placement allows one; it is ignored for the <c>Pointer</c> and
    /// <c>Center</c> placements. The default is <see langword="false" />.
    /// </summary>
    public bool IsPointAtCenter
    {
        get => GetValue(IsPointAtCenterProperty);
        set => SetValue(IsPointAtCenterProperty, value);
    }

    /// <summary>
    /// Callback that lets the owner of the popup adjust the placement that was computed for
    /// <see cref="RequestedPlacement" />. It runs after <see cref="PositionFlipped" /> was raised and also for a
    /// centered popup, which reports both flip values as <see langword="false" />. <see langword="null" />, the
    /// default, keeps the computed placement unchanged.
    /// </summary>
    public CustomPlacementCallback? CustomPlacementCallback
    {
        get => GetValue(CustomPlacementCallbackProperty);
        set => SetValue(CustomPlacementCallbackProperty, value);
    }

    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when the placement pass of the popup completes, with the flip result for both axes, after
    /// <see cref="IsHorizontalFlipped" /> and <see cref="IsVerticalFlipped" /> were updated and before
    /// <see cref="CustomPlacementCallback" /> runs. A centered popup raises it with both values of
    /// <see cref="PopupFlippedEventArgs" /> set to <see langword="false" />.
    /// </summary>
    public event EventHandler<PopupFlippedEventArgs>? PositionFlipped;

    #endregion

    #region 内部属性定义

    internal static readonly StyledProperty<BoxShadows> FrameShadowProperty =
        AvaloniaProperty.Register<Popup, BoxShadows>(nameof(FrameShadow));

    internal static readonly StyledProperty<bool> IsPopupPinnedOpenProperty =
        AvaloniaProperty.Register<Popup, bool>(nameof(IsPopupPinnedOpen));

    internal static readonly DirectProperty<Popup, bool> IsPlayingCloseMotionProperty =
        AvaloniaProperty.RegisterDirect<Popup, bool>(nameof(IsPlayingCloseMotion), popup => popup.IsPlayingCloseMotion);

    internal static readonly StyledProperty<Thickness> MotionMarginProperty =
        AvaloniaProperty.Register<Popup, Thickness>(nameof(MotionMargin));

    internal static readonly StyledProperty<Control?> VerticalPlacementFrameProperty =
        AvaloniaProperty.Register<Popup, Control?>(nameof(VerticalPlacementFrame));

    internal BoxShadows FrameShadow
    {
        get => GetValue(FrameShadowProperty);
        set => SetValue(FrameShadowProperty, value);
    }

    internal bool IsPopupPinnedOpen
    {
        get => GetValue(IsPopupPinnedOpenProperty);
        set => SetCurrentValue(IsPopupPinnedOpenProperty, value);
    }

    private PopupCloseState _closeMotionState;

    internal bool IsPlayingCloseMotion =>
        _closeMotionState is PopupCloseState.Pending or PopupCloseState.Playing or PopupCloseState.Completing;

    internal Thickness MotionMargin
    {
        get => GetValue(MotionMarginProperty);
        private set => SetCurrentValue(MotionMarginProperty, value);
    }

    internal Control? VerticalPlacementFrame
    {
        get => GetValue(VerticalPlacementFrameProperty);
        set => SetValue(VerticalPlacementFrameProperty, value);
    }

    /// <summary>
    /// Whether the open motion (including the wait for the first frame to be prepared) or the close motion is in
    /// progress.
    /// </summary>
    internal bool IsPlayingMotion => _isOpenMotionActive || IsPlayingCloseMotion;

    internal bool IsHostOpen => _hasRaisedOpened;

    #endregion

    #region 内部事件定义

    internal event EventHandler<CancelEventArgs>? CloseRequested;

    #endregion

    #region 运行时字段

    private bool _isOpenMotionActive;

    private bool _hasRaisedOpened;

    private bool _isLogicallyAttachedAtOpen;

    private long _motionVersion;

    private bool _closeRequested;

    private IDisposable? _entryPreparation;

    private Control? _motionTarget;

    private TopLevel? _openTopLevel;

    private PopupLightDismissRegistration? _lightDismissRegistration;

    private ShadowsAwareContainer? _motionFrame;

    private RelativePoint? _activeDefaultOrigin;

    // Lets input pass through the shadow area of a popup hosted in its own window. It is attached and
    // released with each open session.
    private IPopupShadowHitTest? _shadowHitTest;

    // Keeps wheel events raised inside the popup from bubbling out and scrolling an ancestor ScrollViewer of
    // the placement target unintentionally.
    // The subscribed element is recorded so that the subscription made on open is removed again on close.
    private InputElement? _wheelGuardSubscribedOn;

    private int _ignoreRequestedPlacementChange;

    private int _lifecycleCloseDepth;

    private int _pinnedOpenGeneration;

    private bool _isPinnedOpenSuspended;

    private bool _isPinnedOpenReconcileRetried;

    private readonly List<Visual> _pinnedOpenTargetStateSubscriptions = [];

    private Control? _pinnedOpenTrackingTarget;

    private Control? _placementTransformTrackingTarget;

    private IDisposable? _placementTransformTracker;

    #endregion

    #region 构造函数

    static Popup()
    {
        IsOpenProperty.OverrideMetadata<Popup>(
            new StyledPropertyMetadata<bool>(coerce: CoerceIsOpen));
        IsLightDismissEnabledProperty.OverrideMetadata<Popup>(
            new StyledPropertyMetadata<bool>(coerce: (sender, value) => value && !((Popup)sender).IsPopupPinnedOpen));
    }

    public Popup()
    {
        this.AddClosingEventHandler(HandlePopupClosing);
        Opened += HandlePopupOpened;
        Closed += HandlePopupClosed;
    }

    #endregion

    #region 控件 API

    /// <summary>
    /// Called when a property of this popup changes. After the base implementation has run it brings placement,
    /// frame shadow, light dismiss, pinned-open state, motion playback and motion host size in step with the new
    /// value, and cancels a pending close when the popup opens again. When the templated parent is cleared it
    /// closes the popup and detaches the content that the discarded template created.
    /// Overrides must call the base implementation, otherwise the popup stops reacting to its own properties.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == TemplatedParentProperty &&
            change.OldValue is AvaloniaObject previousOwner && change.NewValue is null)
        {
            ReleaseTemplateContent(previousOwner);
        }

        if (change.Property == IsMotionEnabledProperty && !CanPlayMotion)
        {
            SettleDisabledMotion();
        }

        if (change.Property == ChildProperty)
        {
            if (change.OldValue is Control oldChild)
            {
                oldChild.AttachedToVisualTree -= HandleChildAttachedToVisualTree;
            }
            if (change.NewValue is Control newChild)
            {
                newChild.AttachedToVisualTree += HandleChildAttachedToVisualTree;
            }
        }

        if (change.Property == IsOpenProperty && change.GetNewValue<bool>() && _closeRequested)
        {
            CancelCloseAnimation();
        }
        if (change.Property == RequestedPlacementProperty)
        {
            var requestedPlacement = change.GetNewValue<PlacementMode?>();
            if (requestedPlacement is not null)
            {
                var originalPlacement = Placement;
                CustomPopupPlacementCallback = HandleCustomPlacement;
                SetCurrentValue(PlacementProperty, PlacementMode.Custom);
                if (originalPlacement == PlacementMode.Custom)
                {
                    this.HandlePositionChange();
                }
            }
            else
            {
                CustomPopupPlacementCallback = null;
            }
        }
        else if (change.Property == PlacementProperty)
        {
            if (_ignoreRequestedPlacementChange > 0)
            {
                return;
            }
            try
            {
                _ignoreRequestedPlacementChange++;
                if (Placement != PlacementMode.Custom)
                {
                    CustomPopupPlacementCallback = HandleCustomPlacement;
                    SetCurrentValue(RequestedPlacementProperty, Placement);
                    SetCurrentValue(PlacementProperty, PlacementMode.Custom);
                    this.HandlePositionChange();
                }
            }
            finally
            {
                _ignoreRequestedPlacementChange--;
            }
        }
        else if (change.Property == MarginToAnchorProperty ||
                 change.Property == VerticalPlacementFrameProperty)
        {
            this.HandlePositionChange();
        }
        else if (change.Property == PopupRootShadowProperty ||
                 change.Property == OverlayHostShadowProperty ||
                 change.Property == IsUsingOverlayLayerProperty)
        {
            ConfigureFrameShadow();
        }
        else if (change.Property == IsLightDismissEnabledProperty ||
                 change.Property == OverlayInputPassThroughElementProperty)
        {
            _lightDismissRegistration?.Update(this);
        }
        else if (change.Property == IsPopupPinnedOpenProperty)
        {
            CoerceValue(IsLightDismissEnabledProperty);
            if (change.GetNewValue<bool>())
            {
                _isPinnedOpenSuspended = false;
                CancelCloseAnimation();
                UpdatePinnedOpenTargetSubscription();
                QueuePinnedOpen();
            }
            else
            {
                ++_pinnedOpenGeneration;
                ClearPinnedOpenTargetSubscription();
                if (!IsOpen)
                {
                    CloseForLifecycle();
                }
            }
        }
        else if (change.Property == ChildProperty && IsPopupPinnedOpen)
        {
            if (Child is null)
            {
                CloseForLifecycle();
            }
            else
            {
                _isPinnedOpenSuspended = false;
                QueuePinnedOpen();
            }
        }

        if (change.Property == PlacementTargetProperty ||
            change.Property == PlacementProperty ||
            change.Property == RequestedPlacementProperty)
        {
            UpdatePlacementTransformTracker();
            if (change.Property == PlacementTargetProperty && IsPopupPinnedOpen)
            {
                _isPinnedOpenSuspended = false;
                UpdatePinnedOpenTargetSubscription();
                QueuePinnedOpen();
            }
        }

        if ((change.Property == WidthProperty || change.Property == MinWidthProperty || change.Property == MaxWidthProperty ||
             change.Property == HeightProperty || change.Property == MinHeightProperty || change.Property == MaxHeightProperty) &&
            MotionMargin != default && _motionTarget is { } surface && TopLevel.GetTopLevel(surface) is PopupRoot root)
        {
            UpdateMotionHostSizing(root);
        }
    }

    /// <summary>
    /// Called when the popup is attached to a logical tree. It selects the frame shadow for the host kind in use
    /// and, while the popup is pinned open, queues the open. Overrides must call the base member to keep both.
    /// </summary>
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        ConfigureFrameShadow();
        if (IsPopupPinnedOpen)
        {
            _isPinnedOpenSuspended = false;
            UpdatePinnedOpenTargetSubscription();
            QueuePinnedOpen();
        }
    }

    /// <summary>
    /// Called when the popup is detached from a logical tree. It invalidates a queued open of the pinned-open mode
    /// and releases the subscriptions to its placement target. Overrides must call the base member to keep both.
    /// </summary>
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        ++_pinnedOpenGeneration;
        ClearPinnedOpenTargetSubscription();
        base.OnDetachedFromLogicalTree(e);
    }

    #endregion

    #region 内部协作 API

    internal void CancelCloseAnimation()
    {
        if (_closeMotionState == PopupCloseState.Requesting)
        {
            _closeRequested = false;
            CloseMotionState = PopupCloseState.Idle;
            SetCurrentValue(IsOpenProperty, true);
            return;
        }
        if (!IsPlayingCloseMotion)
        {
            return;
        }

        var version = ++_motionVersion;
        _closeRequested = false;
        CloseMotionState = PopupCloseState.Idle;
        if (version != _motionVersion)
        {
            return;
        }
        SetCurrentValue(IsOpenProperty, true);
        if (version != _motionVersion)
        {
            return;
        }
        StartOpenMotion();
        _shadowHitTest?.Invalidate();
    }

    internal void CloseForLifecycle()
    {
        ++_pinnedOpenGeneration;
        _isPinnedOpenSuspended = true;
        _closeRequested       = false;
        CancelMotion();
        CloseMotionState      = PopupCloseState.Idle;
        try
        {
            ++_lifecycleCloseDepth;
            Close();
        }
        finally
        {
            --_lifecycleCloseDepth;
        }
    }

    internal void ReleaseDetachedContentCompositionCaches()
    {
        if (IsHostOpen || Child is not { } child)
        {
            return;
        }

        // A reusable Flyout presenter must not retain the retired host's
        // positioning target through Border's cached composition ancestors.
        foreach (var border in GetContentBorders(child))
        {
            border.ReleaseDetachedCompositionCache();
        }
    }

    internal void NotifyFlipped(bool horizontalFlipped,  bool verticalFlipped)
    {
        IsHorizontalFlipped = horizontalFlipped;
        IsVerticalFlipped   = verticalFlipped;
        PositionFlipped?.Invoke(this, new PopupFlippedEventArgs(horizontalFlipped, verticalFlipped));
    }

    internal void NotifySurfaceReady(Control target)
    {
        var changed = !ReferenceEquals(_motionTarget, target);
        var closing = _closeRequested && IsPlayingCloseMotion;
        var version = _motionVersion;
        if (changed)
        {
            version = CancelMotion();
            if (version != _motionVersion)
            {
                return;
            }
            _motionTarget = target;
            if (!IsCurrentMotion(version, target))
            {
                return;
            }
        }
        if (changed && closing)
        {
            // The old template is gone. Keep the replacement hidden and finish
            // the current close intent; a re-open invalidates this queued close.
            PrepareSurface(target);
            if (IsCurrentMotion(version, target) && _closeRequested)
            {
                QueueFinalClose(version, target);
            }
            return;
        }
        if (CanPlayMotion && OpenMotion is not null && _entryPreparation is null && (changed || !_hasRaisedOpened))
        {
            // The host calls this during logical attach, before its first Show.
            // A scoped override preserves the application's underlying opacity.
            PrepareSurface(target);
        }
        if (!IsCurrentMotion(version, target))
        {
            return;
        }
        if (_hasRaisedOpened && IsOpen && _closeMotionState == PopupCloseState.Idle)
        {
            StartOpenMotion();
        }
    }

    internal void HandleCustomPlacement(CustomPopupPlacement placement)
    {
        var          requestedPlacement = RequestedPlacement!.Value;
        var          isUseOverlayHost   = ShouldUseOverlayLayer || !RuntimePlatform.Features.SupportsNativeWindow;
        var          shadowThickness    = GetPositioningShadowThickness(isUseOverlayHost);
        var          hOffset            = HorizontalOffset;
        var          vOffset            = VerticalOffset;
        var          marginToAnchor     = MarginToAnchor;
        PopupAnchor  anchor;
        PopupGravity gravity;
        var          target             = PlacementTarget ?? Parent as Control;
        var          topLevel           = TopLevel.GetTopLevel(target);
        if (requestedPlacement == PlacementMode.Center)
        {
            if (topLevel == null)
            {
                return;
            }

            if (isUseOverlayHost)
            {
                // Overlay mode: the popup is rendered in the OverlayLayer that covers the whole window, in the
                // client-area logical coordinates of the window. Setting AnchorRectangle to the whole client area
                // lets the positioner with Anchor=None / Gravity=None center the popup in the window.
                placement.AnchorRectangle = new Rect(default, topLevel.ClientSize);
            }
            else
            {
                // Popup Root mode: the popup is its own OS window.
                // ManagedPopupPositioner adds parentGeometry.TopLeft to the AnchorRectangle (window client-area
                // logical coordinates) to convert it to screen coordinates, and uses screen.WorkingArea as the flip
                // constraint bound.
                // With the whole window client area as the anchor, the popup ends up centered on the screen that holds
                // the parent window.
                placement.AnchorRectangle = new Rect(default, topLevel.ClientSize);
            }

            anchor  = PopupAnchor.None;
            gravity = PopupGravity.None;
        }
        else
        {
            if (target is null)
            {
                return;
            }

            if (requestedPlacement == PlacementMode.Pointer)
            {
                if (topLevel == null)
                {
                    return;
                }

                var position = topLevel.PointToClient(topLevel.GetLastPointerPosition() ?? default);
                placement.AnchorRectangle = new Rect(position, new Size(1, 1));
                anchor                    = PopupAnchor.TopLeft;
                gravity                   = PopupGravity.BottomRight;
            }
            else
            {
                (anchor, gravity) = PopupUtils.GetAnchorAndGravity(requestedPlacement);
                if (VerticalPlacementFrame is { } frame &&
                    target.GetPresentationSource()?.RootVisual is { } root &&
                    frame.TransformToVisual(root) is { } transform)
                {
                    // The active input supplies the horizontal anchor. Use the frame's
                    // top and bottom edges before constraint handling chooses a side.
                    var frameBounds = new Rect(frame.Bounds.Size).TransformToAABB(transform);
                    placement.AnchorRectangle = placement.AnchorRectangle.WithY(frameBounds.Y).WithHeight(frameBounds.Height);
                }
            }

            // Keep empty bounds while the arrow is hidden or the part is unavailable: a popup in its own window
            // keeps the full shadow on the side that faces the anchor, and ApplyCustomPlacement falls back to the
            // full shadow thickness (see PopupUtils.GetAnchorSideResidualShadow).
            var arrowIndicatorLayoutBounds = default(Rect);
            if (Child is IArrowAwareShadowMaskInfoProvider provider)
            {
                // If ArrowIndicatorLayoutBounds is not initialized yet, force a layout pass
                if (provider.IsArrowVisible() && PopupUtils.CanEnabledArrow(requestedPlacement))
                {
                    Child.Measure(Size.Infinity);
                    Child.Arrange(new Rect(Child.DesiredSize));
                    var arrowDecoratedBox = provider.GetArrowDecoratedBox();
                    if (arrowDecoratedBox is not null)
                    {
                        arrowIndicatorLayoutBounds = arrowDecoratedBox.ArrowIndicatorLayoutBounds;

                        if (IsPointAtCenter)
                        {
                            var delta = PopupUtils.CalculatePointAtCenterDelta(
                                target, arrowDecoratedBox, requestedPlacement, anchor, gravity);
                            hOffset += delta.X;
                            vOffset += delta.Y;
                        }
                    }
                }
            }
            var (flipX, flipY) = PopupUtils.ApplyCustomPlacement(
                placement,
                requestedPlacement,
                isUseOverlayHost,
                hOffset,
                vOffset,
                marginToAnchor,
                shadowThickness,
                anchor,
                gravity,
                arrowIndicatorLayoutBounds);

            NotifyFlipped(flipX, flipY);
            CustomPlacementCallback?.Invoke(placement, shadowThickness, marginToAnchor, isUseOverlayHost, flipX, flipY);
            return;
        }

        // Center reaches this point: AnchorRectangle is already the whole window client area, and the positioner
        // with Anchor/Gravity=None centers the popup automatically.
        // Centering never flips, so only the Offset is set and the flip result is reported as (false, false).
        placement.Anchor  = anchor;
        placement.Gravity = gravity;
        placement.Offset  = new Point(hOffset, vOffset);
        NotifyFlipped(false, false);
        CustomPlacementCallback?.Invoke(placement, shadowThickness, marginToAnchor, isUseOverlayHost, false, false);
    }

    internal PopupMotionGeometry GetMotionGeometry()
    {
        var placement = RequestedPlacement ?? Placement;
        var bounds = default(Rect);
        var anchorBounds = default(Rect);
        var hasAlignment = false;
        var radius = 0d;
        if (_motionTarget is { } surface)
        {
            _motionFrame ??= surface.GetVisualDescendants().OfType<ShadowsAwareContainer>().FirstOrDefault();
            var shape = _motionFrame?.GetContentShape(surface);
            bounds = shape?.Body ?? new Rect(surface.Bounds.Size);
            // The origin inset follows the radius the frame actually draws, so Control-level corner overrides
            // (Controls[ToolTip].ToolTipCornerRadius, popover radii) are honoured instead of the scope-root
            // BorderRadius Tokens.
            radius = (shape?.CornerRadius ?? _motionFrame?.CornerRadius ?? default).TopLeft;
            if (PopupUtils.IsCanonicalAnchorPlacementMode(placement) &&
                ResolvePlacementTarget() is { } anchor && anchor.IsAttachedToVisualTree() && surface.IsAttachedToVisualTree())
            {
                // Cross-root geometry is a value snapshot, never a retained anchor.
                var topLeft = surface.PointToClient(anchor.PointToScreen(default));
                var bottomRight = surface.PointToClient(anchor.PointToScreen(new Point(anchor.Bounds.Width, anchor.Bounds.Height)));
                anchorBounds = new Rect(topLeft, bottomRight);
                hasAlignment = true;
            }
        }
        return new PopupMotionGeometry(placement, IsHorizontalFlipped, IsVerticalFlipped,
            FlowDirection, bounds, anchorBounds, hasAlignment, radius,
            (TemplatedParent as MenuItem)?.SemanticLevel,
            TemplatedParent is NavMenuItem { Mode: NavMenuMode.Horizontal, IsTopLevel: true });
    }

    internal bool CanOpenPinnedPopup()
    {
        return Child is not null && HasValidPinnedOpenTarget();
    }

    #endregion

    #region 内部实现

    private bool CanPlayMotion => IsMotionEnabled && Motion.IsAnimationEnabled(_motionTarget ?? this);

    private void ReleaseTemplateContent(AvaloniaObject previousOwner)
    {
        // The framework clears visual template descendants. Popup.Child lives
        // outside that traversal; its template bindings can otherwise keep a
        // retained business presenter parented to the discarded popup frame.
        var borders = Child is { } child ? GetContentBorders(child).ToArray() : null;
        var parts = Child?.GetSelfAndLogicalDescendants().OfType<StyledElement>()
            .Where(part => ReferenceEquals(part.TemplatedParent, previousOwner)).ToArray();
        CloseForLifecycle();
        if (borders is not null)
        {
            foreach (var border in borders)
            {
                border.ReleaseDetachedCompositionCache();
            }
        }
        if (parts is null || TemplatedParent is not null)
        {
            return;
        }
        foreach (var part in parts)
        {
            if (ReferenceEquals(part.TemplatedParent, previousOwner) &&
                ReferenceEquals(part.FindLogicalAncestorOfType<Popup>(), this))
            {
                part.SetTemplatedParent(null);
            }
        }
    }

    private static IEnumerable<Border> GetContentBorders(Control root)
    {
        // Closed content can still be logical content while its presenter is
        // no longer a visual child. Walk both existing trees, without creating
        // templates or retaining this traversal after the retirement boundary.
        var pending = new Stack<Control>();
        var visited = new HashSet<Control>();
        pending.Push(root);
        while (pending.Count > 0)
        {
            var current = pending.Pop();
            if (!visited.Add(current))
            {
                continue;
            }
            if (current is Border border)
            {
                yield return border;
            }
            foreach (var visual in current.GetVisualChildren().OfType<Control>())
            {
                pending.Push(visual);
            }
            foreach (var logical in current.GetLogicalChildren().OfType<Control>())
            {
                pending.Push(logical);
            }
        }
    }

    private PopupCloseState CloseMotionState
    {
        get => _closeMotionState;
        set
        {
            var wasClosing = IsPlayingCloseMotion;
            _closeMotionState = value;
            var isClosing = IsPlayingCloseMotion;
            if (wasClosing != isClosing)
            {
                RaisePropertyChanged(IsPlayingCloseMotionProperty, wasClosing, isClosing);
            }
        }
    }

    private void HandlePopupOpened(object? sender, EventArgs e)
    {
        _hasRaisedOpened = true;
        _isPinnedOpenSuspended = false;
        _isLogicallyAttachedAtOpen = ((ILogical)this).IsAttachedToLogicalTree;
        _openTopLevel = ResolvePlacementTarget() is { } target
            ? TopLevel.GetTopLevel(target)
            : null;
        _lightDismissRegistration?.Dispose();
        _lightDismissRegistration = ResolvePlacementTarget() is { } placementTarget
            ? PopupLightDismissRegistration.Acquire(this, placementTarget)
            : null;
        _lightDismissRegistration?.Update(this);
        _closeRequested = false;
        CloseMotionState = PopupCloseState.Idle;
        AttachWheelGuard();
        UpdatePlacementTransformTracker();
        StartOpenMotion();
        AttachShadowHitTest();
    }

    private void StartOpenMotion()
    {
        var version = ++_motionVersion;
        _isOpenMotionActive = false;
        if (_motionTarget is not { } target)
        {
            return;
        }
        if (!CanPlayMotion || OpenMotion is null)
        {
            MotionPlayback.CancelAndReset(target);
            ClearEntryPreparation();
            return;
        }

        // A single layout boundary prepares the physical host. Playback itself
        // always follows Avalonia's clock, with no periodic Dispatcher work.
        _isOpenMotionActive = true;
        _shadowHitTest?.Invalidate();
        Dispatcher.Post(() => PlayOpenMotionWhenPositioned(version, target, Stopwatch.GetTimestamp()), DispatcherPriority.Loaded);
    }

    private void PlayOpenMotionWhenPositioned(long version, Control target, long startedAt)
    {
        if (!IsCurrentMotion(version, target) || !IsOpen || _closeMotionState != PopupCloseState.Idle)
        {
            return;
        }

        if (!IsUsingOverlayLayer &&
            OpenMotion is { Origin: null } and IPopupMotion { RequiresPositionedAnchorForEntry: true } && CanPlayMotion &&
            PopupUtils.IsCanonicalAnchorPlacementMode(RequestedPlacement ?? Placement) &&
            TopLevel.GetTopLevel(target) is PopupRoot root && root.IsWaylandPopupPositionPending())
        {
            // Wayland supplies the PopupRoot position asynchronously. Keep the
            // entry hidden while waiting, even when the final position is (0, 0).
            if (Stopwatch.GetElapsedTime(startedAt) < TimeSpan.FromSeconds(1))
            {
                root.RequestAnimationFrame(_ => PlayOpenMotionWhenPositioned(version, target, startedAt));
                return;
            }

            // A host without a configure cannot supply an aligned origin.
            _isOpenMotionActive = false;
            ClearEntryPreparation();
            if (IsCurrentMotion(version, target))
            {
                _shadowHitTest?.Invalidate();
            }
            return;
        }

        _ = PlayOpenMotionAsync(version, target);
    }

    private async Task PlayOpenMotionAsync(long version, Control target)
    {
        try
        {
            await PlayOpenMotionCoreAsync(version, target);
        }
        finally
        {
            if (version == _motionVersion)
            {
                _isOpenMotionActive = false;
                _activeDefaultOrigin = null;
                _shadowHitTest?.Invalidate();
            }
        }
    }

    private async Task PlayOpenMotionCoreAsync(long version, Control target)
    {
        if (!IsCurrentMotion(version, target) || !IsOpen || _closeMotionState != PopupCloseState.Idle)
        {
            return;
        }
        var preparation = _entryPreparation;
        _entryPreparation = null;
        try
        {
            Task<MotionResult> playback;
            try
            {
                target.UpdateLayout();
                // Layout may synchronously re-enter or replace the host. Capture
                // the effective definition only after it settles.
                if (!IsCurrentMotion(version, target) || !IsOpen || _closeMotionState != PopupCloseState.Idle)
                {
                    return;
                }
                if (!CanPlayMotion || OpenMotion is not { } motion)
                {
                    MotionPlayback.CancelAndReset(target);
                    return;
                }
                var playbackMotion = ResolvePlaybackMotion(motion);
                PrepareMotionMargin(version, target, motion, playbackMotion);
                if (!IsCurrentMotion(version, target) || !IsOpen || _closeMotionState != PopupCloseState.Idle)
                {
                    return;
                }
                if (!CanPlayMotion || OpenMotion is not { } latestMotion)
                {
                    MotionPlayback.CancelAndReset(target);
                    return;
                }
                if (!ReferenceEquals(latestMotion, motion))
                {
                    playbackMotion = ResolvePlaybackMotion(latestMotion);
                }
                playback = playbackMotion.PlayAsync(target, MotionDuration, CancellationToken.None);
            }
            finally
            {
                preparation?.Dispose();
            }
            var result = await playback;
            if (IsCurrentMotion(version, target) && result.Outcome == MotionOutcome.NotReady)
            {
                MotionPlayback.CancelAndReset(target);
            }
        }
        catch (Exception exception)
        {
            Avalonia.Logging.Logger.TryGet(Avalonia.Logging.LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                this, "Popup entry motion failed: {Exception}", exception);
            if (IsCurrentMotion(version, target))
            {
                MotionPlayback.CancelAndReset(target);
            }
        }
    }

    private void HandlePopupClosed(object? sender, EventArgs e)
    {
        DetachShadowHitTest();
        _hasRaisedOpened = false;
        _isOpenMotionActive = false;
        _activeDefaultOrigin = null;
        var lightDismiss = _lightDismissRegistration;
        _lightDismissRegistration = null;
        var previousVersion = _motionVersion;
        lightDismiss?.Dispose();
        if (previousVersion != _motionVersion)
        {
            return;
        }
        var version = CancelMotion();
        if (version != _motionVersion)
        {
            return;
        }
        MotionMargin = default;
        if (version != _motionVersion)
        {
            return;
        }
        _closeRequested              = false;
        CloseMotionState             = PopupCloseState.Idle;
        if (version != _motionVersion)
        {
            return;
        }
        _isLogicallyAttachedAtOpen    = false;
        _motionTarget                  = null;
        _openTopLevel                 = null;
        ClearPlacementTransformTracker();
        DetachWheelGuard();
    }

    private long CancelMotion()
    {
        var version = ++_motionVersion;
        var target = _motionTarget;
        var preparation = _entryPreparation;
        _motionTarget = null;
        _motionFrame = null;
        _entryPreparation = null;
        _isOpenMotionActive = false;
        _activeDefaultOrigin = null;
        Exception? failure = null;
        try
        {
            if (target is not null)
            {
                MotionPlayback.CancelAndReset(target);
            }
        }
        catch (Exception exception)
        {
            failure = exception;
        }
        try
        {
            preparation?.Dispose();
        }
        catch (Exception cleanup) when (failure is not null)
        {
            throw new AggregateException("Popup motion cleanup failed.", failure, cleanup);
        }
        if (failure is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failure).Throw();
        }
        return version;
    }

    private void ClearEntryPreparation()
    {
        var preparation = _entryPreparation;
        _entryPreparation = null;
        preparation?.Dispose();
    }

    private bool IsCurrentMotion(long version, Control target) =>
        version == _motionVersion && ReferenceEquals(target, _motionTarget);

    private void HandlePopupClosing(object? sender, CancelEventArgs e)
    {
        if (_lifecycleCloseDepth > 0 ||
            !CanDelayCloseForMotion() ||
            (IsPopupPinnedOpen && !HasValidPinnedOpenTarget()))
        {
            return;
        }

        if (IsPopupPinnedOpen)
        {
            e.Cancel = true;
            return;
        }

        if (_closeMotionState == PopupCloseState.Completing)
        {
            // Publish completion only after Avalonia has disposed the old host.
            // A synchronous observer may open a new host from that notification.
                return;
        }

        if (_closeMotionState != PopupCloseState.Idle)
        {
            e.Cancel = true;
            return;
        }

        _closeRequested = true;
        CloseMotionState = PopupCloseState.Requesting;
        try
        {
            CloseRequested?.Invoke(this, e);
        }
        catch
        {
            RestoreCancelledCloseRequest();
            throw;
        }
        if (e.Cancel || !_closeRequested || _closeMotionState != PopupCloseState.Requesting)
        {
            e.Cancel = true;
            RestoreCancelledCloseRequest();
            return;
        }
        CloseMotionState = PopupCloseState.Idle;

        // An entry which still owns its transparency preparation has not been
        // presented. Close the hidden host without exposing a full-size exit.
        if (_entryPreparation is not null || !CanPlayMotion ||
            CloseMotion is null ||
            _motionTarget is not { } target)
        {
            return;
        }

        e.Cancel = true;
        if (_closeMotionState != PopupCloseState.Idle)
        {
            return;
        }

        _isOpenMotionActive = false;
        _closeRequested = true;
        var version = ++_motionVersion;
        ClearEntryPreparation();
        CloseMotionState = PopupCloseState.Pending;
        if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Pending || !_closeRequested)
        {
            return;
        }
        _shadowHitTest?.Invalidate();
        Dispatcher.Post(() => _ = PlayCloseMotionAndCloseAsync(version, target), DispatcherPriority.Loaded);
    }

    private void RestoreCancelledCloseRequest()
    {
        if (_closeMotionState == PopupCloseState.Requesting)
        {
            _closeRequested = false;
            CloseMotionState = PopupCloseState.Idle;
            SetCurrentValue(IsOpenProperty, true);
        }
    }


    private async Task PlayCloseMotionAndCloseAsync(long version, Control target)
    {
        if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Pending)
        {
            return;
        }
        CloseMotionState = PopupCloseState.Playing;
        try
        {
            target.UpdateLayout();
            if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Playing)
            {
                return;
            }
            if (!CanPlayMotion || CloseMotion is not { } motion)
            {
                QueueFinalClose(version, target);
                return;
            }
            var playbackMotion = ResolvePlaybackMotion(motion);
            PrepareMotionMargin(version, target, motion, playbackMotion);
            if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Playing)
            {
                return;
            }
            if (!CanPlayMotion || CloseMotion is not { } latestMotion)
            {
                QueueFinalClose(version, target);
                return;
            }
            if (!ReferenceEquals(latestMotion, motion))
            {
                playbackMotion = ResolvePlaybackMotion(latestMotion);
            }
            var result = await playbackMotion.PlayAsync(target, MotionDuration, CancellationToken.None);
            if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Playing)
            {
                return;
            }
            if (result.Outcome != MotionOutcome.NotReady && (!result.IsCompleted || !Motion.IsCurrent(target, result)))
            {
                CloseMotionState = PopupCloseState.Idle;
                _shadowHitTest?.Invalidate();
                return;
            }
        }
        catch (Exception exception)
        {
            Avalonia.Logging.Logger.TryGet(Avalonia.Logging.LogEventLevel.Error, AtomUILogArea.Control)?.Log(
                this, "Popup exit motion failed: {Exception}", exception);
        }
        if (!IsCurrentMotion(version, target) || _closeMotionState != PopupCloseState.Playing)
        {
            return;
        }
        QueueFinalClose(version, target);
    }

    private void QueueFinalClose(long version, Control target)
    {
        CloseMotionState = PopupCloseState.Completing;
        Dispatcher.Post(() =>
        {
            if (IsCurrentMotion(version, target) && _closeMotionState == PopupCloseState.Completing && _closeRequested)
            {
                Close();
            }
        });
    }

    private AbstractMotion ResolvePlaybackMotion(AbstractMotion motion)
    {
        if (motion is not IPopupMotion popupMotion)
        {
            _activeDefaultOrigin = null;
            return motion;
        }

        // Preparation consumes only a value snapshot. It neither mutates the
        // owner's definition nor creates a geometry subscription or back-binding.
        var resolved = popupMotion.Resolve(GetMotionGeometry(), _activeDefaultOrigin);
        _activeDefaultOrigin = resolved.Origin;
        return resolved;
    }

    private void PrepareMotionMargin(long version, Control target, AbstractMotion definition, AbstractMotion playbackMotion)
    {
        if (definition is not IPopupMotion ||
            playbackMotion is not (ZoomBigInMotion or ZoomBigOutMotion) ||
            TopLevel.GetTopLevel(target) is not PopupRoot root)
        {
            return;
        }

        var required = PopupUtils.CalculateZoomMotionMargin(target.Bounds.Size,
            playbackMotion.Origin.GetValueOrDefault(RelativePoint.Center).ToPixels(target.Bounds.Size), root.RenderScaling);
        var current = MotionMargin;
        // Grow before a new action; do not shrink/release the physical buffer
        // until this host closes. Reversals use their retained playback origin.
        var margin = new Thickness(Math.Max(current.Left, required.Left), Math.Max(current.Top, required.Top),
            Math.Max(current.Right, required.Right), Math.Max(current.Bottom, required.Bottom));
        if (margin == current)
        {
            return;
        }
        MotionMargin = margin;
        if (!IsCurrentMotion(version, target))
        {
            return;
        }
        UpdateMotionHostSizing(root);
        target.UpdateLayout();
    }

    private void UpdateMotionHostSizing(PopupRoot root)
    {
        // Avalonia copies the Popup constraints to its host at LocalValue.
        // The runtime native allocation is therefore not expressible as a
        // theme default; add only the buffer while preserving inherited scale.
        var scale = root.Transform as ScaleTransform;
        var scaleX = scale?.ScaleX ?? 1;
        var scaleY = scale?.ScaleY ?? 1;
        var margin = MotionMargin;
        var horizontal = margin.Left + margin.Right;
        var vertical = margin.Top + margin.Bottom;
        root.SetCurrentValue(WidthProperty, Width * scaleX + horizontal);
        root.SetCurrentValue(MinWidthProperty, MinWidth * scaleX + horizontal);
        root.SetCurrentValue(MaxWidthProperty, MaxWidth * scaleX + horizontal);
        root.SetCurrentValue(HeightProperty, Height * scaleY + vertical);
        root.SetCurrentValue(MinHeightProperty, MinHeight * scaleY + vertical);
        root.SetCurrentValue(MaxHeightProperty, MaxHeight * scaleY + vertical);
    }

    private void SettleDisabledMotion()
    {
        if (_motionTarget is not { } target)
        {
            return;
        }
        var closing = _closeRequested && IsPlayingCloseMotion;
        var version = CancelMotion();
        if (version != _motionVersion)
        {
            return;
        }
        // Disabling retires playback but keeps this still-mounted surface as
        // the target of a later close/re-enable. Cleanup may have replaced it.
        _motionTarget = target;
        if (closing && _closeRequested)
        {
            CloseMotionState = PopupCloseState.Completing;
            Close();
        }
        _shadowHitTest?.Invalidate();
    }

    private bool CanDelayCloseForMotion()
    {
        if (_openTopLevel is null ||
            (_isLogicallyAttachedAtOpen && !((ILogical)this).IsAttachedToLogicalTree))
        {
            return false;
        }

        var target = ResolvePlacementTarget();
        return target is not null &&
               target.IsAttachedToVisualTree() &&
               ReferenceEquals(TopLevel.GetTopLevel(target), _openTopLevel) &&
               target.TransformToVisual(_openTopLevel) is not null;
    }

    private Control? ResolvePlacementTarget()
    {
        return PlacementTarget ?? this.FindLogicalAncestorOfType<Control>();
    }

    private void AttachShadowHitTest()
    {
        DetachShadowHitTest();
        // Once open, Child is parented under the host: for a popup in its own window the TopLevel is the PopupRoot,
        // for an overlay popup it is the owning window.
        if (Child is null || TopLevel.GetTopLevel(Child) is not PopupRoot popupRoot)
        {
            return;
        }

        // Point hit testing (Windows) uses Avalonia's hit test directly: the rounded rectangle and the arrow
        // geometry of the frame renderer can be hit, while a hit against the render list does not include
        // BoxShadow, so the shadow area is not hit. Disabled content still counts as part of the content shape.
        // Platforms that submit the input region in advance (X11, Wayland) cannot query point by point, so the same
        // rounded rectangle and arrow geometry are provided and AtomUI.Native rasterizes them.
        _shadowHitTest = NativePopupShadowHitTest.TryAttach(
            popupRoot,
            () => IsPlayingMotion,
            point => popupRoot.InputHitTest(point, enabledElementsOnly: false) is not null,
            () => popupRoot.GetVisualDescendants().OfType<ShadowsAwareContainer>().FirstOrDefault()
                           ?.GetContentShape(popupRoot));
    }

    // The Child of a popup in its own window is attached to the visual tree during the first layout of the
    // PopupRoot, before the native window is mapped (WindowBase.Show lays out first and then calls
    // PlatformImpl.Show).
    // This is the only moment before Opened that yields a PopupRoot that is not mapped yet; Avalonia offers no
    // other public callback for it.
    private void HandleChildAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // In Avalonia 12 the visual root is the TopLevelHost that wraps the TopLevel, so this check uses the owning
        // TopLevel.
        if (_shadowHitTest is null && sender is Visual child && TopLevel.GetTopLevel(child) is PopupRoot popupRoot)
        {
            NativePopupShadowHitTest.SuspendInputBeforeShow(popupRoot);
        }
    }

    private void DetachShadowHitTest()
    {
        var shadowHitTest = _shadowHitTest;
        _shadowHitTest = null;
        shadowHitTest?.Dispose();
    }

    private void PrepareSurface(Control target)
    {
        var preparation = new CompositeDisposable();
        _entryPreparation = preparation;
        preparation.Add(target.SetValue(OpacityProperty, 0, Avalonia.Data.BindingPriority.Animation) ?? Disposable.Empty);
    }

    private enum PopupCloseState
    {
        Idle,
        Requesting,
        Pending,
        Playing,
        Completing
    }

    private Thickness GetPositioningShadowThickness(bool isUseOverlayHost)
    {
        return (isUseOverlayHost ? OverlayHostShadow : PopupRootShadow).Thickness();
    }

    private void QueuePinnedOpen()
    {
        var generation = ++_pinnedOpenGeneration;
        Dispatcher.Post(() =>
        {
            if (generation != _pinnedOpenGeneration ||
                !IsPopupPinnedOpen ||
                _isPinnedOpenSuspended ||
                IsOpen ||
                Child is null)
            {
                return;
            }

            if (!CanOpenPinnedPopup())
            {
                return;
            }

            IsOpen = true;
            CoerceValue(IsOpenProperty);
        });
    }

    private void UpdatePinnedOpenTargetSubscription()
    {
        var target = IsPopupPinnedOpen ? ResolvePlacementTarget() : null;
        if (ReferenceEquals(_pinnedOpenTrackingTarget, target))
        {
            return;
        }

        ClearPinnedOpenTargetSubscription();
        if (target is null)
        {
            return;
        }

        _pinnedOpenTrackingTarget = target;
        target.AttachedToVisualTree += HandlePinnedOpenTargetAttached;
        target.DetachedFromVisualTree += HandlePinnedOpenTargetDetached;
        if (target.IsAttachedToVisualTree())
        {
            SubscribeToPinnedOpenTargetState(target);
        }
    }

    private void ClearPinnedOpenTargetSubscription()
    {
        if (_pinnedOpenTrackingTarget is not null)
        {
            _pinnedOpenTrackingTarget.AttachedToVisualTree -= HandlePinnedOpenTargetAttached;
            _pinnedOpenTrackingTarget.DetachedFromVisualTree -= HandlePinnedOpenTargetDetached;
            _pinnedOpenTrackingTarget = null;
        }
        ClearPinnedOpenTargetStateSubscriptions();
    }

    private void HandlePinnedOpenTargetAttached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is Control target && ReferenceEquals(target, _pinnedOpenTrackingTarget))
        {
            SubscribeToPinnedOpenTargetState(target);
            ReconcilePinnedOpenTargetState();
        }
    }

    private void HandlePinnedOpenTargetDetached(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (!ReferenceEquals(sender, _pinnedOpenTrackingTarget))
        {
            return;
        }

        ClearPinnedOpenTargetStateSubscriptions();
        CloseForLifecycle();
    }

    private void SubscribeToPinnedOpenTargetState(Control target)
    {
        ClearPinnedOpenTargetStateSubscriptions();
        Visual? current = target;
        while (current is not null)
        {
            current.PropertyChanged += HandlePinnedOpenTargetStatePropertyChanged;
            _pinnedOpenTargetStateSubscriptions.Add(current);
            current = current.GetVisualParent();
        }
    }

    private void ClearPinnedOpenTargetStateSubscriptions()
    {
        foreach (var visual in _pinnedOpenTargetStateSubscriptions)
        {
            visual.PropertyChanged -= HandlePinnedOpenTargetStatePropertyChanged;
        }
        _pinnedOpenTargetStateSubscriptions.Clear();
    }

    private void HandlePinnedOpenTargetStatePropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.IsVisibleProperty ||
            e.Property == Visual.BoundsProperty ||
            e.Property == InputElement.IsEffectivelyEnabledProperty)
        {
            ReconcilePinnedOpenTargetState();
        }
    }

    private void ReconcilePinnedOpenTargetState()
    {
        if (!IsPopupPinnedOpen || _pinnedOpenTrackingTarget is null)
        {
            return;
        }

        if (HasValidPinnedOpenTarget())
        {
            _isPinnedOpenReconcileRetried = false;
            _isPinnedOpenSuspended = false;
            QueuePinnedOpen();
            return;
        }

        // In the frame in which IsVisible is restored, the placement target has not been laid out again yet
        // (TransformToVisual is not ready / the size is zero). Judging it invalid right away would miss this
        // restore signal: no later property change triggers a reconcile, and the popup could never open again.
        // When it cannot be presented, retry once after one frame; close as for an invalid target only if the
        // retry still fails.
        if (!_isPinnedOpenReconcileRetried)
        {
            _isPinnedOpenReconcileRetried = true;
            var generation = _pinnedOpenGeneration;
            Dispatcher.Post(() =>
            {
                if (generation == _pinnedOpenGeneration)
                {
                    ReconcilePinnedOpenTargetState();
                }
            }, DispatcherPriority.Loaded);
            return;
        }

        _isPinnedOpenReconcileRetried = false;
        CloseForLifecycle();
    }

    private bool HasValidPinnedOpenTarget()
    {
        var target = ResolvePlacementTarget();
        if (target is not { IsEffectivelyVisible: true, IsEffectivelyEnabled: true } ||
            !target.IsAttachedToVisualTree() ||
            TopLevel.GetTopLevel(target) is not { } topLevel)
        {
            return false;
        }

        return IsPlacementTargetVisibleInTopLevel(target, target.TransformToVisual(topLevel));
    }

    private static bool CoerceIsOpen(AvaloniaObject sender, bool value)
    {
        return value &&
               (sender is not Popup popup ||
                !popup.IsPopupPinnedOpen ||
                popup.CanOpenPinnedPopup());
    }

    private void ConfigureFrameShadow()
    {
        if (IsUsingOverlayLayer)
        {
            FrameShadow = OverlayHostShadow;
        }
        else
        {
            FrameShadow = PopupRootShadow;
        }
    }

    private void AttachWheelGuard()
    {
        if (_wheelGuardSubscribedOn is not null)
        {
            return;
        }
        if (Child is not InputElement child)
        {
            return;
        }
        child.AddHandler(InputElement.PointerWheelChangedEvent, HandleChildPointerWheelBubbled,
            RoutingStrategies.Bubble);
        _wheelGuardSubscribedOn = child;
    }

    private void DetachWheelGuard()
    {
        if (_wheelGuardSubscribedOn is null)
        {
            return;
        }
        _wheelGuardSubscribedOn.RemoveHandler(InputElement.PointerWheelChangedEvent, HandleChildPointerWheelBubbled);
        _wheelGuardSubscribedOn = null;
    }

    private static void HandleChildPointerWheelBubbled(object? sender, PointerWheelEventArgs e)
    {
        // AddHandler did not ask for handledEventsToo, so an inner ScrollViewer or list that already marked the
        // event Handled keeps this handler from being called.
        // Reaching this point means nothing inside the popup consumed the event: swallow it at the popup boundary
        // so that it does not leak to an outer ScrollViewer and cause unintended scrolling.
        e.Handled = true;
    }

    private void UpdatePlacementTransformTracker()
    {
        var target = ShouldTrackPlacementTransform()
            ? PlacementTarget ?? this.FindLogicalAncestorOfType<Control>()
            : null;
        if (ReferenceEquals(_placementTransformTrackingTarget, target))
        {
            return;
        }

        ClearPlacementTransformTracker();
        if (target is null)
        {
            return;
        }

        _placementTransformTrackingTarget = target;
        _placementTransformTracker = TransformTrackingHelper.Track(
            target,
            true,
            HandlePlacementTransformChanged);
    }

    private bool ShouldTrackPlacementTransform()
    {
        if (!IsOpen)
        {
            return false;
        }

        var placement = RequestedPlacement ?? Placement;
        return placement is not PlacementMode.Pointer and not PlacementMode.Center;
    }

    private void ClearPlacementTransformTracker()
    {
        _placementTransformTracker?.Dispose();
        _placementTransformTracker       = null;
        _placementTransformTrackingTarget = null;
    }

    private void HandlePlacementTransformChanged(Visual target, Matrix? matrix)
    {
        if (IsOpen && ReferenceEquals(_placementTransformTrackingTarget, target))
        {
            if (!IsPlacementTargetVisibleInTopLevel(target, matrix))
            {
                if (IsPopupPinnedOpen)
                {
                    CloseForLifecycle();
                }
                else
                {
                    Close();
                }
                return;
            }

            this.HandlePositionChange();
        }
    }

    private static bool IsPlacementTargetVisibleInTopLevel(Visual target, Matrix? matrix)
    {
        if (matrix is null ||
            TopLevel.GetTopLevel(target) is not { } topLevel)
        {
            return false;
        }

        var targetBounds  = new Rect(default, target.Bounds.Size).TransformToAABB(matrix.Value);
        var visibleBounds = new Rect(default, topLevel.ClientSize);
        if (targetBounds.Width <= 0 || targetBounds.Height <= 0)
        {
            return visibleBounds.Contains(targetBounds.Position);
        }

        return visibleBounds.Intersects(targetBounds);
    }

    #endregion
}

/// <summary>
/// Reports the flip result of a placement pass of a <see cref="Popup" />, raised with the
/// <see cref="Popup.PositionFlipped" /> event.
/// </summary>
public class PopupFlippedEventArgs : EventArgs
{
    /// <summary>
    /// Whether the popup is shown on the other side of its target along the X axis.
    /// </summary>
    public bool HorizontalFlipped { get; set; }
    /// <summary>
    /// Whether the popup is shown on the other side of its target along the Y axis.
    /// </summary>
    public bool VerticalFlipped { get; set; }

    /// <summary>
    /// Initializes the instance with the flip results of both axes.
    /// </summary>
    /// <param name="horizontalFlipped">
    /// Whether the popup is shown on the other side of its target along the X axis.
    /// </param>
    /// <param name="verticalFlipped">
    /// Whether the popup is shown on the other side of its target along the Y axis.
    /// </param>
    public PopupFlippedEventArgs(bool horizontalFlipped, bool verticalFlipped)
    {
        HorizontalFlipped = horizontalFlipped;
        VerticalFlipped   = verticalFlipped;
    }
}
