using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the desktop tag. Next to the tag metrics inherited from <see cref="AbstractTagToken" />,
/// they hold the main color of each status, the background and text color of a tag without a color, the
/// text color of Solid tags and the size of the close icon.
/// </summary>
[ControlDesignToken]
public sealed partial class TagToken : AbstractTagToken
{
    /// <summary>
    /// Main color of a tag with the success status, in place of the global success color. It paints the text
    /// and the outline, and the fill of the Solid variant or of a Filled tag while
    /// <see cref="FilledStatusSolid" /> is on. The light status background and the border of the Outlined
    /// variant come from the global ColorSuccessBg / ColorSuccessBorder.
    /// </summary>
    public partial Color SuccessColor { get; set; }

    /// <summary>
    /// Main color of a tag with the info (processing) status, in place of the global info color. It paints the
    /// text and the outline, and the fill of the Solid variant or of a Filled tag while
    /// <see cref="FilledStatusSolid" /> is on. The light status background and the border of the Outlined
    /// variant come from the global ColorInfoBg / ColorInfoBorder.
    /// </summary>
    public partial Color InfoColor { get; set; }

    /// <summary>
    /// Main color of a tag with the warning status, in place of the global warning color. It paints the text
    /// and the outline, and the fill of the Solid variant or of a Filled tag while
    /// <see cref="FilledStatusSolid" /> is on. The light status background and the border of the Outlined
    /// variant come from the global ColorWarningBg / ColorWarningBorder.
    /// </summary>
    public partial Color WarningColor { get; set; }

    /// <summary>
    /// Main color of a tag with the error status, in place of the global error color. It paints the text and
    /// the outline, and the fill of the Solid variant or of a Filled tag while
    /// <see cref="FilledStatusSolid" /> is on. The light status background and the border of the Outlined
    /// variant come from the global ColorErrorBg / ColorErrorBorder.
    /// </summary>
    public partial Color ErrorColor { get; set; }

    /// <summary>
    /// Background of a tag without a preset, status or custom color; the Solid and Outline variants set their
    /// own background.
    /// </summary>
    public partial Color DefaultBg { get; set; }

    /// <summary>
    /// Text color of a tag without a preset, status or custom color; the Solid variant uses
    /// <see cref="SolidTextColor" /> instead. The icon and the close button take the same color.
    /// </summary>
    public partial Color DefaultColor { get; set; }

    /// <summary>
    /// Width and height of the icon in the tag's close button.
    /// </summary>
    public partial double TagCloseIconSize { get; set; }

    /// <summary>
    /// Text color of a tag of the Solid variant that has no preset, status or custom color.
    /// </summary>
    public partial Color SolidTextColor { get; set; }

    /// <summary>
    /// Whether a Filled status tag is painted like the Solid variant, with the status color as fill and the
    /// contrast text as text color. While it is <see langword="false" />, a Filled status tag keeps the light
    /// status background with the status color as text.
    /// </summary>
    public partial bool FilledStatusSolid { get; set; }
}
