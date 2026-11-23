using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractStatisticToken"/> layer.</summary>
public static class ShadcnAbstractStatisticDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractStatisticToken token)
    {
        ShadcnAbstractStatisticValueDerivation.DeriveShared(in context, global, token);
        token.TitleFontSize = global.FontSize;
    }
}
