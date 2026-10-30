using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "close",
    SelectorClass = "semantic-close",
    ContractType = typeof(IconButton),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
public partial class TabItem
{
}
