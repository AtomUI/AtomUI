using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="PaginationToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnPaginationDerivation : IControlDerivation<ShadcnToken, PaginationToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<PaginationToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        ShadcnAbstractPaginationDerivation.DeriveShared(in context, global, token);
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
    }
}
