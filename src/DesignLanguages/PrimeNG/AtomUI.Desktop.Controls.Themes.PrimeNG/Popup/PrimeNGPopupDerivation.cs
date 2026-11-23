using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="PopupToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGPopupDerivation : IControlDerivation<PrimeNGToken, PopupToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<PopupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGPopupTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractPopupCornerDerivation.DeriveShared(in context, global, token);

        token.OverlayHostShadow = global.BoxShadowsSecondary;
        // 窗口型与 Overlay Popup 使用同一阴影；定位按残余阴影补偿，内容相对 anchor 的位置与阴影厚度无关
        token.PopupRootShadow = global.BoxShadowsSecondary;
        token.MarginToAnchor = global.UniformlyMarginXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGPopupTokens.EvaluateOwn(in context, global, token);
    }
}
