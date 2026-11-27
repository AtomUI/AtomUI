using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The icons inside a button. The part is repeatable: the icon set with Icon and the loading indicator are two
// separate nodes, and ButtonIconStyle applies to each of them. The icon is shown only while Icon is set,
// IsIconVisible is true and the button is not loading.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The label area of a button. It has nothing to show while Content is null.
// ButtonContentStyle applies to it.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class Button
{
}
