using AtomUI.Controls.Commons;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;
using ProgressPath = Avalonia.Controls.Shapes.Path;

namespace AtomUI.Controls.Tests.ProgressBar;

public class ProgressRenderCostTests
{
    private static readonly TimeSpan TransitionDuration = TimeSpan.FromMilliseconds(300);

    static ProgressRenderCostTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    // 与 Desktop 默认模板同构：槽、填充、成功段三个盒子与信息区。
    private sealed class TestLine : AbstractGeneralProgressBar
    {
        public TestLine()
        {
            Template = new FuncControlTemplate<TestLine>((_, scope) =>
            {
                var panel = new LineProgressPanel { Name = ProgressBodyPart };
                scope.Register(panel.Name!, panel);
                panel.Children.Add(new Border { Name = ProgressRailPart, Background = Brushes.Gray });
                panel.Children.Add(new Border { Name = ProgressTrackPart, Background = Brushes.Blue });
                panel.Children.Add(new Border { Name = ProgressSuccessPart, Background = Brushes.Green });
                var label = new Label { Name = PercentageLabelPart };
                var labelHost = new LayoutTransformControl { Name = LayoutTransformControlPart, Child = label };
                scope.Register(label.Name!, label);
                scope.Register(labelHost.Name!, labelHost);
                var indicator = new Canvas { Name = ProgressIndicatorPart };
                scope.Register(indicator.Name!, indicator);
                indicator.Children.Add(labelHost);
                panel.Children.Add(indicator);
                return panel;
            });
        }

        internal Border Rail => Find<Border>(ProgressRailPart);
        internal Border Track => Find<Border>(ProgressTrackPart);
        internal Label Label => Find<Label>(PercentageLabelPart);

        private T Find<T>(string name) where T : Control
        {
            return this.GetVisualDescendants().OfType<T>().Single(c => c.Name == name);
        }
    }

    // 与 Desktop 默认模板同构：轨道、填充弧、成功弧三条路径与信息区。
    private sealed class TestCircle : AbstractGeneralCircleProgress
    {
        public TestCircle()
        {
            Template = new FuncControlTemplate<TestCircle>((_, scope) =>
            {
                var panel = new CircleProgressPanel();
                foreach (var name in new[] { ProgressRailPart, ProgressTrackPart, ProgressSuccessPart })
                {
                    var path = new ProgressPath { Name = name, Stroke = Brushes.Blue, StrokeThickness = 6 };
                    scope.Register(name, path);
                    panel.Children.Add(path);
                }

                var label = new Label { Name = PercentageLabelPart };
                var labelHost = new LayoutTransformControl { Name = LayoutTransformControlPart, Child = label };
                scope.Register(label.Name!, label);
                scope.Register(labelHost.Name!, labelHost);
                var indicator = new Canvas { Name = ProgressIndicatorPart };
                scope.Register(indicator.Name!, indicator);
                indicator.Children.Add(labelHost);
                panel.Children.Add(indicator);
                return panel;
            });
        }

        internal ProgressPath Rail => Find(ProgressRailPart);
        internal ProgressPath Track => Find(ProgressTrackPart);
        internal ProgressPath Success => Find(ProgressSuccessPart);

        private ProgressPath Find(string name)
        {
            return this.GetVisualDescendants().OfType<ProgressPath>().Single(c => c.Name == name);
        }
    }

    // 不含路径部件的模板：环形走自绘路径（Mobile ProgressCircle 的形态）。
    private sealed class RenderCircle : AbstractGeneralCircleProgress
    {
        public RenderCircle()
        {
            Template = new FuncControlTemplate<RenderCircle>((_, _) => new Canvas());
        }
    }

    private sealed class WindowContext : IDisposable
    {
        private readonly AvaloniaWindow _window;

        internal WindowContext(Control content)
        {
            _window = new AvaloniaWindow { Width = 400, Height = 300, Content = content };
            _window.Show();
            Dispatcher.UIThread.RunJobs();
        }

        internal AvaloniaWindow Window => _window;

        public void Dispose()
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static void Flush()
    {
        Dispatcher.UIThread.RunJobs();
    }

