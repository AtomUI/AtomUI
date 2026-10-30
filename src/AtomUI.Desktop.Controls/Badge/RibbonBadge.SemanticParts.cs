using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "> .semantic-indicator",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "> .semantic-indicator /template/ .semantic-content",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class RibbonBadge
{
}
