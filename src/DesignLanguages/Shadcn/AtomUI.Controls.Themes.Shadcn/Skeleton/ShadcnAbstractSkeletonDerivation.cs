using AtomUI.Controls.DesignTokens;
using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractSkeletonToken"/> layer.</summary>
public static class ShadcnAbstractSkeletonDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractSkeletonToken token)
    {
        // GradientFromColor, GradientToColor and ParagraphLineHeight feed values derived in the terminal, which
        // assigns their defaults behind an IsExplicit guard of its own Token kind.
        token.TitleHeight           = global.ControlHeight / 2;
        token.BlockRadius           = global.BorderRadiusSM;
        token.LoadingMotionDuration = TimeSpan.FromSeconds(1.4);
    }
}
