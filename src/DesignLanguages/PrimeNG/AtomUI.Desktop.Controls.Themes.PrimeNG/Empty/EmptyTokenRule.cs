using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG view of <see cref="EmptyToken"/>.</summary>
[ControlTokenRule]
internal sealed class EmptyTokenRule : IControlTokenRule<PrimeNGToken, EmptyToken>
{
    public void Apply(in TokenEvaluationContext<PrimeNGToken> context, ControlTokenTarget<EmptyToken> target)
    {
        var token  = target.Own;
        var global = target.Global;
        var controlHeightLG = global.ControlHeightLG;
        token.EmptyImgHeight      = controlHeightLG * 2.5;
        token.EmptyImgHeightMD    = controlHeightLG * 1.85;
        token.EmptyImgHeightSM    = controlHeightLG * 0.875;
        token.DescriptionMargin   = new Thickness(0, global.UniformlyMarginSM, 0, 0);
        token.DescriptionMarginSM = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        token.FooterMargin        = new Thickness(0, global.UniformlyMargin, 0, 0);
    }
}
