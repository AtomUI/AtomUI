using AtomUI.Theme.TokenSystem;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class SpinToken : AbstractControlDesignToken
{
    public const string ID = "Spin";

    public SpinToken()
        : base(ID)
    {
    }

    /// <summary>
    /// 加载圆圈尺寸
    /// </summary>
    public double DotSize { get; set; }

    /// <summary>
    /// 小号加载圆圈尺寸
    /// </summary>
    public double DotSizeSM { get; set; }

    /// <summary>
    /// 大号加载圆圈尺寸
    /// </summary>
    public double DotSizeLG { get; set; }
    
    /// <summary>
    /// 大号加载器尺寸
    /// </summary>
    public double IndicatorSizeLG { get; set; }
    
    /// <summary>
    /// 加载器尺寸
    /// </summary>
    public double IndicatorSize { get; set; }
    
    /// <summary>
    /// 小号加载器尺寸
    /// </summary>
    public double IndicatorSizeSM { get; set; }

    /// <summary>
    /// 加载器的周期时间
    /// </summary>
    public TimeSpan IndicatorDuration { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var controlHeightLG = SharedToken.ControlHeightLG;
        var controlHeight   = SharedToken.ControlHeight;
        var indicatorSize     = controlHeightLG / 2;
        var indicatorSizeSM   = controlHeightLG * 0.35;
        var indicatorSizeLG   = controlHeight;
        IndicatorDuration = SharedToken.MotionDurationSlow * 4;
        DotSize           = ((indicatorSize - SharedToken.UniformlyMarginXXS / 2) / 2) * 0.75;
        DotSizeLG         = ((indicatorSizeLG - SharedToken.UniformlyMarginXXS) / 2) * 0.75;
        DotSizeSM         = ((indicatorSizeSM - SharedToken.UniformlyMarginXXS / 2) / 2) * 0.75;
        IndicatorSize     = indicatorSize + 2;
        IndicatorSizeSM   = indicatorSizeSM + 1;
        IndicatorSizeLG   = indicatorSizeLG + 4;
    }
}