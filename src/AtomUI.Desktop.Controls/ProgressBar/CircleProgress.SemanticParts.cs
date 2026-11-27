using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace AtomUI.Desktop.Controls;

// The root panel that arranges the ring and the centered progress info. CircleProgressBodyStyle applies to it.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The unfilled ring behind the arc that shows the completed value. CircleProgressRailStyle applies to it.
[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    SelectorRoute = "/template/ .semantic-body > .semantic-rail",
    ContractType = typeof(Shape),
    Since = "6.2.0")]
// The arc that shows the completed value. CircleProgressTrackStyle applies to it.
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-body > .semantic-track",
    ContractType = typeof(Shape),
    Since = "6.2.0")]
// The progress info in the center of the ring: the percentage label or a status icon. Present only while the
// progress info is shown. CircleProgressIndicatorStyle applies to it.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "/template/ .semantic-body > .semantic-indicator",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
public partial class CircleProgress
{
}
