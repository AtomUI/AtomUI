using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The image of one thumbnail in the group. It is visible only while the thumbnail has an image
// source. The covers are created per item outside the control template, so the route starts at the
// items scope and crosses into the cover's template.
// ImageGroupPreviewerImageStyle applies to every thumbnail image.
[SemanticPart("image",
    SelectorClass = "semantic-image",
    SelectorRoute = "/template/ .semantic-scope-items >> .semantic-image",
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The mask over one thumbnail, which presents the preview action. It is visible only while the
// cover mask is enabled, and one instance exists per item.
// ImageGroupPreviewerCoverStyle applies to every thumbnail mask.
[SemanticPart("cover",
    SelectorClass = "semantic-cover",
    SelectorRoute = "/template/ .semantic-scope-items >> .semantic-cover",
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The root panel of the preview popup. It is created at run time in another visual root than the
// control, by the overlay host or the native dialog.
// ImageGroupPreviewerPopupRootStyle applies to the root panel.
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
// ImageGroupPreviewerPopupMaskStyle applies to the mask.
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
// ImageGroupPreviewerPopupBodyStyle applies to the body.
[SemanticPart("popup.body",
    SelectorClass = "semantic-popup-body",
    SelectorRoute = ">> .semantic-popup-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The floating toolbar of the preview popup.
// ImageGroupPreviewerPopupFooterStyle applies to the toolbar.
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
// ImageGroupPreviewerPopupActionsStyle applies to the bar.
[SemanticPart("popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = ">> .semantic-popup-footer /template/ .semantic-popup-actions",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class ImageGroupPreviewer
{
}
