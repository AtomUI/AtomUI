using AtomUI.Controls.DesignTokens;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.Shadcn;

/// <summary>
/// The shadcn values of the shared <see cref="AbstractSkeletonToken" /> layer.
/// </summary>
public static class SkeletonTokenCalculator
{
    /// <summary>
    /// Assigns the shadcn values to <paramref name="token" />, the shared
    /// <see cref="AbstractSkeletonToken" /> layer.
    /// </summary>
    public static void Calculate(in TokenEvaluationContext<ShadcnToken> context, DesignToken global, AbstractSkeletonToken token)
    {
        // GradientFromColor, GradientToColor and ParagraphLineHeight feed values derived in the terminal, which
        // assigns their defaults behind an IsExplicit guard of its own Token kind.
        token.TitleHeight           = global.ControlHeight / 2;
        token.BlockRadius           = global.BorderRadiusSM;
        // Ant Design formula kept as is: no registry value is merged into the skeleton tokens.
        token.LoadingMotionDuration = TimeSpan.FromSeconds(1.4);
    }
}
