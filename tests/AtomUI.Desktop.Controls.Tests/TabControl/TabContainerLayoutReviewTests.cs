using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaScrollViewer = Avalonia.Controls.ScrollViewer;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabContainerLayoutReviewTests
{
    static TabContainerLayoutReviewTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<bool, Dock> OwnerPlacementCases
    {
        get
        {
            var cases = new TheoryData<bool, Dock>();
            foreach (var strip in new[] { false, true })
            {
                foreach (var placement in new[] { Dock.Top, Dock.Bottom, Dock.Left, Dock.Right })
                {
                    cases.Add(strip, placement);
                }
            }
            return cases;
        }
    }

    // Degenerate arrange uses the same branch for both sides of each axis.
    public static TheoryData<bool, Dock> NarrowArrangeAxisCases => new()
    {
        { false, Dock.Top },
        { true, Dock.Left }
    };

    [Theory]
    [MemberData(nameof(OwnerPlacementCases))]
    public void Centered_Card_Tabs_Reserve_The_Add_Button_Without_Unnecessary_Overflow(bool strip, Dock placement)
    {
        var owner = CreateCardOwner(strip, placement, itemCount: 3);
        owner.Width = 900;
        owner.Height = 600;
        if (owner is CardTabControl tabs)
        {
            tabs.TabAlignmentCenter = true;
        }
        else
        {
            ((CardTabStrip)owner).TabAlignmentCenter = true;
        }

        ShowInWindow(owner, window =>
        {
            var viewer = FindViewer(owner);
            var addButton = FindAddButton(owner);
            var panel = owner.GetVisualDescendants().OfType<TabsContainerPanel>().ShouldHaveSingleItem();
            var horizontal = placement is Dock.Top or Dock.Bottom;

            addButton.IsVisible.ShouldBeTrue();
            (horizontal ? viewer.ScrollBarMaximum.X : viewer.ScrollBarMaximum.Y).ShouldBe(0, 0.5);
            viewer.GetVisualDescendants().OfType<IconButton>()
                  .Single(button => button.Name == "PART_ScrollMenuIndicator").IsVisible.ShouldBeFalse();

            var viewerBounds = BoundsIn(viewer, panel);
            var buttonBounds = BoundsIn(addButton, panel);
            if (horizontal)
            {
                buttonBounds.Left.ShouldBeGreaterThanOrEqualTo(viewerBounds.Right);
                buttonBounds.Right.ShouldBeLessThanOrEqualTo(panel.Bounds.Width + 0.5);
            }
            else
            {
                buttonBounds.Top.ShouldBeGreaterThanOrEqualTo(viewerBounds.Bottom);
                buttonBounds.Bottom.ShouldBeLessThanOrEqualTo(panel.Bounds.Height + 0.5);
            }

            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            var buttonPoint = addButton.TranslatePoint(new Point(addButton.Bounds.Width / 2,
                addButton.Bounds.Height / 2), window).ShouldNotBeNull();
            var hit = window.InputHitTest(buttonPoint) as Visual;
            (ReferenceEquals(hit, addButton) || hit?.GetVisualAncestors().Contains(addButton) == true).ShouldBeTrue();
        });
    }

    [Theory]
    [MemberData(nameof(NarrowArrangeAxisCases))]
    public void Card_Tabs_Tolerate_Arrange_Smaller_Than_The_Measured_Add_Button(bool strip, Dock placement)
    {
        var owner = CreateCardOwner(strip, placement, itemCount: 3);
        owner.Width = double.NaN;
        owner.Height = double.NaN;

        ShowInWindow(owner, _ =>
        {
            var addButton = FindAddButton(owner);
            var horizontal = placement is Dock.Top or Dock.Bottom;
            (horizontal ? addButton.DesiredSize.Width : addButton.DesiredSize.Height).ShouldBeGreaterThan(20);

            owner.Measure(new Size(900, 600));
            Should.NotThrow(() => owner.Arrange(new Rect(0, 0, 20, 20)));

            var panel = owner.GetVisualDescendants().OfType<TabsContainerPanel>().ShouldHaveSingleItem();
            foreach (var child in new Control[] { panel, FindViewer(owner), addButton })
            {
                double.IsFinite(child.Bounds.Width).ShouldBeTrue();
                double.IsFinite(child.Bounds.Height).ShouldBeTrue();
                child.Bounds.Width.ShouldBeGreaterThanOrEqualTo(0);
                child.Bounds.Height.ShouldBeGreaterThanOrEqualTo(0);
                child.Bounds.X.ShouldBeGreaterThanOrEqualTo(0);
                child.Bounds.Y.ShouldBeGreaterThanOrEqualTo(0);
            }
        });
    }

    [Theory]
    [MemberData(nameof(OwnerPlacementCases))]
    public void Line_Tabs_Center_The_Visible_Tab_Group_Along_The_Placement_Axis(bool strip, Dock placement)
    {
        var owner = CreateLineOwner(strip, placement);

        ShowInWindow(owner, _ =>
        {
            var wrapper = owner.GetVisualDescendants().OfType<Panel>()
                               .Single(panel => panel.Name is "PART_AlignWrapper" or "AlignWrapper");
            var first = owner.ContainerFromIndex(0).ShouldNotBeNull();
            var last = owner.ContainerFromIndex(2).ShouldNotBeNull();
            var firstBounds = BoundsIn(first, wrapper);
            var lastBounds = BoundsIn(last, wrapper);

            if (placement is Dock.Top or Dock.Bottom)
            {
                firstBounds.Left.ShouldBeGreaterThan(0);
                lastBounds.Right.ShouldBeLessThan(wrapper.Bounds.Width);
                ((firstBounds.Left + lastBounds.Right) / 2).ShouldBe(wrapper.Bounds.Width / 2, 0.5);
            }
            else
            {
                firstBounds.Top.ShouldBeGreaterThan(0);
                lastBounds.Bottom.ShouldBeLessThan(wrapper.Bounds.Height);
                ((firstBounds.Top + lastBounds.Bottom) / 2).ShouldBe(wrapper.Bounds.Height / 2, 0.5);
            }
        });
    }

    [Theory]
    [InlineData(false, FlowDirection.LeftToRight)]
    [InlineData(false, FlowDirection.RightToLeft)]
    [InlineData(true, FlowDirection.LeftToRight)]
    [InlineData(true, FlowDirection.RightToLeft)]
    public void Horizontal_Wheel_Follows_Flow_Direction_While_Vertical_Wheel_Keeps_Logical_Order(
        bool strip, FlowDirection flowDirection)
    {
        var owner = CreateCardOwner(strip, Dock.Top, itemCount: 12);
        owner.FlowDirection = flowDirection;

        ShowInWindow(owner, window =>
        {
            var viewer = FindViewer(owner);
            var middle = viewer.ScrollBarMaximum.X / 2;
            middle.ShouldBeGreaterThan(50);
            viewer.Offset = new Vector(middle, 0);
            window.UpdateLayout();

            WheelOverPresenter(window, viewer, new Vector(1, 0));
            if (flowDirection == FlowDirection.RightToLeft) viewer.Offset.X.ShouldBeGreaterThan(middle);
            else viewer.Offset.X.ShouldBeLessThan(middle);

            viewer.Offset = new Vector(middle, 0);
            window.UpdateLayout();
            WheelOverPresenter(window, viewer, new Vector(0, -1));
            viewer.Offset.X.ShouldBeGreaterThan(middle);
        });
    }

    [Theory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public void Rtl_Horizontal_Wheel_Chains_Only_At_The_Requested_Boundary(
        bool strip, bool atEnd, bool chaining)
    {
        var owner = CreateCardOwner(strip, Dock.Top, itemCount: 12);
        owner.FlowDirection = FlowDirection.RightToLeft;
        var outer = new AvaloniaScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = new Border { Width = 900, Height = 220, Child = owner }
        };

        ShowInWindow(outer, window =>
        {
            var viewer = FindViewer(owner);
            viewer.IsScrollChainingEnabled = chaining;
            var boundary = atEnd ? viewer.ScrollBarMaximum.X : 0;
            viewer.Offset = new Vector(boundary, 0);
            var outerMiddle = outer.ScrollBarMaximum.X / 2;
            outerMiddle.ShouldBeGreaterThan(50);
            outer.Offset = new Vector(outerMiddle, 0);
            window.UpdateLayout();

            WheelOverPresenter(window, viewer, new Vector(atEnd ? 1 : -1, 0));

            viewer.Offset.X.ShouldBe(boundary, 0.5);
            if (!chaining) outer.Offset.X.ShouldBe(outerMiddle, 0.5);
            else if (atEnd) outer.Offset.X.ShouldBeLessThan(outerMiddle);
            else outer.Offset.X.ShouldBeGreaterThan(outerMiddle);
        }, width: 500, height: 260);
    }

    private static ItemsControl CreateLineOwner(bool strip, Dock placement)
    {
        if (strip)
        {
            var owner = new AtomUI.Desktop.Controls.TabStrip
            {
                Width = 900, Height = 600, TabStripPlacement = placement,
                TabAlignmentCenter = true, IsMotionEnabled = false, SelectedIndex = 0
            };
            for (var index = 0; index < 3; index++)
            {
                owner.Items.Add(new TabStripItem { Content = new Border { Width = 100, Height = 24 } });
            }
            return owner;
        }

        var tabs = new AtomUI.Desktop.Controls.TabControl
        {
            Width = 900, Height = 600, TabStripPlacement = placement,
            TabAlignmentCenter = true, IsMotionEnabled = false, SelectedIndex = 0
        };
        for (var index = 0; index < 3; index++)
        {
            tabs.Items.Add(new AtomUI.Desktop.Controls.TabItem
            {
                Header = new Border { Width = 100, Height = 24 }, Content = $"Content {index}"
            });
        }
        return tabs;
    }

    private static Control CreateCardOwner(bool strip, Dock placement, int itemCount)
    {
        if (strip)
        {
            var owner = new CardTabStrip
            {
                Width = 340, Height = 220, TabStripPlacement = placement,
                IsShowAddTabButton = true, IsMotionEnabled = false, SelectedIndex = 0
            };
            for (var index = 0; index < itemCount; index++)
            {
                owner.Items.Add(new TabStripItem { Content = new Border { Width = 100, Height = 24 } });
            }
            return owner;
        }

        var tabs = new CardTabControl
        {
            Width = 340, Height = 220, TabStripPlacement = placement,
            IsShowAddTabButton = true, IsMotionEnabled = false, SelectedIndex = 0
        };
        for (var index = 0; index < itemCount; index++)
        {
            tabs.Items.Add(new AtomUI.Desktop.Controls.TabItem
            {
                Header = new Border { Width = 100, Height = 24 }, Content = $"Content {index}"
            });
        }
        return tabs;
    }

    private static TabScrollViewer FindViewer(Control owner) =>
        owner.GetVisualDescendants().OfType<TabScrollViewer>().ShouldHaveSingleItem();

    private static IconButton FindAddButton(Control owner) =>
        owner.GetVisualDescendants().OfType<IconButton>().Single(button => button.Name == "PART_AddTabButton");

    private static Rect BoundsIn(Control control, Visual relativeTo) =>
        new Rect(control.Bounds.Size).TransformToAABB(control.TransformToVisual(relativeTo).ShouldNotBeNull());

    private static void WheelOverPresenter(AvaloniaWindow window, TabScrollViewer viewer, Vector delta)
    {
        var presenter = viewer.GetVisualDescendants().OfType<TabScrollContentPresenter>().ShouldHaveSingleItem();
        var point = presenter.TranslatePoint(new Point(presenter.Bounds.Width / 2, presenter.Bounds.Height / 2),
            window).ShouldNotBeNull();
        window.MouseMove(point);
        window.MouseWheel(point, delta, RawInputModifiers.None);
        Dispatcher.UIThread.RunJobs();
        window.UpdateLayout();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion,
        double width = 900, double height = 600)
    {
        var window = new AvaloniaWindow { Width = width, Height = height, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            assertion(window);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
