using AtomUI.Theme.DesignTokens;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens for <see cref="NumericUpDown" />, extending <see cref="AbstractButtonSpinnerToken" /> with the
/// font size of the small size.
/// </summary>
[ControlDesignToken]
public sealed partial class NumericUpDownToken : AbstractButtonSpinnerToken
{
    /// <summary>
    /// Font size of the input text in the small size. Defaults to <see cref="DesignToken.FontSize" />, the same
    /// as the middle size, as in antd.
    /// </summary>
    public partial double InputFontSizeSM { get; set; }
}
