using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Displays the time left until <see cref="Value" /> or the time passed since it, refreshing itself on a timer
/// while the control is attached to a visual tree and effectively visible.
/// </summary>
public class TimerStatistic : AbstractStatistic
{
    #region 公共属性定义

    /// <summary>
    /// Identifies the <see cref="Value" /> property.
    /// </summary>
    public static readonly StyledProperty<DateTime> ValueProperty =
        AvaloniaProperty.Register<TimerStatistic, DateTime>(nameof(Value));
    
    /// <summary>
    /// Identifies the <see cref="Format" /> property.
    /// </summary>
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<TimerStatistic, string?>(nameof(Format));
    
    /// <summary>
    /// Identifies the <see cref="RefreshDuration" /> property.
    /// </summary>
    public static readonly StyledProperty<TimeSpan> RefreshDurationProperty =
        AvaloniaProperty.Register<TimerStatistic, TimeSpan>(nameof(RefreshDuration), TimeSpan.FromMilliseconds(10));
    
    /// <summary>
    /// The deadline of the countdown, or the start time that the elapsed time counts from. Both are compared with
    /// the local clock, <see cref="DateTime.Now" />. A later time counts down and raises
    /// <see cref="CountdownFinished" /> once when it is reached; an earlier time counts up from it. The default,
    /// <see cref="DateTime.MinValue" />, runs no timer.
    /// </summary>
    public DateTime Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    /// <summary>
    /// The format string that renders the displayed time, for example <c>hh\:mm\:ss\.fff</c>. It is passed to
    /// <see cref="TimeSpan.ToString(string)" /> and is ignored while <see cref="Formatter" /> is set. When it is
    /// <see langword="null" />, the time is shown as hours, minutes and seconds from one hour on, and as minutes and
    /// seconds below.
    /// </summary>
    public string? Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    
    /// <summary>
    /// The interval at which the displayed time is refreshed. It is read when the timer is created: on attachment to
    /// a visual tree, and when <see cref="Value" /> or this property changes while attached. The default is 10 ms.
    /// </summary>
    public TimeSpan RefreshDuration
    {
        get => GetValue(RefreshDurationProperty);
        set => SetValue(RefreshDurationProperty, value);
    }


    /// <summary>
    /// Converts the remaining or the elapsed time into the text to display, replacing the built-in formatting, so
    /// <see cref="Format" /> is not used. As it is a plain field, assigning it does not rebuild the text by itself.
    /// The text is rebuilt when the time is refreshed or <see cref="Format" /> changes on an attached control.
    /// </summary>
    public Func<TimeSpan, string>? Formatter;
    #endregion

    #region 公共事件定义

    /// <summary>
    /// Raised when a countdown reaches <see cref="Value" />: the remaining time has been set to zero and the timer
    /// has been stopped. It is raised once per deadline; setting <see cref="Value" /> again allows it again. It is
    /// not raised while the control counts up.
    /// </summary>
    public event EventHandler? CountdownFinished;

    #endregion

    #region 内部属性定义

    internal static readonly DirectProperty<TimerStatistic, TimeSpan> RemainingTimeProperty =
        AvaloniaProperty.RegisterDirect<TimerStatistic, TimeSpan>(nameof(RemainingTime),
            o => o.RemainingTime,
            (o, v) => o.RemainingTime = v);
    
    internal static readonly DirectProperty<TimerStatistic, string?> RemainingTimeTextProperty =
        AvaloniaProperty.RegisterDirect<TimerStatistic, string?>(nameof(RemainingTimeText),
            o => o.RemainingTimeText,
            (o, v) => o.RemainingTimeText = v);
    
    private TimeSpan _remainingTime;

    internal TimeSpan RemainingTime
    {
        get => _remainingTime;
        set => SetAndRaise(RemainingTimeProperty, ref _remainingTime, value);
    }
    
    private string? _remainingTimeText;

    internal string? RemainingTimeText
    {
        get => _remainingTimeText;
        set => SetAndRaise(RemainingTimeTextProperty, ref _remainingTimeText, value);
    }

    #endregion
    
    private DispatcherTimer? _timer;
    private CompositeDisposable? _effectiveVisibilitySubscriptions;
    private bool _isAttachedToVisualTree;
    private bool _isCountdown;
    private bool _hasCountdownFinished;

    static TimerStatistic()
    {
        AffectsMeasure<TimerStatistic>(ValueProperty,  FormatProperty, RemainingTimeTextProperty);
    }

