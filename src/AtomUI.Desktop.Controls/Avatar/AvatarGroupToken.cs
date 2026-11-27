using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of <see cref="AvatarGroup" />: how far each avatar overlaps the previous one, and the
/// spacing between the avatars folded into the flyout.
/// </summary>
[ControlDesignToken]
public sealed partial class AvatarGroupToken : AbstractControlDesignToken
{
    /// <summary>
    /// Spacing between avatars in the folded avatars popover (antd Avatar groupSpace).
    /// </summary>
    public partial double GroupSpace { get; set; }

    /// <summary>
    /// Overlapping width of adjacent avatars in the group (antd Avatar groupOverlapping).
    /// </summary>
    public partial double GroupOverlapping { get; set; }
}
