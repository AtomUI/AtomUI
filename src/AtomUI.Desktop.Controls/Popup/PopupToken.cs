using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design token of the popup layer that <see cref="Popup" /> and the dropdown and menu controls share: the shadow
/// of the popup in each hosting mode and its default gap to the placement target, on top of the corner radius of
/// <see cref="AbstractPopupCornerToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class PopupToken : AbstractPopupCornerToken
{

    /// <summary>
    /// Shadow of the popup when hosted in an overlay.
    /// </summary>
    public partial BoxShadows OverlayHostShadow { get; set; }

    /// <summary>
    /// Shadow of the popup when hosted in a separate popup root.
    /// </summary>
    public partial BoxShadows PopupRootShadow { get; set; }

    /// <summary>
    /// Default gap between the popup and its placement target.
    /// </summary>
    public partial double MarginToAnchor { get; set; }
}
