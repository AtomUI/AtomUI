using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaButton = Avalonia.Controls.Button;
using AtomUICandidateList = AtomUI.Desktop.Controls.Primitives.CandidateList;
using AtomUICandidateListItem = AtomUI.Desktop.Controls.Primitives.CandidateListItem;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "input",
    SelectorClass = "semantic-textarea",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-textarea",
    ContractType = typeof(TextPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-input >> .semantic-scope-input-frame /template/ .semantic-scope-suffix > .semantic-suffix > .semantic-clear",
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    CrossVisualRoot = true,
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list",
    ContractType = typeof(AtomUICandidateList),
    Since = "6.2.0")]
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
