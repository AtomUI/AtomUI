using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SimplePaginationToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSimplePaginationDerivation : IControlDerivation<PrimeNGToken, SimplePaginationToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SimplePaginationToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGSimplePaginationTokens.EvaluateGlobal(in context, global);
        PrimeNGAbstractPaginationDerivation.DeriveShared(in context, global, token);
        token.ItemInputBg = global.ColorBgContainer;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGSimplePaginationTokens.EvaluateOwn(in context, global, token);
    }
}
