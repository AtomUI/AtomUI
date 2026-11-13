using AtomUI.Desktop.Controls.DesignTokens;
using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

[ControlDesignToken]
public sealed partial class DataGridToken : AbstractControlDesignToken
{
    
    public DataGridToken()

    {
    }
    
    /// <summary>
    /// 表头背景
    /// Background of table header
    /// </summary>
    public partial Color HeaderBg { get; set; }
    
    /// <summary>
    /// 表头文字颜色
    /// Color of table header text
    /// </summary>
    public partial Color HeaderColor { get; set; }
    
    /// <summary>
    /// 表头排序激活态背景色
    /// Background color of table header when sorted
    /// </summary>
    public partial Color HeaderSortActiveBg { get; set; }
    
    /// <summary>
    /// 表头排序激活态悬浮背景色
    /// Background color of table header when sorted and hovered
    /// </summary>
    public partial Color HeaderSortHoverBg { get; set; }
    
    /// <summary>
    /// 表格排序列背景色
    /// Background color of table sorted column
    /// </summary>
    public partial Color BodySortBg { get; set; }
    
    /// <summary>
    /// 表格行悬浮背景色
    /// Background color of table hovered row
    /// </summary>
    public partial Color RowHoverBg { get; set; }
    
    /// <summary>
    /// 表格行选中背景色
    /// Background color of table selected row
    /// </summary>
    public partial Color RowSelectedBg { get; set; }
    
    /// <summary>
    /// 表格行选中悬浮背景色
    /// Background color of table selected row when hovered
    /// </summary>
    public partial Color RowSelectedHoverBg { get; set; }
    
    /// <summary>
    /// 表格行展开背景色
    /// Background color of table expanded row
    /// </summary>
    public partial Color RowExpandedBg { get; set; }
    
    /// <summary>
    /// 单元格内间距（默认大尺寸）
    /// Padding of table cell (large size by default)
    /// </summary>
    public partial Thickness CellPadding { get; set; }
    
    /// <summary>
    /// 单元格内间距（中等尺寸）
    /// Padding of table cell (middle size)
    /// </summary>
    public partial Thickness CellPaddingMD { get; set; }
    
    /// <summary>
    /// 单元格内间距（小尺寸）
    /// Padding of table cell (small size)
    /// </summary>
    public partial Thickness CellPaddingSM { get; set; }
    
    /// <summary>
    /// 表格边框/分割线颜色
    /// Border color of table
    /// </summary>
    public partial Color BorderColor { get; set; }
    
    /// <summary>
    /// 表头圆角
    /// Border radius of table header
    /// </summary>
    public partial CornerRadius HeaderBorderRadius { get; set; }
    
    /// <summary>
    /// 表格底部背景色
    /// Background of footer
    /// </summary>
    public partial Color FooterBg { get; set; }
    
    /// <summary>
    /// 表格底部文字颜色
    /// Color of footer text
    /// </summary>
    public partial Color FooterColor { get; set; }
    
    /// <summary>
    /// 单元格文字大小（默认大尺寸）
    /// Font size of table cell (large size by default)
    /// </summary>
    public partial double CellFontSize { get; set; }
    
    /// <summary>
    /// 单元格文字大小（中等尺寸）
    /// Font size of table cell (middle size)
    /// </summary>
    public partial double CellFontSizeMD { get; set; }
    
    /// <summary>
    /// 单元格文字大小（小尺寸）
    /// Font size of table cell (small size)
    /// </summary>
    public partial double CellFontSizeSM { get; set; }
    
    /// <summary>
    /// 表头分割线颜色
    /// Split border color of table header
    /// </summary>
    public partial Color HeaderSplitColor { get; set; }
    
    /// <summary>
    /// 固定表头排序激活态背景色
    /// Background color of fixed table header when sorted
    /// </summary>
    [ReservedToken("Reserved for a separate sticky header surface; AtomUI renders its header once in the grid viewport.")]
    public partial Color FixedHeaderSortActiveBg { get; set; }
    
    /// <summary>
    /// 表头过滤按钮悬浮背景色
    /// Background color of table header filter button when hovered
    /// </summary>
    public partial Color HeaderFilterHoverBg { get; set; }
    
