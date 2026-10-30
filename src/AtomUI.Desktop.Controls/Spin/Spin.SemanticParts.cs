using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "container",
    SelectorClass = "semantic-container",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "mask",
    SelectorClass = "semantic-mask",
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    ContractType = typeof(SpinIndicator),
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class Spin
{
}
