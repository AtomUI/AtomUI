using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// Semantic Part declarations of NavMenu. The upstream Menu exposes 12 semantic key paths: root, item,
// itemIcon, itemContent, itemTitle and list at the top level; subMenu.item, subMenu.itemIcon,
// subMenu.itemContent, subMenu.itemTitle and subMenu.list in a submenu; and popup.root. The part names reuse
// those key paths verbatim.
// NavMenu is the only public owner. NavMenuItem, NavMenuGroupItem and NavMenuDividerItem are internal containers
// that can neither hold a descriptor nor serve as a ContractType, so every part is declared on NavMenu, and a
// container-level ContractType is the nearest public base class.
// When a container is prepared, NavMenuEntryContainerCoordinator adds a mutually exclusive level marker to each
// item and group container:
//   top-level container  .semantic-item          / .semantic-scope-group
//   submenu container    .semantic-sub-menu-item / .semantic-sub-menu-group
// Every route therefore starts with >>: the anchor is a runtime-created container that /template/ cannot reach,
// and the popup frame is in the property-value subtree of a Popup. CrossNestedOwners is set because the
// generator's route syntax requires it. A marker is added by a static template or by control code; the generator
// does not verify it statically against the owner's theme assets, and NavMenuSemanticPartTests proves that the
// routes resolve.
// A menu item placed directly in the menu, at the top level: its whole container. One instance per top-level
// item, created at runtime.
// NavMenuItemStyle applies to every top-level item container.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    ContractType = typeof(HeaderedSelectingItemsControl),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The icon in the header of a top-level menu item. One per top-level item, created at runtime; shown only when
// the item has an icon.
// NavMenuItemIconStyle applies to the icon.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = ">> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The content area in the header of a top-level menu item. It shows the item title or custom header content.
// One per top-level item, created at runtime.
// NavMenuItemContentStyle applies to it.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The title of a group placed directly in the menu, at the top level. One per top-level group, created at
// runtime; the title row is hidden in Horizontal mode and when IsInlineCollapsed is set.
// NavMenuItemTitleStyle applies to every top-level group title.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = ">> .semantic-scope-group /template/ .semantic-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The list of entries below the title of a top-level group. One per top-level group, created at runtime.
// NavMenuListStyle applies to every top-level group list.
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = ">> .semantic-scope-group /template/ .semantic-list",
    ContractType = typeof(ItemsPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// A menu item below the top level, inside a submenu: its whole container. One per submenu item, created at
// runtime; it can lie in a popup, outside the menu's visual root.
// NavMenuSubMenuItemStyle applies to every submenu item container.
[SemanticPart(
    "subMenu.item",
    SelectorClass = "semantic-sub-menu-item",
    SelectorRoute = ">> .semantic-sub-menu-item",
    ContractType = typeof(HeaderedSelectingItemsControl),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The icon in the header of a submenu item. One per submenu item, created at runtime; shown only when the item
// has an icon.
// NavMenuSubMenuItemIconStyle applies to the icon.
[SemanticPart(
    "subMenu.itemIcon",
    SelectorClass = "semantic-sub-menu-item-icon",
    SelectorRoute = ">> .semantic-sub-menu-item /template/ .semantic-scope-header /template/ .semantic-sub-menu-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The content area in the header of a submenu item. It shows the item title or custom header content.
// One per submenu item, created at runtime.
// NavMenuSubMenuItemContentStyle applies to it.
[SemanticPart(
    "subMenu.itemContent",
    SelectorClass = "semantic-sub-menu-item-content",
    SelectorRoute = ">> .semantic-sub-menu-item /template/ .semantic-scope-header /template/ .semantic-sub-menu-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The title of a group nested inside a submenu, below the top level. One per nested group, created at runtime.
// NavMenuSubMenuItemTitleStyle applies to every nested group title.
[SemanticPart(
    "subMenu.itemTitle",
    SelectorClass = "semantic-sub-menu-item-title",
    SelectorRoute = ">> .semantic-sub-menu-group /template/ .semantic-sub-menu-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The list of entries below the title of a group nested inside a submenu. One per nested group, created at
// runtime.
// NavMenuSubMenuListStyle applies to every nested group list.
[SemanticPart(
    "subMenu.list",
    SelectorClass = "semantic-sub-menu-list",
    SelectorRoute = ">> .semantic-sub-menu-group /template/ .semantic-sub-menu-list",
    ContractType = typeof(ItemsPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The frame of a submenu popup: the border around the items of an open submenu. One per submenu popup, created
// at runtime in the child subtree of a Popup, outside the menu's visual root.
// NavMenuPopupRootStyle applies to every submenu popup frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class NavMenu
{
}
