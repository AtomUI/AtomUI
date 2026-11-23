using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign view of <see cref="AvatarGroupToken"/>.</summary>
[ControlDerivation]
internal sealed class AntDesignAvatarGroupDerivation : IControlDerivation<AntDesignToken, AvatarGroupToken>
{
    public void Derive(in DerivationContext<AntDesignToken> context, ControlView<AvatarGroupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        token.GroupSpace       = global.UniformlyMarginXXS;
        token.GroupOverlapping = global.UniformlyMarginXS;
    }
}
