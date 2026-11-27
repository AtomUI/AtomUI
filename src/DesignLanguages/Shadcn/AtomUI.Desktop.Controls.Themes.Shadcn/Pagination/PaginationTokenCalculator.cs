using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractPaginationToken" /> layer.
/// </summary>
public static class PaginationTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractPaginationToken" /> layer of the pagination and the simple pagination.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractPaginationToken token)
    {
        // Ant Design formula kept as is: no registry value is merged into the shared pagination tokens here.
        token.ItemSize                  = global.ControlHeight;
        token.ItemSizeSM                = global.ControlHeightSM;
        token.ItemActiveColorDisabled   = global.ColorTextDisabled;
        token.PaginationLayoutSpacing             = global.UniformlyMarginXS;
        token.PaginationLayoutMiniSpacing         = Math.Max(global.UniformlyMarginXXS / 2,
                                                       global.ControlOutlineWidth);
        token.PaginationQuickJumperInputWidth     = global.ControlHeightLG * 1.25;
        token.PaginationMiniQuickJumperInputWidth = global.ControlHeightLG * 1.1;
        token.ItemColor                           = global.ColorText;
        token.ItemHoverColor                      = global.ColorText;
        token.ItemHoverBg                         = global.ColorBgTextHover;
        token.ItemBorderRadius                    = global.BorderRadius;
    }
}
