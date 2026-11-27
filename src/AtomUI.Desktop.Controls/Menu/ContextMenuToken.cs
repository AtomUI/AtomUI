using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="ContextMenu" /> popup. They add the offset of the popup from its anchor to the
/// shared values of <see cref="AbstractMenuPopupToken" />.
/// </summary>
[ControlDesignToken]
public sealed partial class ContextMenuToken : AbstractMenuPopupToken
{
    /// <summary>
    /// Horizontal offset of the context menu popup from its anchor.
    /// </summary>
    public partial double ContextMenuOffsetX { get; set; }
    /// <summary>
    /// Vertical offset of the context menu popup from its anchor.
    /// </summary>
    public partial double ContextMenuOffsetY { get; set; }
}
