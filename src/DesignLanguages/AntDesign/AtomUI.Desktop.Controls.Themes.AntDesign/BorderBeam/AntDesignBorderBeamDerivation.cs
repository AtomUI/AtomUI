using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="BorderBeamToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignBorderBeamDerivation : IControlDerivation<AntDesignToken, BorderBeamToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<BorderBeamToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.BeamSize              = 100d;
        token.BeamOpacity           = 0.95d;
        token.MotionDuration        = TimeSpan.FromSeconds(6);
        token.MaxVisibleStopPercent = 70d;
    }
}
