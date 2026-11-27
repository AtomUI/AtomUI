using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The area inside the input frame that presents Content with ContentTemplate.
// ButtonSpinnerContentStyle applies to the presenter.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-scope-frame /template/ .semantic-content",
    ContractType = typeof(ContentPresenter),
    CrossNestedOwners = true,
    Since = "6.2.0")]
// The slot inside the input frame, to the left of the main content, that presents InnerLeftContent with
// InnerLeftContentTemplate. It is hidden while both are unset.
// ButtonSpinnerInnerLeftContentStyle applies to the presenter.
[SemanticPart(
    "innerLeftContent",
    SelectorClass = "semantic-inner-left-content",
    SelectorRoute = "/template/ .semantic-scope-frame /template/ .semantic-inner-left-content",
    ContractType = typeof(ContentPresenter),
    CrossNestedOwners = true,
    Since = "6.2.0")]
// The slot inside the input frame, to the right of the main content, that presents InnerRightContent with
// InnerRightContentTemplate. It is hidden while both are unset.
// ButtonSpinnerInnerRightContentStyle applies to the presenter.
[SemanticPart(
    "innerRightContent",
    SelectorClass = "semantic-inner-right-content",
    SelectorRoute = "/template/ .semantic-scope-frame /template/ .semantic-inner-right-content",
    ContractType = typeof(ContentPresenter),
    CrossNestedOwners = true,
    Since = "6.2.0")]
// The handle inside the input frame that stacks the increment and decrement buttons: at the left edge when
// ButtonSpinnerLocation is Left, otherwise at the right edge. It is never removed: it is only made transparent while
// IsButtonSpinnerVisible is false and, when IsButtonSpinnerFloatable is set, until the pointer is over the control.
// ButtonSpinnerActionsStyle applies to the handle.
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    SelectorRoute = "/template/ .semantic-scope-frame >> .semantic-actions",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
// The upper half of the handle. Clicking it raises the Spin event with SpinDirection.Increase.
// ButtonSpinnerIncreaseButtonStyle applies to the button.
[SemanticPart(
    "increaseButton",
    SelectorClass = "semantic-increase-button",
    SelectorRoute = ">> .semantic-actions /template/ .semantic-increase-button",
    ContractType = typeof(IconButton),
    CrossNestedOwners = true,
    Since = "6.2.0")]
// The lower half of the handle. Clicking it raises the Spin event with SpinDirection.Decrease.
// ButtonSpinnerDecreaseButtonStyle applies to the button.
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
