using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>
/// Identifies the request that selected a new carousel page.
/// </summary>
public enum CarouselChangeSource
{
    /// <summary>
    /// The page change was requested by a user gesture, for example a swipe or a click on a navigation button.
    /// This base class never reports it; a derived carousel passes it for its own gestures.
    /// </summary>
    User,
    /// <summary>
    /// The page change was made by an autoplay tick.
    /// </summary>
    AutoPlay,
    /// <summary>
    /// The selected index changed because the item collection no longer has the old index, not because a page
    /// change was requested.
    /// </summary>
    Resize,
    /// <summary>
    /// The page change was requested by code, for example by <see cref="AbstractCarousel.Next" />. It is also
    /// reported when the selected index changes without a request and <see cref="Resize" /> does not apply.
    /// </summary>
    Programmatic
}

/// <summary>
/// Base class of carousel controls: each item is one page and the selected item is the current page.
/// By default one item is selected whenever any exist.
/// Autoplay runs only while the control is attached, enabled and visible and nothing pauses it.
/// </summary>
public abstract class AbstractCarousel : SelectingItemsControl, IMotionAwareControl
{
    /// <summary>
    /// Identifies the <see cref="IsAutoPlay" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsAutoPlayProperty =
        AvaloniaProperty.Register<AbstractCarousel, bool>(nameof(IsAutoPlay));

    /// <summary>
    /// Identifies the <see cref="AutoPlaySpeed" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty =
        AvaloniaProperty.Register<AbstractCarousel, TimeSpan>(nameof(AutoPlaySpeed), TimeSpan.FromSeconds(3));

    /// <summary>
    /// Identifies the <see cref="IsInfinite" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsInfiniteProperty =
        AvaloniaProperty.Register<AbstractCarousel, bool>(nameof(IsInfinite), true);

    /// <summary>
    /// Identifies the <see cref="IsMotionEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCarousel>();

    /// <summary>
    /// Identifies the <see cref="IsSwipeEnabled" /> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSwipeEnabledProperty =
        AvaloniaProperty.Register<AbstractCarousel, bool>(nameof(IsSwipeEnabled));

    /// <summary>
    /// Gets or sets whether the carousel advances to the next page by itself. Each tick shows the next page for
    /// the same interval, wrapping around when <see cref="CanLoop" /> is true.
    /// Autoplay runs only while the control is attached, enabled and visible, <see cref="IsMotionEnabled" /> is
    /// <see langword="true" />, there are at least two pages and nothing pauses it. It starts and stops by itself
    /// when any of these conditions changes.
    /// </summary>
    public bool IsAutoPlay
    {
        get => GetValue(IsAutoPlayProperty);
        set => SetValue(IsAutoPlayProperty, value);
    }

    /// <summary>
    /// Gets or sets how long each page stays on screen during autoplay. A value of zero or less keeps autoplay
    /// from running. The timer applies a change immediately, also while autoplay runs.
    /// The theme forwards the value to the pagination, whose progress animation lasts as long.
    /// </summary>
    public TimeSpan AutoPlaySpeed
    {
        get => GetValue(AutoPlaySpeedProperty);
        set => SetValue(AutoPlaySpeedProperty, value);
    }

    /// <summary>
    /// Gets or sets whether the carousel loops. While <see langword="true" />, moving past the last page continues
    /// with the first page and moving before the first page continues with the last page. While
    /// <see langword="false" />, <see cref="Next" /> and <see cref="Previous" /> stop at the last and first page.
    /// </summary>
    public bool IsInfinite
    {
        get => GetValue(IsInfiniteProperty);
        set => SetValue(IsInfiniteProperty, value);
    }

    /// <summary>
    /// Gets or sets whether motion runs on the carousel. The theme sets it from the <c>EnableMotion</c> token of
    /// the carousel tokens and forwards it to the pagination. A value set on the carousel overrides the token
    /// value; clearing it returns to the token.
    /// While <see langword="false" />, autoplay does not run even if <see cref="IsAutoPlay" /> is
    /// <see langword="true" />.
    /// </summary>
    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets whether a drag with the pointer changes the page. This class only stores the value.
    /// A derived carousel such as <c>Carousel</c> reads it to track the drag and pauses autoplay meanwhile.
    /// </summary>
    public bool IsSwipeEnabled
    {
        get => GetValue(IsSwipeEnabledProperty);
        set => SetValue(IsSwipeEnabledProperty, value);
    }

    private DispatcherTimer? _autoPlayTimer;
    private MotionGate? _motionGate;
    private bool _isInteractionPaused;
    private bool _isAutoPlayHostAvailable = true;
    private CarouselChangeSource? _pendingSource;

    static AbstractCarousel()
    {
        SelectionModeProperty.OverrideDefaultValue<AbstractCarousel>(SelectionMode.AlwaysSelected);
        AutoScrollToSelectedItemProperty.OverrideDefaultValue<AbstractCarousel>(false);
    }

