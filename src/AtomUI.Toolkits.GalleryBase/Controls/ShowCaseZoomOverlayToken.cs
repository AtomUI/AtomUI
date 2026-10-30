using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed class ShowCaseZoomOverlayToken : AbstractControlDesignToken
{
    public Thickness CardPadding { get; set; }

    public Thickness HeaderMargin { get; set; }

    public FontWeight TitleFontWeight { get; set; }

    public Thickness DescriptionMargin { get; set; }

    public ShowCaseZoomOverlayToken()
    {
    }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        CardPadding        = new Thickness(
            EffectiveGlobalToken.SizeUnit * 10,
            EffectiveGlobalToken.SizeUnit * 12,
            EffectiveGlobalToken.SizeUnit * 10,
            EffectiveGlobalToken.SizeUnit * 8);
        HeaderMargin       = new Thickness(0, 0, 0, EffectiveGlobalToken.SizeUnit * 4);
        TitleFontWeight    = EffectiveGlobalToken.FontWeightStrong;
        DescriptionMargin  = new Thickness(0, EffectiveGlobalToken.SizeUnit * 2 + 2, 0, 0);
    }
}
