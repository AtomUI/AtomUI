using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace AtomUI.Desktop.Controls;

// The root panel that arranges the arc and the centered progress info. DashboardProgressBodyStyle applies to it.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The unfilled part of the arc behind the completed arc. DashboardProgressRailStyle applies to it.
[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    SelectorRoute = "/template/ .semantic-body > .semantic-rail",
    ContractType = typeof(Shape),
    Since = "6.2.0")]
// The arc that shows the completed value. It starts at the clockwise end of the gap.
// DashboardProgressTrackStyle applies to it.
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-body > .semantic-track",
    ContractType = typeof(Shape),
    Since = "6.2.0")]
// The progress info in the center of the arc: the percentage label or a status icon. Present only while the
// progress info is shown. DashboardProgressIndicatorStyle applies to it.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "/template/ .semantic-body > .semantic-indicator",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
public partial class DashboardProgress
{
}