    /// <summary>
    /// Moves to the next page and reports <see cref="CarouselChangeSource.Programmatic" /> as the origin.
    /// The carousel wraps to the first page when <see cref="CanLoop" /> is true and otherwise stays on the last
    /// page. Nothing happens while there are no items.
    /// </summary>
    public void Next() => MoveBy(1, CarouselChangeSource.Programmatic);

    /// <summary>
    /// Moves to the previous page and reports <see cref="CarouselChangeSource.Programmatic" /> as the origin.
    /// The carousel wraps to the last page when <see cref="CanLoop" /> is true and otherwise stays on the first
    /// page. Nothing happens while there are no items.
    /// </summary>
    public void Previous() => MoveBy(-1, CarouselChangeSource.Programmatic);

    /// <summary>
    /// Gets whether <see cref="MoveBy" /> wraps around at the first and last page. The default is the value of
    /// <see cref="IsInfinite" />; a derived carousel overrides it to use a different rule.
    /// </summary>
    protected virtual bool CanLoop => IsInfinite;

    /// <summary>
    /// Moves the current page by <paramref name="offset" /> pages and passes <paramref name="source" /> to
    /// <see cref="RequestPageChange" /> as the origin of the change.
    /// The target index wraps around when <see cref="CanLoop" /> is true and is clamped to the item range otherwise.
    /// Nothing happens while there are no items or the target page is already the current page.
    /// </summary>
    /// <param name="offset">
    /// The number of pages to move. A negative value moves backwards.
    /// </param>
    /// <param name="source">
    /// The origin of the page change, reported to <see cref="OnPageChanged" />.
    /// </param>
    protected void MoveBy(int offset, CarouselChangeSource source)
    {
        if (ItemCount == 0)
        {
            return;
        }

        var target = (long)SelectedIndex + offset;
        if (CanLoop)
        {
            target %= ItemCount;
            if (target < 0)
            {
                target += ItemCount;
            }
        }
        else
        {
            target = Math.Clamp(target, 0, ItemCount - 1);
        }
        RequestPageChange((int)target, source);
    }

    /// <summary>
    /// Selects the page at <paramref name="index" /> as the current page and records <paramref name="source" /> as
    /// the origin of that change; without a request the change is reported as
    /// <see cref="CarouselChangeSource.Programmatic" />.
    /// The index is ignored when it is outside the item range or is already the current page.
    /// The value is set with <c>SetCurrentValue</c>, so a binding on the selected index stays in place.
    /// A derived carousel that overrides this method calls the base member to apply the request.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the page to show.
    /// </param>
    /// <param name="source">
    /// The origin of the page change, reported to <see cref="OnPageChanged" />.
    /// </param>
    protected virtual void RequestPageChange(int index, CarouselChangeSource source)
    {
        if (index < 0 || index >= ItemCount || SelectedIndex == index)
        {
            return;
        }

        var previousSource = _pendingSource;
        _pendingSource = source;
        try
        {
            SetCurrentValue(SelectedIndexProperty, index);
        }
        finally
        {
            _pendingSource = previousSource;
        }
    }

    /// <summary>
    /// Called after the selected index has changed and the autoplay timer has been restarted for the new page.
    /// The default implementation does nothing; a derived carousel overrides it to react to page changes.
    /// </summary>
    /// <param name="oldIndex">
    /// The index of the page that was current before the change.
    /// </param>
    /// <param name="newIndex">
    /// The index of the page that is current now.
    /// </param>
    /// <param name="source">
    /// The origin of the page change.
    /// </param>
    protected virtual void OnPageChanged(int oldIndex, int newIndex, CarouselChangeSource source)
    {
    }

    /// <summary>
    /// Called on each autoplay tick while autoplay can run. The default moves one page forward and reports
    /// <see cref="CarouselChangeSource.AutoPlay" /> as the origin. A derived carousel overrides it to run other
    /// logic; call the base member to keep moving forward.
    /// </summary>
    protected virtual void OnAutoPlayTick() => MoveBy(1, CarouselChangeSource.AutoPlay);

    /// <summary>
    /// Pauses or resumes autoplay while a user interaction is in progress. A derived carousel calls it when such
    /// an interaction starts and ends. Autoplay resumes with a full <see cref="AutoPlaySpeed" /> interval, and
    /// detaching the control clears the pause.
    /// </summary>
    /// <param name="paused">
    /// <see langword="true" /> stops the timer until this method is called with <see langword="false" />.
    /// </param>
    protected void SetAutoPlayInteractionPaused(bool paused)
    {
        if (_isInteractionPaused != paused)
        {
            _isInteractionPaused = paused;
            UpdateAutoPlayTimer();
        }
    }

    /// <summary>
    /// Pauses or resumes autoplay according to whether the host of the carousel can show pages. The host counts
    /// as available until a derived carousel calls this method, for example when the host window is minimized.
    /// </summary>
    /// <param name="available">
    /// <see langword="false" /> stops the timer until this method is called with <see langword="true" />.
    /// </param>
    protected void SetAutoPlayHostAvailable(bool available)
    {
        if (_isAutoPlayHostAvailable != available)
        {
            _isAutoPlayHostAvailable = available;
            UpdateAutoPlayTimer();
        }
    }

