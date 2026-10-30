using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// 语义对齐上游 antd Dropdown 的 Semantic DOM：antd 的 Dropdown 语义部件全部位于弹层侧
// （root / itemTitle / item / itemContent / itemIcon），没有触发器侧部件。AtomUI 保留
// `root` 作为 owner 自身的隐式 Part，因此 antd 的弹层根 `root` 映射为 `popup.root`；
// `itemTitle` 对应 antd 的分组标题（ant-menu-item-group-title），AtomUI 用 MenuItemGroup
// 的标题 ContentPresenter 承载。
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
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
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(MenuItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-item /template/ .semantic-item-content",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
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
