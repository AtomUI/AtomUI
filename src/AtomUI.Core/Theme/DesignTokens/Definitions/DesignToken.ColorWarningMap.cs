using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// Light background tint of the warning palette, used as the background of warning-styled elements.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningBg { get; set; }

    /// <summary>
    /// Hover state of the warning light background tint, read while a warning-styled element is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningBgHover { get; set; }

    /// <summary>
    /// Border color of the warning palette, used for the border of warning-styled elements.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningBorder { get; set; }

    /// <summary>
    /// Hover state of the warning border color, read while a control with a warning border is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningBorderHover { get; set; }

    /// <summary>
    /// Dark hover shade of the warning palette, read while a warning-colored element is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningHover { get; set; }

    // The base warning color ColorWarning itself is not part of this map. It is declared in DesignToken.Seed.cs.

    /// <summary>
    /// Dark active shade of the warning palette, read while a warning-colored element is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningActive { get; set; }

    /// <summary>
    /// Warning text color for the hover state, read while warning-colored text is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningTextHover { get; set; }

    /// <summary>
    /// Warning text color in the default state, read for warning-colored text.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningText { get; set; }

    /// <summary>
    /// Warning text color for the active state, read while warning-colored text is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorWarning" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWarningTextActive { get; set; }
}