using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The radio indicator: the circle with the dot inside it that appears when selected.
// RadioButtonIconStyle applies to the indicator.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The content beside the indicator. Present only when Content is not null.
// RadioButtonLabelStyle applies to it.
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class RadioButton
{
}
