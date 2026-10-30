using System.Reflection;
using System.Xml.Linq;
using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Shouldly;
using Xunit;
using AtomUINavMenu = AtomUI.Desktop.Controls.NavMenu;
using AtomUIWindow = AtomUI.Desktop.Controls.Window;

namespace AtomUI.Desktop.Controls.Tests.NavMenu;

public class NavMenuSemanticPartTests
{
    private const string ItemClass               = "semantic-item";
    private const string ItemIconClass           = "semantic-item-icon";
    private const string ItemContentClass        = "semantic-item-content";
    private const string ItemTitleClass          = "semantic-item-title";
    private const string ListClass               = "semantic-list";
    private const string SubMenuItemClass        = "semantic-sub-menu-item";
    private const string SubMenuItemIconClass    = "semantic-sub-menu-item-icon";
    private const string SubMenuItemContentClass = "semantic-sub-menu-item-content";
    private const string SubMenuItemTitleClass   = "semantic-sub-menu-item-title";
    private const string SubMenuListClass        = "semantic-sub-menu-list";
    private const string GroupScopeClass         = "semantic-scope-group";
    private const string SubMenuGroupScopeClass  = "semantic-sub-menu-group";
    private const string PopupRootClass          = "semantic-popup-root";
    private const string HeaderHop               = "/template/ .semantic-scope-header /template/ .";

