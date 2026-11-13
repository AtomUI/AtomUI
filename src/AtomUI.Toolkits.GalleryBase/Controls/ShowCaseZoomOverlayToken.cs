using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCaseZoomOverlayToken : AbstractControlDesignToken
{
    public partial Thickness CardPadding { get; set; }

    public partial Thickness HeaderMargin { get; set; }

    public partial FontWeight TitleFontWeight { get; set; }

    public partial Thickness DescriptionMargin { get; set; }

    public ShowCaseZoomOverlayToken()
    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
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
