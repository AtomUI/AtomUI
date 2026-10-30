using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// 上游 list 是承载全部 notice 的定位容器，映射到 manager 的隐式 root；listContent 对应
// FeedbackStackPresenter#PART_Items 的排列/顺序/对齐职责（堆叠架构以 ItemsControl 契约暴露）。
// 因此不需要 CrossVisualRoot / RuntimeCreated。
[SemanticPart(
    "listContent",
    SelectorClass = "semantic-list-content",
    ContractType = typeof(ItemsControl),
    Since = "6.2.0")]
public partial class WindowMessageManager
{
}
