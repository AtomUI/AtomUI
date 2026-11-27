using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Tab design tokens of card-type tabs, which draw each tab as a filled card.
/// They cover the card background and the padding of a tab, the gaps between tabs, and the size and
/// margin of the add-tab button.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractCardTabToken : AbstractTabToken
{
    /// <summary>
    /// Background color of an unselected card tab.
    /// </summary>
    public partial Color CardBg { get; set; }
    /// <summary>
    /// Size used to lay out the add-tab button in a card tab strip.
    /// </summary>
    public partial double CardSize { get; set; }
    /// <summary>
    /// Padding inside a default-size card tab in a horizontal strip.
    /// </summary>
    public partial Thickness CardPadding { get; set; }
    /// <summary>
    /// Padding inside a small card tab in a horizontal strip.
    /// </summary>
    public partial Thickness CardPaddingSM { get; set; }
    /// <summary>
    /// Padding inside a large card tab in a horizontal strip.
    /// </summary>
    public partial Thickness CardPaddingLG { get; set; }
    /// <summary>
    /// Padding inside a card tab in a vertical strip.
    /// </summary>
    public partial Thickness VerticalItemPadding { get; set; }
    /// <summary>
    /// Gap between adjacent card tabs.
    /// </summary>
    public partial double CardGutter { get; set; }
    /// <summary>
    /// Margin around the add-tab button in a horizontal strip.
    /// </summary>
    public partial Thickness AddTabButtonMarginHorizontal { get; set; }
    /// <summary>
    /// Margin around the add-tab button in a vertical strip.
    /// </summary>
    public partial Thickness AddTabButtonMarginVertical { get; set; }
}
