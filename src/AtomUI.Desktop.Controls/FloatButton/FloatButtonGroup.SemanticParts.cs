using AtomUI.Theme;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The button that opens the list of items when Trigger is Click or Hover. It is absent when Trigger is Default.
// FloatButtonGroupTriggerStyle applies to the trigger button.
[SemanticPart(
    "trigger",
    SelectorClass = "semantic-trigger",
    ContractType = typeof(FloatButton),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
// The list that lays out the items of the group. With Trigger set to Default it is the whole group; otherwise it
// is the menu that opens next to the trigger and is shown while IsOpen is true.
// FloatButtonGroupListStyle applies to the list.
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
public partial class FloatButtonGroup
{
}
