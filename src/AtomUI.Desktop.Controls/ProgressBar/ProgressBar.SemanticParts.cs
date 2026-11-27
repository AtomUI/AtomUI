using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The root panel that arranges the bar and the progress info. ProgressBarBodyStyle applies to it.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The groove behind the filled track, the unfilled part of the bar. ProgressBarRailStyle applies to it.
[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    SelectorRoute = "/template/ .semantic-body > .semantic-rail",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The filled part of the bar that shows the completed value. ProgressBarTrackStyle applies to it.
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-body > .semantic-track",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The progress info: the percentage label or a status icon, beside the bar or inside it. Present only while the
// progress info is shown. ProgressBarIndicatorStyle applies to it.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "/template/ .semantic-body > .semantic-indicator",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
public partial class ProgressBar
{
}
