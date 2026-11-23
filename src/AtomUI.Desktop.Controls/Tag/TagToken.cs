using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TagToken : AbstractTagToken
{
    /// <summary>
    /// 成功状态标签的主色（文本与描边，实心变体的填充）；与全局 ColorSuccessBg / ColorSuccessBorder 成组。
    /// Main colour of a success tag (text and outline, the fill of the solid variant); pairs with the global
    /// ColorSuccessBg / ColorSuccessBorder.
    /// </summary>
    public partial Color SuccessColor { get; set; }

    /// <summary>
    /// 信息（processing）状态标签的主色；与全局 ColorInfoBg / ColorInfoBorder 成组。
    /// Main colour of an info (processing) tag; pairs with the global ColorInfoBg / ColorInfoBorder.
    /// </summary>
    public partial Color InfoColor { get; set; }

    /// <summary>
    /// 警告状态标签的主色；与全局 ColorWarningBg / ColorWarningBorder 成组。
    /// Main colour of a warning tag; pairs with the global ColorWarningBg / ColorWarningBorder.
    /// </summary>
    public partial Color WarningColor { get; set; }

    /// <summary>
    /// 错误状态标签的主色；与全局 ColorErrorBg / ColorErrorBorder 成组。
    /// Main colour of an error tag; pairs with the global ColorErrorBg / ColorErrorBorder.
    /// </summary>
    public partial Color ErrorColor { get; set; }

    /// <summary>
    /// 默认背景色
    /// Default background color
    /// </summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>
    /// 默认文字颜色
    /// Default text color
    /// </summary>
    public partial Color DefaultColor { get; set; }

    /// <summary>
    /// 关闭图标尺寸
    /// Size of the close icon
    /// </summary>
    public partial double TagCloseIconSize { get; set; }

    /// <summary>
    /// 默认实心标签的文本色
    /// Default text color for solid tag
    /// </summary>
    public partial Color SolidTextColor { get; set; }

    /// <summary>
    /// 填充型状态标签是否使用实心底色
    /// Whether a Filled status tag uses the solid status fill with the contrast text
    /// </summary>
    public partial bool FilledStatusSolid { get; set; }
}
