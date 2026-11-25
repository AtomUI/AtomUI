using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// 品牌主色
    /// 品牌色是体现产品特性和传播理念最直观的视觉元素之一，用于产品的主色调、主按钮、主图标、主文本等
    /// 6 号色
    /// 在 Seed 文件中定义
    /// </summary>

    /// <summary>
    /// 主色浅色背景色
    /// 主色浅色背景颜色，一般用于视觉层级较弱的选中状态。
    /// 1 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBg { get; set; }

    /// <summary>
    /// 主色浅色背景悬浮态
    /// 与主色浅色背景颜色相对应的悬浮态颜色。
    /// 2 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBgHover { get; set; }

    /// <summary>
    /// 主色描边色
    /// 主色梯度下的描边用色，用在 Slider 等组件的描边上。
    /// 3 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBorder { get; set; }

    /// <summary>
    /// 主色描边色悬浮态
    /// 主色梯度下的描边用色的悬浮态，Slider 、Button 等组件的描边 Hover 时会使用。
    /// 4 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBorderHover { get; set; }

    /// <summary>
    /// 主色悬浮态
    /// 主色梯度下的悬浮态。
    /// 5 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryHover { get; set; }

    /// <summary>
    /// 主色激活态
    /// 主色梯度下的深色激活态。
    /// 7 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryActive { get; set; }

    /// <summary>
    /// 主色文本悬浮态
    /// 主色梯度下的文本悬浮态。
    /// 8 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryTextHover { get; set; }

    /// <summary>
    /// 主色文本
    /// 主色梯度下的文本颜色。
    /// 9 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryText { get; set; }

    /// <summary>
    /// 主色文本激活态
    /// 主色梯度下的文本激活态。
    /// 10 号色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryTextActive { get; set; }
}