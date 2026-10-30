using System.Reflection;
using AtomUI.Animations;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomCollapse = AtomUI.Desktop.Controls.Collapse;
using AtomExpander = AtomUI.Desktop.Controls.Expander;

namespace AtomUI.Desktop.Controls.Tests.Motion;

public class ContentExpansionMotionTests
{
    static ContentExpansionMotionTests() => AvaloniaTestApp.EnsureInitialized();

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Equal_Height_Collapse_Accordion_Exchanges_Space_Without_Midpoint_Expansion(double scaling)
    {
        using var scene = new Scene(false, scaling);
        scene.SetExpanded(true);
        var stableTop = scene.OccupiedHeight;
        var fullHeight = scene.Actor.Bounds.Height;
        scene.EnableMotion(true);
        scene.Click(1);
        scene.Step(0);
        scene.Step(500);

        scene.IsExpanded().ShouldBeFalse();
        scene.IsExpanded(1).ShouldBeTrue();
        foreach (var actor in new[] { scene.Actor, scene.ContentActor(1) })
        {
            actor.IsVisible.ShouldBeTrue();
            actor.Bounds.Height.ShouldBeGreaterThan(0);
            actor.Bounds.Height.ShouldBeLessThan(fullHeight);
            actor.Opacity.ShouldBeGreaterThan(0);
            actor.Opacity.ShouldBeLessThan(1);
        }
        scene.OccupiedHeight.ShouldBe(stableTop, 1 / scaling,
            "Equal content heights must exchange space on the same curve and clock frame.");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reversal_Preserves_Rendered_Size_Before_The_Next_Tick(bool expander)
    {
        using var scene = new Scene(expander);
        scene.SetExpanded(true);
        var fullHeight = scene.Actor.Bounds.Height;
        var expandedOccupiedHeight = scene.OccupiedHeight;
        scene.EnableMotion(true);
        scene.Click();
        scene.Step(0);
        scene.Step(400);
        var size = scene.Actor.Bounds.Size;
        scene.Click();

        scene.Actor.Bounds.Height.ShouldBe(size.Height, 1,
            "Reversal must capture the visible size before cancelling the previous animation.");
        scene.Step(400);
        scene.Actor.Bounds.Height.ShouldBe(size.Height, 1);

        // The cancelled close would finish at 1000 ms; the reversed open owns 1400 ms.
        scene.Step(1001);
        scene.IsExpanded().ShouldBeTrue();
        scene.Actor.IsVisible.ShouldBeTrue();
        scene.Actor.Bounds.Height.ShouldBeGreaterThan(size.Height);
        scene.Actor.Bounds.Height.ShouldBeLessThan(fullHeight);
        scene.Step(1401);
        scene.Actor.IsVisible.ShouldBeTrue();
        scene.Actor.Bounds.Height.ShouldBe(fullHeight, 1);
        scene.OccupiedHeight.ShouldBe(expandedOccupiedHeight, 1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reversal_Preserves_Opacity_Before_The_Next_Tick(bool expander)
    {
        using var scene = new Scene(expander);
        scene.SetExpanded(true);
        scene.EnableMotion(true);
        scene.Click();
        scene.Step(0);
        scene.Step(400);
        var opacity = scene.Actor.Opacity;
        opacity.ShouldBeGreaterThan(0);
        opacity.ShouldBeLessThan(1);
        scene.Click();

        scene.Actor.Opacity.ShouldBe(opacity, 0.001,
            "Cancelling and reversing must not restore a fixed opacity endpoint.");
        scene.Step(400);
        scene.Actor.Opacity.ShouldBe(opacity, 0.001);

        scene.Step(1001);
        scene.IsExpanded().ShouldBeTrue();
        scene.Actor.IsVisible.ShouldBeTrue();
        scene.Actor.Opacity.ShouldBeGreaterThan(opacity);
        scene.Actor.Opacity.ShouldBeLessThan(1);
        scene.Step(1401);
        scene.Actor.IsVisible.ShouldBeTrue();
        scene.Actor.Opacity.ShouldBe(1);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Disabling_Motion_Immediately_Projects_The_Current_Target(bool expander, bool opening)
    {
        using var scene = new Scene(expander);
        scene.SetExpanded(opening);
        var targetTop = scene.OccupiedHeight;
        scene.SetExpanded(!opening);
        scene.EnableMotion(true);
        scene.Click();
        scene.Step(0);
        scene.Step(400);
        scene.EnableMotion(false);
        scene.Pump();

        scene.Actor.IsVisible.ShouldBe(opening);
        scene.OccupiedHeight.ShouldBe(targetTop, 1,
            "Disabling motion during playback must settle without another state change or clock tick.");
        if (opening)
            scene.Actor.Opacity.ShouldBe(1);
        scene.Step(1600);
        scene.OccupiedHeight.ShouldBe(targetTop, 1);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Final_Tick_Holds_Target_While_Completion_Continuation_Is_Pending(bool expander, bool opening)
    {
        using var scene = new Scene(expander);
        scene.SetExpanded(opening);
        var targetTop = scene.OccupiedHeight;
        scene.SetExpanded(!opening);
        scene.EnableMotion(true);
        scene.Click();
        scene.Step(0);
        scene.Step(990);
        scene.StepBeforeContinuations(1100);

        scene.OccupiedHeight.ShouldBe(targetTop, 1,
            "The clock's final tick must not restore the previous layout before asynchronous cleanup.");
        scene.Actor.Opacity.ShouldBe(opening ? 1 : 0, 0.001);
    }

    [Theory]
    [InlineData(ExpandDirection.Down, 1)]
    [InlineData(ExpandDirection.Up, 1)]
    [InlineData(ExpandDirection.Left, 1)]
    [InlineData(ExpandDirection.Right, 1)]
    [InlineData(ExpandDirection.Left, 2)]
    [InlineData(ExpandDirection.Right, 2)]
    public void Expander_Content_Keeps_Normal_Text_Size_And_Header_Anchored_Separator(
        ExpandDirection direction, double scaling)
    {
        using var scene = new Scene(true, scaling, direction);
        scene.SetExpanded(true);
        var text = scene.Text;
        var normalBounds = RenderedBounds(text, scene.Expander!);
        var normalLines = text.TextLayout.TextLines.Select(line => line.Length).ToArray();
        var border = scene.Actor.GetVisualDescendants().OfType<PixelAlignedBorder>().First();
        var thickness = border.BorderThickness;
        var normalAnchorDistance = AnchorDistance(RenderedBounds(text, scene.Actor), scene.Actor.Bounds.Size, direction);
        var horizontal = direction is ExpandDirection.Left or ExpandDirection.Right;
        var fullLength = horizontal ? scene.Actor.Bounds.Width : scene.Actor.Bounds.Height;
        scene.EnableMotion(true);
        scene.Click();
        scene.Step(0);
        scene.Step(500);

        scene.IsExpanded().ShouldBeFalse();
        scene.Actor.IsVisible.ShouldBeTrue();
        var currentLength = horizontal ? scene.Actor.Bounds.Width : scene.Actor.Bounds.Height;
        currentLength.ShouldBeGreaterThan(0);
        currentLength.ShouldBeLessThan(fullLength);
        scene.Actor.Opacity.ShouldBeGreaterThan(0);
        scene.Actor.Opacity.ShouldBeLessThan(1);
        var current = RenderedBounds(text, scene.Expander!);
        current.Width.ShouldBe(normalBounds.Width, 1 / scaling, "Text must retain normal rendered width while its viewport clips.");
        current.Height.ShouldBe(normalBounds.Height, 1 / scaling, "Text must not be scaled vertically.");
        text.TextLayout.TextLines.Select(line => line.Length).ShouldBe(normalLines, "Horizontal expansion must not rewrap text at the animated viewport width.");
        AnchorDistance(RenderedBounds(text, scene.Actor), scene.Actor.Bounds.Size, direction).ShouldBe(normalAnchorDistance, 1 / scaling);
        border.BorderThickness.ShouldBe(thickness, "The separator must not animate its thickness.");
        var contentBounds = RenderedBounds(border, scene.Expander!);
        var headerBounds = RenderedBounds(scene.Header, scene.Expander!);
        var separation = direction switch
        {
            ExpandDirection.Down => contentBounds.Top - headerBounds.Bottom,
            ExpandDirection.Up => headerBounds.Top - contentBounds.Bottom,
            ExpandDirection.Left => headerBounds.Left - contentBounds.Right,
            _ => contentBounds.Left - headerBounds.Right
        };
        separation.ShouldBe(0, 1 / scaling, "The content separator must remain adjacent to the header.");
    }

    [Theory]
    [InlineData(ExpandDirection.Up, 1)]
    [InlineData(ExpandDirection.Left, 1)]
    [InlineData(ExpandDirection.Up, 2)]
    [InlineData(ExpandDirection.Left, 2)]
    public void Reversing_A_Trailing_Anchored_Viewport_Preserves_Text_Position_Before_The_First_Tick(
        ExpandDirection direction, double scaling)
    {
        foreach (var time in new[] { 200, 300, 400, 600 })
        {
            using var scene = new Scene(true, scaling, direction);
            scene.SetExpanded(true);
            scene.EnableMotion(true);
            scene.Click();
            scene.Step(0);
            scene.Step(time);
            var position = scene.Text.TranslatePoint(default, scene.Expander!)!.Value;
            var size = scene.Actor.Bounds.Size;

            scene.Click();

            scene.Actor.Bounds.Size.ShouldBe(size);
            scene.Text.TranslatePoint(default, scene.Expander!)!.Value.ShouldBe(position,
                $"Reversal at {time} ms must retain the same pixel anchor as the captured viewport.");
        }
    }

    private static double AnchorDistance(Rect text, Size viewport, ExpandDirection direction) => direction switch
    {
        ExpandDirection.Down => text.Top,
        ExpandDirection.Up => viewport.Height - text.Bottom,
        ExpandDirection.Left => viewport.Width - text.Right,
        _ => text.Left
    };

    private static Rect RenderedBounds(Visual visual, Visual relativeTo)
    {
        var start = visual.TranslatePoint(default, relativeTo)!.Value;
        var end = visual.TranslatePoint(new Point(visual.Bounds.Width, visual.Bounds.Height), relativeTo)!.Value;
        return new Rect(start, end).Normalize();
    }

    private sealed class Scene : IDisposable
    {
        private readonly ManualMotionClock _clock = new();
        private readonly Avalonia.Controls.Window _window;
        private readonly AtomCollapse? _collapse;
        private readonly CollapseItem[] _items = [];
        public AtomExpander? Expander { get; }
        public ContentViewport Actor { get; }
        public Control Header { get; }
        public double OccupiedHeight => Expander?.Bounds.Height ?? Footer.Bounds.Top;
        public TextBlock Footer { get; } = new() { Text = "Following content" };
        public TextBlock Text { get; } = new()
        {
            Text = "Wrapped text preserves its normal line layout throughout horizontal and vertical expansion. " +
                   "The next paragraph must remain readable at every intermediate animation frame.",
            TextWrapping = TextWrapping.Wrap
        };

        public Scene(bool expander, double scaling = 1, ExpandDirection direction = ExpandDirection.Down)
        {
            Control subject;
            Control owner;
            if (expander)
            {
                Expander = new AtomExpander
                {
                    Header = "Panel header", Content = Text, IsMotionEnabled = false,
                    ExpandDirection = direction, MotionDuration = TimeSpan.FromSeconds(1)
                };
                owner = Expander;
                // Keep the Gallery's fixed-height host and the template's DockPanel constraints.
                subject = new Avalonia.Controls.Grid { Height = 280, Children = { Expander } };
            }
            else
            {
                _items = Enumerable.Range(0, 2).Select(i => new CollapseItem
                {
                    Header = $"Panel {i}", Content = new Border { Height = 120 },
                    MotionDuration = TimeSpan.FromSeconds(1)
                }).ToArray();
                _collapse = new AtomCollapse { IsAccordion = true, IsMotionEnabled = false };
                foreach (var item in _items)
                    _collapse.Items.Add(item);
                subject = _collapse;
                owner = _items[0];
            }
            _window = new Avalonia.Controls.Window
            {
                Width = 420, Height = 360,
                Content = new Avalonia.Controls.ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                    Content = new StackPanel { Children = { subject, Footer } }
                }
            };
            _window.Show();
            _window.SetRenderScaling(scaling);
            Pump();
            Actor = Part<ContentViewport>(owner, "PART_ContentViewport");
            Header = Part<Control>(owner, expander ? "PART_HeaderLayoutTransform" : "PART_HeaderDecorator");
            var clockProperty = typeof(Animatable).GetProperty("Clock", BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (var actor in _window.GetVisualDescendants().OfType<ContentViewport>())
                clockProperty.SetValue(actor, _clock.Instance);
        }

        public void SetExpanded(bool expanded)
        {
            if (Expander != null) Expander.IsExpanded = expanded;
            else _items[0].IsSelected = expanded;
            Pump();
        }
        public bool IsExpanded(int index = 0) => Expander?.IsExpanded ?? _items[index].IsSelected;
        public ContentViewport ContentActor(int index) =>
            Part<ContentViewport>(_items[index], "PART_ContentViewport");
        public void EnableMotion(bool enabled)
        {
            if (Expander != null) Expander.IsMotionEnabled = enabled;
            else _collapse!.IsMotionEnabled = enabled;
        }
        public void Click(int index = 0)
        {
            var header = Expander != null ? Part<Control>(Expander, "PART_HeaderDecorator") : Part<Control>(_items[index], "PART_HeaderDecorator");
            var point = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), _window)!.Value;
            _window.MouseMove(point);
            _window.MouseDown(point, MouseButton.Left);
            _window.MouseUp(point, MouseButton.Left);
            Pump();
        }
        public void Step(int milliseconds) { _clock.Step(milliseconds); Pump(); }
        public void StepBeforeContinuations(int milliseconds) { _clock.Step(milliseconds); _window.UpdateLayout(); }
        public void Pump() { Dispatcher.UIThread.RunJobs(); _window.UpdateLayout(); }
        public void Dispose() { _window.Close(); Pump(); _clock.Dispose(); }
        private static T Part<T>(Control owner, string name) where T : Control =>
            owner.GetVisualDescendants().OfType<T>().First(control => control.Name == name);
    }

}
