using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// The content that is dimmed or blurred while IsSpinning is true. SpinContainerStyle applies to the content
// presenter.
[SemanticPart(
    "container",
    SelectorClass = "semantic-container",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// The mask over the content, shown only while IsSpinning is true. Its background is transparent unless
// IsMaskBackgroundEnabled is true. SpinMaskStyle applies to the mask.
[SemanticPart(
    "mask",
    SelectorClass = "semantic-mask",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The centered block that stacks the indicator above the tip text. It is shown only while IsSpinning is true.
// SpinSectionStyle applies to the block.
[SemanticPart(
    "section",
    SelectorClass = "semantic-section",
    ContractType = typeof(StackPanel),
    Since = "6.2.0")]
// The loading indicator: the built-in dots or the custom indicator. SpinIndicatorStyle applies to the indicator.
[SemanticPart(
    "indicator",
    SelectorClass = "semantic-indicator",
    ContractType = typeof(SpinIndicator),
    Since = "6.2.0")]
// The tip text under the indicator, shown only while IsTipVisible is true. SpinDescriptionStyle applies to it.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(TextBlock),
    Since = "6.2.0")]
public partial class Spin
{
}
