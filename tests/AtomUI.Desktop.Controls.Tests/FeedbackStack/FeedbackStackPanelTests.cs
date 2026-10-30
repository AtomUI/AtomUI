using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.FeedbackStack;

public class FeedbackStackPanelTests
{
    [Fact]
    public void Message_Collapses_Only_When_Count_Is_Greater_Than_Threshold()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        var first = AddItem(panel, 100, 20);
        var second = AddItem(panel, 100, 20);
        var third = AddItem(panel, 100, 20);

        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.DesiredSize.Height.ShouldBe(92, 0.01);
        first.IsStackVisible.ShouldBeTrue();
        second.IsStackVisible.ShouldBeTrue();
        third.IsStackVisible.ShouldBeTrue();

        var latest = AddItem(panel, 120, 20);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeTrue();
        latest.IsStackVisible.ShouldBeTrue();
        third.IsStackVisible.ShouldBeFalse();
        second.IsStackVisible.ShouldBeFalse();
        first.IsStackVisible.ShouldBeFalse();
        panel.CollapsedCardSize.ShouldBe(new Size(120, 20));
        panel.DesiredSize.Height.ShouldBe(36, 0.01);
    }

    [Fact]
    public void Hover_Expanded_Message_Puts_Newest_Item_At_The_Top_And_Preserves_Gaps()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        var oldest = AddItem(panel, 80, 20);
        var middle = AddItem(panel, 90, 20);
        var newer = AddItem(panel, 100, 20);
        var latest = AddItem(panel, 110, 20);
        panel.IsStackExpanded = true;

        Layout(panel, 200);

        latest.Bounds.Y.ShouldBe(0, 0.01);
        newer.Bounds.Y.ShouldBe(0, 0.01);
        middle.Bounds.Y.ShouldBe(0, 0.01);
        oldest.Bounds.Y.ShouldBe(0, 0.01);
        GetRenderedY(latest).ShouldBe(0, 0.01);
        GetRenderedY(newer).ShouldBe(36, 0.01);
        GetRenderedY(middle).ShouldBe(72, 0.01);
        GetRenderedY(oldest).ShouldBe(108, 0.01);
        panel.DesiredSize.Height.ShouldBe(128, 0.01);
    }

    [Fact]
    public void Adding_A_Top_Item_Keeps_Existing_Bounds_Anchor_And_Only_Changes_Its_Transform_Target()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        panel.IsStackEnabled = false;
        var first = AddItem(panel, 100, 20);
        Layout(panel, 200);

        var originalBounds = first.Bounds;
        GetRenderedY(first).ShouldBe(0, 0.01);

        var latest = AddItem(panel, 100, 20);
        Layout(panel, 200);

        first.Bounds.ShouldBe(originalBounds);
        GetRenderedY(latest).ShouldBe(0, 0.01);
        GetRenderedY(first).ShouldBe(36, 0.01);
    }

    [Fact]
    public void Motion_Disabled_Arranges_Expanded_Items_At_Final_Bounds_Without_Transform_Churn()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        panel.IsMotionEnabled = false;
        panel.IsStackEnabled = false;
        var oldest = AddItem(panel, 100, 20);
        var latest = AddItem(panel, 100, 20);

        Layout(panel, 200);

        latest.Bounds.Y.ShouldBe(0, 0.01);
        oldest.Bounds.Y.ShouldBe(36, 0.01);
        GetTranslateY(latest).ShouldBe(0, 0.01);
        GetTranslateY(oldest).ShouldBe(0, 0.01);
        oldest.RenderTransform.ShouldBeSameAs(latest.RenderTransform);
    }

    [Fact]
    public void Motion_Disabled_Collapse_Does_Not_Rearrange_Hidden_Items()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        panel.IsMotionEnabled = false;
        panel.IsStackExpanded = true;
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }

        Layout(panel, 200);
        var oldest = panel.Children[0];
        var expandedBounds = oldest.Bounds;
        var expandedTransform = oldest.RenderTransform;

        panel.IsStackExpanded = false;
        Layout(panel, 200);

        oldest.IsHitTestVisible.ShouldBeFalse();
        oldest.Bounds.ShouldBe(expandedBounds);
        oldest.RenderTransform.ShouldBeSameAs(expandedTransform);
    }

    [Fact]
    public void Bottom_Expanded_Items_Share_The_Bottom_Anchor_And_Render_Upward()
    {
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.BottomCenter);
        panel.IsStackEnabled = false;
        var oldest = AddItem(panel, 100, 20);
        var middle = AddItem(panel, 100, 20);
        var latest = AddItem(panel, 100, 20);

        Layout(panel, 200);

        oldest.Bounds.Y.ShouldBe(72, 0.01);
        middle.Bounds.Y.ShouldBe(72, 0.01);
        latest.Bounds.Y.ShouldBe(72, 0.01);
        GetRenderedY(oldest).ShouldBe(0, 0.01);
        GetRenderedY(middle).ShouldBe(36, 0.01);
        GetRenderedY(latest).ShouldBe(72, 0.01);
    }

    [Fact]
    public void Notification_Collapsed_Stack_Uses_Three_Real_Cards_With_Ant_Scale_And_Offset()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        var hidden = AddItem(panel, 100, 40);
        var third = AddItem(panel, 100, 40);
        var second = AddItem(panel, 100, 40);
        var latest = AddItem(panel, 100, 40);

        Layout(panel, 200);

        hidden.IsStackVisible.ShouldBeFalse();
        third.IsStackVisible.ShouldBeTrue();
        second.IsStackVisible.ShouldBeTrue();
        latest.IsStackVisible.ShouldBeTrue();
        latest.Bounds.ShouldBe(new Rect(100, 0, 100, 40));
        second.Bounds.ShouldBe(new Rect(100, 0, 100, 40));
        third.Bounds.ShouldBe(new Rect(100, 0, 100, 40));
        GetScale(latest).ShouldBe(1, 0.001);
        GetScale(second).ShouldBe(0.94, 0.001);
        GetScale(third).ShouldBe(0.88, 0.001);
        GetTranslateY(second).ShouldBe(8, 0.001);
        GetTranslateY(third).ShouldBe(16, 0.001);
        panel.DesiredSize.Height.ShouldBe(56, 0.01);
    }

    [Fact]
    public void Notification_Collapse_Projects_Hidden_Cards_Into_The_Clipped_Back_Layer_Before_Fading()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        var hidden = AddItem(panel, 100, 40);
        AddItem(panel, 100, 40);
        AddItem(panel, 100, 40);
        AddItem(panel, 100, 40);
        panel.IsStackExpanded = true;
        Layout(panel, 200);

        panel.IsStackExpanded = false;
        Layout(panel, 200);

        hidden.IsStackVisible.ShouldBeFalse();
        hidden.IsHitTestVisible.ShouldBeFalse();
        GetScale(hidden).ShouldBe(0.88, 0.001);
        hidden.GetValue(FeedbackStackPanel.StackClipProgressProperty).ShouldBe(0.5, 0.001);
        hidden.Clip.ShouldBeOfType<RectangleGeometry>().Rect.Top.ShouldBe(20, 0.01);
    }

    [Fact]
    public void Notification_Collapsed_Top_Uses_Measured_Heights_To_Align_Layer_Far_Edges()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        AddItem(panel, 100, 80);
        var third = AddItem(panel, 100, 50);
        var second = AddItem(panel, 100, 70);
        var latest = AddItem(panel, 100, 40);

        Layout(panel, 200);

        GetRenderedY(latest).ShouldBe(0, 0.01);
        GetRenderedY(second).ShouldBe(-22, 0.01);
        GetRenderedY(third).ShouldBe(6, 0.01);
        (GetRenderedY(latest) + latest.Bounds.Height).ShouldBe(40, 0.01);
        (GetRenderedY(second) + second.Bounds.Height).ShouldBe(48, 0.01);
        (GetRenderedY(third) + third.Bounds.Height).ShouldBe(56, 0.01);
        panel.DesiredSize.Height.ShouldBe(56, 0.01);
    }

    [Fact]
    public void Notification_Collapsed_Bottom_Mirrors_Measured_Height_Projection()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.BottomLeft);
        AddItem(panel, 100, 80);
        var third = AddItem(panel, 100, 50);
        var second = AddItem(panel, 100, 70);
        var latest = AddItem(panel, 100, 40);

        Layout(panel, 200);

        GetRenderedY(latest).ShouldBe(16, 0.01);
        GetRenderedY(second).ShouldBe(8, 0.01);
        GetRenderedY(third).ShouldBe(0, 0.01);
        panel.DesiredSize.Height.ShouldBe(56, 0.01);
    }

    [Theory]
    [InlineData(1, 1, 20)]
    [InlineData(2, 2, 28)]
    [InlineData(3, 3, 36)]
    [InlineData(5, 3, 52)]
    public void Notification_Collapsed_Stack_Uses_Threshold_For_Visible_Layers_And_Extent(
        int threshold,
        int visibleCount,
        double expectedHeight)
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        panel.StackThreshold = threshold;
        for (var i = 0; i <= threshold; i++)
        {
            AddItem(panel, 100, 20);
        }

        Layout(panel, 200);

        panel.Children.Cast<TestFeedbackControl>().Count(item => item.IsStackVisible).ShouldBe(visibleCount);
        panel.Children.Cast<TestFeedbackControl>()
             .Where(item => !item.IsStackVisible)
             .ShouldAllBe(item => item.Clip is RectangleGeometry);
        panel.DesiredSize.Height.ShouldBe(expectedHeight, 0.01);
    }

    [Fact]
    public void Closing_Item_Retains_Its_Last_Projection_While_Siblings_Reflow_Immediately()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        panel.IsStackEnabled = false;
        var oldest = AddItem(panel, 100, 20);
        var middle = AddItem(panel, 100, 20);
        var latest = AddItem(panel, 100, 20);
        Layout(panel, 200);

        latest.RequestClose();
        panel.InvalidateMeasure();
        Layout(panel, 200);

        panel.DesiredSize.Height.ShouldBe(56, 0.01);
        GetRenderedY(latest).ShouldBe(0, 0.01);
        latest.IsHitTestVisible.ShouldBeFalse();
        GetRenderedY(middle).ShouldBe(0, 0.01);
        GetRenderedY(oldest).ShouldBe(36, 0.01);
    }

    [Fact]
    public void Notification_Collapsed_Back_Layers_Are_Clipped_Toward_The_Placement_Edge()
    {
        var topPanel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        for (var i = 0; i < 4; i++)
        {
            AddItem(topPanel, 100, 40);
        }
        Layout(topPanel, 200);

        var topFrontClip = topPanel.Children[^1].Clip.ShouldBeOfType<RectangleGeometry>();
        topFrontClip.Rect.Top.ShouldBe(-48, 0.01);
        topFrontClip.Rect.Bottom.ShouldBe(88, 0.01);
        var topBackClip = topPanel.Children[^2].Clip.ShouldBeOfType<RectangleGeometry>();
        topBackClip.Rect.Top.ShouldBe(20, 0.01);

        var bottomPanel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.BottomRight);
        for (var i = 0; i < 4; i++)
        {
            AddItem(bottomPanel, 100, 40);
        }
        Layout(bottomPanel, 200);

        var bottomFrontClip = bottomPanel.Children[^1].Clip.ShouldBeOfType<RectangleGeometry>();
        bottomFrontClip.Rect.Top.ShouldBe(-48, 0.01);
        bottomFrontClip.Rect.Bottom.ShouldBe(88, 0.01);
        var bottomBackClip = bottomPanel.Children[^2].Clip.ShouldBeOfType<RectangleGeometry>();
        bottomBackClip.Rect.Bottom.ShouldBe(20, 0.01);
    }

    [Theory]
    [InlineData(NotificationPosition.TopRight)]
    [InlineData(NotificationPosition.BottomRight)]
    public void Notification_Motion_Disabled_Reuses_The_Clip_And_Keeps_The_Animation_Base_In_Sync(NotificationPosition position)
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, position);
        panel.IsMotionEnabled = false;
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 40);
        }

        Layout(panel, 200);

        var backCard = panel.Children[^2];
        var clip = backCard.Clip.ShouldBeOfType<RectangleGeometry>();
        backCard.GetValue(FeedbackStackPanel.StackClipProgressProperty).ShouldBe(0.5);
        (position == NotificationPosition.TopRight ? clip.Rect.Top : clip.Rect.Bottom).ShouldBe(20, 0.01);

        panel.IsStackExpanded = true;
        Layout(panel, 200);
        backCard.GetValue(FeedbackStackPanel.StackClipProgressProperty).ShouldBe(0);
        clip.Rect.ShouldBe(new Rect(-48, -48, 196, 136));
        panel.IsStackExpanded = false;
        Layout(panel, 200);

        backCard.GetValue(FeedbackStackPanel.StackClipProgressProperty).ShouldBe(0.5);
        backCard.Clip.ShouldBeSameAs(clip);
        (position == NotificationPosition.TopRight ? clip.Rect.Top : clip.Rect.Bottom).ShouldBe(20, 0.01);
    }

    [Fact]
    public void Notification_Collapsed_Scale_Origin_Faces_The_Placement_Edge()
    {
        var topPanel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        var bottomPanel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.BottomRight);
        for (var i = 0; i < 4; i++)
        {
            AddItem(topPanel, 100, 40);
            AddItem(bottomPanel, 100, 40);
        }

        Layout(topPanel, 200);
        Layout(bottomPanel, 200);

        topPanel.Children[^2].RenderTransformOrigin.ShouldBe(
            new RelativePoint(0.5, 1, RelativeUnit.Relative));
        bottomPanel.Children[^2].RenderTransformOrigin.ShouldBe(
            new RelativePoint(0.5, 0, RelativeUnit.Relative));
    }

    [Fact]
    public void Bottom_Position_Mirrors_Collapsed_Offsets_And_Keeps_Latest_Nearest_The_Edge()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.BottomLeft);
        var third = AddItem(panel, 100, 40);
        var second = AddItem(panel, 100, 40);
        var latest = AddItem(panel, 100, 40);
        AddItem(panel, 100, 40);

        Layout(panel, 200);

        var actualLatest = (TestFeedbackControl)panel.Children[^1];
        actualLatest.Bounds.Y.ShouldBe(16, 0.01);
        latest.Bounds.Y.ShouldBe(16, 0.01);
        second.Bounds.Y.ShouldBe(16, 0.01);
        GetRenderedY(actualLatest).ShouldBe(16, 0.001);
        GetRenderedY(latest).ShouldBe(8, 0.001);
        GetRenderedY(second).ShouldBe(0, 0.001);
        third.IsStackVisible.ShouldBeFalse();
    }

    [Fact]
    public void Runtime_Stack_Disable_Expands_All_Items_With_Queue_Position_Transforms()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        var items = new TestFeedbackControl[4];
        for (var i = 0; i < items.Length; i++)
        {
            items[i] = AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        GetScale(items[1]).ShouldBe(0.88, 0.001);

        panel.IsStackEnabled = false;
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.DesiredSize.Height.ShouldBe(128, 0.01);
        for (var i = 0; i < items.Length; i++)
        {
            items[i].IsStackVisible.ShouldBeTrue();
            items[i].IsHitTestVisible.ShouldBeTrue();
            GetScale(items[i]).ShouldBe(1, 0.001);
        }
        GetRenderedY(items[3]).ShouldBe(0, 0.01);
        GetRenderedY(items[2]).ShouldBe(36, 0.01);
        GetRenderedY(items[1]).ShouldBe(72, 0.01);
        GetRenderedY(items[0]).ShouldBe(108, 0.01);
    }

    [Fact]
    public void Repeated_Layout_Reuses_Cached_Layer_Transforms()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        var secondLayer = panel.Children[^2].RenderTransform;
        var thirdLayer = panel.Children[^3].RenderTransform;

        for (var i = 0; i < 10; i++)
        {
            panel.InvalidateArrange();
            Layout(panel, 200);
            panel.Children[^2].RenderTransform.ShouldBeSameAs(secondLayer);
            panel.Children[^3].RenderTransform.ShouldBeSameAs(thirdLayer);
        }
    }

    [Fact]
    public void Repeated_Layout_Reuses_Cached_Back_Layer_Clip_Geometry()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 40);
        }
        Layout(panel, 200);
        var secondLayerClip = panel.Children[^2].Clip;
        var thirdLayerClip = panel.Children[^3].Clip;

        for (var i = 0; i < 10; i++)
        {
            panel.InvalidateArrange();
            Layout(panel, 200);
            panel.Children[^2].Clip.ShouldBeSameAs(secondLayerClip);
            panel.Children[^3].Clip.ShouldBeSameAs(thirdLayerClip);
        }
    }

    [Fact]
    public void Repeated_Collapse_And_Expand_Reuses_Both_Transform_Targets()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }

        Layout(panel, 200);
        var card = panel.Children[^2];
        var collapsedTransform = card.RenderTransform;

        panel.IsStackExpanded = true;
        Layout(panel, 200);
        var expandedTransform = card.RenderTransform;
        expandedTransform.ShouldNotBeSameAs(collapsedTransform);

        panel.IsStackExpanded = false;
        Layout(panel, 200);
        card.RenderTransform.ShouldBeSameAs(collapsedTransform);

        panel.IsStackExpanded = true;
        Layout(panel, 200);
        card.RenderTransform.ShouldBeSameAs(expandedTransform);
    }

    [Fact]
    public void Notification_Collapse_Snapshots_Only_Visible_Cards_Whose_Projection_Changes()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        panel.IsStackExpanded = true;
        var oldest = AddItem(panel, 100, 40);
        var third = AddItem(panel, 100, 40);
        var second = AddItem(panel, 100, 40);
        var latest = AddItem(panel, 100, 40);
        Layout(panel, 200);

        panel.IsStackExpanded = false;
        Layout(panel, 200);

        oldest.CollapseSnapshotBeginCount.ShouldBe(0);
        third.CollapseSnapshotBeginCount.ShouldBe(1);
        second.CollapseSnapshotBeginCount.ShouldBe(1);
        latest.CollapseSnapshotBeginCount.ShouldBe(0);
        third.CollapseSnapshotArmCount.ShouldBe(1);
        second.CollapseSnapshotArmCount.ShouldBe(1);
        third.CollapseSnapshotTarget.ShouldBeSameAs(third.RenderTransform);
        second.CollapseSnapshotTarget.ShouldBeSameAs(second.RenderTransform);
    }

    [Fact]
    public void Initial_Notification_Collapsed_Layout_Does_Not_Create_Content_Snapshots()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 40);
        }

        Layout(panel, 200);

        panel.Children.Cast<TestFeedbackControl>()
             .ShouldAllBe(item => item.CollapseSnapshotBeginCount == 0);
    }

    [Fact]
    public void Expanding_Notification_Stack_Releases_Active_Content_Snapshots()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        panel.IsStackExpanded = true;
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 40);
        }
        Layout(panel, 200);

        panel.IsStackExpanded = false;
        Layout(panel, 200);
        var second = (TestFeedbackControl)panel.Children[^2];
        var third = (TestFeedbackControl)panel.Children[^3];
        second.IsCollapseSnapshotActive.ShouldBeTrue();
        third.IsCollapseSnapshotActive.ShouldBeTrue();

        panel.IsStackExpanded = true;
        Layout(panel, 200);

        second.IsCollapseSnapshotActive.ShouldBeFalse();
        third.IsCollapseSnapshotActive.ShouldBeFalse();
        second.CollapseSnapshotReleaseCount.ShouldBe(1);
        third.CollapseSnapshotReleaseCount.ShouldBe(1);
    }

    [Fact]
    public void Disabling_Motion_Releases_Active_Notification_Content_Snapshots()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        panel.IsStackExpanded = true;
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 40);
        }
        Layout(panel, 200);

        panel.IsStackExpanded = false;
        Layout(panel, 200);
        var second = (TestFeedbackControl)panel.Children[^2];
        second.IsCollapseSnapshotActive.ShouldBeTrue();

        panel.IsMotionEnabled = false;
        Layout(panel, 200);

        second.IsCollapseSnapshotActive.ShouldBeFalse();
        second.CollapseSnapshotReleaseCount.ShouldBe(1);
    }

    [Fact]
    public void Closing_Notification_Releases_Its_Active_Content_Snapshot()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        panel.IsStackExpanded = true;
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 40);
        }
        Layout(panel, 200);

        panel.IsStackExpanded = false;
        Layout(panel, 200);
        var second = (TestFeedbackControl)panel.Children[^2];
        second.IsCollapseSnapshotActive.ShouldBeTrue();

        second.RequestClose();
        panel.InvalidateMeasure();
        Layout(panel, 200);

        second.IsCollapseSnapshotActive.ShouldBeFalse();
        second.CollapseSnapshotReleaseCount.ShouldBe(1);
    }

    [Fact]
    public void Cached_Transform_Does_Not_Retain_A_Removed_Card()
    {
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);

        var removedCard = RemoveCachedCard(panel);
        ForceFullCollection();

        removedCard.IsAlive.ShouldBeFalse();
        GC.KeepAlive(panel);
    }

    [Fact]
    public void Presenter_Hover_Expands_And_Collapses_The_Whole_Stack()
    {
        var presenter = new FeedbackStackPresenter
        {
            StackMode = FeedbackStackMode.Message,
            IsStackEnabled = true,
            StackThreshold = 3
        };
        var panel = CreatePanel(FeedbackStackMode.Message, NotificationPosition.TopCenter);
        for (var i = 0; i < 4; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        presenter.AttachPanel(panel);
        presenter.ReportLayoutState(panel.IsCollapsed, panel.CollapsedCardSize, 4);
        var hoverEvents = 0;
        presenter.StackHoverChanged += (_, args) =>
        {
            args.IsStackInteraction.ShouldBeTrue();
            hoverEvents++;
        };

        presenter.SetPointerOverState(true);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.Children.Cast<TestFeedbackControl>().ShouldAllBe(item => item.IsStackVisible);

        presenter.SetPointerOverState(false);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeTrue();
        hoverEvents.ShouldBe(2);
    }

    [Fact]
    public void Presenter_Hover_Remains_A_Stack_Interaction_When_Count_Crosses_Threshold()
    {
        var presenter = new FeedbackStackPresenter
        {
            StackMode = FeedbackStackMode.Notification,
            IsStackEnabled = true,
            StackThreshold = 3
        };
        var panel = CreatePanel(FeedbackStackMode.Notification, NotificationPosition.TopRight);
        for (var i = 0; i < 3; i++)
        {
            AddItem(panel, 100, 20);
        }
        Layout(panel, 200);
        presenter.AttachPanel(panel);
        presenter.ReportLayoutState(panel.IsCollapsed, panel.CollapsedCardSize, 3);

        FeedbackStackHoverChangedEventArgs? hoverState = null;
        presenter.StackHoverChanged += (_, args) => hoverState = args;
        presenter.SetPointerOverState(true);

        hoverState.ShouldNotBeNull();
        hoverState!.IsStackInteraction.ShouldBeTrue();

        AddItem(panel, 100, 20);
        presenter.ReportLayoutState(true, new Size(100, 20), 4);
        Layout(panel, 200);

        panel.IsCollapsed.ShouldBeFalse();
        panel.Children.Cast<TestFeedbackControl>().ShouldAllBe(item => item.IsStackVisible);
    }

    [Fact]
    public void Notification_Presenter_Does_Not_Project_Message_Backplate_State()
    {
        var presenter = new FeedbackStackPresenter
        {
            StackMode = FeedbackStackMode.Notification,
            Position = NotificationPosition.TopRight
        };

        presenter.ReportLayoutState(true, new Size(100, 40), 4);

        presenter.IsFirstBackplateVisible.ShouldBeFalse();
        presenter.IsSecondBackplateVisible.ShouldBeFalse();
        presenter.FirstBackplateWidth.ShouldBe(0);
        presenter.SecondBackplateWidth.ShouldBe(0);
        presenter.FirstBackplateMargin.ShouldBe(default);
        presenter.SecondBackplateMargin.ShouldBe(default);
    }

    private static FeedbackStackPanel CreatePanel(FeedbackStackMode mode, NotificationPosition position)
    {
        return new FeedbackStackPanel
        {
            StackMode = mode,
            Position = position,
            IsStackEnabled = true,
            StackThreshold = 3,
            ExpandedGap = 16,
            CollapsedOffset = 8
        };
    }

    private static TestFeedbackControl AddItem(Panel panel, double width, double height)
    {
        var item = new TestFeedbackControl(new Size(width, height));
        panel.Children.Add(item);
        return item;
    }

    private static void Layout(Control control, double width)
    {
        control.Measure(new Size(width, double.PositiveInfinity));
        control.Arrange(new Rect(0, 0, width, control.DesiredSize.Height));
    }

    private static double GetScale(Control control)
    {
        control.RenderTransform.ShouldNotBeNull();
        return control.RenderTransform.Value.M11;
    }

    private static double GetTranslateY(Control control)
    {
        control.RenderTransform.ShouldNotBeNull();
        return control.RenderTransform.Value.M32;
    }

    private static double GetRenderedY(Control control)
    {
        return control.Bounds.Y + GetTranslateY(control);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference RemoveCachedCard(FeedbackStackPanel panel)
    {
        var card = panel.Children[^2];
        var reference = new WeakReference(card);
        panel.Children.Remove(card);
        return reference;
    }

    private static void ForceFullCollection()
    {
        for (var i = 0; i < 3; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }

    private sealed class TestFeedbackControl : Control, IFeedbackStackItem, IFeedbackStackTransitionSnapshotItem
    {
        private readonly Size _size;

        public TestFeedbackControl(Size size)
        {
            _size = size;
        }

        public bool IsClosing { get; private set; }
        public bool IsClosed { get; private set; }
        public bool IsProgressVisible => false;
        public bool IsStackVisible { get; set; } = true;
        public int CollapseSnapshotBeginCount { get; private set; }
        public int CollapseSnapshotArmCount { get; private set; }
        public int CollapseSnapshotReleaseCount { get; private set; }
        public bool IsCollapseSnapshotActive { get; private set; }
        public ITransform? CollapseSnapshotTarget { get; private set; }

        public void RequestClose()
        {
            IsClosing = true;
        }

        public void UpdateRemaining(TimeSpan remaining)
        {
        }

        public bool TryBeginStackCollapseSnapshot()
        {
            CollapseSnapshotBeginCount++;
            IsCollapseSnapshotActive = true;
            return true;
        }

        public void ArmStackCollapseSnapshot(ITransform targetTransform)
        {
            CollapseSnapshotArmCount++;
            CollapseSnapshotTarget = targetTransform;
        }

        public void ReleaseStackTransitionSnapshot()
        {
            if (!IsCollapseSnapshotActive)
            {
                return;
            }

            IsCollapseSnapshotActive = false;
            CollapseSnapshotReleaseCount++;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return _size;
        }
    }
}
