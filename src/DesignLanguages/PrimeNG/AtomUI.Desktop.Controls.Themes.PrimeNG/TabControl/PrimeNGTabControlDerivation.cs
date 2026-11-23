using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TabControlToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTabControlDerivation : IControlDerivation<PrimeNGToken, TabControlToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TabControlToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGTabControlTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractLineTabDerivation.DeriveShared(in context, global, token);
        token.MenuEdgeThickness = global.ControlHeight;
        token.TabAndContentGutter = global.UniformlyMarginSM;
        token.HorizontalMargin = new Thickness(0, 0, global.UniformlyMargin, 0);
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGTabControlTokens.EvaluateOwn(in context, global, token);
    }
}
