using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens shared by <see cref="MenuToken" />, <see cref="ContextMenuToken" /> and
/// <see cref="MenuFlyoutPresenterToken" />: the frame of a menu popup (padding, width limits, background, shadow)
/// and the items, item states and group titles shown inside it. Each design language fills this layer in its own
/// menu popup token calculator.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractMenuPopupToken : AbstractControlDesignToken
{
    /// <summary>
    /// Padding between the popup frame and its menu items.
    /// </summary>
    public partial Thickness MenuPopupContentPadding { get; set; }
    /// <summary>
    /// Minimum width of a menu popup.
    /// </summary>
    public partial double MenuPopupMinWidth { get; set; }
    /// <summary>
    /// Maximum width of a menu popup.
    /// </summary>
    public partial double MenuPopupMaxWidth { get; set; }
    /// <summary>
    /// Fill of the popup frame behind its menu items.
    /// </summary>
    public partial Color MenuPopupBgColor { get; set; }
    /// <summary>
    /// Foreground of the label and icon in an enabled popup item at rest. Pairs with <see cref="ItemBg" />.
    /// </summary>
    public partial Color ItemColor { get; set; }
    /// <summary>
    /// Foreground of the keyboard shortcut displayed at the end of a popup item. It appears over <see cref="ItemBg" />
    /// or <see cref="ItemHoverBg" />.
    /// </summary>
    public partial Color KeyGestureColor { get; set; }
    /// <summary>
    /// Margin around the label and keyboard shortcut presenters in a popup item.
    /// </summary>
    public partial Thickness ItemMargin { get; set; }
    /// <summary>
    /// Foreground of the label and icon in a popup item under the pointer. Pairs with <see cref="ItemHoverBg" />.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }
    /// <summary>
    /// Foreground of the label and icon in a disabled menu item. Popup items use <see cref="ItemBg" /> behind it.
    /// </summary>
    public partial Color ItemDisabledColor { get; set; }
    /// <summary>
    /// Foreground of the label and icon in a danger popup item at rest. Pairs with <see cref="ItemBg" />.
    /// </summary>
    public partial Color DangerItemColor { get; set; }
    /// <summary>
    /// Foreground of the label and icon in a danger popup item under the pointer. Pairs with <see cref="ItemHoverBg"
    /// />.
    /// </summary>
    public partial Color DangerItemHoverColor { get; set; }
    /// <summary>
    /// Fill of a popup item at rest. Pairs with <see cref="ItemColor" />, <see cref="DangerItemColor" /> or
    /// <see cref="ItemDisabledColor" />.
    /// </summary>
    public partial Color ItemBg { get; set; }
    /// <summary>
    /// Fill of a popup item under the pointer. Pairs with <see cref="ItemHoverColor" /> or
    /// <see cref="DangerItemHoverColor" />.
    /// </summary>
    public partial Color ItemHoverBg { get; set; }
    /// <summary>
    /// Height of an item in a menu popup.
    /// </summary>
    public partial double ItemHeight { get; set; }
    /// <summary>
    /// Width and height of the icon preceding the popup item label.
    /// </summary>
    public partial double ItemIconSize { get; set; }
    /// <summary>
    /// Gap between the end of the popup item's icon and the following label.
    /// </summary>
    public partial double ItemIconMarginInlineEnd { get; set; }
    /// <summary>
    /// Padding inside the title presenter of a popup item group.
    /// </summary>
    public partial Thickness GroupTitlePadding { get; set; }
    /// <summary>
    /// Corner radius of the popup item frame.
    /// </summary>
    public partial CornerRadius ItemBorderRadius { get; set; }
    /// <summary>
    /// Padding inside the popup item frame.
    /// </summary>
    public partial Thickness ItemPaddingInline { get; set; }
    /// <summary>
    /// Minimum height reserved for a separator item inside the popup.
    /// </summary>
    public partial double SeparatorItemHeight { get; set; }

    /// <summary>
    /// The shadows of the menu's popups (the context menu, submenus and menubar drop-downs). Defaults to
    /// <c>BoxShadowsSecondary</c>, the value the shared Popup Token gives other overlays, so a theme can give menus
    /// their own frame (for example an outline ring in a menu-specific border color).
    /// </summary>
    public partial BoxShadows MenuPopupShadows { get; set; }
}
