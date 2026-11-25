using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Motion;

public class MotionGateTests
{
    static MotionGateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    internal sealed class TestActivitySource : IMotionActivitySource
    {
        private bool _isActive = true;

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }
                _isActive = value;
                IsActiveChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int SubscriberCount => IsActiveChanged?.GetInvocationList().Length ?? 0;

        public event EventHandler? IsActiveChanged;
    }

    private sealed class Host : IDisposable
    {
        public readonly Border Owner = new();
        public readonly Panel Parent = new();
        public readonly Window Window;
        public readonly TestActivitySource Source = new();

        public Host(bool attach = true)
        {
            if (attach)
            {
                Parent.Children.Add(Owner);
            }
            Window = new Window { Content = Parent };
            MotionActivity.SetSource(Window, Source);
            Window.Show();
            Run();
        }

        public static void Run() => Dispatcher.UIThread.RunJobs();

        public void Dispose() => Window.Close();
    }

    [Fact]
    public void Truth_table_requires_all_four_conditions()
    {
        foreach (var enabled in new[] { true, false })
        foreach (var attached in new[] { true, false })
        foreach (var visible in new[] { true, false })
        foreach (var active in new[] { true, false })
        {
            using var host = new Host(attached);
            host.Parent.IsVisible = visible;
            host.Source.IsActive = active;
            Host.Run();
            using var gate = new MotionGate(host.Owner, _ => { }) { IsMotionEnabled = enabled };
            Host.Run();

            gate.ShouldRun.ShouldBe(enabled && attached && visible && active,
                $"enabled={enabled} attached={attached} visible={visible} active={active}");
        }
    }

    [Fact]
    public void Callback_fires_only_on_transitions()
    {
        using var host = new Host();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);

        host.Source.IsActive = false;
        Host.Run();
        host.Parent.IsVisible = false;
        Host.Run();
        host.Source.IsActive = true;
        Host.Run();
        host.Parent.IsVisible = true;
        Host.Run();

        calls.ShouldBe([false, true]);
    }

    [Fact]
    public void Flicker_within_one_job_is_coalesced()
    {
        using var host = new Host();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);

        host.Parent.IsVisible = false;
        host.Parent.IsVisible = true;
        host.Source.IsActive = false;
        host.Source.IsActive = true;
        Host.Run();

        calls.ShouldBeEmpty();
        gate.ShouldRun.ShouldBeTrue();
    }

    [Fact]
    public void Change_is_delivered_after_the_job()
    {
        using var host = new Host();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);

        host.Source.IsActive = false;
        calls.ShouldBeEmpty();
        gate.ShouldRun.ShouldBeTrue();

        Host.Run();
        calls.ShouldBe([false]);
        gate.ShouldRun.ShouldBeFalse();
    }

    [Fact]
    public void Disabling_motion_reports_stop_and_enabling_reports_start()
    {
        using var host = new Host();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);

        gate.IsMotionEnabled = false;
        gate.IsMotionEnabled = false;
        Host.Run();
        gate.IsMotionEnabled = true;
        Host.Run();

        calls.ShouldBe([false, true]);
    }

    [Fact]
    public void Nearest_injected_source_wins()
    {
        using var host = new Host();
        var nearer = new TestActivitySource { IsActive = false };
        MotionActivity.SetSource(host.Parent, nearer);
        Host.Run();
        using var gate = new MotionGate(host.Owner, _ => { });
        gate.ShouldRun.ShouldBeFalse();

        nearer.IsActive = true;
        Host.Run();
        gate.ShouldRun.ShouldBeTrue();

        host.Source.IsActive = false;
        Host.Run();
        gate.ShouldRun.ShouldBeTrue();
    }

    [Fact]
    public void Changing_the_injected_source_resubscribes()
    {
        using var host = new Host();
        using var gate = new MotionGate(host.Owner, _ => { });
        var replacement = new TestActivitySource { IsActive = false };

        MotionActivity.SetSource(host.Window, replacement);
        Host.Run();

        gate.ShouldRun.ShouldBeFalse();
        host.Source.SubscriberCount.ShouldBe(0);
        replacement.SubscriberCount.ShouldBe(1);
    }

    [Fact]
    public void Detach_stops_and_reattach_restarts()
    {
        using var host = new Host();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);

        host.Parent.Children.Remove(host.Owner);
        Host.Run();
        host.Source.SubscriberCount.ShouldBe(0);
        host.Parent.Children.Add(host.Owner);
        Host.Run();

        calls.ShouldBe([false, true]);
        gate.ShouldRun.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_unsubscribes_everything()
    {
        using var host = new Host();
        var calls = new List<bool>();
        var gate = new MotionGate(host.Owner, calls.Add);

        gate.Dispose();
        host.Source.IsActive = false;
        host.Parent.IsVisible = false;
        host.Parent.Children.Remove(host.Owner);
        Host.Run();

        calls.ShouldBeEmpty();
        gate.ShouldRun.ShouldBeFalse();
        host.Source.SubscriberCount.ShouldBe(0);
    }

    [Fact]
    public void Owner_visibility_stops_and_restarts()
    {
        using var host = new Host();
        host.Owner.IsVisible = false;
        Host.Run();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);
        Host.Run();
        gate.ShouldRun.ShouldBeFalse();

        host.Owner.IsVisible = true;
        Host.Run();
        gate.ShouldRun.ShouldBeTrue();

        host.Owner.IsVisible = false;
        Host.Run();
        gate.ShouldRun.ShouldBeFalse();

        calls.ShouldBe([true, false]);
    }

    [Fact]
    public void Showing_owner_under_hidden_parent_waits_for_parent()
    {
        using var host = new Host();
        host.Owner.IsVisible  = false;
        host.Parent.IsVisible = false;
        Host.Run();
        var calls = new List<bool>();
        using var gate = new MotionGate(host.Owner, calls.Add);

        host.Owner.IsVisible = true;
        Host.Run();
        gate.ShouldRun.ShouldBeFalse();

        host.Parent.IsVisible = true;
        Host.Run();
        gate.ShouldRun.ShouldBeTrue();
        calls.ShouldBe([true]);
    }

    [Fact]
    public void Hidden_owner_does_not_subscribe_to_ancestors()
    {
        using var host = new Host();
        host.Owner.IsVisible = false;
        Host.Run();
        using var gate = new MotionGate(host.Owner, _ => { });
        gate.IsTrackingAncestorVisibility.ShouldBeFalse();

        host.Owner.IsVisible = true;
        Host.Run();
        gate.IsTrackingAncestorVisibility.ShouldBeTrue();

        host.Owner.IsVisible = false;
        Host.Run();
        gate.IsTrackingAncestorVisibility.ShouldBeFalse();
    }

    [Fact]
    public void Missing_lifetime_counts_as_active()
    {
        MotionActivity.Application.IsActive.ShouldBeTrue();
    }
}
