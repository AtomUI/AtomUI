using AtomUI.Controls;
using AtomUI.Theme;
using AtomUI.Theme.Schema;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Shouldly;
using Xunit;
using AtomUIMenu = AtomUI.Desktop.Controls.Menu;
using AtomUIMenuItem = AtomUI.Desktop.Controls.MenuItem;

namespace AtomUI.Desktop.Controls.Tests.Menu;

public class MenuSemanticPartTests
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

    static MenuSemanticPartTests()
    {
        AvaloniaTestApp.EnsureInitialized();
    }

    [Fact]
    public void Registered_Descriptor_Exposes_Only_The_Upstream_Key_Paths()
    {
        var registry = Application.Current.ShouldNotBeNull()
                                  .GetThemeManager().ShouldNotBeNull()
                                  .SemanticParts;

        registry.TryGetControl(typeof(AtomUIMenu), out var descriptor).ShouldBeTrue(
            "the plain Menu must own its own Semantic Part descriptor instead of borrowing DropdownButton's");
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
            typeof(AtomUIMenuItem));
        AssertRuntimePart(descriptor, "itemIcon", ItemIconClass,
            $">> .{ItemClass} /template/ .{ItemIconClass}",
            typeof(IconPresenter));
        AssertRuntimePart(descriptor, "itemContent", ItemContentClass,
            $">> .{ItemClass} /template/ .{ItemContentClass}",
            typeof(ContentPresenter));
        AssertRuntimePart(descriptor, "itemTitle", ItemTitleClass,
            $">> .{GroupScopeClass} /template/ .{ItemTitleClass}",
            typeof(ContentPresenter));
        AssertRuntimePart(descriptor, "list", ListClass,
            $">> .{GroupScopeClass} /template/ .{ListClass}",
            typeof(ItemsPresenter));

        AssertRuntimePart(descriptor, "subMenu.item", SubMenuItemClass,
            $">> .{SubMenuItemClass}",
            typeof(AtomUIMenuItem),
            crossVisualRoot: true);
        AssertRuntimePart(descriptor, "subMenu.itemIcon", SubMenuItemIconClass,
            $">> .{SubMenuItemClass} /template/ .{SubMenuItemIconClass}",
            typeof(IconPresenter),
            crossVisualRoot: true);
        AssertRuntimePart(descriptor, "subMenu.itemContent", SubMenuItemContentClass,
            $">> .{SubMenuItemClass} /template/ .{SubMenuItemContentClass}",
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

        AssertRuntimePart(descriptor, "popup.root", PopupRootClass,
            $">> .{PopupRootClass}",
            typeof(Border),
            crossVisualRoot: true);
    }

    private static void AssertRoot(SemanticPartDescriptor part)
    {
        part.Path.ShouldBe("root");
        part.SelectorClass.ShouldBeNull();
        part.ContractType.ShouldBe(typeof(AtomUIMenu));
        part.Cardinality.ShouldBe(SemanticPartCardinality.Single);
        part.Customization.ShouldBe(SemanticPartCustomization.Root);
        part.CrossVisualRoot.ShouldBeFalse();
        part.RuntimeCreated.ShouldBeFalse();
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
}
