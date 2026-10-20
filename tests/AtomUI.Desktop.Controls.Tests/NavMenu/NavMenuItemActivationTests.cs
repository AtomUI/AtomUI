using System.Windows.Input;
using AtomUI.Animations;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AvaloniaWindow = Avalonia.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuItemActivationTests
{
    static NavMenuItemActivationTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Press_Only_Does_Not_Select_Or_Dispatch()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            Press(fixture.Window, fixture.SecondContainer);

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode,
                "press must not commit selection");
            fixture.SecondContainer.IsSelected.ShouldBeFalse();
            fixture.ClickedItems.ShouldBeEmpty();
            fixture.SelectedNodes.ShouldBeEmpty();
            fixture.Command.ExecuteCount.ShouldBe(0);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Press_And_Release_On_Same_Item_Commits_With_Order_Guarantee()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var order = new List<string>();
            fixture.Menu.NavMenuNodeSelected += (_, _) =>
            {
                order.Add("NodeSelected");
                fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode,
                    "event subscribers must observe the committed SelectedItem");
            };
            fixture.Command.ExecuteAction = () =>
                order.Add("Command:" + (fixture.Menu.SelectedItem == fixture.SecondNode ? "new" : "old"));
            fixture.Menu.NavMenuItemClick += (_, _) => order.Add("ItemClick");

            Click(fixture.Window, fixture.SecondContainer);

            order.ShouldBe(new[] { "NodeSelected", "Command:new", "ItemClick" });
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
            fixture.SecondContainer.IsSelected.ShouldBeTrue();
            fixture.FirstContainer.IsSelected.ShouldBeFalse();
            fixture.ClickedItems.Count.ShouldBe(1);
            fixture.ClickedItems[0].ShouldBe(fixture.SecondContainer);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Release_On_Different_Item_Does_Not_Commit()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            var thirdPoint  = CenterOf(fixture.ThirdContainer, fixture.Window);
            fixture.Window.MouseMove(secondPoint);
            fixture.Window.MouseDown(secondPoint, MouseButton.Left);
            fixture.Window.MouseMove(thirdPoint);
            fixture.Window.MouseUp(thirdPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.SecondContainer.IsSelected.ShouldBeFalse();
            fixture.ThirdContainer.IsSelected.ShouldBeFalse();
            fixture.ClickedItems.ShouldBeEmpty();
            fixture.Command.ExecuteCount.ShouldBe(0);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(0.9)]
    public void Press_Parent_Header_Release_On_Child_Cancels_Activation(double horizontalPosition)
    {
        var fixture = CreateInlineParentFixture();
        try
        {
            var parent = fixture.ParentContainer;
            parent.Open();
            Dispatcher.UIThread.RunJobs();
            var child = parent.ContainerFromIndex(0).ShouldBeOfType<NavMenuItem>();
            var parentHeader = GetHeader(parent);
            var childHeader = GetHeader(child);
            var parentPoint = parentHeader.TranslatePoint(
                new Point(parentHeader.Bounds.Width * horizontalPosition, parentHeader.Bounds.Height / 2),
                fixture.Window).ShouldNotBeNull();
            var childPoint = childHeader.TranslatePoint(
                new Point(childHeader.Bounds.Width * horizontalPosition, childHeader.Bounds.Height / 2),
                fixture.Window).ShouldNotBeNull();
            var expandedBounds = parent.Bounds;

            child.IsEffectivelyVisible.ShouldBeTrue();
            parent.IsVisualAncestorOf(child).ShouldBeTrue();
            childPoint.Y.ShouldBeGreaterThan(parentPoint.Y + parentHeader.Bounds.Height / 2);
            expandedBounds.Height.ShouldBeGreaterThan(parentHeader.Bounds.Height);

            fixture.Window.MouseMove(parentPoint);
            fixture.Window.MouseDown(parentPoint, MouseButton.Left);
            parent.IsPointerHold.ShouldBeTrue();
            fixture.Window.MouseMove(childPoint);
            parent.IsPointerHold.ShouldBeFalse("the child row is outside the pressed parent header");
            fixture.Window.MouseUp(childPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            parent.IsSubMenuOpen.ShouldBeTrue();
            parent.Bounds.ShouldBe(expandedBounds);
            child.IsEffectivelyVisible.ShouldBeTrue();
            parent.IsPointerHold.ShouldBeFalse();
            fixture.Menu.SelectedItem.ShouldBeNull();
            fixture.ClickedItems.ShouldBeEmpty();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Theory]
    [InlineData(0.1)]
    [InlineData(0.5)]
    [InlineData(0.9)]
    public void Expanded_Parent_Header_Click_Commits_Across_The_Row(double horizontalPosition)
    {
        var fixture = CreateInlineParentFixture();
        try
        {
            var parent = fixture.ParentContainer;
            parent.Open();
            Dispatcher.UIThread.RunJobs();
            var header = GetHeader(parent);
            var point = header.TranslatePoint(
                new Point(header.Bounds.Width * horizontalPosition, header.Bounds.Height / 2),
                fixture.Window).ShouldNotBeNull();

            fixture.Window.MouseMove(point);
            fixture.Window.MouseDown(point, MouseButton.Left);
            parent.IsPointerHold.ShouldBeTrue();
            parent.IsSubMenuOpen.ShouldBeTrue();
            fixture.ClickedItems.ShouldBeEmpty();
            fixture.Window.MouseUp(point, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            parent.IsSubMenuOpen.ShouldBeFalse();
            parent.IsPointerHold.ShouldBeFalse();
            fixture.Menu.SelectedItem.ShouldBeNull();
            fixture.ClickedItems.ShouldBe(new[] { (INavMenuItem)parent });
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Release_Below_Menu_Does_Not_Commit()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            var outsideMenu = new Point(secondPoint.X, 200);
            fixture.Window.MouseMove(secondPoint);
            fixture.Window.MouseDown(secondPoint, MouseButton.Left);
            fixture.Window.MouseMove(outsideMenu);
            fixture.Window.MouseUp(outsideMenu, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.SecondContainer.IsSelected.ShouldBeFalse();
            fixture.ClickedItems.ShouldBeEmpty();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Press_Drag_Out_And_Back_Then_Release_Commits_Once()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            var thirdPoint  = CenterOf(fixture.ThirdContainer, fixture.Window);
            fixture.Window.MouseMove(secondPoint);
            fixture.Window.MouseDown(secondPoint, MouseButton.Left);
            fixture.Window.MouseMove(thirdPoint);
            Dispatcher.UIThread.RunJobs();
            fixture.Window.MouseMove(secondPoint);
            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
            fixture.SecondContainer.IsSelected.ShouldBeTrue();
            fixture.ClickedItems.Count.ShouldBe(1);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Repeated_Commit_On_Same_Node_Raises_Selected_Once_And_Click_Every_Time()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            Click(fixture.Window, fixture.SecondContainer);
            Click(fixture.Window, fixture.SecondContainer);

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
            fixture.SelectedNodes.Count.ShouldBe(1,
                "re-committing the same node must not re-raise NavMenuNodeSelected");
            fixture.ClickedItems.Count.ShouldBe(2,
                "every legal activation raises NavMenuItemClick");
            fixture.Command.ExecuteCount.ShouldBe(2);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Selection_Redirected_During_Commit_Does_Not_Publish_Or_Invoke_The_Superseded_Item()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            fixture.Menu.PropertyChanged += (_, args) =>
            {
                if (args.Property == AtomUI.Desktop.Controls.NavMenu.SelectedItemProperty &&
                    ReferenceEquals(args.NewValue, fixture.SecondNode))
                {
                    fixture.Menu.SelectedItem = fixture.ThirdNode;
                }
            };

            Click(fixture.Window, fixture.SecondContainer);

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.ThirdNode);
            fixture.ThirdContainer.IsSelected.ShouldBeTrue();
            fixture.SelectedNodes.Count.ShouldBe(1,
                "the superseded second-node commit must not publish a stale selected event");
            fixture.SelectedNodes[0].ShouldBeSameAs(fixture.ThirdNode);
            fixture.Command.ExecuteCount.ShouldBe(0,
                "a superseded activation must not execute only part of its invocation pipeline");
            fixture.ClickedItems.ShouldBeEmpty(
                "a superseded activation must not raise NavMenuItemClick");
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Selection_Redirected_By_Selected_Event_Stops_The_Original_Invocation()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            fixture.Menu.NavMenuNodeSelected += (_, args) =>
            {
                if (ReferenceEquals(args.NavMenuNode, fixture.SecondNode))
                {
                    fixture.Menu.SelectedItem = fixture.ThirdNode;
                }
            };

            Click(fixture.Window, fixture.SecondContainer);

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.ThirdNode);
            fixture.Command.ExecuteCount.ShouldBe(0,
                "selection-event reentrancy must stop the superseded node's command");
            fixture.ClickedItems.ShouldBeEmpty(
                "selection-event reentrancy must stop the superseded node's item-click event");
            fixture.SelectedNodes.Count.ShouldBe(2);
            fixture.SelectedNodes[0].ShouldBeSameAs(fixture.SecondNode);
            fixture.SelectedNodes[1].ShouldBeSameAs(fixture.ThirdNode);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Unrelated_Capture_Lost_Does_Not_Cancel_The_Transaction()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            Press(fixture.Window, fixture.SecondContainer);
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue();

            var secondHeader = GetHeader(fixture.SecondContainer);
            secondHeader.RaiseEvent(new PointerCaptureLostEventArgs(
                secondHeader,
                new Avalonia.Input.Pointer(Avalonia.Input.Pointer.GetNextFreeId(), PointerType.Mouse, true)));
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsPointerHold.ShouldBeTrue(
                "capture loss for a different pointer must not terminate the active pointer transaction");

            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
            fixture.ClickedItems.Count.ShouldBe(1);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Captured_Pointer_Lost_Cancels_The_Transaction()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var pointer = BeginSyntheticPress(fixture, fixture.SecondContainer);
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue();

            var secondHeader = GetHeader(fixture.SecondContainer);
            secondHeader.RaiseEvent(new PointerCaptureLostEventArgs(
                secondHeader,
                pointer));
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsPointerHold.ShouldBeFalse();
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.ClickedItems.ShouldBeEmpty();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Transferring_Pointer_Capture_Cancels_Activation_And_Preserves_New_Owner()
    {
        var fixture = CreateInlineLeafFixture();
        IPointer? pointer = null;
        try
        {
            pointer = BeginSyntheticPress(fixture, fixture.SecondContainer);
            var pressedHeader = GetHeader(fixture.SecondContainer);
            var newCaptureOwner = GetHeader(fixture.ThirdContainer);
            pointer.Captured.ShouldBeSameAs(pressedHeader);

            pointer.Capture(newCaptureOwner);

            fixture.SecondContainer.IsPointerHold.ShouldBeFalse();
            pointer.Captured.ShouldBeSameAs(newCaptureOwner);
            pressedHeader.RaiseEvent(new PointerReleasedEventArgs(
                pressedHeader,
                pointer,
                fixture.Window,
                CenterOf(fixture.SecondContainer, fixture.Window),
                101,
                new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.LeftButtonReleased),
                KeyModifiers.None,
                MouseButton.Left));
            Dispatcher.UIThread.RunJobs();

            pointer.Captured.ShouldBeSameAs(newCaptureOwner);
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.SelectedNodes.ShouldBeEmpty();
            fixture.ClickedItems.ShouldBeEmpty();
            fixture.Command.ExecuteCount.ShouldBe(0);
        }
        finally
        {
            pointer?.Capture(null);
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Right_Button_Press_Does_Not_Start_Transaction()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            fixture.Window.MouseMove(secondPoint);
            fixture.Window.MouseDown(secondPoint, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();
            fixture.Window.MouseUp(secondPoint, MouseButton.Right);
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsPointerHold.ShouldBeFalse();
            fixture.SecondContainer.IsSelected.ShouldBeFalse();
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.ClickedItems.ShouldBeEmpty();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Disabled_After_Press_Does_Not_Commit()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            Press(fixture.Window, fixture.SecondContainer);

            fixture.SecondNode.IsEnabled = false;
            Dispatcher.UIThread.RunJobs();

            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.ClickedItems.ShouldBeEmpty();
            fixture.Command.ExecuteCount.ShouldBe(0);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Inline_Parent_Toggles_Submenu_On_Release_Only()
    {
        var fixture = CreateInlineParentFixture();
        try
        {
            Press(fixture.Window, fixture.ParentContainer);
            fixture.ParentContainer.IsSubMenuOpen.ShouldBeFalse(
                "press must not toggle the submenu");

            ReleaseAt(fixture.Window, fixture.ParentContainer);
            fixture.ParentContainer.IsSubMenuOpen.ShouldBeTrue(
                "a legal release toggles the submenu open");

            Click(fixture.Window, fixture.ParentContainer);
            fixture.ParentContainer.IsSubMenuOpen.ShouldBeFalse(
                "a second legal release toggles the submenu closed");
            fixture.ClickedItems.Count.ShouldBe(2,
                "every parent activation raises NavMenuItemClick");
            fixture.Menu.SelectedItem.ShouldBeNull(
                "parent activation must not change SelectedItem");
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Default_Parent_Opens_Popup_On_Release()
    {
        var fixture = CreateVerticalParentFixture();
        try
        {
            Press(fixture.Window, fixture.ParentContainer);
            fixture.ParentContainer.IsSubMenuOpen.ShouldBeFalse(
                "press must not open the popup");

            ReleaseAt(fixture.Window, fixture.ParentContainer);
            RunDispatcherJobsUntil(() => fixture.ParentContainer.IsSubMenuOpen);
            fixture.ParentContainer.IsSubMenuOpen.ShouldBeTrue(
                "a legal release opens the popup");
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Keyboard_Enter_Commits_Leaf_With_Same_Order_As_Pointer()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var order = new List<string>();
            fixture.Menu.NavMenuNodeSelected += (_, _) =>
            {
                order.Add("NodeSelected");
                fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode,
                    "keyboard commit must expose the new SelectedItem to event subscribers");
            };
            fixture.Command.ExecuteAction = () =>
                order.Add("Command:" + (fixture.Menu.SelectedItem == fixture.SecondNode ? "new" : "old"));

            fixture.Menu.Focus(NavigationMethod.Tab).ShouldBeTrue();
            PressKey(fixture.Window, Key.Down, PhysicalKey.ArrowDown);
            PressKey(fixture.Window, Key.Enter, PhysicalKey.Enter);

            order.ShouldBe(new[] { "NodeSelected", "Command:new" });
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
            fixture.ClickedItems.Count.ShouldBe(1);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Keyboard_Enter_On_Inline_Parent_Toggles_And_Clicks()
    {
        var fixture = CreateInlineParentFixture();
        try
        {
            fixture.Menu.SelectedItem.ShouldBeNull();
            fixture.Menu.Focus(NavigationMethod.Tab).ShouldBeTrue();
            PressKey(fixture.Window, Key.Down, PhysicalKey.ArrowDown);
            PressKey(fixture.Window, Key.Enter, PhysicalKey.Enter);

            fixture.ParentContainer.IsSubMenuOpen.ShouldBeTrue(
                "Enter on an inline parent toggles the submenu open");
            fixture.ClickedItems.Count.ShouldBe(1,
                "Enter on a parent dispatches NavMenuItemClick once");
            fixture.Menu.SelectedItem.ShouldBeNull(
                "parent activation must not change SelectedItem");
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Pointer_Transaction_Does_Not_Replace_Keyboard_Active_Item()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            fixture.Menu.Focus(NavigationMethod.Tab).ShouldBeTrue();
            PressKey(fixture.Window, Key.Down, PhysicalKey.ArrowDown);
            fixture.SecondContainer.IsKeyboardActive.ShouldBeTrue(
                "keyboard navigation must establish its own active item");

            Press(fixture.Window, fixture.ThirdContainer);
            fixture.SecondContainer.IsKeyboardActive.ShouldBeTrue(
                "pointer press must not replace the keyboard navigation owner");
            fixture.ThirdContainer.IsPointerHold.ShouldBeTrue(
                "the pointer candidate must own only the hold visual");

            var outsideMenu = new Point(CenterOf(fixture.ThirdContainer, fixture.Window).X, 200);
            fixture.Window.MouseMove(outsideMenu);
            fixture.Window.MouseUp(outsideMenu, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsKeyboardActive.ShouldBeTrue(
                "cancelling the pointer transaction must preserve keyboard state");
            fixture.ThirdContainer.IsPointerHold.ShouldBeFalse();
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Press_Focuses_Item_When_Mode_Allows_Item_Focus()
    {
        var vertical = CreateVerticalParentFixture();
        try
        {
            var leafFixture = vertical;
            Press(leafFixture.Window, leafFixture.ParentContainer);
            leafFixture.ParentContainer.IsFocused.ShouldBeTrue(
                "press must move real focus to the pressed item in modes where items are focusable");
        }
        finally
        {
            vertical.Window.Close();
        }
    }

    [Fact]
    public void Pointer_Hold_Uses_Selected_Background_Without_Changing_Foreground_Or_Selection()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            fixture.Window.MouseMove(secondPoint);
            Dispatcher.UIThread.RunJobs();

            var secondHeader = GetHeader(fixture.SecondContainer);
            secondHeader.IsPointerOver.ShouldBeTrue();
            var hoverBackground = secondHeader.Background;
            var hoverForeground = secondHeader.Foreground;
            var selectedHeader  = GetHeader(fixture.FirstContainer);
            var selectedBackground = selectedHeader.Background;
            var selectedForeground = selectedHeader.Foreground;
            selectedBackground.ShouldNotBe(hoverBackground,
                "the reference pressed background is the selected background, not hover");
            selectedForeground.ShouldNotBe(hoverForeground,
                "the test must distinguish the unchanged hover foreground from selected foreground");

            fixture.Window.MouseDown(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsKeyboardActive.ShouldBeFalse(
                "pointer hold must not mutate the independent keyboard navigation state");
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue();
            secondHeader.Background.ShouldBe(selectedBackground,
                "pressed background must match selected while selection remains uncommitted");
            secondHeader.Foreground.ShouldBe(hoverForeground,
                "pressed must not change the text color");
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.SecondContainer.IsSelected.ShouldBeFalse();

            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsPointerHold.ShouldBeFalse();
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Item_Background_Transition_Uses_Overridden_NavMenu_Easing_Token()
    {
        var node = new NavMenuNode { Header = "Item", ItemKey = "item" };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true
        };
        menu.Items.Add(node);

        var expectedEasing = new LinearEasing();
        var window = new AvaloniaWindow { Width = 320, Height = 240, Content = menu };
        window.Resources[NavMenuTokenKind.ItemBackgroundMotionEasing] = expectedEasing;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            var transition = GetHeader(container).Transitions.ShouldNotBeNull()
                                      .OfType<SolidColorBrushTransition>()
                                      .Single(item => item.Property == BaseNavMenuItemHeader.BackgroundProperty);

            transition.Easing.ShouldBeOfType<LinearEasing>();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Pointer_Transaction_Does_Not_Expose_An_Intermediate_Background()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            fixture.Window.MouseMove(secondPoint);
            Dispatcher.UIThread.RunJobs();

            var secondHeader = GetHeader(fixture.SecondContainer);
            var expectedPressedBackground = GetHeader(fixture.FirstContainer).Background;
            var backgroundChanges = new List<object?>();
            secondHeader.PropertyChanged += (_, args) =>
            {
                if (args.Property == BaseNavMenuItemHeader.BackgroundProperty)
                {
                    backgroundChanges.Add(args.NewValue);
                }
            };

            fixture.Window.MouseDown(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            backgroundChanges.ShouldAllBe(background => Equals(background, expectedPressedBackground),
                "press must transition directly from hover to the selected-background hold visual");
            secondHeader.Background.ShouldBe(expectedPressedBackground);

            backgroundChanges.Clear();
            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            var selectedBackground = secondHeader.Background;
            backgroundChanges.ShouldAllBe(background => Equals(background, selectedBackground),
                "release must preserve the selected-background hold visual until logical selection commits");
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.SecondNode);
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Pointer_Hold_Captures_The_Hand_Cursor_Header()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondHeader = GetHeader(fixture.SecondContainer);
            secondHeader.Cursor.ShouldNotBeNull().ToString().ShouldContain("Hand");

            var pointer = BeginSyntheticPress(fixture, fixture.SecondContainer);

            pointer.Captured.ShouldBeSameAs(secondHeader,
                "the capture owner determines the cursor shown for the duration of pointer hold");
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void Handler_Replacement_Cancels_The_Pointer_Transaction()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            Press(fixture.Window, fixture.SecondContainer);
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue();
            var releasePoint = CenterOf(fixture.SecondContainer, fixture.Window);

            fixture.Menu.Mode = NavMenuMode.Vertical;
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsPointerHold.ShouldBeFalse(
                "detaching the old handler must clear its pointer-owned hold state");
            fixture.Window.MouseUp(releasePoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode);
            fixture.ClickedItems.ShouldBeEmpty();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void PointerHold_Visual_State_Lifecycle()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            var thirdPoint  = CenterOf(fixture.ThirdContainer, fixture.Window);

            fixture.Window.MouseMove(secondPoint);
            fixture.Window.MouseDown(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue(
                "press must set the transaction hold visual");
            fixture.SecondContainer.IsSelected.ShouldBeFalse();

            fixture.Window.MouseMove(thirdPoint);
            Dispatcher.UIThread.RunJobs();
            fixture.SecondContainer.IsPointerHold.ShouldBeFalse(
                "dragging away must clear the transaction hold visual");

            fixture.Window.MouseMove(secondPoint);
            Dispatcher.UIThread.RunJobs();
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue(
                "moving back must restore the transaction hold visual");

            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();
            fixture.SecondContainer.IsPointerHold.ShouldBeFalse(
                "commit must end the transaction hold visual");
            fixture.SecondContainer.IsSelected.ShouldBeTrue();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    [Fact]
    public void New_Press_Replaces_Old_Transaction()
    {
        var fixture = CreateInlineLeafFixture();
        try
        {
            Press(fixture.Window, fixture.SecondContainer);
            fixture.SecondContainer.IsPointerHold.ShouldBeTrue();

            var thirdHeader = fixture.ThirdContainer.ItemHeader
                              ?? throw new InvalidOperationException("item header is not realized");
            var syntheticSource = thirdHeader.GetVisualDescendants()
                                      .OfType<Control>()
                                      .FirstOrDefault(control => control.IsHitTestVisible)
                                  ?? (Control)thirdHeader;
            var syntheticPoint = thirdHeader.TranslatePoint(
                new Point(thirdHeader.Bounds.Width / 2, thirdHeader.Bounds.Height / 2),
                fixture.Window).ShouldNotBeNull();
            var syntheticPointer = new Avalonia.Input.Pointer(
                Avalonia.Input.Pointer.GetNextFreeId(), PointerType.Mouse, true);
            var pressArgs = new PointerPressedEventArgs(
                syntheticSource,
                syntheticPointer,
                fixture.Window,
                syntheticPoint,
                100,
                new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
                KeyModifiers.None);
            syntheticSource.RaiseEvent(pressArgs);
            Dispatcher.UIThread.RunJobs();

            fixture.SecondContainer.IsPointerHold.ShouldBeFalse(
                "a new press must cancel the previous transaction visual");
            fixture.ThirdContainer.IsPointerHold.ShouldBeTrue(
                "a new press must start a transaction on the new target");

            var secondPoint = CenterOf(fixture.SecondContainer, fixture.Window);
            fixture.Window.MouseUp(secondPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            fixture.Menu.SelectedItem.ShouldBeSameAs(fixture.FirstNode,
                "a release from the replaced pointer must not commit");
            fixture.ClickedItems.ShouldBeEmpty();
        }
        finally
        {
            fixture.Window.Close();
        }
    }

    #region fixtures and helpers

    private static BaseNavMenuItemHeader GetHeader(NavMenuItem item) =>
        item.ItemHeader as BaseNavMenuItemHeader
        ?? throw new InvalidOperationException("item header is not realized");

    private sealed class LeafFixture
    {
        public required AvaloniaWindow Window;
        public required AtomUI.Desktop.Controls.NavMenu Menu;
        public required NavMenuNode FirstNode;
        public required NavMenuNode SecondNode;
        public required NavMenuNode ThirdNode;
        public required NavMenuItem FirstContainer;
        public required NavMenuItem SecondContainer;
        public required NavMenuItem ThirdContainer;
        public required TrackingCommand Command;
        public required List<INavMenuItem> ClickedItems;
        public required List<INavMenuNode> SelectedNodes;
    }

    private sealed class ParentFixture
    {
        public required AvaloniaWindow Window;
        public required AtomUI.Desktop.Controls.NavMenu Menu;
        public required NavMenuItem ParentContainer;
        public required List<INavMenuItem> ClickedItems;
    }

    private static LeafFixture CreateInlineLeafFixture()
    {
        var command = new TrackingCommand();
        var first  = new NavMenuNode { Header = "First", ItemKey = "first" };
        var second = new NavMenuNode { Header = "Second", ItemKey = "second", Command = command };
        var third  = new NavMenuNode { Header = "Third", ItemKey = "third" };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(first);
        menu.Items.Add(second);
        menu.Items.Add(third);

        var clickedItems  = new List<INavMenuItem>();
        var selectedNodes = new List<INavMenuNode>();

        var window = new AvaloniaWindow { Width = 320, Height = 240, Content = menu };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        menu.SelectedItem = first;
        Dispatcher.UIThread.RunJobs();

        menu.NavMenuItemClick += (_, args) => clickedItems.Add(args.NavMenuItem);
        menu.NavMenuNodeSelected += (_, args) => selectedNodes.Add(args.NavMenuNode);

        return new LeafFixture
        {
            Window          = window,
            Menu            = menu,
            FirstNode       = first,
            SecondNode      = second,
            ThirdNode       = third,
            FirstContainer  = menu.ContainerFromItem(first).ShouldBeOfType<NavMenuItem>(),
            SecondContainer = menu.ContainerFromItem(second).ShouldBeOfType<NavMenuItem>(),
            ThirdContainer  = menu.ContainerFromItem(third).ShouldBeOfType<NavMenuItem>(),
            Command         = command,
            ClickedItems    = clickedItems,
            SelectedNodes   = selectedNodes
        };
    }

    private static ParentFixture CreateInlineParentFixture()
    {
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(new NavMenuNode { Header = "Child", ItemKey = "child" });
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(parent);

        var clickedItems = new List<INavMenuItem>();
        menu.NavMenuItemClick += (_, args) => clickedItems.Add(args.NavMenuItem);

        var window = new AvaloniaWindow { Width = 320, Height = 240, Content = menu };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return new ParentFixture
        {
            Window          = window,
            Menu            = menu,
            ParentContainer = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>(),
            ClickedItems    = clickedItems
        };
    }

    private static ParentFixture CreateVerticalParentFixture()
    {
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(new NavMenuNode { Header = "Child", ItemKey = "child" });
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Vertical,
            IsMotionEnabled = false
        };
        menu.Items.Add(parent);

        var clickedItems = new List<INavMenuItem>();
        menu.NavMenuItemClick += (_, args) => clickedItems.Add(args.NavMenuItem);

        var visualLayerManager = new Avalonia.Controls.Primitives.VisualLayerManager { Child = menu };
        var overlayProperty = typeof(Avalonia.Controls.Primitives.VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        overlayProperty.ShouldNotBeNull();
        overlayProperty.SetValue(visualLayerManager, true);

        var window = new AvaloniaWindow { Width = 320, Height = 240, Content = visualLayerManager };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        return new ParentFixture
        {
            Window          = window,
            Menu            = menu,
            ParentContainer = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>(),
            ClickedItems    = clickedItems
        };
    }

    private static Point CenterOf(Control control, AvaloniaWindow window)
    {
        var header = control.ShouldBeOfType<NavMenuItem>().ItemHeader
                     ?? throw new InvalidOperationException("item header is not realized");
        return header.TranslatePoint(
            new Point(header.Bounds.Width / 2, header.Bounds.Height / 2),
            window).ShouldNotBeNull();
    }

    private static void Press(AvaloniaWindow window, Control control)
    {
        var point = CenterOf(control, window);
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static IPointer BeginSyntheticPress(LeafFixture fixture, NavMenuItem item)
    {
        var header = item.ItemHeader
                     ?? throw new InvalidOperationException("item header is not realized");
        var source = header.GetVisualDescendants()
                         .OfType<Control>()
                         .FirstOrDefault(control => control.IsHitTestVisible)
                     ?? (Control)header;
        var point = header.TranslatePoint(
            new Point(header.Bounds.Width / 2, header.Bounds.Height / 2),
            fixture.Window).ShouldNotBeNull();
        var pointer = new Avalonia.Input.Pointer(
            Avalonia.Input.Pointer.GetNextFreeId(), PointerType.Mouse, true);
        source.RaiseEvent(new PointerPressedEventArgs(
            source,
            pointer,
            fixture.Window,
            point,
            100,
            new PointerPointProperties(RawInputModifiers.LeftMouseButton, PointerUpdateKind.LeftButtonPressed),
            KeyModifiers.None));
        Dispatcher.UIThread.RunJobs();
        return pointer;
    }

    private static void ReleaseAt(AvaloniaWindow window, Control control)
    {
        var point = CenterOf(control, window);
        window.MouseMove(point);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void Click(AvaloniaWindow window, Control control)
    {
        var point = CenterOf(control, window);
        window.MouseMove(point);
        window.MouseDown(point, MouseButton.Left);
        window.MouseUp(point, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void PressKey(AvaloniaWindow window, Key key, PhysicalKey physicalKey)
    {
        window.KeyPress(key, RawInputModifiers.None, physicalKey, null);
        Dispatcher.UIThread.RunJobs();
    }

    private static void RunDispatcherJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses && !condition(); i++)
        {
            Dispatcher.UIThread.RunJobs();
        }
    }

    private sealed class TrackingCommand : ICommand
    {
        public int ExecuteCount { get; private set; }
        public Action? ExecuteAction { get; set; }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            ExecuteCount++;
            ExecuteAction?.Invoke();
        }

        public event EventHandler? CanExecuteChanged
        {
            add    { }
            remove { }
        }
    }

    #endregion
}
