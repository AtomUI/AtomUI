using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// Message 由两个 public owner 组成，与上游语义键一一对应：notice 级的 root / wrapper / icon / title
// 属于 MessageCard；列表级的 list / listContent 属于 WindowMessageManager（其隐式 root 即上游 list）。
// 三个 Part 都是 MessageCard 自身 ControlTheme 的静态模板节点，route 使用默认 owner 模板边界，
// 因此不需要 CrossVisualRoot / RuntimeCreated。
[SemanticPart(
    "wrapper",
    SelectorClass = "semantic-wrapper",
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Avalonia.Controls.SelectableTextBlock),
    Since = "6.2.0")]
public partial class MessageCard
{
}
