using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.Window;

public class FullScreenTitleBarRevealControllerTests
{
    private sealed class ManualScheduler
    {
        private readonly List<(TimeSpan Delay, Action Action, Handle Handle)> _pending = new();

        public Func<TimeSpan, Action, IDisposable> Schedule => (delay, action) =>
        {
            var handle = new Handle(this);
            _pending.Add((delay, action, handle));
            return handle;
        };

        public int PendingCount => _pending.Count;

        public void FireAll()
        {
            var items = _pending.ToList();
            _pending.Clear();
            foreach (var item in items)
            {
                item.Action();
            }
        }

        private sealed class Handle(ManualScheduler owner) : IDisposable
        {
            public void Dispose() => owner._pending.RemoveAll(item => ReferenceEquals(item.Handle, this));
        }
    }

    private static (FullScreenTitleBarRevealController Controller, ManualScheduler Clock, List<bool> Changes) Create(
        int enterMs = 150,
        int leaveMs = 300)
    {
        var clock   = new ManualScheduler();
        var changes = new List<bool>();
        var controller = new FullScreenTitleBarRevealController(changes.Add, clock.Schedule)
        {
            EnterDelay = TimeSpan.FromMilliseconds(enterMs),
            LeaveDelay = TimeSpan.FromMilliseconds(leaveMs)
        };
        controller.Start();
        return (controller, clock, changes);
    }

    [Fact]
    public void Trigger_Band_Opens_After_Enter_Delay_And_Leaving_Early_Cancels()
    {
        var (controller, clock, changes) = Create();

        controller.SetPointerInTriggerBand(true);
        controller.IsOpen.ShouldBeFalse();
        controller.SetPointerInTriggerBand(false);
        clock.PendingCount.ShouldBe(0);

        controller.SetPointerInTriggerBand(true);
        controller.SetHold(FullScreenTitleBarHolds.Pointer, true);
        clock.FireAll();
        controller.IsOpen.ShouldBeTrue();
        changes.ShouldBe(new[] { true });
    }

    [Fact]
    public void Last_Hold_Starts_Leave_Delay_And_A_New_Hold_Cancels_It()
    {
        var (controller, clock, changes) = Create();
        controller.SetPointerInTriggerBand(true);
        controller.SetHold(FullScreenTitleBarHolds.Pointer, true);
        clock.FireAll();
        controller.SetHold(FullScreenTitleBarHolds.Popup, true);

        controller.SetHold(FullScreenTitleBarHolds.Pointer, false);
        clock.PendingCount.ShouldBe(0);
        controller.SetHold(FullScreenTitleBarHolds.Popup, false);
        clock.PendingCount.ShouldBe(1);
        controller.IsOpen.ShouldBeTrue();

        controller.SetHold(FullScreenTitleBarHolds.Focus, true);
        clock.PendingCount.ShouldBe(0);
        controller.SetHold(FullScreenTitleBarHolds.Focus, false);
        clock.FireAll();

        controller.IsOpen.ShouldBeFalse();
        changes.ShouldBe(new[] { true, false });
    }

    [Fact]
    public void Native_Chrome_Reveals_Without_Delay_And_Blocks_Immediate_Close()
    {
        var (controller, clock, _) = Create();

        controller.SetHold(FullScreenTitleBarHolds.NativeChrome, true);
        controller.IsOpen.ShouldBeTrue();
        clock.PendingCount.ShouldBe(0);

        controller.CloseImmediately();
        controller.IsOpen.ShouldBeTrue();

        controller.SetHold(FullScreenTitleBarHolds.NativeChrome, false);
        clock.FireAll();
        controller.IsOpen.ShouldBeFalse();
    }

    [Fact]
    public void Suppression_Rejects_Reveal_And_Closes_An_Open_Surface()
    {
        var (controller, _, _) = Create(enterMs: 0);
        controller.RequestOpen().ShouldBeTrue();
        controller.IsOpen.ShouldBeTrue();

        controller.SetSuppressed(true);
        controller.IsOpen.ShouldBeFalse();
        controller.RequestOpen().ShouldBeFalse();
        controller.SetPointerInTriggerBand(true);
        controller.SetHold(FullScreenTitleBarHolds.NativeChrome, true);
        controller.IsOpen.ShouldBeFalse();

        controller.SetSuppressed(false);
        controller.SetPointerInTriggerBand(false);
        controller.SetPointerInTriggerBand(true);
        controller.IsOpen.ShouldBeTrue();
    }

    [Fact]
    public void Explicit_Request_Holds_Until_Closed_And_Zero_Leave_Delay_Closes_At_Once()
    {
        var (controller, clock, changes) = Create(leaveMs: 0);

        controller.RequestOpen();
        controller.SetHold(FullScreenTitleBarHolds.Pointer, true);
        controller.SetHold(FullScreenTitleBarHolds.Pointer, false);
        controller.IsOpen.ShouldBeTrue();

        controller.RequestClose();
        controller.IsOpen.ShouldBeFalse();
        clock.PendingCount.ShouldBe(0);
        changes.ShouldBe(new[] { true, false });
    }

    [Fact]
    public void Stop_Closes_And_Ignores_Later_Input()
    {
        var (controller, _, changes) = Create(enterMs: 0);
        controller.SetPointerInTriggerBand(true);
        controller.SetHold(FullScreenTitleBarHolds.Pointer, true);

        controller.Stop();
        controller.SetPointerInTriggerBand(true);

        controller.IsOpen.ShouldBeFalse();
        changes.ShouldBe(new[] { true, false });
    }
}
