using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Shapes;
using AvaloniaButton = Avalonia.Controls.Button;
using AvaloniaBorder = Avalonia.Controls.Border;

namespace AtomUI.Desktop.Controls;

// 弹层内容承载在 Popup 的独立视觉根中；遮罩是 TopLevel VisualLayerManager 中的
// 共享 TourLayer，经逻辑父挂载归属当前打开的 Tour，因此全部部件均为 CrossVisualRoot。
// cover/close/header/title/description/footer/actions/indicators/indicator 的标记
// 由 TourStep / TourStepsView / DefaultTourIndicator 的模板携带，路由越过 Tour
// 自身模板边界，声明 CrossNestedOwners 以跨模板资产校验 marker。
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
[SemanticPart(
    "popup.mask",
    SelectorClass = "semantic-popup-mask",
    SelectorRoute = ">> .semantic-popup-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart(
    "popup.section",
    SelectorClass = "semantic-container",
    SelectorRoute = "/template/ .semantic-popup-root /template/ .semantic-container",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaBorder),
    Since = "6.2.0")]
[SemanticPart(
    "popup.cover",
    SelectorClass = "semantic-popup-cover",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-cover",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.close",
    SelectorClass = "semantic-popup-close",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-close",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
[SemanticPart(
    "popup.header",
    SelectorClass = "semantic-popup-header",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaBorder),
    Since = "6.2.0")]
[SemanticPart(
    "popup.title",
    SelectorClass = "semantic-popup-title",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.description",
    SelectorClass = "semantic-popup-description",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-description",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(AvaloniaBorder),
    Since = "6.2.0")]
[SemanticPart(
    "popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-actions",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "popup.indicators",
    SelectorClass = "semantic-popup-indicators",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-indicators",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.indicator",
    SelectorClass = "semantic-popup-indicator",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-indicator",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Ellipse),
    Since = "6.2.0")]
public partial class Tour
{
}
