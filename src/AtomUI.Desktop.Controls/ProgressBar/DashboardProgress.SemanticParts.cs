using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    SelectorRoute = "/template/ .semantic-body > .semantic-rail",
    ContractType = typeof(Shape),
    Since = "6.2.0")]
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-body > .semantic-track",
    ContractType = typeof(Shape),
    Since = "6.2.0")]
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "/template/ .semantic-body > .semantic-indicator",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
public partial class DashboardProgress
{
}
