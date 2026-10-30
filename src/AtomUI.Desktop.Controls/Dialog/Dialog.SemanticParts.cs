using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// 与上游 antd Modal Semantic DOM（_semantic.tsx，9 槽位）对齐；root 隐式，wrapper 映射 Surface motion 容器。
// Dialog 没有 ControlTemplate，全部部件位于运行时创建的 DialogSurface / OverlayDialogPresenter /
// OverlayDialogHeader / OverlayDialogMask 模板中，因此统一 CrossVisualRoot + CrossNestedOwners + RuntimeCreated，
// marker 静态声明在四个宿主主题上（见 semantic-part.md）。
//
// 路由必须使用 `.semantic-scope-*` 锚点收窄，不能只写宽泛的 `>>`：Dialog body 内嵌 Skeleton（加载态常驻），
// 用户内容还常含 Card/Tooltip/Spin 等语义 owner，它们会发布同名 `semantic-header`/`semantic-title`/`semantic-body`/
// `semantic-footer`/`semantic-container`。宽泛 descendant 会把这些嵌套部件一并命中（系统文档 §3.3 明确禁止公共
// Selector 使用宽泛 descendant）。
[SemanticPart("mask",
    SelectorClass = "semantic-mask",
    SelectorRoute = ">> .semantic-scope-mask /template/ .semantic-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart("wrapper",
    SelectorClass = "semantic-wrapper",
    SelectorRoute = ">> .semantic-scope-presenter > .semantic-wrapper",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.Control),
    Since = "6.2.0")]
[SemanticPart("container",
    SelectorClass = "semantic-container",
    SelectorRoute = ">> .semantic-scope-frame-host > .semantic-container",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart("header",
    SelectorClass = "semantic-header",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart("title",
    SelectorClass = "semantic-title",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart("body",
    SelectorClass = "semantic-body",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart("footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart("close",
    SelectorClass = "semantic-close",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-close",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.Button),
    Since = "6.2.0")]
public partial class Dialog
{
}
