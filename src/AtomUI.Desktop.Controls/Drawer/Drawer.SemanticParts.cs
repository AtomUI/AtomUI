using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The parts use flat names that follow the semantic DOM of Ant Design's Drawer (v6). Its wrapper (the motion
// container) and its dragger have no part here: AtomUI has no resizable drawer yet. Every part lives in the
// DrawerContainer that is created at run time in the adorner layer (ScopeAwareAdornerLayer), outside the
// owner's subtree, so each part declares CrossVisualRoot and RuntimeCreated. The marker classes are declared
// statically in the two internal container themes.
// The mask that dims the area behind the drawer panel. Optional: present only when the drawer shows a mask.
// DrawerMaskStyle applies to the mask.
[SemanticPart("mask",
    SelectorClass = "semantic-mask",
    SelectorRoute = ">> .semantic-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The frame of the drawer panel, which paints its background, border and shadow.
// DrawerSectionStyle applies to the frame.
[SemanticPart("section",
    SelectorClass = "semantic-section",
    SelectorRoute = ">> .semantic-section",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The header row of the drawer panel, which holds the close button, the title and the extra content.
// DrawerHeaderStyle applies to the header.
[SemanticPart("header",
    SelectorClass = "semantic-header",
    SelectorRoute = ">> .semantic-header",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Avalonia.Controls.Grid),
    Since = "6.2.0")]
// The title text in the header. DrawerTitleStyle applies to the title.
[SemanticPart("title",
    SelectorClass = "semantic-title",
    SelectorRoute = ">> .semantic-title",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
// The extra content at the end of the header. Visible only while the drawer has extra content (Extra).
// DrawerExtraStyle applies to the extra content.
[SemanticPart("extra",
    SelectorClass = "semantic-extra",
    SelectorRoute = ">> .semantic-extra",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The body of the drawer panel, which presents the content. DrawerBodyStyle applies to the body.
[SemanticPart("body",
    SelectorClass = "semantic-body",
    SelectorRoute = ">> .semantic-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The footer row under the body. Visible only while the drawer has a footer.
// DrawerFooterStyle applies to the footer.
[SemanticPart("footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = ">> .semantic-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The close button at the start of the header. Visible only while IsShowCloseButton is true.
// DrawerCloseStyle applies to the close button.
[SemanticPart("close",
    SelectorClass = "semantic-close",
    SelectorRoute = ">> .semantic-close",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
public partial class Drawer
{
}
