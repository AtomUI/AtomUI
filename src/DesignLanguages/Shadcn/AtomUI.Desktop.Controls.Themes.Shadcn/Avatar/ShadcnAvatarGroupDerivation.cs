using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="AvatarGroupToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnAvatarGroupDerivation : IControlDerivation<ShadcnToken, AvatarGroupToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<AvatarGroupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.GroupSpace       = global.UniformlyMarginXXS;
        token.GroupOverlapping = global.UniformlyMarginXS;
    }
}
