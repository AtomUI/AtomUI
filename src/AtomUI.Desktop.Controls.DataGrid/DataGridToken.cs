using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken(ControlTokenEvaluationVersion.DependencyDefaultsV2, Revision = 1)]
internal sealed partial class DataGridToken : AbstractControlDesignToken, IControlTokenAliases, IControlTokenDefaults
{
    
    public DataGridToken()

    {
    }
    
    /// <summary>
    /// 表头背景
    /// Background of table header
    /// </summary>
    public Color HeaderBg { get; set; }
    
    /// <summary>
    /// 表头文字颜色
    /// Color of table header text
    /// </summary>
    public Color HeaderColor { get; set; }
    
    /// <summary>
    /// 表头排序激活态背景色
    /// Background color of table header when sorted
    /// </summary>
    public Color HeaderSortActiveBg { get; set; }
    
    /// <summary>
    /// 表头排序激活态悬浮背景色
    /// Background color of table header when sorted and hovered
    /// </summary>
    public Color HeaderSortHoverBg { get; set; }
    
    /// <summary>
    /// 表格排序列背景色
    /// Background color of table sorted column
    /// </summary>
    public Color BodySortBg { get; set; }
    
    /// <summary>
    /// 表格行悬浮背景色
    /// Background color of table hovered row
    /// </summary>
    public Color RowHoverBg { get; set; }
    
    /// <summary>
    /// 表格行选中背景色
    /// Background color of table selected row
    /// </summary>
    public Color RowSelectedBg { get; set; }
    
    /// <summary>
    /// 表格行选中悬浮背景色
    /// Background color of table selected row when hovered
    /// </summary>
    public Color RowSelectedHoverBg { get; set; }
    
    /// <summary>
    /// 表格行展开背景色
    /// Background color of table expanded row
    /// </summary>
    public Color RowExpandedBg { get; set; }
    
    /// <summary>
    /// 单元格内间距（默认大尺寸）
    /// Padding of table cell (large size by default)
    /// </summary>
    public Thickness CellPadding { get; set; }
    
    /// <summary>
    /// 单元格内间距（中等尺寸）
    /// Padding of table cell (middle size)
    /// </summary>
    public Thickness CellPaddingMD { get; set; }
    
    /// <summary>
    /// 单元格内间距（小尺寸）
    /// Padding of table cell (small size)
    /// </summary>
    public Thickness CellPaddingSM { get; set; }
    
    /// <summary>
    /// 表格边框/分割线颜色
    /// Border color of table
    /// </summary>
    public Color BorderColor { get; set; }
    
    /// <summary>
    /// 表头圆角
    /// Border radius of table header
    /// </summary>
    public CornerRadius HeaderBorderRadius { get; set; }
    
    /// <summary>
    /// 表格底部背景色
    /// Background of footer
    /// </summary>
    public Color FooterBg { get; set; }
    
    /// <summary>
    /// 表格底部文字颜色
    /// Color of footer text
    /// </summary>
    public Color FooterColor { get; set; }
    
    /// <summary>
    /// 单元格文字大小（默认大尺寸）
    /// Font size of table cell (large size by default)
    /// </summary>
    public double CellFontSize { get; set; }
    
    /// <summary>
    /// 单元格文字大小（中等尺寸）
    /// Font size of table cell (middle size)
    /// </summary>
    public double CellFontSizeMD { get; set; }
    
    /// <summary>
    /// 单元格文字大小（小尺寸）
    /// Font size of table cell (small size)
    /// </summary>
    public double CellFontSizeSM { get; set; }
    
    /// <summary>
    /// 表头分割线颜色
    /// Split border color of table header
    /// </summary>
    public Color HeaderSplitColor { get; set; }
    
    /// <summary>
    /// 固定表头排序激活态背景色
    /// Background color of fixed table header when sorted
    /// </summary>
    public Color FixedHeaderSortActiveBg { get; set; }
    
    /// <summary>
    /// 表头过滤按钮悬浮背景色
    /// Background color of table header filter button when hovered
    /// </summary>
    public Color HeaderFilterHoverBg { get; set; }
    
    /// <summary>
    /// 过滤下拉菜单选项背景
    /// Background of filter dropdown menu item
    /// </summary>
    public Color FilterDropdownMenuBg { get; set; }
    
    /// <summary>
    /// 过滤下拉菜单颜色
    /// Color of filter dropdown
    /// </summary>
    public Color FilterDropdownBg { get; set; }
    
    /// <summary>
    /// 展开按钮背景色
    /// Background of expand button
    /// </summary>
    public Color ExpandIconBg { get; set; }
    
    /// <summary>
    /// 选择列宽度
    /// Width of selection column
    /// </summary>
    public double SelectionColumnWidth { get; set; }
    
