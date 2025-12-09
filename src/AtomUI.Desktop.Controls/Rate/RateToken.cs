using AtomUI.Theme.Palette;
using AtomUI.Theme.TokenSystem;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal class RateToken : AbstractControlDesignToken
{
    public const string ID = "Rate";
    
    /// <summary>
    /// 星星颜色
    /// Star color
    /// </summary>
    public Color StarColor { get; set; }
    
    /// <summary>
    /// 星星尺寸
    /// Star size
    /// </summary>
    public double StarSize { get; set; }
    
    /// <summary>
    /// 小星星尺寸
    /// Small star size
    /// </summary>
    public double StarSizeSM { get; set; }
    
    /// <summary>
    /// 大星星尺寸
    /// Large star size
    /// </summary>
    public double StarSizeLG { get; set; }
    
    /// <summary>
    /// 星星悬浮时的缩放
    /// Scale of star when hover
    /// </summary>
    public double StarHoverScale { get; set; }
    
    /// <summary>
    /// 星星背景色
    /// Star background color
    /// </summary>
    public Color StarBg { get; set; }
    
    public RateToken()
        : base(ID)
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        StarColor      = SharedToken.ColorPalettes[PresetPrimaryColor.Yellow].Color6;
        StarSize       = SharedToken.ControlHeight * 0.625;
        StarSizeSM     = SharedToken.ControlHeightSM * 0.625;
        StarSizeLG     = SharedToken.ControlHeightLG * 0.625;
        StarHoverScale = 1.2;
        StarBg         = SharedToken.ColorFillContent;
    }
}