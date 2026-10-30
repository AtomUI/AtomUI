using AtomUI.Theme;
using AtomUI.Controls.Primitives;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(DashedBorder),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "cover",
    SelectorClass = "semantic-cover",
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
public partial class Card
{
}
