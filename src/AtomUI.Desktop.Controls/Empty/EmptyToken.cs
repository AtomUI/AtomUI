using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class EmptyToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// 空图片的高度
    /// </summary>
    public partial double EmptyImgHeight { get; set; }

    public partial double EmptyImgHeightSM { get; set; }
    public partial double EmptyImgHeightMD { get; set; }
    
    public partial Thickness DescriptionMargin { get; set; }
    public partial Thickness DescriptionMarginSM { get; set; }
    public partial Thickness FooterMargin { get; set; }

    public EmptyToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var controlHeightLG = EffectiveGlobalToken.ControlHeightLG;
        EmptyImgHeight      = controlHeightLG * 2.5;
        EmptyImgHeightMD    = controlHeightLG * 1.85;
        EmptyImgHeightSM    = controlHeightLG * 0.875;
        DescriptionMargin   = new Thickness(0, EffectiveGlobalToken.UniformlyMarginSM, 0, 0);
        DescriptionMarginSM = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        FooterMargin        = new Thickness(0, EffectiveGlobalToken.UniformlyMargin, 0, 0);
    }
    
}
