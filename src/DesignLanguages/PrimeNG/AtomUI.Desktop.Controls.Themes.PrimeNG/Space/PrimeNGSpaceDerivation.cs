using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="SpaceToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGSpaceDerivation : IControlDerivation<PrimeNGToken, SpaceToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<SpaceToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.GapSmallSize  = global.SpacingXS;
        token.GapMiddleSize = global.Spacing;
        token.GapLargeSize  = global.SpacingLG;
    }
}
