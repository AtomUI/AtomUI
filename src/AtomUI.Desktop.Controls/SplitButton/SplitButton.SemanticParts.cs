using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// 语义对齐上游 Dropdown 的 Semantic DOM：弹层侧五个部件（popup.root / itemTitle / item /
// itemContent / itemIcon）与 DropdownButton 共用共享 MenuFlyout / MenuItem 的运行时注入 marker，
// owner 隔离由生成的专用 Style 沿逻辑祖先链（Popup PlacementTarget = 模板内
// PART_SecondaryButton → SplitButton）达成。与上游不同，AtomUI 的 SplitButton 是
// ContentControl 组合控件，两个触发按钮在 owner 模板内部而非由消费者持有，因此补充发布
// 触发侧 primary / secondary 静态部件供开发者定制。
[SemanticPart(
    "primary",
    SelectorClass = "semantic-primary",
    SelectorRoute = "/template/ .semantic-primary",
    ContractType = typeof(Button),
    Since = "6.2.0")]
[SemanticPart(
    "secondary",
    SelectorClass = "semantic-secondary",
    SelectorRoute = "/template/ .semantic-secondary",
    ContractType = typeof(Button),
    Since = "6.2.0")]
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
public partial class SplitButton
{
}
