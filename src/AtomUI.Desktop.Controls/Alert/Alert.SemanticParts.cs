using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The status icon before the content. Shown only while IsShowIcon is true.
// The template holds one icon per alert type and shows the one that matches Type.
// AlertIconStyle applies to every icon.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(Icon),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The column that stacks the title and the description. AlertSectionStyle applies to the column.
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The title text. The template holds two title nodes for the same message, a wrapping label and a marquee label,
// and shows only one of them, chosen by IsMessageMarqueeEnabled.
// AlertTitleStyle applies to both.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The description text under the title. Present only when Description is not null or empty.
// AlertDescriptionStyle applies to the label.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(Label),
    Since = "6.2.0")]
// The action area at the trailing end of the alert, before the close button.
// Present only when ExtraAction is not null. AlertActionsStyle applies to the area.
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The close button at the trailing end of the alert. Present only when IsClosable is true.
// AlertCloseStyle applies to the button.
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
public partial class Alert
{
}