    /// <summary>
    /// Creates the timer, unstarted, when <see cref="Value" /> is set and starts tracking the effective visibility of
    /// the control, which starts the timer while the control is effectively visible. An override must call the base
    /// member for the time to be refreshed.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isAttachedToVisualTree = true;
        if (HasTimerTarget())
        {
            BuildTimer(false);
            ConfigureEffectiveVisibilityTracking();
        }
    }

    /// <summary>
    /// Stops the timer and ends the tracking of the effective visibility, so that a detached control no longer
    /// refreshes its time. An override must call the base member to release the timer.
    /// </summary>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        ReleaseTimer();
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>
    /// Rebuilds the displayed text when the remaining time or <see cref="Format" /> changes, and rebuilds the timer
    /// when <see cref="Value" /> or <see cref="RefreshDuration" /> changes while the control is attached. A new
    /// <see cref="Value" /> lets <see cref="CountdownFinished" /> be raised again. An override must call the base
    /// member.
    /// </summary>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == RemainingTimeProperty ||
            (change.Property == FormatProperty && _isAttachedToVisualTree))
        {
            GenerateRemainingTimeText();
        }

        if (change.Property == ValueProperty ||
            change.Property == RefreshDurationProperty)
        {
            if (change.Property == ValueProperty)
            {
                _hasCountdownFinished = false;
            }

            if (_isAttachedToVisualTree)
            {
                if (HasTimerTarget())
                {
                    BuildTimer(false);
                    ConfigureEffectiveVisibilityTracking();
                }
                else
                {
                    _effectiveVisibilitySubscriptions?.Dispose();
                    _effectiveVisibilitySubscriptions = null;
                    ReleaseTimer();
                    SetCurrentValue(RemainingTimeProperty, TimeSpan.Zero);
                }
            }
        }
    }

    private void GenerateRemainingTimeText()
    {
        string? formattedText;
        if (Formatter != null)
        {
            formattedText = Formatter(RemainingTime);
        }
        else
        {
            if (Format != null)
            {
                formattedText = RemainingTime.ToString(Format);
            }
            else if (RemainingTime.TotalHours >= 1)
            {
                formattedText = $"{(int)RemainingTime.TotalHours:00}:{RemainingTime.Minutes:00}:{RemainingTime.Seconds:00}";
            }
            else
            {
                formattedText = $"{(int)RemainingTime.TotalMinutes:00}:{RemainingTime.Seconds:00}";
            }
        }
        SetCurrentValue(RemainingTimeTextProperty, formattedText);
    }
    
    private void BuildTimer(bool start)
    {
        ReleaseTimer();

        var now = DateTime.Now;
        _isCountdown          = Value > now;
        _hasCountdownFinished = false;
        
        _timer          =  new DispatcherTimer();
        _timer.Interval =  RefreshDuration;
        _timer.Tick     += HandleTickElapsed;
        if (start)
        {
            _timer.Start();
        }
    }

    private void ReleaseTimer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= HandleTickElapsed;
            _timer = null;
        }
    }

    private void ConfigureEffectiveVisibilityTracking()
    {
        _effectiveVisibilitySubscriptions?.Dispose();
        var subscriptions = new CompositeDisposable();
        _effectiveVisibilitySubscriptions = subscriptions;
        this.TrackEffectiveVisibility(HandleEffectiveVisibilityChanged, subscriptions);
    }

    private void HandleEffectiveVisibilityChanged(bool isEffectivelyVisible)
    {
        if (isEffectivelyVisible)
        {
            RefreshRemainingTime(DateTime.Now);
            if (!_hasCountdownFinished)
            {
                _timer?.Start();
            }
        }
        else
        {
            _timer?.Stop();
        }
    }
    
    private void HandleTickElapsed(object? sender, EventArgs args)
    {
        RefreshRemainingTime(DateTime.Now);
    }

    private void RefreshRemainingTime(DateTime now)
    {
        if (_isCountdown)
        {
            var remaining = Value - now;
            if (remaining <= TimeSpan.Zero)
            {
                SetCurrentValue(RemainingTimeProperty, TimeSpan.Zero);
                NotifyCountdownFinished();
                return;
            }

            SetCurrentValue(RemainingTimeProperty, remaining);
        }
        else
        {
            SetCurrentValue(RemainingTimeProperty, now - Value);
        }
    }

    private bool HasTimerTarget()
    {
        return Value != default;
    }

    private void NotifyCountdownFinished()
    {
        if (_hasCountdownFinished)
        {
            return;
        }

        _hasCountdownFinished = true;
        _timer?.Stop();
        CountdownFinished?.Invoke(this, EventArgs.Empty);
    }
}
