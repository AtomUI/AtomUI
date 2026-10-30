using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-body /template/ .semantic-content",
    CrossNestedOwners = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class GradientColorPicker
{
}
