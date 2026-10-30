using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
public partial class ToggleSwitch
{
}
