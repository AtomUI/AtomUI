using System.Reactive.Disposables;
using AtomUI.Controls;
using Avalonia;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

public class TimerStatistic : AbstractStatistic
{
    #region 公共属性定义

    public static readonly StyledProperty<DateTime> ValueProperty =
        AvaloniaProperty.Register<TimerStatistic, DateTime>(nameof(Value));
    
    public static readonly StyledProperty<string?> FormatProperty =
        AvaloniaProperty.Register<TimerStatistic, string?>(nameof(Format));
    
    public static readonly StyledProperty<TimeSpan> RefreshDurationProperty =
        AvaloniaProperty.Register<TimerStatistic, TimeSpan>(nameof(RefreshDuration), TimeSpan.FromMilliseconds(10));
    
    public DateTime Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }
    
    public string? Format
    {
        get => GetValue(FormatProperty);
        set => SetValue(FormatProperty, value);
    }
    
    public TimeSpan RefreshDuration
    {
        get => GetValue(RefreshDurationProperty);
        set => SetValue(RefreshDurationProperty, value);
    }


    #endregion

    public Func<TimeSpan, string>? Formatter;

    #region 公共事件定义

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
    private bool _requiresTargetClassification = true;

    static TimerStatistic()
    {
        AffectsMeasure<TimerStatistic>(ValueProperty,  FormatProperty, RemainingTimeTextProperty);
    }

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

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isAttachedToVisualTree = false;
        _effectiveVisibilitySubscriptions?.Dispose();
        _effectiveVisibilitySubscriptions = null;
        ReleaseTimer();
        base.OnDetachedFromVisualTree(e);
    }

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
                _requiresTargetClassification = true;
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

        // Rebuilding a clock must not turn an expired countdown into count-up.
        // Only a new public target establishes a new direction and finish state.
        if (_requiresTargetClassification)
        {
            _isCountdown = Value > DateTime.Now;
            _hasCountdownFinished = false;
            _requiresTargetClassification = false;
        }
        
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
