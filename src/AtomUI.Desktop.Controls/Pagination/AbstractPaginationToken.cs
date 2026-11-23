using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public abstract partial class AbstractPaginationToken : AbstractControlDesignToken
{
    /// <summary>Height and minimum width of a pagination item at the default size.</summary>
    public partial double ItemSize { get; set; }
    /// <summary>Height and minimum width of a small pagination item.</summary>
    public partial double ItemSizeSM { get; set; }
    /// <summary>Foreground color of disabled pagination items.</summary>
    public partial Color ItemActiveColorDisabled { get; set; }
    /// <summary>Gap between pagination elements at the default size.</summary>
    public partial double PaginationLayoutSpacing { get; set; }
    /// <summary>Gap between pagination elements at the small size.</summary>
    public partial double PaginationLayoutMiniSpacing { get; set; }
    /// <summary>Width of the go-to-page input at the default size.</summary>
    public partial double PaginationQuickJumperInputWidth { get; set; }
    /// <summary>Width of the go-to-page input at the small size.</summary>
    public partial double PaginationMiniQuickJumperInputWidth { get; set; }

    /// <summary>
    /// 页码选项文本颜色
    /// Text color of a Pagination item at rest (the previous and next buttons included)
    /// </summary>
    public partial Color ItemColor { get; set; }

    /// <summary>
    /// 页码选项悬停文本颜色
    /// Text color of a hovered Pagination item
    /// </summary>
    public partial Color ItemHoverColor { get; set; }

    /// <summary>
    /// 页码选项悬停背景色
    /// Background color of a hovered Pagination item
    /// </summary>
    public partial Color ItemHoverBg { get; set; }

    /// <summary>
    /// 页码选项圆角
    /// Corner radius of a Pagination item
    /// </summary>
    public partial CornerRadius ItemBorderRadius { get; set; }
}
