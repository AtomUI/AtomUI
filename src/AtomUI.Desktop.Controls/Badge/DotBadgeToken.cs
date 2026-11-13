using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DotBadgeToken : AbstractControlDesignToken
{
    public partial double DotSize { get; set; }

    public partial Color DotColor { get; set; }

    public partial double ShadowSize { get; set; }

    public partial Color ShadowColor { get; set; }

    public partial Thickness LabelMargin { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);

        DotSize     = EffectiveGlobalToken.FontSizeSM / 2;
        DotColor    = EffectiveGlobalToken.ColorError;
        ShadowSize  = EffectiveGlobalToken.LineWidth;
        ShadowColor = EffectiveGlobalToken.ColorBorderBg;
        LabelMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, 0, 0);
    }
}
