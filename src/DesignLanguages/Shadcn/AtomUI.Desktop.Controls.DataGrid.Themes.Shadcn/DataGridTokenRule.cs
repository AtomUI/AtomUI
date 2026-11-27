using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Media;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="DataGridToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class DataGridTokenRule : IControlTokenRule<ShadcnToken, DataGridToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<DataGridToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var table  = context.Language.GetTableRecipe(global);
        var colorFillSecondarySolid = ColorUtils.OnBackground(global.ColorFillSecondary, global.ColorBgContainer);
        var colorFillContentSolid = ColorUtils.OnBackground(global.ColorFillContent, global.ColorBgContainer);
        var colorFillAlterSolid = table.HeaderBackground;

        var baseColorAction      = global.ColorIcon;
        var baseColorActionHover = global.ColorIconHover;
        // Header and rows keep the Ant formula's reads of the shadcn globals (the table recipe returns those reads); the
        // registry's text-muted-foreground header and hover:bg-muted/50 rows are not matched exactly.
        token.HeaderBg                    = colorFillAlterSolid;
        token.HeaderColor                 = table.HeaderForeground;
        token.HeaderSortActiveBg          = colorFillSecondarySolid;
        token.HeaderSortHoverBg           = colorFillContentSolid;
        token.BodySortBg                  = colorFillAlterSolid;
        token.RowHoverBg                  = colorFillAlterSolid;
        token.RowSelectedBg               = table.RowSelectedBackground;
        token.RowSelectedHoverBg          = table.RowSelectedHoverBackground;
        token.RowSelectedColor            = table.RowSelectedForeground;
        token.RowExpandedBg               = global.ColorFillAlter;
        token.CellPadding                 = table.CellPadding;
        token.CellPaddingMD               = table.CellPaddingMedium;
        token.CellPaddingSM               = table.CellPaddingSmall;
        token.BorderColor                 = table.BorderColor;
        token.HeaderBorderRadius          = table.CornerRadius;
        token.FooterBg                    = colorFillAlterSolid;
        token.FooterColor                 = global.ColorTextHeading;
        token.CellFontSize                = global.FontSize;
        token.CellFontSizeMD              = global.FontSize;
        token.CellFontSizeSM              = global.FontSize;
        token.HeaderSplitColor            = global.ColorBorderSecondary;
        token.FixedHeaderSortActiveBg     = colorFillSecondarySolid;
        token.HeaderFilterHoverBg         = global.ColorFillContent;

        token.FilterDropdownMenuBg        = global.ColorBgContainer;
        token.FilterDropdownBg            = global.ColorBgContainer;
        token.ExpandIconBg                = global.ColorBgContainer;
        token.SelectionColumnWidth        = global.ControlHeight;

        token.ExpandIconMargin            = new Thickness(0,
            (global.FontSize * global.FontHeight - global.LineWidth * 3) / 2 -
            Math.Ceiling((global.FontSizeSM * 1.4 - global.LineWidth * 3) / 2),
            0,
            0);
        token.HeaderIconColor = ColorUtils.FromRgbF(
            baseColorAction.GetAlphaF() * global.OpacityLoading,
            baseColorAction.GetRedF(),
            baseColorAction.GetGreenF(),
            baseColorAction.GetBlueF());
        token.HeaderIconHoverColor = ColorUtils.FromRgbF(
            baseColorActionHover.GetAlphaF() * global.OpacityLoading,
            baseColorActionHover.GetRedF(),
            baseColorActionHover.GetGreenF(),
            baseColorActionHover.GetBlueF());
        // Reserved geometry is independent, as in the original local calculations. Keep direct field-fed
        // expressions so globals used only by these non-rendered slots are not advertised as consumed.
        token.ExpandIconHalfInner = global.ControlInteractiveSize / 2 - global.LineWidth;
        token.ExpandIconSize = (global.ControlInteractiveSize / 2 - global.LineWidth) * 2 +
                         global.LineWidth * 3;
        token.RowReorderIndicatorSize   = global.SizeMD; // TODO: adjust for SizeType. Remove this TODO when it does.
        token.ExpandIconScale = global.ControlInteractiveSize /
                          ((global.ControlInteractiveSize / 2 - global.LineWidth) * 2 +
                           global.LineWidth * 3);
        token.SortIconSize              = global.FontHeight / 2.5;
        token.SortIndicatorLayoutMargin = new Thickness(global.UniformlyMarginXS, 0, 0, 0);
        token.FilterIndicatorPadding    = new Thickness(global.UniformlyPaddingXXS, global.UniformlyPaddingXS);

        // Initialize the tokens of the alias control (Table).
        token.TableFontSize = token.CellFontSize;
        token.TableBg       = table.RowBackground;
        token.TableRadius   = token.HeaderBorderRadius;

        token.TablePadding                   = token.CellPadding;
        token.TablePaddingMiddle             = token.CellPaddingMD;
        token.TablePaddingSmall              = token.CellPaddingSM;
        token.TableBorderColor               = token.BorderColor;
        token.TableHeaderTextColor           = token.HeaderColor;
        token.TableHeaderBg                  = token.HeaderBg;
        token.TableFooterTextColor           = token.FooterColor;
        token.TableFooterBg                  = token.FooterBg;
        token.TableHeaderCellSplitColor      = token.HeaderSplitColor;
        token.TableHeaderSortBg              = token.HeaderSortActiveBg;
        token.TableHeaderSortHoverBg         = token.HeaderSortHoverBg;
        token.TableBodySortBg                = token.BodySortBg;
        token.TableFixedHeaderSortActiveBg   = token.FixedHeaderSortActiveBg;
        token.TableHeaderFilterActiveBg      = token.HeaderFilterHoverBg;
        token.TableFilterDropdownBg          = token.FilterDropdownBg;
        token.TableRowHoverBg                = token.RowHoverBg;
        token.TableSelectedRowBg             = token.RowSelectedBg;
        token.TableSelectedRowHoverBg        = token.RowSelectedHoverBg;
        token.TableTopLeftColumnCornerRadius = new CornerRadius(global.BorderRadiusLG.TopLeft, 0, 0, 0);

        token.TableFontSizeMiddle       = token.CellFontSizeMD;
        token.TableFontSizeSmall        = token.CellFontSizeSM;
        token.TableSelectionColumnWidth = token.SelectionColumnWidth;
        token.TableExpandIconBg         = token.ExpandIconBg;
        token.TableExpandColumnWidth    = global.ControlInteractiveSize + global.UniformlyPadding * 2;
        token.TableExpandedRowBg        = token.RowExpandedBg;

        // Dropdown
        // TODO Table filter dropdown: these tokens are not used yet. Verify whether the filter dropdown now reads
        // them, wire it up where it does not, and remove this TODO when the tokens are used.
        token.TableFilterDropdownWidth               = 120;
        token.TableFilterDropdownHeight              = 264;
        token.TableFilterDropdownSearchWidth         = 140;
        token.TableFilterButtonSpacing               = global.UniformlyMarginXS;
        token.TableFilterButtonContainerMargin       = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        token.TableFilterButtonLayoutSeparatorMargin = new Thickness(0, global.UniformlyMarginXXS, 0, 0);
        token.TableFilterDropdownPadding             = global.PaddingXS;
        // Aligned with Ant Design (fixed.ts): the shadow at the boundary of a frozen column is
        // inset 10px 0 8px -8px on the left and inset -10px 0 8px -8px on the right.
        token.LeftFrozenShadows = new BoxShadows(new BoxShadow
        {
            OffsetX = 10,
            OffsetY = 0,
            Blur    = 8,
            Spread  = -8,
            IsInset = true,
            Color   = global.ColorSplit
        });
        token.RightFrozenShadows = new BoxShadows(new BoxShadow
        {
            OffsetX = -10,
            OffsetY = 0,
            Blur    = 8,
            Spread  = -8,
            IsInset = true,
            Color   = global.ColorSplit
        });
        token.ColumnReorderActiveBg = colorFillContentSolid;

        token.PaginationMargin   = new Thickness(0, global.UniformlyMargin);
        token.PaginationMarginSM = new Thickness(0, global.UniformlyMarginXS);
        token.PaginationPanelMargin   = new Thickness(global.UniformlyMargin, 0);
        token.PaginationPanelMarginSM = new Thickness(global.UniformlyMarginXS, 0);
    }
}
