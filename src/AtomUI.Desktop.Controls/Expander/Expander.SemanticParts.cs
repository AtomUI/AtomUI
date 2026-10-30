using AtomUI.Controls.Primitives;
using AtomUI.Theme;
using Avalonia.Controls.Presenters;

namespace AtomUI.Desktop.Controls;

[SemanticPart(
    "header",
    SelectorClass = "semantic-header",
    ContractType = typeof(PixelAlignedBorder),
    Since = "6.2.0")]
[SemanticPart(
    "icon",
    SelectorClass = "semantic-icon",
    ContractType = typeof(IconButton),
    Since = "6.2.0")]
[SemanticPart(
    "title",
    SelectorClass = "semantic-title",
    ContractType = typeof(ContentPresenter),
    Since = "6.2.0")]
// body 的承载节点是 PART_ContentViewport（ContentViewport，ContentControl）的内容：
// 折叠稳定态下 viewport 的 IsVisible=False，其内部 ContentPresenter 不挂接视觉子级，节点从视觉树中
// 完全缺席（仅保留逻辑子级），展开后才物化。因此 body 声明 Optional（0 或 1 个静态 marker），
// 而不是 Single。header / icon / title 是模板中的常驻节点，折叠与展开都存在于视觉树。
[SemanticPart(
    "body",
    SelectorClass = "semantic-body",
    ContractType = typeof(ContentPresenter),
    Cardinality = SemanticPartCardinality.Optional,
    Since = "6.2.0")]
public partial class Expander
{
}
