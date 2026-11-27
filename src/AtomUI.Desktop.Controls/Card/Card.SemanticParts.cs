using AtomUI.Theme;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The header area at the top of the card, which holds the title and the extra content.
// Shown only when the header has something to show. CardHeaderStyle applies to the header frame.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(DashedBorder),
    Since = "6.2.0")]
// The title in the header area, to the left of the extra content. Shown only when the header has something
// to show. CardTitleStyle applies to the presenter of the header.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The extra content at the right end of the header area, next to the title. Shown only when the header has
// something to show. CardExtraStyle applies to the presenter of the extra content.
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The cover area between the header area and the body, spanning the width of the card. It has no visible
// extent while Cover and CoverTemplate are unset. CardCoverStyle applies to the clipping frame around the cover.
[SemanticPart(
    "cover",
    SelectorClass = "semantic-cover",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The body area below the header area and the cover, holding the content or the loading indicator.
// Always present. CardBodyStyle applies to the frame of the body.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The actions row at the bottom of the card, one column per entry of Actions. Shown only while Actions has
// an item. CardActionsStyle applies to the row.
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
public partial class Card
{
}