    // 无头渲染计时器按真实时间推进，逐帧睡眠一帧时长再触发
    private static void RunFrames(TimeSpan duration, Action? perFrame = null)
    {
        var frames = (int)Math.Ceiling(duration.TotalMilliseconds / 16) + 2;
        for (var i = 0; i < frames; i++)
        {
            Thread.Sleep(16);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            perFrame?.Invoke();
        }
    }

    private static Transitions CreateTransitions(AvaloniaProperty property)
    {
        return new Transitions
        {
            new DoubleTransition { Property = property, Duration = TransitionDuration }
        };
    }

    [AvaloniaFact]
    public void Indicator_ratio_follows_percentage()
    {
        var line = new TestLine { Width = 200, Minimum = 0, Maximum = 200, Value = 50 };
        using var _ = new WindowContext(line);

        line.IndicatorRatio.ShouldBe(0.25, 0.0001);

        line.Value = 200;
        Flush();
        line.IndicatorRatio.ShouldBe(1, 0.0001);

        line.Minimum = 100;
        Flush();
        line.IndicatorRatio.ShouldBe(line.Percentage / 100, 0.0001);
    }

    [AvaloniaFact]
    public void Ratio_change_does_not_invalidate_measure()
    {
        var line = new TestLine
        {
            Width                 = 300,
            Value                 = 20,
            IsProgressInfoVisible = false,
            StrokeLineCap         = PenLineCap.Flat
        };
        line.Transitions = CreateTransitions(AbstractLineProgress.IndicatorRatioProperty);
        using var context = new WindowContext(line);
        RunFrames(TimeSpan.FromMilliseconds(50));

        line.Value = 80;
        Flush();

        var layoutPasses = 0;
        var widths       = new List<double>();
        context.Window.LayoutUpdated += (_, _) => layoutPasses++;
        RunFrames(TransitionDuration, () =>
        {
            line.IsMeasureValid.ShouldBeTrue();
            line.IsArrangeValid.ShouldBeTrue();
            // 填充按当前比例排布在完整槽内（布局取整，容差一像素）
            line.Track.Bounds.Width.ShouldBe(line.Rail.Bounds.Width * line.IndicatorRatio, 1);
            widths.Add(line.Track.Bounds.Width);
        });

        layoutPasses.ShouldBe(0);
        widths.Distinct().Count().ShouldBeGreaterThan(2);
        line.IndicatorRatio.ShouldBe(0.8, 0.0001);
        line.Track.Bounds.Width.ShouldBe(line.Rail.Bounds.Width * 0.8, 1);
    }

    [AvaloniaFact]
    public void Rail_geometry_rebuilt_only_on_size_or_step_change()
    {
        var circle = new TestCircle { Width = 100, Height = 100, Value = 0, IsMotionEnabled = false };
        using var _ = new WindowContext(circle);
        var builds = circle.RailGeometryBuildCount;
        builds.ShouldBeGreaterThan(0);
        var rail = circle.Rail.Data;

        for (var value = 1; value <= 100; value++)
        {
            circle.Value = value;
            Flush();
        }

        circle.RailGeometryBuildCount.ShouldBe(builds);
        circle.Rail.Data.ShouldBeSameAs(rail);

        circle.StepCount = 4;
        Flush();
        circle.RailGeometryBuildCount.ShouldBe(builds + 1);

        var afterSteps = circle.RailGeometryBuildCount;
        circle.Width  = 120;
        circle.Height = 120;
        Flush();
        circle.RailGeometryBuildCount.ShouldBeGreaterThan(afterSteps);
    }

    [AvaloniaFact]
    public void Angle_change_rebuilds_track_only()
    {
        var circle = new TestCircle { Width = 100, Height = 100, Value = 40, SuccessThreshold = 20 };
        using var _ = new WindowContext(circle);
        var builds  = circle.RailGeometryBuildCount;
        var rail    = circle.Rail.Data;
        var success = circle.Success.Data;
        var track   = circle.Track.Data;

        circle.IndicatorAngle = 200;
        Flush();

        circle.Track.Data.ShouldNotBeSameAs(track);
        circle.Track.Data!.Bounds.Height.ShouldBeGreaterThan(track!.Bounds.Height);
        circle.Rail.Data.ShouldBeSameAs(rail);
        circle.Success.Data.ShouldBeSameAs(success);
        circle.RailGeometryBuildCount.ShouldBe(builds);
    }

