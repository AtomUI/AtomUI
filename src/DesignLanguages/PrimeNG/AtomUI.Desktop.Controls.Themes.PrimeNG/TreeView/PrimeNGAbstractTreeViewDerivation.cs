using AtomUI.DesignLanguages.PrimeNG;
using AtomUI.Theme.DesignTokens;
using AtomUI.Theme.Languages;

namespace AtomUI.Desktop.Controls.Themes.PrimeNG;

/// <summary>The PrimeNG values of the shared <see cref="AbstractTreeViewToken"/> layer.</summary>
public static class PrimeNGAbstractTreeViewDerivation
{
    public static void DeriveShared(in DerivationContext<PrimeNGToken> context, DesignToken global, AbstractTreeViewToken token)
    {
        PrimeNGAbstractTreePresentationDerivation.DeriveShared(in context, global, token);


        token.DragIndicatorLineWidth = global.LineWidthBold;
    }
}
