using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class CheckBoxToken : AbstractControlDesignToken
{
    
    public CheckBoxToken()

    {
    }
    
    public partial double CheckIndicatorSize { get; set; }
    public partial double CheckedMarkSize { get; set; }

    public partial double IndicatorTristateMarkSize { get; set; }
    
    public partial Thickness TextMargin { get; set; }

    /// <summary>The border of an unchecked (or indeterminate) indicator under the pointer.</summary>
    public partial Color IndicatorHoverBorderColor { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        if (!IsExplicit(CheckBoxTokenKind.CheckIndicatorSize))
        {
            CheckIndicatorSize        = EffectiveGlobalToken.ControlInteractiveSize;
        }

        CheckedMarkSize           = CheckIndicatorSize * 0.6;
        IndicatorTristateMarkSize = EffectiveGlobalToken.FontSizeLG / 2;
        IndicatorHoverBorderColor = EffectiveGlobalToken.ColorPrimaryHover;
        TextMargin                = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, EffectiveGlobalToken.UniformlyMarginXS, 0);
    }
    
}
