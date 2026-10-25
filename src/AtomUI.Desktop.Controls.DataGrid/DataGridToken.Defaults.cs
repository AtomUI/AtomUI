using AtomUI.Media;
using AtomUI.Theme.DesignTokens;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls;

internal sealed partial class DataGridToken
{
    [ControlTokenDefault(nameof(HeaderBg))]
    private static Color DefaultForHeaderBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillAlter, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(HeaderColor))]
    private static Color DefaultForHeaderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextHeading;

    [ControlTokenDefault(nameof(HeaderSortActiveBg))]
    private static Color DefaultForHeaderSortActiveBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillSecondary, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(HeaderSortHoverBg))]
    private static Color DefaultForHeaderSortHoverBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillContent, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(BodySortBg))]
    private static Color DefaultForBodySortBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillAlter, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(RowHoverBg))]
    private static Color DefaultForRowHoverBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillAlter, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(RowSelectedBg))]
    private static Color DefaultForRowSelectedBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActive;

    [ControlTokenDefault(nameof(RowSelectedHoverBg))]
    private static Color DefaultForRowSelectedHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ControlItemBgActiveHover;

    [ControlTokenDefault(nameof(RowExpandedBg))]
    private static Color DefaultForRowExpandedBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillAlter;

    [ControlTokenDefault(nameof(CellPadding))]
    private static Thickness DefaultForCellPadding(in ControlTokenDefaultContext context) =>
        context.Global.Padding;

    [ControlTokenDefault(nameof(CellPaddingMD))]
    private static Thickness DefaultForCellPaddingMD(in ControlTokenDefaultContext context) =>
        new Thickness(
            context.Global.PaddingXS.Left,
            context.Global.PaddingSM.Top,
            context.Global.PaddingXS.Right,
            context.Global.PaddingSM.Bottom);

    [ControlTokenDefault(nameof(CellPaddingSM))]
    private static Thickness DefaultForCellPaddingSM(in ControlTokenDefaultContext context) =>
        context.Global.PaddingXS;

    [ControlTokenDefault(nameof(BorderColor))]
    private static Color DefaultForBorderColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorderSecondary;

    [ControlTokenDefault(nameof(HeaderBorderRadius))]
    private static CornerRadius DefaultForHeaderBorderRadius(in ControlTokenDefaultContext context) =>
        context.Global.BorderRadiusLG;

    [ControlTokenDefault(nameof(FooterBg))]
    private static Color DefaultForFooterBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillAlter, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(FooterColor))]
    private static Color DefaultForFooterColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorTextHeading;

    [ControlTokenDefault(nameof(CellFontSize))]
    private static double DefaultForCellFontSize(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(CellFontSizeMD))]
    private static double DefaultForCellFontSizeMD(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(CellFontSizeSM))]
    private static double DefaultForCellFontSizeSM(in ControlTokenDefaultContext context) =>
        context.Global.FontSize;

    [ControlTokenDefault(nameof(HeaderSplitColor))]
    private static Color DefaultForHeaderSplitColor(in ControlTokenDefaultContext context) =>
        context.Global.ColorBorderSecondary;

    [ControlTokenDefault(nameof(FixedHeaderSortActiveBg))]
    private static Color DefaultForFixedHeaderSortActiveBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillSecondary, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(HeaderFilterHoverBg))]
    private static Color DefaultForHeaderFilterHoverBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorFillContent;

    [ControlTokenDefault(nameof(FilterDropdownMenuBg))]
    private static Color DefaultForFilterDropdownMenuBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(FilterDropdownBg))]
    private static Color DefaultForFilterDropdownBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(ExpandIconBg))]
    private static Color DefaultForExpandIconBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(SelectionColumnWidth))]
    private static double DefaultForSelectionColumnWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlHeight;

    [ControlTokenDefault(nameof(LeftFrozenShadows))]
    private static BoxShadows DefaultForLeftFrozenShadows(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = 10,
            OffsetY = 0,
            Blur    = 8,
            Spread  = -8,
            IsInset = true,
            Color   = context.Global.ColorSplit
        });

    [ControlTokenDefault(nameof(RightFrozenShadows))]
    private static BoxShadows DefaultForRightFrozenShadows(in ControlTokenDefaultContext context) =>
        new BoxShadows(new BoxShadow
        {
            OffsetX = -10,
            OffsetY = 0,
            Blur    = 8,
            Spread  = -8,
            IsInset = true,
            Color   = context.Global.ColorSplit
        });

    [ControlTokenDefault(nameof(ColumnReorderActiveBg))]
    private static Color DefaultForColumnReorderActiveBg(in ControlTokenDefaultContext context) =>
        ColorUtils.OnBackground(context.Global.ColorFillContent, context.Global.ColorBgContainer);

    [ControlTokenDefault(nameof(PaginationMargin))]
    private static Thickness DefaultForPaginationMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMargin);

    [ControlTokenDefault(nameof(PaginationMarginSM))]
    private static Thickness DefaultForPaginationMarginSM(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS);

    [ControlTokenDefault(nameof(PaginationPanelMargin))]
    private static Thickness DefaultForPaginationPanelMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMargin, 0);

    [ControlTokenDefault(nameof(PaginationPanelMarginSM))]
    private static Thickness DefaultForPaginationPanelMarginSM(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXS, 0);

    [ControlTokenDefault(nameof(ExpandIconMargin))]
    private static Thickness DefaultForExpandIconMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0,
            (context.Global.FontSize * context.Global.FontHeight - context.Global.LineWidth * 3) / 2 -
            Math.Ceiling((context.Global.FontSizeSM * 1.4 - context.Global.LineWidth * 3) / 2),
            0,
            0);

    [ControlTokenDefault(nameof(ExpandIconHalfInner))]
    private static double DefaultForExpandIconHalfInner(in ControlTokenDefaultContext context) =>
        (context.Global.ControlInteractiveSize / 2 - context.Global.LineWidth);

    [ControlTokenDefault(nameof(ExpandIconSize))]
    private static double DefaultForExpandIconSize(in ControlTokenDefaultContext context) =>
        ((context.Global.ControlInteractiveSize / 2 - context.Global.LineWidth) * 2 + context.Global.LineWidth * 3);

    [ControlTokenDefault(nameof(ExpandIconScale))]
    private static double DefaultForExpandIconScale(in ControlTokenDefaultContext context) =>
        context.Global.ControlInteractiveSize / ((context.Global.ControlInteractiveSize / 2 - context.Global.LineWidth) * 2 + context.Global.LineWidth * 3);

    [ControlTokenDefault(nameof(SortIconSize))]
    private static double DefaultForSortIconSize(in ControlTokenDefaultContext context) =>
        context.Global.FontHeight / 2.5;

    [ControlTokenDefault(nameof(RowReorderIndicatorSize))]
    private static double DefaultForRowReorderIndicatorSize(in ControlTokenDefaultContext context) =>
        context.Global.SizeMD;

    [ControlTokenDefault(nameof(HeaderIconColor))]
    private static Color DefaultForHeaderIconColor(in ControlTokenDefaultContext context) =>
        ColorUtils.FromRgbF(
            context.Global.ColorIcon.GetAlphaF() * context.Global.OpacityLoading,
            context.Global.ColorIcon.GetRedF(),
            context.Global.ColorIcon.GetGreenF(),
            context.Global.ColorIcon.GetBlueF());

    [ControlTokenDefault(nameof(HeaderIconHoverColor))]
    private static Color DefaultForHeaderIconHoverColor(in ControlTokenDefaultContext context) =>
        ColorUtils.FromRgbF(
            context.Global.ColorIconHover.GetAlphaF() * context.Global.OpacityLoading,
            context.Global.ColorIcon.GetRedF(),
            context.Global.ColorIcon.GetGreenF(),
            context.Global.ColorIcon.GetBlueF());

    [ControlTokenDefault(nameof(SortIndicatorLayoutMargin))]
    private static Thickness DefaultForSortIndicatorLayoutMargin(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyMarginXS, 0, 0, 0);

    [ControlTokenDefault(nameof(FilterIndicatorPadding))]
    private static Thickness DefaultForFilterIndicatorPadding(in ControlTokenDefaultContext context) =>
        new Thickness(context.Global.UniformlyPaddingXXS, context.Global.UniformlyPaddingXS);

    [ControlTokenDefault(nameof(TableBg))]
    private static Color DefaultForTableBg(in ControlTokenDefaultContext context) =>
        context.Global.ColorBgContainer;

    [ControlTokenDefault(nameof(TableHeaderCellSplitColor), nameof(HeaderSplitColor))]
    private static Color DefaultForTableHeaderCellSplitColor(in ControlTokenDefaultContext context, Color headerSplitColor) =>
        headerSplitColor;

    [ControlTokenDefault(nameof(TableHeaderSortBg), nameof(HeaderSortActiveBg))]
    private static Color DefaultForTableHeaderSortBg(in ControlTokenDefaultContext context, Color headerSortActiveBg) =>
        headerSortActiveBg;

    [ControlTokenDefault(nameof(TableHeaderSortHoverBg), nameof(HeaderSortHoverBg))]
    private static Color DefaultForTableHeaderSortHoverBg(in ControlTokenDefaultContext context, Color headerSortHoverBg) =>
        headerSortHoverBg;

    [ControlTokenDefault(nameof(TableBodySortBg), nameof(BodySortBg))]
    private static Color DefaultForTableBodySortBg(in ControlTokenDefaultContext context, Color bodySortBg) =>
        bodySortBg;

    [ControlTokenDefault(nameof(TableFixedHeaderSortActiveBg), nameof(FixedHeaderSortActiveBg))]
    private static Color DefaultForTableFixedHeaderSortActiveBg(in ControlTokenDefaultContext context, Color fixedHeaderSortActiveBg) =>
        fixedHeaderSortActiveBg;

    [ControlTokenDefault(nameof(TableHeaderFilterActiveBg), nameof(HeaderFilterHoverBg))]
    private static Color DefaultForTableHeaderFilterActiveBg(in ControlTokenDefaultContext context, Color headerFilterHoverBg) =>
        headerFilterHoverBg;

    [ControlTokenDefault(nameof(TableFilterButtonSpacing))]
    private static double DefaultForTableFilterButtonSpacing(in ControlTokenDefaultContext context) =>
        context.Global.UniformlyMarginXS;

    [ControlTokenDefault(nameof(TableFilterButtonContainerMargin))]
    private static Thickness DefaultForTableFilterButtonContainerMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXS, 0, 0);

    [ControlTokenDefault(nameof(TableFilterButtonLayoutSeparatorMargin))]
    private static Thickness DefaultForTableFilterButtonLayoutSeparatorMargin(in ControlTokenDefaultContext context) =>
        new Thickness(0, context.Global.UniformlyMarginXXS, 0, 0);

    [ControlTokenDefault(nameof(TableFilterDropdownHeight))]
    private static double DefaultForTableFilterDropdownHeight(in ControlTokenDefaultContext context) =>
        264;

    [ControlTokenDefault(nameof(TableFilterDropdownWidth))]
    private static double DefaultForTableFilterDropdownWidth(in ControlTokenDefaultContext context) =>
        120;

    [ControlTokenDefault(nameof(TableFilterDropdownPadding))]
    private static Thickness DefaultForTableFilterDropdownPadding(in ControlTokenDefaultContext context) =>
        context.Global.PaddingXS;

    [ControlTokenDefault(nameof(TableFilterDropdownSearchWidth))]
    private static double DefaultForTableFilterDropdownSearchWidth(in ControlTokenDefaultContext context) =>
        140;

    [ControlTokenDefault(nameof(TableSelectionColumnWidth), nameof(SelectionColumnWidth))]
    private static double DefaultForTableSelectionColumnWidth(in ControlTokenDefaultContext context, double selectionColumnWidth) =>
        selectionColumnWidth;

    [ControlTokenDefault(nameof(TableExpandIconBg), nameof(ExpandIconBg))]
    private static Color DefaultForTableExpandIconBg(in ControlTokenDefaultContext context, Color expandIconBg) =>
        expandIconBg;

    [ControlTokenDefault(nameof(TableExpandColumnWidth))]
    private static double DefaultForTableExpandColumnWidth(in ControlTokenDefaultContext context) =>
        context.Global.ControlInteractiveSize + context.Global.UniformlyPadding * 2;

    [ControlTokenDefault(nameof(TableTopLeftColumnCornerRadius))]
    private static CornerRadius DefaultForTableTopLeftColumnCornerRadius(in ControlTokenDefaultContext context) =>
        new CornerRadius(context.Global.BorderRadiusLG.TopLeft, 0, 0, 0);

}
