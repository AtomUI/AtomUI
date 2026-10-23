using AtomUI.Theme.Algorithms;
using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class RateToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Typeface for character glyph content; retains the platform glyph default.
    /// </summary>
    public FontFamily CharacterFontFamily { get; set; } = FontFamily.Default;

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

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        StarColor      = EffectiveGlobalToken.ColorPalettes[PresetPrimaryColor.Yellow].Color6;
        StarSize       = EffectiveGlobalToken.ControlHeight * 0.625;
        StarSizeSM     = EffectiveGlobalToken.ControlHeightSM * 0.625;
        StarSizeLG     = EffectiveGlobalToken.ControlHeightLG * 0.625;
        StarHoverScale = 1.2;
        StarBg         = EffectiveGlobalToken.ColorFillContent;
    }
    
}
