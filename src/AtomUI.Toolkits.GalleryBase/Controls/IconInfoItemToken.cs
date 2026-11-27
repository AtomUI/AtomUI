using AtomUI.Theme.DesignTokens;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class IconInfoItemToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of the icon name shown with the icon in an item of the icon gallery.
    /// </summary>
    public partial double IconNameFontSize { get; set; }
}
