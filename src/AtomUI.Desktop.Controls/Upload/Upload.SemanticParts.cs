using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace AtomUI.Desktop.Controls;

// The file list under the trigger: a single element created by the template. Its type follows ListType (the
// text and picture list, or the picture-card and picture-circle list), and it is hidden when IsShowUploadList
// is false.
// UploadListStyle applies to the list.
[SemanticPart(
    "list",
    SelectorClass = "semantic-list",
    SelectorRoute = "/template/ .semantic-list",
    ContractType = typeof(ItemsControl),
    Cardinality = SemanticPartCardinality.Single,
    Since = "6.2.0")]
// One entry of the file list, created at runtime for each file; the part repeats and is not in the template.
// UploadItemStyle applies to every entry.
[SemanticPart(
    "item",
    SelectorClass = "semantic-item",
    SelectorRoute = "/template/ .semantic-list > .semantic-item",
    ContractType = typeof(TemplatedControl),
    Cardinality = SemanticPartCardinality.Multiple,
    Since = "6.2.0",
    RuntimeCreated = true)]
public partial class Upload
{
}
