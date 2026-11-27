using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Base layer of the tab design tokens, used by line-type and card-type tabs alike.
/// It covers the title font, the tab item colors and margins, the overflow-menu button and the shadows
/// that mark hidden tabs at the edge of the strip. <see cref="AbstractLineTabToken" /> and
/// <see cref="AbstractCardTabToken" /> add the values of their tab type.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractTabToken : AbstractControlDesignToken
{
    /// <summary>
    /// Font size of tab titles at the standard size.
    /// </summary>
    public partial double TitleFontSize { get; set; }
    /// <summary>
    /// Font size of tab titles at the large size.
    /// </summary>
    public partial double TitleFontSizeLG { get; set; }
    /// <summary>
    /// Font size of tab titles at the small size.
    /// </summary>
    public partial double TitleFontSizeSM { get; set; }

    /// <summary>
    /// The font weight of the tab titles.
    /// </summary>
    public partial FontWeight TitleFontWeight { get; set; }
    /// <summary>
    /// Margin around a tab item in a horizontally arranged tab strip.
    /// </summary>
    public partial Thickness HorizontalItemMargin { get; set; }
    /// <summary>
    /// Resting foreground color of a tab item and of the add-tab button in card-type tabs.
    /// </summary>
    public partial Color ItemColor { get; set; }
    /// <summary>
    /// Foreground color of a tab item and of the add-tab button in card-type tabs while the pointer is over them.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }
    /// <summary>
    /// Foreground color of a selected tab item.
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }
    /// <summary>
    /// Margin around the icon next to a tab title.
    /// </summary>
    public partial Thickness ItemIconMargin { get; set; }
    /// <summary>
    /// Padding inside the overflow-menu button in a horizontal tab strip.
    /// </summary>
    public partial Thickness MenuIndicatorPaddingHorizontal { get; set; }
    /// <summary>
    /// Padding inside the overflow-menu button in a vertical tab strip.
    /// </summary>
    public partial Thickness MenuIndicatorPaddingVertical { get; set; }
    /// <summary>
    /// Shadow that marks hidden tabs beyond the left edge of the tab strip.
    /// </summary>
    public partial BoxShadows BoxShadowTabsOverflowLeft { get; set; }
    /// <summary>
    /// Shadow that marks hidden tabs beyond the right edge of the tab strip.
    /// </summary>
    public partial BoxShadows BoxShadowTabsOverflowRight { get; set; }
    /// <summary>
    /// Shadow that marks hidden tabs beyond the top edge of the tab strip.
    /// </summary>
    public partial BoxShadows BoxShadowTabsOverflowTop { get; set; }
    /// <summary>
    /// Shadow that marks hidden tabs beyond the bottom edge of the tab strip.
    /// </summary>
    public partial BoxShadows BoxShadowTabsOverflowBottom { get; set; }
    /// <summary>
    /// Margin around the tab's close button.
    /// </summary>
    public partial Thickness CloseIconMargin { get; set; }
}
