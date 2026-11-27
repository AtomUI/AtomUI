using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="PopupToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class PopupTokenRule : IControlTokenRule<AntDesignToken, PopupToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<PopupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        PopupCornerTokenCalculator.Calculate(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // Window-style and overlay popups share one shadow. Placement is compensated for the residual shadow, so
        // where the content sits relative to the anchor does not depend on the shadow thickness.
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
    }
}
