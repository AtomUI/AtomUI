using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    SelectorRoute = ">> .semantic-popup-root",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(FlyoutPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.container",
    SelectorClass = "semantic-popup-container",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-container",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "popup.content",
    SelectorClass = "semantic-popup-content",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-content",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
[SemanticPart(
    "popup.arrow",
    SelectorClass = "semantic-popup-arrow",
    SelectorRoute = ">> .semantic-popup-root >> .semantic-popup-arrow",
    CrossVisualRoot = true,
    RuntimeCreated = true,
    ContractType = typeof(ArrowIndicator),
    Since = "6.2.0")]
public partial class FlyoutHost
{
}
