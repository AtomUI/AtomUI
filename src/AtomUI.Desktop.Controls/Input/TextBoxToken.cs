using AtomUI.Theme.DesignTokens;
using Avalonia;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the text box: the padding inside the box for each size.
/// </summary>
[ControlDesignToken]
public sealed partial class TextBoxToken : AbstractControlDesignToken
{

    /// <summary>
    /// Padding inside the text box at the default size.
    /// </summary>
    public partial Thickness ContentPadding { get; set; }

    /// <summary>
    /// Padding inside a small text box.
    /// </summary>
    public partial Thickness ContentPaddingSM { get; set; }

    /// <summary>
    /// Padding inside a large text box.
    /// </summary>
    public partial Thickness ContentPaddingLG { get; set; }

}
