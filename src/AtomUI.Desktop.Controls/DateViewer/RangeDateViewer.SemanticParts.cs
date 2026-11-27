using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The header above a date panel: the period title and the previous and next navigation. There is one per panel:
// two in the two-panel layout and one in the single-panel layout. RangeDateViewerHeaderStyle applies to each header.
[SemanticPart("header", SelectorClass = "semantic-header", ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, Since = "6.2.2")]
// The area under the headers that holds the date panels. RangeDateViewerBodyStyle applies to this area.
[SemanticPart("body", SelectorClass = "semantic-body", ContractType = typeof(Panel), Since = "6.2.2")]
// One calendar panel: the week-day row and the cell grid below it. RangeDateViewerContentStyle applies to each
// panel, whether the viewer shows two panels or a single one.
[SemanticPart("content", SelectorClass = "semantic-content", SelectorRoute = "/template/ .semantic-body > .semantic-content", ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, Since = "6.2.2")]
// One cell of the date grid. The cells are created at runtime inside the calendar panel's own template, so the style
// reaches the cells of every panel through that template. RangeDateViewerCellStyle applies to every cell.
[SemanticPart("cell", SelectorClass = "semantic-cell", SelectorRoute = "/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell", CrossNestedOwners = true, ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, RuntimeCreated = true, Since = "6.2.2")]
// The area below the date value that holds the additional content of a cell. It lives in the cell's own template and
// is created at runtime with the cell. RangeDateViewerCellContentStyle applies to the content area of every cell.
[SemanticPart("cellContent", SelectorClass = "semantic-cell-content", SelectorRoute = "/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell /template/ .semantic-cell-content", CrossNestedOwners = true, ContractType = typeof(ContentControl), Cardinality = SemanticPartCardinality.Multiple, RuntimeCreated = true, Since = "6.2.2")]
public partial class RangeDateViewer
{
}
