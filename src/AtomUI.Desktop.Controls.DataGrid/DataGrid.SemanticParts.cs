using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// 语义部件对齐 Ant Design 6.6.3 Table Semantic DOM（root 由生成器隐式合成；
// header.row 在本控件模板中没有独立节点，不声明；pagination.root/item 由 DataGrid 暴露
// 模板内 Pagination 宿主与其页码项，与上游 Table 的 pagination 区域一致）。
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    SelectorRoute = "/template/ .semantic-section",
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "header.wrapper",
    SelectorClass = "semantic-header-wrapper",
    SelectorRoute = "/template/ .semantic-header-wrapper",
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "header.cell",
    SelectorClass = "semantic-header-cell",
    SelectorRoute = ">> .semantic-header-cell",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-title",
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
[SemanticPart(
    "body.wrapper",
    SelectorClass = "semantic-body-wrapper",
    SelectorRoute = "/template/ .semantic-body-wrapper",
    ContractType = typeof(DataGridRowsPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "body.row",
    SelectorClass = "semantic-body-row",
    SelectorRoute = ">> .semantic-body-row",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "body.cell",
    SelectorClass = "semantic-body-cell",
    SelectorRoute = ">> .semantic-body-row >> .semantic-body-cell",
    ContractType = typeof(DataGridCell),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-footer",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(Grid),
    Since = "6.2.0")]
[SemanticPart(
    "pagination.root",
    SelectorClass = "semantic-pagination-root",
    SelectorRoute = "/template/ .semantic-pagination-root",
    ContractType = typeof(Pagination),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "pagination.item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-pagination-root >> .semantic-item",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class DataGrid
{
}
