using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The semantic parts follow the Semantic DOM of the Ant Design 6.6.3 Table. The generator synthesizes the
// root implicitly, and header.row has no node of its own in this template, so it is not declared.
// pagination.root and pagination.item expose the Pagination hosts of the template and their page items.
// The section is the border that clips the table content inside the frame. DataGridSectionStyle applies to it.
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    SelectorRoute = "/template/ .semantic-section",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The frame around the column header row. DataGridHeaderWrapperStyle applies to the frame.
[SemanticPart(
    "header.wrapper",
    SelectorClass = "semantic-header-wrapper",
    SelectorRoute = "/template/ .semantic-header-wrapper",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// A column header cell in the column header row. One per column, created at runtime.
// DataGridHeaderCellStyle applies to every column header cell.
[SemanticPart(
    "header.cell",
    SelectorClass = "semantic-header-cell",
    SelectorRoute = ">> .semantic-header-cell",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The title area above the table. Present only while Title is set.
// DataGridTitleStyle applies to the title area.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-title",
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
// The presenter that hosts the data rows below the column header. Hidden while the data source is empty.
// DataGridBodyWrapperStyle applies to the presenter.
[SemanticPart(
    "body.wrapper",
    SelectorClass = "semantic-body-wrapper",
    SelectorRoute = "/template/ .semantic-body-wrapper",
    ContractType = typeof(DataGridRowsPresenter),
    Since = "6.2.0")]
// A data row in the body. One per displayed row, created at runtime.
// DataGridBodyRowStyle applies to every data row.
[SemanticPart(
    "body.row",
    SelectorClass = "semantic-body-row",
    SelectorRoute = ">> .semantic-body-row",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// A data cell of a row. One per column in every displayed row, created at runtime.
// DataGridBodyCellStyle applies to every data cell.
[SemanticPart(
    "body.cell",
    SelectorClass = "semantic-body-cell",
    SelectorRoute = ">> .semantic-body-row >> .semantic-body-cell",
    ContractType = typeof(DataGridCell),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
// The footer area below the table. Present only while Footer is set.
// DataGridFooterStyle applies to the footer area.
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-footer",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The area that holds the column headers and the data rows. DataGridContentStyle applies to this area.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(Grid),
    Since = "6.2.0")]
// The Pagination controls that page the data, one above and one below the table.
// DataGridPaginationRootStyle applies to each of them.
[SemanticPart(
    "pagination.root",
    SelectorClass = "semantic-pagination-root",
    SelectorRoute = "/template/ .semantic-pagination-root",
    ContractType = typeof(Pagination),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// A page item inside a Pagination control, created at runtime.
// DataGridPaginationItemStyle applies to every page item.
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
