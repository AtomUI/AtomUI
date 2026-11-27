using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

/// <summary>
/// Design tokens of the avatar: frame size and icon size per size preset, the outline color of grouped
/// avatars, and the fill and foreground of text and icons.
/// </summary>
[ControlDesignToken]
public sealed partial class AvatarToken : AbstractControlDesignToken
{
    /// <summary>
    /// Width and height of a regular avatar frame.
    /// </summary>
    public partial double ContainerSize { get; set; }

    /// <summary>
    /// Width and height of a large avatar frame.
    /// </summary>
    public partial double ContainerSizeLG { get; set; }

    /// <summary>
    /// Width and height of a small avatar frame.
    /// </summary>
    public partial double ContainerSizeSM { get; set; }

    /// <summary>
    /// Width and height of the icon of a regular avatar; it does not set the font size of the initials.
    /// </summary>
    public partial double TextFontSize { get; set; }

    /// <summary>
    /// Width and height of the icon of a large avatar; it does not set the font size of the initials.
    /// </summary>
    public partial double TextFontSizeLG { get; set; }

    /// <summary>
    /// Width and height of the icon of a small avatar; it does not set the font size of the initials.
    /// </summary>
    public partial double TextFontSizeSM { get; set; }

    /// <summary>
    /// Outline separating overlapping avatars in a group.
    /// </summary>
    public partial Color GroupBorderColor { get; set; }

    /// <summary>
    /// Fill behind avatar initials or an icon. Pair with <see cref="AvatarColor" />.
    /// </summary>
    public partial Color AvatarBg { get; set; }

    /// <summary>
    /// Foreground of avatar initials or an icon on <see cref="AvatarBg" />.
    /// </summary>
    public partial Color AvatarColor { get; set; }
}
