using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "source.section",
    SelectorClass = "semantic-source",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "target.section",
    SelectorClass = "semantic-target",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(StackPanel),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-body",
    ContractType = typeof(DockPanel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-list",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-footer",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "source.header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "target.header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "source.title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "target.title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "source.body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-body",
    ContractType = typeof(DockPanel),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "target.body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-body",
    ContractType = typeof(DockPanel),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "source.list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-list",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "target.list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-list",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "source.footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-footer",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "target.footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-footer",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    ContractType = typeof(TransferTreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true,
    CrossNestedOwners = true)]
[SemanticPart(
    "source.item",
    SelectorClass = "semantic-source-item",
    SelectorRoute = ">> .semantic-source-item",
    ContractType = typeof(TransferTreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true,
    CrossNestedOwners = true)]
[SemanticPart(
    "target.item",
    SelectorClass = "semantic-target-item",
    SelectorRoute = ">> .semantic-target-item",
    ContractType = typeof(TransferListItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true,
    CrossNestedOwners = true)]
public partial class TreeTransfer
{
}
