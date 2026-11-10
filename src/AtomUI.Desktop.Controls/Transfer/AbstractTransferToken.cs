using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
internal abstract class AbstractTransferToken : AbstractControlDesignToken
{
    public double ListWidth { get; set; }

    public double ListHeight { get; set; }

    public double HeaderHeight { get; set; }

    public Thickness HeaderPadding { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        ListWidth    = 180;
        ListHeight   = 200;
        HeaderHeight = EffectiveGlobalToken.ControlHeightLG;
        HeaderPadding = new Thickness(
            EffectiveGlobalToken.UniformlyPaddingSM,
            Math.Ceiling((EffectiveGlobalToken.ControlHeightLG -
                          EffectiveGlobalToken.LineWidth -
                          EffectiveGlobalToken.FontHeight) / 2));
    }
}
