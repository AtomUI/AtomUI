using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Toolkits.GalleryBase.Controls;

[ControlDesignToken]
internal sealed partial class GalleryWindowTitleBarToken : AbstractControlDesignToken
{

    /// <summary>
    /// Font weight set on the <see cref="GalleryWindowTitleBar" /> and on the content presenter of its left
    /// add-on area, so that the text of the menu inherits it.
    /// </summary>
    public partial FontWeight MenuFontWeight { get; set; }
    /// <summary>
    /// Outer margin of the content presenter that hosts the menu in the left add-on area of the title bar.
    /// </summary>
    public partial Thickness MenuMargin { get; set; }

}
