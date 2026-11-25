using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class NavMenuToken : AbstractControlDesignToken
{

    /// <summary>Padding inside the submenu popup frame around its child items.</summary>
    public partial Thickness MenuPopupContentPadding { get; set; }

    /// <summary>Minimum width of a submenu popup.</summary>
    public partial double MenuPopupMinWidth { get; set; }

    /// <summary>Maximum width of a submenu popup frame.</summary>
    public partial double MenuPopupMaxWidth { get; set; }

    /// <summary>Maximum height of a submenu popup frame.</summary>
    public partial double MenuPopupMaxHeight { get; set; }
    
    /// <summary>Gap between a top-level item and its submenu popup.</summary>
    public partial double TopLevelItemPopupMarginToAnchor { get; set; }

    /// <summary>Foreground of a group title in the light menu style.</summary>
    public partial Color GroupTitleColor { get; set; }
    
    /// <summary>Height of the row containing a group title.</summary>
    public partial double GroupTitleLineHeight { get; set; }
    
    /// <summary>Font size of the group title.</summary>
    public partial double GroupTitleFontSize { get; set; }
    
    /// <summary>Corner radius of an item header outside the horizontal top level.</summary>
    public partial CornerRadius ItemBorderRadius { get; set; }
    
    /// <summary>Corner radius of the border framing a submenu's child items.</summary>
    public partial CornerRadius SubMenuItemBorderRadius { get; set; }
    
    /// <summary>
    /// Foreground of an enabled item header at rest in the light menu style. The transparent header shows
    /// <see cref="ItemBg"/>, <see cref="SubMenuItemBg"/> or <see cref="MenuPopupBg"/> behind it.
    /// </summary>
    public partial Color ItemColor { get; set; }
    
    /// <summary>
    /// Foreground of an unselected item under the pointer in the light menu style. Pairs with <see cref="ItemHoverBg"/>
    /// or <see cref="HorizontalItemHoverBg"/>.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }
    
    /// <summary>
    /// Reserved foreground role for a hovered horizontal item. The current template uses <see cref="ItemHoverColor"/>
    /// for that state.
    /// </summary>
    [ReservedToken("antd Menu horizontalItemHoverColor. antd 6.7 declares it but its menu style never reads it: " +
                   "horizontal hover text uses itemHoverColor (ItemHoverColor here), so wiring it would diverge from " +
                   "antd's rendering.")]
    public partial Color HorizontalItemHoverColor { get; set; }
    
    /// <summary>
    /// Foreground of a selected item and its ancestors in the light menu style. The selected row pairs with
    /// <see cref="ItemSelectedBg"/>.
    /// </summary>
    public partial Color ItemSelectedColor { get; set; }
    
    /// <summary>
    /// Foreground of a selected horizontal top-level item or its ancestor in the light menu style. Pairs with
    /// <see cref="HorizontalItemSelectedBg"/> and also paints the active bar.
    /// </summary>
    public partial Color HorizontalItemSelectedColor { get; set; }
    
    /// <summary>Foreground of a disabled item header in the light menu style.</summary>
    public partial Color ItemDisabledColor { get; set; }
    
    /// <summary>Foreground of a danger item header at rest in the light menu style.</summary>
    public partial Color DangerItemColor { get; set; }
    
    /// <summary>
    /// Foreground of an unselected danger item under the pointer in the light menu style. Pairs with the hover
    /// background of that layout.
    /// </summary>
    public partial Color DangerItemHoverColor { get; set; }
    
    /// <summary>
    /// Foreground of a selected danger item. Pairs with <see cref="DangerItemSelectedBg"/>.
    /// </summary>
    public partial Color DangerItemSelectedColor { get; set; }
    
    /// <summary>Fill of an unselected danger item while the pointer is held down.</summary>
    public partial Color DangerItemActiveBg { get; set; }
    
    /// <summary>
    /// Fill of a selected danger item. Pairs with <see cref="DangerItemSelectedColor"/>.
    /// </summary>
    public partial Color DangerItemSelectedBg { get; set; }
    
    /// <summary>
    /// Fill of the navigation menu's main surface in the light menu style, behind transparent headers and group titles.
    /// </summary>
    public partial Color ItemBg { get; set; }
    
    /// <summary>
    /// Fill of an item header under the pointer in the light menu style. Pairs with <see cref="ItemHoverColor"/> on
    /// unselected standard items.
    /// </summary>
    public partial Color ItemHoverBg { get; set; }
    
    /// <summary>Fill behind the child items of an expanded submenu in the light menu style.</summary>
    public partial Color SubMenuItemBg { get; set; }
    
    /// <summary>
    /// Fill of an unselected item header while it is the active item in keyboard navigation in the light menu style.
    /// </summary>
    public partial Color ItemActiveBg { get; set; }
    
    /// <summary>
    /// Fill of a selected item, also used while the pointer is held on an unselected standard item. The selected state
    /// pairs with <see cref="ItemSelectedColor"/>.
    /// </summary>
    public partial Color ItemSelectedBg { get; set; }

    /// <summary>Size of the icon preceding the item label when its header is expanded.</summary>
    public partial double ItemIconSize { get; set; }
    
    /// <summary>
    /// Fill of a horizontal top-level header on the selected path in the light menu style. Pairs with
    /// <see cref="HorizontalItemSelectedColor"/>.
    /// </summary>
    public partial Color HorizontalItemSelectedBg { get; set; }

    /// <summary>Horizontal scale of the active indicator bar relative to its available width.</summary>
    public partial double ActiveBarScaleX { get; set; }
    
    /// <summary>Height of the active indicator bar beneath a horizontal item.</summary>
    public partial double ActiveBarHeight { get; set; }
    
    /// <summary>Margin around item headers and group title rows.</summary>
    public partial Thickness ItemContentMargin { get; set; }
    
    /// <summary>
    /// Padding inside an item header. The horizontal top-level header clears this padding.
    /// </summary>
    public partial Thickness ItemContentPadding { get; set; }
    
    /// <summary>Gap between adjacent items in a vertically stacked menu panel.</summary>
    public partial double VerticalItemsPanelSpacing { get; set; }
    
    /// <summary>Margin around the panel containing a submenu's expanded child items.</summary>
    public partial Thickness VerticalChildItemsMargin { get; set; }
    
    /// <summary>Padding around items in the main vertical or inline menu panel.</summary>
    public partial Thickness VerticalMenuContentPadding { get; set; }
    
    /// <summary>Margin around the label and keyboard shortcut presenters in an item header.</summary>
    public partial Thickness ItemMargin { get; set; }
    
    /// <summary>Margin around the frame of a top-level horizontal item.</summary>
    public partial Thickness HorizontalItemMargin { get; set; }
    
    /// <summary>
    /// Fill of a horizontal top-level item under the pointer or with its submenu open in the light menu style. Hovered
    /// items pair with <see cref="ItemHoverColor"/>.
    /// </summary>
    public partial Color HorizontalItemHoverBg { get; set; }
    
    /// <summary>Corner radius of the frame of a top-level horizontal item.</summary>
    public partial CornerRadius HorizontalItemBorderRadius { get; set; }
    
    /// <summary>
    /// Height of an item header in vertical and inline menus or inside a submenu popup.
    /// </summary>
    public partial double ItemHeight { get; set; }
    
    /// <summary>Horizontal indent added for each nesting level in inline mode.</summary>
    public partial double InlineItemIndentUnit { get; set; }
    
    /// <summary>
    /// Reserved compatibility width for a collapsed menu. The inline menu uses <see cref="InlineCollapsedWidth"/>.
    /// </summary>
    [ReservedToken("antd Menu collapsedWidth (controlHeightLG * 2). The inline collapsed width here is " +
                   "InlineCollapsedWidth (48); CollapsedWidth is kept as the antd-named compatibility token and no " +
                   "element reads it.")]
    public partial double CollapsedWidth { get; set; }

    /// <summary>Width of the menu surface when inline mode is collapsed.</summary>
    public partial double InlineCollapsedWidth { get; set; }
    
    /// <summary>Fill of the submenu popup surface in the light menu style.</summary>
    public partial Color MenuPopupBg { get; set; }

    /// <summary>
    /// The shadows of the horizontal and vertical menu's submenu popups. Defaults to <c>BoxShadowsSecondary</c>, the
    /// value the shared Popup Token gives other overlays, so a theme can give menus their own frame.
    /// </summary>
    public partial BoxShadows MenuPopupShadows { get; set; }
    
    /// <summary>Line height of the label in a top-level horizontal item.</summary>
    public partial double HorizontalLineHeight { get; set; }
    
    /// <summary>Margin between the icon and label in a top-level horizontal item.</summary>
    public partial Thickness IconMargin { get; set; }
    
    /// <summary>Width and height of an item icon in the collapsed menu.</summary>
    public partial double CollapsedIconSize { get; set; }
    
    // Dark
    /// <summary>Fill of the main navigation menu surface in the dark menu style.</summary>
    public partial Color DarkMenuBg { get; set; }
    /// <summary>Fill of the submenu popup surface in the dark menu style.</summary>
    public partial Color DarkMenuPopupBg { get; set; }
    
    /// <summary>
    /// Foreground of an enabled item header at rest in the dark menu style. Pairs with <see cref="DarkItemBg"/>.
    /// </summary>
    public partial Color DarkItemColor { get; set; }
    
    /// <summary>
    /// Foreground of a danger item header at rest in the dark menu style. Pairs with <see cref="DarkItemBg"/>.
    /// </summary>
    public partial Color DarkDangerItemColor { get; set; }
    
    /// <summary>
    /// Fill of an item header at rest in the dark menu style. Pairs with <see cref="DarkItemColor"/>,
    /// <see cref="DarkDangerItemColor"/> or <see cref="DarkItemDisabledColor"/>.
    /// </summary>
    public partial Color DarkItemBg { get; set; }
    
    /// <summary>Fill behind the child items of an expanded submenu in the dark menu style.</summary>
    public partial Color DarkSubMenuItemBg { get; set; }
    
    /// <summary>
    /// Foreground of a selected item and its ancestors in the dark menu style. The selected row pairs with
    /// <see cref="DarkItemSelectedBg"/>.
    /// </summary>
    public partial Color DarkItemSelectedColor { get; set; }
    
    /// <summary>
    /// Fill of a selected item, also used while the pointer is held on an unselected standard item in the dark menu
    /// style. The selected state pairs with <see cref="DarkItemSelectedColor"/>.
    /// </summary>
    public partial Color DarkItemSelectedBg { get; set; }
    
    /// <summary>
    /// Fill of an item under the pointer or active in keyboard navigation in the dark menu style. Hovered unselected
    /// standard items pair with <see cref="DarkItemHoverColor"/>.
    /// </summary>
    public partial Color DarkItemHoverBg { get; set; }
    
    /// <summary>Foreground of a group title in the dark menu style.</summary>
    public partial Color DarkGroupTitleColor { get; set; }
    
    /// <summary>
    /// Foreground of an unselected item under the pointer in the dark menu style. Pairs with
    /// <see cref="DarkItemHoverBg"/>.
    /// </summary>
    public partial Color DarkItemHoverColor { get; set; }
    
    /// <summary>
    /// Foreground of a disabled item header in the dark menu style. Pairs with <see cref="DarkItemBg"/>.
    /// </summary>
    public partial Color DarkItemDisabledColor { get; set; }
    
    /// <summary>
    /// Fill of a selected danger item in the dark menu style. Pairs with <see cref="DarkDangerItemSelectedColor"/>.
    /// </summary>
    public partial Color DarkDangerItemSelectedBg { get; set; }
    
    /// <summary>
    /// Foreground of an unselected danger item under the pointer in the dark menu style. Pairs with the hover
    /// background of that layout.
    /// </summary>
    public partial Color DarkDangerItemHoverColor { get; set; }
    
    /// <summary>
    /// Foreground of a selected danger item in the dark menu style. Pairs with <see cref="DarkDangerItemSelectedBg"/>.
    /// </summary>
    public partial Color DarkDangerItemSelectedColor { get; set; }
    
    /// <summary>
    /// Fill of an unselected danger item while the pointer is held down in the dark menu style.
    /// </summary>
    public partial Color DarkDangerItemActiveBg { get; set; }

    #region 菜单相关 Token

    /// <summary>Height of the navigation menu in horizontal mode.</summary>
    public partial double MenuHorizontalHeight { get; set; }
    
    /// <summary>Width and height of the arrow indicating an item's submenu.</summary>
    public partial double MenuArrowSize { get; set; }

    /// <summary>
    /// Reserved background role for a separate submenu layer. The current template paints one popup surface with
    /// <see cref="MenuPopupBg"/>.
    /// </summary>
    [ReservedToken("antd Menu menuSubMenuBg (colorBgElevated). In antd the submenu popup's later popupBg rule paints the " +
                   "same element, so the popup surface is popupBg; here that surface is MenuPopupBg and there is no " +
                   "second submenu layer to paint.")]
    public partial Color MenuSubMenuBg { get; set; }
    
    #endregion
}
