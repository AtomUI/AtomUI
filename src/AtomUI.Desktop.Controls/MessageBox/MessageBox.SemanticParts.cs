using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// MessageBox reuses the visual tree of Dialog and exposes the same eight parts with the same routes. The generator
// emits a descriptor for each public control and does not inherit the one of a base class, as SimplePagination
// shows for Pagination, so the parts of Dialog are declared here again.
// Dialog has no ControlTemplate of its own. Every part lives in the template of a runtime-created host: the
// DialogSurface, the OverlayDialogPresenter, the OverlayDialogHeader or the OverlayDialogMask. That is why every
// part is declared with CrossVisualRoot, CrossNestedOwners and RuntimeCreated.
// The routes are anchored with the `.semantic-scope-*` classes on purpose. The body embeds a Skeleton, and the
// content can contain other semantic owners that publish parts with the same `semantic-header`, `semantic-title`,
// `semantic-body`, `semantic-footer` and `semantic-container` classes. A broad descendant selector would match
// those nested parts as well.
//
// The mask behind the message box. It is shown only for a modal message box, for example one opened with
// ShowMessageBoxModalAsync. MessageBoxMaskStyle applies to the mask.
[SemanticPart("mask",
    SelectorClass = "semantic-mask",
    SelectorRoute = ">> .semantic-scope-mask /template/ .semantic-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The host that presents the message box surface inside the motion root of the presenter. It is an optional
// part, so a style for it must not assume that the node exists. MessageBoxWrapperStyle applies to the wrapper.
[SemanticPart("wrapper",
    SelectorClass = "semantic-wrapper",
    SelectorRoute = ">> .semantic-scope-presenter > .semantic-wrapper",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.Control),
    Since = "6.2.0")]
// The frame of the message box surface. It paints the content background and the corner radius, and clips the
// header, the body and the footer. MessageBoxContainerStyle applies to the frame.
[SemanticPart("container",
    SelectorClass = "semantic-container",
    SelectorRoute = ">> .semantic-scope-frame-host > .semantic-container",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The header above the body that holds the title and the close button. It is an optional part that is collapsed
// while the header is hidden. MessageBoxHeaderStyle applies to the header.
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
// MessageBoxTitleStyle applies to the title.
[SemanticPart("title",
    SelectorClass = "semantic-title",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
// The content area between the header and the footer. It hosts the message box content inside a Skeleton that is
// always present and shows the loading state. MessageBoxBodyStyle applies to the content area.
[SemanticPart("body",
    SelectorClass = "semantic-body",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The footer below the body that hosts the buttons of the message box. It is always in the tree and is only
// collapsed while the footer is not visible. MessageBoxFooterStyle applies to the footer.
[SemanticPart("footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The close button at the end of the header. It is an optional part that is collapsed while the message box is
// not closable. MessageBoxCloseStyle applies to the close button.
[SemanticPart("close",
    SelectorClass = "semantic-close",
    SelectorRoute = ">> .semantic-scope-content-layer > .semantic-scope-header /template/ .semantic-close",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Avalonia.Controls.Button),
    Since = "6.2.0")]
public partial class MessageBox
{
}
