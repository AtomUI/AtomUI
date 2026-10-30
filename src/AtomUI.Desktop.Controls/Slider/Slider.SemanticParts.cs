using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "rail",
    SelectorClass = "semantic-rail",
    SelectorRoute = "/template/ .semantic-rail",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "tracks",
    SelectorClass = "semantic-tracks",
    SelectorRoute = "/template/ .semantic-tracks",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0",
    RuntimeCreated = true)]
[SemanticPart(
    "track",
    SelectorClass = "semantic-track",
    SelectorRoute = "/template/ .semantic-track",
    ContractType = typeof(Border),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
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
