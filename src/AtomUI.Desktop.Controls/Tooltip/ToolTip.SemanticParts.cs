using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "container",
    SelectorClass = "semantic-container",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-container",
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "arrow",
    SelectorClass = "semantic-arrow",
    CrossNestedOwners = true,
    SelectorRoute = "/template/ .semantic-scope-arrow-decorated-box /template/ .semantic-arrow",
    ContractType = typeof(ArrowIndicator),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
public partial class ToolTip
{
}
