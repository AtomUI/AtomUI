using AtomUI.Theme;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The content inside the track: the off content and the on content of the switch. Each is shown only while it
// is not null. The part occurs twice, once per content.
// ToggleSwitchContentStyle applies to both presenters.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0")]
// The handle that slides between the off and the on end of the track. It also hosts the loading indicator
// while the switch is loading.
// ToggleSwitchIndicatorStyle applies to the handle.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    ContractType = typeof(TemplatedControl),
    Since = "6.2.0")]
public partial class ToggleSwitch
{
}
