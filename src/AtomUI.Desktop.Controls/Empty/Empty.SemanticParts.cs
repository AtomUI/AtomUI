using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "image",
    SelectorClass = "semantic-image",
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class Empty
{
}
