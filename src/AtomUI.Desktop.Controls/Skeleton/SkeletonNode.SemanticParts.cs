using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The square block behind the centered content that shows the shimmer. Repeatable, one per placeholder block;
// SkeletonNodeContentStyle applies to each.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
public partial class SkeletonNode
{
}
