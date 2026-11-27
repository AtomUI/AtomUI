using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the line editor: the font size of the entered text per size, and the switches that give
/// the input frame a transparent or bottom-only border.
/// </summary>
[ControlDesignToken]
public sealed partial class LineEditToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of text entered in a regular line editor.
    /// </summary>
    public partial double InputFontSize { get; set; }

    /// <summary>
    /// Font size of text entered in a large line editor.
    /// </summary>
    public partial double InputFontSizeLG { get; set; }

    /// <summary>
    /// Font size of text entered in a small line editor.
    /// </summary>
    public partial double InputFontSizeSM { get; set; }

    /// <summary>
    /// Whether the input frame keeps a transparent border at rest and on hover.
    /// </summary>
    public partial bool TransparentOutlineBorder { get; set; }

    /// <summary>
    /// Whether the input frame draws only its bottom border.
    /// </summary>
    public partial bool BottomOutlineBorder { get; set; }

}
