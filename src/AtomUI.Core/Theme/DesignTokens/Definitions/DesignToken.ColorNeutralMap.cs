using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
     // ----------   Text   ---------- //
    /// <summary>
    /// 一级文本色
    /// 最深的文本色。为了符合W3C标准，默认的文本颜色使用了该色，同时这个颜色也是最深的中性色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorText { get; set; }

    /// <summary>
    /// 二级文本色
    /// 作为第二梯度的文本色，一般用在不那么需要强化文本颜色的场景，例如 Label 文本、Menu 的文本选中态等场景。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorTextSecondary { get; set; }

    /// <summary>
    /// 三级文本色
    /// 第三级文本色一般用于描述性文本，例如表单的中的补充说明文本、列表的描述性文本等场景。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorTextTertiary { get; set; }

    /// <summary>
    /// 四级文本色
    /// 第四级文本色是最浅的文本色，例如表单的输入提示文本、禁用色文本等。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorTextQuaternary { get; set; }

    // ----------   Border   ---------- //
    /// <summary>
    /// 一级边框色
    /// 默认使用的边框颜色, 用于分割不同的元素，例如：表单的分割线、卡片的分割线等。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBorder { get; set; }

    /// <summary>
    /// 二级边框色
    /// 比默认使用的边框色要浅一级，此颜色和 colorSplit 的颜色一致。使用的是实色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBorderSecondary { get; set; }

    // ----------   Fill   ---------- //

    /// <summary>
    /// 一级填充色
    /// 最深的填充色，用于拉开与二、三级填充色的区分度，目前只用在 Slider 的 hover 效果。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFill { get; set; }

    /// <summary>
    /// 二级填充色
    /// 二级填充色可以较为明显地勾勒出元素形体，如 Rate、Skeleton 等。也可以作为三级填充色的 Hover 状态，如 Table 等。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFillSecondary { get; set; }

    /// <summary>
    /// 三级填充色
    /// 三级填充色用于勾勒出元素形体的场景，如 Slider、Segmented 等。如无强调需求的情况下，建议使用三级填色作为默认填色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFillTertiary { get; set; }

    /// <summary>
    /// 四级填充色
    /// 最弱一级的填充色，适用于不易引起注意的色块，例如斑马纹、区分边界的色块等。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorFillQuaternary { get; set; }

    // ----------   Surface   ---------- //
    /// <summary>
    /// 布局背景色
    /// 该色用于页面整体布局的背景色，只有需要在页面中处于 B1 的视觉层级时才会使用该 token，其他用法都是错误的
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgLayout { get; set; }

    /// <summary>
    /// 组件容器背景色
    /// 组件的容器背景色，例如：默认按钮、输入框等。务必不要将其与 `colorBgElevated` 混淆。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgContainer { get; set; }

    /// <summary>
    /// 浮层容器背景色
    /// 浮层容器背景色，在暗色模式下该 token 的色值会比 `colorBgContainer` 要亮一些。例如：模态框、弹出框、菜单等。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgElevated { get; set; }

    /// <summary>
    /// 引起注意的背景色
    /// 该色用于引起用户强烈关注注意的背景色，目前只用在 Tooltip 的背景色上。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSpotlight { get; set; }

    /// <summary>
    /// 毛玻璃容器背景色
    /// 控制毛玻璃容器的背景色，通常为透明色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgBlur { get; set; }
    
    // ----------   Solid   ---------- //
    /// <summary>
    /// 实心的背景颜色，目前只用在默认实心按钮背景色上。
    /// Solid background color active state, currently only used in the active effect of the default solid button.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSolid { get; set; }
    
    /// <summary>
    /// 实心的背景颜色激活态，目前只用在默认实心按钮的 active 效果。
    /// Solid background color active state, currently only used in the active effect of the default solid button.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSolidActive { get; set; }
    
    /// <summary>
    /// 实心的背景颜色悬浮态，目前只用在默认实心按钮的 hover 效果。
    /// Solid background color hover state, currently only used in the hover effect of the default solid button.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgSolidHover { get; set; }
}