using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The header above the calendar: the period title and the previous and next navigation.
// It is shown only while ShowHeader is true. DateViewerHeaderStyle applies to the header.
[SemanticPart("header", SelectorClass = "semantic-header", ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Optional, Since = "6.2.2")]
// The area between the header and the footer that holds the calendar panels.
// DateViewerBodyStyle applies to this area.
[SemanticPart("body", SelectorClass = "semantic-body", ContractType = typeof(Panel), Since = "6.2.2")]
// One calendar panel: the week-day row and the cell grid below it. The range viewer can show two panels,
// and DateViewerContentStyle applies to each of them.
[SemanticPart("content", SelectorClass = "semantic-content", SelectorRoute = "/template/ .semantic-body > .semantic-content", ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, Since = "6.2.2")]
// One cell of the date grid. The cells are created at runtime inside the calendar panel's own template, so the
// style reaches them through that template. DateViewerCellStyle applies to every cell. The background, border
// and corner radius set on a cell are drawn by its rounded value frame.
[SemanticPart("cell", SelectorClass = "semantic-cell", SelectorRoute = "/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell", CrossNestedOwners = true, ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, RuntimeCreated = true, Since = "6.2.2")]
// The area below the date value that holds the additional content of a cell. It lives in the cell's own template
// and is created at runtime with the cell. It stays hidden unless the cell has content to show.
// DateViewerCellContentStyle applies to the content area of every cell.
[SemanticPart("cellContent", SelectorClass = "semantic-cell-content", SelectorRoute = "/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell /template/ .semantic-cell-content", CrossNestedOwners = true, ContractType = typeof(ContentControl), Cardinality = SemanticPartCardinality.Multiple, RuntimeCreated = true, Since = "6.2.2")]
// The default footer under the calendar panels, which holds the Today link.
// It is shown only while IsDefaultFooterVisible is true. DateViewerFooterStyle applies to the footer.
[SemanticPart("footer", SelectorClass = "semantic-footer", ContractType = typeof(DashedBorder), Cardinality = SemanticPartCardinality.Optional, Since = "6.2.2")]
public partial class DateViewer
{
}
