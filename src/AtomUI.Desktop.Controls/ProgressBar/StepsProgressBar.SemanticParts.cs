using AtomUI.Controls.Commons;
using AtomUI.Generated.AtomUIDesktopControls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

namespace AtomUI.Desktop.Controls;

// The root panel that draws the steps and arranges the progress info. StepsProgressBarBodyStyle applies to it.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// One rectangle per step. The rectangles are created at runtime by the step panel as its direct children.
// StepsProgressBarTrackStyle applies to every step.
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-body > .semantic-track",
    ContractType = typeof(Rectangle),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The progress info: the percentage label or a status icon. Present only while the progress info is shown.
// StepsProgressBarIndicatorStyle applies to it.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = "/template/ .semantic-body > .semantic-indicator",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
public partial class StepsProgressBar
{
    /// <summary>
    /// Calls the base method, then hands the selector class of the track semantic part to the
    /// <c>StepsProgressPanel</c> named <c>PART_ProgressBody</c> in the template. The panel adds the class to every
    /// step rectangle it creates, so that StepsProgressBarTrackStyle reaches the steps.
    /// </summary>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        e.NameScope.Find<StepsProgressPanel>(AbstractProgressBar.ProgressBodyPart)
         ?.SetTrackSemanticClass(StepsProgressBarSemanticParts.TrackClass);
    }
}
