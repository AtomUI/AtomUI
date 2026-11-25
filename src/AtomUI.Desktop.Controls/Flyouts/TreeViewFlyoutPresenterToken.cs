using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class TreeViewFlyoutPresenterToken : AbstractPopupCornerToken
{
    
    /// <summary>Background color of the tree view flyout panel.</summary>
    public partial Color PopupBgColor { get; set; }

    /// <summary>Padding inside the tree view flyout panel.</summary>
    public partial Thickness PopupContentPadding { get; set; }

    /// <summary>Minimum width of the tree view flyout panel.</summary>
    public partial double PopupMinWidth { get; set; }

    /// <summary>Maximum width of the tree view flyout panel.</summary>
    public partial double PopupMaxWidth { get; set; }

    /// <summary>Minimum height of the tree view flyout panel.</summary>
    public partial double PopupMinHeight { get; set; }

    /// <summary>Maximum height of the tree view flyout panel.</summary>
    public partial double PopupMaxHeight { get; set; }
    
}
