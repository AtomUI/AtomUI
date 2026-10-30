using Avalonia;
using Avalonia.Animation;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomCardTabControl = AtomUI.Desktop.Controls.CardTabControl;
using AtomTabControl = AtomUI.Desktop.Controls.TabControl;
using AtomTabItem = AtomUI.Desktop.Controls.TabItem;
using AtomTabStrip = AtomUI.Desktop.Controls.TabStrip;
using AtomTabStripItem = AtomUI.Desktop.Controls.TabStripItem;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabReorderTests
{
    static TabReorderTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void TabControl_Drag_Reorders_Writable_ItemsSource_And_Keeps_Selected_Item()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var events = new List<TabReorderedEventArgs>();
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source,
            SelectedItem        = second
        };
        tabControl.TabReordered += (_, args) => events.Add(args);

        ShowInWindow(tabControl, window =>
        {
            DragContainerAfter(window, tabControl, 1, 2);

            source.ShouldBe([first, third, second]);
            tabControl.SelectedItem.ShouldBeSameAs(second);
            tabControl.SelectedIndex.ShouldBe(2);
            events.Count.ShouldBe(1);
            events[0].Item.ShouldBeSameAs(second);
            events[0].OldIndex.ShouldBe(1);
            events[0].NewIndex.ShouldBe(2);
        });
    }

    [Fact]
    public void TabControl_Drag_Near_Overflow_Edge_AutoScrolls_Header_Viewport()
    {
        var tabControl = new AtomTabControl
        {
            Width               = 180,
            Height              = 140,
            IsTabReorderEnabled = true
        };
        for (var i = 0; i < 12; i++)
        {
            tabControl.Items.Add(new AtomTabItem
            {
                Header  = $"Tab {i + 1}",
                Content = $"Content {i + 1}"
            });
        }

        ShowInWindow(tabControl, window =>
        {
            var scrollViewer = GetVisualDescendant<TabScrollViewer>(tabControl);
            RunJobsUntil(() => scrollViewer.Extent.Width > scrollViewer.Viewport.Width);
            scrollViewer.Offset.X.ShouldBe(0);

            var source = GetContainer<AtomTabItem>(tabControl, 0);
            DragContainerToScrollEndEdge(window, source, scrollViewer);

            scrollViewer.Offset.X.ShouldBeGreaterThan(0);
        });
    }

    [Fact]
    public void TabControl_Drag_Keeps_Dragged_Tab_Anchored_When_Header_Viewport_Scrolls()
    {
        var tabControl = new AtomTabControl
        {
            Width               = 180,
            Height              = 140,
            IsTabReorderEnabled = true
        };
        for (var i = 0; i < 12; i++)
        {
            tabControl.Items.Add(new AtomTabItem
            {
                Header  = $"Tab {i + 1}",
                Content = $"Content {i + 1}"
            });
        }

        ShowInWindow(tabControl, window =>
        {
            var scrollViewer = GetVisualDescendant<TabScrollViewer>(tabControl);
            RunJobsUntil(() => scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var dragPoint = BeginDragOnTrack(window, tabControl.TabStripPlacement, dragged);
            var beforeOffset = scrollViewer.Offset.X;
            var beforeTranslate = GetTranslateX(dragged);

            scrollViewer.Offset = new Vector(beforeOffset + 24, scrollViewer.Offset.Y);
            Dispatcher.UIThread.RunJobs();
            var offsetDelta = scrollViewer.Offset.X - beforeOffset;
            offsetDelta.ShouldBeGreaterThan(0);

            GetTranslateX(dragged).ShouldBe(
                beforeTranslate + offsetDelta,
                1.0,
                "dragged tab preview must compensate header viewport scroll without waiting for another pointer move");

            ReleasePointer(window, dragPoint);
        });
    }

    [Fact]
    public void TabControl_Drag_Applies_Chrome_Like_Live_Reorder_Preview_Before_Release()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 1);
            var shifted = GetContainer<AtomTabItem>(tabControl, 2);
            dragged.IsMotionEnabled = false;
            Dispatcher.UIThread.RunJobs();

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            GetTranslateX(dragged).ShouldBeGreaterThan(0);
            GetTranslateY(dragged).ShouldBe(0, "horizontal tab drag must stay on the tab track");
            WaitForTranslateXLessThan(shifted, 0);
            WaitForOpaqueBackground(dragged);
            IsEffectivelyTransparent(dragged.Background).ShouldBeFalse("dragged tab must keep an opaque surface while it overlaps sibling tabs");
            source.ShouldBe(
                [first, second, third],
                "live preview should not commit the logical source before pointer release");

            ReleasePointer(window, releasePoint);
            source.ShouldBe(
                [first, third, second],
                "pointer release should commit the previewed target exactly once");
            GetTranslateX(dragged).ShouldBe(0);
            GetTranslateX(shifted).ShouldBe(0);
        });
    }

    [Fact]
    public void TabControl_Drag_Shifts_Sibling_To_Actual_Line_Tab_Layout_Slot()
    {
        var source = new AvaloniaList<TabItemData>
        {
            new() { Header = "first" },
            new() { Header = "second" },
            new() { Header = "third" }
        };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 1);
            var shifted = GetContainer<AtomTabItem>(tabControl, 2);
            var expectedShift = TranslateToWindow(dragged, default, window).X -
                                TranslateToWindow(shifted, default, window).X;

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            WaitForTranslateXCloseTo(shifted, expectedShift);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void CardTabControl_Drag_Shifts_Sibling_To_Actual_Card_Tab_Layout_Slot()
    {
        var source = new AvaloniaList<TabItemData>
        {
            new() { Header = "first" },
            new() { Header = "second" },
            new() { Header = "third" }
        };
        var tabControl = new AtomCardTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 1);
            var shifted = GetContainer<AtomTabItem>(tabControl, 2);
            var expectedShift = TranslateToWindow(dragged, default, window).X -
                                TranslateToWindow(shifted, default, window).X;

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            WaitForTranslateXCloseTo(shifted, expectedShift);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void TabControl_Drag_Uses_Half_Threshold_For_Sibling_Shift_And_Commit()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var target  = GetContainer<AtomTabItem>(tabControl, 1);

            var beforeHalf = DragContainerToPreviewNearTargetHalf(
                window,
                tabControl.TabStripPlacement,
                dragged,
                target,
                crossesHalf: false);

            GetTranslateX(target).ShouldBe(
                0,
                "sibling tabs must not move before the dragged tab crosses the covered tab half-width threshold");
            source.ShouldBe([first, second, third]);

            ReleasePointer(window, beforeHalf);
            source.ShouldBe(
                [first, second, third],
                "partial sibling displacement before the half-overlap threshold must not commit a reorder");
            GetTranslateX(target).ShouldBe(0);

            var draggedAgain = GetContainer<AtomTabItem>(tabControl, 0);
            var targetAgain  = GetContainer<AtomTabItem>(tabControl, 1);
            var afterHalf = DragContainerToPreviewNearTargetHalf(
                window,
                tabControl.TabStripPlacement,
                draggedAgain,
                targetAgain,
                crossesHalf: true);
            GetTranslateX(draggedAgain).ShouldBeGreaterThan(0);
            WaitForTranslateXLessThan(targetAgain, 0);

            ReleasePointer(window, afterHalf);
            source.ShouldBe([second, first, third]);
        });
    }

    [Fact]
    public void TabControl_Drag_Animates_Sibling_Live_Reorder_Transform_And_Restores_Transitions()
    {
        var source = new AvaloniaList<TabItemData>
        {
            new() { Header = "first" },
            new() { Header = "second" },
            new() { Header = "third" }
        };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var shifted = GetContainer<AtomTabItem>(tabControl, 1);
            var originalTransform = shifted.RenderTransform;

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            var previewTransform = shifted.RenderTransform.ShouldBeOfType<TranslateTransform>();
            previewTransform.Transitions.ShouldNotBeNull("sibling displacement should use short transform axis transitions instead of jumping instantly");
            previewTransform.Transitions!.Any(IsTranslateTransformTransition).ShouldBeTrue();

            ReleasePointer(window, releasePoint);

            ReferenceEquals(shifted.RenderTransform, originalTransform)
                .ShouldBeTrue("temporary reorder transforms and their transitions must be removed after the drag session");
        });
    }

    [Fact]
    public void TabControl_Drag_Animates_Sibling_Return_To_Original_Position_During_Live_Reorder()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var shifted = GetContainer<AtomTabItem>(tabControl, 1);

            DragContainerToPreviewNearTargetHalf(
                window,
                tabControl.TabStripPlacement,
                dragged,
                shifted,
                crossesHalf: true);
            WaitForTranslateXLessThan(shifted, 0);

            var beforeHalf = DragPointerNearTargetHalf(
                window,
                tabControl.TabStripPlacement,
                dragged,
                shifted,
                crossesHalf: false);

            var previewTransform = shifted.RenderTransform.ShouldBeOfType<TranslateTransform>();
            previewTransform.Transitions.ShouldNotBeNull("returning sibling tab should keep transform transitions during live reorder");
            previewTransform.Transitions!.Any(IsTranslateTransformTransition).ShouldBeTrue();

            ReleasePointer(window, beforeHalf);
        });
    }

    [Fact]
    public void TabControl_Selected_Indicator_Follows_Dragged_Selected_Tab_During_Live_Reorder()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            IsMotionEnabled     = false,
            ItemsSource         = source,
            SelectedItem        = first
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var shifted = GetContainer<AtomTabItem>(tabControl, 1);
            var beforeIndicatorX = GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform);

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform)
                .ShouldBe(beforeIndicatorX + GetTranslateX(dragged), 0.5);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void TabControl_Selected_Indicator_Does_Not_Lag_When_Dragging_Selected_Tab_With_Motion_Enabled()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            IsMotionEnabled     = true,
            ItemsSource         = source,
            SelectedItem        = first
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var shifted = GetContainer<AtomTabItem>(tabControl, 1);
            var beforeIndicatorX = GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform);

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform)
                .ShouldBe(beforeIndicatorX + GetTranslateX(dragged), 0.5);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void TabControl_Selected_Indicator_Does_Not_Jump_To_Stale_Layout_When_Releasing_Selected_Tab_Reorder()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            IsMotionEnabled     = true,
            ItemsSource         = source,
            SelectedItem        = second
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 1);
            var shifted = GetContainer<AtomTabItem>(tabControl, 2);

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            window.MouseUp(releasePoint, MouseButton.Left);

            var releaseIndicatorX = GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform);

            Dispatcher.UIThread.RunJobs();
            WaitForSelectedIndicatorXCloseTo(tabControl, releaseIndicatorX);
        });
    }

    [Fact]
    public void TabStrip_Selected_Indicator_Does_Not_Lag_When_Dragging_Selected_Tab_With_Motion_Enabled()
    {
        var first  = new AtomTabStripItem { Content = "first" };
        var second = new AtomTabStripItem { Content = "second" };
        var third  = new AtomTabStripItem { Content = "third" };
        var tabStrip = new AtomTabStrip
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            IsMotionEnabled     = true
        };
        tabStrip.Items.Add(first);
        tabStrip.Items.Add(second);
        tabStrip.Items.Add(third);
        tabStrip.SelectedItem = first;

        ShowInWindow(tabStrip, window =>
        {
            var dragged = GetContainer<AtomTabStripItem>(tabStrip, 0);
            var shifted = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var beforeIndicatorX = GetTransformOffsetX(tabStrip.SelectedIndicatorRenderTransform);

            var releasePoint = DragContainerToPreviewAfter(window, tabStrip.TabStripPlacement, dragged, shifted);

            GetTransformOffsetX(tabStrip.SelectedIndicatorRenderTransform)
                .ShouldBe(beforeIndicatorX + GetTranslateX(dragged), 0.5);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void TabStrip_Selected_Indicator_Does_Not_Jump_To_Stale_Layout_When_Releasing_Selected_Tab_Reorder()
    {
        var first  = new AtomTabStripItem { Content = "first" };
        var second = new AtomTabStripItem { Content = "second" };
        var third  = new AtomTabStripItem { Content = "third" };
        var tabStrip = new AtomTabStrip
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            IsMotionEnabled     = true
        };
        tabStrip.Items.Add(first);
        tabStrip.Items.Add(second);
        tabStrip.Items.Add(third);
        tabStrip.SelectedItem = second;

        ShowInWindow(tabStrip, window =>
        {
            var dragged = GetContainer<AtomTabStripItem>(tabStrip, 1);
            var shifted = GetContainer<AtomTabStripItem>(tabStrip, 2);

            var releasePoint = DragContainerToPreviewAfter(window, tabStrip.TabStripPlacement, dragged, shifted);

            window.MouseUp(releasePoint, MouseButton.Left);

            var releaseIndicatorX = GetTransformOffsetX(tabStrip.SelectedIndicatorRenderTransform);

            Dispatcher.UIThread.RunJobs();
            WaitForSelectedIndicatorXCloseTo(tabStrip, releaseIndicatorX);
        });
    }

    [Fact]
    public void TabControl_Selected_Indicator_Follows_Shifted_Selected_Tab_During_Live_Reorder()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new AvaloniaList<TabItemData> { first, second, third };
        var tabControl = new AtomTabControl
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            IsMotionEnabled     = false,
            ItemsSource         = source,
            SelectedItem        = second
        };

        ShowInWindow(tabControl, window =>
        {
            var dragged = GetContainer<AtomTabItem>(tabControl, 0);
            var shifted = GetContainer<AtomTabItem>(tabControl, 1);
            var beforeIndicatorX = GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform);
            var expectedShift = TranslateToWindow(dragged, default, window).X -
                                TranslateToWindow(shifted, default, window).X;

            var releasePoint = DragContainerToPreviewAfter(window, tabControl.TabStripPlacement, dragged, shifted);

            GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform)
                .ShouldBe(beforeIndicatorX + expectedShift, 0.5);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void TabStrip_ReadOnly_ItemsSource_Drag_Cleans_Up_Without_Reordered_Event()
    {
        var first  = new TabItemData { Header = "first" };
        var second = new TabItemData { Header = "second" };
        var third  = new TabItemData { Header = "third" };
        var source = new[] { first, second, third };
        var reorderedCount = 0;
        var tabStrip = new AtomTabStrip
        {
            Width               = 360,
            IsTabReorderEnabled = true,
            ItemsSource         = source
        };
        tabStrip.TabReordered += (_, _) => reorderedCount++;

        ShowInWindow(tabStrip, window =>
        {
            DragContainerAfter(window, tabStrip, 1, 2);

            source.ShouldBe([first, second, third]);
            reorderedCount.ShouldBe(0);
        });
    }

    [Fact]
    public void TabStrip_Left_Placement_Drag_Uses_Vertical_Axis()
    {
        var first  = new AtomTabStripItem { Content = "first" };
        var second = new AtomTabStripItem { Content = "second" };
        var third  = new AtomTabStripItem { Content = "third" };
        var tabStrip = new AtomTabStrip
        {
            Width               = 160,
            Height              = 220,
            IsTabReorderEnabled = true,
            TabStripPlacement   = Dock.Left
        };
        tabStrip.Items.Add(first);
        tabStrip.Items.Add(second);
        tabStrip.Items.Add(third);
        tabStrip.SelectedItem = second;

        ShowInWindow(tabStrip, window =>
        {
            DragContainerAfter(window, tabStrip, 0, 2);

            tabStrip.Items.Cast<object?>().ShouldBe([second, third, first]);
            tabStrip.SelectedItem.ShouldBeSameAs(second);
            tabStrip.SelectedIndex.ShouldBe(0);
        });
    }

    [Fact]
    public void TabStrip_Left_Placement_Drag_Applies_Vertical_Live_Reorder_Preview_Before_Release()
    {
        var first  = new AtomTabStripItem { Content = "first" };
        var second = new AtomTabStripItem { Content = "second" };
        var third  = new AtomTabStripItem { Content = "third" };
        var tabStrip = new AtomTabStrip
        {
            Width               = 160,
            Height              = 220,
            IsTabReorderEnabled = true,
            TabStripPlacement   = Dock.Left
        };
        tabStrip.Items.Add(first);
        tabStrip.Items.Add(second);
        tabStrip.Items.Add(third);

        ShowInWindow(tabStrip, window =>
        {
            var dragged = GetContainer<AtomTabStripItem>(tabStrip, 0);
            var shifted = GetContainer<AtomTabStripItem>(tabStrip, 1);

            var releasePoint = DragContainerToPreviewAfter(window, tabStrip.TabStripPlacement, dragged, shifted);

            GetTranslateX(dragged).ShouldBe(0, "vertical tab drag must stay on the tab track");
            GetTranslateY(dragged).ShouldBeGreaterThan(0);
            WaitForTranslateYLessThan(shifted, 0);
            tabStrip.Items.Cast<object?>().ShouldBe(
                [first, second, third],
                "live preview should not commit the logical source before pointer release");

            ReleasePointer(window, releasePoint);
            tabStrip.Items.Cast<object?>().ShouldBe(
                [second, first, third],
                "pointer release should commit the previewed target exactly once");
            GetTranslateY(dragged).ShouldBe(0);
            GetTranslateY(shifted).ShouldBe(0);
        });
    }

    [Fact]
    public void TabStrip_Left_Placement_Drag_Ignores_Secondary_Axis_Pointer_Offset()
    {
        var first  = new AtomTabStripItem { Content = "first" };
        var second = new AtomTabStripItem { Content = "second" };
        var third  = new AtomTabStripItem { Content = "third" };
        var tabStrip = new AtomTabStrip
        {
            Width               = 220,
            Height              = 220,
            IsTabReorderEnabled = true,
            TabStripPlacement   = Dock.Left
        };
        tabStrip.Items.Add(first);
        tabStrip.Items.Add(second);
        tabStrip.Items.Add(third);

        ShowInWindow(tabStrip, window =>
        {
            var dragged = GetContainer<AtomTabStripItem>(tabStrip, 0);
            var shifted = GetContainer<AtomTabStripItem>(tabStrip, 1);

            var releasePoint = DragContainerToPreviewAfter(
                window,
                tabStrip.TabStripPlacement,
                dragged,
                shifted,
                secondaryAxisOffset: 48);

            GetTranslateX(dragged).ShouldBe(0, "vertical tab drag must ignore horizontal pointer drift");
            GetTranslateY(dragged).ShouldBeGreaterThan(0);
            WaitForTranslateYLessThan(shifted, 0);

            ReleasePointer(window, releasePoint);
        });
    }

    [Fact]
    public void TabStrip_Left_Placement_Drag_Keeps_Dragged_Tab_Anchored_When_Header_Viewport_Scrolls()
    {
        var tabStrip = new AtomTabStrip
        {
            Width               = 160,
            Height              = 140,
            IsTabReorderEnabled = true,
            TabStripPlacement   = Dock.Left
        };
        for (var i = 0; i < 12; i++)
        {
            tabStrip.Items.Add(new AtomTabStripItem { Content = $"Tab {i + 1}" });
        }

        ShowInWindow(tabStrip, window =>
        {
            var scrollViewer = GetVisualDescendant<TabScrollViewer>(tabStrip);
            RunJobsUntil(() => scrollViewer.Extent.Height > scrollViewer.Viewport.Height);

            var dragged = GetContainer<AtomTabStripItem>(tabStrip, 0);
            var dragPoint = BeginDragOnTrack(window, tabStrip.TabStripPlacement, dragged);
            var beforeOffset = scrollViewer.Offset.Y;
            var beforeTranslate = GetTranslateY(dragged);

            scrollViewer.Offset = new Vector(scrollViewer.Offset.X, beforeOffset + 24);
            Dispatcher.UIThread.RunJobs();
            var offsetDelta = scrollViewer.Offset.Y - beforeOffset;
            offsetDelta.ShouldBeGreaterThan(0);

            GetTranslateY(dragged).ShouldBe(
                beforeTranslate + offsetDelta,
                1.0,
                "dragged tab preview must compensate header viewport scroll without waiting for another pointer move");

            ReleasePointer(window, dragPoint);
        });
    }

    private static void DragContainerAfter(AvaloniaWindow window, AtomTabControl tabControl, int sourceIndex, int targetIndex)
    {
        var source = GetContainer<AtomTabItem>(tabControl, sourceIndex);
        var target = GetContainer<AtomTabItem>(tabControl, targetIndex);
        DragCenterToTrailingEdge(window, tabControl.TabStripPlacement, source, target);
    }

    private static void DragContainerAfter(AvaloniaWindow window, AtomTabStrip tabStrip, int sourceIndex, int targetIndex)
    {
        var source = GetContainer<AtomTabStripItem>(tabStrip, sourceIndex);
        var target = GetContainer<AtomTabStripItem>(tabStrip, targetIndex);
        DragCenterToTrailingEdge(window, tabStrip.TabStripPlacement, source, target);
    }

    private static T GetContainer<T>(ItemsControl owner, int index)
        where T : Control
    {
        RunJobsUntil(() => owner.ContainerFromIndex(index) is T);
        return owner.ContainerFromIndex(index).ShouldBeOfType<T>();
    }

    private static void DragCenterToTrailingEdge(AvaloniaWindow window, Dock placement, Control source, Control target)
    {
        var start = TranslateToWindow(source, new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window);
        var end = placement is Dock.Top or Dock.Bottom
            ? TranslateToWindow(target, new Point(target.Bounds.Width + 12, target.Bounds.Height / 2), window)
            : TranslateToWindow(target, new Point(target.Bounds.Width / 2, target.Bounds.Height + 12), window);

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.MouseMove(end);
        Dispatcher.UIThread.RunJobs();
        window.MouseUp(end, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static Point DragContainerToPreviewAfter(
        AvaloniaWindow window,
        Dock placement,
        Control source,
        Control target,
        double secondaryAxisOffset = 0)
    {
        var start = TranslateToWindow(source, new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window);
        var end = GetNearTargetHalfPoint(window, placement, source, target, crossesHalf: true);
        if (placement is Dock.Top or Dock.Bottom)
        {
            end = new Point(end.X, end.Y + secondaryAxisOffset);
        }
        else
        {
            end = new Point(end.X + secondaryAxisOffset, end.Y);
        }

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.MouseMove(end);
        Dispatcher.UIThread.RunJobs();

        return end;
    }

    private static Point BeginDragOnTrack(AvaloniaWindow window, Dock placement, Control source)
    {
        var start = TranslateToWindow(source, new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window);
        var dragPoint = placement is Dock.Top or Dock.Bottom
            ? new Point(start.X + 24, start.Y)
            : new Point(start.X, start.Y + 24);

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.MouseMove(dragPoint);
        Dispatcher.UIThread.RunJobs();

        return dragPoint;
    }

    private static Point DragContainerToPreviewNearTargetHalf(
        AvaloniaWindow window,
        Dock placement,
        Control source,
        Control target,
        bool crossesHalf)
    {
        var start = TranslateToWindow(source, new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window);
        var end   = GetNearTargetHalfPoint(window, placement, source, target, crossesHalf);

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        window.MouseMove(end);
        Dispatcher.UIThread.RunJobs();

        return end;
    }

    private static Point DragPointerNearTargetHalf(
        AvaloniaWindow window,
        Dock placement,
        Control source,
        Control target,
        bool crossesHalf)
    {
        var end = GetNearTargetHalfPoint(window, placement, source, target, crossesHalf);
        window.MouseMove(end);
        Dispatcher.UIThread.RunJobs();
        return end;
    }

    private static Point GetNearTargetHalfPoint(
        AvaloniaWindow window,
        Dock placement,
        Control source,
        Control target,
        bool crossesHalf)
    {
        var start = TranslateToWindow(source, new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window);
        var sourceOrigin = TranslateToWindow(source, default, window);
        var targetOrigin = TranslateToWindow(target, default, window);
        var edgeOffset   = crossesHalf ? 2 : -2;

        if (placement is Dock.Top or Dock.Bottom)
        {
            var sourceTrailing = sourceOrigin.X + source.Bounds.Width;
            var targetMidpoint = targetOrigin.X + target.Bounds.Width / 2;
            var delta          = targetMidpoint - sourceTrailing + edgeOffset;
            return new Point(start.X + delta, targetOrigin.Y + target.Bounds.Height / 2);
        }
        else
        {
            var sourceTrailing = sourceOrigin.Y + source.Bounds.Height;
            var targetMidpoint = targetOrigin.Y + target.Bounds.Height / 2;
            var delta          = targetMidpoint - sourceTrailing + edgeOffset;
            return new Point(targetOrigin.X + target.Bounds.Width / 2, start.Y + delta);
        }
    }

    private static void ReleasePointer(AvaloniaWindow window, Point point)
    {
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static double GetTranslateX(Control control)
    {
        return control.RenderTransform is TranslateTransform transform ? transform.X : 0;
    }

    private static double GetTranslateY(Control control)
    {
        return control.RenderTransform is TranslateTransform transform ? transform.Y : 0;
    }

    private static void WaitForTranslateXLessThan(Control control, double value)
    {
        for (var i = 0; i < 32 && GetTranslateX(control) >= value; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
        GetTranslateX(control).ShouldBeLessThan(value);
    }

    private static void WaitForTranslateYLessThan(Control control, double value)
    {
        for (var i = 0; i < 32 && GetTranslateY(control) >= value; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
        GetTranslateY(control).ShouldBeLessThan(value);
    }

    private static void WaitForTranslateXCloseTo(Control control, double value)
    {
        for (var i = 0; i < 256 && Math.Abs(GetTranslateX(control) - value) > 0.5; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick(1);
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
        GetTranslateX(control).ShouldBe(value, 0.5);
    }

    private static bool IsEffectivelyTransparent(IBrush? brush)
    {
        return brush is null ||
               brush.Opacity <= 0 ||
               brush is ISolidColorBrush solidColorBrush && solidColorBrush.Color.A == 0;
    }

    private static bool IsTranslateTransformTransition(ITransition transition)
    {
        return transition.Property == TranslateTransform.XProperty ||
               transition.Property == TranslateTransform.YProperty;
    }

    private static double GetTransformOffsetX(ITransform? transform)
    {
        return transform?.Value.M31 ?? 0;
    }

    private static void WaitForSelectedIndicatorXCloseTo(AtomTabControl tabControl, double value)
    {
        for (var i = 0; i < 128 && Math.Abs(GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform) - value) > 0.5; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
        GetTransformOffsetX(tabControl.SelectedIndicatorRenderTransform).ShouldBe(value, 0.5);
    }

    private static void WaitForSelectedIndicatorXCloseTo(AtomTabStrip tabStrip, double value)
    {
        for (var i = 0; i < 128 && Math.Abs(GetTransformOffsetX(tabStrip.SelectedIndicatorRenderTransform) - value) > 0.5; i++)
        {
            Dispatcher.UIThread.RunJobs();
            Thread.Sleep(10);
        }
        GetTransformOffsetX(tabStrip.SelectedIndicatorRenderTransform).ShouldBe(value, 0.5);
    }

    private static void WaitForOpaqueBackground(AtomTabItem control)
    {
        for (var i = 0; i < 32; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (!IsEffectivelyTransparent(control.Background))
            {
                return;
            }
            Thread.Sleep(10);
        }
    }

    private static void DragContainerToScrollEndEdge(AvaloniaWindow window, Control source, ScrollViewer scrollViewer)
    {
        var start = TranslateToWindow(source, new Point(source.Bounds.Width / 2, source.Bounds.Height / 2), window);
        var scrollViewerOrigin = TranslateToWindow(scrollViewer, default, window);
        var end = new Point(
            scrollViewerOrigin.X + scrollViewer.Bounds.Width - 2,
            scrollViewerOrigin.Y + scrollViewer.Bounds.Height / 2);

        window.MouseMove(start);
        window.MouseDown(start, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
        for (var i = 0; i < 4; i++)
        {
            window.MouseMove(end);
            Dispatcher.UIThread.RunJobs();
        }
        window.MouseUp(end, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static Point TranslateToWindow(Control source, Point point, AvaloniaWindow window)
    {
        var translated = source.TranslatePoint(point, window);
        translated.ShouldNotBeNull();
        return translated.Value;
    }

    private static T GetVisualDescendant<T>(Control owner)
        where T : Visual
    {
        RunJobsUntil(() => owner.GetVisualDescendants().OfType<T>().Any());
        return owner.GetVisualDescendants().OfType<T>().First();
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        var window = new AvaloniaWindow
        {
            Width   = 520,
            Height  = 360,
            Content = content
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window);
        }
        finally
        {
            window.Close();
        }
    }

    private static void RunJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses; i++)
        {
            Dispatcher.UIThread.RunJobs();
            if (condition())
            {
                return;
            }
        }
    }
}
