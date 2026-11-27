using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCaseItemToken : AbstractControlDesignToken
{

    /// <summary>
    /// Padding inside the card of a showcase item, around the preview, the title and the description.
    /// </summary>
    public partial Thickness CardPadding { get; set; }
    /// <summary>
    /// Corner radius of the card of a showcase item.
    /// </summary>
    public partial CornerRadius CardCornerRadius { get; set; }
    /// <summary>
    /// Shadow of the card of a showcase item.
    /// </summary>
    public partial BoxShadows CardShadow { get; set; }
    /// <summary>
    /// Outer margin of the preview area of a showcase item.
    /// </summary>
    public partial Thickness PreviewMargin { get; set; }
    /// <summary>
    /// Outer margin of the preview area of a showcase item that carries a ribbon badge. The badge template uses it
    /// in place of <see cref="PreviewMargin" />.
    /// </summary>
    public partial Thickness BadgePreviewMargin { get; set; }
    /// <summary>
    /// Outer margin of the description block of a showcase item.
    /// </summary>
    public partial Thickness DescriptionMargin { get; set; }
    /// <summary>
    /// Font weight of the title of a showcase item.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    /// <summary>
    /// Minimum height of the placeholder that stands in for the preview of a showcase item while the item waits to
    /// be loaded. The theme forwards it to the <c>DeferredPlaceholderHeight</c> property of the item.
    /// </summary>
    public partial double DeferredPlaceholderHeight { get; set; }
    /// <summary>
    /// Corner radius of the placeholder that stands in for the preview of a showcase item while the item waits to
    /// be loaded.
    /// </summary>
    public partial CornerRadius DeferredPlaceholderCornerRadius { get; set; }

}
