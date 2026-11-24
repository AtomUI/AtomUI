using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="SpaceToken"/>.</summary>
[ControlTokenRule]
internal sealed class SpaceTokenRule : IControlTokenRule<AntDesignToken, SpaceToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<SpaceToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.GapSmallSize  = global.SpacingXS;
        token.GapMiddleSize = global.Spacing;
        token.GapLargeSize  = global.SpacingLG;
    }
}
