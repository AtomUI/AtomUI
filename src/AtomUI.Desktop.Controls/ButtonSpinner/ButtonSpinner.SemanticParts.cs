using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-scope-frame /template/ .semantic-content",
    ContractType = typeof(ContentPresenter),
    CrossNestedOwners = true,
    Since = "6.2.0")]
[SemanticPart(
    "innerLeftContent",
    SelectorClass = "semantic-inner-left-content",
    SelectorRoute = "/template/ .semantic-scope-frame /template/ .semantic-inner-left-content",
    ContractType = typeof(ContentPresenter),
    CrossNestedOwners = true,
    Since = "6.2.0")]
[SemanticPart(
    "innerRightContent",
    SelectorClass = "semantic-inner-right-content",
    SelectorRoute = "/template/ .semantic-scope-frame /template/ .semantic-inner-right-content",
    ContractType = typeof(ContentPresenter),
    CrossNestedOwners = true,
    Since = "6.2.0")]
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    SelectorRoute = "/template/ .semantic-scope-frame >> .semantic-actions",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
[SemanticPart(
    "increaseButton",
    SelectorClass = "semantic-increase-button",
    SelectorRoute = ">> .semantic-actions /template/ .semantic-increase-button",
    ContractType = typeof(IconButton),
    CrossNestedOwners = true,
    Since = "6.2.0")]
[SemanticPart(
    "decreaseButton",
    SelectorClass = "semantic-decrease-button",
    SelectorRoute = ">> .semantic-actions /template/ .semantic-decrease-button",
    ContractType = typeof(IconButton),
    CrossNestedOwners = true,
    Since = "6.2.0")]
public partial class ButtonSpinner
{
}
