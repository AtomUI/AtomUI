using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens of <see cref="Pagination" />, the pager with numbered page items. It adds the colors, the font
/// weight and the padding of a page item to the tokens it shares with the simple pager.
/// </summary>
[ControlDesignToken]
public sealed partial class PaginationToken : AbstractPaginationToken
{

    /// <summary>
    /// Background color of a page item when it is neither hovered nor the current page.
    /// </summary>
    public partial Color ItemBg { get; set; }
    
        
    /// <summary>
    /// Background color of the page item of the current page.
    /// </summary>
    public partial Color ItemActiveBg { get; set; }
    
    
    /// <summary>
    /// Background color of the previous and next buttons of a bordered pager. AtomUI has no bordered pager, so no
    /// element draws this value.
    /// </summary>
    [ReservedToken("antd Pagination itemLinkBg: the background of the bordered pagination's previous / next buttons. " +
                   "AtomUI Pagination has no bordered variant, so no element draws it.")]
    public partial Color ItemLinkBg { get; set; }
    
    /// <summary>
    /// Background color of the page item of the current page while the item is disabled.
    /// </summary>
    public partial Color ItemActiveBgDisabled { get; set; }
    
    
    
    /// <summary>
    /// Horizontal offset of the focus shadow around the page number input of the simple pager. That input is a
    /// LineEdit drawn with the focus shadow of the input family, so no element draws this value.
    /// </summary>
    [ReservedToken("antd Pagination inputOutlineOffset (internal): the x offset of the simple pager input's focus " +
                   "box-shadow. The simple pager input here is a LineEdit drawn with the Input family's focus shadow.")]
    public partial Thickness InputOutlineOffset { get; set; }
    
    
    
    
    
    
    /// <summary>
    /// Text color of the page item of the current page.
    /// </summary>
    public partial Color ItemActiveColor { get; set; }

    /// <summary>
    /// Border color of the page item of the current page.
    /// </summary>
    public partial Color ItemActiveBorderColor { get; set; }

    /// <summary>
    /// Text color of the page item of the current page while the pointer is over it.
    /// </summary>
    public partial Color ItemActiveHoverColor { get; set; }

    /// <summary>
    /// Border color of the page item of the current page while the pointer is over it.
    /// </summary>
    public partial Color ItemActiveHoverBorderColor { get; set; }

    /// <summary>
    /// Font weight of the page number in the page item of the current page.
    /// </summary>
    public partial FontWeight ItemActiveFontWeight { get; set; }
    
    /// <summary>
    /// Inline padding between the page number and the edges of its pagination item.
    /// </summary>
    public partial Thickness PaginationItemPaddingInline { get; set; }
    
}
