using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>
/// The PrimeNG view of <see cref="SpaceToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class SpaceTokenRule : IControlTokenRule<PrimeNGToken, SpaceToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<SpaceToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.GapSmallSize  = global.SpacingXS;
        token.GapMiddleSize = global.Spacing;
        token.GapLargeSize  = global.SpacingLG;
    }
}
