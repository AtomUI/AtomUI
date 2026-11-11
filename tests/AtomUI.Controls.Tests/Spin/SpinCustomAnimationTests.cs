using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Avalonia.Threading;
using Shouldly;
using Xunit;
using static AtomUI.Controls.Tests.Motion.MotionGateTests;

namespace AtomUI.Controls.Tests.Spin;

public class SpinCustomAnimationTests
{
    static SpinCustomAnimationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class RecordingIndicator : AbstractSpinIndicator
    {
        public readonly List<string> Calls = new();
        public Visual? LastStartTarget;
        public bool StartResult = true;

        public RecordingIndicator()
        {
            Theme = new ControlTheme(typeof(RecordingIndicator))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<RecordingIndicator>((_, scope) =>
                    {
                        var root = new Panel();
                        var layout = new SpinIndicatorDotPanel { Name = "BuiltInIndicatorLayout", Width = 20, Height = 20 };
                        for (var i = 0; i < 4; i++)
                        {
                            layout.Children.Add(new Border { Width = 6, Height = 6 });
                        }
                        root.Children.Add(layout.RegisterInNameScope(scope));
                        var part = new Border { Name = "PART_IndicatorLayout", Width = 10, Height = 10 };
                        root.Children.Add(part.RegisterInNameScope(scope));
                        return root;
                    }))
                }
            };
        }

        public Control? Part => this.GetTemplateDescendants().OfType<Control>().FirstOrDefault(c => c.Name == "PART_IndicatorLayout");

        protected override bool StartCustomAnimation(Visual target)
        {
            Calls.Add("start");
            LastStartTarget = target;
            return StartResult;
        }

        protected override void StopCustomAnimation(Visual target)
        {
            Calls.Add("stop");
        }

        public void Restart() => RestartIndicatorAnimation();
    }

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
    public void Default_mode_is_rotate_and_unchanged()
    {
        var spin = new RecordingIndicator();
        using var host = new Host(spin);

        spin.AnimationMode.ShouldBe(IndicatorAnimationMode.Rotate);
        spin.IsIndicatorAnimationRunning.ShouldBeTrue();
        spin.Calls.ShouldBeEmpty();

        var layout = spin.GetTemplateDescendants().OfType<SpinIndicatorDotPanel>().Single();
        host.Source.IsActive = false;
        host.Run();
        layout.Children.ShouldAllBe(dot => dot.Opacity == 0.3);
    }

    [Fact]
    public void Custom_mode_calls_start_once_when_gate_opens()
    {
        var spin = new RecordingIndicator { AnimationMode = IndicatorAnimationMode.Custom };
        using var host = new Host(spin);

        spin.Calls.ShouldBe(new[] { "start" });
        spin.LastStartTarget.ShouldBeSameAs(spin.Part);
    }

    [Fact]
    public void Custom_mode_calls_stop_when_gate_closes()
    {
        var spin = new RecordingIndicator { AnimationMode = IndicatorAnimationMode.Custom };
        using var host = new Host(spin);

        host.Source.IsActive = false;
        host.Run();
        spin.Calls.ShouldBe(new[] { "start", "stop" });

        host.Source.IsActive = true;
        host.Run();
        spin.Calls.ShouldBe(new[] { "start", "stop", "start" });
    }

    [Fact]
    public void Custom_mode_does_not_rotate()
    {
        var spin = new RecordingIndicator { AnimationMode = IndicatorAnimationMode.Custom };
        using var host = new Host(spin);

        // 旋转模式停止时会把内置圆点透明度重置为 0.3，自定义模式不触碰内置圆点。
        var layout = spin.GetTemplateDescendants().OfType<SpinIndicatorDotPanel>().Single();
        foreach (var dot in layout.Children)
        {
            dot.Opacity = 1;
        }
        host.Source.IsActive = false;
        host.Run();

        layout.Children.ShouldAllBe(dot => dot.Opacity == 1);
    }

    [Fact]
    public void Running_flag_tracks_custom_animation()
    {
        var spin = new RecordingIndicator { AnimationMode = IndicatorAnimationMode.Custom };
        using var host = new Host(spin);
        var started = 0;
        spin.IndicatorAnimationStarted += (_, _) => started++;
        spin.IsIndicatorAnimationRunning.ShouldBeTrue();

        host.Source.IsActive = false;
        host.Run();
        spin.IsIndicatorAnimationRunning.ShouldBeFalse();

        spin.StartResult = false;
        host.Source.IsActive = true;
        host.Run();
        spin.IsIndicatorAnimationRunning.ShouldBeFalse();
        started.ShouldBe(0);
        spin.Calls.Count(c => c == "stop").ShouldBe(1);
    }

    [Fact]
    public void Mode_change_while_running_switches_animation()
    {
        var spin = new RecordingIndicator();
        using var host = new Host(spin);

        spin.AnimationMode = IndicatorAnimationMode.Custom;
        host.Run();
        spin.Calls.ShouldBe(new[] { "start" });
        spin.IsIndicatorAnimationRunning.ShouldBeTrue();

        spin.AnimationMode = IndicatorAnimationMode.Rotate;
        host.Run();
        spin.Calls.ShouldBe(new[] { "start", "stop" });
        spin.IsIndicatorAnimationRunning.ShouldBeTrue();
    }

    [Fact]
    public void Restart_calls_stop_then_start()
    {
        var spin = new RecordingIndicator { AnimationMode = IndicatorAnimationMode.Custom };
        using var host = new Host(spin);

        spin.Restart();

        spin.Calls.ShouldBe(new[] { "start", "stop", "start" });
    }
}
