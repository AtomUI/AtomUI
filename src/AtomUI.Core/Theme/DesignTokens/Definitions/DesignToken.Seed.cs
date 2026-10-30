using Avalonia;
using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// 品牌主色
    /// 品牌色是体现产品特性和传播理念最直观的视觉元素之一。在你完成品牌主色的选取之后，我们会自动帮你生成一套完整的色板，并赋予它们有效的设计语义
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color ColorPrimary { get; set; }

    /// <summary>
    /// 成功色
    /// 用于表示操作成功的 Token 序列，如 Result、Progress 等组件会使用该组梯度变量。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color ColorSuccess { get; set; }

    /// <summary>
    /// 警戒色
    /// 用于表示操作警告的 Token 序列，如 Notification、 Alert等警告类组件或 Input 输入类等组件会使用该组梯度变量。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color ColorWarning { get; set; }

    /// <summary>
    /// 错误色
    /// 用于表示操作失败的 Token 序列，如失败按钮、错误状态提示（Result）组件等。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color ColorError { get; set; }

    /// <summary>
    /// 信息色
    /// 用于表示操作信息的 Token 序列，如 Alert 、Tag、 Progress 等组件都有用到该组梯度变量。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color ColorInfo { get; set; }

    /// <summary>
    /// 基础文本色
    /// 用于派生文本色梯度的基础变量，v5 中我们添加了一层文本色的派生算法可以产出梯度明确的文本色的梯度变量。但请不要在代码中直接使用该 Seed Token ！
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color? ColorTextBase { get; set; }

    /// <summary>
    /// 基础背景色
    /// 用于派生背景色梯度的基础变量，v5 中我们添加了一层背景色的派生算法可以产出梯度明确的背景色的梯度变量。但请不要在代码中直接使用该 Seed Token ！
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color? ColorBgBase { get; set; }

    /// <summary>
    /// 超链接颜色
    /// 控制超链接的颜色。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color? ColorLink { get; set; }

    /// <summary>
    /// 透明色
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public Color ColorTransparent { get; set; }

    //  ----------   Font   ---------- //
    /// <summary>
    /// 字体
    /// 字体家族优先使用系统默认的界面字体，同时提供一套利于屏显的备用字体库，保证在不同平台与显示密度下字体始终保持良好的易读性与可读性。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public FontFamily? FontFamily { get; set; }

    /// <summary>
    /// 默认字号
    /// 设计系统中使用最广泛的字体大小，文本梯度也将基于该字号进行派生。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double FontSize { get; set; } = 14;

    //  ----------   Line   ---------- //
    /// <summary>
    /// 基础线宽
    /// 用于控制组件边框、分割线等的宽度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double LineWidth { get; set; } = 1;

    /// <summary>
    /// 线条样式
    /// 用于控制组件边框、分割线等的样式，默认是实线
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public LineStyle LineType { get; set; } = LineStyle.Solid;

    //  ----------   BorderRadius   ---------- //
    /// <summary>
    /// 基础圆角
    /// 基础组件的圆角大小，例如按钮、输入框、卡片等
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public CornerRadius BorderRadius { get; set; }

    /// <summary>
    /// 尺寸变化单位
    /// 用于控制组件尺寸的变化单位，基础单位为 4，便于更加细致地控制尺寸梯度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double SizeUnit { get; set; } = 4;

    /// <summary>
    /// 尺寸步长
    /// 用于控制组件尺寸的基础步长，尺寸步长结合尺寸变化单位，就可以派生各种尺寸梯度。通过调整步长即可得到不同的布局模式，例如 V5 紧凑模式下的尺寸步长为 2
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double SizeStep { get; set; } = 4;

    /// <summary>
    /// 基础高度
    /// Ant Design 中按钮和输入框等基础控件的高度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double SizePopupArrow { get; set; } = 16;

    /// <summary>
    /// 基础高度
    /// Ant Design 中按钮和输入框等基础控件的高度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double ControlHeight { get; set; } = 32;

    //  ----------   zIndex   ---------- //
    /// <summary>
    /// 浮层基础 zIndex
    /// 所有组件的基础 Z 轴值，用于一些悬浮类的组件的可以基于该值 Z 轴控制层级，例如 BackTop、 Affix 等
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public int ZIndexBase { get; set; } = 0;

    /// <summary>
    /// 浮层基础 zIndex
    /// 浮层类组件的基础 Z 轴值，用于一些悬浮类的组件的可以基于该值 Z 轴控制层级，例如 FloatButton、 Affix、Modal 等
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public int ZIndexPopupBase { get; set; } = 1000;

    //  ----------   Opacity   ---------- //
    /// <summary>
    /// 图片不透明度
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public double OpacityImage { get; set; } = 1.0;

    //  ----------   motion   ---------- //
    /// <summary>
    /// 动画时长变化单位
    /// 用于控制动画时长的变化单位
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public int MotionUnit { get; set; } = 100;

    /// <summary>
    /// 动画基础时长。
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public int MotionBase { get; set; } = 0;

    //  ----------   Style   ---------- //

    /// <summary>
    /// 动画风格
    /// 
    /// 用于配置动画效果，为 `false` 时则关闭动画
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public bool EnableMotion { get; set; } = true;
    
    /// <summary>
    /// 是否开启波浪动画
    ///
    /// 按钮，单选按钮等点击后的波浪动画效果
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Seed)]
    public bool EnableWaveSpirit { get; set; } = true;
}