using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// PopupConfirm 继承 FlyoutHost，弹层由 PopupConfirmFlyout 在运行时创建、跨视觉根，
// 内部 PopupConfirmContainer 为 internal 容器，不作为 public Semantic owner。因此
// PopupConfirm 自持完整契约，语义对齐上游 antd Popconfirm 的语义 DOM
// （root / container / icon / title / content / arrow）：上游弹层根 root 映射为
// popup.root，上游 description 槽位映射为 popup.description，因为 Popover 家族的
// popup.content 已固定表示弹层框体的内容呈现面，不能再用作描述节点。
//
// popup.root / popup.container / popup.content / popup.arrow 四个框体件的 marker 由
// FlyoutHost 家族共享的代码路径注入（Flyout.CreatePresenter 注入 popup.root，
// FlyoutPresenter.OnApplyTemplate 注入 popup.container/content/arrow），此处复用既有
// marker 类而不是重复注入。
//
// popup.icon / popup.title / popup.description / popup.actions 的 marker 静态声明在
// PopupConfirmContainer 自身主题 PopupConfirmContainerTheme.axaml 上；该主题只服务
// PopupConfirm，静态标记不会污染其它控件。全部部件 RuntimeCreated=true：承载节点由
// CreatePresenter 在运行时创建，不在 PopupConfirm 自身模板内；route 因此显式声明为
// 从 owner 逻辑祖先链经 popup.root 下钻的后代路由。
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(FlyoutPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "popup.content",
    SelectorClass = "semantic-popup-content",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.arrow",
    SelectorClass = "semantic-popup-arrow",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-arrow",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowIndicator),
    Since = "6.2.0")]
[SemanticPart(
    "popup.icon",
    SelectorClass = "semantic-popup-icon",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-icon",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.title",
    SelectorClass = "semantic-popup-title",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-title",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "popup.description",
    SelectorClass = "semantic-popup-description",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-description",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-actions",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
public partial class PopupConfirm
{
}
