using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens that depend on the variant of a button: the fills, outlines, label and icon foregrounds and
/// shadows of the default, primary, danger, ghost, link and text variants. Each design language fills them in
/// its <c>ButtonVariantTokenCalculator</c>. The dropdown, submit and reset button tokens inherit this layer as
/// well.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractButtonVariantToken : AbstractControlDesignToken
{
    /// <summary>
    /// Foreground over the solid danger fill supplied by <see cref="DangerAccentColor" />.
    /// </summary>
    public partial Color DangerColor { get; set; }

    /// <summary>
    /// Resting danger color used on the solid fill and on the outline or text of other danger variants.
    /// </summary>
    public partial Color DangerAccentColor { get; set; }

    /// <summary>
    /// Elevation shadow underneath the danger button.
    /// </summary>
    public partial BoxShadows DangerShadow { get; set; }

    /// <summary>
    /// Fill of a pressed default button. Pair with <see cref="DefaultActiveColor" />.
    /// </summary>
    public partial Color DefaultActiveBg { get; set; }

    /// <summary>
    /// Outline stroke of a pressed default button.
    /// </summary>
    public partial Color DefaultActiveBorderColor { get; set; }

    /// <summary>
    /// Label and icon foreground over <see cref="DefaultActiveBg" /> while pressed.
    /// </summary>
    public partial Color DefaultActiveColor { get; set; }

    /// <summary>
    /// Resting fill of the default button. Pair with <see cref="DefaultColor" />.
    /// </summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>
    /// Outline stroke of the default button at rest.
    /// </summary>
    public partial Color DefaultBorderColor { get; set; }

    /// <summary>
    /// Resting label and icon foreground over <see cref="DefaultBg" />.
    /// </summary>
    public partial Color DefaultColor { get; set; }

    /// <summary>
    /// Outline stroke of a default ghost button.
    /// </summary>
    public partial Color DefaultGhostBorderColor { get; set; }

    /// <summary>
    /// Label and icon foreground over <see cref="GhostBg" /> in the default ghost variant.
    /// </summary>
    public partial Color DefaultGhostColor { get; set; }

    /// <summary>
    /// Fill of a default button under the pointer. Pair with <see cref="DefaultHoverColor" />.
    /// </summary>
    public partial Color DefaultHoverBg { get; set; }

    /// <summary>
    /// Outline stroke of a default button under the pointer.
    /// </summary>
    public partial Color DefaultHoverBorderColor { get; set; }

    /// <summary>
    /// Label and icon foreground over <see cref="DefaultHoverBg" /> under the pointer.
    /// </summary>
    public partial Color DefaultHoverColor { get; set; }

    /// <summary>
    /// Elevation shadow of the default button.
    /// </summary>
    public partial BoxShadows DefaultShadow { get; set; }

    /// <summary>
    /// Background of the ghost button behind its foreground and outline.
    /// </summary>
    public partial Color GhostBg { get; set; }

    /// <summary>
    /// Resting label and icon foreground in the link variant. Hover and pressed foregrounds use the global link roles.
    /// </summary>
    public partial Color LinkColor { get; set; }

    /// <summary>
    /// Fill behind a link button under the pointer.
    /// </summary>
    public partial Color LinkHoverBg { get; set; }

    /// <summary>
    /// Label and icon foreground on the solid primary fill.
    /// </summary>
    public partial Color PrimaryColor { get; set; }

    /// <summary>
    /// Elevation shadow of the primary button.
    /// </summary>
    public partial BoxShadows PrimaryShadow { get; set; }

    /// <summary>
    /// Label and icon foreground on the default solid button fill.
    /// </summary>
    public partial Color SolidTextColor { get; set; }

    /// <summary>
    /// Fill under the label and icon when the pointer is over a text button.
    /// </summary>
    public partial Color TextHoverBg { get; set; }

    /// <summary>
    /// Label and icon foreground while a text button is pressed.
    /// </summary>
    public partial Color TextTextActiveColor { get; set; }

    /// <summary>
    /// Resting label and icon foreground of a text button.
    /// </summary>
    public partial Color TextTextColor { get; set; }

    /// <summary>
    /// Label and icon foreground on <see cref="TextHoverBg" /> under the pointer.
    /// </summary>
    public partial Color TextTextHoverColor { get; set; }
}
