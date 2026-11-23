using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="GroupBoxToken"/>.</summary>
[ControlDerivation]
internal sealed class PrimeNGGroupBoxDerivation : IControlDerivation<PrimeNGToken, GroupBoxToken>
{
    public void Derive(in DerivationContext<PrimeNGToken> context, ControlView<GroupBoxToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // PrimeNG: the control-global overrides, read by the formulas below.
        PrimeNGGroupBoxTokens.EvaluateGlobal(in context, global);
        token.ContentPadding           = global.PaddingXS;
        token.HeaderContainerMargin = new Thickness(global.UniformlyMargin, global.UniformlyMarginXS,
            global.UniformlyMargin, 0);
        token.HeaderContentPadding = new Thickness(global.UniformlyPaddingXXS, 0);
        token.HeaderIconMargin     = new Thickness(0, 0, global.UniformlyMarginXXS, 0);
        token.HeaderFontWeight     = FontWeight.Normal;
        // PrimeNG: the preset's control values, over the formulas.
        PrimeNGGroupBoxTokens.EvaluateOwn(in context, global, token);
    }
}
