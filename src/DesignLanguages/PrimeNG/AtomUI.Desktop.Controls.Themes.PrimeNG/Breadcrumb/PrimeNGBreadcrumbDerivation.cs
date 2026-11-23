using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="BreadcrumbToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGBreadcrumbDerivation : IControlDerivation<PrimeNGToken, BreadcrumbToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<BreadcrumbToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGBreadcrumbTokens.EvaluateGlobal(in context, global);
        token.ItemColor                   = global.ColorTextDescription;
        token.LastItemColor               = global.ColorText;
        token.LinkColor                   = global.ColorTextDescription;
        token.LinkHoverColor              = global.ColorText;
        token.LinkHoverBgColor            = global.ColorBgTextHover;
        token.SeparatorColor              = global.ColorTextDescription;
        token.SeparatorMargin             = new Thickness(global.UniformlyMarginXXS, 0);
        token.BreadcrumbItemContentPadding = new Thickness(global.UniformlyPaddingXXS, global.UniformlyPaddingXXS,
            global.UniformlyPaddingXXS, global.UniformlyPaddingXXS);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGBreadcrumbTokens.EvaluateOwn(in context, global, token);
    }
}
