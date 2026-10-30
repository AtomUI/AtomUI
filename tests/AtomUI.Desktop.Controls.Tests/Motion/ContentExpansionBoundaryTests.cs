using System.Reflection;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using MotionApi = AtomUI.Animations.Motion;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomExpander = AtomUI.Desktop.Controls.Expander;

namespace AtomUI.Desktop.Controls.Tests.Motion;

public class ContentExpansionBoundaryTests
{
    static ContentExpansionBoundaryTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(ExpandDirection.Down)]
    [InlineData(ExpandDirection.Up)]
    [InlineData(ExpandDirection.Left)]
    [InlineData(ExpandDirection.Right)]
    public void Fixed_Root_Size_Preserves_The_Frame_While_The_Content_Viewport_Clips(ExpandDirection direction)
    {
        using var scene = new Scene(direction);
        var full = scene.Extent;
        var rootSize = scene.Expander.Bounds.Size;
        var contentSize = scene.Actor.Presenter!.Bounds.Size;
        scene.Expander.IsExpanded = false;
        scene.Step(0);
        scene.Step(500);

        scene.Extent.ShouldBeGreaterThan(0);
        scene.Extent.ShouldBeLessThan(full - 1);
        scene.Expander.Bounds.Size.ShouldBe(rootSize);
        scene.Actor.Presenter.Bounds.Size.ShouldBe(contentSize);
        var header = scene.Expander.GetVisualDescendants().OfType<Control>().First(c => c.Name == "PART_HeaderLayoutTransform");
        var content = scene.Actor.TranslatePoint(default, scene.Expander)!.Value;
        var heading = header.TranslatePoint(default, scene.Expander)!.Value;
        var gap = direction switch
        {
            ExpandDirection.Down => content.Y - heading.Y - header.Bounds.Height,
            ExpandDirection.Up => heading.Y - content.Y - scene.Actor.Bounds.Height,
            ExpandDirection.Left => heading.X - content.X - scene.Actor.Bounds.Width,
            _ => content.X - heading.X - header.Bounds.Width
        };
        gap.ShouldBe(0, 1);
        scene.Step(1100);
        scene.Actor.IsVisible.ShouldBeFalse();
        scene.Expander.IsExpanded = true;
        scene.Step(1100);
        scene.Step(1600);
        scene.Extent.ShouldBeGreaterThan(0);
        scene.Extent.ShouldBeLessThan(full - 1);
        scene.StepBeforeContinuations(2200);
        scene.Extent.ShouldBe(full, 1, "The final frame must match the restored stretched layout.");
        scene.Pump();
        scene.Extent.ShouldBe(full, 1);
        scene.Expander.Bounds.Size.ShouldBe(rootSize);
    }

    [Fact]
    public async Task Reentrant_Visibility_Projection_Disabling_Motion_Leaves_No_Active_Execution()
    {
        using var scene = new Scene(ExpandDirection.Down);
        Task<MotionResult>? superseded = null;
        using var subscription = scene.Actor.GetObservable(MotionApi.IsVisibleProperty).Subscribe(visible =>
        {
            if (!visible)
            {
                superseded = MotionApi.GetVisibilityTask(scene.Actor);
                scene.Expander.IsMotionEnabled = false;
            }
        });
        scene.Expander.IsExpanded = false;
        scene.Pump();
        scene.Actor.IsVisible.ShouldBeFalse();
        MotionApi.GetVisibilityTask(scene.Actor).IsCompleted.ShouldBeTrue();
        _ = superseded.ShouldNotBeNull();
        superseded.IsCompleted.ShouldBeTrue();
        (await superseded).Outcome.ShouldBe(MotionOutcome.Superseded);
        scene.Step(0);
        scene.Step(1100);
        (await superseded).Outcome.ShouldBe(MotionOutcome.Superseded);
    }

    [Fact]
    public void Reentrant_Stable_State_Projection_Cannot_Overwrite_A_Newer_Target()
    {
        using var scene = new Scene(ExpandDirection.Down);
        scene.Expander.IsMotionEnabled = false;
        using var subscription = scene.Actor.GetObservable(Visual.IsVisibleProperty).Subscribe(visible =>
        {
            if (!visible)
            {
                scene.Expander.IsExpanded = true;
            }
        });

        scene.Expander.IsExpanded = false;
        scene.Pump();
        scene.Expander.IsExpanded.ShouldBeTrue();
        scene.Actor.IsVisible.ShouldBeTrue();
        scene.Actor.Opacity.ShouldBe(1);
    }

    [Fact]
    public void Changing_Content_During_Opening_Preserves_Current_Size_And_Original_Deadline()
    {
        using var scene = new Scene(ExpandDirection.Down);
        scene.Expander.IsMotionEnabled = false;
        scene.Expander.ClearValue(Control.HeightProperty);
        var content = new Border { Height = 120 };
        scene.Expander.Content = content;
        scene.Pump();
        var initialFull = scene.Extent;
        scene.Expander.IsExpanded = false;
        scene.Pump();
        scene.Expander.IsMotionEnabled = true;
        scene.Expander.IsExpanded = true;
        scene.Step(0);
        scene.Step(400);
        var presented = scene.Extent;
        content.Height = 220;
        scene.Pump();
        scene.Extent.ShouldBe(presented, 1);
        scene.Step(800);
        scene.Extent.ShouldBeGreaterThan(presented);
        scene.StepBeforeContinuations(1100);
        scene.Extent.ShouldBe(initialFull + 100, 1);
        scene.Pump();
        scene.Extent.ShouldBe(initialFull + 100, 1);
        MotionApi.GetVisibilityTask(scene.Actor).IsCompleted.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ExpandDirection.Up)]
    [InlineData(ExpandDirection.Left)]
    [InlineData(ExpandDirection.Right)]
    public void Direction_Change_During_Playback_Settles_Current_Target(ExpandDirection direction)
    {
        using var scene = new Scene(ExpandDirection.Down);
        scene.Expander.IsExpanded = false;
        scene.Step(0);
        scene.Step(400);
        scene.Expander.ExpandDirection = direction;
        scene.Pump();
        scene.Actor.IsVisible.ShouldBeFalse();
        MotionApi.GetVisibilityTask(scene.Actor).IsCompleted.ShouldBeTrue();
        scene.Expander.IsExpanded = true;
        scene.Step(400);
        scene.Step(1500);
        scene.Actor.IsVisible.ShouldBeTrue();
        scene.Actor.Opacity.ShouldBe(1);
        scene.Extent.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Three_Level_Expansion_Tracks_Animating_Descendants_Without_Extending_Parent_Deadline()
    {
        using var scene = new Scene(ExpandDirection.Down);
        var grandchild = new AtomExpander
        {
            Header = "Third level", Content = new Border { Height = 80 },
            IsMotionEnabled = false, IsExpanded = true, MotionDuration = TimeSpan.FromSeconds(1)
        };
        var child = new AtomExpander
        {
            Header = "Second level", Content = grandchild,
            IsMotionEnabled = false, IsExpanded = true, MotionDuration = TimeSpan.FromSeconds(1)
        };
        scene.Expander.IsMotionEnabled = false;
        scene.Expander.ClearValue(Control.HeightProperty);
        scene.Expander.Content = child;
        scene.Pump();
        child.IsExpanded = false;
        scene.Pump();
        var finalExtent = scene.Extent;
        child.IsExpanded = true;
        scene.Expander.IsExpanded = false;
        scene.Pump();
        scene.Expander.IsMotionEnabled = child.IsMotionEnabled = grandchild.IsMotionEnabled = true;
        scene.Expander.IsExpanded = true;
        scene.Step(0);
        scene.Step(200);
        child.IsExpanded = grandchild.IsExpanded = false;
        scene.Step(200);
        for (var time = 300; time <= 1100; time += 100)
            scene.Step(time);

        scene.Actor.IsVisible.ShouldBeTrue();
        MotionApi.GetVisibilityTask(scene.Actor).IsCompleted.ShouldBeTrue(
            "Repeated descendant layout changes must not restart the parent animation clock.");
        scene.Step(1300);
        scene.Extent.ShouldBe(finalExtent, 1);
        scene.Expander.IsExpanded.ShouldBeTrue();
        child.IsExpanded.ShouldBeFalse();
        grandchild.IsExpanded.ShouldBeFalse();
    }

    private sealed class Scene : IDisposable
    {
        private readonly ManualMotionClock _clock = new();
        private readonly Avalonia.Controls.Window _window;
        public AtomExpander Expander { get; }
        public ContentViewport Actor { get; }
        public double Extent => Expander.ExpandDirection is ExpandDirection.Left or ExpandDirection.Right ? Actor.Bounds.Width : Actor.Bounds.Height;

        public Scene(ExpandDirection direction)
        {
            Expander = new AtomExpander
            {
                Header = "Fixed size", Height = 300, Width = 320,
                Content = new TextBlock { Text = "Content stays at its normal size while its viewport clips.", TextWrapping = TextWrapping.Wrap },
                IsExpanded = true, IsMotionEnabled = false, ExpandDirection = direction,
                MotionDuration = TimeSpan.FromSeconds(1)
            };
            _window = new Avalonia.Controls.Window { Width = 400, Height = 400, Content = new StackPanel { Children = { Expander } } };
            _window.Show();
            Pump();
            Actor = Expander.GetVisualDescendants().OfType<ContentViewport>().First(c => c.Name == "PART_ContentViewport");
            Expander.IsMotionEnabled = true;
        }

        public void Step(int ms) { _clock.Step(ms); Pump(); }
        public void StepBeforeContinuations(int ms) { _clock.Step(ms); _window.UpdateLayout(); }
        public void Pump() { Dispatcher.UIThread.RunJobs(); _window.UpdateLayout(); }
        public void Dispose() { _window.Close(); Pump(); _clock.Dispose(); }
    }
}
