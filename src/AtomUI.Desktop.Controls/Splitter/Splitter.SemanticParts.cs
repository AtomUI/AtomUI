using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The drag bar inside a split bar handle: the strip that accepts the drag and carries the grip in its center.
// There is one per handle, and the handles are created at run time, so the route has to cross the template
// of each handle to reach the bar.
// SplitterDraggerStyle applies to every drag bar.
[SemanticPart(
    "dragger",
    SelectorClass = "semantic-dragger",
    SelectorRoute = "/template/ .semantic-scope-panel > .semantic-scope-handle /template/ .semantic-dragger",
    ContractType = typeof(Thumb),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// A content pane of the splitter. There is one for each child that is not a handle, and SplitterPanel adds
// the selector class at run time, so the route reaches a pane only as a direct child of the scope panel in
// the template.
// SplitterPanelStyle applies to every pane.
[SemanticPart(
    "panel",
    SelectorClass = "semantic-panel",
    SelectorRoute = "/template/ .semantic-scope-panel > .semantic-panel",
    ContractType = typeof(Control),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Splitter
{
}
