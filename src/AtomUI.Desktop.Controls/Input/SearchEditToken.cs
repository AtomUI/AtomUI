using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of the search box: the font size of the text in a small search box.
/// </summary>
[ControlDesignToken]
public sealed partial class SearchEditToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of the text in a small search box.
    /// By default it equals the regular font size, <see cref="DesignToken.FontSize" />, as in Ant Design.
    /// </summary>
    public partial double InputFontSizeSM { get; set; }
}
