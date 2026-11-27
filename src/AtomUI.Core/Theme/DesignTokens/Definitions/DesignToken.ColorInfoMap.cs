using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// Light background tint of the info palette, used as the background of info-styled elements.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoBg { get; set; }

    /// <summary>
    /// Hover state of the info light background tint, read while an info-styled element is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoBgHover { get; set; }

    /// <summary>
    /// Border color of the info palette, used for the border of info-styled elements.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoBorder { get; set; }

    /// <summary>
    /// Hover state of the info border color, read while a control with an info border is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoBorderHover { get; set; }

    /// <summary>
    /// Dark hover shade of the info palette, read while an info-colored element is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoHover { get; set; }

    // The base info color ColorInfo itself is not part of this map. It is declared in DesignToken.Seed.cs.

    /// <summary>
    /// Dark active shade of the info palette, read while an info-colored element is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoActive { get; set; }

    /// <summary>
    /// Info text color for the hover state, read while info-colored text is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoTextHover { get; set; }

    /// <summary>
    /// Info text color in the default state, read for info-colored text.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoText { get; set; }

    /// <summary>
    /// Info text color for the active state, read while info-colored text is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorInfo" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorInfoTextActive { get; set; }
}