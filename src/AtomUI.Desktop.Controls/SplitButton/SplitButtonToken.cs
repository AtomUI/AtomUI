using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="SplitButton" />: the gap between the control and its flyout, the color of
/// the seam between its two buttons in the solid styles, and whether that seam is inset.
/// </summary>
[ControlDesignToken]
public sealed partial class SplitButtonToken : AbstractControlDesignToken
{
    /// <summary>
    /// Gap between the split button and its flyout.
    /// </summary>
    public partial double GutterToFlyout { get; set; }

    /// <summary>
    /// Color of the seam between the primary action and flyout toggle in the solid primary style.
    /// </summary>
    public partial Color SolidSeparatorColor { get; set; }

    /// <summary>
    /// Color of the seam between the primary action and flyout toggle in the solid danger style.
    /// </summary>
    public partial Color DangerSeparatorColor { get; set; }

    /// <summary>
    /// Whether a primary split button reserves a layout slot for an inset separator between its two buttons.
    /// </summary>
    public partial bool InsetSeparator { get; set; }
}
