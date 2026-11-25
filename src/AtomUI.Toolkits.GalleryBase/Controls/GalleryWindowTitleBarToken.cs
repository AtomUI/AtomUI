using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryWindowTitleBarToken : AbstractControlDesignToken
{

    public partial FontWeight MenuFontWeight { get; set; }
    public partial Thickness MenuMargin { get; set; }

}
