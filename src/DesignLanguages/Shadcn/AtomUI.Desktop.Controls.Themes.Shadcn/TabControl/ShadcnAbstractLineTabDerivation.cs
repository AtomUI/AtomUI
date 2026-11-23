using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractLineTabToken"/> layer.</summary>
public static class ShadcnAbstractLineTabDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractLineTabToken token)
    {
        ShadcnAbstractTabDerivation.DeriveShared(in context, global, token);
        token.InkBarColor = global.ColorPrimary;
        token.InkBarThickness = global.LineWidthBold;
        token.HorizontalItemGutter = 32;
        token.HorizontalItemPadding = new Thickness(0, global.UniformlyPaddingSM);
        token.HorizontalItemPaddingLG = new Thickness(0, global.UniformlyPadding);
        token.HorizontalItemPaddingSM = new Thickness(0, global.UniformlyPaddingXS);
        token.VerticalItemGutter = global.SpacingXXS;
    }
}
