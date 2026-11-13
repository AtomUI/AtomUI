using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TourToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 关闭按钮尺寸
    /// Close button size
    /// </summary>
    public partial double CloseBtnSize { get; set; }
    
    /// <summary>
    /// Primary 模式上一步按钮背景色
    /// Background color of previous button in primary type
    /// </summary>
    public partial Color PrimaryPrevBtnBg { get; set; }
    
    /// <summary>
    /// Primary 模式下一步按钮悬浮背景色
    /// Hover background color of next button in primary type
    /// </summary>
    public partial Color PrimaryNextBtnHoverBg { get; set; }
    
    /// <summary>
    /// Tour 内容视图的最小宽度
    /// </summary>
    public partial double TourViewMinWidth { get; set; }
    
    /// <summary>
    /// Tour 内容视图的最小高度
    /// </summary>
    public partial double TourViewMinHeight { get; set; }
    
    /// <summary>
    /// 标题字体颜色
    /// Font color of title
    /// </summary>
    public partial Color HeaderColor { get; set; }

    #region 内部使用
    
    public partial double IndicatorSize { get; set; }
    public partial double PopupMarginToAnchor { get; set; }
    public partial CornerRadius TourBorderRadius { get; set; }
    
    #endregion
    
    public TourToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CloseBtnSize          = EffectiveGlobalToken.FontSize * EffectiveGlobalToken.RelativeLineHeight;
        PrimaryPrevBtnBg      = EffectiveGlobalToken.ColorTextLightSolid.SetAlphaF(0.15);
        PrimaryNextBtnHoverBg = ColorUtils.OnBackground(EffectiveGlobalToken.ColorBgTextHover, EffectiveGlobalToken.ColorWhite);
        IndicatorSize         = 6;
        TourBorderRadius      = EffectiveGlobalToken.BorderRadiusLG;
        TourViewMinWidth      = 200;
        TourViewMinHeight     = 120;
        HeaderColor           = EffectiveGlobalToken.ColorTextHeading;
        PopupMarginToAnchor   = EffectiveGlobalToken.SpacingXXS;
    }
    
}
