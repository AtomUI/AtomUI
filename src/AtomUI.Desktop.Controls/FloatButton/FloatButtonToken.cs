using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class FloatButtonToken : AbstractControlDesignToken, IControlTokenDefaults
{

    /// <summary>
    /// FloatButton 尺寸
    /// Size of FloatButton
    /// </summary>
    public double FloatButtonSize { get; set; }
    
    /// <summary>
    /// FloatButton 图标尺寸
    /// Icon size of FloatButton
    /// </summary>
    public double FloatButtonIconSize { get; set; }
    
    /// <summary>
    /// 正方形的徽章偏移大小
    /// </summary>
    public double SquareBadgeOffset { get; set; }
    
    /// <summary>
    /// 原型的徽章偏移大小
    /// </summary>
    public double CircleBadgeOffset { get; set; }
    
    /// <summary>
    /// 主要按钮文本颜色
    /// </summary>
    public Color PrimaryColor { get; set; }
    
    /// <summary>
    /// 描述文本行高
    /// </summary>
    public double DescriptionLineHeight { get; set; }
    
    /// <summary>
    /// 默认的基于定位的水平偏移值
    /// </summary>
    public double FloatOffsetX { get; set; }
    
    /// <summary>
    /// 默认的基于定位的垂直偏移值
    /// </summary>
    public double FloatOffsetY { get; set; }
    
    [ControlTokenDefault(nameof(PrimaryColor))]
    private static Color DefaultForPrimaryColor(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid;

    [ControlTokenDefault(nameof(FloatButtonIconSize))]
    private static double DefaultForFloatButtonIconSize(in ControlTokenDefaultContext context) => context.Global.FontSizeIcon * 1.5;

    [ControlTokenDefault(nameof(FloatButtonSize))]
    private static double DefaultForFloatButtonSize(in ControlTokenDefaultContext context) => context.Global.ControlHeightLG;

    [ControlTokenDefault(nameof(DescriptionLineHeight))]
    private static double DefaultForDescriptionLineHeight(in ControlTokenDefaultContext context) => context.Global.FontSizeSM * 1.2;

    [ControlTokenDefault(nameof(FloatOffsetX))]
    private static double DefaultForFloatOffsetX(in ControlTokenDefaultContext context) => context.Global.UniformlyMargin;

    [ControlTokenDefault(nameof(FloatOffsetY))]
    private static double DefaultForFloatOffsetY(in ControlTokenDefaultContext context) => context.Global.UniformlyMargin;

    [ControlTokenDefault(nameof(SquareBadgeOffset))]
    private static double DefaultForSquareBadgeOffset(in ControlTokenDefaultContext context) => context.Global.BorderRadius.BottomLeft * ((Math.Sqrt(2) - 1) / Math.Sqrt(2));

    [ControlTokenDefault(nameof(CircleBadgeOffset))]
    private static double DefaultForCircleBadgeOffset(in ControlTokenDefaultContext context) => context.Global.ControlHeight / 2 * ((Math.Sqrt(2) - 1) / Math.Sqrt(2));
}