    /// <summary>
    /// 过滤下拉菜单选项背景
    /// Background of filter dropdown menu item
    /// </summary>
    public partial Color FilterDropdownMenuBg { get; set; }
    
    /// <summary>
    /// 过滤下拉菜单颜色
    /// Color of filter dropdown
    /// </summary>
    public partial Color FilterDropdownBg { get; set; }
    
    /// <summary>
    /// 展开按钮背景色
    /// Background of expand button
    /// </summary>
    public partial Color ExpandIconBg { get; set; }
    
    /// <summary>
    /// 选择列宽度
    /// Width of selection column
    /// </summary>
    [ReservedToken("Reserved: selection columns auto-measure their real selector and CellPadding; the legacy default width would narrow or clip that layout.")]
    public partial double SelectionColumnWidth { get; set; }
    
    /// <summary>
    /// 左侧列固定阴影
    /// </summary>
    public partial BoxShadows LeftFrozenShadows { get; set; }
    
    /// <summary>
    /// 右侧列固定阴影
    /// </summary>
    public partial BoxShadows RightFrozenShadows { get; set; }
    
    /// <summary>
    /// 列拖动排序时候的当前列的背景颜色
    /// </summary>
    public partial Color ColumnReorderActiveBg { get; set; }
    
    /// <summary>
    /// 分页器的外边距
    /// </summary>
    public partial Thickness PaginationMargin { get; set; }
    
    /// <summary>
    /// 分页器的外边距（小号）
    /// </summary>
    public partial Thickness PaginationMarginSM { get; set; }

    /// <summary>
    /// 分页面板的水平内边距
    /// </summary>
    public partial Thickness PaginationPanelMargin { get; set; }

    /// <summary>
    /// 分页面板的水平内边距（小号）
    /// </summary>
    public partial Thickness PaginationPanelMarginSM { get; set; }

    #region 内部 Token

    [ReservedToken("Reserved for the Ant Design inline row-indent expander layout; AtomUI uses a dedicated detail column.")]
    internal partial Thickness ExpandIconMargin { get; set; }
    [ReservedToken("Reserved for the Ant Design CSS expander positioning model; AtomUI centers fixed plus/minus glyphs in a Canvas.")]
    internal partial double ExpandIconHalfInner { get; set; }
    [ReservedToken("Reserved for the Ant Design unscaled CSS expander box; AtomUI uses SizeMS for its final box size.")]
    internal partial double ExpandIconSize { get; set; }
    [ReservedToken("Reserved for the Ant Design CSS expander transform; AtomUI sizes its centered expander directly.")]
    internal partial double ExpandIconScale { get; set; }
    internal partial double SortIconSize { get; set; }
    internal partial double RowReorderIndicatorSize { get; set; }
    internal partial Color HeaderIconColor { get; set; }
    internal partial Color HeaderIconHoverColor { get; set; }
    internal partial Thickness SortIndicatorLayoutMargin { get; set; }
    internal partial Thickness FilterIndicatorPadding { get; set; }

    #endregion

    #region 内部别名定义
    internal partial double TableFontSize { get; set; }
    internal partial Color TableBg { get; set; }
    internal partial CornerRadius TableRadius { get; set; }
    internal partial Thickness TablePadding { get; set; }
    internal partial Thickness TablePaddingMiddle { get; set; }
    internal partial Thickness TablePaddingSmall { get; set; }
    internal partial Color TableBorderColor { get; set; }
    internal partial Color TableHeaderTextColor { get; set; }
    internal partial Color TableHeaderBg { get; set; }
    internal partial Color TableFooterTextColor { get; set; }
    internal partial Color TableFooterBg { get; set; }
    internal partial Color TableHeaderCellSplitColor { get; set; }
    internal partial Color TableHeaderSortBg { get; set; }
    internal partial Color TableHeaderSortHoverBg { get; set; }
    internal partial Color TableBodySortBg { get; set; }
    [ReservedToken("Reserved alias for the separate sticky header surface that AtomUI does not create.")]
    internal partial Color TableFixedHeaderSortActiveBg { get; set; }
    internal partial Color TableHeaderFilterActiveBg { get; set; }
    internal partial Color TableFilterDropdownBg { get; set; }
    internal partial double TableFilterButtonSpacing  { get; set; }
    internal partial Thickness TableFilterButtonContainerMargin { get; set; }
    internal partial Thickness TableFilterButtonLayoutSeparatorMargin { get; set; }

