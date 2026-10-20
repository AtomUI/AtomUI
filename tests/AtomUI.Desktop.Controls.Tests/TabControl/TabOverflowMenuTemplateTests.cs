using System.Reflection;
using System.Runtime.CompilerServices;
using AtomUI.Controls.Primitives;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.TabControl;

public class TabOverflowMenuTemplateTests
{
    static TabOverflowMenuTemplateTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    public static TheoryData<OwnerKind> OwnerKinds =>
    [
        OwnerKind.TabControl,
        OwnerKind.CardTabControl,
        OwnerKind.TabStrip,
        OwnerKind.CardTabStrip
    ];

    // Shared TabScrollViewer behavior needs one representative of each owner base.
    public static TheoryData<OwnerKind> BaseOwnerKinds =>
    [
        OwnerKind.TabControl,
        OwnerKind.TabStrip
    ];

    // Popup hosting, visual effects, and retention live in the shared viewer.
    public static TheoryData<OwnerKind> SharedViewerOwner => [OwnerKind.TabControl];

    // Each owner and placement keeps its own directional shadow and viewport checks.
    public static TheoryData<OwnerKind, Dock> OwnerPlacementCases
    {
        get
        {
            var cases = new TheoryData<OwnerKind, Dock>();
            foreach (var ownerKind in OwnerKinds)
            {
                cases.Add(ownerKind, Dock.Top);
                cases.Add(ownerKind, Dock.Bottom);
                cases.Add(ownerKind, Dock.Left);
                cases.Add(ownerKind, Dock.Right);
            }
            return cases;
        }
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void OverflowPopupTemplate_Defaults_To_Null_And_Is_Inherited(OwnerKind kind)
    {
        var owner = CreateOwner(kind);
        GetOverflowPopupTemplate(owner).ShouldBeNull();

        var template = new CapturingTemplate();
        SetOverflowPopupTemplate(owner, template);

        GetOverflowPopupTemplate(owner).ShouldBeSameAs(template);
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void Overflow_Snapshot_Preserves_Logical_Item_Header_Template_And_Effective_State(OwnerKind kind)
    {
        var headerTemplate = CreateHeaderTemplate();
        var owner = CreateOwner(kind, headerTemplate, isClosable: true);

        ShowInWindow(owner, window =>
        {
            var context = OpenOverflow(owner).OverflowPopupContext.ShouldNotBeNull();

            context.Items.ShouldNotBeEmpty();
            foreach (var item in context.Items)
            {
                var index = FindLogicalItemIndex(owner, item.Item);
                index.ShouldBeGreaterThanOrEqualTo(0);
                var container = GetContainer(owner, index);
                item.Header.ShouldBeSameAs(GetHeader(container));
                item.HeaderTemplate.ShouldBeSameAs(GetHeaderTemplate(container));
                item.IsEnabled.ShouldBe(container.IsEffectivelyEnabled);
                item.IsClosable.ShouldBe(GetIsClosable(container));
                item.IsSelected.ShouldBe(index == GetSelectedIndex(owner));
            }
        });
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void Partially_Clipped_Item_Is_Included(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, window =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            viewer.Offset = new Vector(10, 0);
            Dispatcher.UIThread.RunJobs();

            OpenOverflow(owner).OverflowPopupContext!
                               .Items
                               .ShouldContain(item => ReferenceEquals(item.Item, GetLogicalItem(owner, 0)));
        });
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void TryActivate_Uses_Current_Owner_Selection_And_Requires_Explicit_Dismiss(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var target = context.Items.Last(item => item.IsEnabled);
            var targetIndex = FindLogicalItemIndex(owner, target.Item);

            context.TryActivate(target).ShouldBeTrue();
            GetSelectedIndex(owner).ShouldBe(targetIndex);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();

            context.Dismiss();
            viewer.IsOverflowPopupOpen.ShouldBeFalse();
            context.Items.ShouldBeEmpty();
            context.SelectedItem.ShouldBeNull();
            context.TryActivate(target).ShouldBeFalse();
            context.TryClose(target).ShouldBeFalse();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Selection_Change_Republishes_Immutable_Snapshot(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var firstSnapshot = context.Items;
            var target = firstSnapshot.Last();
            var targetIndex = FindLogicalItemIndex(owner, target.Item);
            var notifications = new List<string?>();
            context.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);

            SetSelectedIndex(owner, targetIndex);
            Dispatcher.UIThread.RunJobs();

            context.Items.ShouldNotBeSameAs(firstSnapshot);
            context.SelectedItem.ShouldNotBeNull();
            context.SelectedItem!.Item.ShouldBeSameAs(target.Item);
            notifications.ShouldContain(nameof(TabOverflowPopupContext.Items));
            notifications.ShouldContain(nameof(TabOverflowPopupContext.SelectedItem));
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void External_Collection_Mutation_Invalidates_And_Closes_Session(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var staleItem = context.Items.First();

            AddItem(owner, "External mutation");
            Dispatcher.UIThread.RunJobs();

            viewer.IsOverflowPopupOpen.ShouldBeFalse();
            context.Items.ShouldBeEmpty();
            context.SelectedItem.ShouldBeNull();
            context.TryActivate(staleItem).ShouldBeFalse();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Successful_Close_Uses_Owner_Events_And_Invalidates_Session(OwnerKind kind)
    {
        var owner = CreateOwner(kind, isClosable: true);
        var eventOrder = new List<string>();
        AddCloseEventHandlers(owner, eventOrder, cancel: false);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var target = context.Items.First(item => item.IsClosable);
            var oldCount = GetItemCount(owner);

            context.TryClose(target).ShouldBeTrue();

            eventOrder.ShouldBe(["closing", "closed"]);
            GetItemCount(owner).ShouldBe(oldCount - 1);
            viewer.IsOverflowPopupOpen.ShouldBeFalse();
            context.Items.ShouldBeEmpty();
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Cancelled_Close_Leaves_Owner_And_Session_Untouched(OwnerKind kind)
    {
        var owner = CreateOwner(kind, isClosable: true);
        var eventOrder = new List<string>();
        AddCloseEventHandlers(owner, eventOrder, cancel: true);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var target = context.Items.First(item => item.IsClosable);
            var oldItems = context.Items;
            var oldCount = GetItemCount(owner);

            context.TryClose(target).ShouldBeFalse();

            eventOrder.ShouldBe(["closing"]);
            GetItemCount(owner).ShouldBe(oldCount);
            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            context.Items.ShouldBeSameAs(oldItems);
        });
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Custom_Template_Root_Is_Lazy_And_Reused_Between_Ordinary_Closes(OwnerKind kind)
    {
        var template = new CapturingTemplate();
        var owner = CreateOwner(kind);
        SetOverflowPopupTemplate(owner, template);

        ShowInWindow(owner, _ =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            template.BuildCount.ShouldBe(0);
            viewer.OverflowPopupContext.ShouldBeNull();

            var firstContext = OpenOverflow(owner).OverflowPopupContext.ShouldNotBeNull();
            var root = template.Root.ShouldNotBeNull();
            template.BuildCount.ShouldBe(1);

            firstContext.Dismiss();
            Dispatcher.UIThread.RunJobs();
            firstContext.Items.ShouldBeEmpty();

            OpenOverflow(owner).OverflowPopupContext.ShouldBeSameAs(firstContext);
            template.BuildCount.ShouldBe(1);
            template.Root.ShouldBeSameAs(root);
        });
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Template_Replacement_Performs_Full_Teardown(OwnerKind kind)
    {
        var oldTemplate = new CapturingTemplate();
        var newTemplate = new CapturingTemplate();
        var owner = CreateOwner(kind);
        SetOverflowPopupTemplate(owner, oldTemplate);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var oldContext = viewer.OverflowPopupContext.ShouldNotBeNull();
            var oldRoot = oldTemplate.Root.ShouldNotBeNull();

            SetOverflowPopupTemplate(owner, newTemplate);
            Dispatcher.UIThread.RunJobs();

            viewer.IsOverflowPopupOpen.ShouldBeFalse();
            viewer.OverflowPopupContext.ShouldBeNull();
            oldContext.Items.ShouldBeEmpty();
            oldRoot.GetVisualParent().ShouldBeNull();

            OpenOverflow(owner);
            newTemplate.BuildCount.ShouldBe(1);
            newTemplate.Root.ShouldNotBeSameAs(oldRoot);
        });
    }

    [Theory]
    [InlineData(Dock.Top, PlacementMode.BottomEdgeAlignedLeft)]
    [InlineData(Dock.Bottom, PlacementMode.TopEdgeAlignedLeft)]
    [InlineData(Dock.Right, PlacementMode.LeftEdgeAlignedBottom)]
    [InlineData(Dock.Left, PlacementMode.RightEdgeAlignedBottom)]
    public void Popup_Placement_Follows_TabStrip_Edge(Dock dock, PlacementMode expected)
    {
        var owner = CreateOwner(OwnerKind.TabControl, placement: dock);

        ShowInWindow(owner, _ =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            viewer.OverflowPopupPlacement.ShouldBe(expected);
            OpenOverflow(owner).OverflowPopup!.RequestedPlacement.ShouldBe(expected);
        });
    }

    [Theory]
    [MemberData(nameof(OwnerPlacementCases))]
    public void Overflow_Edge_Indicators_Use_Directional_Outer_Shadows(
        OwnerKind kind,
        Dock placement)
    {
        var owner = CreateOwner(kind, placement: placement);
        var isHorizontal = placement is Dock.Top or Dock.Bottom;

        ShowInWindow(owner, _ =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            var presenter = viewer.GetVisualDescendants()
                                  .OfType<TabScrollContentPresenter>()
                                  .ShouldHaveSingleItem();
            var startIndicator = viewer.GetVisualDescendants()
                                       .OfType<TabOverflowEdgeIndicator>()
                                       .Single(border => border.Name == "PART_ScrollStartEdgeIndicator");
            var endIndicator = viewer.GetVisualDescendants()
                                     .OfType<TabOverflowEdgeIndicator>()
                                     .Single(border => border.Name == "PART_ScrollEndEdgeIndicator");

            AssertDirectionalOuterShadow(startIndicator, isHorizontal, isStart: true);
            AssertDirectionalOuterShadow(endIndicator, isHorizontal, isStart: false);

            var startBounds = TransformBounds(startIndicator, presenter);
            var endBounds = TransformBounds(endIndicator, presenter);
            if (isHorizontal)
            {
                startBounds.Right.ShouldBe(0, 0.5);
                endBounds.Left.ShouldBe(presenter.Bounds.Width, 0.5);
                startIndicator.Width.ShouldBe(32, 0.5);
                endIndicator.Width.ShouldBe(32, 0.5);
            }
            else
            {
                startBounds.Bottom.ShouldBe(0, 0.5);
                endBounds.Top.ShouldBe(presenter.Bounds.Height, 0.5);
                startIndicator.Height.ShouldBe(32, 0.5);
                endIndicator.Height.ShouldBe(32, 0.5);
            }

            startIndicator.IsVisible.ShouldBeFalse();
            endIndicator.IsVisible.ShouldBeTrue();

            viewer.Offset = isHorizontal
                ? new Vector(viewer.ScrollBarMaximum.X, 0)
                : new Vector(0, viewer.ScrollBarMaximum.Y);
            Dispatcher.UIThread.RunJobs();

            startIndicator.IsVisible.ShouldBeTrue();
            endIndicator.IsVisible.ShouldBeFalse();
        });
    }

