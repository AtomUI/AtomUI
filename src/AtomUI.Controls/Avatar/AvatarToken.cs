using AtomUI.Theme.DesignTokens;
using Avalonia.Media;

namespace AtomUI.Controls;

[ControlDesignToken]
public sealed partial class AvatarToken : AbstractControlDesignToken
{
    /// <summary>Width and height of a regular avatar frame.</summary>
    public partial double ContainerSize { get; set; }

    /// <summary>Width and height of a large avatar frame.</summary>
    public partial double ContainerSizeLG { get; set; }

    /// <summary>Width and height of a small avatar frame.</summary>
    public partial double ContainerSizeSM { get; set; }

    /// <summary>Font size for initials in a regular avatar.</summary>
    public partial double TextFontSize { get; set; }

    /// <summary>Font size for initials in a large avatar.</summary>
    public partial double TextFontSizeLG { get; set; }

    /// <summary>Font size for initials in a small avatar.</summary>
    public partial double TextFontSizeSM { get; set; }

    /// <summary>Outline separating overlapping avatars in a group.</summary>
    public partial Color GroupBorderColor { get; set; }

    /// <summary>Fill behind avatar initials or an icon. Pair with <see cref="AvatarColor"/>.</summary>
    public partial Color AvatarBg { get; set; }

    /// <summary>Foreground of avatar initials or an icon on <see cref="AvatarBg"/>.</summary>
    public partial Color AvatarColor { get; set; }
}
