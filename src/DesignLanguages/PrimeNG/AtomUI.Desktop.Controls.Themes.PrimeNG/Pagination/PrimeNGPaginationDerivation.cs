using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="PaginationToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGPaginationDerivation : IControlDerivation<PrimeNGToken, PaginationToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<PaginationToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGPaginationTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractPaginationDerivation.DeriveShared(in context, global, token);
        token.PaginationItemPaddingInline = new Thickness(global.UniformlyMarginXXS * 1.5, 0);
        token.ItemBg                    = global.ColorBgContainer;
        token.ItemActiveBg              = global.ColorBgContainer;
        token.ItemLinkBg                = global.ColorBgContainer;
        token.ItemActiveBgDisabled      = global.ControlItemBgActiveDisabled;
        token.ItemActiveColor            = global.ColorPrimary;
        token.ItemActiveBorderColor      = global.ColorPrimary;
        token.ItemActiveHoverColor       = global.ColorPrimaryHover;
        token.ItemActiveHoverBorderColor = global.ColorPrimaryHover;
        token.ItemActiveFontWeight       = global.FontWeightStrong;

        token.InputOutlineOffset                  = new Thickness(0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGPaginationTokens.EvaluateOwn(in context, global, token);
    }
}
