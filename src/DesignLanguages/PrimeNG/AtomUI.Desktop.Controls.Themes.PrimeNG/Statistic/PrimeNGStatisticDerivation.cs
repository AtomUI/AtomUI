using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="StatisticToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGStatisticDerivation : IControlDerivation<PrimeNGToken, StatisticToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<StatisticToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractStatisticDerivation.DeriveShared(in context, global, token);
    }
}
