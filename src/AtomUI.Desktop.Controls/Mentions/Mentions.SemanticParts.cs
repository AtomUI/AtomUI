using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;
using AtomUICandidateList = AtomUI.Desktop.Controls.Primitives.CandidateList;
using AtomUICandidateListItem = AtomUI.Desktop.Controls.Primitives.CandidateListItem;

namespace AtomUI.Desktop.Controls;

// The prefix presenter inside the input frame of the text area.
// MentionsPrefixStyle applies to the presenter.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The content area of the input frame; it holds the placeholder and the editable text.
// MentionsContentStyle applies to the panel.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The placeholder text block inside the content area of the input frame.
// MentionsPlaceholderStyle applies to the text block.
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The text presenter that renders the editable text inside the content area.
// MentionsInputStyle applies to the presenter.
[SemanticPart(
    "input",
    SelectorClass = "semantic-textarea",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-textarea",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
// The clear button in the suffix area of the input frame.
// MentionsClearStyle applies to the button.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// The root border of the candidate popup; it lives in the popup's own visual root while the popup is open.
// MentionsPopupRootStyle applies to the border.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The candidate list inside the popup root; it is hidden while candidates are loaded.
// MentionsPopupListStyle applies to the list.
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    CrossVisualRoot = true,
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list",
    ContractType = typeof(AtomUICandidateList),
    Since = "6.2.0")]
// One row of the candidate list; the list creates one item at runtime for every candidate.
// MentionsPopupListItemStyle applies to every row.
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    CrossVisualRoot = true,
    SelectorRoute = "/template/ .semantic-popup-list >> .semantic-popup-list-item",
    ContractType = typeof(AtomUICandidateListItem),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class Mentions
{
}
