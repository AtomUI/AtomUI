using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The groove: the full-length track behind the filled segments. Created in code by SliderTrack, one per slider.
// SliderRailStyle applies to the groove.
[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    SelectorRoute = "/template/ .semantic-rail",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// The container of the filled segments. Created in code by SliderTrack, one per slider; visible only while the
// track is included.
// SliderTracksStyle applies to the container.
[SemanticPart(
    "tracks",
    SelectorClass = "semantic-tracks",
    SelectorRoute = "/template/ .semantic-tracks",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
// A filled track segment. Created in code by SliderTrack and repeated: one segment for a single value, and one
// per gap between neighbouring values in range mode.
// SliderTrackStyle applies to every segment.
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-track",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
// A draggable thumb. Created in code by SliderTrack and repeated: one for a single value, and one per value in
// range mode.
// SliderHandleStyle applies to every thumb.
[SemanticPart(
    "handle",
    SelectorClass = "semantic-handle",
    SelectorRoute = "/template/ .semantic-handle",
    ContractType = typeof(SliderThumb),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Slider
{
}
