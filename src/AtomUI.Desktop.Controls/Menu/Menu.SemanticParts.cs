using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// Semantic Parts of the plain Menu. The Menu of upstream antd 6.6.3 has 12 public key paths in total: root, item,
// itemIcon, itemContent, itemTitle and list at the first level; subMenu.item, subMenu.itemIcon, subMenu.itemContent,
// subMenu.itemTitle and subMenu.list; and popup.root. The Part names are these key paths verbatim.
//
// Menu, MenuItem and MenuItemGroup are public types, but the menu item containers are created at runtime by
// Menu / MenuItem / MenuItemGroup, so every Part is declared on Menu. This is the same owner model as NavMenu.
//
// MenuSemanticLevelScope injects the level markers when a container is prepared. A top-level container carries only
// .semantic-item / .semantic-scope-group, and a submenu container only .semantic-sub-menu-item /
// .semantic-sub-menu-group. Reusers outside the Menu subtree (the popups of ContextMenu, MenuFlyout and
// DropdownButton) pass no level and keep the existing .semantic-item behavior.
//
// The menu item template declares the icon / content markers of both levels statically, as NavMenu does, and the
// container anchor of the route decides which level is hit. The title and list of the group template work the
// same way. That is why every route starts with >>, and the parts inside a popup add CrossVisualRoot.
//
// One entry of the first level of the menu bar. There is one container per top-level item, created at runtime.
// MenuItemStyle applies to each of these item containers.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    ContractType = typeof(MenuItem),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The icon of an entry in the first level of the menu bar. It is visible only while the item has an icon.
// MenuItemIconStyle applies to the icon of each such entry.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The text of an entry in the first level of the menu bar. It presents the Header of the item.
// MenuItemContentStyle applies to the text area of each such entry.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The title of a group of entries in the first level of the menu bar. One per group container.
// MenuItemTitleStyle applies to the title of each group.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = ">> .semantic-scope-group /template/ .semantic-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The list of the entries inside a group of the first menu bar level. One per group container.
// MenuListStyle applies to each list.
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = ">> .semantic-scope-group /template/ .semantic-list",
    ContractType = typeof(ItemsPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
// One entry inside a submenu, at any nesting depth. There is one container per submenu item, created at runtime
// in the separate visual root of the popup. MenuSubMenuItemStyle applies to each submenu entry.
[SemanticPart(
    "subMenu.item",
    SelectorClass = "semantic-sub-menu-item",
    SelectorRoute = ">> .semantic-sub-menu-item",
    ContractType = typeof(MenuItem),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The icon of a submenu entry. It is visible only while the item has an icon.
// MenuSubMenuItemIconStyle applies to the icon of each submenu entry.
[SemanticPart(
    "subMenu.itemIcon",
    SelectorClass = "semantic-sub-menu-item-icon",
    SelectorRoute = ">> .semantic-sub-menu-item /template/ .semantic-sub-menu-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The text of a submenu entry. It presents the Header of the item.
// MenuSubMenuItemContentStyle applies to the text area of each submenu entry.
[SemanticPart(
    "subMenu.itemContent",
    SelectorClass = "semantic-sub-menu-item-content",
    SelectorRoute = ">> .semantic-sub-menu-item /template/ .semantic-sub-menu-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
// The title of a group inside a submenu. One per group container.
// MenuSubMenuItemTitleStyle applies to the title of each group.
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
// The list of the entries inside a group of a submenu. One per group container.
// MenuSubMenuListStyle applies to each list.
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
// The root frame of a submenu popup. One per submenu popup, in the separate visual root that exists only while
// the submenu is open. MenuPopupRootStyle applies to each popup root.
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
public partial class Menu
{
}
