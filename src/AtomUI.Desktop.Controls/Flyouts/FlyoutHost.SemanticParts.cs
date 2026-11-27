using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The presenter that shows the flyout content; it is the whole surface of the open popup. It is created in
// code by Flyout.CreatePresenter and lives in the popup's own visual root, so the route crosses visual roots.
// FlyoutHostPopupRootStyle applies to the presenter.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(FlyoutPresenter),
    Since = "6.2.0")]
// The panel inside the popup that wraps the flyout content. The border belongs to a template shared with
// other arrow-decorated boxes, so FlyoutPresenter marks it in code when the template is applied. It exists
// whenever the flyout is open.
// FlyoutHostPopupContainerStyle applies to the panel.
[SemanticPart(
    "popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The area of the flyout that displays the Flyout content. The presenter belongs to a template shared with
// other arrow-decorated boxes, so FlyoutPresenter marks it in code when the template is applied. It exists
// whenever the flyout is open.
// FlyoutHostPopupContentStyle applies to the presenter.
[SemanticPart(
    "popup.content",
    SelectorClass = "semantic-popup-content",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The arrow that points from the flyout to its placement target. It is visible only while the effective
// arrow visibility is on: Flyout.IsArrowVisible is set and the placement supports an arrow. FlyoutPresenter
// marks it in code when the template is applied.
// FlyoutHostPopupArrowStyle applies to the indicator.
[SemanticPart(
    "popup.arrow",
    SelectorClass = "semantic-popup-arrow",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-arrow",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowIndicator),
    Since = "6.2.0")]
public partial class FlyoutHost
{
}
