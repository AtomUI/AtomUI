using System.Collections.ObjectModel;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Threading;
using Shouldly;
using Xunit;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuEntryContainerTests
{
    static NavMenuEntryContainerTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Node_HeaderTemplate_Changes_Use_Current_Owner_Fallback_When_Cleared()
    {
        var node = new NavMenuNode { Header = "Node" };
        var firstFallback = new FuncDataTemplate<INavMenuNode>((_, _) => new TextBlock { Text = "First fallback" });
        var nextFallback = new FuncDataTemplate<INavMenuNode>((_, _) => new TextBlock { Text = "Next fallback" });
        var nodeTemplate = new FuncDataTemplate<INavMenuNode>((_, _) => new TextBlock { Text = "Node template" });
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode = NavMenuMode.Inline,
            IsMotionEnabled = false,
            ItemTemplate = firstFallback
        };
        menu.Items.Add(node);
        ShowInWindow(menu, _ =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            container.HeaderTemplate.ShouldBeSameAs(firstFallback);
            node.HeaderTemplate = nodeTemplate;
            Dispatcher.UIThread.RunJobs();
            container.HeaderTemplate.ShouldBeSameAs(nodeTemplate);
            menu.ItemTemplate = nextFallback;
            Dispatcher.UIThread.RunJobs();
            // ItemsControl may recreate containers when its ItemTemplate changes.
            container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            container.HeaderTemplate.ShouldBeSameAs(nodeTemplate);
            node.HeaderTemplate = null;
            Dispatcher.UIThread.RunJobs();
            container.HeaderTemplate.ShouldBeSameAs(nextFallback);
        });
    }

    [Fact]
    public void Realized_Node_Tracks_ItemKey_Changes_And_Null()
    {
        var node = new NavMenuNode { Header = "Node", ItemKey = "old" };
        var menu = new AtomUI.Desktop.Controls.NavMenu { Mode = NavMenuMode.Inline, IsMotionEnabled = false };
        menu.Items.Add(node);
        ShowInWindow(menu, _ =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            node.ItemKey = "new";
            Dispatcher.UIThread.RunJobs();
            container.ItemKey.ShouldBe(node.ItemKey);
            node.ItemKey = null;
            Dispatcher.UIThread.RunJobs();
            container.ItemKey.ShouldBeNull();
        });
    }

    [Fact]
    public void Pure_Node_Menu_Still_Realizes_Nested_Children_From_The_Canonical_Entry_Source()
    {
        var child = new NavMenuNode { Header = "Child" };
        var parent = new NavMenuNode { Header = "Parent" };
        parent.Children.Add(child);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(parent);

        var window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = menu };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var parentContainer = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            menu.ItemTemplate.ShouldNotBeNull();
            parentContainer.Header.ShouldBeSameAs(parent);
            parentContainer.HeaderTemplate.ShouldNotBeNull();
            parentContainer.DataContext.ShouldBeSameAs(parent);
            parentContainer.ItemsSource.ShouldBeSameAs(parent.Entries);
            parentContainer.ItemCount.ShouldBe(1);
            parentContainer.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            parentContainer.ContainerFromItem(child).ShouldBeOfType<NavMenuItem>();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void Root_Entry_Composition_Uses_Dedicated_Containers_And_Keeps_Groups_Semantically_Transparent()
    {
        var directNode = new NavMenuNode { Header = "Direct" };
        var groupedNode = new NavMenuNode { Header = "Grouped" };
        var groupedDivider = new NavMenuDivider();
        var group = new NavMenuGroup { Header = "Section" };
        group.Entries.Add(groupedNode);
        group.Entries.Add(groupedDivider);
        var rootDivider = new NavMenuDivider();
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(directNode);
        menu.Items.Add(group);
        menu.Items.Add(rootDivider);

        ShowInWindow(menu, _ =>
        {
            menu.ContainerFromItem(directNode).ShouldBeOfType<NavMenuItem>();
            var groupContainer = menu.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
            menu.ContainerFromItem(rootDivider).ShouldBeOfType<NavMenuDividerItem>();

            groupContainer.Header.ShouldBe("Section");
            groupContainer.ItemsSource.ShouldBeSameAs(group.Entries);
            var groupedNodeContainer = groupContainer.ContainerFromItem(groupedNode).ShouldBeOfType<NavMenuItem>();
            groupContainer.ContainerFromItem(groupedDivider).ShouldBeOfType<NavMenuDividerItem>();
            groupedNodeContainer.Level.ShouldBe(0);
            groupedNodeContainer.IsTopLevel.ShouldBeTrue();
            ((INavMenuItem)groupedNodeContainer).Parent.ShouldBeSameAs(menu);
        });
    }

    [Fact]
    public void Nested_Group_Inherits_The_Closest_Node_Semantic_Context()
    {
        var child = new NavMenuNode { Header = "Child" };
        var group = new NavMenuGroup { Header = "Nested" };
        group.Entries.Add(child);
        var parent = new NavMenuNode { Header = "Parent" };
        parent.Entries.Add(group);
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode = NavMenuMode.Inline,
            IsMotionEnabled = false
        };
        menu.Items.Add(parent);

        ShowInWindow(menu, window =>
        {
            var parentContainer = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            parentContainer.IsSubMenuOpen = true;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var groupContainer = parentContainer.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
            var childContainer = groupContainer.ContainerFromItem(child).ShouldBeOfType<NavMenuItem>();
            childContainer.Level.ShouldBe(1);
            childContainer.IsTopLevel.ShouldBeFalse();
            ((INavMenuItem)childContainer).Parent.ShouldBeSameAs(parentContainer);
        });
    }

    [Fact]
    public void Replacing_Entry_Kinds_Does_Not_Recycle_An_Incompatible_Container()
    {
        var source = new ObservableCollection<INavMenuEntry> { new NavMenuNode() };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode = NavMenuMode.Inline,
            IsMotionEnabled = false,
            ItemsSource = source
        };

        ShowInWindow(menu, window =>
        {
            menu.ContainerFromIndex(0).ShouldBeOfType<NavMenuItem>();

            source[0] = new NavMenuGroup();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            menu.ContainerFromIndex(0).ShouldBeOfType<NavMenuGroupItem>();

            source[0] = new NavMenuDivider();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            menu.ContainerFromIndex(0).ShouldBeOfType<NavMenuDividerItem>();
        });
    }

    [Fact]
    public void Removed_Node_Container_Releases_All_Owner_Bindings()
    {
        var firstTemplate = new FuncDataTemplate<object?>((_, _) => new TextBlock { Text = "First" });
        var replacementTemplate = new FuncDataTemplate<object?>((_, _) => new TextBlock { Text = "Replacement" });
        var node = new NavMenuNode { Header = "Node" };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                    = NavMenuMode.Inline,
            IsDarkStyle             = false,
            IsItemBackgroundEnabled = true,
            IsMotionEnabled         = true,
            ShouldUseOverlayPopup   = true,
            ItemTemplate            = firstTemplate
        };
        menu.Items.Add(node);

        ShowInWindow(menu, window =>
        {
            var container = menu.ContainerFromItem(node).ShouldBeOfType<NavMenuItem>();
            container.HeaderTemplate.ShouldBeSameAs(firstTemplate);

            menu.Items.Remove(node);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            menu.ContainerFromItem(node).ShouldBeNull();

            menu.Mode = NavMenuMode.Horizontal;
            menu.IsDarkStyle = true;
            menu.IsItemBackgroundEnabled = false;
            menu.IsMotionEnabled = false;
            menu.ShouldUseOverlayPopup = false;
            menu.ItemTemplate = replacementTemplate;
            Dispatcher.UIThread.RunJobs();

            container.OwnerMenu.ShouldBeNull();
            container.Mode.ShouldBe(NavMenuMode.Vertical);
            container.IsDarkStyle.ShouldBeFalse();
            container.IsItemBackgroundEnabled.ShouldBeTrue();
            container.IsMotionEnabled.ShouldBeFalse();
            container.ShouldUseOverlayPopup.ShouldBeFalse();
            container.HeaderTemplate.ShouldBeNull();
        });
    }

    [Fact]
    public void Clear_Node_Container_Resets_Transient_Selection_State()
    {
        var menu = new AtomUI.Desktop.Controls.NavMenu();
        var container = new NavMenuItem
        {
            IsSelected       = true,
            IsInSelectedPath = true,
            IsSubMenuOpen    = true
        };

        NavMenuEntryContainerCoordinator.ClearContainer(menu, container);

        container.IsSelected.ShouldBeFalse();
        container.IsInSelectedPath.ShouldBeFalse();
        container.IsSubMenuOpen.ShouldBeFalse();
    }

    [Fact]
    public void Removed_Group_Container_Releases_All_Owner_Bindings()
    {
        var firstTemplate = new FuncDataTemplate<object?>((_, _) => new TextBlock { Text = "First" });
        var replacementTemplate = new FuncDataTemplate<object?>((_, _) => new TextBlock { Text = "Replacement" });
        var group = new NavMenuGroup { Header = "Group" };
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode                    = NavMenuMode.Inline,
            IsDarkStyle             = false,
            IsItemBackgroundEnabled = true,
            IsMotionEnabled         = true,
            ShouldUseOverlayPopup   = true,
            ItemTemplate            = firstTemplate
        };
        menu.Items.Add(group);

        ShowInWindow(menu, window =>
        {
            var container = menu.ContainerFromItem(group).ShouldBeOfType<NavMenuGroupItem>();
            container.ItemTemplate.ShouldBeSameAs(firstTemplate);

            menu.Items.Remove(group);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            menu.Mode = NavMenuMode.Horizontal;
            menu.IsDarkStyle = true;
            menu.IsItemBackgroundEnabled = false;
            menu.IsMotionEnabled = false;
            menu.ShouldUseOverlayPopup = false;
            menu.ItemTemplate = replacementTemplate;
            Dispatcher.UIThread.RunJobs();

            container.OwnerMenu.ShouldBeNull();
            container.Mode.ShouldBe(NavMenuMode.Vertical);
            container.IsDarkStyle.ShouldBeFalse();
            container.IsItemBackgroundEnabled.ShouldBeTrue();
            container.IsMotionEnabled.ShouldBeFalse();
            container.ShouldUseOverlayPopup.ShouldBeFalse();
            container.ItemTemplate.ShouldBeNull();
        });
    }

    [Fact]
    public void Removed_Divider_Container_Releases_All_Owner_Bindings()
    {
        var divider = new NavMenuDivider();
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode        = NavMenuMode.Inline,
            IsDarkStyle = false
        };
        menu.Items.Add(divider);

        ShowInWindow(menu, window =>
        {
            var container = menu.ContainerFromItem(divider).ShouldBeOfType<NavMenuDividerItem>();

            menu.Items.Remove(divider);
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            menu.Mode = NavMenuMode.Horizontal;
            menu.IsDarkStyle = true;
            Dispatcher.UIThread.RunJobs();

            container.Mode.ShouldBe(NavMenuMode.Vertical);
            container.IsDarkStyle.ShouldBeFalse();
            container.Orientation.ShouldBe(Orientation.Horizontal);
        });
    }

    [Theory]
    [InlineData(NavMenuMode.Inline, Orientation.Horizontal)]
    [InlineData(NavMenuMode.Vertical, Orientation.Horizontal)]
    [InlineData(NavMenuMode.Horizontal, Orientation.Vertical)]
    public void Root_Divider_Orientation_Follows_The_Menu_Mode(
        NavMenuMode mode,
        Orientation expectedOrientation)
    {
        var divider = new NavMenuDivider();
        var menu = new AtomUI.Desktop.Controls.NavMenu
        {
            Mode            = mode,
            IsMotionEnabled = false
        };
        menu.Items.Add(divider);

        ShowInWindow(menu, _ =>
        {
            var container = menu.ContainerFromItem(divider).ShouldBeOfType<NavMenuDividerItem>();
            container.Orientation.ShouldBe(expectedOrientation);
        });
    }

    private static void ShowInWindow(
        Control content,
        Action<Avalonia.Controls.Window> assertion)
    {
        var window = new Avalonia.Controls.Window { Width = 320, Height = 240, Content = content };
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
}
