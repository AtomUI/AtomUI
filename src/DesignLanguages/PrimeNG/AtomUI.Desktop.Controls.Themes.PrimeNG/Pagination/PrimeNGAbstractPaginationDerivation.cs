using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractPaginationToken"/> layer.</summary>
public static class PrimeNGAbstractPaginationDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractPaginationToken token)
    {
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