    [ReservedToken("Reserved for a constrained filter dropdown viewport; the current flyout measures its actual items.")]
    internal partial double TableFilterDropdownHeight { get; set; }
    internal partial double TableFilterDropdownWidth { get; set; }
    internal partial Thickness TableFilterDropdownPadding { get; set; }
    [ReservedToken("Reserved for a filter search editor; the current filter flyout has no search editor.")]
    internal partial double TableFilterDropdownSearchWidth { get; set; }
    
    internal partial Color TableRowHoverBg { get; set; }
    internal partial Color TableSelectedRowBg { get; set; }
    internal partial Color TableSelectedRowHoverBg { get; set; }
    internal partial double TableFontSizeMiddle { get; set; }
    internal partial double TableFontSizeSmall { get; set; }
    [ReservedToken("Reserved alias: selection column width follows automatic selector and CellPadding measurement, or the explicit column Width.")]
    internal partial double TableSelectionColumnWidth { get; set; }
    internal partial Color TableExpandIconBg { get; set; }
    [ReservedToken("Reserved: detail columns auto-measure the expander and CellPadding; applying the legacy fixed width changes the existing layout.")]
    internal partial double TableExpandColumnWidth { get; set; }
    internal partial Color TableExpandedRowBg { get; set; }
    
    internal partial CornerRadius TableTopLeftColumnCornerRadius { get; set; }
    
    #endregion
    