    [Theory]
    [InlineData(Dock.Top)]
    [InlineData(Dock.Bottom)]
    [InlineData(Dock.Left)]
    [InlineData(Dock.Right)]
    public void Visible_End_Edge_Shadow_Uses_A_Viewport_Clipped_Input_Transparent_Layer(Dock placement)
    {
        var owner = CreateOwner(OwnerKind.TabControl, placement: placement);
        var isHorizontal = placement is Dock.Top or Dock.Bottom;

        ShowInWindow(owner, window =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            var presenter = viewer.GetVisualDescendants()
                                  .OfType<TabScrollContentPresenter>()
                                  .ShouldHaveSingleItem();
            var endIndicator = viewer.GetVisualDescendants()
                                     .OfType<TabOverflowEdgeIndicator>()
                                     .Single(border => border.Name == "PART_ScrollEndEdgeIndicator");

            var indicatorBounds = TransformBounds(endIndicator, window);
            var presenterBounds = TransformBounds(presenter, window);
            endIndicator.IsHitTestVisible.ShouldBeFalse();
            var layer = endIndicator.GetVisualParent().ShouldBeOfType<Canvas>();
            layer.ClipToBounds.ShouldBeTrue();
            layer.IsHitTestVisible.ShouldBeFalse();
            TransformBounds(layer, window).ShouldBe(presenterBounds);

            var shadow = endIndicator.BoxShadow[0];
            if (isHorizontal)
            {
                shadow.OffsetX.ShouldBeLessThan(0);
                indicatorBounds.Left.ShouldBe(presenterBounds.Right, 0.5);
                (indicatorBounds.Left - 12).ShouldBeGreaterThanOrEqualTo(presenterBounds.Left);
            }
            else
            {
                shadow.OffsetY.ShouldBeLessThan(0);
                indicatorBounds.Top.ShouldBe(presenterBounds.Bottom, 0.5);
                (indicatorBounds.Top - 12).ShouldBeGreaterThanOrEqualTo(presenterBounds.Top);
            }
        });
    }

    [Theory]
    [MemberData(nameof(OwnerPlacementCases))]
    public void Overflow_Shadow_Casters_Follow_The_Viewport_Boundary(OwnerKind kind, Dock placement)
    {
        var owner = CreateOwner(kind, placement: placement);
        ShowInWindow(owner, window =>
        {
            var oppositeAxis = placement is Dock.Top or Dock.Bottom ? Dock.Left : Dock.Top;
            foreach (var currentPlacement in new[] { placement, oppositeAxis, placement })
            {
                if (owner is BaseTabControl tabControl) tabControl.TabStripPlacement = currentPlacement;
                else ((BaseTabStrip)owner).TabStripPlacement = currentPlacement;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                var viewer = GetOverflowScrollViewer(owner);
                var viewport = viewer.GetVisualDescendants().OfType<Panel>()
                                     .Single(control => control.Name == "ScrollContentViewport");
                var start = viewer.GetVisualDescendants().OfType<TabOverflowEdgeIndicator>()
                                  .Single(control => control.Name == "PART_ScrollStartEdgeIndicator");
                var end = viewer.GetVisualDescendants().OfType<TabOverflowEdgeIndicator>()
                                .Single(control => control.Name == "PART_ScrollEndEdgeIndicator");
                var horizontal = currentPlacement is Dock.Top or Dock.Bottom;
                viewer.Offset = horizontal ? new Vector(viewer.ScrollBarMaximum.X / 2, 0)
                                           : new Vector(0, viewer.ScrollBarMaximum.Y / 2);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                start.IsVisible.ShouldBeTrue();
                end.IsVisible.ShouldBeTrue();
                var startBounds = TransformBounds(start, viewport);
                var endBounds = TransformBounds(end, viewport);
                // Outer shadows exclude the caster. Its inward face must meet the
                // scroll boundary even after switching axes on the same owner.
                if (horizontal)
                {
                    startBounds.Right.ShouldBe(0, 0.5);
                    endBounds.Left.ShouldBe(viewport.Bounds.Width, 0.5);
                    startBounds.Height.ShouldBe(viewport.Bounds.Height, 0.5);
                    endBounds.Height.ShouldBe(viewport.Bounds.Height, 0.5);
                }
                else
                {
                    startBounds.Bottom.ShouldBe(0, 0.5);
                    endBounds.Top.ShouldBe(viewport.Bounds.Height, 0.5);
                    startBounds.Width.ShouldBe(viewport.Bounds.Width, 0.5);
                    endBounds.Width.ShouldBe(viewport.Bounds.Width, 0.5);
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Default_Overflow_Menu_Hides_The_Scrollbar_But_Remains_Scrollable(OwnerKind kind)
    {
        var owner = CreateOwner(kind);
        for (var index = 0; index < 20; index++)
        {
            AddItem(owner, $"Additional document {index:00}");
        }

        ShowInWindow(owner, window =>
        {
            var menu = OpenOverflow(owner).OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
            var scrollViewer = menu.GetVisualDescendants()
                                   .OfType<AtomUI.Desktop.Controls.ScrollViewer>()
                                   .ShouldHaveSingleItem();
            var verticalScrollBar = scrollViewer.GetVisualDescendants()
                                                .OfType<AtomUI.Desktop.Controls.ScrollBar>()
                                                .Single(item => item.Orientation == Orientation.Vertical);

            scrollViewer.VerticalScrollBarVisibility.ShouldBe(ScrollBarVisibility.Hidden);
            verticalScrollBar.IsVisible.ShouldBeFalse();
            scrollViewer.ScrollBarMaximum.Y.ShouldBeGreaterThan(0);

            var wheelPoint = scrollViewer.TranslatePoint(
                new Point(scrollViewer.Bounds.Width / 2, scrollViewer.Bounds.Height / 2),
                window);
            wheelPoint.ShouldNotBeNull();
            window.MouseMove(wheelPoint.Value);
            window.MouseWheel(wheelPoint.Value, new Vector(0, -1), RawInputModifiers.None);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            scrollViewer.Offset.Y.ShouldBeGreaterThan(0);

            scrollViewer.Offset = new Vector(0, scrollViewer.ScrollBarMaximum.Y);
            Dispatcher.UIThread.RunJobs();
            scrollViewer.Offset.Y.ShouldBeGreaterThan(0);
        });
    }

    private static void AssertDirectionalOuterShadow(TabOverflowEdgeIndicator indicator, bool isHorizontal, bool isStart)
    {
        indicator.BoxShadow.Count.ShouldBe(1);
        var shadow = indicator.BoxShadow[0];
        shadow.IsInset.ShouldBeFalse();
        shadow.Blur.ShouldBe(8);
        shadow.Spread.ShouldBe(-8);
        shadow.Color.ShouldBe(Color.FromArgb(20, 0, 0, 0));
        shadow.OffsetX.ShouldBe(isHorizontal ? (isStart ? 10 : -10) : 0);
        shadow.OffsetY.ShouldBe(isHorizontal ? 0 : (isStart ? 10 : -10));
    }

    private static Rect TransformBounds(Control control, Visual relativeTo)
    {
        var transform = control.TransformToVisual(relativeTo).ShouldNotBeNull();
        return new Rect(control.Bounds.Size).TransformToAABB(transform);
    }


    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Detach_Performs_Full_Teardown_Regardless_Of_Pinned_State(OwnerKind kind)
    {
        var template = new CapturingTemplate();
        var owner = CreateOwner(kind);
        SetOverflowPopupTemplate(owner, template);
        SetPinnedOpen(owner, true);

        ShowInWindowWithHost(owner, (_, host) =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var root = template.Root.ShouldNotBeNull();

            host.Children.Remove(owner);
            Dispatcher.UIThread.RunJobs();

            viewer.IsOverflowPopupOpen.ShouldBeFalse();
            viewer.OverflowPopupContext.ShouldBeNull();
            viewer.OverflowOwner.ShouldBeNull();
            context.Items.ShouldBeEmpty();
            root.GetVisualParent().ShouldBeNull();
        });
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void Default_Template_Uses_Shared_Overflow_Menu(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            viewer.OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
            viewer.OverflowPopupRoot!.GetVisualDescendants()
                  .OfType<TabOverflowMenuItem>()
                  .ShouldNotBeEmpty();
        });
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Default_Overflow_Surface_Uses_The_Menu_Corner_Radius(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var menu = OpenOverflow(owner).OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
            var shadowContainer = menu.GetVisualAncestors()
                                      .OfType<ShadowsAwareContainer>()
                                      .ShouldHaveSingleItem();

            menu.CornerRadius.TopLeft.ShouldBeGreaterThan(0);
            shadowContainer.CornerRadius.ShouldBe(menu.CornerRadius);
        });
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Opening_Overflow_From_Pointer_Does_Not_Move_Focus_Into_The_Popup(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = GetOverflowScrollViewer(owner);
            var indicator = GetMenuIndicator(viewer);
            indicator.Focus();
            TopLevel.GetTopLevel(owner)!.FocusManager.GetFocusedElement().ShouldBeSameAs(indicator);

            indicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, indicator));
            Dispatcher.UIThread.RunJobs();

            viewer.IsOverflowPopupOpen.ShouldBeTrue();
            TopLevel.GetTopLevel(owner)!.FocusManager.GetFocusedElement().ShouldBeSameAs(indicator);
        });
    }

    [Theory]
    [MemberData(nameof(OwnerKinds))]
    public void Menu_Indicator_Is_Reserved_When_Final_Arrange_Is_Narrower_Than_Measure(OwnerKind kind)
    {
        var owner = CreateOwner(kind);
        while (GetItemCount(owner) > 4)
        {
            RemoveLastItem(owner);
        }
        owner.Width = double.NaN;
        var host = new ScopeAwareOverlayLayerPanel { Width = 900, Height = 240 };
        host.Children.Add(owner);
        var window = new AvaloniaWindow
        {
            Width = 900,
            Height = 240,
            Content = CreatePopupOverlayHost(host)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var viewer = owner.GetVisualDescendants().OfType<TabScrollViewer>().ShouldHaveSingleItem();
            GetMenuIndicator(viewer).IsVisible.ShouldBeFalse();

            owner.Measure(new Size(900, 240));
            owner.Arrange(new Rect(0, 0, 220, 240));

            var indicator = GetMenuIndicator(viewer);
            indicator.IsVisible.ShouldBeTrue();
            var transform = indicator.TransformToVisual(owner).ShouldNotBeNull();
            var bounds = new Rect(indicator.Bounds.Size).TransformToAABB(transform);
            bounds.Right.ShouldBeLessThanOrEqualTo(owner.Bounds.Width + 0.5);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Theory]
    [MemberData(nameof(BaseOwnerKinds))]
    public void Default_Menu_Keyboard_Activation_Disabled_State_And_Escape_Are_Session_Safe(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var disabled = context.Items.Single(item => !item.IsEnabled);
            context.TryActivate(disabled).ShouldBeFalse();
            viewer.IsOverflowPopupOpen.ShouldBeTrue();

            var target = context.Items.Last(item => item.IsEnabled);
            var targetIndex = FindLogicalItemIndex(owner, target.Item);
            var menu = viewer.OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
            var menuItem = menu.GetVisualDescendants()
                               .OfType<TabOverflowMenuItem>()
                               .Single(item => ReferenceEquals(item.OverflowItem, target));

            menuItem.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Source = menuItem,
                Key = Key.Enter
            });
            Dispatcher.UIThread.RunJobs();

            GetSelectedIndex(owner).ShouldBe(targetIndex);
            viewer.IsOverflowPopupOpen.ShouldBeFalse();

            menu = OpenOverflow(owner).OverflowPopupRoot.ShouldBeOfType<TabOverflowMenu>();
            menu.RaiseEvent(new KeyEventArgs
            {
                RoutedEvent = InputElement.KeyDownEvent,
                Source = menu,
                Key = Key.Escape
            });
            Dispatcher.UIThread.RunJobs();
            viewer.IsOverflowPopupOpen.ShouldBeFalse();
        });
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Repeated_Ordinary_Open_Close_Reuses_One_Context_And_Visual_Root(OwnerKind kind)
    {
        var owner = CreateOwner(kind);

        ShowInWindow(owner, _ =>
        {
            var viewer = OpenOverflow(owner);
            var context = viewer.OverflowPopupContext.ShouldNotBeNull();
            var root = viewer.OverflowPopupRoot.ShouldNotBeNull();
            var openVisualCount = root.GetVisualDescendants().Count() + 1;
            context.Dismiss();
            Dispatcher.UIThread.RunJobs();

            for (var iteration = 0; iteration < 40; iteration++)
            {
                OpenOverflow(owner);
                viewer.OverflowPopupContext.ShouldBeSameAs(context);
                viewer.OverflowPopupRoot.ShouldBeSameAs(root);
                (root.GetVisualDescendants().Count() + 1).ShouldBe(openVisualCount);

                context.Dismiss();
                Dispatcher.UIThread.RunJobs();
                context.Items.ShouldBeEmpty();
                context.SelectedItem.ShouldBeNull();
            }
        });
    }

    [Fact]
    public void Saved_Closed_Context_Does_Not_Retain_Action_Target_Or_Its_Owner_Graph()
    {
        var release = CreateClosedContextTargetReference();

        CollectGarbage();

        release.Target.IsAlive.ShouldBeFalse();
        release.OwnerGraph.IsAlive.ShouldBeFalse();
        release.Context.Items.ShouldBeEmpty();
        release.Context.SelectedItem.ShouldBeNull();
    }

    [Theory]
    [MemberData(nameof(SharedViewerOwner))]
    public void Detached_Opened_Owner_Popup_And_Context_Are_Collectible(OwnerKind kind)
    {
        var references = CreateDetachedOverflowGraphReferences(kind);

        CollectGarbage();

        references.Owner.IsAlive.ShouldBeFalse();
        references.Viewer.IsAlive.ShouldBeFalse();
        references.Context.IsAlive.ShouldBeFalse();
        references.PopupRoot.IsAlive.ShouldBeFalse();
    }

    private static Control CreateOwner(
        OwnerKind kind,
        IDataTemplate? headerTemplate = null,
        bool isClosable = false,
        Dock placement = Dock.Top)
    {
        if (kind is OwnerKind.TabControl or OwnerKind.CardTabControl)
        {
            BaseTabControl owner = kind == OwnerKind.CardTabControl
                ? new CardTabControl()
                : new AtomUI.Desktop.Controls.TabControl();
            owner.Width = placement is Dock.Top or Dock.Bottom ? 220 : 360;
            owner.Height = placement is Dock.Left or Dock.Right ? 160 : 240;
            owner.SelectedIndex = 0;
            owner.IsTabClosable = isClosable;
            owner.IsMotionEnabled = false;
            owner.TabStripPlacement = placement;
            for (var i = 0; i < 12; i++)
            {
                owner.Items.Add(new AtomUI.Desktop.Controls.TabItem
                {
                    Header = $"Document {i + 1:00}",
                    HeaderTemplate = headerTemplate,
                    Content = $"Content {i + 1:00}",
                    IsClosable = isClosable && i % 2 == 0,
                    IsEnabled = i != 9
                });
            }
            return owner;
        }

        BaseTabStrip strip = kind == OwnerKind.CardTabStrip
            ? new CardTabStrip()
            : new AtomUI.Desktop.Controls.TabStrip();
        strip.Width = placement is Dock.Top or Dock.Bottom ? 220 : 360;
        strip.Height = placement is Dock.Left or Dock.Right ? 160 : 240;
        strip.SelectedIndex = 0;
        strip.IsTabClosable = isClosable;
        strip.IsMotionEnabled = false;
        strip.TabStripPlacement = placement;
        for (var i = 0; i < 12; i++)
        {
            strip.Items.Add(new TabStripItem
            {
                Content = $"Document {i + 1:00}",
                ContentTemplate = headerTemplate,
                IsClosable = isClosable && i % 2 == 0,
                IsEnabled = i != 9
            });
        }
        return strip;
    }

    private static TabScrollViewer OpenOverflow(Control owner)
    {
        var viewer = GetOverflowScrollViewer(owner);
        var indicator = GetMenuIndicator(viewer);
        indicator.IsVisible.ShouldBeTrue();
        indicator.RaiseEvent(new RoutedEventArgs(AvaloniaButton.ClickEvent, indicator));
        Dispatcher.UIThread.RunJobs();
        viewer.IsOverflowPopupOpen.ShouldBeTrue();
        return viewer;
    }

    private static IconButton GetMenuIndicator(TabScrollViewer viewer)
    {
        return viewer.GetVisualDescendants()
                     .OfType<IconButton>()
                     .Single(button => button.Name == "PART_ScrollMenuIndicator");
    }

    private static TabScrollViewer GetOverflowScrollViewer(Control owner)
    {
        var viewer = owner.GetVisualDescendants().OfType<TabScrollViewer>().ShouldHaveSingleItem();
        if (viewer.TabStripPlacement is Dock.Top or Dock.Bottom)
        {
            viewer.Extent.Width.ShouldBeGreaterThan(viewer.Viewport.Width);
        }
        else
        {
            viewer.Extent.Height.ShouldBeGreaterThan(viewer.Viewport.Height);
        }
        return viewer;
    }

    private static object? GetLogicalItem(Control owner, int index) => owner switch
    {
        BaseTabControl tabControl => tabControl.ItemsView[index],
        BaseTabStrip tabStrip => tabStrip.ItemsView[index],
        _ => throw new ArgumentOutOfRangeException(nameof(owner))
    };

    private static int FindLogicalItemIndex(Control owner, object? item) => owner switch
    {
        BaseTabControl tabControl => tabControl.ItemsView.IndexOf(item),
        BaseTabStrip tabStrip => tabStrip.ItemsView.IndexOf(item),
        _ => -1
    };

    private static Control GetContainer(Control owner, int index) => owner switch
    {
        BaseTabControl tabControl => tabControl.ContainerFromIndex(index).ShouldNotBeNull(),
        BaseTabStrip tabStrip => tabStrip.ContainerFromIndex(index).ShouldNotBeNull(),
        _ => throw new ArgumentOutOfRangeException(nameof(owner))
    };

    private static object? GetHeader(Control container) => container switch
    {
        AtomUI.Desktop.Controls.TabItem item => item.Header,
        TabStripItem item => item.Content,
        _ => null
    };

    private static IDataTemplate? GetHeaderTemplate(Control container) => container switch
    {
        AtomUI.Desktop.Controls.TabItem item => item.HeaderTemplate,
        TabStripItem item => item.ContentTemplate,
        _ => null
    };

    private static bool GetIsClosable(Control container) => container switch
    {
        AtomUI.Desktop.Controls.TabItem item => item.IsClosable,
        TabStripItem item => item.IsClosable,
        _ => false
    };

    private static int GetSelectedIndex(Control owner) => owner switch
    {
        BaseTabControl tabControl => tabControl.SelectedIndex,
        BaseTabStrip tabStrip => tabStrip.SelectedIndex,
        _ => -1
    };

    private static void SetSelectedIndex(Control owner, int index)
    {
        switch (owner)
        {
            case BaseTabControl tabControl:
                tabControl.SelectedIndex = index;
                break;
            case BaseTabStrip tabStrip:
                tabStrip.SelectedIndex = index;
                break;
        }
    }

    private static int GetItemCount(Control owner) => owner switch
    {
        BaseTabControl tabControl => tabControl.ItemCount,
        BaseTabStrip tabStrip => tabStrip.ItemCount,
        _ => 0
    };

    private static void AddItem(Control owner, string text)
    {
        switch (owner)
        {
            case BaseTabControl tabControl:
                tabControl.Items.Add(new AtomUI.Desktop.Controls.TabItem { Header = text, Content = text });
                break;
            case BaseTabStrip tabStrip:
                tabStrip.Items.Add(new TabStripItem { Content = text });
                break;
        }
    }

    private static void RemoveLastItem(Control owner)
    {
        switch (owner)
        {
            case BaseTabControl tabControl:
                tabControl.Items.RemoveAt(tabControl.ItemCount - 1);
                break;
            case BaseTabStrip tabStrip:
                tabStrip.Items.RemoveAt(tabStrip.ItemCount - 1);
                break;
        }
    }

    private static IDataTemplate? GetOverflowPopupTemplate(Control owner) => owner switch
    {
        BaseTabControl tabControl => tabControl.OverflowPopupTemplate,
        BaseTabStrip tabStrip => tabStrip.OverflowPopupTemplate,
        _ => null
    };

    private static void SetOverflowPopupTemplate(Control owner, IDataTemplate template)
    {
        switch (owner)
        {
            case BaseTabControl tabControl:
                tabControl.OverflowPopupTemplate = template;
                break;
            case BaseTabStrip tabStrip:
                tabStrip.OverflowPopupTemplate = template;
                break;
        }
    }

    private static void SetPinnedOpen(Control owner, bool value)
    {
        switch (owner)
        {
            case BaseTabControl tabControl:
                tabControl.IsPopupPinnedOpen = value;
                break;
            case BaseTabStrip tabStrip:
                tabStrip.IsPopupPinnedOpen = value;
                break;
        }
    }

    private static void AddCloseEventHandlers(Control owner, ICollection<string> order, bool cancel)
    {
        if (owner is BaseTabControl tabControl)
        {
            tabControl.Closing += (_, args) =>
            {
                order.Add("closing");
                args.Cancel = cancel;
            };
            tabControl.Closed += (_, _) => order.Add("closed");
        }
        else if (owner is BaseTabStrip tabStrip)
        {
            tabStrip.Closing += (_, args) =>
            {
                order.Add("closing");
                args.Cancel = cancel;
            };
            tabStrip.Closed += (_, _) => order.Add("closed");
        }
    }

    private static FuncDataTemplate<string> CreateHeaderTemplate()
    {
        return new FuncDataTemplate<string>((item, _) => new TextBlock { Text = item });
    }

    private static void ShowInWindow(Control content, Action<AvaloniaWindow> assertion)
    {
        ShowInWindowWithHost(content, (window, _) => assertion(window));
    }

    private static void ShowInWindowWithHost(
        Control content,
        Action<AvaloniaWindow, ScopeAwareOverlayLayerPanel> assertion)
    {
        var host = new ScopeAwareOverlayLayerPanel { Width = 420, Height = 320 };
        host.Children.Add(content);
        var window = new AvaloniaWindow
        {
            Width = 420,
            Height = 320,
            Content = CreatePopupOverlayHost(host)
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            assertion(window, host);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static ContextTargetReleaseResult CreateClosedContextTargetReference()
    {
        var context = new TabOverflowPopupContext();
        var ownerGraph = new object();
        var target = new RetainingActionTarget(ownerGraph);
        var targetReference = new WeakReference(target);
        var ownerGraphReference = new WeakReference(ownerGraph);

        context.OpenSession(1, Array.Empty<TabOverflowItem>(), null, target);
        context.CloseSession();

        return new ContextTargetReleaseResult(context, targetReference, ownerGraphReference);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static OverflowGraphReferences CreateDetachedOverflowGraphReferences(OwnerKind kind)
    {
        var template = new CapturingTemplate();
        var owner = CreateOwner(kind);
        SetOverflowPopupTemplate(owner, template);
        var host = new ScopeAwareOverlayLayerPanel { Width = 420, Height = 320 };
        host.Children.Add(owner);
        var window = new AvaloniaWindow
        {
            Width = 420,
            Height = 320,
            Content = CreatePopupOverlayHost(host)
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var viewer = OpenOverflow(owner);
        var context = viewer.OverflowPopupContext.ShouldNotBeNull();
        var root = viewer.OverflowPopupRoot.ShouldNotBeNull();
        var references = new OverflowGraphReferences(
            new WeakReference(owner),
            new WeakReference(viewer),
            new WeakReference(context),
            new WeakReference(root));

        host.Children.Remove(owner);
        Dispatcher.UIThread.RunJobs();
        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
        return references;
    }

    private static void CollectGarbage()
    {
        for (var iteration = 0; iteration < 3; iteration++)
        {
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
        }
    }

    private static VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var visualLayerManager = new VisualLayerManager { Child = content };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
        return visualLayerManager;
    }

    public enum OwnerKind
    {
        TabControl,
        CardTabControl,
        TabStrip,
        CardTabStrip
    }

    private sealed record ContextTargetReleaseResult(
        TabOverflowPopupContext Context,
        WeakReference Target,
        WeakReference OwnerGraph);

    private sealed record OverflowGraphReferences(
        WeakReference Owner,
        WeakReference Viewer,
        WeakReference Context,
        WeakReference PopupRoot);

    private sealed class RetainingActionTarget(object ownerGraph) : ITabOverflowPopupActionTarget
    {
        private readonly object _ownerGraph = ownerGraph;

        public bool TryActivate(long sessionId, TabOverflowItem item) => false;

        public bool TryClose(long sessionId, TabOverflowItem item) => false;

        public void Dismiss(long sessionId)
        {
            GC.KeepAlive(_ownerGraph);
        }
    }

    private sealed class CapturingTemplate : IDataTemplate
    {
        public int BuildCount { get; private set; }
        public Control? Root { get; private set; }

        public bool Match(object? data) => data is TabOverflowPopupContext;

        public Control Build(object? param)
        {
            param.ShouldBeOfType<TabOverflowPopupContext>();
            BuildCount++;
            return Root = new Border { Name = $"OverflowRoot{BuildCount}" };
        }
    }
}
