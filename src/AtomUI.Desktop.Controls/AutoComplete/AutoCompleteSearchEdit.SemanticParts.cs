using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;
using AtomUICandidateList = AtomUI.Desktop.Controls.Primitives.CandidateList;
using AtomUICandidateListItem = AtomUI.Desktop.Controls.Primitives.CandidateListItem;

namespace AtomUI.Desktop.Controls;

// The prefix area before the text, inside the input. AutoCompleteSearchEditPrefixStyle applies to the area.
// The node is in the template of the input frame of the nested search box, so the route crosses the input
// scope and that frame template.
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The area of the input that holds the text presenter and the placeholder.
// AutoCompleteSearchEditContentStyle applies to the area.
// The node is in the template of the nested search box, so the route crosses the input scope.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The placeholder text inside the content area. AutoCompleteSearchEditPlaceholderStyle applies to the text.
// The node is in the template of the nested search box, so the route crosses the input scope.
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The text presenter that draws the entered text. AutoCompleteSearchEditInputStyle applies to the presenter.
// The node is in the template of the nested search box, so the route crosses the input scope.
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-input",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
// The button that clears the entered text. AutoCompleteSearchEditClearStyle applies to the button.
// The node is in the template of the input frame of the nested search box, so the route crosses the input
// scope and that frame template.
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
// The root of the suggestion popup. It lives in a separate popup visual root, so the route crosses visual roots.
// AutoCompleteSearchEditPopupRootStyle applies to the root.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The list of suggestions inside the popup. It lives in the popup visual root.
// AutoCompleteSearchEditPopupListStyle applies to the list.
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    CrossVisualRoot = true,
    SelectorRoute = "/template/ .semantic-popup-root > .semantic-popup-list",
    ContractType = typeof(AtomUICandidateList),
    Since = "6.2.0")]
// One suggestion row of the popup list. The rows are created at runtime, one per suggestion, and live in the
// popup visual root. AutoCompleteSearchEditPopupListItemStyle applies to every row.
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    CrossVisualRoot = true,
    SelectorRoute = "/template/ .semantic-popup-list >> .semantic-popup-list-item",
    ContractType = typeof(AtomUICandidateListItem),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class AutoCompleteSearchEdit
{
}
