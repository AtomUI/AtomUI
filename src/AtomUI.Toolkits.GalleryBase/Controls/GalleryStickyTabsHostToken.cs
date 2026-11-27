using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryStickyTabsHostToken : AbstractControlDesignToken
{

    /// <summary>
    /// Padding inside the sticky content border of the <see cref="GalleryStickyTabsHost" />, around the sticky
    /// content.
    /// </summary>
    public partial Thickness StickyContentPadding { get; set; }
    /// <summary>
    /// Background of the sticky content border of the <see cref="GalleryStickyTabsHost" />. The border is shown
    /// only while the host has sticky content.
    /// </summary>
    public partial IBrush? StickyBackground { get; set; }
    /// <summary>
    /// Border brush of the sticky content border of the <see cref="GalleryStickyTabsHost" />.
    /// The theme draws only the bottom edge of the border with it.
    /// </summary>
    public partial IBrush? StickyBorderBrush { get; set; }

}
