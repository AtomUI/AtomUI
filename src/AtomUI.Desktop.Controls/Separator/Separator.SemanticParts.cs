using AtomUI.Controls.Commons;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    ContractType = typeof(SeparatorRail),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class Separator
{
}