    internal bool IsAutoPlayTimerRunning => _autoPlayTimer?.IsEnabled == true;

    /// <summary>
    /// Called after the autoplay timer has started or stopped. It is not called when the timer restarts after a
    /// page change. The default implementation does nothing.
    /// </summary>
    /// <param name="isRunning">
    /// <see langword="true" /> when the timer has started, <see langword="false" /> when it has stopped.
    /// </param>
    protected virtual void OnAutoPlayRunningChanged(bool isRunning)
    {
    }

    /// <summary>
    /// Calls the base member, then starts the autoplay timer when <see cref="IsAutoPlay" /> is true and the other
    /// conditions for autoplay hold. A derived override calls the base member to keep autoplay working.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureAutoPlay();
    }

    /// <summary>
    /// Stops the autoplay timer and clears the interaction pause, then calls the base member. A derived override
    /// calls the base member so that autoplay stops.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopAutoPlay();
        _isInteractionPaused = false;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Calls the base member and then keeps autoplay in step with the change. A change of the selected index
    /// restarts the autoplay timer and calls <see cref="OnPageChanged" />. A change of <see cref="IsAutoPlay" />
    /// configures autoplay anew. A change of <see cref="AutoPlaySpeed" />, <see cref="IsMotionEnabled" />, the
    /// item count or the enabled state updates the timer.
    /// A derived override calls the base member, otherwise autoplay stops following these changes.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        var pageChanged = change.Property == SelectedIndexProperty;
        var source = _pendingSource;
        if (pageChanged)
        {
            _pendingSource = null;
        }
        base.OnPropertyChanged(change);

        if (pageChanged)
        {
            var oldIndex = change.GetOldValue<int>();
            var newIndex = change.GetNewValue<int>();
            var changeSource = source ?? (oldIndex >= ItemsView.Count && oldIndex >= 0
                ? CarouselChangeSource.Resize : CarouselChangeSource.Programmatic);
            UpdateAutoPlayTimer(restart: true);
            OnPageChanged(oldIndex, newIndex, changeSource);
        }
        else if (change.Property == IsAutoPlayProperty)
        {
            ConfigureAutoPlay();
        }
        else if (change.Property == IsMotionEnabledProperty && _motionGate is { } gate)
        {
            gate.IsMotionEnabled = IsMotionEnabled;
            UpdateAutoPlayTimer();
        }
        else if (change.Property == AutoPlaySpeedProperty || change.Property == ItemCountProperty ||
                 change.Property == IsEffectivelyEnabledProperty)
        {
            UpdateAutoPlayTimer();
        }
    }

    private void ConfigureAutoPlay()
    {
        if (!IsAutoPlay || !this.IsAttachedToVisualTree())
        {
            StopAutoPlay();
            return;
        }

        if (_motionGate is null)
        {
            _motionGate = new MotionGate(this, _ => UpdateAutoPlayTimer()) { IsMotionEnabled = IsMotionEnabled };
        }
        UpdateAutoPlayTimer();
    }

    private void StopAutoPlay()
    {
        _motionGate?.Dispose();
        _motionGate = null;
        if (_autoPlayTimer is { } timer)
        {
            var wasRunning = timer.IsEnabled;
            timer.Stop();
            timer.Tick -= HandleAutoPlayTick;
            _autoPlayTimer = null;
            if (wasRunning)
            {
                OnAutoPlayRunningChanged(false);
            }
        }
    }

    private bool CanAutoPlay => _motionGate?.ShouldRun == true && IsAutoPlay && IsMotionEnabled &&
        IsEffectivelyEnabled && IsEffectivelyVisible && _isAutoPlayHostAvailable && !_isInteractionPaused &&
        AutoPlaySpeed > TimeSpan.Zero && ItemCount > 1;

    private void UpdateAutoPlayTimer(bool restart = false)
    {
        if (!CanAutoPlay)
        {
            if (IsAutoPlayTimerRunning)
            {
                _autoPlayTimer!.Stop();
                OnAutoPlayRunningChanged(false);
            }
            return;
        }

        if (_autoPlayTimer is null)
        {
            _autoPlayTimer = new DispatcherTimer();
            _autoPlayTimer.Tick += HandleAutoPlayTick;
        }
        var wasRunning = _autoPlayTimer.IsEnabled;
        if (restart)
        {
            _autoPlayTimer.Stop();
        }
        _autoPlayTimer.Interval = AutoPlaySpeed;
        _autoPlayTimer.Start();
        if (!wasRunning)
        {
            OnAutoPlayRunningChanged(true);
        }
    }

    private void HandleAutoPlayTick(object? sender, EventArgs e)
    {
        if (CanAutoPlay)
        {
            OnAutoPlayTick();
        }
    }
}
