using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="TimerStatisticToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGTimerStatisticDerivation : IControlDerivation<PrimeNGToken, TimerStatisticToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<TimerStatisticToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        PrimeNGAbstractStatisticDerivation.DeriveShared(in context, global, token);
    }
}
