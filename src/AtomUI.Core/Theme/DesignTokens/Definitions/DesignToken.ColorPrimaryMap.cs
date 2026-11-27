using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    // The brand primary color ColorPrimary itself is not part of this map. It is declared in DesignToken.Seed.cs.

    /// <summary>
    /// Light background tint of the primary palette, generally for selected states of low visual weight.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBg { get; set; }

    /// <summary>
    /// Hover state of the primary light background tint.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBgHover { get; set; }

    /// <summary>
    /// Border color of the primary palette, used for the borders of controls such as Slider.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBorder { get; set; }

    /// <summary>
    /// Hover state of the primary border color, used while a bordered control such as Slider or Button is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryBorderHover { get; set; }

    /// <summary>
    /// Hover shade of the primary color, read while a primary-colored element is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryHover { get; set; }

    /// <summary>
    /// Dark active shade of the primary color, read while a primary-colored element is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryActive { get; set; }

    /// <summary>
    /// Primary text color for the hover state, read while primary-colored text is hovered.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryTextHover { get; set; }

    /// <summary>
    /// Primary text color in the default state, read for primary-colored text.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryText { get; set; }

    /// <summary>
    /// Primary text color for the active state, read while primary-colored text is pressed.
    /// The Ant Design language derives it from the palette generated for <see cref="ColorPrimary" />.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorPrimaryTextActive { get; set; }
}