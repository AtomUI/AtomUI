using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn view of <see cref="AvatarGroupToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AvatarGroupTokenRule : IControlTokenRule<ShadcnToken, AvatarGroupToken>
{
    public void Apply(in TokenEvaluationContext<ShadcnToken> context, ControlTokenTarget<AvatarGroupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.GroupSpace       = global.UniformlyMarginXXS;
        token.GroupOverlapping = global.UniformlyMarginXS;
    }
}
