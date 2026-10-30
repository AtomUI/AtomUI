using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// MessageBox 复用 Dialog 的 DialogSurface/OverlayDialogHeader 视觉树，公开与 Dialog 完全相同的 8 个 Semantic Part。
// 生成器按 public Control 生成 descriptor，不继承基类 descriptor（先例：SimplePagination 对 Pagination），
// 因此这里重复声明同一组契约；字段与路由语义见 semantic-part.md 与 Dialog.SemanticParts.cs，不在本文件重复注释。
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
public partial class MessageBox
{
}
