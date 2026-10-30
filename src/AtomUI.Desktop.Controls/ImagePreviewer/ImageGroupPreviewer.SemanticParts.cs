using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

[SemanticPart("image",
    SelectorClass = "semantic-image",
    SelectorRoute = "/template/ .semantic-scope-items >> .semantic-image",
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart("cover",
    SelectorClass = "semantic-cover",
    SelectorRoute = "/template/ .semantic-scope-items >> .semantic-cover",
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart("popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart("popup.mask",
    SelectorClass = "semantic-popup-mask",
    SelectorRoute = ">> .semantic-popup-mask",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Optional,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart("popup.body",
    SelectorClass = "semantic-popup-body",
    SelectorRoute = ">> .semantic-popup-body",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart("popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = ">> .semantic-popup-footer",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart("popup.actions",
    SelectorClass = "semantic-popup-actions",
    SelectorRoute = ">> .semantic-popup-footer /template/ .semantic-popup-actions",
    CrossVisualRoot = true,
    CrossNestedOwners = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class ImageGroupPreviewer
{
}
