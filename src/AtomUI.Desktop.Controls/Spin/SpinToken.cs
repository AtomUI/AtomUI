using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class SpinToken : AbstractControlDesignToken
{
    
    public SpinToken()

    {
    }

    /// <summary>
    /// 加载圆圈尺寸
    /// </summary>
    public partial double DotSize { get; set; }

    /// <summary>
    /// 小号加载圆圈尺寸
    /// </summary>
    public partial double DotSizeSM { get; set; }

    /// <summary>
    /// 大号加载圆圈尺寸
    /// </summary>
    public partial double DotSizeLG { get; set; }
    
    /// <summary>
    /// 大号加载器尺寸
    /// </summary>
    public partial double IndicatorSizeLG { get; set; }
    
    /// <summary>
    /// 加载器尺寸
    /// </summary>
    public partial double IndicatorSize { get; set; }
    
    /// <summary>
    /// 小号加载器尺寸
    /// </summary>
    public partial double IndicatorSizeSM { get; set; }

    /// <summary>
    /// 加载器的周期时间
    /// </summary>
    public partial TimeSpan IndicatorDuration { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;
        var controlHeight   = EffectiveGlobalToken.ControlHeight;
        var indicatorSize     = controlHeightLG / 2;
        var indicatorSizeSM   = controlHeightLG * 0.35;
        var indicatorSizeLG   = controlHeight;
        IndicatorDuration = EffectiveGlobalToken.MotionDurationSlow * 4;
        DotSize           = ((indicatorSize - EffectiveGlobalToken.UniformlyMarginXXS / 2) / 2) * 0.75;
        DotSizeLG         = ((indicatorSizeLG - EffectiveGlobalToken.UniformlyMarginXXS) / 2) * 0.75;
        DotSizeSM         = ((indicatorSizeSM - EffectiveGlobalToken.UniformlyMarginXXS / 2) / 2) * 0.75;
        IndicatorSize     = indicatorSize + 2;
        IndicatorSizeSM   = indicatorSizeSM + 1;
        IndicatorSizeLG   = indicatorSizeLG + 4;
    }
    
}
