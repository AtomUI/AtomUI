using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="AvatarGroupToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGAvatarGroupDerivation : IControlDerivation<PrimeNGToken, AvatarGroupToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<AvatarGroupToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGAvatarGroupTokens.EvaluateGlobal(in context, global);
        token.GroupSpace       = global.UniformlyMarginXXS;
        token.GroupOverlapping = global.UniformlyMarginXS;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGAvatarGroupTokens.EvaluateOwn(in context, global, token);
    }
}
