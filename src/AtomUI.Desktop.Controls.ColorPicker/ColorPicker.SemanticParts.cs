using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// In this namespace the simple name `TextBlock` is shadowed by AtomUI.Desktop.Controls.TextBlock, but the
// template node of the description part (PART_ColorText) is Avalonia's own TextBlock. The contract must
// therefore name Avalonia.Controls.TextBlock explicitly; the alias follows the convention of ColorPicker.cs.
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

// The color swatch in the trigger, which shows the current color.
// ColorPickerBodyStyle applies to the swatch.
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Control),
    Since = "6.2.0")]
// The painted color area inside the swatch. The node belongs to the template of the swatch itself, so the
// selector has to cross the picker template, the body node and the nested template.
// ColorPickerContentStyle applies to that area.
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-body /template/ .semantic-content",
    CrossNestedOwners = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
// The text beside the swatch that shows the selected color as text. It is collapsed, not removed, while
// IsTextVisible is false.
// ColorPickerDescriptionStyle applies to the Avalonia TextBlock.
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(AvaloniaTextBlock),
    Since = "6.2.0")]
// The root frame of the popup. It is the direct child of the Popup, carries the outer edge of the popup
// and holds the arrow box. It belongs to the visual root of the popup, so the style has to cross visual roots.
// ColorPickerPopupRootStyle applies to that frame.
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class ColorPicker
{
}
