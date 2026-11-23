using AtomUI.Controls.Commons;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using static AtomUI.Controls.Tests.Motion.MotionGateTests;

namespace AtomUI.Controls.Tests.Motion;

public class ContinuousMotionGateTests
{
    static ContinuousMotionGateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class TestSpinIndicator : AbstractSpinIndicator
    {
        public TestSpinIndicator()
        {
            Theme = new ControlTheme(typeof(TestSpinIndicator))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<TestSpinIndicator>((_, scope) =>
                    {
                        var layout = new SpinIndicatorDotPanel { Name = "BuiltInIndicatorLayout", Width = 20, Height = 20 };
                        for (var i = 0; i < 4; i++)
                        {
                            layout.Children.Add(new Border { Width = 6, Height = 6 });
                        }
                        return layout.RegisterInNameScope(scope);
                    }))
                }
            };
        }
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

    [Fact]
    public void Spin_runs_when_every_condition_holds()
    {
        var spin = new TestSpinIndicator();
        using var host = new Host(spin);

        spin.IsIndicatorAnimationRunning.ShouldBeTrue();
    }

    [Fact]
    public void Disabling_motion_stops_and_reenabling_starts_once()
    {
        var spin = new TestSpinIndicator();
        using var host = new Host(spin);
        var starts = 0;
        spin.IndicatorAnimationStarted += (_, _) => starts++;

        spin.IsMotionEnabled = false;
        host.Run();
        spin.IsIndicatorAnimationRunning.ShouldBeFalse();

        spin.IsMotionEnabled = true;
        host.Run();
        spin.IsIndicatorAnimationRunning.ShouldBeTrue();
        starts.ShouldBe(1);
    }

    [Fact]
    public void Inactive_source_stops_spin()
    {
        var spin = new TestSpinIndicator();
        using var host = new Host(spin);

        host.Source.IsActive = false;
        host.Run();
        spin.IsIndicatorAnimationRunning.ShouldBeFalse();

        host.Source.IsActive = true;
        host.Run();
        spin.IsIndicatorAnimationRunning.ShouldBeTrue();
    }

    [Fact]
    public void Hidden_ancestor_stops_spin()
    {
        var spin = new TestSpinIndicator();
        using var host = new Host(spin);

        host.Parent.IsVisible = false;
        host.Run();

        spin.IsIndicatorAnimationRunning.ShouldBeFalse();
    }

    [Fact]
    public void Marquee_pauses_when_inactive_and_resumes_from_offset()
    {
        var marquee = new TestMarqueeLabel
        {
            Width = 60,
            Text  = "A notice that is much longer than the sixty pixels it is given"
        };
        using var host = new Host(marquee);
        marquee.IsMarqueeAnimating.ShouldBeTrue();
        WaitUntilScrolled(marquee);
        var offset = marquee.PivotOffset;

        host.Source.IsActive = false;
        host.Run();
        marquee.IsMarqueeAnimating.ShouldBeFalse();
        marquee.PivotOffset.ShouldBe(offset, 0.5);

        host.Source.IsActive = true;
        host.Run();
        marquee.IsMarqueeAnimating.ShouldBeTrue();
        marquee.PivotOffset.ShouldBe(offset, 0.5);
    }

    [Fact]
    public void Quick_stop_and_start_keeps_the_new_run_current()
    {
        var marquee = new TestMarqueeLabel
        {
            Width = 60,
            Text  = "A notice that is much longer than the sixty pixels it is given"
        };
        using var host = new Host(marquee);
        Tick(30);

        // Stop and restart within one dispatcher job: the stopped run's completion must not clear the new run.
        host.Parent.IsVisible = false;
        host.Parent.IsVisible = true;
        host.Run();
        Tick(5);

        marquee.IsMarqueeAnimating.ShouldBeTrue();
        PrivateField<bool>(marquee, "_animationRunning").ShouldBeTrue();
        PrivateField<object?>(marquee, "_animation").ShouldNotBeNull();
    }

    [Fact]
    public void Restart_in_the_next_job_keeps_the_new_run_current()
    {
        var marquee = new TestMarqueeLabel
        {
            Width = 60,
            Text  = "A notice that is much longer than the sixty pixels it is given"
        };
        using var host = new Host(marquee);
        Tick(30);

        // The stopped run completes only after its successor started; it must not clear the successor's state.
        host.Parent.IsVisible = false;
        host.Run();
        host.Parent.IsVisible = true;
        host.Run();
        Tick(5);

        marquee.IsMarqueeAnimating.ShouldBeTrue();
        PrivateField<bool>(marquee, "_animationRunning").ShouldBeTrue();
        PrivateField<object?>(marquee, "_animation").ShouldNotBeNull();
    }

    private static T PrivateField<T>(AbstractMarqueeLabel marquee, string name) =>
        (T)typeof(AbstractMarqueeLabel)
           .GetField(name, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
           .GetValue(marquee)!;

    /// <summary>Animation time follows the wall clock, not render ticks, so movement needs real elapsed time.</summary>
    private static void WaitUntilScrolled(AbstractMarqueeLabel marquee)
    {
        var deadline = DateTime.UtcNow.AddSeconds(3);
        while (marquee.PivotOffset >= 0)
        {
            DateTime.UtcNow.ShouldBeLessThan(deadline, "the marquee did not start scrolling");
            Thread.Sleep(16);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Tick(int count)
    {
        for (var i = 0; i < count; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Marquee_with_motion_disabled_does_not_start()
    {
        var marquee = new TestMarqueeLabel
        {
            Width           = 60,
            IsMotionEnabled = false,
            Text            = "A notice that is much longer than the sixty pixels it is given"
        };
        using var host = new Host(marquee);

        marquee.IsMarqueeAnimating.ShouldBeFalse();
        marquee.PivotOffset.ShouldBe(0);
    }
}
