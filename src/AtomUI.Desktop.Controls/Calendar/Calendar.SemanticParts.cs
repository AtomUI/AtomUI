using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
[SemanticPart(
    "item",
    SelectorClass = "semantic-cell",
    SelectorRoute = "/template/ .semantic-content /template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell",
    CrossNestedOwners = true,
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
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
