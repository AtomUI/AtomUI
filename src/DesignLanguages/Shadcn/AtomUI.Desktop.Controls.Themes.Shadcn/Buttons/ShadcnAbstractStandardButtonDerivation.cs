using AtomUI.DesignLanguages.Shadcn;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;
using Avalonia.Media;

namespace AtomUI.Desktop.Controls.Themes.Shadcn;

/// <summary>The shadcn values of the shared <see cref="AbstractStandardButtonToken"/> layer.</summary>
public static class ShadcnAbstractStandardButtonDerivation
{
    public static void DeriveShared(in DerivationContext<ShadcnToken> context, DesignToken global, AbstractStandardButtonToken token)
    {
        ShadcnAbstractButtonGeometryDerivation.DeriveShared(in context, global, token);
        token.FontWeight = FontWeight.Normal;
        token.DefaultBorderColorDisabled = global.ColorBorder;
    }
}
