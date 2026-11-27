using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The list that lays out the character cells and their separators in a row.
// OtpLineEditCellListStyle applies to the list.
[SemanticPart(
    "cellList",
    SelectorClass = "semantic-cell-list",
    SelectorRoute = "/template/ .semantic-cell-list",
    ContractType = typeof(ItemsControl),
    Since = "6.2.0")]
// One character cell. It repeats: the list creates one per character position at runtime.
// OtpLineEditCellStyle applies to every cell.
[SemanticPart(
    "cell",
    SelectorClass = "semantic-cell",
    SelectorRoute = "/template/ .semantic-cell-list > .semantic-scope-cell > .semantic-cell",
    ContractType = typeof(ContentControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The separator drawn after a character cell. It repeats: the list creates one per cell at runtime, and the
// node is collapsed when the input has no Separator or the cell is the last one.
// OtpLineEditSeparatorStyle applies to every separator.
[SemanticPart(
    "separator",
    SelectorClass = "semantic-separator",
    SelectorRoute = "/template/ .semantic-cell-list > .semantic-scope-cell > .semantic-separator",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class OtpLineEdit
{
}
