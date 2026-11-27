using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The row that holds the status icon and the text column of title and description. The action area, the
// close button and the remaining-time bar are outside it. Present once the template is applied.
// NotificationCardWrapperStyle applies to it.
// The card-level parts other than root belong to the public owner NotificationCard; the list and listContent
// parts belong to WindowNotificationManager. Except for progress, the parts are static nodes of the card's own
// ControlTheme inside the default owner template boundary, so they need no CrossVisualRoot.
[SemanticPart(
    "wrapper",
    SelectorClass = "semantic-wrapper",
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
// The status icon in front of the text. Present only while the card's Icon is not null.
// NotificationCardIconStyle applies to it.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
// The text column to the right of the status icon, holding the title and the description.
// NotificationCardSectionStyle applies to it.
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The title above the description.
// NotificationCardTitleStyle applies to it.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(SelectableTextBlock),
    Since = "6.2.0")]
// The message below the title.
// NotificationCardDescriptionStyle applies to it.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The action area below the description. Present only while Actions is not null.
// NotificationCardActionsStyle applies to it.
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The close button overlaid on the top-right corner of the card.
// NotificationCardCloseStyle applies to it.
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The remaining-time bar along the bottom edge of the card. The card creates it at runtime, only while
// IsShowProgress is set and the card has an expiration, so it is not a template node and needs an explicit
// SelectorRoute.
// NotificationCardProgressStyle applies to it.
[SemanticPart(
    "progress",
    SelectorClass = "semantic-progress",
    SelectorRoute = "/template/ .semantic-progress",
    ContractType = typeof(Control),
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class NotificationCard
{
}
