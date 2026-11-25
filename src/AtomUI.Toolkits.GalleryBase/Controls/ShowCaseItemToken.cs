using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class ShowCaseItemToken : AbstractControlDesignToken
{

    public partial Thickness CardPadding { get; set; }
    public partial CornerRadius CardCornerRadius { get; set; }
    public partial BoxShadows CardShadow { get; set; }
    public partial Thickness PreviewMargin { get; set; }
    public partial Thickness BadgePreviewMargin { get; set; }
    public partial Thickness DescriptionMargin { get; set; }
    public partial FontWeight TitleFontWeight { get; set; }
    public partial double DeferredPlaceholderHeight { get; set; }
    public partial CornerRadius DeferredPlaceholderCornerRadius { get; set; }

}
