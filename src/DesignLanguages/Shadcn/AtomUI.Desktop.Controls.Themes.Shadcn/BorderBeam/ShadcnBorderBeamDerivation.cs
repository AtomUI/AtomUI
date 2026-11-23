using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="BorderBeamToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnBorderBeamDerivation : IControlDerivation<ShadcnToken, BorderBeamToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<BorderBeamToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.BeamSize              = 100d;
        token.BeamOpacity           = 0.95d;
        token.MotionDuration        = TimeSpan.FromSeconds(6);
        token.MaxVisibleStopPercent = 70d;
    }
}
