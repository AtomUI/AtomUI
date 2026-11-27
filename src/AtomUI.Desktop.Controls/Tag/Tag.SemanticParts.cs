using AtomUI.Controls;
using AtomUI.Theme;

namespace AtomUI.Desktop.Controls;

// The icon to the left of the tag text. Visible only while the tag has an icon.
// TagIconStyle applies to the icon.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    SelectorRoute = "/template/ .semantic-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The text label between the icon and the close button.
// TagContentStyle applies to the label.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(TextBlock),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The close button to the right of the tag text. Visible only while the tag is closable.
// TagCloseStyle applies to the button.
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    SelectorRoute = "/template/ .semantic-close",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
public partial class Tag
{
}
