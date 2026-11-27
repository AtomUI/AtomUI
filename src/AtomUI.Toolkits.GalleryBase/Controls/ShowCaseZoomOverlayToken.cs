using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCaseZoomOverlayToken : AbstractControlDesignToken
{
    /// <summary>
    /// Padding inside the card of the zoom overlay, around its header and the zoom stage.
    /// </summary>
    public partial Thickness CardPadding { get; set; }

    /// <summary>
    /// Outer margin of the header of the zoom overlay card.
    /// </summary>
    public partial Thickness HeaderMargin { get; set; }

    /// <summary>
    /// Font weight of the title in the header of the zoom overlay card.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }

    /// <summary>
    /// Outer margin of the description block of the zoom overlay card.
    /// </summary>
    public partial Thickness DescriptionMargin { get; set; }
}
