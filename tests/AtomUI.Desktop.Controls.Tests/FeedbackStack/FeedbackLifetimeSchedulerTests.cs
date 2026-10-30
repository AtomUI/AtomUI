using System.Runtime.CompilerServices;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackLifetimeSchedulerTests
{
    [Fact]
    public void Finite_Items_Close_At_Their_Deadline_And_Schedule_The_Nearest_Wakeup()
    {
        var clock = new ManualFeedbackClock();
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(clock, wakeup);
        var early = new TestFeedbackItem();
        var late = new TestFeedbackItem();

        scheduler.Register(late, TimeSpan.FromSeconds(5));
        scheduler.Register(early, TimeSpan.FromSeconds(2));

        wakeup.IsScheduled.ShouldBeTrue();
        wakeup.Due.ShouldBe(TimeSpan.FromSeconds(2));

        clock.Advance(TimeSpan.FromSeconds(2));
        wakeup.Fire();

        early.CloseCount.ShouldBe(1);
        late.CloseCount.ShouldBe(0);
        wakeup.Due.ShouldBe(TimeSpan.FromSeconds(3));

        clock.Advance(TimeSpan.FromSeconds(3));
        wakeup.Fire();

        late.CloseCount.ShouldBe(1);
        wakeup.IsScheduled.ShouldBeFalse();
    }

    [Fact]
    public void Zero_Duration_Items_Are_Permanent_And_Do_Not_Start_A_Wakeup()
    {
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(new ManualFeedbackClock(), wakeup);

        scheduler.Register(new TestFeedbackItem(), TimeSpan.Zero);

        scheduler.Count.ShouldBe(0);
        wakeup.IsScheduled.ShouldBeFalse();
    }

    [Fact]
    public void Pause_Resumes_From_Remaining_Time_Instead_Of_Restarting_Duration()
    {
        var clock = new ManualFeedbackClock();
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(clock, wakeup);
        var item = new TestFeedbackItem();
        scheduler.Register(item, TimeSpan.FromSeconds(5));

        clock.Advance(TimeSpan.FromSeconds(2));
        scheduler.SetPaused(item, true);
        wakeup.IsScheduled.ShouldBeFalse();

        clock.Advance(TimeSpan.FromSeconds(10));
        scheduler.SetPaused(item, false);
        wakeup.Due.ShouldBe(TimeSpan.FromSeconds(3));

        clock.Advance(TimeSpan.FromMilliseconds(2999));
        wakeup.Fire();
        item.CloseCount.ShouldBe(0);

        clock.Advance(TimeSpan.FromMilliseconds(1));
        wakeup.Fire();
        item.CloseCount.ShouldBe(1);
    }

    [Fact]
    public void Whole_Stack_Pause_Can_Coexist_With_Item_Pause()
    {
        var clock = new ManualFeedbackClock();
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(clock, wakeup);
        var first = new TestFeedbackItem();
        var second = new TestFeedbackItem();
        scheduler.Register(first, TimeSpan.FromSeconds(5));
        scheduler.Register(second, TimeSpan.FromSeconds(5));

        clock.Advance(TimeSpan.FromSeconds(1));
        scheduler.SetPaused(first, true);
        scheduler.SetAllPaused(true);
        clock.Advance(TimeSpan.FromSeconds(10));
        scheduler.SetAllPaused(false);

        wakeup.IsScheduled.ShouldBeTrue();
        clock.Advance(TimeSpan.FromSeconds(4));
        wakeup.Fire();

        first.CloseCount.ShouldBe(0);
        second.CloseCount.ShouldBe(1);

        scheduler.SetPaused(first, false);
        clock.Advance(TimeSpan.FromSeconds(4));
        wakeup.Fire();
        first.CloseCount.ShouldBe(1);
    }

    [Fact]
    public void Progress_Refresh_Only_Updates_Visible_Active_Items()
    {
        var clock = new ManualFeedbackClock();
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(clock, wakeup)
        {
            ProgressRefreshInterval = TimeSpan.FromMilliseconds(80)
        };
        var visible = new TestFeedbackItem { IsProgressVisible = true, IsStackVisible = true };
        var hidden = new TestFeedbackItem { IsProgressVisible = true, IsStackVisible = false };
        var noProgress = new TestFeedbackItem { IsProgressVisible = false, IsStackVisible = true };
        scheduler.Register(visible, TimeSpan.FromSeconds(5));
        scheduler.Register(hidden, TimeSpan.FromSeconds(5));
        scheduler.Register(noProgress, TimeSpan.FromSeconds(5));

        wakeup.Due.ShouldBe(TimeSpan.FromMilliseconds(80));
        clock.Advance(TimeSpan.FromMilliseconds(80));
        wakeup.Fire();

        visible.RemainingUpdates.ShouldBe(1);
        visible.LastRemaining.ShouldBe(TimeSpan.FromMilliseconds(4920));
        hidden.RemainingUpdates.ShouldBe(0);
        noProgress.RemainingUpdates.ShouldBe(0);
    }

    [Fact]
    public void Remove_Clear_And_Dispose_Stop_The_Single_Wakeup_Idempotently()
    {
        var wakeup = new ManualFeedbackWakeup();
        var scheduler = new FeedbackLifetimeScheduler(new ManualFeedbackClock(), wakeup);
        var first = new TestFeedbackItem();
        var second = new TestFeedbackItem();
        scheduler.Register(first, TimeSpan.FromSeconds(1));
        scheduler.Register(second, TimeSpan.FromSeconds(2));

        scheduler.Remove(first).ShouldBeTrue();
        scheduler.Count.ShouldBe(1);
        scheduler.Clear();
        scheduler.Count.ShouldBe(0);
        wakeup.IsScheduled.ShouldBeFalse();

        scheduler.Dispose();
        scheduler.Dispose();
        wakeup.DisposeCount.ShouldBe(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Last_Wakeup_Releases_Expired_And_Already_Closing_Items(bool alreadyClosing)
    {
        var clock = new ManualFeedbackClock();
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(clock, wakeup);
        var reference = RegisterUnownedItem(scheduler, alreadyClosing);

        clock.Advance(TimeSpan.FromSeconds(1));
        wakeup.Fire();
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        scheduler.Count.ShouldBe(0);
        wakeup.IsScheduled.ShouldBeFalse();
        reference.IsAlive.ShouldBeFalse();
        GC.KeepAlive(scheduler);
    }

    [Fact]
    public void Next_Wakeup_Uses_The_Time_After_Close_Callbacks()
    {
        var clock = new ManualFeedbackClock();
        var wakeup = new ManualFeedbackWakeup();
        using var scheduler = new FeedbackLifetimeScheduler(clock, wakeup);
        var early = new TestFeedbackItem { OnClose = () => clock.Advance(TimeSpan.FromSeconds(1)) };
        var late = new TestFeedbackItem();
        scheduler.Register(early, TimeSpan.FromSeconds(2));
        scheduler.Register(late, TimeSpan.FromSeconds(5));

        clock.Advance(TimeSpan.FromSeconds(2));
        wakeup.Fire();

        early.CloseCount.ShouldBe(1);
        wakeup.Due.ShouldBe(TimeSpan.FromSeconds(2));
        clock.Advance(wakeup.Due);
        wakeup.Fire();
        late.CloseCount.ShouldBe(1);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RegisterUnownedItem(FeedbackLifetimeScheduler scheduler, bool alreadyClosing)
    {
        var item = new TestFeedbackItem();
        scheduler.Register(item, TimeSpan.FromSeconds(1));
        if (alreadyClosing)
        {
            item.RequestClose();
        }
        return new WeakReference(item);
    }

    private sealed class TestFeedbackItem : IFeedbackStackItem
    {
        public bool IsClosing { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsProgressVisible { get; set; }
        public bool IsStackVisible { get; set; } = true;
        public int CloseCount { get; private set; }
        public int RemainingUpdates { get; private set; }
        public TimeSpan LastRemaining { get; private set; }
        public Action? OnClose { get; init; }

        public void RequestClose()
        {
            CloseCount++;
            IsClosing = true;
            OnClose?.Invoke();
        }

        public void UpdateRemaining(TimeSpan remaining)
        {
            RemainingUpdates++;
            LastRemaining = remaining;
        }
    }

    private sealed class ManualFeedbackClock : IFeedbackClock
    {
        public TimeSpan Now { get; private set; }

        public void Advance(TimeSpan elapsed)
        {
            Now += elapsed;
        }
    }

    private sealed class ManualFeedbackWakeup : IFeedbackWakeup
    {
        private Action? _callback;

        public bool IsScheduled { get; private set; }
        public TimeSpan Due { get; private set; }
        public int DisposeCount { get; private set; }

        public void Schedule(TimeSpan due, Action callback)
        {
            Due = due;
            _callback = callback;
            IsScheduled = true;
        }

        public void Cancel()
        {
            IsScheduled = false;
            _callback = null;
        }

        public void Fire()
        {
            var callback = _callback;
            IsScheduled = false;
            _callback = null;
            callback?.Invoke();
        }

        public void Dispose()
        {
            DisposeCount++;
            Cancel();
        }
    }
}
