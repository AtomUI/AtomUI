using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class TourToken : AbstractControlDesignToken, IControlTokenDefaults
{
    
    /// <summary>
    /// 关闭按钮尺寸
    /// Close button size
    /// </summary>
    public double CloseBtnSize { get; set; }
    
    /// <summary>
    /// Primary 模式上一步按钮背景色
    /// Background color of previous button in primary type
    /// </summary>
    public Color PrimaryPrevBtnBg { get; set; }
    
    /// <summary>
    /// Primary 模式下一步按钮悬浮背景色
    /// Hover background color of next button in primary type
    /// </summary>
    public Color PrimaryNextBtnHoverBg { get; set; }
    
    /// <summary>
    /// Tour 内容视图的最小宽度
    /// </summary>
    public double TourViewMinWidth { get; set; }
    
    /// <summary>
    /// Tour 内容视图的最小高度
    /// </summary>
    public double TourViewMinHeight { get; set; }
    
    /// <summary>
    /// 标题字体颜色
    /// Font color of title
    /// </summary>
    public Color HeaderColor { get; set; }

    #region 内部使用
    
    public double IndicatorSize { get; set; }
    public double PopupMarginToAnchor { get; set; }
    public CornerRadius TourBorderRadius { get; set; }
    
    #endregion
    


    [ControlTokenDefault(nameof(CloseBtnSize))]
    private static double DefaultForCloseBtnSize(in ControlTokenDefaultContext context) => context.Global.FontSize * context.Global.RelativeLineHeight;

    [ControlTokenDefault(nameof(PrimaryPrevBtnBg))]
    private static Color DefaultForPrimaryPrevBtnBg(in ControlTokenDefaultContext context) => context.Global.ColorTextLightSolid.SetAlphaF(0.15);

    [ControlTokenDefault(nameof(PrimaryNextBtnHoverBg))]
    private static Color DefaultForPrimaryNextBtnHoverBg(in ControlTokenDefaultContext context) => ColorUtils.OnBackground(context.Global.ColorBgTextHover, context.Global.ColorWhite);

    [ControlTokenDefault(nameof(IndicatorSize))]
    private static double DefaultForIndicatorSize(in ControlTokenDefaultContext context) => 6;

    [ControlTokenDefault(nameof(TourBorderRadius))]
    private static CornerRadius DefaultForTourBorderRadius(in ControlTokenDefaultContext context) => context.Global.BorderRadiusLG;

    [ControlTokenDefault(nameof(TourViewMinWidth))]
    private static double DefaultForTourViewMinWidth(in ControlTokenDefaultContext context) => 200;

    [ControlTokenDefault(nameof(TourViewMinHeight))]
    private static double DefaultForTourViewMinHeight(in ControlTokenDefaultContext context) => 120;

    [ControlTokenDefault(nameof(HeaderColor))]
    private static Color DefaultForHeaderColor(in ControlTokenDefaultContext context) => context.Global.ColorTextHeading;

    [ControlTokenDefault(nameof(PopupMarginToAnchor))]
    private static double DefaultForPopupMarginToAnchor(in ControlTokenDefaultContext context) => context.Global.SpacingXXS;
}
