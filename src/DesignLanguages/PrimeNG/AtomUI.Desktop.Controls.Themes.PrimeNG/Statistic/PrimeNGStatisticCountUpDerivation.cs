using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="StatisticCountUpToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGStatisticCountUpDerivation : IControlDerivation<PrimeNGToken, StatisticCountUpToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<StatisticCountUpToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractStatisticValueDerivation.DeriveShared(in context, global, token);
    }
}
