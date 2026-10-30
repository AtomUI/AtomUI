using AtomUI.Controls;
using AtomUI.Desktop.Controls;
using AtomUI.Icons.AntDesign;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AtomUI.Performance;

using AtomPopup = AtomUI.Desktop.Controls.Popup;

internal static partial class Program
{
    private static bool RunNavMenuStateVerification()
    {
        var failures = new List<string>();
        VerifyNavMenuHeaderIconState(failures);
        VerifyNavMenuPopupTemplateState(failures);
        VerifyNavMenuInlineSubmenuTemplateState(failures);
        VerifyNavMenuDefaultPathsApply(failures);
        VerifyNavMenuInteractionSubscriptionsAreLazy(failures);
        VerifyNavMenuContainerBindingsRelease(failures);

        if (failures.Count == 0)
        {
            Console.WriteLine("NavMenu state verification passed.");
            return true;
        }

        Console.Error.WriteLine("NavMenu state verification failed:");
        foreach (var failure in failures)
        {
            Console.Error.WriteLine($"- {failure}");
        }
        return false;
    }

    private static void VerifyNavMenuHeaderIconState(ICollection<string> failures)
    {
        var menu = CreateNavMenuLeaf(NavMenuMode.Inline);
        using var realized = RealizeControl(menu);

        var iconPresenter = FindVisualByName<IconPresenter>(menu, "ItemIconPresenter");
        Expect(iconPresenter != null,
            "NavMenu leaf header should create the template ItemIconPresenter.",
            failures);
        Expect(iconPresenter?.IsVisible == false,
            "NavMenu leaf header without Icon should keep ItemIconPresenter hidden.",
            failures);

        var node = menu.Items[0] as NavMenuNode;
        Expect(node != null, "NavMenu test node should be available.", failures);
        if (node is null)
        {
            return;
        }

        node.Icon = new MailOutlined();
        RefreshLayout(realized.Window);
        Expect(iconPresenter?.IsVisible == true,
            "NavMenu header should show ItemIconPresenter when Icon is assigned.",
            failures);

        node.Icon = null;
        RefreshLayout(realized.Window);
        Expect(iconPresenter?.IsVisible == false,
            "NavMenu header should hide ItemIconPresenter when Icon is cleared.",
            failures);
    }

    private static void VerifyNavMenuPopupTemplateState(ICollection<string> failures)
    {
        var menu = CreateNavMenuWithSubmenu(NavMenuMode.Vertical);
        menu.IsMotionEnabled = false;
        using var realized = RealizeControl(menu);
        var submenu  = menu.ContainerFromIndex(2) as NavMenuItem;
        var popup    = submenu == null ? null : FindVisualByName<AtomPopup>(submenu, "PART_Popup");

        Expect(submenu != null, "Vertical NavMenu submenu container should be realized.", failures);
        Expect(popup != null, "Vertical NavMenu submenu should expose PART_Popup.", failures);
        Expect(popup?.Child != null,
            "Vertical NavMenu submenu should keep template popup frame available.",
            failures);

        if (submenu is null || popup is null)
        {
            return;
        }

        Expect((popup.Child as Control)?.Name == "PART_PopupFrame",
            "Vertical NavMenu popup content should keep PART_PopupFrame as template child.",
            failures);

        Expect(popup.IsOpen == false,
            "Closed vertical NavMenu submenu popup should stay closed.",
            failures);
    }

    private static void VerifyNavMenuInlineSubmenuTemplateState(ICollection<string> failures)
    {
        var menu = CreateNavMenuWithSubmenu(NavMenuMode.Inline);
        menu.IsMotionEnabled = false;
        using var realized = RealizeControl(menu);
        var leaf    = menu.ContainerFromIndex(0) as NavMenuItem;
        var submenu = menu.ContainerFromIndex(2) as NavMenuItem;

        Expect(leaf != null, "Inline NavMenu leaf container should be realized.", failures);
        Expect(submenu != null, "Inline NavMenu submenu container should be realized.", failures);
        if (leaf is null || submenu is null)
        {
            return;
        }

        var leafViewport = FindVisualByTypeName(leaf, "ContentViewport", "PART_ChildItemsViewport");
        var firstViewport = FindVisualByTypeName(submenu, "ContentViewport", "PART_ChildItemsViewport");

        Expect(leafViewport != null,
            "Inline NavMenu leaf should keep the template child ContentViewport.",
            failures);
        Expect(firstViewport != null,
            "Inline NavMenu submenu should keep the template child ContentViewport.",
            failures);
        Expect(firstViewport?.IsVisible == false,
            "Closed inline NavMenu submenu content viewport should be hidden.",
            failures);

        submenu.IsSubMenuOpen = true;
        RefreshLayout(realized.Window);
        Expect(firstViewport?.IsVisible == true,
            "Opened inline NavMenu submenu content viewport should be visible.",
            failures);

        submenu.IsSubMenuOpen = false;
        RefreshLayout(realized.Window);
        Expect(firstViewport?.IsVisible == false,
            "Closed inline NavMenu submenu content viewport should be hidden.",
            failures);

        submenu.IsSubMenuOpen = true;
        RefreshLayout(realized.Window);
        Expect(ReferenceEquals(firstViewport,
                FindVisualByTypeName(submenu, "ContentViewport", "PART_ChildItemsViewport")),
            "Inline NavMenu submenu should reuse child ContentViewport on second open.",
            failures);
    }

