using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="FlyoutHostToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGFlyoutHostDerivation : IControlDerivation<PrimeNGToken, FlyoutHostToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<FlyoutHostToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGFlyoutHostTokens.EvaluateGlobal(in context, global);
        token.MarginToAnchor    = global.UniformlyMarginXXS;
        token.OverlayHostShadow = global.BoxShadowsSecondary;
        token.PopupRootShadow   = global.BoxShadowsSecondary;
        token.HorizontalOffset = global.UniformlyMarginXS;
        token.VerticalOffset   = global.UniformlyMarginXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGFlyoutHostTokens.EvaluateOwn(in context, global, token);
    }
}
