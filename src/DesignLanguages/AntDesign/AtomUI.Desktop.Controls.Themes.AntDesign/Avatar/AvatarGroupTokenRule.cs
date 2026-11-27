using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>
/// The AntDesign view of <see cref="AvatarGroupToken" />.
/// </summary>
[ControlTokenRule]
internal sealed class AvatarGroupTokenRule : IControlTokenRule<AntDesignToken, AvatarGroupToken>
{
    public void Apply(in TokenEvaluationContext<AntDesignToken> context, ControlTokenTarget<AvatarGroupToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        token.GroupSpace       = global.UniformlyMarginXXS;
        token.GroupOverlapping = global.UniformlyMarginXS;
    }
}
