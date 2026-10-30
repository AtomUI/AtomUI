using AtomUI.Controls.Primitives;
using System.Reflection;
using AtomUI.Animations;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomExpander = AtomUI.Desktop.Controls.Expander;
using AtomButton = AtomUI.Desktop.Controls.Button;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.Motion;

public class ContentExpansionInputTests
{
    static ContentExpansionInputTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Nested_Wheel_Scrolling_And_Boundary_Chaining_Remain_Available_During_Collapse(double scaling)
    {
        using var scene = new InputScene(scaling);
        scene.AssertOverflowAndScrollbars();
        var viewport = scene.Inner.Viewport;
        var extent = scene.Inner.Extent;
        scene.StartClosing();

        // Shrinking the expansion viewport must not resize the fixed-height inner scroller.
        scene.Inner.Viewport.ShouldBe(viewport);
        scene.Inner.Extent.ShouldBe(extent);
        scene.AssertOverflowAndScrollbars();
        var point = scene.Inner.TranslatePoint(new Point(20, 16), scene.Window)!.Value;
        scene.Actor.TranslatePoint(default, scene.Window)!.Value.Y.ShouldBeLessThan(point.Y);
        point.Y.ShouldBeLessThan(scene.Actor.TranslatePoint(new Point(0, scene.Actor.Bounds.Height), scene.Window)!.Value.Y);

        scene.Wheel(point);

        scene.Inner.Offset.Y.ShouldBeGreaterThan(0, "Wheel input inside the still-visible content must scroll the inner viewport.");
        scene.Outer.Offset.Y.ShouldBe(0, "The outer scroller must not consume a wheel event handled by the inner scroller.");

        // Only boundary setup uses Offset; the transition to the outer scroller is real wheel input.
        scene.Inner.Offset = new Vector(0, scene.Inner.Extent.Height - scene.Inner.Viewport.Height);
        scene.Pump();
        var innerBoundary = scene.Inner.Offset.Y;
        scene.Wheel(point);

        scene.Inner.Offset.Y.ShouldBe(innerBoundary);
        scene.Outer.Offset.Y.ShouldBeGreaterThan(0,
            "At the inner boundary, wheel input must chain to the overflowing outer ScrollViewer.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Animation_Clipping_Rejects_Hidden_Buttons_And_Preserves_Visible_Button_Input(double scaling)
    {
        using var scene = new InputScene(scaling);
        scene.AssertOverflowAndScrollbars();
        var visibleClicks = 0;
        var clippedClicks = 0;
        scene.VisibleButton.Click += (_, _) => visibleClicks++;
        scene.ClippedButton.Click += (_, _) => clippedClicks++;

        // Prove both buttons are reachable under the original fixed-size and scrolling constraints.
        scene.ClickCenter(scene.VisibleButton);
        scene.ClickCenter(scene.ClippedButton);
        visibleClicks.ShouldBe(1);
        clippedClicks.ShouldBe(1);
        visibleClicks = clippedClicks = 0;
        scene.StartClosing();

        var visiblePoint = scene.Center(scene.VisibleButton);
        var clippedPoint = scene.Center(scene.ClippedButton);
        var viewportBottom = scene.Actor.TranslatePoint(new Point(0, scene.Actor.Bounds.Height), scene.Window)!.Value.Y;
        visiblePoint.Y.ShouldBeLessThan(viewportBottom);
        clippedPoint.Y.ShouldBeGreaterThan(viewportBottom,
            "The second button must be outside the animated viewport, rather than merely transparent.");
        clippedPoint.Y.ShouldBeLessThan(scene.Window.ClientSize.Height,
            "The negative hit-test point must remain inside the actual window.");
        scene.ClippedButton.IsEffectivelyVisible.ShouldBeTrue(
            "This regression must exercise ancestor clipping rather than hiding or disabling the button.");
        scene.ClippedButton.IsEffectivelyEnabled.ShouldBeTrue();

        scene.Click(clippedPoint);
        clippedClicks.ShouldBe(0, "A real pointer click beyond the expansion viewport must not reach clipped content.");
        scene.Click(visiblePoint);
        visibleClicks.ShouldBe(1, "The same animation frame must preserve input in the visible region.");
    }

    private sealed class InputScene : IDisposable
    {
        private readonly ManualMotionClock _clock = new();
        private readonly AtomExpander _expander;
        public AvaloniaWindow Window { get; }
        public AvaloniaScrollViewer Inner { get; }
        public AvaloniaScrollViewer Outer { get; }
        public ContentViewport Actor { get; }
        public AtomButton VisibleButton { get; } = new() { Content = "Visible action", Height = 32 };
        public AtomButton ClippedButton { get; } = new() { Content = "Clipped action", Height = 32 };

        public InputScene(double scaling)
        {
            var rows = new StackPanel
            {
                Children =
                {
                    VisibleButton,
                    new Border { Height = 112 },
                    ClippedButton
                }
            };
            for (var i = 0; i < 30; i++)
                rows.Children.Add(new TextBlock { Text = $"Scrollable row {i}", Height = 32 });
            Inner = new AvaloniaScrollViewer
            {
                Height = 220,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                IsScrollChainingEnabled = true,
                Content = rows
            };
            _expander = new AtomExpander
            {
                Header = "Nested scrolling",
                Content = Inner,
                ContentPadding = default(Thickness),
                IsExpanded = true,
                IsMotionEnabled = false,
                MotionDuration = TimeSpan.FromSeconds(1)
            };
            Outer = new AvaloniaScrollViewer
            {
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Content = new StackPanel
                {
                    Children =
                    {
                        new Avalonia.Controls.Grid { Height = 300, Children = { _expander } },
                        new Border { Height = 900 }
                    }
                }
            };
            Window = new AvaloniaWindow { Width = 420, Height = 360, Content = Outer };
            Window.Show();
            Window.SetRenderScaling(scaling);
            Pump();
            Actor = _expander.GetVisualDescendants().OfType<ContentViewport>()
                .First(control => control.Name == "PART_ContentViewport");
            typeof(Animatable).GetProperty("Clock", BindingFlags.Instance | BindingFlags.NonPublic)!
                .SetValue(Actor, _clock.Instance);
        }

        public void StartClosing()
        {
            var fullHeight = Actor.Bounds.Height;
            _expander.IsMotionEnabled = true;
            var header = _expander.GetVisualDescendants().OfType<Control>()
                .First(control => control.Name == "PART_HeaderDecorator");
            ClickCenter(header);
            _expander.IsExpanded.ShouldBeFalse();
            _clock.Step(0);
            Pump();
            _clock.Step(500);
            Pump();
            Actor.IsVisible.ShouldBeTrue();
            Actor.Bounds.Height.ShouldBeGreaterThan(0);
            Actor.Bounds.Height.ShouldBeLessThan(fullHeight);
            Actor.Opacity.ShouldBeGreaterThan(0);
            Actor.Opacity.ShouldBeLessThan(1);
        }

        public void AssertOverflowAndScrollbars()
        {
            Inner.Bounds.Height.ShouldBe(220);
            Inner.Viewport.Height.ShouldBeGreaterThan(0);
            Inner.Extent.Height.ShouldBeGreaterThan(Inner.Viewport.Height + 500);
            Outer.Viewport.Height.ShouldBeGreaterThan(0);
            Outer.Extent.Height.ShouldBeGreaterThan(Outer.Viewport.Height + 500);
            foreach (var viewer in new[] { Inner, Outer })
            {
                var scrollbar = viewer.GetVisualDescendants().OfType<Avalonia.Controls.Primitives.ScrollBar>()
                    .First(bar => bar.Orientation == Orientation.Vertical && ReferenceEquals(bar.TemplatedParent, viewer));
                scrollbar.IsEffectivelyVisible.ShouldBeTrue();
                scrollbar.Bounds.Width.ShouldBeGreaterThan(0);
                scrollbar.Bounds.Height.ShouldBeGreaterThan(0);
                scrollbar.Maximum.ShouldBeGreaterThan(0);
            }
        }

        public Point Center(Control control) => control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2), Window)!.Value;
        public void ClickCenter(Control control) => Click(Center(control));
        public void Click(Point point)
        {
            Window.MouseMove(point);
            Window.MouseDown(point, MouseButton.Left);
            Window.MouseUp(point, MouseButton.Left);
            Pump();
        }
        public void Wheel(Point point)
        {
            Window.MouseMove(point);
            Window.MouseWheel(point, new Vector(0, -1), RawInputModifiers.None);
            Pump();
        }
        public void Pump() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Window.Close(); Pump(); _clock.Dispose(); }
    }

}
