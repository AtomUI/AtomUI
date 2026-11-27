using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The header presenter above the body: the default header, or the content of HeaderTemplate when that is set.
// CalendarHeaderStyle applies to the presenter.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The body below the header. It holds the date grid with the range bars drawn above it, and the separator line
// of the compact layout. CalendarBodyStyle applies to the body.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The date grid inside the body. CalendarContentStyle applies to it.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// One date or month cell of the grid. The date panel creates the cells at runtime, one per day or month shown,
// inside its own nested templates, so the route crosses them. CalendarItemStyle applies to every cell.
[SemanticPart(
    "item",
    SelectorClass = "semantic-cell",
    SelectorRoute = "/template/ .semantic-content /template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell",
    CrossNestedOwners = true,
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The content area inside one date or month cell, created at runtime together with its cell, one per cell.
// CalendarItemContentStyle applies to every cell content.
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-cell-content",
    SelectorRoute = "/template/ .semantic-content /template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell /template/ .semantic-cell-content",
    CrossNestedOwners = true,
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Calendar
{
}
