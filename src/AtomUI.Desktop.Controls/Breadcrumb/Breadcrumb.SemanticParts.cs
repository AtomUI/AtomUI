using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// One entry of the path. The breadcrumb creates it at runtime as the container of an item, so there is one
// per item. BreadcrumbItemStyle applies to every item.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(BreadcrumbItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The separator between two adjacent items. The breadcrumb creates it at runtime as its own logical child,
// one fewer than there are items. BreadcrumbSeparatorStyle applies to every separator.
[SemanticPart(
    "separator",
    SelectorClass = "semantic-separator",
    SelectorRoute = "> .semantic-separator",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Breadcrumb
{
}
