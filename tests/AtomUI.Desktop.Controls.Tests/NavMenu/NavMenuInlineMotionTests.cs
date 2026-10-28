using System.Reflection;
using AtomUI.Controls.Primitives;
using AtomUI.Desktop.Controls.Tests.Motion;
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

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuInlineMotionTests
{
    static NavMenuInlineMotionTests() => AvaloniaTestApp.EnsureInitialized();

    [Fact]
    public void Accordion_Closes_Previous_Branch_When_The_Next_Branch_Starts_Opening()
    {
        using var scene = new MenuScene();
        scene.OpenWithoutMotion(0);
        scene.EnableMotion();

        scene.ClickHeader(1);

        scene.Items[0].IsSubMenuOpen.ShouldBeFalse(
            "Accordion exclusivity must be applied before waiting for the new branch's animation.");
        scene.Items[1].IsSubMenuOpen.ShouldBeTrue();
        scene.Actors[0].IsVisible.ShouldBeTrue("The old branch remains visible while it collapses.");
    }

    [Fact]
    public void Opening_And_Closing_Animate_The_Full_Height_Without_Jumping_The_Following_Content()
    {
        using var scene = new MenuScene();
        var collapsedTop = scene.Footer.Bounds.Top;
        scene.OpenWithoutMotion(0);
        var expandedTop = scene.Footer.Bounds.Top;
        scene.Items[0].IsSubMenuOpen = false;
        scene.Pump();
        scene.EnableMotion();

        scene.ClickHeader(0);
        scene.Step(0);
        scene.Footer.Bounds.Top.ShouldBe(collapsedTop, 1,
            "Opening must begin at zero height, without reserving 80% of the content immediately.");
        scene.Step(500);
        scene.Footer.Bounds.Top.ShouldBeGreaterThan(collapsedTop + 1);
        scene.Footer.Bounds.Top.ShouldBeLessThan(expandedTop - 1);
        scene.Step(1100);
        scene.Footer.Bounds.Top.ShouldBe(expandedTop, 1);

        scene.ClickHeader(0);
        scene.Step(1100);
        scene.Step(1600);
        scene.Footer.Bounds.Top.ShouldBeLessThan(expandedTop - (expandedTop - collapsedTop) * 0.25,
            "Closing must traverse the full height instead of stopping at 80% and then hiding.");
        scene.Step(2200);
        scene.Footer.Bounds.Top.ShouldBe(collapsedTop, 1);
        scene.Actors[0].IsVisible.ShouldBeFalse();
    }

    [Fact]
    public void Switching_Equal_Height_Accordion_Branches_Keeps_The_Footer_Stationary()
    {
        using var scene = new MenuScene();
        scene.OpenWithoutMotion(0);
        var footerTop = scene.Footer.Bounds.Top;
        scene.EnableMotion();

        scene.ClickHeader(1);
        for (var milliseconds = 0; milliseconds <= 1200; milliseconds += 100)
        {
            scene.Step(milliseconds);
            scene.Footer.Bounds.Top.ShouldBe(footerTop, 1,
                $"Equal-height branches must exchange space concurrently (frame {milliseconds} ms).");
        }

        scene.Actors[0].IsVisible.ShouldBeFalse();
        scene.Actors[1].IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Reversing_A_Closing_Branch_Uses_Its_Current_Height_And_Honors_The_Latest_Click()
    {
        using var scene = new MenuScene();
        scene.OpenWithoutMotion(0);
        var expandedTop = scene.Footer.Bounds.Top;
        scene.EnableMotion();
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(400);
        var interruptedTop = scene.Footer.Bounds.Top;
        interruptedTop.ShouldBeLessThan(expandedTop);

        scene.ClickHeader(0);

        scene.Items[0].IsSubMenuOpen.ShouldBeTrue("A click during collapse must reverse the motion.");
        scene.Footer.Bounds.Top.ShouldBe(interruptedTop, 1, "Reversal must also preserve layout before its first clock tick.");
        scene.Step(400);
        scene.Footer.Bounds.Top.ShouldBe(interruptedTop, 1, "Reversal must resume from the rendered height.");
        scene.Step(1600);
        scene.Items[0].IsSubMenuOpen.ShouldBeTrue();
        scene.Actors[0].IsVisible.ShouldBeTrue();
        scene.Actors[0].Opacity.ShouldBe(1);
        scene.Footer.Bounds.Top.ShouldBe(expandedTop, 1);
    }

    [Fact]
    public void Rapid_Accordion_Switches_Preserve_The_Current_Layout_And_Last_Requested_Branch()
    {
        using var scene = new MenuScene();
        scene.OpenWithoutMotion(0);
        var stableTop = scene.Footer.Bounds.Top;
        scene.EnableMotion();
        scene.ClickHeader(1);
        scene.Step(0);
        scene.Step(200);
        var interruptedTop = scene.Footer.Bounds.Top;

        scene.ClickHeader(2);
        scene.Step(200);

        scene.Footer.Bounds.Top.ShouldBe(interruptedTop, 1);
        scene.Items.Select(item => item.IsSubMenuOpen).ShouldBe([false, false, true]);
        scene.Step(1500);
        scene.Actors.Select(actor => actor.IsVisible).ShouldBe([false, false, true]);
        scene.Footer.Bounds.Top.ShouldBe(stableTop, 1);
    }

    [Fact]
    public void Cancelled_Close_Does_Not_Raise_A_Stale_Closed_Event()
    {
        using var scene = new MenuScene();
        scene.OpenWithoutMotion(0);
        scene.EnableMotion();
        var closed = 0;
        scene.Items[0].SubmenuClosed += (_, _) => closed++;
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(300);
        scene.ClickHeader(0);
        scene.Step(300);
        scene.Step(1500);

        closed.ShouldBe(0);
        scene.Items[0].IsSubMenuOpen.ShouldBeTrue();
        scene.Actors[0].IsVisible.ShouldBeTrue();
    }

    [Fact]
    public void Disabling_Motion_During_Collapse_Immediately_Applies_The_Closed_State()
    {
        using var scene = new MenuScene();
        var collapsedTop = scene.Footer.Bounds.Top;
        scene.OpenWithoutMotion(0);
        scene.EnableMotion();
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(300);

        scene.DisableMotion();
        scene.Pump();

        scene.Actors[0].IsVisible.ShouldBeFalse();
        double.IsNaN(scene.Actors[0].Height).ShouldBeTrue();
        scene.Footer.Bounds.Top.ShouldBe(collapsedTop, 1);
        scene.Step(1500);
        scene.Footer.Bounds.Top.ShouldBe(collapsedTop, 1);
    }

    [Fact]
    public void Detaching_During_Motion_Clears_Animated_Height_And_Reattaches_In_The_Requested_State()
    {
        using var scene = new MenuScene();
        scene.EnableMotion();
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(300);
        var actor = scene.Actors[0];

        scene.Detach();

        double.IsNaN(actor.Height).ShouldBeTrue();
        scene.Step(1500);
        scene.Reattach();
        scene.Items[0].IsSubMenuOpen.ShouldBeTrue();
        actor.IsVisible.ShouldBeTrue();
        actor.Opacity.ShouldBe(1);
        double.IsNaN(actor.Height).ShouldBeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Opening_Does_Not_Reserve_Expanded_Height_Before_The_First_Animation_Tick(double renderScaling)
    {
        using var scene = new MenuScene(renderScaling);
        var collapsedTop = scene.Footer.Bounds.Top;
        scene.EnableMotion();

        scene.ClickHeader(0);

        scene.Footer.Bounds.Top.ShouldBe(collapsedTop, 1,
            "The render loop can draw before the animation clock's first tick.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Closing_Keeps_Expanded_Height_Before_The_First_Animation_Tick(double renderScaling)
    {
        using var scene = new MenuScene(renderScaling);
        scene.OpenWithoutMotion(0);
        var expandedTop = scene.Footer.Bounds.Top;
        scene.EnableMotion();

        scene.ClickHeader(0);

        scene.Footer.Bounds.Top.ShouldBe(expandedTop, 1,
            "Collapse must not snap shut while waiting for the first clock tick.");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Opening_Holds_Its_Final_Frame_Before_Completion_Continuations_Run(double renderScaling)
    {
        using var scene = new MenuScene(renderScaling);
        scene.OpenWithoutMotion(0);
        var expandedTop = scene.Footer.Bounds.Top;
        scene.Items[0].IsSubMenuOpen = false;
        scene.Pump();
        scene.EnableMotion();
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(990);

        scene.StepBeforeContinuations(1100);

        scene.Actors[0].Opacity.ShouldBe(1);
        scene.Footer.Bounds.Top.ShouldBe(expandedTop, 1,
            "The final frame must remain intact while asynchronous completion is pending.");
        scene.Pump();
        scene.Actors[0].Opacity.ShouldBe(1);
        scene.Footer.Bounds.Top.ShouldBe(expandedTop, 1);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Closing_Reaches_Zero_Height_Before_Completion_Continuations_Run(double renderScaling)
    {
        using var scene = new MenuScene(renderScaling);
        var collapsedTop = scene.Footer.Bounds.Top;
        scene.OpenWithoutMotion(0);
        scene.EnableMotion();
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(990);

        scene.StepBeforeContinuations(1100);

        scene.Footer.Bounds.Top.ShouldBe(collapsedTop, 1,
            "The final clock tick must not restore expanded height or external margin before cleanup runs.");
        scene.Actors[0].Opacity.ShouldBe(0);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Reversing_Motion_Preserves_Opacity_Before_The_Next_Animation_Tick(double renderScaling)
    {
        using var scene = new MenuScene(renderScaling);
        scene.OpenWithoutMotion(0);
        scene.EnableMotion();
        scene.ClickHeader(0);
        scene.Step(0);
        scene.Step(400);
        var opacity = scene.Actors[0].Opacity;

        scene.ClickHeader(0);

        scene.Actors[0].Opacity.ShouldBe(opacity, 0.001);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void Child_Text_Keeps_Its_Position_Within_The_Clipped_Content_During_Motion(double renderScaling)
    {
        using var scene = new MenuScene(renderScaling);
        scene.OpenWithoutMotion(0);
        var actor = scene.Actors[0];
        var text = actor.GetVisualDescendants().OfType<TextBlock>().First(block => block.Text == "Option 1");
        var expandedPosition = text.TranslatePoint(default, actor)!.Value;
        var expandedSize = text.Bounds.Size;
        scene.EnableMotion();
        scene.ClickHeader(0);
        for (var milliseconds = 0; milliseconds < 1000; milliseconds += 16)
        {
            scene.Step(milliseconds);
            text.TranslatePoint(default, actor)!.Value.ShouldBe(expandedPosition,
                $"The text origin must stay fixed while only its viewport collapses ({milliseconds} ms).");
            text.Bounds.Size.ShouldBe(expandedSize);
        }
    }

    [Fact]
    public void Queued_Same_Value_Request_Cannot_Report_A_Superseded_Completion()
    {
        using var scene = new MenuScene();
        var opened = 0;
        scene.Items[0].SubmenuOpened += (_, _) => opened++;
        scene.Items[0].IsSubMenuOpen = true;
        scene.Items[0].IsSubMenuOpen = false;
        scene.Items[0].IsSubMenuOpen = true;
        scene.Pump();

        opened.ShouldBe(1);
        scene.Items[0].IsSubMenuOpen.ShouldBeTrue();
    }

    [Fact]
    public void Detached_Queued_Request_Cannot_Report_Completion()
    {
        using var scene = new MenuScene();
        var opened = 0;
        scene.Items[0].SubmenuOpened += (_, _) => opened++;
        scene.Items[0].IsSubMenuOpen = true;
        scene.Detach();
        opened.ShouldBe(0);
        scene.Reattach();
        scene.Actors[0].IsVisible.ShouldBe(scene.Items[0].IsSubMenuOpen);
    }

    private sealed class MenuScene : IDisposable
    {
        private readonly ManualMotionClock _clock = new();
        private readonly Avalonia.Controls.Window _window;
        private readonly AtomUI.Desktop.Controls.NavMenu _menu;
        private object? _detachedContent;

        public NavMenuItem[] Items { get; }
        public ContentViewport[] Actors { get; }
        public TextBlock Footer { get; } = new() { Text = "Accordion mode" };

        public MenuScene(double renderScaling = 1)
        {
            _menu = new AtomUI.Desktop.Controls.NavMenu
            {
                Mode = NavMenuMode.Inline,
                IsAccordionMode = true,
                IsMotionEnabled = false,
                Width = 300,
                Margin = new Thickness(0, 0, 0, 20),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            for (var i = 0; i < 3; i++)
            {
                var node = new NavMenuNode { Header = $"Navigation {i + 1}" };
                node.Children.Add(new NavMenuNode { Header = $"Option {i * 2 + 1}" });
                node.Children.Add(new NavMenuNode { Header = $"Option {i * 2 + 2}" });
                _menu.Items.Add(node);
            }

            _window = new Avalonia.Controls.Window
            {
                Width = 480,
                Height = 400,
                Content = new Avalonia.Controls.ScrollViewer
                {
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = new StackPanel { Children = { _menu, Footer } }
                }
            };
            _window.Show();
            _window.SetRenderScaling(renderScaling);
            Pump();
            Items = _menu.Items.Cast<NavMenuNode>()
                         .Select(node => (NavMenuItem)_menu.ContainerFromItem(node)!).ToArray();
            Actors = Items.Select(item => item.GetVisualDescendants().OfType<ContentViewport>()
                                               .First(actor => actor.Name == "PART_ChildItemsViewport")).ToArray();

        }

        public void OpenWithoutMotion(int index)
        {
            Items[index].Open();
            Pump();
        }

        public void EnableMotion()
        {
            _menu.IsMotionEnabled = true;
            foreach (var item in Items)
            {
                item.OpenCloseMotionDuration = TimeSpan.FromSeconds(1);
            }
        }

        public void DisableMotion() => _menu.IsMotionEnabled = false;

        public void Detach()
        {
            _detachedContent = _window.Content;
            _window.Content = null;
            Pump();
        }

        public void Reattach()
        {
            _window.Content = _detachedContent;
            _detachedContent = null;
            Pump();
        }

        public void ClickHeader(int index)
        {
            var header = Items[index].GetVisualDescendants().OfType<Control>().First(control => control.Name == "PART_Header");
            var point = header.TranslatePoint(new Point(header.Bounds.Width / 2, header.Bounds.Height / 2), _window)!.Value;
            _window.MouseMove(point);
            _window.MouseDown(point, MouseButton.Left);
            _window.MouseUp(point, MouseButton.Left);
            Pump();
        }

        public void Step(int milliseconds)
        {
            _clock.Step(milliseconds);
            Pump();
        }

        public void StepBeforeContinuations(int milliseconds)
        {
            _clock.Step(milliseconds);
            _window.UpdateLayout();
        }

        public void Pump()
        {
            Dispatcher.UIThread.RunJobs();
            _window.UpdateLayout();
        }

        public void Dispose()
        {
            _window.Close();
            Pump();
            _clock.Dispose();
        }
    }

}
