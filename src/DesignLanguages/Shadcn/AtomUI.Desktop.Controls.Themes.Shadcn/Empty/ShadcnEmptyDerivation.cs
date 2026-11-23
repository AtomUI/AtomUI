using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn view of <see cref="EmptyToken"/>.</summary>
[ControlDerivation]
internal sealed class ShadcnEmptyDerivation : IControlDerivation<ShadcnToken, EmptyToken>
{
    public void Derive(in DerivationContext<ShadcnToken> context, ControlView<EmptyToken> view)
    {
        var token  = view.Token;
        var global = view.Global;
        // The preset illustration draws its tones from the fill ramp, which the shadcn variables put on one neutral stop:
        // rebuild the antd ladder (15% / 6% / 4% / 2% of the foreground) as foreground-over-background composites.
        var emptyForeground = context.Language.Foreground;
        var emptyBackground = context.Language.Background;
        global.ColorFill            = ShadcnRecipes.CompositeOver(emptyForeground, 0.15, emptyBackground);
        global.ColorFillSecondary   = ShadcnRecipes.CompositeOver(emptyForeground, 0.06, emptyBackground);
        global.ColorFillTertiary    = ShadcnRecipes.CompositeOver(emptyForeground, 0.04, emptyBackground);
        global.ColorFillQuaternary  = ShadcnRecipes.CompositeOver(emptyForeground, 0.02, emptyBackground);
        global.ColorBorderSecondary = ShadcnRecipes.CompositeOver(emptyForeground, 0.06, emptyBackground);
        var controlHeightLG = global.ControlHeightLG;
        token.EmptyImgHeight      = controlHeightLG * 2.5;
        token.EmptyImgHeightMD    = controlHeightLG * 1.85;
        token.EmptyImgHeightSM    = controlHeightLG * 0.875;
        token.DescriptionMargin   = new Thickness(0, global.UniformlyMarginSM, 0, 0);
        token.DescriptionMarginSM = new Thickness(0, global.UniformlyMarginXS, 0, 0);
        token.FooterMargin        = new Thickness(0, global.UniformlyMargin, 0, 0);
    }
}
