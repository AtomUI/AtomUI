using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// manager 的隐式 root 是承载全部通知卡片的容器；listContent 对应
// FeedbackStackPresenter#PART_Items 的排列 / 顺序 / 对齐职责（堆叠架构以 ItemsControl 契约暴露）。
// 因此不需要 CrossVisualRoot / RuntimeCreated。
[SemanticPart(
    "listContent",
    SelectorClass = "semantic-list-content",
    ContractType = typeof(ItemsControl),
    Since = "6.2.0")]
public partial class WindowNotificationManager
{
}
