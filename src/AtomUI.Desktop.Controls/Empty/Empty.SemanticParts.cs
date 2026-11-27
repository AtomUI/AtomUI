using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The built-in illustration above the description. Shown only while Image is null; a custom image is
// presented by a separate node that has no part. EmptyImageStyle applies to the illustration.
[SemanticPart(
    "image",
    SelectorClass = "semantic-image",
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The description node under the illustration, which presents the description text or custom description
// content. Hidden while IsDescriptionVisible is false. EmptyDescriptionStyle applies to it.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The footer under the description. Visible only while Footer is set. EmptyFooterStyle applies to it.
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class Empty
{
}
