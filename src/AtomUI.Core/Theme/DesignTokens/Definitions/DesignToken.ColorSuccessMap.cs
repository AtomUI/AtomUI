using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// 成功色的浅色背景颜色
    /// 成功色的浅色背景颜色，用于 Tag 和 Alert 的成功态背景色
    /// 1 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBg { get; set; }

    /// <summary>
    /// 成功色的浅色背景色悬浮态
    /// 成功色浅色背景颜色，一般用于视觉层级较弱的选中状态，不过 antd 目前没有使用到该 token
    /// 2 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBgHover { get; set; }

    /// <summary>
    /// 成功色的描边色
    /// 成功色的描边色，用于 Tag 和 Alert 的成功态描边色
    /// 3 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBorder { get; set; }

    /// <summary>
    /// 成功色的描边色悬浮态
    /// 4 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBorderHover { get; set; }

    /// <summary>
    /// 成功色的深色悬浮态
    /// 5 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessHover { get; set; }

    /// <summary>
    /// 成功色
    /// 默认的成功色，如 Result、Progress 等组件中都有使用该颜色
    /// 6 号色
    /// 在 Seed 文件中定义
    /// </summary>

    /// <summary>
    /// 成功色的深色激活态
    /// 7 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessActive { get; set; }

    /// <summary>
    /// 成功色的文本悬浮态
    /// 8 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessTextHover { get; set; }

    /// <summary>
    /// 成功色的文本默认态
    /// 9 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessText { get; set; }

    /// <summary>
    /// 成功色的文本激活态
    /// 10 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessTextActive { get; set; }
}