using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// One item of the masonry layout: the child itself for children placed directly, or the container generated
// for a bound data item. Repeated, one per item, and created at runtime rather than by the template.
// MasonryItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Masonry
{
}
