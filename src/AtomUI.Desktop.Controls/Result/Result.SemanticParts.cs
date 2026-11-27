using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The status image above the title. The template holds two nodes of this part, the status icon and the
// error-code illustration; Status decides which of them is visible. ResultIconStyle applies to both.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The title of the result, under the status image. Present only while Header is set.
// ResultTitleStyle applies to it.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The subtitle under the title. Present only while SubHeader is set. ResultSubTitleStyle applies to it.
[SemanticPart(
    "subTitle",
    SelectorClass = "semantic-sub-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The area for the extra content between the subtitle and the main content. It carries no content while Extra is
// not set. ResultExtraStyle applies to it.
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The main content area below the subtitle and the extra area, with a filled background. Present only while
// Content is set. ResultBodyStyle applies to it.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class Result
{
}
