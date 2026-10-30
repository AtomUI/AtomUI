using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart(
    "avatar",
    SelectorClass = "semantic-avatar",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class CardMetaContent
{
}
