using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="BorderBeamToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGBorderBeamDerivation : IControlDerivation<PrimeNGToken, BorderBeamToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<BorderBeamToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.BeamSize              = 100d;
        token.BeamOpacity           = 0.95d;
        token.MotionDuration        = TimeSpan.FromSeconds(6);
        token.MaxVisibleStopPercent = 70d;
    }
}
