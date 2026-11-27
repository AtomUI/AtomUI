using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The part set follows the nine slots of the semantic DOM of the upstream antd Modal: the root is implicit, and
// the wrapper part maps to the motion container of the dialog surface.
// Dialog has no ControlTemplate. Every part lives in the template of a runtime-created DialogSurface,
// OverlayDialogPresenter, OverlayDialogHeader or OverlayDialogMask, so all parts are declared with
// CrossVisualRoot, CrossNestedOwners and RuntimeCreated, and the scope markers are declared statically on the
// four host themes.
//
// Routes must be narrowed with the `.semantic-scope-*` anchors and must not rely on a bare `>>`. The dialog body
// embeds a Skeleton that is always present, and user content often contains semantic owners such as Card, Tooltip
// and Spin. They publish parts with the same `semantic-header`, `semantic-title`, `semantic-body`,
// `semantic-footer` and `semantic-container` classes, and a broad descendant selector would match these nested
// parts as well. For the same reason a public selector must not use a broad descendant combinator.
//
// The mask behind the dialog surface. It is shown only for a modal dialog.
// DialogMaskStyle applies to the mask.
[SemanticPart("mask",
    SelectorClass = "semantic-mask",
    SelectorRoute = ">> .semantic-scope-mask /template/ .semantic-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The host that presents the dialog surface inside the motion root of the presenter. It is an optional part,
// so a style for it must not assume that the node exists. DialogWrapperStyle applies to the wrapper.
[SemanticPart("wrapper",
    SelectorClass = "semantic-wrapper",
    SelectorRoute = ">> .semantic-scope-presenter > .semantic-wrapper",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.Control),
    Since = "6.2.0")]
// The frame of the dialog surface that paints the content background and the corner radius, and clips the
// header, the body and the footer. DialogContainerStyle applies to the frame.
[SemanticPart("container",
    SelectorClass = "semantic-container",
    SelectorRoute = ">> .semantic-scope-frame-host > .semantic-container",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The header above the body that holds the title icon, the title and the caption buttons. It is an optional
// part: it is collapsed when the header is hidden, and a window host always hides it because the native
// window owns the title bar. DialogHeaderStyle applies to the header.
[SemanticPart("header",
    SelectorClass = "semantic-header",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The title text in the header. It is an optional part that exists only together with the header.
// DialogTitleStyle applies to the title.
[SemanticPart("title",
    SelectorClass = "semantic-title",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
// The content area between the header and the footer. It hosts the dialog content inside a Skeleton that is
// always present and shows the loading state. DialogBodyStyle applies to the content area.
[SemanticPart("body",
    SelectorClass = "semantic-body",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The footer below the body that hosts the dialog buttons. It is always in the tree and is only collapsed
// while the footer is not visible, for example when DialogOptions.IsFooterVisible is false.
// DialogFooterStyle applies to the footer.
[SemanticPart("footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The close button at the end of the header. It is an optional part: it is collapsed when the dialog is not
// closable, and a window host hides the whole header. DialogCloseStyle applies to the close button.
[SemanticPart("close",
    SelectorClass = "semantic-close",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-close",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.Button),
    Since = "6.2.0")]
public partial class Dialog
{
}
