using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Tab design tokens of line-type tabs, which mark the selected tab with a colored line.
/// They cover the color and thickness of that line, the gaps between tabs in a horizontal and in a
/// vertical strip, and the padding of a tab in a horizontal strip.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractLineTabToken : AbstractTabToken
{
    /// <summary>
    /// Color of the line marking the selected tab.
    /// </summary>
    public partial Color InkBarColor { get; set; }
    /// <summary>
    /// Thickness of the line marking the selected tab.
    /// </summary>
    public partial double InkBarThickness { get; set; }
    /// <summary>
    /// Gap between tabs in a horizontal strip.
    /// </summary>
    public partial double HorizontalItemGutter { get; set; }
    /// <summary>
    /// Padding inside a default-size tab in a horizontal strip.
    /// </summary>
    public partial Thickness HorizontalItemPadding { get; set; }
    /// <summary>
    /// Padding inside a large tab in a horizontal strip.
    /// </summary>
    public partial Thickness HorizontalItemPaddingLG { get; set; }
    /// <summary>
    /// Padding inside a small tab in a horizontal strip.
    /// </summary>
    public partial Thickness HorizontalItemPaddingSM { get; set; }
    /// <summary>
    /// Gap between tabs in a vertical strip.
    /// </summary>
    public partial double VerticalItemGutter { get; set; }
}
