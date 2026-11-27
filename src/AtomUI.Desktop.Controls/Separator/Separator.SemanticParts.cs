using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// The line of the separator, drawn as two segments, one on each side of the title. SeparatorRailStyle applies to
// each segment.
[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    ContractType = typeof(SeparatorRail),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The title between the two line segments of a horizontal separator. It is visible only while a title is set; the
// theme hides it on a vertical separator. SeparatorContentStyle applies to it.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class Separator
{
}
