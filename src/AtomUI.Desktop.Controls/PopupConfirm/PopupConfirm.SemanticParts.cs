using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// PopupConfirm derives from FlyoutHost. The popup is created at runtime by PopupConfirmFlyout in another visual
// root, and the internal PopupConfirmContainer is not a public semantic owner, so PopupConfirm owns the complete
// contract. The parts follow the semantic DOM of antd Popconfirm (root / container / icon / title / content /
// arrow). The upstream root maps to popup.root. The upstream description slot maps to popup.description, because
// popup.content is fixed to the content presentation surface of the popup box across the Popover family and
// cannot also be the description node.
//
// The markers of the four frame parts popup.root, popup.container, popup.content and popup.arrow are injected by
// the code path shared by the FlyoutHost family: Flyout.CreatePresenter injects popup.root, and
// FlyoutPresenter.OnApplyTemplate injects popup.container, popup.content and popup.arrow. The existing marker
// classes are reused here instead of being injected again.
//
// The markers of popup.icon, popup.title, popup.description and popup.actions are declared statically in
// PopupConfirmContainerTheme.axaml, the theme of PopupConfirmContainer. That theme serves PopupConfirm only, so
// the static markers do not affect other controls.
//
// Every part sets RuntimeCreated to true: its host node is created by CreatePresenter at runtime and is not part
// of the template of PopupConfirm. Each route therefore descends from the owner's logical ancestors through
// popup.root.
// The root of the confirmation popup while it is open, in a separate visual root.
// PopupConfirmPopupRootStyle applies to the root.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(FlyoutPresenter),
    Since = "6.2.0")]
// The frame box of the confirmation popup, under the popup root. Runtime-created in another visual root.
// PopupConfirmPopupContainerStyle applies to the box.
[SemanticPart(
    "popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The content presentation surface of the popup box. It is not the description node, which is popup.description.
// Runtime-created in another visual root.
// PopupConfirmPopupContentStyle applies to the content surface.
[SemanticPart(
    "popup.content",
    SelectorClass = "semantic-popup-content",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The arrow that points from the popup to its placement target. Runtime-created in another visual root.
// PopupConfirmPopupArrowStyle applies to the arrow.
[SemanticPart(
    "popup.arrow",
    SelectorClass = "semantic-popup-arrow",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-arrow",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowIndicator),
    Since = "6.2.0")]
// The confirmation icon at the leading side of the popup. Present only while the Icon property is not null.
// Runtime-created in another visual root.
// PopupConfirmPopupIconStyle applies to the icon.
[SemanticPart(
    "popup.icon",
    SelectorClass = "semantic-popup-icon",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-icon",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
// The title text above the description. Runtime-created in another visual root.
// PopupConfirmPopupTitleStyle applies to the title.
[SemanticPart(
    "popup.title",
    SelectorClass = "semantic-popup-title",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-title",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
// The description under the title. It presents ConfirmContent, so it is present only while that is not null.
// It takes the upstream description slot, because popup.content is the content surface of the popup box.
// Runtime-created in another visual root.
// PopupConfirmPopupDescriptionStyle applies to the description.
[SemanticPart(
    "popup.description",
    SelectorClass = "semantic-popup-description",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-description",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The row of action buttons at the bottom of the popup, aligned to the right. It holds the cancel button, which
// is visible only while IsShowCancelButton is set, and the OK button.
// Runtime-created in another visual root.
// PopupConfirmPopupActionsStyle applies to the row.
[SemanticPart(
    "popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-actions",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
public partial class PopupConfirm
{
}
