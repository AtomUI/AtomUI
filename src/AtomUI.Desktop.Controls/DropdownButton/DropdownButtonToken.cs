using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the drop-down button: the margin around the drop-down indicator per button size and the
/// gap between the drop-down popup and its anchor.
/// </summary>
[ControlDesignToken]
public sealed partial class DropdownButtonToken : AbstractButtonGeometryToken
{
    /// <summary>
    /// Margin around the drop-down indicator at the default button size.
    /// </summary>
    public partial Thickness ExtraContentMargin { get; set; }
    /// <summary>
    /// Margin around the drop-down indicator on a large button.
    /// </summary>
    public partial Thickness ExtraContentMarginLG { get; set; }
    /// <summary>
    /// Margin around the drop-down indicator on a small button.
    /// </summary>
    public partial Thickness ExtraContentMarginSM { get; set; }
    /// <summary>
    /// Gap between the drop-down popup and its anchor.
    /// </summary>
    public partial double MarginToAnchor { get; set; }
}
