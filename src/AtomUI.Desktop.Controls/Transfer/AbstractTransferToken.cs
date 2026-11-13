using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTransferToken : AbstractControlDesignToken
{
    public partial double ListWidth { get; set; }

    public partial double ListHeight { get; set; }

    public partial double HeaderHeight { get; set; }

    public partial Thickness HeaderPadding { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
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