    protected override void CalculateTokenValues(bool isDarkMode)
    {
        base.CalculateTokenValues(isDarkMode);
        var colorFillSecondarySolid = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillSecondary, EffectiveGlobalToken.ColorBgContainer);
        var colorFillContentSolid = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillContent, EffectiveGlobalToken.ColorBgContainer);
        var colorFillAlterSolid = ColorUtils.OnBackground(EffectiveGlobalToken.ColorFillAlter, EffectiveGlobalToken.ColorBgContainer);

        var baseColorAction      = EffectiveGlobalToken.ColorIcon;
        var baseColorActionHover = EffectiveGlobalToken.ColorIconHover;
        

        if (!IsExplicit(DataGridTokenKind.HeaderBg))
        {
            HeaderBg                    = colorFillAlterSolid;
        }

        if (!IsExplicit(DataGridTokenKind.HeaderColor))
        {
            HeaderColor                 = EffectiveGlobalToken.ColorTextHeading;
        }

        if (!IsExplicit(DataGridTokenKind.HeaderSortActiveBg))
        {
            HeaderSortActiveBg          = colorFillSecondarySolid;
        }

        if (!IsExplicit(DataGridTokenKind.HeaderSortHoverBg))
        {
            HeaderSortHoverBg           = colorFillContentSolid;
        }

        if (!IsExplicit(DataGridTokenKind.BodySortBg))
        {
            BodySortBg                  = colorFillAlterSolid;
        }

        if (!IsExplicit(DataGridTokenKind.RowHoverBg))
        {
            RowHoverBg                  = colorFillAlterSolid;
        }

        if (!IsExplicit(DataGridTokenKind.RowSelectedBg))
        {
            RowSelectedBg               = EffectiveGlobalToken.ControlItemBgActive;
        }

        if (!IsExplicit(DataGridTokenKind.RowSelectedHoverBg))
        {
            RowSelectedHoverBg          = EffectiveGlobalToken.ControlItemBgActiveHover;
        }

        if (!IsExplicit(DataGridTokenKind.RowExpandedBg))
        {
            RowExpandedBg               = EffectiveGlobalToken.ColorFillAlter;
        }

        if (!IsExplicit(DataGridTokenKind.CellPadding))
        {
            CellPadding                 = EffectiveGlobalToken.Padding;
        }

        // 对齐 AntD：中等尺寸单元格横向 paddingXS、纵向 paddingSM（12x8）。

        if (!IsExplicit(DataGridTokenKind.CellPaddingMD))
        {
            CellPaddingMD               = new Thickness(
                EffectiveGlobalToken.PaddingXS.Left,
                EffectiveGlobalToken.PaddingSM.Top,
                EffectiveGlobalToken.PaddingXS.Right,
                EffectiveGlobalToken.PaddingSM.Bottom);
        }

        if (!IsExplicit(DataGridTokenKind.CellPaddingSM))
        {
            CellPaddingSM               = EffectiveGlobalToken.PaddingXS;
        }

        // BorderColor 派生出 TableBorderColor，用户显式覆盖必须先于派生值生效。
        if (!IsExplicit(DataGridTokenKind.BorderColor))
        {
            BorderColor = EffectiveGlobalToken.ColorBorderSecondary;
        }

        if (!IsExplicit(DataGridTokenKind.HeaderBorderRadius))
        {
            HeaderBorderRadius          = EffectiveGlobalToken.BorderRadiusLG;
        }

        if (!IsExplicit(DataGridTokenKind.FooterBg))
        {
            FooterBg                    = colorFillAlterSolid;
        }

        if (!IsExplicit(DataGridTokenKind.FooterColor))
        {
            FooterColor                 = EffectiveGlobalToken.ColorTextHeading;
        }

        if (!IsExplicit(DataGridTokenKind.CellFontSize))
        {
            CellFontSize                = EffectiveGlobalToken.FontSize;
        }

        if (!IsExplicit(DataGridTokenKind.CellFontSizeMD))
        {
            CellFontSizeMD              = EffectiveGlobalToken.FontSize;
        }

        if (!IsExplicit(DataGridTokenKind.CellFontSizeSM))
        {
            CellFontSizeSM              = EffectiveGlobalToken.FontSize;
        }

        if (!IsExplicit(DataGridTokenKind.HeaderSplitColor))
        {
            HeaderSplitColor            = EffectiveGlobalToken.ColorBorderSecondary;
        }

        if (!IsExplicit(DataGridTokenKind.FixedHeaderSortActiveBg))
        {
            FixedHeaderSortActiveBg     = colorFillSecondarySolid;
        }

        if (!IsExplicit(DataGridTokenKind.HeaderFilterHoverBg))
        {
            HeaderFilterHoverBg         = EffectiveGlobalToken.ColorFillContent;
        }

        FilterDropdownMenuBg        = EffectiveGlobalToken.ColorBgContainer;

        if (!IsExplicit(DataGridTokenKind.FilterDropdownBg))
        {
            FilterDropdownBg            = EffectiveGlobalToken.ColorBgContainer;
        }

        if (!IsExplicit(DataGridTokenKind.ExpandIconBg))
        {
            ExpandIconBg                = EffectiveGlobalToken.ColorBgContainer;
        }

        if (!IsExplicit(DataGridTokenKind.SelectionColumnWidth))
        {
            SelectionColumnWidth        = EffectiveGlobalToken.ControlHeight;
        }

        ExpandIconMargin            = new Thickness(0, 
            (EffectiveGlobalToken.FontSize * EffectiveGlobalToken.FontHeight - EffectiveGlobalToken.LineWidth * 3) / 2 -
            Math.Ceiling((EffectiveGlobalToken.FontSizeSM * 1.4 - EffectiveGlobalToken.LineWidth * 3) / 2),
            0, 
            0);
        HeaderIconColor = ColorUtils.FromRgbF(
            baseColorAction.GetAlphaF() * EffectiveGlobalToken.OpacityLoading,
            baseColorAction.GetRedF(),
            baseColorAction.GetGreenF(),
            baseColorAction.GetBlueF());
        HeaderIconHoverColor = ColorUtils.FromRgbF(
            baseColorActionHover.GetAlphaF() * EffectiveGlobalToken.OpacityLoading,
            baseColorActionHover.GetRedF(),
            baseColorActionHover.GetGreenF(),
            baseColorActionHover.GetBlueF());
        // Reserved geometry is independent, as in the original local calculations. Keep direct field-fed
        // expressions so globals used only by these non-rendered slots are not advertised as consumed.
        ExpandIconHalfInner = EffectiveGlobalToken.ControlInteractiveSize / 2 - EffectiveGlobalToken.LineWidth;
        ExpandIconSize = (EffectiveGlobalToken.ControlInteractiveSize / 2 - EffectiveGlobalToken.LineWidth) * 2 +
                         EffectiveGlobalToken.LineWidth * 3;
        RowReorderIndicatorSize   = EffectiveGlobalToken.SizeMD; // TODO 需要根据 SizeType 做一定的调整
        ExpandIconScale = EffectiveGlobalToken.ControlInteractiveSize /
                          ((EffectiveGlobalToken.ControlInteractiveSize / 2 - EffectiveGlobalToken.LineWidth) * 2 +
                           EffectiveGlobalToken.LineWidth * 3);
        SortIconSize              = EffectiveGlobalToken.FontHeight / 2.5;
        SortIndicatorLayoutMargin = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0, 0, 0);
        FilterIndicatorPadding    = new Thickness(EffectiveGlobalToken.UniformlyPaddingXXS, EffectiveGlobalToken.UniformlyPaddingXS);
        
        // 别名控件初始化
        TableFontSize = CellFontSize;
        TableBg       = EffectiveGlobalToken.ColorBgContainer;
        TableRadius   = HeaderBorderRadius;
        
        TablePadding                   = CellPadding;
        TablePaddingMiddle             = CellPaddingMD;
        TablePaddingSmall              = CellPaddingSM;
        TableBorderColor               = BorderColor;
        TableHeaderTextColor           = HeaderColor;
        TableHeaderBg                  = HeaderBg;
        TableFooterTextColor           = FooterColor;
        TableFooterBg                  = FooterBg;
        TableHeaderCellSplitColor      = HeaderSplitColor;
        TableHeaderSortBg              = HeaderSortActiveBg;
        TableHeaderSortHoverBg         = HeaderSortHoverBg;
        TableBodySortBg                = BodySortBg;
        TableFixedHeaderSortActiveBg   = FixedHeaderSortActiveBg;
        TableHeaderFilterActiveBg      = HeaderFilterHoverBg;
        TableFilterDropdownBg          = FilterDropdownBg;
        TableRowHoverBg                = RowHoverBg;
        TableSelectedRowBg             = RowSelectedBg;
        TableSelectedRowHoverBg        = RowSelectedHoverBg;
        TableTopLeftColumnCornerRadius = new CornerRadius(EffectiveGlobalToken.BorderRadiusLG.TopLeft, 0, 0, 0);

        TableFontSizeMiddle       = CellFontSizeMD;
        TableFontSizeSmall        = CellFontSizeSM;
        TableSelectionColumnWidth = SelectionColumnWidth;
        TableExpandIconBg         = ExpandIconBg;
        TableExpandColumnWidth    = EffectiveGlobalToken.ControlInteractiveSize + EffectiveGlobalToken.UniformlyPadding * 2;
        TableExpandedRowBg        = RowExpandedBg;
        
        // Dropdown
        // TODO 暂未使用
        TableFilterDropdownWidth               = 120;
        TableFilterDropdownHeight              = 264;
        TableFilterDropdownSearchWidth         = 140;
        TableFilterButtonSpacing               = EffectiveGlobalToken.UniformlyMarginXS;
        TableFilterButtonContainerMargin       = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS, 0, 0);
        TableFilterButtonLayoutSeparatorMargin = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXXS, 0, 0);
        TableFilterDropdownPadding             = EffectiveGlobalToken.PaddingXS;
        // 对齐 AntD fixed.ts：冻结列分界阴影为 inset 10px 0 8px -8px（左）与 inset -10px 0 8px -8px（右）。
        LeftFrozenShadows = new BoxShadows(new BoxShadow
        {
            OffsetX = 10,
            OffsetY = 0,
            Blur    = 8,
            Spread  = -8,
            IsInset = true,
            Color   = EffectiveGlobalToken.ColorSplit
        });
        RightFrozenShadows = new BoxShadows(new BoxShadow
        {
            OffsetX = -10,
            OffsetY = 0,
            Blur    = 8,
            Spread  = -8,
            IsInset = true,
            Color   = EffectiveGlobalToken.ColorSplit
        });
        ColumnReorderActiveBg = colorFillContentSolid;

        PaginationMargin   = new Thickness(0, EffectiveGlobalToken.UniformlyMargin);
        PaginationMarginSM = new Thickness(0, EffectiveGlobalToken.UniformlyMarginXS);
        PaginationPanelMargin   = new Thickness(EffectiveGlobalToken.UniformlyMargin, 0);
        PaginationPanelMarginSM = new Thickness(EffectiveGlobalToken.UniformlyMarginXS, 0);
    }
    
}
