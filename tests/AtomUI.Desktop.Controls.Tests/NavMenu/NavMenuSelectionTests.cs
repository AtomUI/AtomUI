using System.Reflection;
using AtomUI.Controls.Primitives;
using MotionApi = AtomUI.Animations.Motion;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuSelectionTests
{
    static NavMenuSelectionTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Loaded_Selection_Wins_Over_Queued_Default(bool queueExplicitSelection)
    {
        var first = new NavMenuNode { Header = "First", ItemKey = "first" };
        var second = new NavMenuNode { Header = "Second", ItemKey = "second" };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode = NavMenuMode.Inline,
            IsMotionEnabled = false,
            DefaultSelectedPath = new TreeNodePath("first")
        };
        menu.Items.Add(first);
        menu.Items.Add(second);
        var selectedNodes = new List<INavMenuNode>();
        menu.NavMenuNodeSelected += (_, args) => selectedNodes.Add(args.NavMenuNode);
        menu.Loaded += (_, _) =>
        {
            if (queueExplicitSelection)
            {
                Dispatcher.UIThread.Post(() => menu.SelectedItem = second, DispatcherPriority.Loaded);
            }
            else
            {
                menu.SelectedItem = second;
            }
        };
        var window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(second);
            selectedNodes.ShouldBe([second]);
            menu.ContainerFromItem(first).ShouldBeOfType<NavMenuItem>().IsSelected.ShouldBeFalse();
            menu.ContainerFromItem(second).ShouldBeOfType<NavMenuItem>().IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData("clear", false)]
    [InlineData("root", false)]
    [InlineData("sibling", false)]
    [InlineData("branch", false)]
    [InlineData("clear", true)]
    [InlineData("root", true)]
    [InlineData("branch", true)]
    public void Removing_Selected_Entry_Reconciles_The_Previously_Applied_Ancestor_Path(
        string nextSelection, bool removeParentBranch)
    {
        var first = new NavMenuNode { Header = "First", ItemKey = "first" };
        var sibling = new NavMenuNode { Header = "Sibling", ItemKey = "sibling" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(first);
        parent.Children.Add(sibling);
        var otherLeaf = new NavMenuNode { Header = "Other leaf", ItemKey = "other-leaf" };
        var otherBranch = new NavMenuNode { Header = "Other branch", ItemKey = "other-branch" };
        otherBranch.Children.Add(otherLeaf);
        var ancestor = new NavMenuNode { Header = "Ancestor", ItemKey = "ancestor" };
        ancestor.Children.Add(parent);
        ancestor.Children.Add(otherBranch);
        var rootLeaf = new NavMenuNode { Header = "Root leaf", ItemKey = "root-leaf" };
        var menu = new AtomUI.Desktop.Controls.NavMenu { Mode = NavMenuMode.Inline, IsMotionEnabled = false };
        menu.Items.Add(ancestor);
        menu.Items.Add(rootLeaf);
        var window = new Avalonia.Controls.Window { Width = 360, Height = 480, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.SelectedItem = first;
            Dispatcher.UIThread.RunJobs();
            var ancestorItem = menu.ContainerFromItem(ancestor).ShouldBeOfType<NavMenuItem>();
            var parentItem = ancestorItem.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            var firstItem = parentItem.ContainerFromItem(first).ShouldBeOfType<NavMenuItem>();
            ancestorItem.IsInSelectedPath.ShouldBeTrue();
            parentItem.IsInSelectedPath.ShouldBeTrue();
            firstItem.IsSelected.ShouldBeTrue();

            if (removeParentBranch)
            {
                ancestor.Children.Remove(parent);
            }
            else
            {
                parent.Children.Remove(first);
            }
            Dispatcher.UIThread.RunJobs();
            // Removal does not rewrite the public selection request.
            menu.SelectedItem.ShouldBeSameAs(first);
            INavMenuNode? nextNode = nextSelection switch
            {
                "root" => rootLeaf,
                "sibling" => sibling,
                "branch" => otherLeaf,
                _ => null
            };
            menu.SelectedItem = nextNode;
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeSameAs(nextNode);
            firstItem.IsSelected.ShouldBeFalse();
            ancestorItem.IsInSelectedPath.ShouldBe(nextSelection is "sibling" or "branch");
            parentItem.IsInSelectedPath.ShouldBe(nextSelection == "sibling");
            GetItemHeader(ancestorItem).IsInSelectedPath.ShouldBe(nextSelection is "sibling" or "branch");
            if (nextNode is not null)
            {
                menu.FindRealizedMenuItem(nextNode).ShouldNotBeNull().IsSelected.ShouldBeTrue();
            }
            if (nextSelection == "branch")
            {
                ancestorItem.ContainerFromItem(otherBranch).ShouldBeOfType<NavMenuItem>()
                    .IsInSelectedPath.ShouldBeTrue();
            }
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clearing_Selected_Path_Tolerates_Synchronous_Container_Recycle_Or_Selection(bool selectAnotherItem)
    {
        var leaf = new NavMenuNode { Header = "Leaf", ItemKey = "leaf" };
        var branch = new NavMenuNode { Header = "Branch", ItemKey = "branch" };
        branch.Children.Add(leaf);
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(branch);
        var other = new NavMenuNode { Header = "Other", ItemKey = "other" };
        var menu = new AtomUI.Desktop.Controls.NavMenu { Mode = NavMenuMode.Inline, IsMotionEnabled = false };
        menu.Items.Add(parent);
        menu.Items.Add(other);
        var window = new Avalonia.Controls.Window { Width = 360, Height = 400, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.SelectedItem = leaf;
            Dispatcher.UIThread.RunJobs();
            var parentItem = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            var branchItem = parentItem.ContainerFromItem(branch).ShouldBeOfType<NavMenuItem>();
            var leafItem = branchItem.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>();
            var otherItem = menu.ContainerFromItem(other).ShouldBeOfType<NavMenuItem>();
            parentItem.PropertyChanged += (_, args) =>
            {
                if (args.Property == NavMenuItem.IsInSelectedPathProperty && args.NewValue is false)
                {
                    if (selectAnotherItem)
                    {
                        menu.SelectNavMenuItem(otherItem);
                    }
                    else
                    {
                        parent.Children.Remove(branch);
                    }
                }
            };

            Should.NotThrow(() => menu.SelectedItem = null);
            Dispatcher.UIThread.RunJobs();

            parentItem.IsInSelectedPath.ShouldBeFalse();
            branchItem.IsInSelectedPath.ShouldBeFalse();
            leafItem.IsSelected.ShouldBeFalse();
            menu.SelectedItem.ShouldBeSameAs(selectAnotherItem ? other : null);
            otherItem.IsSelected.ShouldBe(selectAnotherItem);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Selection_Event_Can_Remove_The_Selected_Leaf_And_Select_Another_Branch()
    {
        var leaf = new NavMenuNode { Header = "Leaf", ItemKey = "leaf" };
        var sibling = new NavMenuNode { Header = "Sibling", ItemKey = "sibling" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(leaf);
        parent.Children.Add(sibling);
        var other = new NavMenuNode { Header = "Other", ItemKey = "other" };
        var menu = new AtomUI.Desktop.Controls.NavMenu { Mode = NavMenuMode.Inline, IsMotionEnabled = false };
        menu.Items.Add(parent);
        menu.Items.Add(other);
        var selectedNodes = new List<INavMenuNode>();
        menu.NavMenuNodeSelected += (_, args) =>
        {
            selectedNodes.Add(args.NavMenuNode);
            if (ReferenceEquals(args.NavMenuNode, leaf))
            {
                parent.Children.Remove(leaf);
                menu.SelectedItem = other;
            }
        };
        var window = new Avalonia.Controls.Window { Width = 360, Height = 400, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.SelectedItem = leaf;
            Dispatcher.UIThread.RunJobs();

            selectedNodes.ShouldBe([leaf, other]);
            menu.SelectedItem.ShouldBeSameAs(other);
            menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>().IsInSelectedPath.ShouldBeFalse();
            menu.ContainerFromItem(other).ShouldBeOfType<NavMenuItem>().IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Recreated_Or_Reattached_Selection_Path_Is_Cleared_After_Selected_Leaf_Removal(bool rebuildContainers)
    {
        var leaf = new NavMenuNode { Header = "Leaf", ItemKey = "leaf" };
        var sibling = new NavMenuNode { Header = "Sibling", ItemKey = "sibling" };
        var parent = new NavMenuNode { Header = "Parent", ItemKey = "parent" };
        parent.Children.Add(leaf);
        parent.Children.Add(sibling);
        var menu = new AtomUI.Desktop.Controls.NavMenu { Mode = NavMenuMode.Inline, IsMotionEnabled = false };
        menu.Items.Add(parent);
        var content = CreatePopupOverlayHost(menu);
        var window = new Avalonia.Controls.Window { Width = 360, Height = 400, Content = content };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            menu.SelectedItem = leaf;
            Dispatcher.UIThread.RunJobs();
            var originalParent = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            originalParent.IsInSelectedPath.ShouldBeTrue();

            if (rebuildContainers)
            {
                menu.Mode = NavMenuMode.Horizontal;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                menu.Mode = NavMenuMode.Inline;
            }
            else
            {
                window.Content = null;
                Dispatcher.UIThread.RunJobs();
                window.Content = content;
            }
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var currentParent = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            currentParent.IsInSelectedPath.ShouldBeTrue();
            menu.SelectedItem.ShouldBeSameAs(leaf);
            currentParent.Open();
            Dispatcher.UIThread.RunJobs();
            currentParent.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>().IsSelected.ShouldBeTrue();

            parent.Children.Remove(leaf);
            Dispatcher.UIThread.RunJobs();
            menu.SelectedItem = null;
            Dispatcher.UIThread.RunJobs();
            currentParent.IsInSelectedPath.ShouldBeFalse();
            originalParent.IsInSelectedPath.ShouldBeFalse();

            parent.Children.Add(leaf);
            Dispatcher.UIThread.RunJobs();
            menu.SelectedItem = leaf;
            Dispatcher.UIThread.RunJobs();
            currentParent.IsInSelectedPath.ShouldBeTrue();
            currentParent.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>().IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void NavMenu_Ignores_Stale_Async_SelectedItem_Replay()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        var first = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        var second = new NavMenuNode
        {
            Header  = "Second",
            ItemKey = "second"
        };
        menu.Items.Add(first);
        menu.Items.Add(second);

        var selectedNodes = new List<INavMenuNode>();
        menu.NavMenuNodeSelected += (_, args) => selectedNodes.Add(args.NavMenuNode);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem = first;
            menu.SelectedItem = second;
            Dispatcher.UIThread.RunJobs();

            selectedNodes.ShouldBe([second]);
            menu.SelectedItem.ShouldBeSameAs(second);
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Horizontal)]
    [InlineData(NavMenuMode.Vertical)]
    [InlineData(NavMenuMode.Inline)]
    public void DefaultSelectedPath_Selects_Nested_Item_Without_Waiting_For_Timer(NavMenuMode mode)
    {
        var leaf = new NavMenuNode
        {
            Header  = "Leaf",
            ItemKey = "leaf"
        };
        var child = new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        };
        child.Children.Add(leaf);
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(child);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                = mode,
            IsMotionEnabled     = false,
            DefaultSelectedPath = new TreeNodePath("parent/child/leaf")
        };
        menu.Items.Add(parent);

        var selectedNodes = new List<INavMenuNode>();
        menu.NavMenuNodeSelected += (_, args) => selectedNodes.Add(args.NavMenuNode);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = mode == NavMenuMode.Inline ? menu : CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            RunDispatcherJobsUntil(() => ReferenceEquals(menu.SelectedItem, leaf));

            menu.SelectedItem.ShouldBeSameAs(leaf);
            selectedNodes.ShouldBe([leaf]);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void DefaultSelectedPath_Traverses_Groups_Without_Consuming_Path_Segments()
    {
        var leaf = new NavMenuNode
        {
            Header  = "Leaf",
            ItemKey = "leaf"
        };
        var childGroup = new NavMenuGroup { Header = "Child group" };
        childGroup.Entries.Add(leaf);
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Entries.Add(childGroup);
        var rootGroup = new NavMenuGroup { Header = "Root group" };
        rootGroup.Entries.Add(parent);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                = NavMenuMode.Inline,
            IsMotionEnabled     = false,
            DefaultSelectedPath = new TreeNodePath("parent/leaf")
        };
        menu.Items.Add(rootGroup);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            RunDispatcherJobsUntil(() => ReferenceEquals(menu.SelectedItem, leaf));

            var rootGroupContainer = menu.ContainerFromItem(rootGroup).ShouldBeOfType<NavMenuGroupItem>();
            var parentContainer = rootGroupContainer.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            var childGroupContainer = parentContainer.ContainerFromItem(childGroup).ShouldBeOfType<NavMenuGroupItem>();
            var leafContainer = childGroupContainer.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>();
            menu.SelectedItem.ShouldBeSameAs(leaf);
            parentContainer.IsInSelectedPath.ShouldBeTrue();
            leafContainer.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Horizontal)]
    [InlineData(NavMenuMode.Vertical)]
    [InlineData(NavMenuMode.Inline)]
    public void DefaultOpenPaths_Opens_Nested_Submenu_Without_Waiting_For_Timer(NavMenuMode mode)
    {
        var leaf = new NavMenuNode
        {
            Header  = "Leaf",
            ItemKey = "leaf"
        };
        var child = new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        };
        child.Children.Add(leaf);
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(child);

        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false,
            DefaultOpenPaths =
            [
                new TreeNodePath("parent/child")
            ]
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = mode == NavMenuMode.Inline ? menu : CreatePopupOverlayHost(menu)
        };

        try
        {
            window.Show();
            RunDispatcherJobsUntil(() =>
            {
                var parentContainer = menu.ContainerFromItem(parent) as NavMenuItem;
                var childContainer  = parentContainer?.ContainerFromItem(child) as NavMenuItem;
                return parentContainer?.IsSubMenuOpen == true &&
                       childContainer?.IsSubMenuOpen == true;
            });

            var realizedParent = menu.ContainerFromItem(parent) as NavMenuItem;
            var realizedChild  = realizedParent?.ContainerFromItem(child) as NavMenuItem;
            realizedParent.ShouldNotBeNull();
            realizedChild.ShouldNotBeNull();
            realizedParent.IsSubMenuOpen.ShouldBeTrue();
            realizedChild.IsSubMenuOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Vertical)]
    [InlineData(NavMenuMode.Horizontal)]
    [InlineData(NavMenuMode.Inline)]
    public void NavMenu_Clears_Selected_Container_When_SelectedItem_Is_Set_To_Null(NavMenuMode mode)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false
        };
        var first = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        menu.Items.Add(first);

        var selectedNodes = new List<INavMenuNode>();
        menu.NavMenuNodeSelected += (_, args) => selectedNodes.Add(args.NavMenuNode);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem = first;
            Dispatcher.UIThread.RunJobs();

            var firstContainer = menu.ContainerFromItem(first) as Control;
            firstContainer.ShouldNotBeNull();
            IsSelected(firstContainer).ShouldBeTrue();

            menu.SelectedItem = null;
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem.ShouldBeNull();
            IsSelected(firstContainer).ShouldBeFalse();

            selectedNodes.Clear();
            menu.SelectedItem = first;
            Dispatcher.UIThread.RunJobs();

            IsSelected(firstContainer).ShouldBeTrue();
            selectedNodes.ShouldBe([first]);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void NavMenu_Selects_New_Item_After_Replacing_The_Selected_Item_Tree()
    {
        var first = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        var replacement = new NavMenuNode
        {
            Header  = "Replacement",
            ItemKey = "replacement"
        };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(first);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var firstContainer = menu.ContainerFromItem(first).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(firstContainer);

            menu.Items.Clear();
            menu.Items.Add(replacement);
            Dispatcher.UIThread.RunJobs();

            var replacementContainer = menu.ContainerFromItem(replacement).ShouldBeOfType<NavMenuItem>();
            var replacementHeader = GetItemHeader(replacementContainer);
            var replacementPoint = GetCenterPoint(replacementHeader, window);
            Should.NotThrow(() =>
            {
                window.MouseDown(replacementPoint, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
                window.MouseUp(replacementPoint, MouseButton.Left);
                Dispatcher.UIThread.RunJobs();
            });

            replacementContainer.IsSelected.ShouldBeTrue();
            menu.SelectedItem.ShouldBeSameAs(replacement);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void NavMenu_Detach_Then_Programmatic_Sibling_Selection_Clears_Previous_Selection()
    {
        var first = new NavMenuNode
        {
            Header  = "First",
            ItemKey = "first"
        };
        var second = new NavMenuNode
        {
            Header  = "Second",
            ItemKey = "second"
        };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(first);
        menu.Items.Add(second);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem = first;
            Dispatcher.UIThread.RunJobs();

            var firstContainer = menu.ContainerFromItem(first).ShouldBeOfType<NavMenuItem>();
            firstContainer.IsSelected.ShouldBeTrue();

            window.Content = null;
            Dispatcher.UIThread.RunJobs();

            menu.SelectedItem = second;
            window.Content    = menu;
            Dispatcher.UIThread.RunJobs();

            var secondContainer = menu.ContainerFromItem(second).ShouldBeOfType<NavMenuItem>();
            firstContainer.IsSelected.ShouldBeFalse(
                "visual detach must not discard the identity needed to clear the applied selection.");
            secondContainer.IsSelected.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Submenu_Child_Press_Does_Not_Apply_Active_Background_To_Parent_Header()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        var child = new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(child);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            var childContainer = (Control)((ItemsControl)parentContainer).ContainerFromItem(child)!;
            var parentHeader   = GetItemHeader(parentContainer);
            var childHeader    = GetItemHeader(childContainer);

            MouseDown(childHeader, window);
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(parentHeader.Background).ShouldBe(
                Colors.Transparent,
                "pressing a child menu item must not make its parent header look active");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Selection_Marks_All_Ancestor_Submenus_As_Selected_Path()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        var leaf = new NavMenuNode
        {
            Header  = "Option 1",
            ItemKey = "option-1"
        };
        var secondLevel = new NavMenuNode
        {
            Header  = "Item 1",
            ItemKey = "item-1"
        };
        secondLevel.Children.Add(leaf);
        var firstLevel = new NavMenuNode
        {
            Header  = "Navigation Three - Submenu",
            ItemKey = "navigation-three"
        };
        firstLevel.Children.Add(secondLevel);
        menu.Items.Add(firstLevel);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 360,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var firstLevelContainer = (NavMenuItem)menu.ContainerFromItem(firstLevel)!;
            firstLevelContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var secondLevelContainer = (NavMenuItem)firstLevelContainer.ContainerFromItem(secondLevel)!;
            secondLevelContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var leafContainer = (NavMenuItem)secondLevelContainer.ContainerFromItem(leaf)!;
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(leafContainer);
            Dispatcher.UIThread.RunJobs();

            firstLevelContainer.IsInSelectedPath.ShouldBeTrue(
                "Ant Design colors every ancestor submenu title in the selected path.");
            secondLevelContainer.IsInSelectedPath.ShouldBeTrue(
                "Every inline ancestor must expose selected-path state to its header.");
            leafContainer.IsSelected.ShouldBeTrue();

            var firstLevelHeader  = GetItemHeader(firstLevelContainer);
            var secondLevelHeader = GetItemHeader(secondLevelContainer);
            firstLevelHeader.IsInSelectedPath.ShouldBeTrue(
                "The selected-path state must reach the first ancestor header so its icon, text and arrow use the selected color.");
            secondLevelHeader.IsInSelectedPath.ShouldBeTrue(
                "Nested ancestor headers must receive selected-path state consistently.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Group_Leaf_Selection_Uses_The_Root_Semantic_Selection_Owner()
    {
        var leaf = new NavMenuNode
        {
            Header  = "Leaf",
            ItemKey = "leaf"
        };
        var group = new NavMenuGroup { Header = "Section" };
        group.Entries.Add(leaf);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(group);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var groupContainer = menu.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
            var leafContainer = groupContainer.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(leafContainer);

            leafContainer.IsSelected.ShouldBeTrue();
            menu.SelectedItem.ShouldBeSameAs(leaf);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Selection_Through_A_Group_Uses_The_Closest_Node_Selected_Path()
    {
        var leaf = new NavMenuNode
        {
            Header  = "Leaf",
            ItemKey = "leaf"
        };
        var group = new NavMenuGroup { Header = "Section" };
        group.Entries.Add(leaf);
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Entries.Add(group);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();
            var groupContainer = parentContainer.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
            var leafContainer = groupContainer.ContainerFromItem(leaf).ShouldBeOfType<NavMenuItem>();
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(leafContainer);

            parentContainer.IsInSelectedPath.ShouldBeTrue();
            leafContainer.IsSelected.ShouldBeTrue();
            menu.SelectedItem.ShouldBeSameAs(leaf);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Selected_Path_Submenu_Header_Keeps_Selected_Foreground_When_Hovered()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        var leaf = new NavMenuNode
        {
            Header  = "Option 1",
            ItemKey = "option-1"
        };
        var parent = new NavMenuNode
        {
            Header  = "Navigation Three - Submenu",
            ItemKey = "navigation-three"
        };
        parent.Children.Add(leaf);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (NavMenuItem)menu.ContainerFromItem(parent)!;
            parentContainer.Open();
            Dispatcher.UIThread.RunJobs();

            var leafContainer = (NavMenuItem)parentContainer.ContainerFromItem(leaf)!;
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(leafContainer);
            Dispatcher.UIThread.RunJobs();

            var parentHeader             = GetItemHeader(parentContainer);
            var selectedForegroundBefore = GetSolidBrushColor(parentHeader.Foreground);
            selectedForegroundBefore.ShouldNotBeNull();
            parentHeader.IsInSelectedPath.ShouldBeTrue();

            MouseMove(parentHeader, window);
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(parentHeader.Foreground).ShouldBe(
                selectedForegroundBefore,
                "hover must not override the selected-path submenu title foreground; Ant Design keeps selected ancestor submenu titles in the selected color.");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Item_Background_Control_Propagates_To_Header_And_Submenu_Frame()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                    = NavMenuMode.Inline,
            IsMotionEnabled         = false,
            IsItemBackgroundEnabled = false
        };
        var grandchild = new NavMenuNode
        {
            Header  = "Grandchild",
            ItemKey = "grandchild"
        };
        var child = new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        };
        child.Children.Add(grandchild);
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(child);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            var childContainer = (NavMenuItem)((ItemsControl)parentContainer).ContainerFromItem(child)!;
            var childMenuItem  = (INavMenuItem)childContainer;
            childMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            var grandchildContainer = (NavMenuItem)childContainer.ContainerFromItem(grandchild)!;
            var parentHeader    = GetItemHeader(parentContainer);
            var childHeader     = GetItemHeader(childContainer);
            var grandchildHeader = GetItemHeader(grandchildContainer);
            var childFrame      = GetChildItemsFrame(parentContainer);
            var grandchildFrame = GetChildItemsFrame(childContainer);

            parentHeader.IsItemBackgroundEnabled.ShouldBeFalse();
            childHeader.IsItemBackgroundEnabled.ShouldBeFalse(
                "the menu-level background control must be inherited by nested inline menu items.");
            grandchildHeader.IsItemBackgroundEnabled.ShouldBeFalse(
                "every realized descendant header must receive the same background control value.");
            GetSolidBrushColor(parentHeader.Background).ShouldBe(Colors.Transparent);
            GetSolidBrushColor(childFrame.Background).ShouldBe(Colors.Transparent);
            GetSolidBrushColor(grandchildFrame.Background).ShouldBe(Colors.Transparent);

            MouseMove(grandchildHeader, window);
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(grandchildHeader.Background).ShouldNotBe(
                Colors.Transparent,
                "NavMenuItemHeader hover visuals must stay available when NavMenuItem background is disabled.");

            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(grandchildContainer);
            Dispatcher.UIThread.RunJobs();

            GetSolidBrushColor(grandchildHeader.Background).ShouldNotBe(
                Colors.Transparent,
                "NavMenuItemHeader selected visuals must stay available when NavMenuItem background is disabled.");

            menu.IsItemBackgroundEnabled = true;
            Dispatcher.UIThread.RunJobs();

            parentHeader.IsItemBackgroundEnabled.ShouldBeTrue();
            childHeader.IsItemBackgroundEnabled.ShouldBeTrue();
            grandchildHeader.IsItemBackgroundEnabled.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(NavMenuMode.Horizontal)]
    [InlineData(NavMenuMode.Vertical)]
    [InlineData(NavMenuMode.Inline)]
    public void Selecting_Realized_Item_Does_Not_Temporarily_Disable_Motion(NavMenuMode mode)
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = true
        };
        var item = new NavMenuNode
        {
            Header  = "Item",
            ItemKey = "item"
        };
        menu.Items.Add(item);

        var disabledDuringSelection = false;
        menu.PropertyChanged += (_, args) =>
        {
            if (args.Property == AtomUI.Desktop.Controls.NavMenu.IsMotionEnabledProperty &&
                args.NewValue is false)
            {
                disabledDuringSelection = true;
            }
        };

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var itemContainer = (NavMenuItem)menu.ContainerFromItem(item)!;
            menu.InteractionHandler.ShouldNotBeNull();
            menu.SelectNavMenuItem(itemContainer);
            Dispatcher.UIThread.RunJobs();

            disabledDuringSelection.ShouldBeFalse(
                "a user item selection is already applied by the interaction handler and must not replay the path by disabling motion");
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Submenu_Open_With_Motion_Disabled_Restores_Actor_Opacity_After_Animated_Close()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true
        };

        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        });
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            SetTimeSpanProperty(parentContainer, "OpenCloseMotionDuration", TimeSpan.FromMilliseconds(1));

            var actor = GetChildItemsViewport(parentContainer);
            var parentMenuItem = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            WaitForInlineMotionCompletion(actor);

            parentMenuItem.Close();
            WaitForInlineMotionCompletion(actor);

            actor.IsVisible.ShouldBeFalse();
            actor.Opacity.ShouldBe(1.0);

            menu.IsMotionEnabled = false;
            DrainDispatcher();

            parentMenuItem.Open();
            DrainDispatcher();

            actor.IsVisible.ShouldBeTrue();
            actor.Opacity.ShouldBe(1.0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Submenu_Open_With_Motion_Disabled_Clears_Child_Header_Transitions_After_Animated_Close()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true
        };

        var child = new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        };
        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(child);
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            SetTimeSpanProperty(parentContainer, "OpenCloseMotionDuration", TimeSpan.FromMilliseconds(1));

            var parentMenuItem = (INavMenuItem)parentContainer;
            parentMenuItem.Open();
            DrainDispatcher();

            var childContainer = (Control)((ItemsControl)parentContainer).ContainerFromItem(child)!;
            var childHeader    = GetItemHeader(childContainer);

            childHeader.IsMotionEnabled.ShouldBeTrue();
            childHeader.Transitions.ShouldNotBeNull();

            parentMenuItem.Close();
            DrainDispatcher();

            menu.IsMotionEnabled = false;
            DrainDispatcher();

            parentMenuItem.Open();
            DrainDispatcher();

            childHeader.IsMotionEnabled.ShouldBeFalse();
            childHeader.Transitions.ShouldBeNull();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Inline_Submenu_Open_With_Motion_Disabled_Overrides_Pending_Animated_Close()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = true
        };

        var parent = new NavMenuNode
        {
            Header  = "Parent",
            ItemKey = "parent"
        };
        parent.Children.Add(new NavMenuNode
        {
            Header  = "Child",
            ItemKey = "child"
        });
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = (Control)menu.ContainerFromItem(parent)!;
            var parentMenuItem  = (INavMenuItem)parentContainer;

            SetTimeSpanProperty(parentContainer, "OpenCloseMotionDuration", TimeSpan.FromMilliseconds(1));
            parentMenuItem.Open();
            WaitForInlineMotionCompletion(GetChildItemsViewport(parentContainer));

            SetTimeSpanProperty(parentContainer, "OpenCloseMotionDuration", TimeSpan.FromMilliseconds(500));
            parentMenuItem.Close();
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            var actor = GetChildItemsViewport(parentContainer);
            actor.IsVisible.ShouldBeTrue();
            (!MotionApi.GetVisibilityTask(actor).IsCompleted).ShouldBeTrue();

            menu.IsMotionEnabled = false;
            Dispatcher.UIThread.RunJobs();

            parentMenuItem.Open();
            Dispatcher.UIThread.RunJobs();

            parentMenuItem.IsSubMenuOpen.ShouldBeTrue();

            MotionApi.GetVisibilityTask(actor).IsCompleted.ShouldBeTrue();
            Thread.Sleep(600);
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();

            actor.IsVisible.ShouldBeTrue();
            actor.Opacity.ShouldBe(1.0);
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void SelectedItem_Is_Already_Updated_When_NodeSelected_Event_Raises()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        var first = new NavMenuNode { Header = "First", ItemKey = "first" };
        var second = new NavMenuNode { Header = "Second", ItemKey = "second" };
        menu.Items.Add(first);
        menu.Items.Add(second);

        var window = new Avalonia.Controls.Window
        {
            Width   = 320,
            Height  = 240,
            Content = menu
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var secondContainer = menu.ContainerFromItem(second).ShouldBeOfType<NavMenuItem>();

            INavMenuNode? observed = null;
            menu.NavMenuNodeSelected += (_, _) => observed = menu.SelectedItem;
            menu.SelectNavMenuItem(secondContainer);

            observed.ShouldBeSameAs(second,
                "NavMenuNodeSelected subscribers must observe the committed SelectedItem.");
            menu.SelectedItem.ShouldBeSameAs(second);
        }
        finally
        {
            window.Close();
        }
    }

    private static bool IsSelected(Control container)
    {
        return GetBoolProperty(container, "IsSelected");
    }

    private static bool GetBoolProperty(Control container, string propertyName)
    {
        var property = container.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        return (bool)property.GetValue(container)!;
    }

    private static ContentViewport GetChildItemsViewport(Control container)
    {
        var field = container.GetType().GetField(
            "_childItemsViewport",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (ContentViewport)field.GetValue(container)!;
    }

    private static Border GetChildItemsFrame(Control container)
    {
        var actor = GetChildItemsViewport(container);
        actor.Content.ShouldBeOfType<Border>();
        return (Border)actor.Content;
    }

    private static BaseNavMenuItemHeader GetItemHeader(Control container)
    {
        var field = container.GetType().GetField(
            "_itemHeader",
            BindingFlags.Instance | BindingFlags.NonPublic);
        field.ShouldNotBeNull();
        return (BaseNavMenuItemHeader)field.GetValue(container)!;
    }

    private static void SetTimeSpanProperty(Control container, string propertyName, TimeSpan value)
    {
        var property = container.GetType().GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(container, value);
    }

    private static void MouseDown(Control control, Avalonia.Controls.Window window)
    {
        MouseMove(control, window);
        window.MouseDown(GetCenterPoint(control, window), MouseButton.Left);
    }

    private static void MouseMove(Control control, Avalonia.Controls.Window window)
    {
        window.MouseMove(GetCenterPoint(control, window));
    }

    private static Point GetCenterPoint(Control control, Avalonia.Controls.Window window)
    {
        var point = control.TranslatePoint(
            new Point(control.Bounds.Width / 2, control.Bounds.Height / 2),
            window);

        point.ShouldNotBeNull();
        return point.Value;
    }

    private static Color? GetSolidBrushColor(IBrush? brush)
    {
        return brush is ISolidColorBrush solidColorBrush
            ? solidColorBrush.Color
            : null;
    }

    private static void WaitForInlineMotionCompletion(ContentViewport actor)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(2);
        do
        {
            Dispatcher.UIThread.RunJobs();
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            if (MotionApi.GetVisibilityTask(actor).IsCompleted)
            {
                return;
            }
            Thread.Sleep(10);
        } while (DateTime.UtcNow < deadline);

        MotionApi.GetVisibilityTask(actor).IsCompleted.ShouldBeTrue();
    }

    private static void DrainDispatcher()
    {
        Dispatcher.UIThread.RunJobs();
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Thread.Sleep(20);
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private static void RunDispatcherJobsUntil(Func<bool> condition, int maxPasses = 128)
    {
        for (var i = 0; i < maxPasses && !condition(); i++)
        {
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static Avalonia.Controls.Primitives.VisualLayerManager CreatePopupOverlayHost(Control content)
    {
        var visualLayerManager = new Avalonia.Controls.Primitives.VisualLayerManager
        {
            Child = content
        };

        EnablePopupOverlayLayer(visualLayerManager);
        return visualLayerManager;
    }

    private static void EnablePopupOverlayLayer(Avalonia.Controls.Primitives.VisualLayerManager visualLayerManager)
    {
        var property = typeof(Avalonia.Controls.Primitives.VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);

        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
    }
}
