using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The block that stands in for the button and shows the shimmer. Repeatable, one per placeholder block;
// SkeletonButtonContentStyle applies to each.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
public partial class SkeletonButton
{
}
