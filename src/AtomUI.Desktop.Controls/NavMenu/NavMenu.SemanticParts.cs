using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// NavMenu 的 Semantic Part 声明。上游 Menu 公开的 Semantic 键路径共 12 个：一级 root、item、itemIcon、
// itemContent、itemTitle、list，子菜单 subMenu.item、subMenu.itemIcon、subMenu.itemContent、subMenu.itemTitle、
// subMenu.list，以及 popup.root；Part 名称逐字沿用这些键路径。
//
// 只有 NavMenu 是 public owner。NavMenuItem、NavMenuGroupItem、NavMenuDividerItem 都是 internal 容器，
// 既不能持有 descriptor，也不能作为 ContractType，因此全部 Part 声明在 NavMenu 上，容器级 ContractType
// 使用其最近 public 基类。
//
// 菜单项与分组容器由 NavMenuEntryContainerCoordinator 在 prepare 时按语义层级注入互斥 marker：
//   一级容器  .semantic-item        / .semantic-scope-group
//   子菜单容器 .semantic-sub-menu-item / .semantic-sub-menu-group
// 因此 route 一律以 >> 开头（锚点位于运行时创建容器，无法用 /template/ 到达；弹层框体还位于 Popup 的
// 属性值子树），按生成器 route 语法要求声明 CrossNestedOwners。marker 由静态模板或控件代码添加，
// 生成器不按 owner 主题资产做静态校验，可达性由 NavMenuSemanticPartTests 证明。
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    ContractType = typeof(HeaderedSelectingItemsControl),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "itemIcon",
    SelectorClass = "semantic-item-icon",
    SelectorRoute = ">> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-scope-header /template/ .semantic-item-content",
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
    ContractType = typeof(HeaderedSelectingItemsControl),
    Cardinality = SemanticPartCardinality.Multiple,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    CrossVisualRoot = true,
    Since = "6.2.0")]
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
public partial class NavMenu
{
}
