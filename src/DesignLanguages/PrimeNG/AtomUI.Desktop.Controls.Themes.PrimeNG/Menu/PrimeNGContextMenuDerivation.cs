using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="ContextMenuToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGContextMenuDerivation : IControlDerivation<PrimeNGToken, ContextMenuToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<ContextMenuToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGContextMenuTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractMenuPopupDerivation.DeriveShared(in context, global, token);
        token.ItemColor = global.ColorText;
        token.ItemHoverColor = token.ItemColor;
        token.ContextMenuOffsetX = global.SpacingXXS;
        token.ContextMenuOffsetY = global.SpacingXXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGContextMenuTokens.EvaluateOwn(in context, global, token);
    }
}
