using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the flyout host: the gap between a flyout and its placement target, its shadow in each
/// hosting mode (overlay or separate popup root) and the offsets that fine-tune its calculated placement.
/// </summary>
[ControlDesignToken]
public sealed partial class FlyoutHostToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Default gap between the flyout and its placement target.
    /// </summary>
    public partial double MarginToAnchor { get; set; }
    
    /// <summary>
    /// Shadow around the flyout when it is hosted in an overlay.
    /// </summary>
    public partial BoxShadows OverlayHostShadow { get; set; }
    
    /// <summary>
    /// Shadow around the flyout when it is hosted in a separate popup root.
    /// </summary>
    public partial BoxShadows PopupRootShadow { get; set; }
    
    /// <summary>
    /// Horizontal offset of the flyout from its calculated placement.
    /// </summary>
    public partial double HorizontalOffset { get; set; }
    
    /// <summary>
    /// Vertical offset of the flyout from its calculated placement.
    /// </summary>
    public partial double VerticalOffset { get; set; }
    
}
