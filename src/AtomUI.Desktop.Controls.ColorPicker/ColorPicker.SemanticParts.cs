using AtomUI.Theme;
using Avalonia.Controls;

namespace AtomUI.Desktop.Controls;

// 注意：本命名空间内的 `TextBlock` 简单名会被 AtomUI.Desktop.Controls.TextBlock 遮蔽，
// 而 description 部件的模板节点（PART_ColorText）是 Avalonia 原生 TextBlock，
// 合同必须显式指向 Avalonia.Controls.TextBlock（别名与 ColorPicker.cs 主文件惯例一致）。
using AvaloniaTextBlock = Avalonia.Controls.TextBlock;

[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(Control),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    SelectorRoute = "/template/ .semantic-body /template/ .semantic-content",
    CrossNestedOwners = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
[SemanticPart(
    "description",
    SelectorClass = "semantic-description",
    ContractType = typeof(AvaloniaTextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "popup.root",
    SelectorClass = "semantic-popup-root",
    CrossVisualRoot = true,
    ContractType = typeof(Border),
    Since = "6.2.0")]
public partial class ColorPicker
{
}
