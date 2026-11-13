using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryWindowTitleBarToken : AbstractControlDesignToken
{

    public partial FontWeight MenuFontWeight { get; set; }
    public partial Thickness MenuMargin { get; set; }

    public GalleryWindowTitleBarToken()

    {
    }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        MenuFontWeight = FontWeight.Normal;
        MenuMargin     = new Thickness(0, 0, EffectiveGlobalToken.SizeUnit * 2, 0);
    }

}
