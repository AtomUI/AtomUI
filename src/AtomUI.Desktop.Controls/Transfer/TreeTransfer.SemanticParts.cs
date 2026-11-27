using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The left panel of the transfer. It frames the source tree view with a title bar, an optional search box and
// an optional footer.
// TreeTransferSourceSectionStyle applies to the panel.
[SemanticPart(
    "source.section",
    SelectorClass = "semantic-source",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The right panel of the transfer. It frames the target view with a title bar, an optional search box and an
// optional footer.
// TreeTransferTargetSectionStyle applies to the panel.
[SemanticPart(
    "target.section",
    SelectorClass = "semantic-target",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The vertical stack of move buttons between the two panels.
// TreeTransferActionsStyle applies to the stack.
[SemanticPart(
    "actions",
    SelectorClass = "semantic-actions",
    ContractType = typeof(StackPanel),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// The title bar at the top of a panel. It holds the select-all check box, the selection menu, the item count
// and the title. One part per panel (source and target), each created when the nested template of the hosting
// TransferItemDecorator is applied; the route crosses that host for this.
// TreeTransferHeaderStyle applies to the title bar of both panels.
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title at the right end of a panel title bar. One part per panel (source and target), each created when
// the nested template of the hosting TransferItemDecorator is applied; the route crosses that host for this.
// TreeTransferTitleStyle applies to the title of both panels.
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The area of a panel between its title bar and footer. It holds the search box and the item view.
// One part per panel (source and target), each created when the nested template of the hosting
// TransferItemDecorator is applied.
// TreeTransferBodyStyle applies to the body of both panels.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-body",
    ContractType = typeof(DockPanel),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The area of a panel that presents the item view. One part per panel (source and target), each created when
// the nested template of the hosting TransferItemDecorator is applied; the route crosses that host for this.
// TreeTransferListStyle applies to it in both panels.
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-list",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The strip at the bottom of a panel that presents the footer set for that panel.
// Present only while such a footer is set. One part per panel (source and target), each created when the
// nested template of the hosting TransferItemDecorator is applied.
// TreeTransferFooterStyle applies to the footer of both panels.
[SemanticPart(
    "footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-scope-section /template/ .semantic-footer",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title bar of the source panel only.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferSourceHeaderStyle applies to it.
[SemanticPart(
    "source.header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title bar of the target panel only.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferTargetHeaderStyle applies to it.
[SemanticPart(
    "target.header",
    SelectorClass = "semantic-header",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title at the right end of the source panel title bar.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferSourceTitleStyle applies to it.
[SemanticPart(
    "source.title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The title at the right end of the target panel title bar.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferTargetTitleStyle applies to it.
[SemanticPart(
    "target.title",
    SelectorClass = "semantic-title",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-title",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The area of the source panel between its title bar and footer, with the search box and the source tree
// view. Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferSourceBodyStyle applies to it.
[SemanticPart(
    "source.body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-body",
    ContractType = typeof(DockPanel),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The area of the target panel between its title bar and footer, with the search box and the target view.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferTargetBodyStyle applies to it.
[SemanticPart(
    "target.body",
    SelectorClass = "semantic-body",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-body",
    ContractType = typeof(DockPanel),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The area of the source panel that presents the source tree view.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferSourceListStyle applies to it.
[SemanticPart(
    "source.list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-list",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The area of the target panel that presents the target view.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferTargetListStyle applies to it.
[SemanticPart(
    "target.list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-list",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The strip at the bottom of the source panel. Present only while a source footer is set.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferSourceFooterStyle applies to it.
[SemanticPart(
    "source.footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-source /template/ .semantic-footer",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The strip at the bottom of the target panel. Present only while a target footer is set.
// Created when the nested template of the hosting TransferItemDecorator is applied.
// TreeTransferTargetFooterStyle applies to it.
[SemanticPart(
    "target.footer",
    SelectorClass = "semantic-footer",
    SelectorRoute = "/template/ .semantic-target /template/ .semantic-footer",
    ContractType = typeof(PixelAlignedBorder),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// A tree node of the transfer. One part per node; the nodes are created at runtime by the nested view and
// belong to it, not to the transfer template.
// TreeTransferItemStyle applies to every node.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = ">> .semantic-item",
    ContractType = typeof(TransferTreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true,
    CrossNestedOwners = true)]
// A tree node of the source panel only. One part per node; the nodes are created at runtime by the nested
// source view and belong to it, not to the transfer template.
// TreeTransferSourceItemStyle applies to those nodes.
[SemanticPart(
    "source.item",
    SelectorClass = "semantic-source-item",
    SelectorRoute = ">> .semantic-source-item",
    ContractType = typeof(TransferTreeViewItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true,
    CrossNestedOwners = true)]
// A row of the target panel only. One part per row; the rows are created at runtime by the nested target
// view and belong to it, not to the transfer template.
// TreeTransferTargetItemStyle applies to those rows.
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
