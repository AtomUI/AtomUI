using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractStatisticToken"/> layer.</summary>
public static class AntDesignAbstractStatisticDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractStatisticToken token)
    {
        AntDesignAbstractStatisticValueDerivation.DeriveShared(in context, global, token);
        token.TitleFontSize = global.FontSize;
    }
}