    /// <summary>
    /// 左侧列固定阴影
    /// </summary>
    public BoxShadows LeftFrozenShadows { get; set; }
    
    /// <summary>
    /// 右侧列固定阴影
    /// </summary>
    public BoxShadows RightFrozenShadows { get; set; }
    
    /// <summary>
    /// 列拖动排序时候的当前列的背景颜色
    /// </summary>
    public Color ColumnReorderActiveBg { get; set; }
    
    /// <summary>
    /// 分页器的外边距
    /// </summary>
    public Thickness PaginationMargin { get; set; }
    
    /// <summary>
    /// 分页器的外边距（小号）
    /// </summary>
    public Thickness PaginationMarginSM { get; set; }

    /// <summary>
    /// 分页面板的水平内边距
    /// </summary>
    public Thickness PaginationPanelMargin { get; set; }

    /// <summary>
    /// 分页面板的水平内边距（小号）
    /// </summary>
    public Thickness PaginationPanelMarginSM { get; set; }

    #region 内部 Token

    internal Thickness ExpandIconMargin { get; set; }
    internal double ExpandIconHalfInner { get; set; }
    internal double ExpandIconSize { get; set; }
    internal double ExpandIconScale { get; set; }
    internal double SortIconSize { get; set; }
    internal double RowReorderIndicatorSize { get; set; }
    internal Color HeaderIconColor { get; set; }
    internal Color HeaderIconHoverColor { get; set; }
    internal Thickness SortIndicatorLayoutMargin { get; set; }
    internal Thickness FilterIndicatorPadding { get; set; }

    #endregion

    #region 内部别名定义
    [ControlTokenAlias(nameof(CellFontSize))]
    internal double TableFontSize { get; set; }
    internal Color TableBg { get; set; }
    [ControlTokenAlias(nameof(HeaderBorderRadius))]
    internal CornerRadius TableRadius { get; set; }
    [ControlTokenAlias(nameof(CellPadding))]
    internal Thickness TablePadding { get; set; }
    [ControlTokenAlias(nameof(CellPaddingMD))]
    internal Thickness TablePaddingMiddle { get; set; }
    [ControlTokenAlias(nameof(CellPaddingSM))]
    internal Thickness TablePaddingSmall { get; set; }
    [ControlTokenAlias(nameof(BorderColor))]
    internal Color TableBorderColor { get; set; }
    [ControlTokenAlias(nameof(HeaderColor))]
    internal Color TableHeaderTextColor { get; set; }
    [ControlTokenAlias(nameof(HeaderBg))]
    internal Color TableHeaderBg { get; set; }
    [ControlTokenAlias(nameof(FooterColor))]
    internal Color TableFooterTextColor { get; set; }
    [ControlTokenAlias(nameof(FooterBg))]
    internal Color TableFooterBg { get; set; }
    internal Color TableHeaderCellSplitColor { get; set; }
    internal Color TableHeaderSortBg { get; set; }
    internal Color TableHeaderSortHoverBg { get; set; }
    internal Color TableBodySortBg { get; set; }
    internal Color TableFixedHeaderSortActiveBg { get; set; }
    internal Color TableHeaderFilterActiveBg { get; set; }
    [ControlTokenAlias(nameof(FilterDropdownBg))]
    internal Color TableFilterDropdownBg { get; set; }
    internal double TableFilterButtonSpacing  { get; set; }
    internal Thickness TableFilterButtonContainerMargin { get; set; }
    internal Thickness TableFilterButtonLayoutSeparatorMargin { get; set; }

    internal double TableFilterDropdownHeight { get; set; }
    internal double TableFilterDropdownWidth { get; set; }
    internal Thickness TableFilterDropdownPadding { get; set; }
    internal double TableFilterDropdownSearchWidth { get; set; }
    
    [ControlTokenAlias(nameof(RowHoverBg))]
    internal Color TableRowHoverBg { get; set; }
    [ControlTokenAlias(nameof(RowSelectedBg))]
    internal Color TableSelectedRowBg { get; set; }
    [ControlTokenAlias(nameof(RowSelectedHoverBg))]
    internal Color TableSelectedRowHoverBg { get; set; }
    [ControlTokenAlias(nameof(CellFontSizeMD))]
    internal double TableFontSizeMiddle { get; set; }
    [ControlTokenAlias(nameof(CellFontSizeSM))]
    internal double TableFontSizeSmall { get; set; }
    internal double TableSelectionColumnWidth { get; set; }
    internal Color TableExpandIconBg { get; set; }
    internal double TableExpandColumnWidth { get; set; }
    [ControlTokenAlias(nameof(RowExpandedBg))]
    internal Color TableExpandedRowBg { get; set; }
    
    internal CornerRadius TableTopLeftColumnCornerRadius { get; set; }
    
    #endregion
}
