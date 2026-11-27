using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the <see cref="CompactSpaceAddOn" />: the background color of the add-on and its
/// padding for each size preset.
/// </summary>
[ControlDesignToken]
public sealed partial class CompactSpaceAddOnToken : AbstractControlDesignToken
{
    /// <summary>
    /// Background color of the add-on in the outlined and filled styles. Other styles leave the add-on
    /// transparent. The disabled state, and the error and warning statuses of the filled style, use their own
    /// colors instead.
    /// </summary>
    public partial Color AddonBg { get; set; }

    /// <summary>
    /// Padding inside the add-on at the default size.
    /// </summary>
    public partial Thickness AddOnPadding { get; set; }

    /// <summary>
    /// Padding inside a small add-on.
    /// </summary>
    public partial Thickness AddOnPaddingSM { get; set; }

    /// <summary>
    /// Padding inside a large add-on.
    /// </summary>
    public partial Thickness AddOnPaddingLG { get; set; }
}
