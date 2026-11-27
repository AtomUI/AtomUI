using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The icon of the button, above the description in the Square shape. Both shape templates carry it; the Square
// shape hides it while Icon is null. FloatButtonIconStyle applies to the icon presenter.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
// The description below the icon. Only the Square shape template has this part, and it is hidden while Content is
// null. FloatButtonContentStyle applies to the content presenter.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
public partial class FloatButton
{
}
