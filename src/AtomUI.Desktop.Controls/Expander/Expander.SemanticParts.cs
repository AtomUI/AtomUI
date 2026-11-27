using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The header area with the expand icon, the title and the add-on content; in Header trigger mode it is the
// element that toggles the expander. ExpanderHeaderStyle applies to it.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
// The expand icon button in the header. It stays in the visual tree but is hidden while IsShowExpandIcon is
// false. ExpanderIconStyle applies to the button.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
// The title area of the header, which presents Header and HeaderTemplate. ExpanderTitleStyle applies to it.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The body presents the Content of the expander while it is expanded. ExpanderBodyStyle applies to it.
// The node is the content of PART_ContentViewport (a ContentViewport, which is a ContentControl). In the
// settled collapsed state the viewport has IsVisible=False, its inner ContentPresenter attaches no visual
// child, and the node is absent from the visual tree (it remains a logical child only); it materializes
// once the expander is expanded. The body is therefore declared Optional (0 or 1 static marker) instead of
// Single. header / icon / title are permanent nodes of the template: they are in the visual tree whether
// the expander is collapsed or expanded.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
public partial class Expander
{
}
