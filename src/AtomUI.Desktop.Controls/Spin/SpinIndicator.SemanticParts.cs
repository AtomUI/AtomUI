using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The built-in four-dot panel or the presenter of a custom indicator; only one of them is visible.
// SpinIndicatorContentStyle applies to each of the two nodes.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
public partial class SpinIndicator
{
}
