using AtomUI.Theme.Resources;
using AtomUI.Utils;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Size preset of a count badge indicator. It selects the theme values for the indicator height, the corner
/// radius and the text font size.
/// </summary>
public enum CountBadgeSize
{
    /// <summary>
    /// The regular set of those values.
    /// </summary>
    Default,
    /// <summary>
    /// The small set of those values.
    /// </summary>
    Small
}

/// <summary>
/// Base class of badges that show a count or custom content in an indicator. With a
/// <see cref="DecoratedTarget" /> the indicator is rendered in the adorner layer at the target's top-right
/// corner; without one the badge consists of the indicator alone. Derived classes create the indicator in
/// <c>CreateBadgeAdorner</c>.
/// </summary>
public abstract class AbstractCountBadge : Control, IMotionAwareControl
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="BadgeColor" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> BadgeColorProperty =
        AvaloniaProperty.Register<AbstractCountBadge, string?>(
            nameof(BadgeColor));

    /// <summary>
    /// Identifies the <see cref="Count" /> property.
    /// </summary>
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<AbstractCountBadge, int>(nameof(Count),
            coerce: (o, v) => Math.Max(0, v));

    /// <summary>
    /// Identifies the <see cref="DecoratedTarget" /> property.
    /// </summary>
    public static readonly StyledProperty<Control?> DecoratedTargetProperty =
        AvaloniaProperty.Register<AbstractCountBadge, Control?>(nameof(DecoratedTarget));

    /// <summary>
    /// Identifies the <see cref="Offset" /> property.
    /// </summary>
    public static readonly StyledProperty<Point> OffsetProperty =
        AvaloniaProperty.Register<AbstractCountBadge, Point>(nameof(Offset));

    /// <summary>
    /// Identifies the <see cref="OverflowCount" /> property.
    /// </summary>
    public static readonly StyledProperty<int> OverflowCountProperty =
        AvaloniaProperty.Register<AbstractCountBadge, int>(nameof(OverflowCount), 99,
            coerce: (o, v) => Math.Max(0, v));

    /// <summary>
    /// Identifies the <see cref="IsZeroVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsZeroVisibleProperty =
        AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(IsZeroVisible));

    /// <summary>
    /// Identifies the <see cref="Size" /> property.
    /// </summary>
    public static readonly StyledProperty<CountBadgeSize> SizeProperty =
        AvaloniaProperty.Register<AbstractCountBadge, CountBadgeSize>(nameof(Size));

    /// <summary>
    /// Identifies the <see cref="Content" /> property.
    /// </summary>
    public static readonly StyledProperty<object?> ContentProperty =
        AvaloniaProperty.Register<AbstractCountBadge, object?>(nameof(Content));

    /// <summary>
    /// Identifies the <see cref="IsBordered" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsBorderedProperty =
        AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(IsBordered), true);

    /// <summary>
    /// Identifies the <see cref="BadgeIsVisible" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> BadgeIsVisibleProperty =
        AvaloniaProperty.Register<AbstractCountBadge, bool>(nameof(BadgeIsVisible), true);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCountBadge>();

    /// <summary>
    /// Fill of the indicator. A preset color name such as <c>Red</c>, or a status name such as <c>primary</c>,
    /// <c>success</c>, <c>warning</c>, <c>error</c> or <c>danger</c>, is resolved from the theme and follows theme
    /// changes. Any other value is parsed as a color, for example <c>#52c41a</c>. <see langword="null" />, an empty
    /// string or a value that cannot be resolved leaves the theme's indicator color in effect.
    /// </summary>
    public string? BadgeColor
    {
        get => GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    /// <summary>
    /// Number shown in the indicator: the count itself, or <c>OverflowCount+</c> once it exceeds
    /// <see cref="OverflowCount" />. Negative values are clamped to 0, and 0 hides the indicator unless
    /// <see cref="IsZeroVisible" /> is <see langword="true" />. <see cref="Content" /> replaces the count when it is
    /// not <see langword="null" />.
    /// </summary>
    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>
    /// The control the indicator decorates. When set, the control becomes a child of this badge and the indicator is
    /// rendered in the adorner layer over the control's top-right corner. When <see langword="null" />, the badge
    /// consists of the indicator alone. This is the XAML content property.
    /// </summary>
    [Content]
    public Control? DecoratedTarget
    {
        get => GetValue(DecoratedTargetProperty);
        set => SetValue(DecoratedTargetProperty, value);
    }

    /// <summary>
    /// Moves the indicator away from the top-right corner of <see cref="DecoratedTarget" />, in device-independent
    /// pixels: X to the right, Y downwards. It is ignored while <see cref="DecoratedTarget" /> is
    /// <see langword="null" />.
    /// </summary>
    public Point Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    /// <summary>
    /// Highest count shown as a number; above it the indicator shows <c>OverflowCount+</c>. Negative values are
    /// clamped to 0. The default is 99. It has no effect while <see cref="Content" /> is not <see langword="null" />.
    /// </summary>
    public int OverflowCount
    {
        get => GetValue(OverflowCountProperty);
        set => SetValue(OverflowCountProperty, value);
    }

    /// <summary>
    /// Shows the indicator with <c>0</c> when <see cref="Count" /> is 0. The default, <see langword="false" />, hides
    /// it instead. It has no effect while <see cref="Content" /> is not <see langword="null" />.
    /// </summary>
    public bool IsZeroVisible
    {
        get => GetValue(IsZeroVisibleProperty);
        set => SetValue(IsZeroVisibleProperty, value);
    }

    /// <summary>
    /// Size preset of the indicator. <see cref="CountBadgeSize.Small" /> switches the theme to its small indicator
    /// height, corner radius and text font size.
    /// </summary>
    public CountBadgeSize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>
    /// Content shown in the indicator instead of the count. <see langword="null" /> shows the count. An empty string
    /// hides the indicator; any other value is always shown, whatever <see cref="Count" /> and
    /// <see cref="IsZeroVisible" /> say.
    /// </summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>
    /// Draws the outline around the indicator that separates it from its target, using the theme's shadow size and
    /// shadow color tokens. <see langword="false" /> leaves the indicator without an outline.
    /// </summary>
    public bool IsBordered
    {
        get => GetValue(IsBorderedProperty);
        set => SetValue(IsBorderedProperty, value);
    }

    /// <summary>
    /// Whether the indicator is shown. Set it to <see langword="false" /> to hide the indicator whatever the count
    /// says. The badge assigns this property itself when it attaches and whenever <see cref="Content" />,
    /// <see cref="Count" /> or <see cref="IsZeroVisible" /> changes, so an explicit value does not survive those
    /// updates.
    /// </summary>
    public bool BadgeIsVisible
    {
        get => GetValue(BadgeIsVisibleProperty);
        set => SetValue(BadgeIsVisibleProperty, value);
    }

    /// <summary>
    /// Plays the show and hide motion of the indicator; with <see langword="false" /> the indicator appears and
    /// disappears immediately. While the badge is attached, an unset value follows the <c>EnableMotion</c> Token.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    #endregion

    private protected AbstractCountBadgeAdorner? _badgeAdorner;
    private protected AdornerLayer? _adornerLayer;
    private const int MaxAdornerLayerRetryCount = 30;
    private bool _adornerLayerRetryScheduled;
    private int _adornerLayerRetryCount;
    private IDisposable? _motionBinding;
    private AncestorVisibilityTracker? _ancestorVisibilityTracker;
    private IDisposable? _themeSubscription;
    private Panel? _standaloneContentHost;
    private static readonly ThemeTokenResolver s_themeTokenResolver = new();

    static AbstractCountBadge()
    {
        AffectsMeasure<AbstractCountBadge>(DecoratedTargetProperty,
            ContentProperty,
            CountProperty,
            OverflowCountProperty,
            SizeProperty);
        AffectsRender<AbstractCountBadge>(BadgeColorProperty, OffsetProperty);
        // A decorating control must wrap its target instead of stretching to fill the parent slot, as
        // AbstractDotBadge does. Stretching would drag the indicator anchor and the root semantic part marker to the
        // right edge of the parent slot.
        HorizontalAlignmentProperty.OverrideDefaultValue<AbstractCountBadge>(Avalonia.Layout.HorizontalAlignment.Left);
        VerticalAlignmentProperty.OverrideDefaultValue<AbstractCountBadge>(Avalonia.Layout.VerticalAlignment.Top);
    }

    // Live ancestor-visibility subscriptions; 0 when the tracker is not active.
    internal int TrackedAncestorCount => _ancestorVisibilityTracker?.SubscriptionCount ?? 0;

    // A product that delegates its target to another badge owns that transfer.
    internal virtual bool SynchronizesDecoratedTarget => true;

    /// <summary>
    /// Creates the indicator. It is called every time the indicator is prepared, so an override stores the adorner in
    /// <c>_badgeAdorner</c> and returns that same instance on later calls.
    /// </summary>
    /// <remarks>
    /// The base class works only through <c>_badgeAdorner</c>: it shows and hides that instance, places it in and
    /// removes it from the adorner layer, and applies the color to it. An override therefore creates the instance only
    /// while the field is <see langword="null" />, and calls <see cref="SetupTokenBindings" /> and
    /// <see cref="ConfigureDotColor" /> for a new instance, so that the values of the badge reach the indicator.
    /// </remarks>
    private protected abstract AbstractCountBadgeAdorner CreateBadgeAdorner();

    private void PrepareAdorner()
    {
        var badgeAdorner = CreateBadgeAdorner();
        if (DecoratedTarget is not null)
        {
            DetachStandaloneAdorner(badgeAdorner);
            AttachChild(DecoratedTarget);
            badgeAdorner.IsAdornerMode = true;
            _adornerLayer = AdornerLayer.GetAdornerLayer(this);
            // A missing layer is not an error: this also runs before the badge is attached, and attaching prepares the
            // indicator again.
            if (_adornerLayer == null)
            {
                ScheduleAdornerLayerRetry();
                return;
            }

            _adornerLayerRetryCount     = 0;
            _adornerLayerRetryScheduled = false;
            badgeAdorner.ApplyToTarget(_adornerLayer, this);
            UpdateAncestorVisibilityTracking();
            SyncLayerAdornerVisibility();
        }
        else
        {
            DetachLayerAdorner();
            badgeAdorner.IsAdornerMode = false;
            AttachStandaloneAdorner(badgeAdorner);
            badgeAdorner.ApplyToTarget(null, this, () => IsVisible = true);
        }
    }

    // In adorner mode the indicator renders in the AdornerLayer, which is not part of this control's subtree. When
    // an ancestor hides the host subtree with IsVisible=false alone, this control is not detached and Avalonia
    // leaves the visual in the adorner layer visible, so the effective visibility has to be pushed to the indicator
    // explicitly. Without it the indicator stays on the page at degenerate layout coordinates.
    private void SyncLayerAdornerVisibility()
    {
        if (_badgeAdorner is { IsAdornerMode: true } && _adornerLayer is not null)
        {
            _badgeAdorner.SetCurrentValue(IsVisibleProperty, IsEffectivelyVisible && BadgeIsVisible);
        }
    }

    private void HideAdorner(bool enableMotion)
    {
        // Nothing to hide: the indicator is created only when the badge is first shown.
        if (_badgeAdorner is null)
        {
            return;
        }

        if (DecoratedTarget is null)
        {
            if (enableMotion)
            {
                var badgeAdorner = _badgeAdorner;
                badgeAdorner.DetachFromTarget(null,
                    true,
                    () =>
                    {
                        if (_badgeAdorner == badgeAdorner)
                        {
                            DetachStandaloneAdorner(badgeAdorner);
                            IsVisible = false;
                        }
                    });
            }
            else
            {
                DetachStandaloneAdorner(_badgeAdorner);
                _badgeAdorner.DetachFromTarget(null, false);
            }
        }
        else
        {
            if (enableMotion)
            {
                var badgeAdorner = _badgeAdorner;
                var adornerLayer = _adornerLayer;
                if (adornerLayer is null)
                {
                    badgeAdorner.DetachFromTarget(null,
                        true,
                        () =>
                        {
                            if (_badgeAdorner == badgeAdorner)
                            {
                                DetachStandaloneAdorner(badgeAdorner);
                            }
                        });
                }
                else
                {
                    badgeAdorner.DetachFromTarget(adornerLayer,
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
                DetachStandaloneAdorner(_badgeAdorner);
                _badgeAdorner.DetachFromTarget(_adornerLayer, false);
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
    /// Attaches <see cref="DecoratedTarget" /> as a child, derives <see cref="BadgeIsVisible" /> from the count and
    /// content, creates and shows the indicator while that value is <see langword="true" />, and starts the theme and
    /// ancestor-visibility subscriptions. An override must call the base member first.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _motionBinding?.Dispose();
        _motionBinding = this.ConfigureMotionBindingStyle();
        _adornerLayerRetryCount = 0;
        // The target is attached regardless of the indicator. It must be attached even while the badge starts hidden,
        // by a zero count, empty content or BadgeIsVisible=false.
        SyncDecoratedTargetChild(null);
        SetupShowZero();
        if (BadgeIsVisible)
        {
            PrepareAdorner();
        }
        // The subscription does not fire for the first context, and a re-attached badge reuses its adorner: resolve
        // against the context of this attach (the theme may have changed, or the badge moved to another provider).
        ConfigureDotColor(BadgeColor);

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
        if (this.IsAttachedToVisualTree() && BadgeColorUtils.IsThemeDependent(BadgeColor))
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
        if (_badgeAdorner is not null && BadgeColor is { } badgeColor)
        {
            ConfigureDotColor(badgeColor);
        }
    }

    /// <summary>
    /// Disposes the motion binding, the theme subscription and the ancestor-visibility tracking, cancels a pending
    /// adorner layer retry and detaches the indicator without motion. The indicator is kept for the next attach. An
    /// override must call the base member first.
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
    /// Binds the indicator's offset, size, overflow count, count, content, border and motion properties to those of
    /// this badge. The derived class calls it once the indicator exists; it does nothing before that. An override
    /// calls this member to create the bindings.
    /// </summary>
    private protected virtual void SetupTokenBindings()
    {
        if (_badgeAdorner is not null)
        {
            _badgeAdorner[!AbstractCountBadgeAdorner.OffsetProperty]        = this[!OffsetProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.SizeProperty]          = this[!SizeProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.OverflowCountProperty] = this[!OverflowCountProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.CountProperty]         = this[!CountProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.ContentProperty]       = this[!ContentProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.IsBorderedProperty]    = this[!IsBorderedProperty];
            _badgeAdorner[!AbstractCountBadgeAdorner.IsMotionEnabledProperty] = this[!IsMotionEnabledProperty];
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

    // A standalone indicator must not be attached directly to Control.VisualChildren: a plain control collection
    // lacks the logical-attach linkage of a Panel, so the adorner subtree stays unstyled (no ControlTheme setter
    // takes effect, the desired size stays 0 and the host lays out blank). The internal Panel host supplies that
    // linkage.
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

        // The first attach can happen inside the host's attach cascade: the logical root has not reached this Panel
        // yet, the child never gets AttachedToLogicalTree, no ControlTheme or template setter applies and the desired
        // size stays 0. Setting the logical parent explicitly runs SetParent, FindLogicalRoot and ApplyStyling. A
        // visual re-attach is not an option: detaching cancels the pending show motion and leaves the indicator at
        // opacity 0. The synchronous set can fail inside the cascade, so the Post call retries it.
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
        if (_badgeAdorner is null)
        {
            return;
        }

        _badgeAdorner.DetachFromTarget(_adornerLayer, false);
        _adornerLayer = null;
    }

    // Attaching and replacing the target does not depend on the indicator: while the indicator is not created or is
    // hidden, the old target is still removed and the new one attached.
    private void SyncDecoratedTargetChild(Control? oldDecoratedTarget)
    {
        if (!SynchronizesDecoratedTarget)
        {
            return;
        }

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
    /// Moves the existing indicator to the mode that matches the current <see cref="DecoratedTarget" />. With a
    /// target the indicator is switched to adorner mode and the target becomes a child of the badge. Without one it
    /// leaves the adorner layer and is attached as the badge's own content while <see cref="BadgeIsVisible" /> is
    /// <see langword="true" />. It does nothing while no indicator exists.
    /// </summary>
    /// <param name="oldDecoratedTarget">
    /// The target that was replaced, which is detached as a child.
    /// </param>
    private protected virtual void NotifyDecoratedTargetChanged(Control? oldDecoratedTarget = null)
    {
        if (_badgeAdorner is not null)
        {
            DetachChild(oldDecoratedTarget);
            if (DecoratedTarget is null)
            {
                DetachLayerAdorner();
                _badgeAdorner.IsAdornerMode = false;
                if (BadgeIsVisible)
                {
                    AttachStandaloneAdorner(_badgeAdorner);
                }
            }
            else if (DecoratedTarget is not null)
            {
                DetachStandaloneAdorner(_badgeAdorner);
                _badgeAdorner.IsAdornerMode = true;
                AttachChild(DecoratedTarget);
            }
        }
    }

    /// <summary>
    /// Keeps the indicator in step with the badge: <see cref="BadgeIsVisible" /> shows or hides it,
    /// <see cref="DecoratedTarget" /> swaps the attached child and the indicator mode, <see cref="BadgeColor" />
    /// recolours it, and <see cref="Content" />, <see cref="Count" /> and <see cref="IsZeroVisible" /> re-derive
    /// <see cref="BadgeIsVisible" />. An override must call the base member first.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == BadgeIsVisibleProperty)
        {
            if (BadgeIsVisible)
            {
                SetupShowZero();
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
                NotifyDecoratedTargetChanged(change.GetOldValue<Control?>());
                if (BadgeIsVisible)
                {
                    PrepareAdorner();
                }

                UpdateAncestorVisibilityTracking();
            }

            if (change.Property == BadgeColorProperty)
            {
                ConfigureDotColor(change.GetNewValue<string>());
                UpdateThemeSubscription();
            }
        }

        if (change.Property == ContentProperty)
        {
            SetupShowZero();
        }

        if (change.Property == CountProperty ||
            change.Property == IsZeroVisibleProperty)
        {
            if (change.Property == CountProperty)
            {
                var oldCount = change.GetOldValue<int>();
                var newCount = change.GetNewValue<int>();
                if (oldCount > 0 && newCount == 0 && !IsZeroVisible)
                {
                    _badgeAdorner?.PreserveCountTextForHide();
                }
            }
            else if (Count == 0 && IsZeroVisible)
            {
                _badgeAdorner?.RefreshCountText();
            }

            SetupShowZero();
        }
    }

    private void SetupShowZero()
    {
        // Explicit content decides on its own: an empty string means nothing to show, any other value is
        // always shown whatever the count says. Null falls through to the count logic.
        if (Content is string { Length: 0 })
        {
            BadgeIsVisible = false;
        }
        else if (Content is not null)
        {
            BadgeIsVisible = true;
        }
        else if (Count == 0 && !IsZeroVisible)
        {
            BadgeIsVisible = false;
        }
        else if (Count > 0 || IsZeroVisible)
        {
            BadgeIsVisible = true;
        }
    }
    
    /// <summary>
    /// Writes the resolved color into the indicator, or clears it so the indicator falls back to the theme's
    /// indicator color when the color is unset or invalid. Does nothing while no indicator exists. The base class
    /// calls it on attach, when <see cref="BadgeColor" /> changes and when the theme changes; the derived class calls
    /// it once the indicator exists. An override calls this member to apply the resolved color.
    /// </summary>
    /// <param name="colorStr">
    /// The value of <see cref="BadgeColor" /> to resolve.
    /// </param>
    private protected virtual void ConfigureDotColor(string? colorStr)
    {
        if (_badgeAdorner is null)
        {
            return;
        }

        if (BadgeColorUtils.CalculateColor(this, colorStr) is { } brush)
        {
            _badgeAdorner.BadgeColor = brush;
        }
        else
        {
            _badgeAdorner.ClearValue(AbstractCountBadgeAdorner.BadgeColorProperty);
        }
    }
}
