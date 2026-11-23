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
}
