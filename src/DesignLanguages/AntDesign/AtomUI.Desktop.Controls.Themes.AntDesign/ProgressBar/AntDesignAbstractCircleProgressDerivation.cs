using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractCircleProgressToken"/> layer.</summary>
public static class AntDesignAbstractCircleProgressDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractCircleProgressToken token)
    {
        AntDesignAbstractProgressDerivation.DeriveShared(in context, global, token);
        token.CircleTextColor           = global.ColorText;
        token.CircleMinimumIconSize     = global.SizeXS;
        token.CircleMinimumTextFontSize = global.FontSizeSM - 2;
    }
}
