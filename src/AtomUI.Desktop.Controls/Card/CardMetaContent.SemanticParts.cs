using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The block beside the avatar that stacks the title above the description.
// CardMetaContentSectionStyle applies to the block.
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The avatar at the left of the content. It holds an avatar only while Avatar is set.
// CardMetaContentAvatarStyle applies to the presenter that hosts the avatar.
[SemanticPart(
    "avatar",
    SelectorClass = "semantic-avatar",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The title above the description. It holds the header only while Header is set.
// CardMetaContentTitleStyle applies to the presenter that shows the header.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The description below the title. It holds the description only while Content is set.
// CardMetaContentDescriptionStyle applies to the presenter that shows the content.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class CardMetaContent
{
}
