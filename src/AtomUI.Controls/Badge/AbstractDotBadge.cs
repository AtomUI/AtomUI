using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Status of a dot badge, which the theme maps to the color of the dot.
/// </summary>
public enum DotBadgeStatus
{
    /// <summary>
    /// No specific status. Painted with the theme's placeholder text color.
    /// </summary>
    Default,
    /// <summary>
    /// Painted with the theme's success color.
    /// </summary>
    Success,
    /// <summary>
    /// Painted with the theme's info color.
    /// </summary>
    Processing,
    /// <summary>
    /// Painted with the theme's error color.
    /// </summary>
    Error,
    /// <summary>
    /// Painted with the theme's warning color.
    /// </summary>
    Warning
}

/// <summary>
/// Base class of badges that show a status dot. With a <see cref="DecoratedTarget" /> the dot is rendered in the
/// adorner layer over the target's top-right corner; without one the badge consists of the dot and its
/// <see cref="Text" /> alone. Derived classes create the indicator in <c>CreateDotBadgeAdorner</c>.
/// </summary>
public abstract class AbstractDotBadge : Control, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="DotColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> DotColorProperty =
        AvaloniaProperty.Register<AbstractDotBadge, string?>(
            nameof(DotColor));

    /// <summary>
    /// Identifies the <see cref="Status" /> property.
    /// </summary>
    public static readonly StyledProperty<DotBadgeStatus?> StatusProperty =
        AvaloniaProperty.Register<AbstractDotBadge, DotBadgeStatus?>(
            nameof(Status));

    /// <summary>
    /// Identifies the <see cref="Text" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<AbstractDotBadge, string?>(nameof(Text));

    /// <summary>
    /// Identifies the <see cref="DecoratedTarget" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<AbstractDotBadge, Control?>(nameof(DecoratedTarget));

    /// <summary>
    /// Identifies the <see cref="Offset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractDotBadge, Point>(nameof(Offset));

    /// <summary>
    /// Identifies the <see cref="IsBordered" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<AbstractDotBadge, bool>(nameof(IsBordered));

    /// <summary>
    /// Identifies the <see cref="BadgeIsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<AbstractDotBadge, bool>(nameof(BadgeIsVisible), true);
    
    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty
        = MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractDotBadge>();

    /// <summary>
    /// Fill of the dot. A preset color name such as <c>Red</c>, or a status color name such as <c>primary</c>,
    /// <c>success</c>, <c>warning</c>, <c>error</c> or <c>danger</c>, is resolved from the theme and follows theme
    /// changes. Any other value is parsed as a color, for example <c>#52c41a</c>. <see langword="null" />, an
    /// empty string or a value that cannot be resolved removes the override: the color of
    /// <see cref="Status" /> applies, or the theme's default dot color when that is unset too.
    /// </summary>
    public string? DotColor
    {
        get => GetValue(DotColorProperty);
        set => SetValue(DotColorProperty, value);
    }

    /// <summary>
    /// Status of the dot, which selects its color. <see langword="null" /> leaves the theme's default dot
    /// color in effect. An explicit <see cref="DotColor" /> overrides the status color.
    /// </summary>
    public DotBadgeStatus? Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Text shown beside the dot while <see cref="DecoratedTarget" /> is <see langword="null" />. The indicator
    /// theme hides the label while a target is decorated, so the value has no effect then.
    /// </summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// The control the dot decorates. When set, the control becomes a child of this badge and the dot is rendered
    /// in the adorner layer over the control's top-right corner. When <see langword="null" />, the badge consists
    /// of the dot and its <see cref="Text" /> alone. This is the XAML content property.
    /// </summary>
    [Content]
    public Control? DecoratedTarget
    {
        get => GetValue(DecoratedTargetProperty);
        set => SetValue(DecoratedTargetProperty, value);
    }

    /// <summary>
    /// Moves the dot away from the top-right corner of <see cref="DecoratedTarget" />, in device-independent
    /// pixels: X to the right, Y downwards. It is ignored while <see cref="DecoratedTarget" /> is
    /// <see langword="null" />.
    /// </summary>
    public Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Switch for a ring around the dot that separates it from <see cref="DecoratedTarget" />. The value is
    /// forwarded to the indicator, where a theme that draws a ring can select on its <c>IsBordered</c>. The
    /// Desktop dot draws no ring, so the value has no visible effect there. The default is
    /// <see langword="false" />.
    /// </summary>
    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    /// <summary>
    /// Whether the dot is shown. <see langword="false" /> hides only the dot: <see cref="DecoratedTarget" /> stays
    /// attached and visible. Without a target the whole badge, including <see cref="Text" />, is hidden instead.
    /// The default is <see langword="true" />.
    /// </summary>
    public bool BadgeIsVisible
    {
        get => GetValue(BadgeIsVisibleProperty);
        set => SetValue(BadgeIsVisibleProperty, value);
    }
    
    /// <summary>
    /// Plays the show and hide motion of the dot shown over a <see cref="DecoratedTarget" />; with
    /// <see langword="false" /> the dot appears and disappears immediately. A dot without a target always appears
    /// without motion. While the badge is attached, an unset value follows the <c>EnableMotion</c> Token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    private protected AbstractDotBadgeAdorner? _dotBadgeAdorner;
    private protected AdornerLayer? _adornerLayer;
    private const int MaxAdornerLayerRetryCount = 30;
    private bool _adornerLayerRetryScheduled;
    private int _adornerLayerRetryCount;
    private IDisposable? _motionBinding;
    private AncestorVisibilityTracker? _ancestorVisibilityTracker;
    private IDisposable? _themeSubscription;
    private Panel? _standaloneContentHost;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    static AbstractDotBadge()
    {
        AffectsMeasure<AbstractDotBadge>(DecoratedTargetProperty, TextProperty);
        AffectsRender<AbstractDotBadge>(DotColorProperty, StatusProperty);
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractDotBadge>(HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractDotBadge>(VerticalAlignment.Top);
    }

    // Live ancestor-visibility subscriptions; 0 when the tracker is not active.
    internal int TrackedAncestorCount => _ancestorVisibilityTracker?.SubscriptionCount ?? 0;

    /// <summary>
    /// Creates the indicator of the dot badge; <c>DotBadge</c> in AtomUI.Desktop.Controls overrides it.
    /// </summary>
    /// <remarks>
    /// The base class calls it each time it prepares the indicator, then sets <c>IsAdornerMode</c> and places the
    /// indicator in the adorner layer of <c>DecoratedTarget</c>, or as a standalone child when there is no decorated
    /// target. The base class tracks the returned instance only through <c>_dotBadgeAdorner</c>, so the override
    /// itself must store the instance there and call <c>SetupTokenBindings</c>, which forwards <c>Status</c>,
    /// <c>Text</c>, <c>Offset</c>, <c>IsBordered</c> and <c>IsMotionEnabled</c> to it: hiding, colouring and moving
    /// the indicator go through that field. Create the instance only while <c>_dotBadgeAdorner</c> is
    /// <see langword="null" />, because the base class removes only the instance stored there and would leave a
    /// replaced one in place.
    /// </remarks>
    private protected abstract AbstractDotBadgeAdorner CreateDotBadgeAdorner();

    private void PrepareAdorner()
    {
        var dotBadgeAdorner = CreateDotBadgeAdorner();
        if (DecoratedTarget is not null)
        {
            DetachStandaloneAdorner(dotBadgeAdorner);
            AttachChild(DecoratedTarget);
            dotBadgeAdorner.IsAdornerMode = true;
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            // A missing layer is retried instead of thrown on: the layer can still be created after this attach,
            // for example while the host is loading. The retry gives up after MaxAdornerLayerRetryCount attempts,
            // which leaves the dot unshown.
            if (_adornerLayer == null)
            {
                ScheduleAdornerLayerRetry();
                return;
            }

            _adornerLayerRetryCount     = 0;
            _adornerLayerRetryScheduled = false;
            dotBadgeAdorner.ApplyToTarget(_adornerLayer, this);
            UpdateAncestorVisibilityTracking();
            SyncLayerAdornerVisibility();
        }
        else
        {
            DetachLayerAdorner();
            dotBadgeAdorner.IsAdornerMode = false;
            AttachStandaloneAdorner(dotBadgeAdorner);
            dotBadgeAdorner.ShowWithoutMotion();
            IsVisible = true;
        }
    }

    // The dot renders in an AdornerLayer outside this subtree. When an ancestor hides the host subtree with
    // IsVisible=false alone, this control is not detached and Avalonia does not hide the dot, so its visibility
    // must follow the effective visibility explicitly; otherwise the dot drifts on the page with degenerate
    // layout coordinates.
    private void SyncLayerAdornerVisibility()
    {
        if (_dotBadgeAdorner is { IsAdornerMode: true } && _adornerLayer is not null)
        {
            _dotBadgeAdorner.SetCurrentValue(IsVisibleProperty, IsEffectivelyVisible && BadgeIsVisible);
        }
    }

    private void HideAdorner(bool enableMotion)
    {
        // No adorner means the dot was never shown since the last reset, so there is nothing to hide.
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        if (DecoratedTarget is null)
        {
            if (enableMotion)
            {
                var dotBadgeAdorner = _dotBadgeAdorner;
                dotBadgeAdorner.DetachFromTargetAsync(null,
                    true,
                    () =>
                    {
                        if (_dotBadgeAdorner == dotBadgeAdorner)
                        {
                            DetachStandaloneAdorner(dotBadgeAdorner);
                            IsVisible = false;
                        }
                    });
            }
            else
            {
                DetachStandaloneAdorner(_dotBadgeAdorner);
                _dotBadgeAdorner.DetachFromTargetAsync(null, false);
            }
        }
        else
        {
            if (enableMotion)
            {
                var dotBadgeAdorner = _dotBadgeAdorner;
                var adornerLayer    = _adornerLayer;
                if (adornerLayer is null)
                {
                    dotBadgeAdorner.DetachFromTargetAsync(null,
                        true,
                        () =>
                        {
                            if (_dotBadgeAdorner == dotBadgeAdorner)
                            {
                                DetachStandaloneAdorner(dotBadgeAdorner);
                            }
                        });
                }
                else
                {
                    dotBadgeAdorner.DetachFromTargetAsync(adornerLayer,
                        true,
                        () =>
                        {
                            if (_adornerLayer == adornerLayer)
                            {
                                _adornerLayer = null;
                            }
                        });
                }
            }
            else
            {
                DetachStandaloneAdorner(_dotBadgeAdorner);
                _dotBadgeAdorner.DetachFromTargetAsync(_adornerLayer, false);
                _adornerLayer = null;
            }
        }

        if (!enableMotion)
        {
            if (DecoratedTarget is null)
            {
                IsVisible = false;
            }
        }
    }

    /// <summary>
    /// Called when the badge attaches to the visual tree. It attaches <see cref="DecoratedTarget" />, shows the
    /// dot when <see cref="BadgeIsVisible" /> is set, and starts the motion binding, the ancestor visibility
    /// tracking and the theme subscription of <see cref="DotColor" />. Derived overrides must call the base
    /// method; <see cref="OnDetachedFromVisualTree" /> releases what it starts.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        _adornerLayerRetryCount = 0;
        // The target is attached regardless of dot visibility: it must also be attached when BadgeIsVisible is
        // initially false.
        SyncDecoratedTargetChild(null);
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        // The subscription does not fire for the first context, and a re-attached badge reuses its adorner: resolve
        // against the context of this attach (the theme may have changed, or the badge moved to another provider).
        ConfigureDotColor(DotColor);

        UpdateAncestorVisibilityTracking();
        UpdateThemeSubscription();
    }

    // Only a layer adorner can drift when an ancestor is hidden; an inline adorner is a plain child
    // of this control and hides with it, so inline mode needs no subscriptions at all.
    private void UpdateAncestorVisibilityTracking()
    {
        if (DecoratedTarget is not null && this.IsAttachedToVisualTree())
        {
            // The adorner layer's host hides the layer with its adorners, so tracking stops below it.
            var boundary = _adornerLayer?.GetVisualParent();
            if (_ancestorVisibilityTracker is null || !ReferenceEquals(_ancestorVisibilityTracker.Boundary, boundary))
            {
                _ancestorVisibilityTracker?.Dispose();
                _ancestorVisibilityTracker = new AncestorVisibilityTracker(this, SyncLayerAdornerVisibility, boundary);
            }
        }
        else
        {
            _ancestorVisibilityTracker?.Dispose();
            _ancestorVisibilityTracker = null;
        }
    }

    // Named colors (presets and statuses) depend on the theme snapshot; a hex value or no color needs no subscription.
    private void UpdateThemeSubscription()
    {
        if (this.IsAttachedToVisualTree() && BadgeColorUtils.IsThemeDependent(DotColor))
        {
            if (_themeSubscription is null)
            {
                _themeSubscription = s_themeTokenResolver.Subscribe(this, _ => RefreshThemeColor());
                // Subscribing does not report the current snapshot, and a theme switch while detached was
                // missed. A reused adorner still carries the old color, so resolve once on (re)subscription.
                RefreshThemeColor();
            }
        }
        else
        {
            _themeSubscription?.Dispose();
            _themeSubscription = null;
        }
    }

    private void RefreshThemeColor()
    {
        if (_dotBadgeAdorner is not null && DotColor is { } dotColor)
        {
            ConfigureDotColor(dotColor);
        }
    }

    /// <summary>
    /// Called when the badge detaches from the visual tree. It releases the motion binding, the theme
    /// subscription and the ancestor visibility tracking, drops a pending retry for the adorner layer, and removes
    /// the dot without motion. Derived overrides must call the base method.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = null;
        _themeSubscription?.Dispose();
        _themeSubscription = null;
        _ancestorVisibilityTracker?.Dispose();
        _ancestorVisibilityTracker = null;
        Loaded -= HandleAdornerLayerRetryLoaded;
        _adornerLayerRetryScheduled = false;
        _adornerLayerRetryCount     = 0;
        HideAdorner(false);
    }

    private void ScheduleAdornerLayerRetry()
    {
        if (_adornerLayerRetryScheduled ||
            !this.IsAttachedToVisualTree() ||
            _adornerLayerRetryCount >= MaxAdornerLayerRetryCount)
        {
            return;
        }

        _adornerLayerRetryScheduled = true;
        if (IsLoaded)
        {
            if (_adornerLayerRetryCount == 0)
            {
                Dispatcher.UIThread.Post(RetryPrepareAdorner, DispatcherPriority.Loaded);
            }
            else
            {
                DispatcherTimer.RunOnce(RetryPrepareAdorner, TimeSpan.FromMilliseconds(16));
            }
        }
        else
        {
            Loaded += HandleAdornerLayerRetryLoaded;
        }
    }

    private void HandleAdornerLayerRetryLoaded(object? sender, RoutedEventArgs e)
    {
        Loaded -= HandleAdornerLayerRetryLoaded;
        Dispatcher.UIThread.Post(RetryPrepareAdorner, DispatcherPriority.Loaded);
    }

    private void RetryPrepareAdorner()
    {
        if (!_adornerLayerRetryScheduled)
        {
            return;
        }

        _adornerLayerRetryScheduled = false;
        if (!this.IsAttachedToVisualTree() ||
            !BadgeIsVisible ||
            DecoratedTarget is null)
        {
            return;
        }

        _adornerLayerRetryCount++;
        var adornerLayer = AdornerLayer.GetAdornerLayer(this);
        if (adornerLayer is null)
        {
            ScheduleAdornerLayerRetry();
            return;
        }

        PrepareAdorner();
    }

    /// <summary>
    /// Binds the internal indicator stored in <c>_dotBadgeAdorner</c> to <see cref="Status" />, <see cref="Text" />,
    /// <see cref="Offset" />, <see cref="IsBordered" /> and <see cref="IsMotionEnabled" />, so that later changes of
    /// those properties reach the indicator. It does nothing while that field is <see langword="null" />.
    /// </summary>
    /// <remarks>
    /// The base class does not call it. An override of <see cref="CreateDotBadgeAdorner" /> calls it once for each new
    /// instance it stores in <c>_dotBadgeAdorner</c>, as <c>DotBadge</c> does. Overrides must call the base member,
    /// which creates the five bindings.
    /// </remarks>
    private protected virtual void SetupTokenBindings()
    {
        if (_dotBadgeAdorner is not null)
        {
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.StatusProperty] = this[!StatusProperty];
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.TextProperty]   = this[!TextProperty];
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.OffsetProperty] = this[!OffsetProperty];
            _dotBadgeAdorner[!AbstractDotBadgeAdorner.IsBorderedProperty] = this[!IsBorderedProperty];
            _dotBadgeAdorner[!IsMotionEnabledProperty]                = this[!IsMotionEnabledProperty];
        }
    }

    private void AttachChild(Control child)
    {
        if (child.GetVisualParent() == this)
        {
            return;
        }

        child.SetLogicalParent(this);
        VisualChildren.Add(child);
        LogicalChildren.Add(child);
    }

    // A standalone indicator must not be added to Control.VisualChildren directly: a plain control collection
    // lacks the logical attach cascade of a Panel, so the adorner subtree stays unstyled (no ControlTheme setter
    // applies, the desired size stays 0 and the host lays out blank). Attaching it through the internal Panel host
    // supplies the cascade.
    private void AttachStandaloneAdorner(Control adorner)
    {
        if (_standaloneContentHost is null)
        {
            _standaloneContentHost = new Panel();
            AttachChild(_standaloneContentHost);
        }

        if (!_standaloneContentHost.Children.Contains(adorner))
        {
            _standaloneContentHost.Children.Add(adorner);
        }

        // The first attach can run inside the host's attach cascade: the logical root does not reach this Panel
        // yet, so the child never receives AttachedToLogicalTree, no ControlTheme or template setter applies and
        // the desired size stays 0. Set the logical parent explicitly (SetParent, then FindLogicalRoot, then
        // ApplyStyling). Do not detach and re-attach the visual instead: a detach cancels the pending show motion
        // and leaves the indicator at opacity 0. The synchronous set can fail inside the cascade, so a posted call
        // retries it.
        if (((StyledElement)adorner).Parent is null)
        {
            var hostPanel = _standaloneContentHost;
            ((ISetLogicalParent)adorner).SetParent(hostPanel);
            if (((StyledElement)adorner).Parent is null)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_standaloneContentHost == hostPanel &&
                        hostPanel.Children.Contains(adorner) &&
                        ((StyledElement)adorner).Parent is null)
                    {
                        ((ISetLogicalParent)adorner).SetParent(hostPanel);
                    }
                }, DispatcherPriority.Loaded);
            }
        }
    }

    private void DetachStandaloneAdorner(Control? adorner)
    {
        if (adorner is null)
        {
            return;
        }

        if (_standaloneContentHost is not { } host)
        {
            return;
        }

        host.Children.Remove(adorner);
        if (host.Children.Count == 0)
        {
            DetachChild(host);
            _standaloneContentHost = null;
        }
    }

    private void DetachChild(Control? child)
    {
        if (child is null)
        {
            return;
        }

        if (child.GetVisualParent() == this)
        {
            VisualChildren.Remove(child);
        }

        if (LogicalChildren.Contains(child))
        {
            LogicalChildren.Remove(child);
        }

        child.SetLogicalParent(null);
    }

    private void DetachLayerAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        _dotBadgeAdorner.DetachFromTargetAsync(_adornerLayer, false);
        _adornerLayer = null;
    }

    private void ResetDotBadgeAdorner()
    {
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        DetachStandaloneAdorner(_dotBadgeAdorner);
        DetachLayerAdorner();
        _dotBadgeAdorner = null;
    }

    // Attaching and replacing the target is independent of the indicator: while the indicator is not created or
    // hidden, the old target must still be removed and the new one attached.
    private void SyncDecoratedTargetChild(Control? oldDecoratedTarget)
    {
        if (oldDecoratedTarget is not null &&
            !ReferenceEquals(oldDecoratedTarget, DecoratedTarget) &&
            oldDecoratedTarget.GetVisualParent() == this)
        {
            DetachChild(oldDecoratedTarget);
        }

        if (DecoratedTarget is not null && this.IsAttachedToVisualTree())
        {
            AttachChild(DecoratedTarget);
        }
    }

    /// <summary>
    /// Switches the indicator stored in <c>_dotBadgeAdorner</c> and the children of the badge between standalone mode
    /// and adorner mode, as the current <see cref="DecoratedTarget" /> requires. It does nothing while that field is
    /// <see langword="null" />.
    /// </summary>
    /// <param name="oldDecoratedTarget">
    /// A control that is detached from the badge first, usually the previous <see cref="DecoratedTarget" />.
    /// <see langword="null" /> detaches nothing.
    /// </param>
    /// <remarks>
    /// Without a target the indicator leaves adorner mode and the adorner layer without motion, and it becomes a
    /// standalone child of the badge while <see cref="BadgeIsVisible" /> is <see langword="true" />.
    /// With a target the indicator is removed from the standalone host, enters adorner mode and the target is
    /// attached as a child of the badge. The indicator is placed in the target's adorner layer later, when it is
    /// prepared.
    /// The base class calls it without an argument when <see cref="DecoratedTarget" /> changes while the badge is
    /// attached to a visual tree, and prepares the indicator again afterwards when <see cref="BadgeIsVisible" /> is
    /// <see langword="true" />. An override of <see cref="CreateDotBadgeAdorner" /> calls it for a new instance, as
    /// <c>DotBadge</c> does. Overrides must call the base member, which performs the switch.
    /// </remarks>
    private protected virtual void NotifyDecoratedTargetChanged(Control? oldDecoratedTarget = null)
    {
        if (_dotBadgeAdorner is not null)
        {
            DetachChild(oldDecoratedTarget);
            if (DecoratedTarget is null)
            {
                _dotBadgeAdorner.IsAdornerMode = false;
                DetachLayerAdorner();
                if (BadgeIsVisible)
                {
                    AttachStandaloneAdorner(_dotBadgeAdorner);
                }
            }
            else
            {
                DetachStandaloneAdorner(_dotBadgeAdorner);
                _dotBadgeAdorner.IsAdornerMode = true;
                AttachChild(DecoratedTarget);
            }
        }
    }

    /// <summary>
    /// Called when a property of the badge changes. It shows or hides the dot when <see cref="BadgeIsVisible" />
    /// or the badge's own <c>IsVisible</c> changes, rebuilds the dot for a changed
    /// <see cref="DecoratedTarget" />, and applies a changed <see cref="DotColor" /> while the badge is attached.
    /// Derived overrides must call the base method.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsVisibleProperty ||
            change.Property == BadgeIsVisibleProperty)
        {
            var badgeIsVisible = change.GetNewValue<bool>();
            if (badgeIsVisible)
            {
                PrepareAdorner();
            }
            else
            {
                HideAdorner(IsMotionEnabled && IsLoaded);
            }
        }

        if (change.Property == DecoratedTargetProperty)
        {
            SyncDecoratedTargetChild(change.GetOldValue<Control?>());
        }

        if (this.IsAttachedToVisualTree())
        {
            if (change.Property == DecoratedTargetProperty)
            {
                var oldDecoratedTarget = change.GetOldValue<Control?>();
                DetachChild(oldDecoratedTarget);
                if ((oldDecoratedTarget is null) != (DecoratedTarget is null))
                {
                    ResetDotBadgeAdorner();
                }
                NotifyDecoratedTargetChanged();
                if (BadgeIsVisible)
                {
                    PrepareAdorner();
                }

                UpdateAncestorVisibilityTracking();
            }

            if (change.Property == DotColorProperty)
            {
                ConfigureDotColor(change.GetNewValue<string>());
                UpdateThemeSubscription();
            }
        }
    }

    /// <summary>
    /// Writes the resolved color into the internal indicator, or clears it so the indicator falls back to the
    /// theme's indicator color when the color is unset or invalid. It does nothing while no indicator exists.
    /// The base class calls it when the badge attaches, and when <see cref="DotColor" /> or the theme changes
    /// while the badge is attached.
    /// </summary>
    private protected virtual void ConfigureDotColor(string? colorStr)
    {
        if (_dotBadgeAdorner is null)
        {
            return;
        }

        if (BadgeColorUtils.CalculateColor(this, colorStr) is { } brush)
        {
            _dotBadgeAdorner.BadgeDotColor = brush;
        }
        else
        {
            _dotBadgeAdorner.ClearValue(AbstractDotBadgeAdorner.BadgeDotColorProperty);
        }
    }
}
