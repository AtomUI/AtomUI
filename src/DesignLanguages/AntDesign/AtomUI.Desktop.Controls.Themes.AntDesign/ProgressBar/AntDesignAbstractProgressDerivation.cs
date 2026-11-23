using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractProgressToken"/> layer.</summary>
public static class AntDesignAbstractProgressDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractProgressToken token)
    {
        token.DefaultColor   = global.ColorInfo;
        token.RemainingColor = global.ColorFillSecondary;
    }
}
