using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AvatarGroupToken"/>.</summary>
[ControlTokenRule]
internal sealed class AvatarGroupTokenRule : IControlTokenRule<PrimeNGToken, AvatarGroupToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<AvatarGroupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAvatarGroupTokens.EvaluateGlobal(in context, global);
        token.GroupSpace       = global.UniformlyMarginXXS;
        token.GroupOverlapping = global.UniformlyMarginXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAvatarGroupTokens.EvaluateOwn(in context, global, token);
    }
}
