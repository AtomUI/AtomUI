using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-list",
    ContractType = typeof(ItemsControl),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "/template/ .semantic-list > .semantic-item",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Upload
{
}
