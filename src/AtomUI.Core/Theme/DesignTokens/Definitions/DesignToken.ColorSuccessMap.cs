using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// Light background tint of the success palette, used for the success state of Tag and Alert.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBg { get; set; }

    /// <summary>
    /// Hover state of the success light background tint. The Button variant token calculations read it.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBgHover { get; set; }

    /// <summary>
    /// Border color of the success palette, used for the success state of Tag and Alert.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBorder { get; set; }

    /// <summary>
    /// Hover state of the success border color, read while a control with a success border is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessBorderHover { get; set; }

    /// <summary>
    /// Dark hover shade of the success palette, read while a success-colored element is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessHover { get; set; }

    // The base success color ColorSuccess itself is not part of this map. It is declared in DesignToken.Seed.cs.

    /// <summary>
    /// Dark active shade of the success palette, read while a success-colored element is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessActive { get; set; }

    /// <summary>
    /// Success text color for the hover state, read while success-colored text is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessTextHover { get; set; }

    /// <summary>
    /// Success text color in the default state, read for success-colored text.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessText { get; set; }

    /// <summary>
    /// Success text color for the active state, read while success-colored text is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorSuccess" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorSuccessTextActive { get; set; }
}