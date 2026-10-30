using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
public partial class FloatButton
{
}
