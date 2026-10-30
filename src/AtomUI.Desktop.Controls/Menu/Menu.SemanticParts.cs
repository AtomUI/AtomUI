using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// Plain Menu 的 Semantic Part 声明。上游 antd 6.6.3 的 Menu 公开键路径共 12 个：一级 root、item、itemIcon、
// itemContent、itemTitle、list，子菜单 subMenu.item、subMenu.itemIcon、subMenu.itemContent、subMenu.itemTitle、
// subMenu.list，以及 popup.root；Part 名称逐字沿用这些键路径。
//
// Menu、MenuItem、MenuItemGroup 都是 public 类型，但菜单项容器由 Menu / MenuItem / MenuItemGroup 在运行时
// 生成，因此全部 Part 声明在 Menu 上，与 NavMenu 保持同一 owner 模型。
//
// 层级 marker 由 MenuSemanticLevelScope 在容器 prepare 时按语义层级注入：顶层容器只带 .semantic-item /
// .semantic-scope-group，子菜单容器只带 .semantic-sub-menu-item / .semantic-sub-menu-group。Menu 子树之外的
// 复用方（ContextMenu、MenuFlyout、DropdownButton 弹层）不下发语义层级，保持既有 .semantic-item 行为不变。
//
// 菜单项模板同时静态声明两个层级的 icon / content marker（同 NavMenu 做法），由 route 的容器 anchor 决定
// 命中哪一层；分组模板的标题与列表同理。因此 route 以 >> 开头，弹层内部件再加 CrossVisualRoot。
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    ContractType = typeof(MenuItem),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "itemTitle",
    SelectorClass = "semantic-item-title",
    SelectorRoute = ">> .semantic-scope-group /template/ .semantic-item-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = ">> .semantic-scope-group /template/ .semantic-list",
    ContractType = typeof(ItemsPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
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
