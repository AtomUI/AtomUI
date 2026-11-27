using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// The color swatch in the trigger, which shows the current gradient.
// GradientColorPickerBodyStyle applies to the swatch.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The painted color area inside the swatch. The node belongs to the template of the swatch itself, so the
// selector has to cross the picker template, the body node and the nested template.
// GradientColorPickerContentStyle applies to that area.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-body /template/ .semantic-content",
    CrossNestedOwners = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The area beside the swatch that lists the gradient stops as text. It is collapsed, not removed, while
// IsTextVisible is false.
// GradientColorPickerDescriptionStyle applies to that panel.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(Panel),
    Since = "6.2.0")]
// The root frame of the popup. It is the direct child of the Popup, carries the outer edge of the popup
// and holds the arrow box. It belongs to the visual root of the popup, so the style has to cross visual roots.
// GradientColorPickerPopupRootStyle applies to that frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class GradientColorPicker
{
}
