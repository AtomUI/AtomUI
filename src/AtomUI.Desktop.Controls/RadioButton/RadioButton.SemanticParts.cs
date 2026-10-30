using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class RadioButton
{
}
