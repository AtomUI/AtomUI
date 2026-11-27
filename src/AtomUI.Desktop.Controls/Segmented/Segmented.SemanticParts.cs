using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// One option of the segmented control. Created at runtime and repeated, one per item: generated for data items
// and also added to SegmentedItem containers declared in XAML.
// SegmentedItemStyle applies to every option.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "> .semantic-item",
    ContractType = typeof(SegmentedItem),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The icon at the start of an option, in the template of the option. Present only when the option has an Icon.
// It repeats with the options, which are created at runtime.
// SegmentedIconStyle applies to every icon.
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    SelectorRoute = "> .semantic-item /template/ .semantic-icon",
    ContractType = typeof(IconPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The text of an option, in the template of the option. It repeats with the options, which are created at
// runtime.
// SegmentedLabelStyle applies to every label.
[SemanticPart(
    "label",
    SelectorClass = "semantic-label",
    SelectorRoute = "> .semantic-item /template/ .semantic-label",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Segmented
{
}
