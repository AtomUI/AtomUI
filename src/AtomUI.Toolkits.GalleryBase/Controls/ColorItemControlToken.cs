using AtomUI.Theme.DesignTokens;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ColorItemControlToken : AbstractControlDesignToken
{
    public partial double HexFontSize { get; set; }

    public partial double HexOpacity { get; set; }

    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        HexFontSize = 10;
        HexOpacity = 0.8;
    }
}
