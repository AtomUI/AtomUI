using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using static AtomUI.Controls.Tests.Motion.MotionGateTests;

namespace AtomUI.Controls.Tests.MarqueeLabel;

public class MarqueeStartTests
{
    private const string LongText = "A notice that is much longer than the sixty pixels it is given";

    static MarqueeStartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestMarqueeLabel : AbstractMarqueeLabel;

    private sealed class Host : IDisposable
    {
        public readonly Panel Parent = new();
        public readonly Window Window;
        public readonly TestActivitySource Source = new();

        public Host(Control content)
        {
            Parent.Children.Add(content);
            Window = new Window { Width = 400, Height = 200, Content = Parent };
            MotionActivity.SetSource(Window, Source);
            Window.Show();
            Run();
        }

        public void Run()
        {
            Dispatcher.UIThread.RunJobs();
            Window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
        }

        public void Dispose() => Window.Close();
    }

    private static TestMarqueeLabel CreateLabel(TimeSpan startDelay = default)
    {
        return new TestMarqueeLabel { Width = 60, Text = LongText, StartDelay = startDelay };
    }

    private static bool WaitUntil(Func<bool> condition, int timeoutMilliseconds = 3000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (!condition())
        {
            if (DateTime.UtcNow > deadline)
            {
                return false;
            }
            // RunJobs does not fire dispatcher timers in Headless; a short nested loop does.
            using var slice = new CancellationTokenSource(TimeSpan.FromMilliseconds(15));
            Dispatcher.UIThread.MainLoop(slice.Token);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        }
        return true;
    }

    [Fact]
    public void Defaults_keep_existing_behavior()
    {
        var label = new TestMarqueeLabel();

        label.StartDelay.ShouldBe(TimeSpan.Zero);
        label.IsPausedOnPointerOver.ShouldBeTrue();
    }

    [Fact]
    public void Zero_delay_starts_immediately()
    {
        var label = CreateLabel();
        using var host = new Host(label);

        label.IsMarqueeAnimating.ShouldBeTrue();
    }

    [Fact]
    public void Start_delay_defers_first_cycle()
    {
        var label = CreateLabel(TimeSpan.FromMilliseconds(200));
        using var host = new Host(label);

        label.IsMarqueeAnimating.ShouldBeFalse();
        label.PivotOffset.ShouldBe(0);

        WaitUntil(() => label.IsMarqueeAnimating).ShouldBeTrue("the delayed start never happened");
        WaitUntil(() => label.PivotOffset < 0).ShouldBeTrue("the marquee did not scroll after the delay");
    }

    [Fact]
    public void Gate_close_cancels_pending_delay()
    {
        var label = CreateLabel(TimeSpan.FromMilliseconds(300));
        using var host = new Host(label);
        label.IsMarqueeAnimating.ShouldBeFalse();

        label.HasPendingStartDelay.ShouldBeTrue();

        label.IsMotionEnabled = false;
        host.Run();
        label.HasPendingStartDelay.ShouldBeFalse();
        // Well past the delay: nothing may start while the gate is closed.
        WaitUntil(() => label.IsMarqueeAnimating, 600).ShouldBeFalse();
        label.PivotOffset.ShouldBe(0);

        // Reopening restarts, with the delay again.
        label.IsMotionEnabled = true;
        host.Run();
        label.IsMarqueeAnimating.ShouldBeFalse();
        WaitUntil(() => label.IsMarqueeAnimating).ShouldBeTrue();
    }

    [Fact]
    public void Gate_reopen_waits_the_full_delay()
    {
        var label = CreateLabel(TimeSpan.FromMilliseconds(600));
        using var host = new Host(label);
        // Most of the first delay elapses before the gate closes.
        WaitUntil(() => false, 350);
        label.IsMarqueeAnimating.ShouldBeFalse();

        label.IsMotionEnabled = false;
        host.Run();
        label.IsMotionEnabled = true;
        host.Run();

        // The first delay would have ended about 250 ms from here; the restarted one must not.
        WaitUntil(() => label.IsMarqueeAnimating, 400).ShouldBeFalse();
        WaitUntil(() => label.IsMarqueeAnimating).ShouldBeTrue();
    }

    [Fact]
    public void Detach_cancels_pending_delay()
    {
        var label = CreateLabel(TimeSpan.FromMilliseconds(200));
        using var host = new Host(label);

        label.HasPendingStartDelay.ShouldBeTrue();

        host.Parent.Children.Remove(label);
        host.Run();
        label.HasPendingStartDelay.ShouldBeFalse();

        WaitUntil(() => label.IsMarqueeAnimating, 500).ShouldBeFalse();
    }

    [Fact]
    public void Pointer_over_pauses_by_default()
    {
        var label = CreateLabel();
        using var host = new Host(label);
        label.IsMarqueeAnimating.ShouldBeTrue();

        SetPointerOver(label, true);
        label.IsMarqueeAnimating.ShouldBeFalse();

        SetPointerOver(label, false);
        label.IsMarqueeAnimating.ShouldBeTrue();
    }

    [Fact]
    public void Pointer_over_ignored_when_disabled()
    {
        var label = CreateLabel();
        label.IsPausedOnPointerOver = false;
        using var host = new Host(label);
        label.IsMarqueeAnimating.ShouldBeTrue();

        SetPointerOver(label, true);
        label.IsMarqueeAnimating.ShouldBeTrue();

        SetPointerOver(label, false);
        label.IsMarqueeAnimating.ShouldBeTrue();
    }

    [Fact]
    public void Resume_after_pointer_leave_applies_the_delay()
    {
        var label = CreateLabel(TimeSpan.FromMilliseconds(200));
        using var host = new Host(label);
        WaitUntil(() => label.IsMarqueeAnimating).ShouldBeTrue();

        SetPointerOver(label, true);
        label.IsMarqueeAnimating.ShouldBeFalse();

        SetPointerOver(label, false);
        label.IsMarqueeAnimating.ShouldBeFalse();
        WaitUntil(() => label.IsMarqueeAnimating).ShouldBeTrue();
    }

    // IsPointerOver is a direct property with a protected setter; drive it the way the input system does.
    private static void SetPointerOver(TestMarqueeLabel label, bool value)
    {
        var setter = typeof(InputElement).GetProperty(nameof(InputElement.IsPointerOver))!.GetSetMethod(true)!;
        setter.Invoke(label, [value]);
        Dispatcher.UIThread.RunJobs();
    }
}
