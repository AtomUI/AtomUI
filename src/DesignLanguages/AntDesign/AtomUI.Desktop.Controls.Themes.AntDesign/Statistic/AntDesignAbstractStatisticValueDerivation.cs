using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractStatisticValueToken"/> layer.</summary>
public static class AntDesignAbstractStatisticValueDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractStatisticValueToken token)
    {
        token.ContentFontSize = global.FontSizeHeading3;
    }
}
