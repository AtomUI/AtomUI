using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// Light background color derived from the error color. Palette index 1 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorBg { get; set; }

    /// <summary>
    /// Light background color derived from the error color for the hover state. Palette index 2 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorBgHover { get; set; }

    /// <summary>
    /// Light background color derived from the error color for the active state. Palette index 3 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorBgActive { get; set; }

    /// <summary>
    /// Border color derived from the error color. Palette index 3 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorBorder { get; set; }

    /// <summary>
    /// Border color derived from the error color for the hover state. Palette index 4 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorBorderHover { get; set; }

    /// <summary>
    /// Darker error color for the hover state. Palette index 5 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorHover { get; set; }

    // The error color itself, palette index 6, is the Seed token ColorError and is declared with the Seed tokens.

    /// <summary>
    /// Darker error color for the active state. Palette index 7 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorActive { get; set; }

    /// <summary>
    /// Text color derived from the error color for the hover state. Palette index 8 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorTextHover { get; set; }

    /// <summary>
    /// Text color derived from the error color for the default state. Palette index 9 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorText { get; set; }

    /// <summary>
    /// Text color derived from the error color for the active state. Palette index 10 of the error color.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorErrorTextActive { get; set; }
}