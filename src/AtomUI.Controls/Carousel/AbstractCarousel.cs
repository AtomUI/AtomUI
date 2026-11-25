using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AtomUI.Controls.Commons;

/// <summary>Identifies the request that selected a new carousel page.</summary>
public enum CarouselChangeSource
{
    User,
    AutoPlay,
    Resize,
    Programmatic
}

/// <summary>Provides page selection and lifecycle-aware autoplay for carousel controls.</summary>
public abstract class AbstractCarousel : SelectingItemsControl, IMotionAwareControl
{
    public static readonly StyledProperty<bool> IsAutoPlayProperty =
        AvaloniaProperty.Register<AbstractCarousel, bool>(nameof(IsAutoPlay));

    public static readonly StyledProperty<TimeSpan> AutoPlaySpeedProperty =
        AvaloniaProperty.Register<AbstractCarousel, TimeSpan>(nameof(AutoPlaySpeed), TimeSpan.FromSeconds(3));

    public static readonly StyledProperty<bool> IsInfiniteProperty =
        AvaloniaProperty.Register<AbstractCarousel, bool>(nameof(IsInfinite), true);

    public static readonly StyledProperty<bool> IsMotionEnabledProperty =
        MotionAwareControlProperty.IsMotionEnabledProperty.AddOwner<AbstractCarousel>();

    public static readonly StyledProperty<bool> IsSwipeEnabledProperty =
        AvaloniaProperty.Register<AbstractCarousel, bool>(nameof(IsSwipeEnabled));

    public bool IsAutoPlay
    {
        get => GetValue(IsAutoPlayProperty);
        set => SetValue(IsAutoPlayProperty, value);
    }

    public TimeSpan AutoPlaySpeed
    {
        get => GetValue(AutoPlaySpeedProperty);
        set => SetValue(AutoPlaySpeedProperty, value);
    }

    public bool IsInfinite
    {
        get => GetValue(IsInfiniteProperty);
        set => SetValue(IsInfiniteProperty, value);
    }

    public bool IsMotionEnabled
    {
        get => GetValue(IsMotionEnabledProperty);
        set => SetValue(IsMotionEnabledProperty, value);
    }

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

    public void Next() => MoveBy(1, CarouselChangeSource.Programmatic);

    public void Previous() => MoveBy(-1, CarouselChangeSource.Programmatic);

    protected virtual bool CanLoop => IsInfinite;

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

    protected virtual void OnPageChanged(int oldIndex, int newIndex, CarouselChangeSource source)
    {
    }

    protected virtual void OnAutoPlayTick() => MoveBy(1, CarouselChangeSource.AutoPlay);

    protected void SetAutoPlayInteractionPaused(bool paused)
    {
        if (_isInteractionPaused != paused)
        {
            _isInteractionPaused = paused;
            UpdateAutoPlayTimer();
        }
    }

    protected void SetAutoPlayHostAvailable(bool available)
    {
        if (_isAutoPlayHostAvailable != available)
        {
            _isAutoPlayHostAvailable = available;
            UpdateAutoPlayTimer();
        }
    }

    internal bool IsAutoPlayTimerRunning => _autoPlayTimer?.IsEnabled == true;

    protected virtual void OnAutoPlayRunningChanged(bool isRunning)
    {
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        ConfigureAutoPlay();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        StopAutoPlay();
        _isInteractionPaused = false;
        base.OnDetachedFromVisualTree(e);
    }

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
