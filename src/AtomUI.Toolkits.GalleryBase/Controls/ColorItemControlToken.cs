using AtomUI.Theme.DesignTokens;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ColorItemControlToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of the hexadecimal value shown on the color swatch.
    /// </summary>
    public partial double HexFontSize { get; set; }

    /// <summary>
    /// Opacity of the hexadecimal value shown on the color swatch, from 0 to 1.
    /// </summary>
    public partial double HexOpacity { get; set; }
}
