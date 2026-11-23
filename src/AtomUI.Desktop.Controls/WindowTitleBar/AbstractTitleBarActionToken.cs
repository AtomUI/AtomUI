using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractTitleBarActionToken : AbstractControlDesignToken
{
    /// <summary>Resting background color of a title bar action in an active window, behind <see cref="ActiveColor"/>.</summary>
    public partial Color ActiveBgColor { get; set; }
    /// <summary>Foreground color of a title bar action in an active window, over <see cref="ActiveBgColor"/> at rest.</summary>
    public partial Color ActiveColor { get; set; }
    /// <summary>Background color of a hovered title bar action in an active window.</summary>
    public partial Color ActiveHoverBgColor { get; set; }
    /// <summary>Background color of a pressed title bar action in an active window.</summary>
    public partial Color ActivePressedBgColor { get; set; }
    /// <summary>Inset of the painted caption button background from the button's outer bounds.</summary>
    public partial Thickness CaptionButtonBackgroundInset { get; set; }
    /// <summary>Width and height of the icon in a caption button.</summary>
    public partial double CaptionButtonIconSize { get; set; }
    /// <summary>Padding inside a caption button.</summary>
    public partial Thickness CaptionButtonPadding { get; set; }
    /// <summary>
    /// Resting background color of a title bar action in an inactive window, behind <see cref="InactiveColor"/>
    /// .
    /// </summary>
    public partial Color InactiveBgColor { get; set; }
    /// <summary>
    /// Foreground color of a title bar action in an inactive window, over <see cref="InactiveBgColor"/> at
    /// rest.
    /// </summary>
    public partial Color InactiveColor { get; set; }
    /// <summary>Background color of a hovered title bar action in an inactive window.</summary>
    public partial Color InactiveHoverBgColor { get; set; }
}
