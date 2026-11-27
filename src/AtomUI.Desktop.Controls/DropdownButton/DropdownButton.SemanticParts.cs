using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The part names follow the semantic DOM of Ant Design's Dropdown: all of its parts are on the popup side
// (root / itemTitle / item / itemContent / itemIcon) and the trigger side has none. AtomUI keeps root as the
// owner's own implicit part, so Ant Design's popup root is named popup.root here, and itemTitle is Ant
// Design's group title (ant-menu-item-group-title).
// The root box (ArrowDecoratedBox) of the drop-down popup, created at run time outside the owner's subtree.
// DropdownButtonPopupRootStyle applies to the box.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
// The title of an item group in the drop-down menu, hosted in the title ContentPresenter of a MenuItemGroup.
// One part per group, created at run time in the popup, so the route crosses the nested group owner into its
// template. DropdownButtonItemTitleStyle applies to every group title.
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = ">> .semantic-item-title-group /template/ .semantic-item-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// A menu item of the drop-down menu: one part per item, created at run time in the popup outside the owner's
// subtree. DropdownButtonItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(MenuItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The content of a menu item, in the item's own template: one part per item.
// DropdownButtonItemContentStyle applies to every item content.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-content",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The icon of a menu item, in the item's own template: one part per item.
// DropdownButtonItemIconStyle applies to every item icon.
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-icon",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
public partial class DropdownButton
{
}
