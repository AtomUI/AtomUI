using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "dragger",
    SelectorClass = "semantic-dragger",
    SelectorRoute = "/template/ .semantic-scope-panel > .semantic-scope-handle /template/ .semantic-dragger",
    ContractType = typeof(Thumb),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "panel",
    SelectorClass = "semantic-panel",
    SelectorRoute = "/template/ .semantic-scope-panel > .semantic-panel",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Splitter
{
}
