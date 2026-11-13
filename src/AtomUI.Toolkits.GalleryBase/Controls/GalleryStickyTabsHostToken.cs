using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryStickyTabsHostToken : AbstractControlDesignToken
{

    public partial Thickness StickyContentPadding { get; set; }
    public partial IBrush? StickyBackground { get; set; }
    public partial IBrush? StickyBorderBrush { get; set; }

    public GalleryStickyTabsHostToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        StickyContentPadding = new Thickness(EffectiveGlobalToken.SizeUnit * 7, 0);
        StickyBackground     = new SolidColorBrush(EffectiveGlobalToken.ColorBgLayout);
        StickyBorderBrush    = new SolidColorBrush(EffectiveGlobalToken.ColorBorderSecondary);
    }

}
