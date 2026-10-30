using AtomUI.Controls;
using AtomUI.Theme;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

// header 的承载节点是 PART_HeaderContent。它是模板中的常驻静态节点，且同时是边框缺口的几何来源
// （GroupBox.CalculateHeaderGapBounds 读取它的 Bounds），因此 cardinality 是 Single 而不是 Optional。
[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(Border),
    Since = "6.2.0")]
// icon 的 ContractType 取公开的 IconPresenter；HeaderIcon 为 null 时节点 IsVisible=false，
// 但不从模板缺席，因此仍是 Single。
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconPresenter),
    Since = "6.2.0")]
// title 的 ContractType 取 Avalonia.Controls.TextBlock 基类而不是节点派生类型
// （AtomUI.Desktop.Controls.TextBlock）。派生类型同样满足契约，取基类让后续把节点替换为
// 普通 TextBlock 保持兼容；收窄到派生类型会成为破坏性变更。
// 必须完全限定：本文件位于 AtomUI.Desktop.Controls 命名空间内，非限定的 TextBlock
// 会解析到同命名空间的派生类型，而不是 Avalonia 基类。
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(Avalonia.Controls.TextBlock),
    Since = "6.2.0")]
[SemanticPart(
    "content",
    SelectorClass = "semantic-content",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
public partial class GroupBox
{
}
