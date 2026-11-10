using AtomUI.Theme.DesignTokens;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed class ColorItemControlToken : AbstractControlDesignToken
{
    public double HexFontSize { get; set; }

    public double HexOpacity { get; set; }

    public override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HexFontSize = 10;
        HexOpacity = 0.8;
    }
}
