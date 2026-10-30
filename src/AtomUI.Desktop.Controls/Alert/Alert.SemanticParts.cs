using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(Icon),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(Label),
    Since = "6.2.0")]
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
public partial class Alert
{
}
