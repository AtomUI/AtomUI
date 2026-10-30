using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart("header", SelectorClass = "semantic-header", ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, Since = "6.2.2")]
[SemanticPart("body", SelectorClass = "semantic-body", ContractType = typeof(Panel), Since = "6.2.2")]
[SemanticPart("content", SelectorClass = "semantic-content", SelectorRoute = "/template/ .semantic-body > .semantic-content", ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, Since = "6.2.2")]
[SemanticPart("cell", SelectorClass = "semantic-cell", SelectorRoute = "/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell", CrossNestedOwners = true, ContractType = typeof(TemplatedControl), Cardinality = SemanticPartCardinality.Multiple, RuntimeCreated = true, Since = "6.2.2")]
[SemanticPart("cellContent", SelectorClass = "semantic-cell-content", SelectorRoute = "/template/ .semantic-body > .semantic-content /template/ .semantic-scope-panel > .semantic-scope-cells > .semantic-cell /template/ .semantic-cell-content", CrossNestedOwners = true, ContractType = typeof(ContentControl), Cardinality = SemanticPartCardinality.Multiple, RuntimeCreated = true, Since = "6.2.2")]
public partial class RangeDateViewer
{
}
