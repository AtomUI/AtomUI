using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using AvaloniaButton = Avalonia.Controls.Button;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    SelectorRoute = "/template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-content > .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(AvaloniaButton),
    Since = "6.2.0")]
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item",
    CrossNestedOwners = true,
    ContractType = typeof(Tag),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "itemContent",
    SelectorClass = "semantic-item-content",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-content",
    CrossNestedOwners = true,
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "itemRemove",
    SelectorClass = "semantic-item-remove",
    SelectorRoute = ">> .semantic-scope-tags >> .semantic-item /template/ .semantic-item-remove",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
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
    SelectorRoute = "/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list",
    CrossVisualRoot = true,
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    SelectorRoute = "/template/ .semantic-popup-root > .semantic-scope-view /template/ .semantic-scope-frame >> .semantic-popup-list-item",
    CrossVisualRoot = true,
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    RuntimeCreated = true,
    Since = "6.2.0")]
public partial class Cascader
{
}
