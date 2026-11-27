using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The ribbon with its folded corner. It hangs over a top corner of the target, or stands alone when the badge
// has no target. The badge creates it at run time as a child of the badge.
// RibbonBadgeIndicatorStyle applies to the ribbon.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "> .semantic-indicator",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The ribbon's label, a text block inside the ribbon's template. It exists only together with the ribbon.
// RibbonBadgeContentStyle applies to the label.
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
