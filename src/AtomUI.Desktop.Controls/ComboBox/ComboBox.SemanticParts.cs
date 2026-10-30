using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// 契约真源见 docs/controls/desktop/navigation/combo-box/semantic-part.md。
// 注意：本命名空间下存在 AtomUI 自有的 TextBox / TextBlock / ScrollViewer，
// 因此这三个 ContractType 必须显式写 Avalonia 基类名，避免解析到 AtomUI 派生类型。
[SemanticPart(
    "prefix",
    SelectorClass = "semantic-prefix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-prefix > .semantic-prefix",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "frame",
    SelectorClass = "semantic-frame",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-frame",
    CrossNestedOwners = true,
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-content",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
[SemanticPart(
    "placeholder",
    SelectorClass = "semantic-placeholder",
    SelectorRoute = "/template/ .semantic-content > .semantic-placeholder",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "input",
    SelectorClass = "semantic-input",
    SelectorRoute = "/template/ .semantic-content > .semantic-input",
    ContractType = typeof(Avalonia.Controls.TextBox),
    Since = "6.2.0")]
[SemanticPart(
    "suffix",
    SelectorClass = "semantic-suffix",
    SelectorRoute = "/template/ .semantic-scope-input /template/ .semantic-scope-suffix > .semantic-suffix",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    SelectorRoute = ">> .semantic-scope-handle /template/ .semantic-indicator",
    CrossNestedOwners = true,
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = "/template/ .semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "popup.list",
    SelectorClass = "semantic-popup-list",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list",
    CrossVisualRoot = true,
    ContractType = typeof(Avalonia.Controls.ScrollViewer),
    Since = "6.2.0")]
[SemanticPart(
    "popup.empty",
    SelectorClass = "semantic-popup-empty",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-empty",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "popup.listItem",
    SelectorClass = "semantic-popup-list-item",
    SelectorRoute = "/template/ .semantic-popup-root >> .semantic-popup-list-item",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    Cardinality = SemanticPartCardinality.Multiple,
    ContractType = typeof(ComboBoxItem),
    Since = "6.2.0")]
public partial class ComboBox
{
}