    private static void VerifyNavMenuInteractionSubscriptionsAreLazy(ICollection<string> failures)
    {
        var menu = CreateNavMenuWithSubmenu(NavMenuMode.Vertical);
        menu.IsMotionEnabled = false;
        using var realized = RealizeControl(menu);
        var handler = menu.InteractionHandler as DefaultNavMenuInteractionHandler;
        var submenu = menu.ContainerFromIndex(2) as NavMenuItem;

        Expect(handler != null, "Vertical NavMenu should have an interaction handler.", failures);
        if (handler is null || submenu is null)
        {
            return;
        }

        Expect(GetPrivateField(handler, "AtomUI.Desktop.Controls.DefaultNavMenuInteractionHandler", "_inputManagerSubscription") == null,
            "Closed vertical NavMenu should not subscribe to global input manager before first submenu open.",
            failures);
        Expect(GetPrivateField(handler, "AtomUI.Desktop.Controls.DefaultNavMenuInteractionHandler", "_attachedWindow") == null,
            "Closed vertical NavMenu should not subscribe to Window.Deactivated before first submenu open.",
            failures);

        // TODO: DefaultNavMenuInteractionHandler.NotifySubmenuOpenStateChanged was renamed/removed during the
        // Avalonia 12 migration. Re-wire the open/close attach assertions once the new submenu-state surface lands.
    }

    private static void VerifyNavMenuDefaultPathsApply(ICollection<string> failures)
    {
        var menu = CreateNavMenuWithDefaultOpenPath();
        menu.IsMotionEnabled = false;
        using var realized = RealizeControl(menu);

        for (var i = 0; i < 4; i++)
        {
            RefreshLayout(realized.Window);
        }

        var rootNode      = menu.Items[2] as NavMenuNode;
        var group1Node    = rootNode?.Children[0] as NavMenuNode;
        var group2Node    = rootNode?.Children[1] as NavMenuNode;
        var option1Node   = group1Node?.Children[0] as NavMenuNode;
        var rootItem      = rootNode == null ? null : menu.ContainerFromItem(rootNode) as NavMenuItem;
        var group1Item    = group1Node == null || rootItem == null ? null : rootItem.ContainerFromItem(group1Node) as NavMenuItem;
        var group2Item    = group2Node == null || rootItem == null ? null : rootItem.ContainerFromItem(group2Node) as NavMenuItem;
        var option1Item   = option1Node == null || group1Item == null ? null : group1Item.ContainerFromItem(option1Node) as NavMenuItem;

        Expect(rootItem?.IsSubMenuOpen == true,
            "Default paths should open the root NavMenu submenu.",
            failures);
        Expect(group1Item?.IsSubMenuOpen == true,
            "DefaultSelectedPath should open the selected branch.",
            failures);
        Expect(group2Item?.IsSubMenuOpen == true,
            "DefaultOpenPaths should open the configured branch.",
            failures);
        Expect(option1Item?.IsSelected == true,
            "DefaultSelectedPath should select the target leaf.",
            failures);
        Expect(ReferenceEquals(menu.SelectedItem, option1Node),
            "DefaultSelectedPath should update NavMenu.SelectedItem.",
            failures);
    }

    private static void VerifyNavMenuContainerBindingsRelease(ICollection<string> failures)
    {
        var menu = CreateNavMenuLeaf(NavMenuMode.Inline);
        using var realized = RealizeControl(menu);
        var node      = menu.Items[0] as NavMenuNode;
        var container = menu.ContainerFromIndex(0) as NavMenuItem;

        Expect(node != null, "NavMenu binding test node should be available.", failures);
        Expect(container != null, "NavMenu binding test container should be available.", failures);
        if (node is null || container is null)
        {
            return;
        }

        menu.Items.RemoveAt(0);
        RefreshLayout(realized.Window);
        node.Icon = new MailOutlined();
        RefreshLayout(realized.Window);

        Expect(container.Icon == null,
            "Cleared NavMenu container should release INavMenuNode.Icon binding.",
            failures);
        Expect(container.GetVisualParent() == null,
            "Cleared NavMenu container should not keep a visual parent.",
            failures);
    }
}
