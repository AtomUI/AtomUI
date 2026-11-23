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

}
