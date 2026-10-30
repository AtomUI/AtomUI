using AtomUI.Theme;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "trigger",
    SelectorClass = "semantic-trigger",
    ContractType = typeof(FloatButton),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
public partial class FloatButtonGroup
{
}
