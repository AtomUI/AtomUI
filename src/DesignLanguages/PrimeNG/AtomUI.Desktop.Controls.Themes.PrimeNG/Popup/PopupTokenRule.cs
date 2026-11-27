using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="PopupToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class PopupTokenRule : IControlTokenRule<PrimeNGToken, PopupToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<PopupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides (MotionDurationMid), read by the Popup theme (MotionDuration)
        // and not by the formulas below.
        PrimeNGPopupTokens.EvaluateGlobal(in context, global);
        PopupCornerTokenCalculator.Calculate(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // Window-style and overlay popups share one shadow. Placement compensates for the residual shadow, so the
        // position of the content relative to the anchor does not depend on the shadow thickness.
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGPopupTokens.EvaluateOwn(in context, global, token);
    }
}
