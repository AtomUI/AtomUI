using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="PopupToken"/>.</summary>
[ControlTokenRule]
internal sealed class PopupTokenRule : IControlTokenRule<PrimeNGToken, PopupToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<PopupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGPopupTokens.EvaluateGlobal(in context, global);
        PopupCornerTokenCalculator.Calculate(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGPopupTokens.EvaluateOwn(in context, global, token);
    }
}
