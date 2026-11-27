using Avalonia.Media;

namespace AtomUI.Theme.DesignTokens;

[GlobalDesignToken]
public partial class DesignToken
{
    /// <summary>
    /// Pure white. It does not change with the theme.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorWhite { get; set; } = Color.FromRgb(255, 255, 255);

    /// <summary>
    /// Mask color that covers the content behind a floating layer.
    /// The masks of Drawer, overlay dialogs and other masked components read this token.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBgMask { get; set; }

    /// <summary>
    /// Pure black. It does not change with the theme.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color ColorBlack { get; set; } = Color.FromRgb(0, 0, 0);

    /// <summary>
    /// Background of selected text, read by text input controls and selectable text.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color SelectionBackground { get; set; }

    /// <summary>
    /// Foreground of selected text, read by text input controls and selectable text.
    /// </summary>
    [DesignTokenKind(DesignTokenKind.Map)]
    public partial Color SelectionForeground { get; set; }
}