using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Own design tokens of the <see cref="Card" />: header, body, actions row, grid items and shadows.
/// </summary>
[ControlDesignToken]
public sealed partial class CardToken : AbstractControlDesignToken
{
    
    /// <summary>
    /// Background color of the card header. A card in inner mode ignores it and uses the global
    /// <c>ColorFillAlter</c> color.
    /// </summary>
    public partial Color HeaderBg { get; set; }
    
    /// <summary>
    /// Font size of the card header text in the large size.
    /// </summary>
    public partial double HeaderFontSizeLG { get; set; }
    
    /// <summary>
    /// Font size of the card header text in the middle size.
    /// </summary>
    public partial double HeaderFontSize { get; set; }
    
    /// <summary>
    /// Font size of the card header text in the small size.
    /// </summary>
    public partial double HeaderFontSizeSM { get; set; }
    
    /// <summary>
    /// Minimum height of the card header in the large size.
    /// </summary>
    public partial double HeaderHeightLG { get; set; }
    
    /// <summary>
    /// Minimum height of the card header in the middle size.
    /// </summary>
    public partial double HeaderHeight { get; set; }
    
    /// <summary>
    /// Minimum height of the card header in the small size.
    /// </summary>
    public partial double HeaderHeightSM { get; set; }
    
    /// <summary>
    /// Padding of the card body in the large size; grid items and the tab content area take the same value.
    /// </summary>
    public partial Thickness BodyPaddingLG { get; set; }
    
    /// <summary>
    /// Padding of the card body in the middle size; grid items and the tab content area take the same value.
    /// </summary>
    public partial Thickness BodyPadding { get; set; }
    
    /// <summary>
    /// Padding of the card body in the small size; grid items and the tab content area take the same value.
    /// </summary>
    public partial Thickness BodyPaddingSM { get; set; }
    
    /// <summary>
    /// Padding of the card header in the large size.
    /// </summary>
    public partial Thickness HeaderPaddingLG { get; set; }
    
    /// <summary>
    /// Padding of the card header in the middle size.
    /// </summary>
    public partial Thickness HeaderPadding { get; set; }
    
    /// <summary>
    /// Padding of the card header in the small size.
    /// </summary>
    public partial Thickness HeaderPaddingSM { get; set; }
    
    /// <summary>
    /// Background color of the actions row at the bottom of the card.
    /// </summary>
    public partial Color ActionsBg { get; set; }
    
    /// <summary>
    /// Gap between the items of the actions row. Reserved and without effect: each action fills the whole height of
    /// the row, so no template reads this value.
    /// </summary>
    [ReservedToken("antd Card actionsLiMargin (the vertical margin of each action). The AtomUI action row is a " +
                   "ControlHeightLG-high row whose actions stretch over the whole cell, so there is no item margin to set.")]
    public partial double ActionsSpacing { get; set; }
    
    /// <summary>
    /// Offset that pulls the tab bar up onto the bottom border of the card header. Reserved and without effect: the
    /// tab bar of a card with tabs is rendered in the body below the header, so no template reads this value.
    /// </summary>
    [ReservedToken("antd Card tabsMarginBottom (pulls the tabs inside the card head onto the head border). CardTabsContent " +
                   "renders its tab bar in the body below the header, so there is no head to overlap.")]
    public partial Thickness TabsMarginBottom { get; set; }
    
    /// <summary>
    /// Text color of the extra area in the card header.
    /// </summary>
    public partial Color ExtraColor { get; set; }
    
    /// <summary>
    /// Shadow of a hoverable card while the pointer is over it.
    /// </summary>
    public partial BoxShadows CardShadows { get; set; }

    /// <summary>
    /// Resting shadow of an outlined card while the pointer is not over it; an AtomUI extension for presets, antd has
    /// none. A borderless card ignores it and rests on the global <c>BoxShadowsTertiary</c>. The default is
    /// transparent layers matching <see cref="CardShadows" />, so the hover transition keeps its layer count.
    /// </summary>
    public partial BoxShadows RestingShadows { get; set; }
    
    /// <summary>
    /// Padding of a card header that also holds the tab bar. Reserved and without effect: the tab bar is rendered in
    /// the body, so the header keeps the height and padding of its size and no template reads this value.
    /// </summary>
    [ReservedToken("antd Card cardHeadPadding (the title padding of a head that also holds the tabs). CardTabsContent " +
                   "renders its tab bar in the body, so the header keeps its normal height and padding.")]
    public partial Thickness CardHeadPadding { get; set; }
    
    /// <summary>
    /// Base padding of a card grid item. Reserved and without effect: a grid item follows the card size and takes
    /// <see cref="BodyPaddingLG" />, <see cref="BodyPadding" /> or <see cref="BodyPaddingSM" />, and no template reads
    /// this value.
    /// </summary>
    [ReservedToken("antd Card cardPaddingBase (the padding of a grid item). CardGridItem follows the card size with " +
                   "BodyPaddingLG / BodyPadding / BodyPaddingSM instead.")]
    public partial Thickness CardPaddingBase { get; set; }
    
    /// <summary>
    /// Width and height of the icons of the action buttons in the actions row.
    /// </summary>
    public partial double CardActionsIconSize { get; set; }
    
    /// <summary>
    /// Shadow of a card grid item at rest. The separators between grid items are painted by the grid container and
    /// are not part of this shadow.
    /// </summary>
    public partial BoxShadows CardGridItemShadows { get; set; }

    /// <summary>
    /// Shadow of a card grid item while the pointer is over it.
    /// </summary>
    public partial BoxShadows CardGridHoverShadows { get; set; }
    
}
