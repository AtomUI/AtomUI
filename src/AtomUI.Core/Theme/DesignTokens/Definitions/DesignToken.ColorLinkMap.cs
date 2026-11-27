using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    // The base link color ColorLink itself is not part of this map; only its hover and active states are.

    /// <summary>
    /// Color of a hyperlink while it is hovered.
    /// The Ant Design language derives it from the link palette, which it generates for <c>ColorLink</c>, or for
    /// <see cref="ColorInfo" /> when <c>ColorLink</c> is null.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorLinkHover { get; set; }

    /// <summary>
    /// Color of a hyperlink in the active state, that is while it is being pressed.
    /// The Ant Design language derives it from the link palette, which it generates for <c>ColorLink</c>, or for
    /// <see cref="ColorInfo" /> when <c>ColorLink</c> is null.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorLinkActive { get; set; }
}