    [AvaloniaFact]
    public void Full_text_size_is_cached()
    {
        var circle = new TestCircle { Width = 100, Height = 100, Value = 0, IsMotionEnabled = false };
        using var _ = new WindowContext(circle);
        var measures = circle.FullTextMeasureCount;

        for (var value = 1; value <= 100; value++)
        {
            circle.Value = value;
            Flush();
        }

        circle.FullTextMeasureCount.ShouldBe(measures);

        circle.FontSize = 30;
        circle.Value    = 50;
        Flush();
        circle.FullTextMeasureCount.ShouldBe(measures + 1);

        circle.ProgressTextFormat = "{0:0.0}%";
        Flush();
        circle.FullTextMeasureCount.ShouldBe(measures + 2);
    }

    [AvaloniaFact]
    public void Desktop_value_transition_still_animates()
    {
        // Desktop 主题过渡的是 Value 本身：标签逐帧计数，比例每帧跟随百分比
        var line = new TestLine { Width = 300, Value = 20 };
        line.Transitions = CreateTransitions(RangeBase.ValueProperty);
        using var _ = new WindowContext(line);
        RunFrames(TimeSpan.FromMilliseconds(50));

        line.Value = 80;
        Flush();

        var intermediateLabels = new HashSet<string>();
        RunFrames(TransitionDuration, () =>
        {
            line.IndicatorRatio.ShouldBe(line.Percentage / 100, 0.0001);
            if (line.Value > 20 && line.Value < 80 && line.Label.Content is string text)
            {
                intermediateLabels.Add(text);
            }
        });

        intermediateLabels.Count.ShouldBeGreaterThan(1);
        line.Value.ShouldBe(80, 0.0001);
        line.IndicatorRatio.ShouldBe(0.8, 0.0001);
        line.Label.Content.ShouldBe("80%");
    }

    [AvaloniaFact]
    public void Ratio_transition_from_zero_with_round_cap_does_not_layout()
    {
        var line = new TestLine
        {
            Width                 = 300,
            Value                 = 0,
            IsProgressInfoVisible = false,
            StrokeLineCap         = PenLineCap.Round
        };
        line.Transitions = CreateTransitions(AbstractLineProgress.IndicatorRatioProperty);
        using var context = new WindowContext(line);
        RunFrames(TimeSpan.FromMilliseconds(50));
        line.Track.Bounds.Width.ShouldBe(0);

        line.Value = 80;
        Flush();

        var layoutPasses = 0;
        context.Window.LayoutUpdated += (_, _) => layoutPasses++;
        RunFrames(TransitionDuration, () => line.Track.IsVisible.ShouldBeTrue());

        layoutPasses.ShouldBe(0);
        line.IndicatorRatio.ShouldBe(0.8, 0.0001);
        line.Track.Bounds.Width.ShouldBe(line.Rail.Bounds.Width * 0.8, 1);
    }

    [AvaloniaFact]
    public void Render_path_steps_reuse_rail_geometry_during_angle_sweep()
    {
        var circle = new RenderCircle
        {
            Width            = 100,
            Height           = 100,
            Value            = 30,
            StepCount        = 6,
            SuccessThreshold = 20,
            StrokeBrush      = Brushes.Blue,
            IsMotionEnabled  = false
        };
        using var _ = new WindowContext(circle);
        RunFrames(TimeSpan.FromMilliseconds(32));
        var builds = circle.RailGeometryBuildCount;
        builds.ShouldBeGreaterThan(0);

        for (var angle = 0; angle <= 360; angle += 20)
        {
            circle.IndicatorAngle = angle;
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Flush();
        }

        circle.RailGeometryBuildCount.ShouldBe(builds);

        circle.StepCount = 8;
        Flush();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Flush();
        circle.RailGeometryBuildCount.ShouldBe(builds + 1);
    }
}
