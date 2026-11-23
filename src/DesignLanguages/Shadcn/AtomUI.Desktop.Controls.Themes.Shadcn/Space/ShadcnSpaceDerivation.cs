using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="SpaceToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnSpaceDerivation : IControlDerivation<ShadcnToken, SpaceToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<SpaceToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.GapSmallSize  = global.SpacingXS;
        token.GapMiddleSize = global.Spacing;
        token.GapLargeSize  = global.SpacingLG;
    }
}
