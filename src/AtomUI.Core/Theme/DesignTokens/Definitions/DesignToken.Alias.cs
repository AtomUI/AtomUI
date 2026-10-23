using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// 内容区域背景色（悬停）
    /// 控制内容区域背景色在鼠标悬停时的样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorFillContentHover { get; set; }

    /// <summary>
    /// 替代背景色
    /// 控制元素替代背景色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorFillAlter { get; set; }

    /// <summary>
    /// 内容区域背景色
    /// 控制内容区域的背景色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorFillContent { get; set; }

    /// <summary>
    /// 容器禁用态下的背景色
    /// 控制容器在禁用状态下的背景色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorBgContainerDisabled { get; set; }

    /// <summary>
    /// 文本悬停态背景色
    /// 控制文本在悬停状态下的背景色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorBgTextHover { get; set; }

    /// <summary>
    /// 文本激活态背景色
    /// 控制文本在激活状态下的背景色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorBgTextActive { get; set; }

    /// <summary>
    /// 背景边框颜色
    /// 控制元素背景边框的颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorBorderBg { get; set; }

    /// <summary>
    /// 分割线颜色
    /// 用于作为分割线的颜色，此颜色和 colorBorderSecondary 的颜色一致，但是用的是透明色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorSplit { get; set; }

    /// <summary>
    /// 占位文本颜色
    /// 控制占位文本的颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorTextPlaceholder { get; set; }

    /// <summary>
    /// 禁用字体颜色
    /// 控制禁用状态下的字体颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorTextDisabled { get; set; }

    /// <summary>
    /// 标题字体颜色
    /// 控制标题字体颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorTextHeading { get; set; }

    /// <summary>
    /// 文本标签字体颜色
    /// 控制文本标签字体颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorTextLabel { get; set; }

    /// <summary>
    /// 文本描述字体颜色
    /// 控制文本描述字体颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorTextDescription { get; set; }

    /// <summary>
    /// 固定文本高亮颜色
    /// 控制带背景色的文本，例如 Primary Button 组件中的文本高亮颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorTextLightSolid { get; set; }

    /// <summary>
    /// 弱操作图标颜色
    /// 控制弱操作图标的颜色，例如 allowClear 或 Alert 关闭按钮。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorIcon { get; set; }

    /// <summary>
    /// 弱操作图标悬浮态颜色
    /// 控制弱操作图标在悬浮状态下的颜色，例如 allowClear 或 Alert 关闭按钮。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorIconHover { get; set; }

    /// <summary>
    /// 高亮颜色
    /// 控制页面元素高亮时的颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorHighlight { get; set; }

    /// <summary>
    /// 输入组件的 Outline 颜色
    /// 控制输入组件的外轮廓线颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorControlOutline { get; set; }

    /// <summary>
    /// 警告状态下的 Outline 颜色
    /// 控制输入组件警告状态下的外轮廓线颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorWarningOutline { get; set; }

    /// <summary>
    /// 错误状态下的 Outline 颜色
    /// 控制输入组件错误状态下的外轮廓线颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorErrorOutline { get; set; }

    // Font
    /// <summary>
    /// 选择器、级联选择器等中的操作图标字体大小
    /// 控制选择器、级联选择器等中的操作图标字体大小。正常情况下与 fontSizeSM 相同。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double FontSizeIcon { get; set; }

    /// <summary>
    /// 内联 Icon 大小定义, 最小的尺寸
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double IconSizeXS { get; set; }

    /// <summary>
    /// 内联 Icon 大小定义
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double IconSizeSM { get; set; }

    /// <summary>
    /// 内联 Icon 大小定义, 正常的尺寸
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double IconSize { get; set; }

    /// <summary>
    /// 内联 Icon 大小定义, 最大的尺寸
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double IconSizeLG { get; set; }

    /// <summary>
    /// 标题类组件（如 h1、h2、h3）或选中项的字体粗细
    /// 控制标题类组件（如 h1、h2、h3）或选中项的字体粗细。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public FontWeight FontWeightStrong { get; set; }

    // Control
    /// <summary>
    /// 输入组件的外轮廓线宽度
    /// 控制标题类组件（如 h1、h2、h3）或选中项的字体粗细。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double ControlOutlineWidth { get; set; }

    /// <summary>
    /// 输入类控件中号内容内边距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness InputPadding { get; set; }

    /// <summary>
    /// 输入类控件小号内容内边距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness InputPaddingSM { get; set; }

    /// <summary>
    /// 输入类控件大号内容内边距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness InputPaddingLG { get; set; }

    /// <summary>
    /// 输入类控件内容相对边框的补偿边距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness InputContentMargin { get; set; }

    /// <summary>
    /// 输入类控件激活态外轮廓阴影。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public BoxShadows InputActiveShadow { get; set; }

    /// <summary>
    /// 输入类控件错误激活态外轮廓阴影。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public BoxShadows InputErrorActiveShadow { get; set; }

    /// <summary>
    /// 输入类控件警告激活态外轮廓阴影。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public BoxShadows InputWarningActiveShadow { get; set; }

    /// <summary>
    /// 控制组件项在鼠标悬浮时的背景颜色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ControlItemBgHover { get; set; } // Note. It also is a color

    /// <summary>
    /// 控制组件项在激活状态下的背景颜色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ControlItemBgActive { get; set; } // Note. It also is a color

    /// <summary>
    /// 控制组件项在鼠标悬浮且激活状态下的背景颜色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ControlItemBgActiveHover { get; set; } // Note. It also is a color

    /// <summary>
    /// 控制组件的交互大小
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double ControlInteractiveSize { get; set; }

    /// <summary>
    /// 控制组件项在禁用状态下的激活背景颜色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ControlItemBgActiveDisabled { get; set; } // Note. It also is a color

    /// <summary>
    /// 线条宽度(聚焦态)
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double LineWidthFocus { get; set; }
    
    /// <summary>
    /// 聚焦态边框厚度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness FocusVisualBorderThickness {  get; set; }
    
    /// <summary>
    /// 聚焦态边框颜色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Color ColorFocusBorder { get; set; }

    /// <summary>
    /// 波浪动画的波动范围
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double WaveAnimationRange { get; set; }

    /// <summary>
    /// 波浪动画的初始透明度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double WaveStartOpacity { get; set; }

    /// <summary>波纹尺寸动画的精确时长，默认跟随慢速动效。</summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public TimeSpan WaveSizeMotionDuration { get; set; }

    /// <summary>波纹透明度动画的精确时长，默认保留慢速动效后的50ms尾部。</summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public TimeSpan WaveOpacityMotionDuration { get; set; }

    // Padding
    /// <summary>
    /// 极小内间距
    /// 控制元素的极小内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPaddingXXS { get; set; }
    
    // Padding
    /// <summary>
    /// 极小内间距
    /// 控制元素的极小内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness PaddingXXS { get; set; }

    /// <summary>
    /// 特小内间距
    /// 控制元素的特小内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPaddingXS { get; set; }
    
    /// <summary>
    /// 特小内间距
    /// 控制元素的特小内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness PaddingXS { get; set; }

    /// <summary>
    /// 小内间距
    /// 控制元素的小内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPaddingSM { get; set; }
    
    /// <summary>
    /// 小内间距
    /// 控制元素的小内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness PaddingSM { get; set; }

    /// <summary>
    /// 内间距
    /// 控制元素的内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPadding { get; set; }
    
    /// <summary>
    /// 内间距
    /// 控制元素的内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness Padding { get; set; }

    /// <summary>
    /// 中等内间距
    /// 控制元素的中等内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPaddingMD { get; set; }
    
    /// <summary>
    /// 中等内间距
    /// 控制元素的中等内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness PaddingMD { get; set; }

    /// <summary>
    /// 大内间距
    /// 控制元素的大内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPaddingLG { get; set; }
    
    /// <summary>
    /// 大内间距
    /// 控制元素的大内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness PaddingLG { get; set; }

    /// <summary>
    /// 特大内间距
    /// 控制元素的特大内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyPaddingXL { get; set; }
    
    /// <summary>
    /// 特大内间距
    /// 控制元素的特大内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness PaddingXL { get; set; }

    // Padding Content
    /// <summary>
    /// 内容水平内间距（LG）
    /// 控制内容元素水平内间距，适用于大屏幕设备。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentHorizontalLG { get; set; }

    /// <summary>
    /// 内容水平内间距
    /// 控制内容元素水平内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentHorizontal { get; set; }

    /// <summary>
    /// 内容水平内间距（SM）
    /// 控制内容元素水平内间距，适用于小屏幕设备。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentHorizontalSM { get; set; }

    /// <summary>
    /// 内容水平内间距（XS）
    /// 控制内容元素水平内间距，适用于小屏幕设备。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentHorizontalXS { get; set; }

    /// <summary>
    /// 内容垂直内间距（LG）
    /// 控制内容元素垂直内间距，适用于大屏幕设备。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentVerticalLG { get; set; }

    /// <summary>
    /// 内容垂直内间距
    /// 控制内容元素垂直内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentVertical { get; set; }

    /// <summary>
    /// 内容垂直内间距（SM）
    /// 控制内容元素垂直内间距，适用于小屏幕设备。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double PaddingContentVerticalSM { get; set; }

    /// <summary>
    /// 外边距 XXS
    /// 控制元素外边距，最小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginXXS { get; set; }
    
    /// <summary>
    /// 外边距 XXS
    /// 控制元素外边距，最小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginXXS { get; set; }

    /// <summary>
    /// 外边距 XS
    /// 控制元素外边距，小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginXS { get; set; }
    
    /// <summary>
    /// 外边距 XS
    /// 控制元素外边距，小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginXS { get; set; }

    /// <summary>
    /// 外边距 SM
    /// 控制元素外边距，中小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginSM { get; set; }
    
    /// <summary>
    /// 外边距 SM
    /// 控制元素外边距，中小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginSM { get; set; }

    /// <summary>
    /// 外边距
    /// 控制元素外边距，中等尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMargin { get; set; }
    
    /// <summary>
    /// 外边距
    /// 控制元素外边距，中等尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness Margin { get; set; }

    /// <summary>
    /// 外边距 MD
    /// 控制元素外边距，中大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginMD { get; set; }
    
    /// <summary>
    /// 外边距 MD
    /// 控制元素外边距，中大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginMD { get; set; }

    /// <summary>
    /// 外边距 LG
    /// 控制元素外边距，大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginLG { get; set; }
    
    /// <summary>
    /// 外边距 LG
    /// 控制元素外边距，大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginLG { get; set; }

    /// <summary>
    /// 外边距 XL
    /// 控制元素外边距，超大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginXL { get; set; }

    /// <summary>
    /// 外边距 XL
    /// 控制元素外边距，超大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginXL { get; set; }
    
    /// <summary>
    /// 外边距 XXL
    /// 控制元素外边距，最大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double UniformlyMarginXXL { get; set; }
    
    /// <summary>
    /// 外边距 XXL
    /// 控制元素外边距，最大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public Thickness MarginXXL { get; set; }
    
    /// <summary>
    /// 布局元素之间间隔，最小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingXXS { get; set; }

    /// <summary>
    /// 布局元素之间间隔，小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingXS { get; set; }

    /// <summary>
    /// 布局元素之间间隔，中小尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingSM { get; set; }

    /// <summary>
    /// 布局元素之间间隔，中等尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double Spacing { get; set; }

    /// <summary>
    /// 布局元素之间间隔，中大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingMD { get; set; }

    /// <summary>
    /// 布局元素之间间隔，大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingLG { get; set; }
    
    /// <summary>
    /// 布局元素之间间隔，超大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingXL { get; set; }
    
    /// <summary>
    /// 布局元素之间间隔，最大尺寸。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double SpacingXXL { get; set; }

    // =============== Legacy: should be remove ===============
    /// <summary>
    /// 加载状态透明度
    /// 控制加载状态的透明度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double OpacityLoading { get; set; }

    /// <summary>
    /// 一级阴影
    /// 控制元素阴影样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public BoxShadows BoxShadows { get; set; }

    /// <summary>
    /// 二级阴影
    /// 控制元素二级阴影样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public BoxShadows BoxShadowsSecondary { get; set; }

    /// <summary>
    /// 三级阴影
    /// 控制元素三级盒子阴影样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public BoxShadows BoxShadowsTertiary { get; set; }

    /// <summary>
    /// 链接文本装饰
    /// 控制链接文本的装饰样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public TextDecorationInfo? LinkDecoration { get; set; }

    /// <summary>
    /// 链接鼠标悬浮时文本装饰
    /// 控制链接鼠标悬浮时文本的装饰样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public TextDecorationInfo? LinkHoverDecoration { get; set; }

    /// <summary>
    /// 链接聚焦时文本装饰
    /// 控制链接聚焦时文本的装饰样式。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public TextDecorationInfo? LinkFocusDecoration { get; set; }

    /// <summary>
    /// 控制水平内间距
    /// 控制元素水平内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double ControlPaddingHorizontal { get; set; }

    /// <summary>
    /// 控制中小尺寸水平内间距
    /// 控制元素中小尺寸水平内间距。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public double ControlPaddingHorizontalSM { get; set; }

    // Media queries breakpoints
    /// <summary>
    /// 屏幕宽度（像素） - 超小屏幕
    /// 控制超小屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXS { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超小屏幕最小值
    /// 控制超小屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXSMin { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超小屏幕最大值
    /// 控制超小屏幕的最大宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXSMax { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 小屏幕
    /// 控制小屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenSM { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 小屏幕最小值
    /// 控制小屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenSMMin { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 小屏幕最大值
    /// 控制小屏幕的最大宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenSMMax { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 中等屏幕
    /// 控制中等屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenMD { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 中等屏幕最小值
    /// 控制中等屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenMDMin { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 中等屏幕最大值
    /// 控制中等屏幕的最大宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenMDMax { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 大屏幕
    /// 控制大屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenLG { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 大屏幕最小值
    /// 控制大屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenLGMin { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 大屏幕最大值
    /// 控制大屏幕的最大宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenLGMax { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超大屏幕
    /// 控制超大屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXL { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超大屏幕最小值
    /// 控制超大屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXLMin { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超大屏幕最大值
    /// 控制超大屏幕的最大宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXLMax { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超超大屏幕
    /// 控制超超大屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXXL { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超超大屏幕最小值
    /// 控制超超大屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXXLMin { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超超大屏幕最大值
    /// 控制超超大屏幕的最大宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXXLMax { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超超超大屏幕
    /// 控制超超超大屏幕的屏幕宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXXXL { get; set; }

    /// <summary>
    /// 屏幕宽度（像素） - 超超超大屏幕最小值
    /// 控制超超超大屏幕的最小宽度。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Alias)]
    public int ScreenXXXLMin { get; set; }
}
