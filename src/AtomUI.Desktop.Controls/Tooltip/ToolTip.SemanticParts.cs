using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The border that paints the background and border of the tip and holds the tip content.
// ToolTipContainerStyle applies to the border.
// The node belongs to the template of the arrow-decorated box, so the route crosses that template.
[SemanticPart(
    "container",
    SelectorClass = "semantic-container",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-container",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The arrow that points from the tip to its target. Shown only while the arrow-decorated box shows its arrow.
// ToolTipArrowStyle applies to the arrow.
// The node belongs to the template of the arrow-decorated box, so the route crosses that template.
[SemanticPart(
    "arrow",
    SelectorClass = "semantic-arrow",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-arrow",
    ContractType = typeof(ArrowIndicator),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
public partial class ToolTip
{
}
