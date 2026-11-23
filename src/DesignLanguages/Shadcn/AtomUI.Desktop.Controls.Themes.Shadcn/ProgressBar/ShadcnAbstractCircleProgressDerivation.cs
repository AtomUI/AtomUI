using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractCircleProgressToken"/> layer.</summary>
public static class ShadcnAbstractCircleProgressDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractCircleProgressToken token)
    {
        ShadcnAbstractProgressDerivation.DeriveShared(in context, global, token);
        token.CircleTextColor           = global.ColorText;
        token.CircleMinimumIconSize     = global.SizeXS;
        token.CircleMinimumTextFontSize = global.FontSizeSM - 2;
    }
}