    static NavMenuSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Upstream_Key_Paths()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUINavMenu), out var descriptor).ShouldBeTrue();
        descriptor.ShouldNotBeNull();
        descriptor.Parts.Select(static part => part.Name)
                  .ShouldBe(
                      [
                          "root",
                          "item",
                          "itemContent",
                          "itemIcon",
                          "itemTitle",
                          "list",
                          "popup.root",
                          "subMenu.item",
                          "subMenu.itemContent",
                          "subMenu.itemIcon",
                          "subMenu.itemTitle",
                          "subMenu.list"
                      ]);

        AssertRoot(descriptor.Parts.Single(static part => part.Name == "root"));

        AssertRuntimePart(descriptor, "item", ItemClass,
            $">> .{ItemClass}",
            typeof(HeaderedSelectingItemsControl));
        AssertRuntimePart(descriptor, "itemIcon", ItemIconClass,
            $">> .{ItemClass} {HeaderHop}{ItemIconClass}",
            typeof(IconPresenter));
        AssertRuntimePart(descriptor, "itemContent", ItemContentClass,
            $">> .{ItemClass} {HeaderHop}{ItemContentClass}",
            typeof(ContentPresenter));
        AssertRuntimePart(descriptor, "itemTitle", ItemTitleClass,
            $">> .{GroupScopeClass} /template/ .{ItemTitleClass}",
            typeof(ContentPresenter));
        AssertRuntimePart(descriptor, "list", ListClass,
            $">> .{GroupScopeClass} /template/ .{ListClass}",
            typeof(ItemsPresenter));

        AssertRuntimePart(descriptor, "subMenu.item", SubMenuItemClass,
            $">> .{SubMenuItemClass}",
            typeof(HeaderedSelectingItemsControl),
            crossVisualRoot: true);
        AssertRuntimePart(descriptor, "subMenu.itemIcon", SubMenuItemIconClass,
            $">> .{SubMenuItemClass} {HeaderHop}{SubMenuItemIconClass}",
            typeof(IconPresenter),
            crossVisualRoot: true);
        AssertRuntimePart(descriptor, "subMenu.itemContent", SubMenuItemContentClass,
            $">> .{SubMenuItemClass} {HeaderHop}{SubMenuItemContentClass}",
            typeof(ContentPresenter),
            crossVisualRoot: true);
        AssertRuntimePart(descriptor, "subMenu.itemTitle", SubMenuItemTitleClass,
            $">> .{SubMenuGroupScopeClass} /template/ .{SubMenuItemTitleClass}",
            typeof(ContentPresenter),
            crossVisualRoot: true);
        AssertRuntimePart(descriptor, "subMenu.list", SubMenuListClass,
            $">> .{SubMenuGroupScopeClass} /template/ .{SubMenuListClass}",
            typeof(ItemsPresenter),
            crossVisualRoot: true);

        var popupRoot = descriptor.Parts.Single(static part => part.Name == "popup.root");
        popupRoot.Path.ShouldBe("popup.root");
        popupRoot.SelectorClass.ShouldBe(PopupRootClass);
        popupRoot.SelectorRoute.ShouldBe($">> .{PopupRootClass}");
        popupRoot.ContractType.ShouldBe(typeof(Border));
        popupRoot.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        popupRoot.Customization.ShouldBe(SemanticPartCustomization.Selector);
        popupRoot.CrossVisualRoot.ShouldBeTrue();
        popupRoot.CrossNestedOwners.ShouldBeTrue();
        popupRoot.RuntimeCreated.ShouldBeTrue();
        popupRoot.Since.ShouldBe("6.2.0");

        // The item / group containers are internal, so they cannot own a descriptor.
        registry.TryGetControl(typeof(NavMenuItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(NavMenuGroupItem), out _).ShouldBeFalse();
        registry.TryGetControl(typeof(NavMenuDividerItem), out _).ShouldBeFalse();
    }

    [Fact]
    public void Templates_Implement_Only_The_Approved_Static_Markers()
    {
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/BaseNavMenuItemHeaderTheme.axaml",
            Array.Empty<string>());
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuItemTheme.axaml",
            [
                "semantic-popup-root:NavMenuPopupFrame",
                "semantic-popup-root:NavMenuPopupFrame",
                "semantic-scope-header:HorizontalNavMenuItemHeader",
                "semantic-scope-header:InlineNavMenuItemHeader",
                "semantic-scope-header:VerticalNavMenuItemHeader"
            ]);

        string[] headerMarkers =
        [
            $"{ItemContentClass}:ContentPresenter",
            $"{ItemIconClass}:IconPresenter",
            $"{SubMenuItemContentClass}:ContentPresenter",
            $"{SubMenuItemIconClass}:IconPresenter"
        ];
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/HorizontalNavMenuItemHeaderTheme.axaml",
            headerMarkers);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/VerticalNavMenuItemHeaderTheme.axaml",
            headerMarkers);
        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/InlineNavMenuItemHeaderTheme.axaml",
            headerMarkers);

        AssertThemeMarkers(
            "src/AtomUI.Desktop.Controls/NavMenu/Themes/NavMenuGroupItemTheme.axaml",
            [
                $"{ItemTitleClass}:ContentPresenter",
                $"{ListClass}:ItemsPresenter",
                $"{SubMenuItemTitleClass}:ContentPresenter",
                $"{SubMenuListClass}:ItemsPresenter"
            ]);
    }

    [Fact]
    public void Inline_Menu_Marks_First_Level_And_Submenu_Containers_By_Level()
    {
        var (menu, root, subMenuNode) = CreateMenuWithSubmenu(NavMenuMode.Inline);

        ShowInWindow(menu, root, host =>
        {
            var subMenuContainer = OpenSubMenu(menu, subMenuNode);

            // First level: the two leaf containers plus the leaf inside the top-level group.
            Marked(host, ItemClass).Length.ShouldBe(3);
            Marked(host, GroupScopeClass).Length.ShouldBe(1);
            Marked(host, ItemTitleClass).Length.ShouldBe(2);
            Marked(host, ListClass).Length.ShouldBe(2);

            // Submenu level: the nested leaf and the leaf inside the submenu's group.
            Marked(host, SubMenuItemClass).Length.ShouldBe(2);
            Marked(host, SubMenuGroupScopeClass).Length.ShouldBe(1);
            Marked(host, SubMenuItemTitleClass).Length.ShouldBe(2);
            Marked(host, SubMenuListClass).Length.ShouldBe(2);

            // Depth isolation: a first-level container never carries the submenu identity.
            Marked<NavMenuItem>(host, ItemClass).ShouldContain(subMenuContainer);
            Marked<NavMenuItem>(host, SubMenuItemClass).ShouldNotContain(subMenuContainer);

            // Inline mode expands inside the visual tree, so there is no popup layer.
            Marked(host, PopupRootClass).Length.ShouldBe(0);
        });
    }

    [Fact]
    public void Vertical_Popup_Marks_Submenu_Containers_And_Popup_Root()
    {
        var (menu, root, subMenuNode) = CreateMenuWithSubmenu(NavMenuMode.Vertical);

        ShowInWindow(menu, root, host =>
        {
            // Pinning realizes the popup subtree, including the nested containers.
            var subMenuContainer = menu.ContainerFromItem(subMenuNode).ShouldBeOfType<NavMenuItem>();

            Marked(host, ItemClass).Length.ShouldBe(3);
            Marked(host, SubMenuItemClass).Length.ShouldBe(2);
            Marked<NavMenuItem>(host, SubMenuItemClass).ShouldNotContain(subMenuContainer);

            // Horizontal / Vertical templates own the popup frame node.
            Marked(host, PopupRootClass).Length.ShouldBeGreaterThanOrEqualTo(1);

            // Closing and reopening the popup branch does not change the popup frame marker count:
            // the frame is part of the item template, not of the popup's realized content.
            var popupCount = Marked(host, PopupRootClass).Length;
            menu.IsPopupPinnedOpen = false;
            Dispatcher.UIThread.RunJobs();
            Marked(host, PopupRootClass).Length.ShouldBe(popupCount);

            menu.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            Marked(host, PopupRootClass).Length.ShouldBe(popupCount);
        });
    }

    [Fact]
    public void Submenu_Route_Does_Not_Match_First_Level_Containers()
    {
        var (menu, root, _) = CreateMenuWithSubmenu(NavMenuMode.Inline);
        menu.Classes.Add("semantic-owner");
        menu.Styles.Add(new Style(selector =>
            selector.OfType<AtomUINavMenu>()
                    .Class("semantic-owner")
                    .Descendant()
                    .Class(SubMenuItemClass))
        {
            Setters = { new Setter(Control.TagProperty, "subMenu.item") }
        });

        ShowInWindow(menu, root, host =>
        {
            Marked<NavMenuItem>(host, SubMenuItemClass)
                .ShouldAllBe(static item => Equals(item.Tag, "subMenu.item"));
            Marked<NavMenuItem>(host, ItemClass).ShouldAllBe(static item => Equals(item.Tag, null));
        });
    }

    [Fact]
    public void Expansion_And_Selection_Do_Not_Change_Marker_Counts()
    {
        var (menu, root, subMenuNode) = CreateMenuWithSubmenu(NavMenuMode.Inline);

        ShowInWindow(menu, root, host =>
        {
            var subMenuContainer = OpenSubMenu(menu, subMenuNode);
            var baseline = Marked(host, SubMenuItemClass).Length;

            subMenuContainer.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, false);
            Dispatcher.UIThread.RunJobs();
            Marked(host, SubMenuItemClass).Length.ShouldBe(baseline);

            subMenuContainer.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
            Dispatcher.UIThread.RunJobs();
            Marked(host, SubMenuItemClass).Length.ShouldBe(baseline);
        });
    }

    private static NavMenuItem OpenSubMenu(AtomUINavMenu menu, NavMenuNode subMenuNode)
    {
        var container = menu.ContainerFromItem(subMenuNode).ShouldBeOfType<NavMenuItem>();
        container.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, true);
        Dispatcher.UIThread.RunJobs();
        return container;
    }

    private static void AssertRuntimePart(
        ControlSemanticDescriptor descriptor,
        string name,
        string selectorClass,
        string selectorRoute,
        Type contractType,
        bool crossVisualRoot = false)
    {
        var part = descriptor.Parts.Single(candidate => candidate.Name == name);
        part.Path.ShouldBe(name);
        part.SelectorClass.ShouldBe(selectorClass);
        part.SelectorRoute.ShouldBe(selectorRoute);
        part.ContractType.ShouldBe(contractType);
        part.Cardinality.ShouldBe(SemanticPartCardinality.Multiple);
        part.Customization.ShouldBe(SemanticPartCustomization.Selector);
        part.CrossVisualRoot.ShouldBe(crossVisualRoot);
        part.CrossNestedOwners.ShouldBeTrue();
        part.RuntimeCreated.ShouldBeTrue();
        part.Since.ShouldBe("6.2.0");
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUINavMenu));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
    }

    [Fact]
    public void Pinned_Open_Declared_Before_Containers_Exist_Still_Opens_Submenu()
    {
        // 复刻 Gallery 语义预览的用法：在容器生成前就声明钉住打开（AXAML 里
        // IsPopupPinnedOpen="True" 会在模板应用、容器生成之前写入属性）。钉住请求以节点为键保存在
        // NavMenu 上，独立于容器生命周期，因此模板应用与 attach 时都不需要容器存在。
        var parent = new NavMenuNode { Header = "Parent" };
        parent.Children.Add(new NavMenuNode { Header = "Child" });

        var menu = new AtomUINavMenu
        {
            Mode                  = NavMenuMode.Vertical,
            IsMotionEnabled       = false,
            ShouldUseOverlayPopup = true,
            IsPopupPinnedOpen     = true
        };
        menu.Items.Add(new NavMenuNode { Header = "Leaf" });
        menu.Items.Add(parent);

        var root   = CreateRoot(menu);
        var window = new AtomUIWindow { Width = 520, Height = 400, Content = root };
        try
        {
            window.Show();
            for (var i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            var container = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            container.IsPopupPinnedOpen.ShouldBeTrue(
                "a declarative pin must reach containers that were created after the property was set");
            container.IsSubMenuOpen.ShouldBeTrue();
            container.Popup.ShouldNotBeNull().IsOpen.ShouldBeTrue();

            // 钉住语义：普通关闭请求不得收起弹层。
            container.Close();
            container.SetCurrentValue(NavMenuItem.IsSubMenuOpenProperty, false);
            Dispatcher.UIThread.RunJobs();

            container.IsSubMenuOpen.ShouldBeTrue();
            container.Popup.ShouldNotBeNull().IsOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void Pinned_Open_Request_Survives_Container_Rebuild_And_Late_Items()
    {
        // 钉住请求必须是独立于容器的持久状态：Mode 切换会让 ItemsPresenter 回收并重建全部容器，
        // 之后追加的条目也会新建容器。只要容器对应的节点仍在钉住路径上，重建后的容器必须重新带上
        // 钉住并保持弹层打开，而不是因为“请求早已消费”而丢失。
        var parent = new NavMenuNode { Header = "Parent" };
        parent.Children.Add(new NavMenuNode { Header = "Child" });

        var menu = new AtomUINavMenu
        {
            Mode                  = NavMenuMode.Vertical,
            IsMotionEnabled       = false,
            ShouldUseOverlayPopup = true
        };
        menu.Items.Add(parent);

        var root   = CreateRoot(menu);
        var window = new AtomUIWindow { Width = 520, Height = 400, Content = root };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            menu.IsPopupPinnedOpen = true;
            Dispatcher.UIThread.RunJobs();
            menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>()
                .IsPopupPinnedOpen.ShouldBeTrue();

            // Inline 与 Vertical 之间往返会重建菜单项容器。
            menu.Mode = NavMenuMode.Inline;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            menu.Mode = NavMenuMode.Vertical;
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();

            var rebuilt = menu.ContainerFromItem(parent).ShouldBeOfType<NavMenuItem>();
            rebuilt.IsPopupPinnedOpen.ShouldBeTrue(
                "the pin request must be re-applied to containers rebuilt after the request was made");
            rebuilt.IsSubMenuOpen.ShouldBeTrue();
            rebuilt.Popup.ShouldNotBeNull().IsOpen.ShouldBeTrue();

            // 钉住路径以节点为键，因此请求写入后追加的条目不会改变已解析的目标。
            menu.Items.Add(new NavMenuNode { Header = "Late" });
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            rebuilt.IsPopupPinnedOpen.ShouldBeTrue();
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [Fact]
    public void IsPopupPinnedOpen_Is_Public_Api_For_Cross_Assembly_Previews()
    {
        // Gallery 的语义预览在另一个程序集里通过 AXAML 声明钉住打开，因此该属性必须公开。
        // 先例：Tour / DropdownButton / AbstractSelect 等已经公开同一属性。
        var property = typeof(AtomUINavMenu).GetProperty(nameof(AtomUINavMenu.IsPopupPinnedOpen));
        property.ShouldNotBeNull();
        property.GetGetMethod().ShouldNotBeNull().IsPublic.ShouldBeTrue();
        property.GetSetMethod().ShouldNotBeNull().IsPublic.ShouldBeTrue();
    }

    /// <summary>
    /// Builds a menu with one first-level leaf, one first-level group and one submenu that itself
    /// contains a nested leaf and a group whose title and list markers are shared across levels.
    /// </summary>
    private static (AtomUINavMenu Menu, VisualLayerManager Root, NavMenuNode SubMenuNode) CreateMenuWithSubmenu(
        NavMenuMode mode)
    {
        var nestedGroup = new NavMenuGroup { Header = "Nested Group" };
        nestedGroup.Entries.Add(new NavMenuNode { Header = "Nested Group Leaf" });

        var parent = new NavMenuNode { Header = "Parent" };
        parent.Entries.Add(new NavMenuNode { Header = "Nested Leaf" });
        parent.Entries.Add(nestedGroup);

        var topGroup = new NavMenuGroup { Header = "Top Group" };
        topGroup.Entries.Add(new NavMenuNode { Header = "Top Group Leaf" });

        var menu = new AtomUINavMenu
        {
            Mode                  = mode,
            IsMotionEnabled       = false,
            ShouldUseOverlayPopup = true
        };
        menu.Items.Add(new NavMenuNode { Header = "Leaf" });
        menu.Items.Add(topGroup);
        menu.Items.Add(parent);

        var root = CreateRoot(menu);
        return (menu, root, parent);
    }

    private static VisualLayerManager CreateRoot(Control content)
    {
        var visualLayerManager = new VisualLayerManager { Child = content };
        var property = typeof(VisualLayerManager).GetProperty(
            "EnablePopupOverlayLayer",
            BindingFlags.Instance | BindingFlags.NonPublic);
        property.ShouldNotBeNull();
        property.SetValue(visualLayerManager, true);
        return visualLayerManager;
    }

    private static void ShowInWindow(AtomUINavMenu menu, VisualLayerManager root, Action<VisualLayerManager> assertion)
    {
        var window = new AtomUIWindow
        {
            Width   = 520,
            Height  = 400,
            Content = root
        };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            if (menu.Mode != NavMenuMode.Inline)
            {
                menu.IsPopupPinnedOpen = true;
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
            }

            assertion(root);
        }
        finally
        {
            window.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    private static T[] Marked<T>(Visual root, string semanticClass) where T : Control
    {
        return root.GetVisualDescendants()
                   .OfType<T>()
                   .Where(control => control.Classes.Contains(semanticClass))
                   .ToArray();
    }

    private static Control[] Marked(Visual root, string semanticClass)
    {
        return root.GetVisualDescendants()
                   .OfType<Control>()
                   .Where(control => control.Classes.Contains(semanticClass))
                   .ToArray();
    }

    private static void AssertThemeMarkers(string relativePath, string[] expectedMarkers)
    {
        var document = XDocument.Load(GetRepoFile(relativePath), LoadOptions.SetLineInfo);
        var literalSemanticMarkers = document.Descendants()
                                             .Attributes()
                                             .Where(static attribute =>
                                                 attribute.Name.LocalName == "Classes" &&
                                                 attribute.Value.Split(
                                                             (char[]?)null,
                                                             StringSplitOptions.RemoveEmptyEntries)
                                                         .Any(static value => value.StartsWith(
                                                             "semantic-",
                                                             StringComparison.Ordinal)))
                                             .ToArray();
        var classPropertyMarkers = document.Descendants()
                                           .SelectMany(static element => element.Attributes()
                                               .Where(static attribute => attribute.Name.LocalName.StartsWith(
                                                   "Classes.semantic-",
                                                   StringComparison.Ordinal))
                                               .Select(attribute => (Element: element, Attribute: attribute)))
                                           .ToArray();

        literalSemanticMarkers.ShouldBeEmpty();
        classPropertyMarkers.ShouldAllBe(static marker =>
            string.Equals(marker.Attribute.Value, "true", StringComparison.OrdinalIgnoreCase));
        classPropertyMarkers.Select(static marker =>
                                $"{marker.Attribute.Name.LocalName["Classes.".Length..]}:{marker.Element.Name.LocalName}")
                            .OrderBy(static value => value, StringComparer.Ordinal)
                            .ShouldBe(expectedMarkers);
    }

    private static string GetRepoFile(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            directory = directory.Parent;
        }

        throw new FileNotFoundException($"Could not locate repository file '{relativePath}'.");
    }
}
