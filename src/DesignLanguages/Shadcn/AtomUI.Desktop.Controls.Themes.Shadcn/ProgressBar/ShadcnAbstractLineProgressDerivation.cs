using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractLineProgressToken"/> layer.</summary>
public static class ShadcnAbstractLineProgressDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractLineProgressToken token)
    {
        ShadcnAbstractProgressDerivation.DeriveShared(in context, global, token);
        token.LineInfoIconSize    = global.IconSize;
        token.LineInfoIconSizeSM  = global.IconSizeSM;
        token.LineExtraInfoMargin = global.ControlPaddingHorizontalSM;
    }
}
