using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The status dot, created by the badge at run time. With a target it is rendered in the adorner layer over the
// target's top-right corner, outside the badge's own visual tree, so the route crosses a visual root.
// DotBadgeIndicatorStyle applies to the dot.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "> .semantic-scope-indicator /template/ .semantic-indicator",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Optional,
    CrossVisualRoot = true,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class DotBadge
{
}
