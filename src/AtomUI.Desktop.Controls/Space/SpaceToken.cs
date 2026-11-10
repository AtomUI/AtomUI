using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class SpaceToken : AbstractControlDesignToken
{

    /// <summary>
    /// 小间距尺寸
    /// </summary>
    public double GapSmallSize  { get; set; }
    /// <summary>
    /// 中等间距尺寸
    /// </summary>
    public double GapMiddleSize  { get; set; }
    
    /// <summary>
    /// 大间距尺寸
    /// </summary>
    public double GapLargeSize  { get; set; }
    
    public SpaceToken()

    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        GapSmallSize  = EffectiveGlobalToken.SpacingXS;
        GapMiddleSize = EffectiveGlobalToken.Spacing;
        GapLargeSize  = EffectiveGlobalToken.SpacingLG;

    }
    
}
