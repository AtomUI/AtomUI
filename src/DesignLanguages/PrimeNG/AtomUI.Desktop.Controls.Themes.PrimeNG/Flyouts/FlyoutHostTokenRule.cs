using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="FlyoutHostToken"/>.</summary>
[ControlTokenRule]
internal sealed class FlyoutHostTokenRule : IControlTokenRule<PrimeNGToken, FlyoutHostToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<FlyoutHostToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
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
