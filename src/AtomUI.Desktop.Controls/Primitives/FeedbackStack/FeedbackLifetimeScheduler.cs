using System.Diagnostics;
using Avalonia.Threading;

namespace AtomUI.Desktop.Controls;

/// <summary>Coordinates feedback lifetimes through one lazily-created dispatcher timer.</summary>
internal sealed class FeedbackLifetimeScheduler : IDisposable
{
    private static readonly TimeSpan MinimumWakeup = TimeSpan.FromMilliseconds(1);
    private readonly IFeedbackClock _clock;
    private readonly IFeedbackWakeup _wakeup;
    private readonly Dictionary<IFeedbackStackItem, Entry> _entries = new();
    private readonly List<IFeedbackStackItem> _removeItems = new();
    private readonly List<IFeedbackStackItem> _expiredItems = new();
    private bool _isAllPaused;
    private bool _isDisposed;

    internal FeedbackLifetimeScheduler()
        : this(StopwatchFeedbackClock.Instance, new DispatcherFeedbackWakeup())
    {
    }

    internal FeedbackLifetimeScheduler(IFeedbackClock clock, IFeedbackWakeup wakeup)
    {
        _clock = clock;
        _wakeup = wakeup;
    }

    internal TimeSpan ProgressRefreshInterval { get; set; } = TimeSpan.FromMilliseconds(80);

    internal int Count => _entries.Count;

    internal bool IsAllPaused => _isAllPaused;

    internal void Register(IFeedbackStackItem item, TimeSpan duration)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        Remove(item);
        if (duration <= TimeSpan.Zero || item.IsClosing || item.IsClosed)
        {
            return;
        }

        var now = _clock.Now;
        _entries.Add(item, new Entry
        {
            Deadline = now + duration,
            Remaining = duration
        });
        ScheduleNext(now);
    }

    internal bool Remove(IFeedbackStackItem item)
    {
        if (!_entries.Remove(item))
        {
            return false;
        }

        ScheduleNext(_clock.Now);
        return true;
    }

    internal void SetPaused(IFeedbackStackItem item, bool isPaused)
    {
        if (!_entries.TryGetValue(item, out var entry) || entry.IsItemPaused == isPaused)
        {
            return;
        }

        var now = _clock.Now;
        if (isPaused)
        {
            if (!_isAllPaused)
            {
                CaptureRemaining(entry, now);
            }
            entry.IsItemPaused = true;
        }
        else
        {
            entry.IsItemPaused = false;
            if (!_isAllPaused)
            {
                entry.Deadline = now + entry.Remaining;
            }
        }

        ScheduleNext(now);
    }

    internal void SetAllPaused(bool isPaused)
    {
        if (_isAllPaused == isPaused)
        {
            return;
        }

        var now = _clock.Now;
        if (isPaused)
        {
            foreach (var entry in _entries.Values)
            {
                if (!entry.IsItemPaused)
                {
                    CaptureRemaining(entry, now);
                }
            }
        }

        _isAllPaused = isPaused;
        if (!isPaused)
        {
            foreach (var entry in _entries.Values)
            {
                if (!entry.IsItemPaused)
                {
                    entry.Deadline = now + entry.Remaining;
                }
            }
        }

        ScheduleNext(now);
    }

    internal void Refresh()
    {
        if (_isDisposed)
        {
            return;
        }
        ScheduleNext(_clock.Now);
    }

    internal void Clear()
    {
        _entries.Clear();
        _removeItems.Clear();
        _expiredItems.Clear();
        _isAllPaused = false;
        _wakeup.Cancel();
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        Clear();
        _wakeup.Dispose();
    }

    private void OnWakeup()
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            var now = _clock.Now;

            foreach (var pair in _entries)
            {
                var item = pair.Key;
                var entry = pair.Value;
                if (item.IsClosing || item.IsClosed)
                {
                    _removeItems.Add(item);
                    continue;
                }
                if (_isAllPaused || entry.IsItemPaused)
                {
                    continue;
                }

                var remaining = entry.Deadline - now;
                if (remaining <= TimeSpan.Zero)
                {
                    entry.Remaining = TimeSpan.Zero;
                    _expiredItems.Add(item);
                    continue;
                }

                entry.Remaining = remaining;
                if (item.IsProgressVisible && item.IsStackVisible)
                {
                    item.UpdateRemaining(remaining);
                }
            }

            for (var i = 0; i < _removeItems.Count; i++)
            {
                _entries.Remove(_removeItems[i]);
            }
            for (var i = 0; i < _expiredItems.Count; i++)
            {
                _entries.Remove(_expiredItems[i]);
            }
            for (var i = 0; i < _expiredItems.Count; i++)
            {
                _expiredItems[i].RequestClose();
            }
        }
        finally
        {
            // A drained queue may never wake again; release this batch immediately.
            _removeItems.Clear();
            _expiredItems.Clear();
            ScheduleNext(_clock.Now);
        }
    }

    private void ScheduleNext(TimeSpan now)
    {
        _wakeup.Cancel();
        if (_isDisposed || _isAllPaused || _entries.Count == 0)
        {
            return;
        }

        var next = TimeSpan.MaxValue;
        foreach (var pair in _entries)
        {
            var item = pair.Key;
            var entry = pair.Value;
            if (entry.IsItemPaused || item.IsClosing || item.IsClosed)
            {
                continue;
            }

            var remaining = entry.Deadline - now;
            if (remaining < next)
            {
                next = remaining;
            }
            if (item.IsProgressVisible && item.IsStackVisible && ProgressRefreshInterval > TimeSpan.Zero &&
                ProgressRefreshInterval < next)
            {
                next = ProgressRefreshInterval;
            }
        }

        if (next == TimeSpan.MaxValue)
        {
            return;
        }
        if (next < MinimumWakeup)
        {
            next = MinimumWakeup;
        }
        _wakeup.Schedule(next, OnWakeup);
    }

    private static void CaptureRemaining(Entry entry, TimeSpan now)
    {
        var remaining = entry.Deadline - now;
        entry.Remaining = remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    private sealed class Entry
    {
        internal TimeSpan Deadline { get; set; }
        internal TimeSpan Remaining { get; set; }
        internal bool IsItemPaused { get; set; }
    }
}

internal sealed class StopwatchFeedbackClock : IFeedbackClock
{
    internal static StopwatchFeedbackClock Instance { get; } = new();

    private readonly long _origin = Stopwatch.GetTimestamp();

    private StopwatchFeedbackClock()
    {
    }

    public TimeSpan Now => Stopwatch.GetElapsedTime(_origin);
}

internal sealed class DispatcherFeedbackWakeup : IFeedbackWakeup
{
    private DispatcherTimer? _timer;
    private Action? _callback;
    private bool _isDisposed;

    public void Schedule(TimeSpan due, Action callback)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        _callback = callback;
        _timer ??= CreateTimer();
        _timer.Stop();
        _timer.Interval = due;
        _timer.Start();
    }

    public void Cancel()
    {
        _timer?.Stop();
        _callback = null;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        Cancel();
        if (_timer is not null)
        {
            _timer.Tick -= OnTick;
            _timer = null;
        }
    }

    private DispatcherTimer CreateTimer()
    {
        var timer = new DispatcherTimer();
        timer.Tick += OnTick;
        return timer;
    }

    private void OnTick(object? sender, EventArgs e)
    {
        _timer?.Stop();
        var callback = _callback;
        _callback = null;
        callback?.Invoke();
    }
}
