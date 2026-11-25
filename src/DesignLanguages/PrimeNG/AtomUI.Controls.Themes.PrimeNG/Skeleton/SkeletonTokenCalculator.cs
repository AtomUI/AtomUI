using AtomUI.Controls.DesignTokens;
using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractSkeletonToken"/> layer.</summary>
public static class SkeletonTokenCalculator
{
    public static void Calculate(in TokenEvaluationContext<PrimeNGToken> context, DesignToken global, AbstractSkeletonToken token)
    {
        // GradientFromColor, GradientToColor and ParagraphLineHeight feed values derived in the terminal, which
        // assigns their defaults behind an IsExplicit guard of its own Token kind.
        token.TitleHeight           = global.ControlHeight / 2;
        token.BlockRadius           = global.BorderRadiusSM;
        token.LoadingMotionDuration = TimeSpan.FromSeconds(1.4);
    }
}
