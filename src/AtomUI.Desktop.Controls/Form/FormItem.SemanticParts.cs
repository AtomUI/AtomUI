using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

namespace AtomUI.Desktop.Controls;

// The label text at the start of the item. It is hidden while LabelText is null.
// FormItemLabelStyle applies to the label text block.
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    ContractType = typeof(AvaloniaTextBlock),
    Since = "6.2.0")]
// The area beside or below the label that shows the Content of the item, usually the input control.
// FormItemContentStyle applies to the content presenter.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The extra content under the input, given by Extra and ExtraTemplate. It is shown only while Extra is not null.
// FormItemExtraStyle applies to the content presenter.
[SemanticPart(
    "extra",
    SelectorClass = "semantic-extra",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The panel under the input that stacks the validation messages and the help text. It is collapsed to zero height
// while the item has neither. FormItemHelpStyle applies to the panel.
[SemanticPart(
    "help",
    SelectorClass = "semantic-help",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// One text block per line in the help panel: each error message, then each warning message, then the help text.
// The message blocks are created by the control at runtime when the messages change. The help text block comes
// from the template and is hidden while Help is null. FormItemHelpItemStyle applies to every text block.
[SemanticPart(
    "helpItem",
    SelectorClass = "semantic-help-item",
    SelectorRoute = "/template/ .semantic-help > .semantic-help-item",
    ContractType = typeof(AvaloniaTextBlock),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class FormItem
{
}
