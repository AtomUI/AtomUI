using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class MenuToken : AbstractMenuPopupToken
{




















    /// <summary>
    /// Foreground of the label and icon of a menu bar item at rest. Pairs with <see cref="TopLevelItemBg"/>.
    /// </summary>
    public partial Color TopLevelItemColor { get; set; }

    /// <summary>
    /// Foreground of the label and icon of a menu bar item while its submenu is open. Pairs with
    /// <see cref="TopLevelItemSelectedBg"/>.
    /// </summary>
    public partial Color TopLevelItemSelectedColor { get; set; }

    /// <summary>
    /// Foreground of the label and icon of a menu bar item under the pointer. Pairs with
    /// <see cref="TopLevelItemHoverBg"/>.
    /// </summary>
    public partial Color TopLevelItemHoverColor { get; set; }

    /// <summary>Fill of a menu bar item at rest. Pairs with <see cref="TopLevelItemColor"/>.</summary>
    public partial Color TopLevelItemBg { get; set; }

    /// <summary>
    /// Fill of a menu bar item while its submenu is open. Pairs with <see cref="TopLevelItemSelectedColor"/>.
    /// </summary>
    public partial Color TopLevelItemSelectedBg { get; set; }

    /// <summary>
    /// Fill of a menu bar item under the pointer. Pairs with <see cref="TopLevelItemHoverColor"/>.
    /// </summary>
    public partial Color TopLevelItemHoverBg { get; set; }

    /// <summary>Corner radius of a small menu bar item's frame.</summary>
    public partial CornerRadius TopLevelItemBorderRadiusSM { get; set; }

    /// <summary>Corner radius of a standard or custom-sized menu bar item's frame.</summary>
    public partial CornerRadius TopLevelItemBorderRadius { get; set; }

    /// <summary>Corner radius of a large menu bar item's frame.</summary>
    public partial CornerRadius TopLevelItemBorderRadiusLG { get; set; }

    /// <summary>Padding inside a small menu bar item's frame.</summary>
    public partial Thickness TopLevelItemPaddingSM { get; set; }

    /// <summary>Padding inside a standard or custom-sized menu bar item's frame.</summary>
    public partial Thickness TopLevelItemPadding { get; set; }

    /// <summary>Padding inside a large menu bar item's frame.</summary>
    public partial Thickness TopLevelItemPaddingLG { get; set; }

    /// <summary>Font size of the label in a small menu bar item.</summary>
    public partial double TopLevelItemFontSizeSM { get; set; }

    /// <summary>Font size of the label in a standard or custom-sized menu bar item.</summary>
    public partial double TopLevelItemFontSize { get; set; }

    /// <summary>Font size of the label in a large menu bar item.</summary>
    public partial double TopLevelItemFontSizeLG { get; set; }

    /// <summary>Line height of the label in a standard or custom-sized menu bar item.</summary>
    public partial double TopLevelItemLineHeight { get; set; }

    /// <summary>Line height of the label in a large menu bar item.</summary>
    public partial double TopLevelItemLineHeightLG { get; set; }

    /// <summary>Line height of the label in a small menu bar item.</summary>
    public partial double TopLevelItemLineHeightSM { get; set; }

    /// <summary>
    /// Gap between a menu bar item and its submenu when the submenu opens in the overlay layer.
    /// </summary>
    public partial double TopLevelItemPopupMarginToAnchor { get; set; }

}
