using AtomUI.Theme.DesignTokens;
using AtomUI.Desktop.Controls.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract class AbstractColorPickerToken : AbstractControlDesignToken
{
    public double ColorBlockDisabledOpacity { get; set; }

    public BoxShadows ColorBlockInnerShadows { get; set; }

    public double TransparentBgSize { get; set; }

    public Thickness TriggerPadding { get; set; }

    public Thickness TriggerTextMargin { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        ColorBlockDisabledOpacity = 0.4;
        ColorBlockInnerShadows = new BoxShadows(new BoxShadow { IsInset = true, Spread = EffectiveGlobalToken.LineWidth, Color = EffectiveGlobalToken.ColorFillSecondary });
        TransparentBgSize = EffectiveGlobalToken.SizeXS;
        TriggerPadding = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS - EffectiveGlobalToken.LineWidth);
        TriggerTextMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, EffectiveGlobalToken.UniformlyMarginXS - EffectiveGlobalToken.UniformlyPaddingXXS + EffectiveGlobalToken.LineWidth, 0);
    }
}
