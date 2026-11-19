using AtomUI.Controls;
using AtomUI.Controls.Commons;
using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUISkeleton = AtomUI.Desktop.Controls.Skeleton;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Skeleton;

public class SkeletonShimmerDesktopTests
{
    static SkeletonShimmerDesktopTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    private sealed class Host : IDisposable
    {
        public readonly AvaloniaWindow Window;

        public Host(Control content)
        {
            Window = new AvaloniaWindow { Width = 480, Height = 300, Content = content };
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

        public List<AbstractSkeleton> Skeletons =>
            Window.GetVisualDescendants().OfType<AbstractSkeleton>().ToList();

        public void Dispose()
        {
            Window.Content = null;
            Window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Composite_has_single_shimmer_source()
    {
        var skeleton = new AtomUISkeleton
        {
            IsLoading     = true,
            IsActive      = true,
            IsShowAvatar  = true,
            ParagraphRows = 3
        };
        using var host = new Host(skeleton);

        skeleton.IsShimmerHost.ShouldBeTrue();
        skeleton.IsShimmerRunning.ShouldBeTrue();
        host.Skeletons.Count(s => s.IsShimmerRunning).ShouldBe(1);
        // The first activation hangs one visual on the placeholder layer and never moves it.
        skeleton.ShimmerVisualCreateCount.ShouldBe(1);
        skeleton.ShimmerVisualReleaseCount.ShouldBe(0);
        host.Skeletons.Count(s => s.IsShimmerHost).ShouldBe(1);
        // Avatar, title and three paragraph rows.
        skeleton.ShimmerBlockCount.ShouldBe(5);
        skeleton.ShimmerLayer.ShouldNotBeNull().Name.ShouldBe(AbstractSkeleton.ShimmerLayerPart);
        var scene = skeleton.LastShimmerScene.ShouldNotBeNull();
        scene.Blocks.Length.ShouldBe(5);
        scene.Width.ShouldBe(skeleton.ShimmerLayer!.Bounds.Width);
        // Rows narrower than the placeholder share its trajectory: every block lies inside one band width.
        scene.Blocks.ShouldAllBe(block => block.Rect.Right <= scene.Width + 0.001);
        foreach (var nested in host.Skeletons.Where(s => !ReferenceEquals(s, skeleton)))
        {
            nested.IsShimmerHost.ShouldBeFalse();
            nested.ShimmerVisual.ShouldBeNull();
        }
    }

    [Fact]
    public void Placeholder_not_created_when_not_loading()
    {
        var skeleton = new AtomUISkeleton { IsShowAvatar = true, Content = new Avalonia.Controls.TextBlock { Text = "x" } };
        using var host = new Host(skeleton);

        skeleton.Placeholder.ShouldBeNull();
        skeleton.GetVisualDescendants().OfType<AbstractSkeleton>().ShouldBeEmpty();
        // Border#PART_RootLayout, Panel#PART_PlaceholderHost, ContentPresenter, TextBlock.
        skeleton.GetVisualDescendants().Count().ShouldBe(4);

        skeleton.IsLoading = true;
        host.Pump();
        skeleton.Placeholder.ShouldNotBeNull();
        skeleton.GetVisualDescendants().OfType<SkeletonParagraph>().Count().ShouldBe(1);
    }

    [Fact]
    public void Placeholder_parts_are_template_scoped()
    {
        var skeleton = new AtomUISkeleton { IsLoading = true, IsShowAvatar = true };
        using var host = new Host(skeleton);

        var placeholder = skeleton.Placeholder.ShouldNotBeNull();
        placeholder.TemplatedParent.ShouldBeSameAs(skeleton);
        placeholder.Name.ShouldBe(AbstractSkeleton.ShimmerLayerPart);

        var avatar = skeleton.GetVisualDescendants().OfType<SkeletonAvatar>().Single();
        avatar.TemplatedParent.ShouldBeSameAs(skeleton);
        avatar.Classes.ShouldContain("semantic-avatar");
        // `^ /template/ atom|SkeletonAvatar` reaches the placeholder's avatar.
        avatar.Margin.Right.ShouldBeGreaterThan(0);
        // `^[IsShowTitle=True] /template/ atom|SkeletonParagraph` too.
        skeleton.GetVisualDescendants().OfType<SkeletonParagraph>().Single().Margin.Top.ShouldBeGreaterThan(0);
        // Template bindings resolve against the skeleton.
        skeleton.IsActive = true;
        host.Pump();
        avatar.IsActive.ShouldBeTrue();
        skeleton.GetVisualDescendants().OfType<SkeletonTitle>().Single().IsActive.ShouldBeTrue();
        // The placeholder's names live in its own scope, not the control template's.
        skeleton.GetVisualDescendants().OfType<DockPanel>().Single().Classes.ShouldContain("semantic-header");
    }

    [Fact]
    public void Theme_motion_parameters()
    {
        var line = new SkeletonLine();
        using var host = new Host(line);

        line.MotionDuration.ShouldBe(TimeSpan.FromSeconds(1.4));
        line.MotionEasingCurve.ShouldBeNull();
        line.EffectiveShimmerEasing.ShouldBeOfType<SplineEasing>().X1.ShouldBe(0.25);
        line.IsMotionEnabled.ShouldBeTrue();
        line.TryFindResource(SkeletonTokenKind.GradientFromColor, line.ActualThemeVariant, out var fromResource).ShouldBeTrue();
        line.TryFindResource(SkeletonTokenKind.GradientToColor, line.ActualThemeVariant, out var toResource).ShouldBeTrue();
        var from = fromResource.ShouldBeAssignableTo<ISolidColorBrush>().ShouldNotBeNull();
        var to   = toResource.ShouldBeAssignableTo<ISolidColorBrush>().ShouldNotBeNull();
        from.Color.ShouldNotBe(to.Color);
        // The band's base colour is the blocks' own background, so a resting frame and the band edges match.
        from.Color.ShouldBe(((ISolidColorBrush)line.Background!).Color);
        line.IsActive = true;
        host.Pump();
        line.LastShimmerScene.ShouldNotBeNull().From.ShouldBe(from.Color);
        line.LastShimmerScene!.Value.To.ShouldBe(to.Color);
    }

    [Fact]
    public void Element_templates_mark_one_always_visible_shimmer_block()
    {
        Control[] elements =
        [
            new SkeletonLine(), new SkeletonAvatar(), new SkeletonButton(), new SkeletonInput(), new SkeletonImage(),
            new SkeletonNode()
        ];
        var panel = new StackPanel();
        foreach (var element in elements)
        {
            panel.Children.Add(element);
        }
        using var host = new Host(panel);

        foreach (var element in elements.Cast<AbstractSkeleton>())
        {
            var blocks = element.GetVisualDescendants().OfType<Border>().Where(AbstractSkeleton.GetIsShimmerBlock).ToList();
            blocks.Count.ShouldBe(1, element.GetType().Name);
            blocks[0].Name.ShouldBe(AbstractSkeleton.ContentLayerPart);
            blocks[0].IsVisible.ShouldBeTrue();
            element.ShimmerLayer.ShouldBeSameAs(blocks[0]);
            element.IsShimmerHost.ShouldBeTrue();
            element.ShimmerBlockCount.ShouldBe(1);
        }
    }

    [Fact]
    public void Skeleton_in_content_keeps_its_own_shimmer()
    {
        var line     = new SkeletonLine { IsActive = true };
        var skeleton = new AtomUISkeleton { IsActive = true, Content = line };
        using var host = new Host(skeleton);

        skeleton.IsShimmerRunning.ShouldBeFalse();
        line.IsShimmerHost.ShouldBeTrue();
        line.IsShimmerRunning.ShouldBeTrue();
        ElementComposition.GetElementChildVisual(line.ShimmerLayer!).ShouldBeSameAs(line.ShimmerVisual);
    }
}
