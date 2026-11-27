using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The image on the cover of the previewer. It is visible only while an image source is set. The
// route enters the cover scope and crosses into the cover's template, where the node is declared.
// ImagePreviewerImageStyle applies to the image.
[SemanticPart("image",
    SelectorClass = "semantic-image",
    SelectorRoute = "/template/ .semantic-scope-cover /template/ .semantic-image",
    CrossNestedOwners = true,
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The mask over the cover image, which presents the preview action. It is transparent while the
// previewer is at rest and shown on hover, so the semantic preview outlines it whatever its opacity.
// ImagePreviewerCoverStyle applies to the mask.
[SemanticPart("cover",
    SelectorClass = "semantic-cover",
    SelectorRoute = "/template/ .semantic-scope-cover /template/ .semantic-cover",
    CrossNestedOwners = true,
    RestHidden = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The root panel of the preview popup. It is created at run time in another visual root than the
// control, by the overlay host or the native dialog.
// ImagePreviewerPopupRootStyle applies to the root panel.
[SemanticPart("popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The panel that dims the content behind the preview popup. It is optional: only the overlay host
// template provides it, the native dialog has none.
// ImagePreviewerPopupMaskStyle applies to the mask.
[SemanticPart("popup.mask",
    SelectorClass = "semantic-popup-mask",
    SelectorRoute = ">> .semantic-popup-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The body panel of the preview popup that shows the current image.
// ImagePreviewerPopupBodyStyle applies to the body.
[SemanticPart("popup.body",
    SelectorClass = "semantic-popup-body",
    SelectorRoute = ">> .semantic-popup-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The floating toolbar of the preview popup.
// ImagePreviewerPopupFooterStyle applies to the toolbar.
[SemanticPart("popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = ">> .semantic-popup-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The bar inside the floating toolbar that holds the preview operation buttons. The route enters
// the toolbar's template from the footer part.
// ImagePreviewerPopupActionsStyle applies to the bar.
[SemanticPart("popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = ">> .semantic-popup-footer /template/ .semantic-popup-actions",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class ImagePreviewer
{
}
