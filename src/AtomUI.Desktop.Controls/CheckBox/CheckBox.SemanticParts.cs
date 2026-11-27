using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The check box indicator: the box with the check mark or the indeterminate mark inside it.
// CheckBoxIconStyle applies to the indicator.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The content beside the indicator. Present only when Content is not null.
// CheckBoxLabelStyle applies to it.
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class CheckBox
{
}
