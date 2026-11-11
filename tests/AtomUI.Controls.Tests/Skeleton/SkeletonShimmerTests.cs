using AtomUI.Controls.Commons;
using AtomUI.Controls.Tests.Motion;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Rendering.Composition;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;

namespace AtomUI.Controls.Tests.Skeleton;

public class SkeletonShimmerTests
{
    static SkeletonShimmerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private static readonly IBrush BlockBrush = new ImmutableSolidColorBrush(Colors.LightGray);

    // A line whose only block is the template's content layer, like the Desktop element templates.
    private sealed class TestLine : AbstractSkeletonLine
    {
        public TestLine()
        {
            Background        = BlockBrush;
            MotionDuration    = TimeSpan.FromSeconds(1.4);
            MotionEasingCurve = new LinearEasing();
            Height            = 16;
            Theme             = CreateTheme();
        }

        public Border ContentLayer =>
            this.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_ContentLayer");

        public static ControlTheme CreateTheme(string contentLayerName = "PART_ContentLayer")
        {
            return new ControlTheme(typeof(TestLine))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<TestLine>((line, scope) =>
                    {
                        var content = new Border { Name = contentLayerName, CornerRadius = new CornerRadius(4) };
                        content.Bind(Border.BackgroundProperty, line.GetObservable(BackgroundProperty));
                        AbstractSkeleton.SetIsShimmerBlock(content, true);
                        var root = new Panel { Name = RootLayoutPart, Children = { content } };
                        root.RegisterInNameScope(scope);
                        content.RegisterInNameScope(scope);
                        return root;
                    }))
                }
            };
        }
    }

    private sealed class TestParagraph : AbstractSkeletonParagraph
    {
        public TestParagraph()
        {
            Background        = BlockBrush;
            MotionDuration    = TimeSpan.FromSeconds(1.4);
            MotionEasingCurve = new LinearEasing();
            Theme = new ControlTheme(typeof(TestParagraph))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<TestParagraph>((_, scope) =>
                    {
                        var layout = new StackPanel { Name = LineLayoutPart, Spacing = 8 };
                        return layout.RegisterInNameScope(scope);
                    }))
                }
            };
        }

        public IReadOnlyList<Border> Lines =>
            this.GetVisualDescendants().OfType<Border>().ToList();

        protected override Control CreateLine()
        {
            var line = new Border { Height = 16, CornerRadius = new CornerRadius(2) };
            AbstractSkeleton.SetIsShimmerBlock(line, true);
            return line;
        }

        protected override void ApplyLineWidth(Control line, Dimension? width)
        {
        }

        protected override void ApplyLineState(Control line)
        {
            ((Border)line).Background = Background;
        }
    }

    // Composite: template = PART_PlaceholderHost with the content presenter; the placeholder (built only while
    // loading) is Border#PART_ShimmerLayer holding an avatar block, a nested line and a nested paragraph.
    private sealed class TestComposite : AbstractCompositeSkeleton
    {
        public TestComposite()
        {
            MotionDuration    = TimeSpan.FromSeconds(1.4);
            MotionEasingCurve = new LinearEasing();
            Theme             = CreateTheme();
        }

        public static ControlTheme CreateTheme()
        {
            return new ControlTheme(typeof(TestComposite))
            {
                Setters =
                {
                    new Setter(TemplateProperty, new FuncControlTemplate<TestComposite>((composite, scope) =>
                    {
                        var presenter = new ContentPresenter { Name = "PART_ContentPresenter" };
                        presenter.Bind(ContentPresenter.ContentProperty, composite.GetObservable(ContentProperty));
                        presenter.Bind(IsVisibleProperty, composite.GetObservable(IsContentVisibleProperty));
                        var host = new Panel { Name = PlaceholderHostPart, Children = { presenter } };
                        host.RegisterInNameScope(scope);
                        return host;
                    })),
                    new Setter(PlaceholderTemplateProperty, new FuncControlTemplate<TestComposite>((_, scope) =>
                    {
                        var avatar = new Border
                        {
                            Width = 40, Height = 40, CornerRadius = new CornerRadius(20), Background = BlockBrush,
                            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left
                        };
                        AbstractSkeleton.SetIsShimmerBlock(avatar, true);
                        var layer = new Border
                        {
                            Name = ShimmerLayerPart,
                            Child = new StackPanel
                            {
                                Children = { avatar, new TestLine(), new TestParagraph { Rows = 2 } }
                            }
                        };
                        layer.RegisterInNameScope(scope);
                        return layer;
                    }))
                }
            };
        }
    }

    private sealed class TestHost : IDisposable
    {
        public readonly Window Window;
        public readonly MotionGateTests.TestActivitySource Source = new();

        public TestHost(Control content, double width = 400)
        {
            Window = new Window { Width = width, Height = 300, Content = content };
            MotionActivity.SetSource(Window, Source);
            Window.Show();
            Pump();
        }

        public void Pump()
        {
            for (var i = 0; i < 3; i++)
            {
                Dispatcher.UIThread.RunJobs();
                Window.UpdateLayout();
                AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            }
            Dispatcher.UIThread.RunJobs();
        }

        public int RunningSources =>
            Window.GetVisualDescendants().OfType<AbstractSkeleton>().Count(skeleton => skeleton.IsShimmerRunning);

        public void Dispose()
        {
            Window.Content = null;
            Window.Close();
        }
    }

    private static bool HasAnimationValue(Border block) => block.IsAnimating(Border.BackgroundProperty);

    [Fact]
    public void Band_sample_matches_desktop_trajectory()
    {
        var width    = 200d;
        var duration = TimeSpan.FromSeconds(1.4);
        var linear   = new LinearEasing();
        // The band's brightest stop sits PeakOffset of the band width (1.52W) from its left edge.
        double Peak(double seconds) =>
            SkeletonShimmerVisualHandler.SampleBandLeft(seconds, width, duration, linear) +
            SkeletonShimmerVisualHandler.PeakOffset * SkeletonShimmerVisualHandler.BandWidthFactor * width;

        SkeletonShimmerVisualHandler.SampleBandLeft(0, width, duration, linear).ShouldBe(-2 * width, 1e-9);
        Peak(0).ShouldBe(-1.52 * width, 0.001 * width);
        Peak(1.4 - 1e-9).ShouldBe(1.48 * width, 0.001 * width);
        Peak(0.7).ShouldBe(-0.02 * width, 0.001 * width);
        // Periodic: one cycle later the band is back at the start.
        SkeletonShimmerVisualHandler.SampleBandLeft(1.4 * 3 + 0.35, width, duration, linear)
                                    .ShouldBe(SkeletonShimmerVisualHandler.SampleBandLeft(0.35, width, duration, linear), 1e-9);

        var curve = AbstractSkeleton.DefaultShimmerEasing.ShouldBeOfType<SplineEasing>();
        curve.X1.ShouldBe(0.25);
        curve.Y1.ShouldBe(0.1);
        curve.X2.ShouldBe(0.25);
        curve.Y2.ShouldBe(1.0);
        new TestLine { MotionEasingCurve = null }.EffectiveShimmerEasing.ShouldBeSameAs(AbstractSkeleton.DefaultShimmerEasing);
    }

    [Fact]
    public void Band_brush_uses_absolute_band_geometry()
    {
        var brush = SkeletonShimmerVisualHandler.CreateBandBrush(Colors.LightGray, Colors.WhiteSmoke, 100);
        brush.StartPoint.ShouldBe(new RelativePoint(0, 0, RelativeUnit.Absolute));
        brush.EndPoint.Unit.ShouldBe(RelativeUnit.Absolute);
        brush.EndPoint.Point.X.ShouldBe(152, 1e-9);
        brush.EndPoint.Point.Y.ShouldBe(0);
        brush.SpreadMethod.ShouldBe(GradientSpreadMethod.Pad);
        brush.GradientStops.Select(stop => stop.Offset).ShouldBe([0, 0.316, 1]);
        brush.GradientStops.Select(stop => stop.Color).ShouldBe([Colors.LightGray, Colors.WhiteSmoke, Colors.LightGray]);
    }

    // Runs the per-frame work — sampling with the default spline and drawing the clipped blocks — on the drawing
    // context the compositor hands to custom drawing.
    private sealed class FrameLoop : ICustomDrawOperation
    {
        private readonly RoundedRect[] _blocks =
        [
            new(new Rect(0, 0, 40, 40), 20),
            new(new Rect(56, 0, 144, 16), 4),
            new(new Rect(56, 32, 200, 16), new CornerRadius(4, 0, 0, 4))
        ];

        private readonly IImmutableBrush _brush =
            SkeletonShimmerVisualHandler.CreateBandBrush(Colors.LightGray, Colors.WhiteSmoke, 256);

        public long AllocatedBytes { get; private set; } = -1;

        public Rect Bounds => new(0, 0, 256, 48);

        public void Render(ImmediateDrawingContext context)
        {
            Frames(context, 10);
            var before = GC.GetAllocatedBytesForCurrentThread();
            Frames(context, 1000);
            AllocatedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        }

        private void Frames(ImmediateDrawingContext context, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var left = SkeletonShimmerVisualHandler.SampleBandLeft(i / 60d, 256, TimeSpan.FromSeconds(1.4),
                    AbstractSkeleton.DefaultShimmerEasing);
                SkeletonShimmerVisualHandler.DrawFrame(context, _blocks, _brush, left);
            }
        }

        public bool HitTest(Point p) => false;

        public bool Equals(ICustomDrawOperation? other) => false;

        public void Dispose()
        {
        }
    }

    private sealed class FrameLoopHost : Control
    {
        public readonly FrameLoop Loop = new();

        public override void Render(DrawingContext context) => context.Custom(Loop);
    }

    [Fact]
    public void Sample_and_render_allocate_nothing()
    {
        var host = new FrameLoopHost { Width = 256, Height = 48 };
        using var testHost = new TestHost(host);

        host.Loop.AllocatedBytes.ShouldBe(0);
    }

    [Fact]
    public void Handler_follows_scene_start_and_stop_messages()
    {
        var line = new TestLine { IsActive = true, Width = 200 };
        using var host = new TestHost(line);

        line.IsShimmerRunning.ShouldBeTrue();
        var handler = line.ShimmerHandler.ShouldNotBeNull();
        handler.IsRunning.ShouldBeTrue();
        handler.Blocks.ShouldBe([new RoundedRect(new Rect(0, 0, 200, 16), new CornerRadius(4))]);
        handler.BandWidth.ShouldBe(200);
        ElementComposition.GetElementChildVisual(line.ContentLayer).ShouldBeSameAs(line.ShimmerVisual);

        line.IsActive = false;
        host.Pump();
        handler.IsRunning.ShouldBeFalse();
    }

    [Fact]
    public void Standalone_paragraph_is_its_own_host()
    {
        var paragraph = new TestParagraph { Rows = 3, IsActive = true };
        using var host = new TestHost(paragraph);

        paragraph.IsShimmerHost.ShouldBeTrue();
        paragraph.ShimmerBlockCount.ShouldBe(3);
        paragraph.IsShimmerRunning.ShouldBeTrue();
        paragraph.ShimmerLayer.ShouldBeSameAs(paragraph);
        host.RunningSources.ShouldBe(1);
        var scene = paragraph.LastShimmerScene.ShouldNotBeNull();
        scene.Blocks.Length.ShouldBe(3);
        scene.Width.ShouldBe(400);
        scene.Blocks[1].Rect.Y.ShouldBe(24);
    }

    [Fact]
    public void Gate_truth_table_starts_and_stops()
    {
        var line   = new TestLine { IsActive = true };
        var parent = new Panel { Children = { line } };
        using var host = new TestHost(parent);
        line.IsShimmerRunning.ShouldBeTrue();

        line.IsMotionEnabled = false;
        host.Pump();
        line.IsShimmerRunning.ShouldBeFalse();
        line.IsMotionEnabled = true;
        host.Pump();
        line.IsShimmerRunning.ShouldBeTrue();

        parent.IsVisible = false;
        host.Pump();
        line.IsShimmerRunning.ShouldBeFalse();
        parent.IsVisible = true;
        host.Pump();
        line.IsShimmerRunning.ShouldBeTrue();

        host.Source.IsActive = false;
        host.Pump();
        line.IsShimmerRunning.ShouldBeFalse();
        host.Source.IsActive = true;
        host.Pump();
        line.IsShimmerRunning.ShouldBeTrue();

        line.MotionDuration = TimeSpan.Zero;
        host.Pump();
        line.IsShimmerRunning.ShouldBeFalse();
        line.MotionDuration = TimeSpan.FromSeconds(1.4);
        host.Pump();
        line.IsShimmerRunning.ShouldBeTrue();

        parent.Children.Remove(line);
        host.Pump();
        line.IsShimmerRunning.ShouldBeFalse();
        line.IsShimmerHost.ShouldBeFalse();
        parent.Children.Add(line);
        host.Pump();
        line.IsShimmerRunning.ShouldBeTrue();

        line.IsActive = false;
        host.Pump();
        line.IsShimmerRunning.ShouldBeFalse();
    }

    [Fact]
    public void Stop_restores_block_backgrounds()
    {
        var line = new TestLine();
        using var host = new TestHost(line);
        var block = line.ContentLayer;
        block.Background.ShouldBeSameAs(BlockBrush);
        HasAnimationValue(block).ShouldBeFalse();

        line.IsActive = true;
        host.Pump();
        block.Background.ShouldBeNull();
        HasAnimationValue(block).ShouldBeTrue();

        line.IsActive = false;
        host.Pump();
        block.Background.ShouldBeSameAs(BlockBrush);
        HasAnimationValue(block).ShouldBeFalse();
    }

    [Fact]
    public void Placeholder_not_created_when_not_loading()
    {
        var composite = new TestComposite { Content = new TextBlock { Text = "content" } };
        using var host = new TestHost(composite);
        var placeholderHost = composite.GetVisualDescendants().OfType<Panel>()
                                       .Single(panel => panel.Name == AbstractCompositeSkeleton.PlaceholderHostPart);

        composite.Placeholder.ShouldBeNull();
        placeholderHost.Children.Count.ShouldBe(1);
        composite.GetVisualDescendants().OfType<AbstractSkeleton>().ShouldBeEmpty();

        composite.IsLoading = true;
        host.Pump();
        var placeholder = composite.Placeholder.ShouldNotBeNull();
        placeholderHost.Children.Count.ShouldBe(2);
        placeholderHost.Children[0].ShouldBeSameAs(placeholder);
        placeholder.TemplatedParent.ShouldBeSameAs(composite);
        composite.ShimmerLayer.ShouldBeSameAs(placeholder);

        composite.IsLoading = false;
        host.Pump();
        composite.Placeholder.ShouldBeNull();
        placeholder.GetVisualParent().ShouldBeNull();
        placeholderHost.Children.Count.ShouldBe(1);
        composite.ShimmerBlockCount.ShouldBe(0);
    }

    [Fact]
    public void Composite_runs_one_source_for_nested_blocks()
    {
        var composite = new TestComposite { IsLoading = true, IsActive = true };
        using var host = new TestHost(composite);

        composite.IsShimmerHost.ShouldBeTrue();
        composite.IsShimmerRunning.ShouldBeTrue();
        host.RunningSources.ShouldBe(1);
        // Avatar, the nested line's content layer and two paragraph rows.
        composite.ShimmerBlockCount.ShouldBe(4);
        var nested = composite.GetVisualDescendants().OfType<AbstractSkeleton>().ToList();
        nested.Count.ShouldBe(2);
        nested.ShouldAllBe(skeleton => !skeleton.IsShimmerHost && skeleton.ShimmerBlockCount == 0);
        nested.ShouldAllBe(skeleton => skeleton.ShimmerVisual == null);
        // The band spans the whole placeholder, so rows narrower than it share one trajectory.
        composite.LastShimmerScene.ShouldNotBeNull().Width.ShouldBe(400);
    }

    [Fact]
    public void Skeleton_in_content_keeps_its_own_shimmer()
    {
        var line      = new TestLine { IsActive = true };
        var composite = new TestComposite { IsActive = true, Content = line };
        using var host = new TestHost(composite);

        composite.IsShimmerRunning.ShouldBeFalse();
        composite.ShimmerBlockCount.ShouldBe(0);
        line.IsShimmerHost.ShouldBeTrue();
        line.ShimmerBlockCount.ShouldBe(1);
        line.IsShimmerRunning.ShouldBeTrue();
        host.RunningSources.ShouldBe(1);
    }

    [Fact]
    public void Rapid_loading_toggle_keeps_single_source()
    {
        var composite = new TestComposite { IsActive = true };
        using var host = new TestHost(composite);
        var released = new List<Border>();

        for (var i = 0; i < 5; i++)
        {
            composite.IsLoading = true;
            composite.IsLoading = false;
            composite.IsLoading = true;
            Dispatcher.UIThread.RunJobs();
            var blocks = composite.Placeholder!.GetVisualDescendants().OfType<Border>()
                                  .Where(AbstractSkeleton.GetIsShimmerBlock).ToList();
            composite.IsLoading = false;
            released.AddRange(blocks);
            composite.IsLoading = true;
            host.Pump();

            host.RunningSources.ShouldBe(1);
            composite.IsShimmerRunning.ShouldBeTrue();
            composite.ShimmerBlockCount.ShouldBe(4);
            ElementComposition.GetElementChildVisual(composite.Placeholder!).ShouldBeSameAs(composite.ShimmerVisual);
        }

        released.ShouldAllBe(block => !HasAnimationValue(block));
        composite.IsLoading = false;
        host.Pump();
        host.RunningSources.ShouldBe(0);
        composite.ShimmerVisual.ShouldBeNull();
    }

    [Fact]
    public void Resize_rebuilds_clip_once()
    {
        var paragraph = new TestParagraph { Rows = 2, IsActive = true };
        using var host = new TestHost(paragraph);
        var builds = paragraph.ShimmerSceneBuildCount;
        paragraph.LastShimmerScene!.Value.Width.ShouldBe(400);

        host.Window.Width = 300;
        host.Pump();
        paragraph.ShimmerSceneBuildCount.ShouldBe(builds + 1);
        paragraph.LastShimmerScene!.Value.Width.ShouldBe(300);
        paragraph.LastShimmerScene!.Value.Blocks.ShouldAllBe(block => block.Rect.Width == 300);

        // Later layout passes that move nothing keep the scene.
        paragraph.InvalidateMeasure();
        host.Pump();
        host.Window.InvalidateArrange();
        host.Pump();
        paragraph.ShimmerSceneBuildCount.ShouldBe(builds + 1);
    }

    [Fact]
    public void Colors_and_timing_resend_the_scene_while_running()
    {
        var line = new TestLine { IsActive = true };
        using var host = new TestHost(line);
        line.ShimmerToColor = Brushes.Red;
        line.LastShimmerScene!.Value.To.ShouldBe(Colors.Red);
        line.MotionDuration = TimeSpan.FromSeconds(2);
        line.LastShimmerScene!.Value.Duration.ShouldBe(TimeSpan.FromSeconds(2));
        var easing = new QuadraticEaseIn();
        line.MotionEasingCurve = easing;
        line.LastShimmerScene!.Value.Easing.ShouldBeSameAs(easing);
    }

    [Fact]
    public void Scene_easing_is_a_private_copy_of_a_spline()
    {
        var spline = new SplineEasing(0.42, 0, 0.58, 1);
        var line   = new TestLine { IsActive = true, MotionEasingCurve = spline };
        using var host = new TestHost(line);

        var sent = line.LastShimmerScene.ShouldNotBeNull().Easing.ShouldBeOfType<SplineEasing>();
        sent.ShouldNotBeSameAs(spline);
        (sent.X1, sent.Y1, sent.X2, sent.Y2).ShouldBe((0.42, 0d, 0.58, 1d));
        line.ShimmerHandler.ShouldNotBeNull();

        // The shared default curve is copied as well; stateless built-in easings pass through.
        line.MotionEasingCurve = null;
        var copiedDefault = line.LastShimmerScene!.Value.Easing.ShouldBeOfType<SplineEasing>();
        copiedDefault.ShouldNotBeSameAs(AbstractSkeleton.DefaultShimmerEasing);
        var linear = new LinearEasing();
        line.MotionEasingCurve = linear;
        line.LastShimmerScene!.Value.Easing.ShouldBeSameAs(linear);
    }

    [Fact]
    public void First_activation_creates_one_handler()
    {
        var line = new TestLine { IsActive = true };
        using (var host = new TestHost(line))
        {
            line.IsShimmerRunning.ShouldBeTrue();
            line.ShimmerVisualCreateCount.ShouldBe(1);
            line.ShimmerVisualReleaseCount.ShouldBe(0);
            line.ShimmerLayer.ShouldBeSameAs(line.ContentLayer);
        }

        var composite = new TestComposite { IsLoading = true, IsActive = true };
        using (var host = new TestHost(composite))
        {
            composite.IsShimmerRunning.ShouldBeTrue();
            composite.ShimmerVisualCreateCount.ShouldBe(1);
            composite.ShimmerVisualReleaseCount.ShouldBe(0);
            composite.ShimmerLayer.ShouldBeSameAs(composite.Placeholder);
        }
    }

    [Fact]
    public void Band_without_blocks_requests_no_frames()
    {
        var line = new TestLine { IsActive = true };
        using var host = new TestHost(line);
        var handler = line.ShimmerHandler.ShouldNotBeNull();
        handler.IsFrameRequested.ShouldBeTrue();

        // The only block disappears: the host keeps running, the handler stops asking for frames.
        line.ContentLayer.IsVisible = false;
        host.Pump();
        line.IsShimmerRunning.ShouldBeTrue();
        handler.Blocks.ShouldBeEmpty();
        var requests = handler.FrameRequestCount;
        host.Pump();
        host.Pump();
        handler.FrameRequestCount.ShouldBe(requests);
        handler.IsFrameRequested.ShouldBeFalse();

        // A scene with blocks re-arms the frames.
        line.ContentLayer.IsVisible = true;
        host.Pump();
        handler.Blocks.Length.ShouldBe(1);
        handler.FrameRequestCount.ShouldBeGreaterThan(requests);
        host.Pump();
        handler.IsFrameRequested.ShouldBeTrue();
    }

    [Fact]
    public void Template_reapply_keeps_single_handler()
    {
        var line = new TestLine { IsActive = true };
        using var host = new TestHost(line);
        var oldLayer  = line.ContentLayer;
        var oldVisual = line.ShimmerVisual.ShouldNotBeNull();

        line.Theme = TestLine.CreateTheme(AbstractSkeleton.ShimmerLayerPart);
        host.Pump();

        var newLayer = line.GetVisualDescendants().OfType<Border>().Single();
        newLayer.ShouldNotBeSameAs(oldLayer);
        line.ShimmerLayer.ShouldBeSameAs(newLayer);
        ElementComposition.GetElementChildVisual(oldLayer).ShouldBeNull();
        ElementComposition.GetElementChildVisual(newLayer).ShouldBeSameAs(line.ShimmerVisual);
        line.ShimmerVisual.ShouldNotBeSameAs(oldVisual);
        (line.ShimmerVisualCreateCount - line.ShimmerVisualReleaseCount).ShouldBe(1);
        line.IsShimmerRunning.ShouldBeTrue();
        line.ShimmerBlockCount.ShouldBe(1);
        HasAnimationValue(oldLayer).ShouldBeFalse();
    }
}
