using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// One entry of the page navigation strip: the previous or next button, a page number, or a button that
// jumps several pages back or forward. The items are created at runtime, one per page or button, once
// the pager's template is applied.
// PaginationItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "/template/ .semantic-scope-nav > .semantic-item",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Pagination
{
}
