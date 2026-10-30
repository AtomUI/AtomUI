using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "> .semantic-scope-indicator /template/ .semantic-indicator",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Optional,
    CrossVisualRoot = true,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class CountBadge
{
}
