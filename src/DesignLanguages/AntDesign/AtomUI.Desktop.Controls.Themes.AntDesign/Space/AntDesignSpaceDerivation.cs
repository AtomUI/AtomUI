using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SpaceToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignSpaceDerivation : IControlDerivation<AntDesignToken, SpaceToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<SpaceToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.GapSmallSize  = global.SpacingXS;
        token.GapMiddleSize = global.Spacing;
        token.GapLargeSize  = global.SpacingLG;
    }
}
