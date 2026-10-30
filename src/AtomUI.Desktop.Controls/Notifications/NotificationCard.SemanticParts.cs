using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// NotificationCard 的 Semantic Part 声明。模板内的节点拓扑为：
//   root(NotificationCard) > wrapper > (icon, section > (title, description)) + actions + close + progress
// 除 root 外的卡片级 Part 全部属于 public owner NotificationCard；list / listContent 属于服务宿主
// WindowNotificationManager。progress 由 NotificationCard 运行时创建（仅在显示进度且未持久化时存在），
// 因此声明 RuntimeCreated 并给出显式 SelectorRoute；其余 Part 都是 NotificationCard 自身 ControlTheme
// 的静态模板节点，使用默认 owner 模板边界，不需要 CrossVisualRoot。
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
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(SelectableTextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
[SemanticPart(
    "progress",
    SelectorClass = "semantic-progress",
    SelectorRoute = "/template/ .semantic-progress",
    ContractType = typeof(Control),
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class NotificationCard
{
}
