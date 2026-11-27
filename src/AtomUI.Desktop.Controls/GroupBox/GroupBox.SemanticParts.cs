using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The header in the gap of the group's border, carried by PART_HeaderContent. The node is a
// permanent static node of the template and the geometry source of the border gap
// (GroupBox.CalculateHeaderGapBounds reads its Bounds), so its cardinality is Single rather than
// Optional. GroupBoxHeaderStyle applies to the header.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The optional icon beside the header label. Its ContractType is the public IconPresenter. When
// HeaderIcon is null the node is only hidden (IsVisible=false) and never absent from the template,
// so its cardinality is still Single. GroupBoxIconStyle applies to the icon.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
// The title text of the header. The ContractType is the Avalonia.Controls.TextBlock base class, not
// the derived node type (AtomUI.Desktop.Controls.TextBlock). The derived type satisfies the contract
// as well; taking the base class keeps the contract compatible with a later replacement of the node
// by a plain TextBlock, whereas narrowing to the derived type would be a breaking change. The name
// must stay fully qualified: this file lives in the AtomUI.Desktop.Controls namespace, where an
// unqualified TextBlock resolves to the derived type instead of the Avalonia base class.
// GroupBoxTitleStyle applies to the title.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
// The area under the header that presents the group's Content. GroupBoxContentStyle applies to the
// content presenter.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class GroupBox
{
}
