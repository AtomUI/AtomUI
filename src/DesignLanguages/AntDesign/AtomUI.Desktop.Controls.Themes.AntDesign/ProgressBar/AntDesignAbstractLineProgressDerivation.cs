using AtomUI.DesignLanguages.AntDesign;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.AntDesign;

/// <summary>The AntDesign values of the shared <see cref="AbstractLineProgressToken"/> layer.</summary>
public static class AntDesignAbstractLineProgressDerivation
{
    public static void DeriveShared(in DerivationContext<AntDesignToken> context, DesignToken global, AbstractLineProgressToken token)
    {
        AntDesignAbstractProgressDerivation.DeriveShared(in context, global, token);
        token.LineInfoIconSize    = global.IconSize;
        token.LineInfoIconSizeSM  = global.IconSizeSM;
        token.LineExtraInfoMargin = global.ControlPaddingHorizontalSM;
    }
}
