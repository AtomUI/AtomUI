using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

using AvaloniaListBoxItem = Avalonia.Controls.ListBoxItem;

namespace AtomUI.Desktop.Controls;

[SemanticPart("prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart("input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
[SemanticPart("secondaryInput",
    SelectorClass = "semantic-secondary-input",
    SelectorRoute = "/template/ .semantic-secondary-input",
    ContractType = typeof(TextBox),
    Since = "6.2.0")]
[SemanticPart("suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart("clear",
    SelectorClass = "semantic-clear",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-clear",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
[SemanticPart("popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(ArrowDecoratedBox),
    Since = "6.2.0")]
[SemanticPart("popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(DockPanel),
    Since = "6.2.0")]
[SemanticPart("popup.content",
    SelectorClass = "semantic-time-content",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Grid),
    Since = "6.2.0")]
[SemanticPart("popup.column",
    SelectorClass = "semantic-time-column",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-column",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart("popup.item",
    SelectorClass = "semantic-time-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-time-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(AvaloniaListBoxItem),
    Since = "6.2.0")]
[SemanticPart("popup.footer",
    SelectorClass = "semantic-popup-footer",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-footer",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
public partial class RangeTimePicker
{
}
