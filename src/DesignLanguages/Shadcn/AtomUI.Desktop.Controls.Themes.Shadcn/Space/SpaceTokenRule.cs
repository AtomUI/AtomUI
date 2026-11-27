using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="SpaceToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SpaceTokenRule : IControlTokenRule<ShadcnToken, SpaceToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<SpaceToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.GapSmallSize  = global.SpacingXS;
        token.GapMiddleSize = global.Spacing;
        token.GapLargeSize  = global.SpacingLG;
    }
}
