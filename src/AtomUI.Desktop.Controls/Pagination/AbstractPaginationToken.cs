using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

/// <summary>
/// Design tokens shared by the pagers: the size of a page item, the gaps between the pager's elements,
/// the width of the quick jumper input, and the colors and corner radius of a page item. Each design
/// language fills the values for its themes; <c>PaginationToken</c> and <c>SimplePaginationToken</c>
/// derive from it.
/// </summary>
[ControlDesignToken]
public abstract partial class AbstractPaginationToken : AbstractControlDesignToken
{
    /// <summary>
    /// Height and minimum width of a pagination item at the default size.
    /// </summary>
    public partial double ItemSize { get; set; }
    /// <summary>
    /// Height and minimum width of a small pagination item.
    /// </summary>
    public partial double ItemSizeSM { get; set; }
    /// <summary>
    /// Foreground color of disabled pagination items.
    /// </summary>
    public partial Color ItemActiveColorDisabled { get; set; }
    /// <summary>
    /// Gap between pagination elements at the default size.
    /// </summary>
    public partial double PaginationLayoutSpacing { get; set; }
    /// <summary>
    /// Gap between pagination elements at the small size.
    /// </summary>
    public partial double PaginationLayoutMiniSpacing { get; set; }
    /// <summary>
    /// Width of the go-to-page input at the default size.
    /// </summary>
    public partial double PaginationQuickJumperInputWidth { get; set; }
    /// <summary>
    /// Width of the go-to-page input at the small size.
    /// </summary>
    public partial double PaginationMiniQuickJumperInputWidth { get; set; }

    /// <summary>
    /// Text color of a page item at rest, the previous and next buttons included.
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// Text color of a page item while it is hovered.
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// Background color of a page item while it is hovered.
    /// </summary>
    public partial Color ItemHoverBg { get; set; }

    /// <summary>
    /// Corner radius of a page item.
    /// </summary>
    public partial CornerRadius ItemBorderRadius { get; set; }
}
