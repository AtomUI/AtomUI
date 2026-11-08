using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal sealed class CheckBoxToken : AbstractControlDesignToken
{
    
    public CheckBoxToken()

    {
    }
    
    public double CheckIndicatorSize { get; set; }
    public double CheckedMarkSize { get; set; }

    public double IndicatorTristateMarkSize { get; set; }
    
    public Thickness TextMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(CheckBoxTokenKind.CheckIndicatorSize))
        {
            CheckIndicatorSize        = EffectiveGlobalToken.ControlInteractiveSize;
        }

        CheckedMarkSize           = CheckIndicatorSize * 0.6;
        IndicatorTristateMarkSize = EffectiveGlobalToken.FontSizeLG / 2;
        TextMargin                = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
    }
    
}
